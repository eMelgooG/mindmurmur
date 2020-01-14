import argparse
import datetime
import logging
import msvcrt
import numpy as np
import socketserver
import sys
import signal

from pythonosc.osc_bundle import OscBundle
from pythonosc.osc_message import OscMessage
from collections import deque
from statsmodels.tsa import arima_model
from threading import Thread, Lock, Event

from common.rabbit_controller import RabbitController

QUEUE_SIZE = 300  # band powers are calculated at 10hz, storing a 30 seconds worth of data in dequeue

EMIT_STAGE_PERIOD_SECONDS = 60  # evaluating stages every minute
EMIT_EEGDATA_PERIOD_SECONDS = 1  # evaluating eegdata every second
AUTO_ADVANCE_LEVEL_PERIOD_SECONDS = 1 # check for auto advance level every second
AUTO_ADVANCE_AFTER_SECONDS_WHILE_HEADSET_WORN = 120 # auto advance levels every 2 minutes while meditator in session
AUTO_ADVANCE_AFTER_SECONDS_WHILE_HEADSET_OFF = 600 # auto advance levels every 10 minutes while no one is meditating
LISTEN_FOR_KEY_SECONDS = 0.05 # listening to keys 20 times per second
ARIMA_PARAMS = (4, 0, 1)
LOWER_THRESHOLD = -0.03
UPPER_THRESHOLD = 0.01

logger = logging.getLogger(__name__)


class suppress_stdout:
    def __enter__(self):
        self.stdout = sys.stdout
        sys.stdout = None

    def __exit__(self, type, value, traceback):
        sys.stdout = self.stdout


class OscUDPHandler(socketserver.BaseRequestHandler):

    def handle(self):
        dgram = self.request[0]
        with suppress_stdout():
            message = OscBundle(dgram).content(0) \
                if OscBundle.dgram_is_bundle(dgram) \
                else OscMessage(dgram)
        logger.info('{address} {params}'.format(address=message.address,
                                                params=message.params))
        if message.address == '/muse/elements/alpha_absolute':
            # print('got alpha ' + str(message.params))
            self.server.queue.append(np.mean(message.params[1:3]))  # storing mean value of abs_alpha in
            #  channels 2 and 3
            self.server.raw_values[0:4] = message.params # set alpha
        elif message.address == '/muse/elements/beta_absolute':
            # print('got beta ' + str(message.params))
            self.server.raw_values[4:8] = message.params # set beta
        elif message.address == '/muse/elements/gamma_absolute':
            # print('got gamma ' + str(message.params))
            self.server.raw_values[8:12] = message.params # set gamma
        elif message.address == '/muse/elements/delta_absolute':
            # print('got delta ' + str(message.params))
            self.server.raw_values[12:16] = message.params # set delta
        elif message.address == '/muse/elements/theta_absolute':
            # print('got theta ' + str(message.params))
            self.server.raw_values[16:20] = message.params # set theta
        elif message.address == '/muse/elements/blink':
            self.server.increment_blink()
            self.server.raw_values[20] = self.server.blink_events # set blink
        elif message.address == '/muse/acc':
            pass
            # think how to store accelerometer data, we'll need it to detect if person moved too much


class OscUDPServer(socketserver.UDPServer):
    def __init__(self, server_address):
        super().__init__(server_address, OscUDPHandler)


class ThreadingOscUDPServer(socketserver.ThreadingMixIn, OscUDPServer):

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        signal.signal(signal.SIGINT, self._signal_handler)
        self.rabbit = RabbitController('localhost', 5672, 'guest', 'guest', '/')
        self.queue = deque(maxlen=QUEUE_SIZE)  # we only use append, therefore no need in queue.Queue
        self.blink_events = 0  # counter of blink events
        self.state = None
        self.state_last_published = None
        self.levels_advance_upwards = True # auto advance is moving upwards
        self.raw_values = [0] * 22
        self.lock = Lock()
        self._stop = Event()
        self._timer_thread = None
        self.start_emitting_messages()

    def increment_blink(self):
        with self.lock:
            self.blink_events += 1

    def _signal_handler(self, _, unused_frame):
        self._stop.set()

    def start_emitting_messages(self):
        self.state = 1
        self.rabbit.publish_state(self.state)
        self.state_last_published = datetime.datetime.now()
        Thread(target=self.predict_next_level, daemon=True).start()
        Thread(target=self.update_rawvalues, daemon=True).start()
        Thread(target=self.listen_for_keys, daemon=True).start()
        Thread(target=self.auto_advance_level, daemon=True).start()

    def predict_next_level(self):
        rabbit = RabbitController('localhost', 5672, 'guest', 'guest', '/')
        while not self._stop.is_set():
            self._stop.wait(EMIT_STAGE_PERIOD_SECONDS)
            prior_state = self.state
            try:
                data = np.array(self.queue, dtype=np.float64)
                model = arima_model.ARIMA(data, order=ARIMA_PARAMS)
                model = model.fit(disp=0)
                forecast = model.predict(start=1, end=20)
            except ValueError:
                print("Skipping state evaluation due to insufficient amount of data collected.")
                return
            data_filtered = data[np.where(np.logical_and(np.greater_equal(data, np.percentile(data, 5)),
                                                         np.less_equal(data, np.percentile(data, 95))))]
            mean_diff = np.mean(forecast) - np.mean(data_filtered)
            # add more logic there considering movement and blinks
            if mean_diff > UPPER_THRESHOLD:
                self.state = min(self.state + 1, 5)
                self.levels_advance_upwards = True # auto advance up after upward transition
            elif mean_diff < LOWER_THRESHOLD:
                self.state = max(self.state - 1, 1)
                self.levels_advance_upwards = False # auto advance down after downward transition

            # send to the bus
            if self.state != prior_state:
                print("[ ] EMITTING STATE: %s" %(self.state))
                rabbit.publish_state(self.state)
                self.state_last_published = datetime.datetime.now()

            with self.lock:
                self.blink_events = 0

    def update_rawvalues(self):
        rabbit = RabbitController('localhost', 5672, 'guest', 'guest', '/')
        while not self._stop.is_set():
            self._stop.wait(EMIT_EEGDATA_PERIOD_SECONDS)
            # set state in raw_valceiceilues
            self.raw_values[21] = int(self.state)
            # send to the bus
            print("[ ] EMITTING EEGDATA: %s" %(self.raw_values))
            rabbit.publish_eegdata(self.raw_values)

    def listen_for_keys(self):
        rabbit = RabbitController('localhost', 5672, 'guest', 'guest', '/')
        while not self._stop.is_set():
            self._stop.wait(LISTEN_FOR_KEY_SECONDS)
            if msvcrt.kbhit():
                ch = msvcrt.getch()
                if ch:
                    key = ch.decode()
                    if key in ['1', '2', '3', '4', '5']:
                        self.state = int(key)
                        print("[ ] PRESSED KEY '%s', EMITTING STATE: %s" %(key, self.state))
                        rabbit.publish_state(self.state)
                        self.state_last_published = datetime.datetime.now()

    def auto_advance_level(self):
        rabbit = RabbitController('localhost', 5672, 'guest', 'guest', '/')
        while not self._stop.is_set():
            self._stop.wait(AUTO_ADVANCE_LEVEL_PERIOD_SECONDS)

            # headset is worn if nonzero values exist in the queue
            headset_worn = False
            for value in self.raw_values[0:20]:
                if value > 0:
                    headset_worn = True
                    break

            seconds_since_state_publish = (datetime.datetime.now() - self.state_last_published).total_seconds()

            should_advance_headset_worn = headset_worn and seconds_since_state_publish > AUTO_ADVANCE_AFTER_SECONDS_WHILE_HEADSET_WORN
            should_advance_headset_off = not headset_worn and seconds_since_state_publish > AUTO_ADVANCE_AFTER_SECONDS_WHILE_HEADSET_OFF

            should_advance = should_advance_headset_worn or should_advance_headset_off

            if should_advance:
                if self.state >= 5:
                    self.levels_advance_upwards = False
                if self.state <= 1:
                    self.levels_advance_upwards = True

                next_state = self.state + 1 if self.levels_advance_upwards else self.state - 1
                self.state = next_state

                advancing_direction = "UPWARDS" if self.levels_advance_upwards else "DOWNWARDS"
                print("[ ] AUTOMATICALLY ADVANCING %s, EMITTING STATE: %s" %(advancing_direction, self.state))
                rabbit.publish_state(self.state)
                self.state_last_published = datetime.datetime.now()

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument("--ip",
                        default="0.0.0.0", help="The ip to listen on")
    parser.add_argument("--port",
                        type=int, default=7000, help="The port to listen on")
    args = parser.parse_args()

    logging.basicConfig(level=logging.WARNING,
                        format='%(asctime)s %(levelname)-8s %(message)s')

    server = ThreadingOscUDPServer((args.ip, args.port))
    print("Serving on {}".format(server.server_address))

    server.serve_forever()
    self._stop.set()
