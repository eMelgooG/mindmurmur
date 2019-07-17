import os
import sys


class Runner():

    def __init__(self):
        self.base_path = os.path.dirname(os.path.realpath(__file__))

    def run(self, name):
        script_name = os.path.join(self.base_path, 'scripts', 'run_' + name + '.ps1')
        if len(sys.argv) > 1 and sys.argv[1] == 'test':
            return 0
        return os.system(f"start powershell {script_name.replace(' ', '` ')}")
