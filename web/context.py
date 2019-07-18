import socket

def available_states():
    return [{'id': str(level+1), 'name': str(level+1)} for level in range(5)]


class Context():

    def __init__(self, bus):
        self.bus = bus

    def fetch(self, request):
        hostparts = request.host.split(':')
        port = hostparts[1] if len(hostparts) >= 2 else 80
        return {
            'state': self.bus.get_state(),
            'states': available_states(),
            'state_history': self.bus.get_state_history(),
            'heart_rate': self.bus.get_heart_rate(),
            'heart_rate_history': self.bus.get_heart_rate_history(),
            'ip': socket.gethostbyname(socket.gethostname()),
            'port': port,
        }
