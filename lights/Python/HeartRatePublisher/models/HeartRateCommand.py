import json
import uuid


class HeartRateCommand(object):
    """An instance of a heart rate command

    Attributes:
        CommandId:  Unique id of the command
        HeartRate:  Current Heart rate
    """

    def __init__(self, heart_rate):
        self.CommandId = str(uuid.uuid4())
        self.HeartRate = heart_rate

    def to_json(self):
        return json.dumps(self, default=lambda o: o.__dict__,sort_keys=True, indent=4)

    def to_string(self):
        return "({0}, {1})".format(self.CommandId, self.HeartRate)
