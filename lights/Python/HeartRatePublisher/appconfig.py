#!/usr/bin/env python

"""
Config settings for Pika and RabbitMQ
"""
rabbit = {'host': 'localhost',
         'userName': 'guest',
         'password': 'guest',
         'port': '5672',
         'virtualHost': '/',
         'routingKey': '',
         'exchange_color': 'MindMurmur.Domain.Messages.ColorControlCommand, MindMurmur.Domain',
         'exchange_heart': 'MindMurmur.Domain.Messages.HeartRateCommand, MindMurmur.Domain'}
HOST = 'localhost'
PORT = 5672
USER = 'guest'
PASS = 'guest'
VIRTUALHOST = '/'
QUEUE_NAME_COLOR = 'MindMurmur.Domain.Messages.ColorControlCommand, MindMurmur.Domain_colorCommand'
QUEUE_NAME_HEART = 'MindMurmur.Domain.Messages.HeartRateCommand, MindMurmur.Domain_heartRateCommand'
EXCHANGE_COLOR = 'MindMurmur.Domain.Messages.ColorControlCommand, MindMurmur.Domain'
EXCHANGE_HEART = 'MindMurmur.Domain.Messages.HeartRateCommand, MindMurmur.Domain'
