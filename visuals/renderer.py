import math
import time
import traceback
import wx

from fr0stlib.pyflam3 import _flam4

class RenderPanel(wx.Panel):
    def __init__(self, parent, renderer):
        wx.Panel.__init__(self, parent)
        self.parent = parent
        self.renderer = renderer
        self.Bind(wx.EVT_PAINT, self.OnPaint)
        self.Bind(wx.EVT_ERASE_BACKGROUND, self.OnEraseBackground)
        self.Bind(wx.EVT_LEFT_DOWN, self.OnLeftDown)
        self.bmp = wx.EmptyBitmap(1, 1, 32)
        self.current_second = math.floor(time.clock())
        self.renders_this_second = 0

    def render(self, flame):
        pw, ph = map(float, self.Size)
        fw, fh = map(float, flame.size)
        ratio = min(pw/fw, ph/fh)
        size = int(fw * ratio), int(fh * ratio)
        self.renderer.enqueue_render(flame, size, self.render_complete)

    def render_complete(self, size, output_buffer):
        this_second = math.floor(time.clock())
        if self.current_second != this_second:
            # rolled over into a new second, print FPS
            #print("Renders/sec: %d" % self.renders_this_second)
            self.current_second = this_second
            self.renders_this_second = 0

        w, h = size
        self.bmp = wx.BitmapFromBufferRGBA(w, h, output_buffer)
        self.Refresh()
        self.renders_this_second += 1

    def OnPaint(self, event):
        fw,fh = self.bmp.GetSize()
        pw,ph = self.Size
        dc = wx.BufferedDC(wx.PaintDC(self))
        dc.DrawBitmap(self.bmp, (pw-fw)/2, (ph-fh)/2, True)

    def OnEraseBackground(self, event):
        pass # avoid flicker

    def OnLeftDown(self, event):
        self.parent.SetFocus()


class RenderFrame(wx.Frame):
    def __init__(self, parent, renderer):
        print("[>] GUI START")
        wx.Frame.__init__(self, parent)

        self.image = RenderPanel(self, renderer)
        sizer = wx.BoxSizer(wx.VERTICAL)
        sizer.Add(self.image, 1, wx.EXPAND)
        self.SetSizer(sizer)
        self.SetDoubleBuffered(True)

        self.Show()
        self.ShowFullScreen(True)
        self.SetWindowStyle(self.GetWindowStyle() | wx.BORDER_NONE | wx.CLIP_CHILDREN)
        self.SetFocus()


    def stop(self):
        self.Hide()
        self.ShowFullScreen(False)
        self.SetWindowStyle(self.GetWindowStyle() & ~wx.STAY_ON_TOP)
        self.Close()


    def render(self, flame):
        self.image.render(flame)


def flam3_render(flame, size, quality, transparent=0, **kwds):
    """Passes render requests on to flam3."""
    frame = Genome.load(to_string(flame), **kwds)
    output_buffer, stats = frame.render(size, quality, transparent)
    return output_buffer


def flam4_render(flame, size, quality, **kwds):
    """Passes requests on to flam4. Works on windows only for now."""
    flame = flame if type(flame) is Flame else Flame(flame)
    flam4Flame = _flam4.loadFlam4(flame)
    output_buffer = _flam4.renderFlam4(flam4Flame, size, quality, **kwds)
    return output_buffer


render_funcs = {'flam3': flam3_render,
                'flam4': flam4_render}

class Renderer():
    def __init__(self):
        self.render_func = render_funcs['flam4']
        self.settings = {'estimator': 0.0,
                         'filter_radius': 0.25,
                         'quality': 10,
                         'spatial_oversample': 2,
                         'progress_func': self.progress}
        self.queue = []
        self.keeprendering = True
        self.start_worker()

    def start_worker(self):
        worker_thread = threading.Thread(target=self.worker)
        worker_thread.daemon = True
        worker_thread.start()

    def worker(self):
        try:
            while self.keeprendering:
                if self.queue:
                    (flame, size, complete_callback) = self.queue.pop()
                    try:
                        output_buffer = self.render_func(flame, size, **self.settings)
                    except Exception:
                        # Make sure rendering never crashes due to malformed flames.
                        traceback.print_exc()
                        next

                    complete_callback(size, output_buffer)
                else:
                    time.sleep(0.005) # max 200 queue inspections/sec
        except wx.PyDeadObjectError:
            pass # happens when Mind Murmur is stopped

    def enqueue_render(self, flame, size, complete_callback):
        if not len(self.queue):
            self.queue.append((flame, size, complete_callback))

    def progress(self, py_object, progress, stage, eta):
        pass
