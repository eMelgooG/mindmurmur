import json
import uuid


class ColorControlCommand(object):
    """An instance of a color control command

    Attributes:
        CommandId:   Unique id of the command
        ColorRed: Red value of RGB
        ColorGreen: Green value of RGB
        ColorBlue: Blue value of RGB
    """

    def __init__(self):
        self.CommandId = uuid.uuid4()
        self.ColorRed = 0
        self.ColorGreen = 0
        self.ColorBlue = 0

    def __init__(self, commandid, color_red, color_green, color_blue):
        self.CommandId = commandid
        self.ColorRed = color_red
        self.ColorGreen = color_green
        self.ColorBlue = color_blue

    def set_colors(self, color_red, color_green, color_blue):
        self.ColorRed = color_red
        self.ColorGreen = color_green
        self.ColorBlue = color_blue

    def to_json(self):
        return json.dumps(self, default=lambda o: o.__dict__,sort_keys=True, indent=4)

    def to_string(self):
        return "({0}, {1}, {2}, {3})".format(self.CommandId, self.ColorRed, self.ColorGreen, self.ColorBlue)
