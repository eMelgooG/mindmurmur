import logging
import time
import math
import numpy
import os
import re
import threading
import traceback
import wx

from fr0stlib import Flame
from fr0stlib.render import save_image
from fr0stlib.render import to_string as flame_to_string
from fr0stlib.pyflam3 import Genome, byref, flam3_interpolate

from eegsources import *
from common.rabbit_controller import RabbitController
from input_controller import InputController
from renderer import Renderer, RenderFrame

class MMEngine():
    def __init__(self, eeg_source, gui):
        print("[>] _INIT")
        self.eeg_source = eeg_source
        self.frame_index = 0
        self.speed = 0.4
        self.channels = 24
        self.sinelength = 300 # frames
        self.gui = gui
        self.maxfps = 60
        self.states_flames = []
        self.user_connected = False

        # init rabbitMQ connection
        self.rabbit = RabbitController('localhost', 5672, 'guest', 'guest', '/')

        self.input_controller = InputController(self)

        # attach keyboard events.
        self.input_controller.bind_keyboardevents(self.gui)

        # reference to global or defined herebefore
        self.retreive_params()
        self.flame = self.states_flames[0]
        # set these 3 to start fractal interpolation
        self.transition_pct = None
        self.transition_from = None
        self.transition_to = None
        # sets the frame at which the user disconnected
        self.disconnected_at = None

        # Init and transition so animation begins right away
        self.set_meditation_state(1, False)
        self.set_meditation_state(set_next=True)

        # Listen to meditation state events
        self.eeg_source.set_meditation_state_handler(self.set_meditation_state)

    def run(self):
        print("[>] RUNNING")
        self.keeprendering = True
        current_second = math.floor(time.clock())
        frames_this_second = 0
        while self.keeprendering:
            # fps timer
            t0 = time.clock()

            # fps counting
            latest_second = math.floor(time.clock())
            if current_second != latest_second:
                # rolled over into a new second, print FPS
                #print("FPS: %d" % frames_this_second)
                current_second = latest_second
                frames_this_second = 0

            try:
                if(self.user_connected):
                    self.user_connected_frame()
                else:
                    self.idle_frame()

                # render with new values
                self.render()

                # count frame number
                self.frame_index += 1
                self.frame_index_sincestate += 1

            except Exception as ex:
                print('[!] error during MMEngine RUN loop: ' + str(ex))
                traceback.print_exc()
                self.keeprendering = False
            finally:
                # sleep to keep a decent fps
                delay = t0 + 1./self.maxfps - time.clock()
                if delay > 0.:
                    time.sleep(delay)
                frames_this_second += 1
        self.stop()


    def idle_frame(self):
        # read data
        eegdata = self.eeg_source.read_data()

        # apply transition
        self.apply_transition(duration_sec = 60)

        # no data
        if(eegdata is None or eegdata.is_empty() == True):
            # do nothing during 1 minute.
            if(time.clock() > self.last_sincestate_reset + 60):
            # or transition to next state:
                self.set_meditation_state(set_next=True)

        # data received
        else:
            # if inactive for more than 30 seconds
            if(time.clock() > self.last_sincestate_reset + 30):
                print("[ ] NEW SESSION")
                self.retreive_params()
                # back to state 1
                self.set_meditation_state(1)
            else:
                print("[ ] USER RECONNECTED")
            # stop idling
            self.user_connected = True

    def user_connected_frame(self):
        # if flames were designed for transition,
        # update the running flame
        self.apply_transition(duration_sec = 10 if self.meditation_state == 1 else 30)

        # read data
        eegdata = self.eeg_source.read_data()

        # data found
        if(eegdata is not None and eegdata.is_empty() == False):

            # [!] new meditation state reached
            if(self.meditation_state != eegdata.meditation_state \
                # and if transitionned more than a minute ago
                and time.clock() > self.last_sincestate_reset + 60):
                # [>] set new state
                self.set_meditation_state(eegdata.meditation_state)

            #TODO get heartbeat
            heartbeat = 60
            # send data to RabbitMQ bus
            self.rabbit.publish_heart(heartbeat)

            # transform fractal with new values from data
            self.animate(eegdata)

        # no data is found
        else:
            print("[ ] USER DISCONNECTED")
            # go to idling.
            self.frame_index = 0
            self.frame_index_sincestate = 0
            self.last_sincestate_reset = time.clock()
            self.user_connected = False


    def stop(self):
        print("[>] STOP")
        self.keeprendering = False
        self.gui.image.renderer.keeprendering = False
        self.gui.stop()

    def zoom(self, zoomamount = 1):
        self.flame.scale *= zoomamount

    def set_meditation_state(self, newstate = 0, transtition = True, set_prev = False, set_next = False):
        if(set_prev):
            newstate = self.meditation_state - 1 if self.meditation_state > 1 else 5
        elif(set_next):
            newstate = self.meditation_state + 1 if self.meditation_state < 5 else 1

        # save state
        self.meditation_state = newstate
        self.frame_index_sincestate = 0
        self.last_sincestate_reset = time.clock()

        # find appropriate flame
        flame_per_state = int(len(self.states_flames) / 5)
        flame_index = (newstate - 1) * flame_per_state \
                    + numpy.random.randint(0, flame_per_state)

        print("[ ] ENTERING STATE %s" %(newstate))
        if(transtition):
            print("[ ] TRANSITION TO FLAME %s" %(self.states_flames[flame_index].name))
            # start transition
            self.transition_pct = 0.0
            self.transition_from = self.flame
            self.transition_to = self.states_flames[flame_index]
        else:
            print("[ ] LOADING FLAME %s" %(self.states_flames[flame_index].name))
            self.flame = self.load_flame(self.states_flames[flame_index])



    def move(self, x = 0, y = 0):
        try:
            move_x =  y * np.sin(self.flame.rotate * np.pi / 180.) \
                    + x * np.cos(self.flame.rotate * np.pi / 180.)
            move_y =  y * np.cos(self.flame.rotate * np.pi / 180.) \
                    + x * np.sin(self.flame.rotate * np.pi / 180.)
            move_x /= self.flame.scale
            move_y /= self.flame.scale


            self.flame.center[0] += move_x
            self.flame.center[1] += move_y
        except:
            print("[!] error during flame move")

    def rotate(self, deg_angle = 0):
        try:
            self.flame.rotate += deg_angle
        except:
            print("[!] error during flame rotate")

    def recenter(self):
        self.flame.center = 0, 0
        self.flame.rotate = 0

    # retreive the global fractal color from the current flame's xforms
    def get_flamecolor_rgb(self):
        r,g,b, = 0,0,0
        weight = 0
        # read colors for each xform
        for xf in flame.xform:
            if(xf.weight > 0):
                gradientlocation = int(xf.color * (len(flame.gradient) - 1))
                xcolors = flame.gradient[gradientlocation]
                r = r + xcolors[0] * xf.weight
                g = g + xcolors[1] * xf.weight
                b = b + xcolors[2] * xf.weight
                weight = weight + xf.weight
        if weight == 0:
            return [0,0,0]
        return [int(r / weight), int(g / weight), int(b / weight)]

    def retreive_params(self, show_dialog = False):
        flames = get_flames()

        # the 1st frame is used to be worked on. not a state
        self.states_flames = flames[1:]

        if len(self.states_flames) < 2:
            raise ValueError("Need to select at least 2 flames")

        for flame in self.states_flames:
            flame.size = 960, 540
        return

    def apply_transition(self, duration_sec = 10):
        if(self.transition_pct is not None and self.transition_from is not None and self.transition_to is not None):
            # do the transition
            # apply interpolation transition
            lerp_pct = easing_cubic(self.transition_pct)
            newflame = self.load_flame(self.transition_from, self.transition_to, lerp_pct)
            if(newflame is not None):
                self.flame = newflame

            if(self.transition_pct >= 1):
                # end of transition
                self.transition_pct = None
            else:
                # add 1 / nth to the transition
                self.transition_pct += 1 / float(duration_sec * self.maxfps)
            return True
        else:
            return False


    # lerp is interpolation percentage [0 - 1] between origin and target
    def load_flame(self, flame_origin, flame_target = None, lerp = 0.0):
        loaded_flame = self.flame
        try:
            if(lerp == 0.0 or flame_target is None):
                loaded_flame = flame_origin
            elif(lerp >= 1.0 or flame_origin is None):
                loaded_flame = flame_target
            else:
                # interpolation:
                flame_origin.time = 0
                flame_target.time = 1
                flames_lerp = [flame_origin, flame_target]
                flames_str = "<flames>%s</flames>" % "".join(map(flame_to_string, flames_lerp))
                genomes, ngenomes = Genome.from_string(flames_str)
                targetflame = Genome()
                flam3_interpolate(genomes, ngenomes, lerp, 0, byref(targetflame))
                loaded_flame = Flame(targetflame.to_string())

        except Exception as ex:
            print('[!] error during interpolation at %s: %s' %(lerp, str(ex)))
            traceback.print_exc()
            return None

        return loaded_flame

    # process new EEGData and animate flame
    def animate(self, eegdata):
        # if transition is set, animate the target flame
        # otherwise the curren one
        flame_to_move = self.transition_to if self.transition_to is not None else self.flame
        if (flame_to_move.xform is None or len(flame_to_move.xform) == 0):
            return False
        try:
            # animate one xform at a time
            form = flame_to_move.xform[self.frame_index_sincestate % len(flame_to_move.xform)]
            if(form.animate and eegdata is not None):
                # ROTATION
                # calculate rotation amount from BETA
                rotate_delta = eegdata.beta
                form.rotate(rotate_delta * 1.0 * self.speed)

                # MOVEMENT
                # calculate move amount from GAMMA
                # every n frames is a cycle of X back and forth.
                move_delta = eegdata.gamma * np.sin(self.frame_index * (np.pi * 2.0) / (self.sinelength * 1.0))
                form.move(move_delta * 0.01 * self.speed)

                # SCALE
                # calculate rotation amount from DELTA
                # every n frames is a cycle of X back and forth.
                scale_delta = eegdata.delta * np.sin(self.frame_index * (np.pi * 2.0) / (self.sinelength * 5.0))
                form.scale(1 + (scale_delta * 0.01 * self.speed))


                # # ZOOM
                # # calculate zoom amount from data elements
                # data = eegdata.waves[dataindex % len(eegdata.waves)]
                # dataindex += 1 # next data from audiodata
                # # every n frames is a cycle of X back and forth.
                # data *= np.cos(self.frame_index * (np.pi * 2) / self.sinelength)
                # zoom_delta = data * 0.01 * self.speed
                # form.zoom(1 + zoom_delta)

            return True
        except Exception as ex:
            logging.exception(ex)
            print('[!] error during animation: ' + str(ex))

            # SHOW preview on Fr0st
            return False

    def render(self):
        self.gui.render(self.flame)


def get_flames():
    return open_flame("playa.flame")


def open_flame(path):
    if os.path.exists(path):
        return [Flame(s) for s in load_flamestrings(path)]
    else:
        raise FileNotFoundError(path)


def split_flamestrings(string):
    return re.findall(r'<flame .*?</flame>', string, re.DOTALL)


def load_flamestrings(filename):
    """Reads a flame file and returns a list of flame strings."""
    return split_flamestrings(open(filename).read())


def easing_sine(percent, minvalue = 0, maxvalue = 1):
	return -(maxvalue - minvalue)/2 * (math.cos(math.pi*percent) - 1) + minvalue


def easing_cubic(percent, minvalue = 0, maxvalue = 1):
    percent *= 2.
    if(percent < 1) : return ((maxvalue - minvalue) / 2.) * percent * percent * percent + minvalue
    percent -= 2
    return ((maxvalue - minvalue) / 2.) * (percent * percent * percent + 2) + minvalue


# RUN
print('[$] - BEGIN SCRIPT -')
#audio_folder = get_scriptpath() + "/mindmurmur/sounds_controllers/sound_controller_demo_files/soundscape_controller_demo_files"
# 1 - Dummy DATA
# eeg = EEGDummy()
eeg = EEGFromRabbitMQ('localhost', 5672, 'guest', 'guest', '/')
# audio = get_audio_source(get_scriptpath() + '/mindmurmur/audio/midnightstar_crop.wav')
# eeg = EEGFromAudio(audio)
# 2 - DATA from json file
# eeg = EEGFromJSONFile(get_scriptpath() + '/mindmurmur/data/Muse-B1C1_2018-06-11--07-48-41_1528717729867.json') # extra small
# eeg = EEGFromJSONFile(get_scriptpath() + '/mindmurmur/data/Muse-B1C1_2018-06-10--18-35-09_1528670624296.json') # medium
# eeg = EEGFromJSONFile(get_scriptpath() + '/mindmurmur/data/Muse-B1C1_2018-07-16--07-24-35_1531745297756.json') # large (16 july)
#eeg = EEGFromJSONFile(get_scriptpath() + '/mindmurmur/data/Muse-B1C1_2018-07-17--07-00-11_1531868655676.json') # large (17 july)

app = wx.App(False)

renderer = Renderer()
frame = RenderFrame(None, renderer)
#engine = MMEngine(eeg, frame, audio_folder)
engine = MMEngine(eeg, frame)

engine_thread = threading.Thread(target=engine.run)
engine_thread.daemon = True
engine_thread.start()

app.MainLoop()
print('[x] - END SCRIPT -')