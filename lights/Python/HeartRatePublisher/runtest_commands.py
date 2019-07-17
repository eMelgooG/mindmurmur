import pika
import random
import logging
import appconfig as cfg
import uuid
import time
from models import ColorControlCommand as color
from models import HeartRateCommand as hr
from send import Publisher

logger = logging.getLogger(__name__)

pub = Publisher(cfg.rabbit)

credentials = pika.PlainCredentials(cfg.USER, cfg.PASS)
params = pika.ConnectionParameters(cfg.HOST, cfg.PORT, cfg.VIRTUALHOST, credentials)
connection = pika.BlockingConnection(params)
channel = connection.channel()

i = 1
while i < 30:
    print(i)

    colorCmd = color.ColorControlCommand(str(uuid.uuid4()), random.randint(0, 255), random.randint(0, 255),
                                         random.randint(0, 255))
    heartCmd = hr.HeartRateCommand(random.randint(60, 95))
    #  print(colorCmd.to_json())
    #  print(heartCmd.to_json())

    # publish heart command
    pub.publish_heart(heartCmd)
    time.sleep(10)

    # publish color command
    pub.publish_color(colorCmd)

    i += 1
    time.sleep(10)

# close connection
connection.close()
