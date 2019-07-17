import pika
import logging
import appconfig as cfg

# This is not really needed, but useful if testing is needed on receiving.

logger = logging.getLogger(__name__)

connection = pika.BlockingConnection(pika.ConnectionParameters(host=cfg.HOST))
channel = connection.channel()

channel.queue_declare(queue='hello')
channel.queue_declare(queue=cfg.QUEUE_NAME_COLOR, durable=True)
channel.queue_declare(queue=cfg.QUEUE_NAME_HEART, durable=True)


def callback_color(ch, method, properties, body):
    print(" [x] Received Color %r" % body)


def callback_heart(ch, method, properties, body):
    print(" [x] Received Heart %r" % body)


def callback(ch, method, properties, body):
    print(" [x] Received hello %r" % body)


channel.basic_consume(callback,
                      queue='hello',
                      no_ack=True)

channel.basic_consume(callback_color,
                      queue=cfg.QUEUE_NAME_COLOR,
                      no_ack=True)

channel.basic_consume(callback_heart,
                      queue=cfg.QUEUE_NAME_HEART,
                      no_ack=True)

print(' [*] Waiting for messages. To exit press CTRL+C')
channel.start_consuming()
