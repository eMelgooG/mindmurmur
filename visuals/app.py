import wx

class RenderFrame(wx.Frame):
    def __init__(self, parent):
        self.flame = OpenFlame("playa.flame")
        self.bmp = 
        self.oldbmp


    def OpenFlame(self, path):
        if os.path.exists(path):
            # scan the file to see if it's valid
            self.flamestrings = load_flamestrings(path)
            if not flamestrings:
                ErrorMessage(self, "%s is not a valid flame file."
                             " Please choose a different file." % path)
                self.OnFlameOpen(None)
                return
        else:
            raise FileNotFoundError(path)


def split_flamestrings(string):
    return re.findall(r'<flame .*?</flame>', string, re.DOTALL)


def load_flamestrings(filename):
    """Reads a flame file and returns a list of flame strings."""
    return split_flamestrings(open(filename).read())


if __name__ == "main":
    app = wx.App(False)
    frame =