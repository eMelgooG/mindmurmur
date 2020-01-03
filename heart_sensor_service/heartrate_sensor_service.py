import pygatt
import uuid
from construct import BitStruct, BitsInteger, Flag, Bit
import logging
import datetime
import signal
import time
from collections import namedtuple
import argparse
from common.rabbit_controller import RabbitController
from threading import Event

SENSOR_MAC_ADDRESS = '00:a0:50:56:a9:34'
RECEIVE_CHARACTERISTIC = '49535343-1E4D-4BD9-BA61-23C647249616'

DATA_SERVICE_UUID = uuid.UUID('49535343-fe7d-4ae5-8fa9-9fafd205e455')
RENAME_CHARACTERISTIC_UUID = uuid.UUID('00005343-0000-1000-8000-00805F9B34FB')
DATA_READ_TIMEOUT_SECONDS = 10

PARSING_SCHEMA = {
    0: BitStruct(signal_strength=BitsInteger(4), has_signal=Flag, probe_unplugged=Flag, pulse_beep=Flag, sync_bit=Flag),
    1: BitStruct(pleth=BitsInteger(7), sync_bit=Flag, reserved_bit=Bit),
    2: BitStruct(bargraph=BitsInteger(4), no_finger=Flag, pulse_research=Flag, pr_last_bit=Bit, sync_bit=Flag),
    3: BitStruct(pr_bits=BitsInteger(7), sync_bit=Flag),
    4: BitStruct(spo2=BitsInteger(7), sync_bit=Flag)
}

HeartRateData = namedtuple('HeartRateData', ['signal_strength', 'has_signal', 'pleth', 'bargraph', 'no_finger',
                                             'pulse_rate', 'spo2', 'timestamp'])


def to_bytes(n, length, endianess='big'):
    h = '%x' % n
    s = ('0'*(len(h) % 2) + h).zfill(length*2).decode('hex')
    return s if endianess == 'big' else s[::-1]


class HeartrateSensorService(object):

    def __init__(self, adapter, connection_address, polling_interval):
        self.rabbit = RabbitController('localhost', 5672, 'guest', 'guest', '/')
        self.polling_interval = polling_interval
        self.adapter = adapter
        self.connection_address = connection_address
        self.adapter.start()
        self.device = adapter.connect(connection_address)
        self.device.subscribe(RECEIVE_CHARACTERISTIC, callback=self._receive_data_callback)
        self._stop = Event()
        self.heart_rate_value = 0
        signal.signal(signal.SIGINT, self._signal_handler)

    def restart(self):
        self._stop.set()
        try:
            self.device.disconnect()
            self.adapter.clear_bond()
        except pygatt.exceptions.NotConnectedError:
            pass
        self.adapter.start()
        self.device = adapter.connect(self.connection_address)
        self.device.subscribe(RECEIVE_CHARACTERISTIC, callback=self._receive_data_callback)
        self._stop.clear()
        self.publish_heart_rate_data()

    def publish_heart_rate_data(self):
        while not self._stop.is_set():
            try:
                if self.heart_rate_value:
                    print("publishing HR " + str(self.heart_rate_value))
                    self.rabbit.publish_heart(self.heart_rate_value)
                time.sleep(self.polling_interval)
            except Exception:
                logging.exception("Exception while sending data to the message queue:")

    def _signal_handler(self, *args):
        self._stop.set()
        self.device.disconnect()
        self.adapter.stop()

    def _receive_data_callback(self, _, raw_data):
        try:
            packet_dict = {}
            # byte_1_data_container = PARSING_SCHEMA[0].parse(to_bytes(raw_data[0], 1, 'little'))
            # byte_3_data_container = PARSING_SCHEMA[2].parse(to_bytes(raw_data[2], 1, 'little'))
            # packet_dict['signal_strength'] = byte_1_data_container['signal_strength']
            # packet_dict['has_signal'] = byte_1_data_container['has_signal']
            # packet_dict['bargraph'] = byte_3_data_container['bargraph']
            # packet_dict['no_finger'] = byte_3_data_container['no_finger']
            # packet_dict['spo2'] = raw_data[4]
            pleth = int(raw_data[1])
            pulse_rate = int(raw_data[3] | ((raw_data[2] & 0x40) << 1))
            ts = str(datetime.datetime.now())
            # hr_object = HeartRateData(**packet_dict)
            if pleth >= 100 or pulse_rate >= 110:
                # most likely corrupted data
                return
            self.heart_rate_value = pulse_rate
        except Exception as e:
            logging.debug('Error on parsing raw heart rate data:')
            logging.exception(e)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--sensor_mac",
                        type=str, default=SENSOR_MAC_ADDRESS, help="Bluetooth heartrate sensor MAC address")
    parser.add_argument("--read_timeout",
                        type=int, default=DATA_READ_TIMEOUT_SECONDS, help="Timeout for polling heart rate data.")
    args = parser.parse_args()

    logging.basicConfig(level=logging.INFO,
                        format='%(asctime)s %(levelname)-8s %(message)s')

    adapter = pygatt.BGAPIBackend()
    service = HeartrateSensorService(adapter, args.sensor_mac, args.read_timeout)
    service.publish_heart_rate_data()
