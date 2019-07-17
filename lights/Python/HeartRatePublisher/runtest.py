import pika
import logging
import appconfig as cfg

logger = logging.getLogger(__name__)

credentials = pika.PlainCredentials(cfg.USER, cfg.PASS)
params = pika.ConnectionParameters(cfg.HOST, cfg.PORT, cfg.VIRTUALHOST, credentials)
connection = pika.BlockingConnection(params)
channel = connection.channel()

channel.queue_declare(queue='hello')
props = pika.BasicProperties(user_id=cfg.USER)

channel.basic_publish(exchange='',
                      properties=props,
                      routing_key='hello',
                      body='Boom, looks like you can send messages to Rabbit!')

print(" [x] Sent 'Boom, looks like you can send messages to Rabbit!'")


def callback(ch, method, properties, body):
    print(" [x] Received %r" % body)


channel.basic_consume(callback,
                      queue='hello',
                      no_ack=True)

try:
    input("Press enter to continue")
except SyntaxError:
    pass
