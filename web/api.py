import traceback


class API():

    def __init__(self, bus, runner):
        self.bus = bus
        self.runner = runner

    def history(self, since):
        return {
            'state': self.bus.get_state_history(since),
            'heart_rate': self.bus.get_heart_rate_history(since),
        }

    def send_state(self, name):
        try:
            self.bus.send_state(name)
            return True, 'Meditation Level ' + name + ' set!'
        except:
            return False, 'Meditation Level ' + name + ' could not be set: ' + traceback.format_exc()

    def send_heart_rate(self, name):
        try:
            self.bus.send_heart_rate(name)
            return True, 'Heart Rate ' + name + ' set!'
        except:
            return False, 'Heart Rate ' + name + ' could not be set: ' + traceback.format_exc()
