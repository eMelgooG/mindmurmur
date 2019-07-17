import pika
import logging
import appconfig as cfg
import traceback

logger = logging.getLogger(__name__)

# This was largely sourced from https://www.devmashup.com/creating-a-rabbitmq-publisher-in-python/


class Publisher:
    def __init__(self, config):
        self.config = config

    def publish_heart(self, message):
        connection = None
        try:
            connection = self._create_connection()
            channel = connection.channel()
            props = pika.BasicProperties(type=cfg.EXCHANGE_HEART, delivery_mode=2)

            channel.exchange_declare(exchange=cfg.EXCHANGE_HEART,
                                     passive=True)
            channel.basic_publish(exchange=cfg.EXCHANGE_HEART,
                                  routing_key='',
                                  properties=props,
                                  body=message.to_json())

            print(" [x] Sent heart message %r {0}" % message, message.to_json())
        except Exception as e:
            print(repr(e))
            traceback.print_exc()
            raise e
        finally:
            if connection:
                connection.close()

    def publish_color(self, message):
        connection = None
        try:
            connection = self._create_connection()
            channel = connection.channel()
            props = pika.BasicProperties(type=cfg.EXCHANGE_COLOR, delivery_mode=2)

            channel.exchange_declare(exchange=cfg.EXCHANGE_COLOR,
                                     passive=True)
            channel.basic_publish(exchange=cfg.EXCHANGE_COLOR,
                                  properties=props,
                                  routing_key='',
                                  body=message.to_json())

            print(" [x] Sent color message %r {0}" % message, message.to_json())
        except Exception as e:
            print(repr(e))
            traceback.print_exc()
            raise e
        finally:
            if connection:
                connection.close()

    def _create_connection(self):
        credentials = pika.PlainCredentials(self.config['userName'], self.config['password'])
        parameters = pika.ConnectionParameters(self.config['host'], self.config['port'],
                                               self.config['virtualHost'], credentials, ssl=False)
        return pika.BlockingConnection(parameters)