# Python MindMurmur library for sending messages to LED light controls
## RabbitMQ

RabbitMQ is used to provide real time decoupled integration between the python code that is used for fractals and heart rate monitoring and the C# code that is used to control the LED light strips. You will need to install RabbitMQ with all of the default settings.

Everything has been built and tested with python 3.6.4


### Installation
* Prereq: Erlang - http://www.erlang.org/downloads
* RabbitMQ from here http://www.rabbitmq.com/download.html

Run through setup procedure and accept everyhting as default.

The default username and password is *guest*.

### Overview

![alt text](../_resource/LEDFlow.png "LED Integration Overview")



- - - -

## Python

### Checking to make sure it works

#### run_test.py
This can be used to ensure that RabbitMQ has been configured correctly.
You should see a result of

```
[x] Sent Boom, looks like you can send messages to Rabbit!
Press enter to continue
```

## **The Guts**

### Configuration File: appconfig.py

The exchange and queue names have to be configuraed as they are below. These are setup so they are aligned to what EasyNetQ (https://github.com/EasyNetQ/EasyNetQ/wiki/Quick-Start) needs in C# for it to work.


_Current configuration_

```
HOST = 'localhost'
PORT = 5672
USER = 'guest'
PASS = 'guest'
VIRTUALHOST = '/'
QUEUE_NAME_COLOR = 'MindMurmur.Domain.Messages.ColorControlCommand, MindMurmur.Domain_colorCommand'
QUEUE_NAME_HEART = 'MindMurmur.Domain.Messages.HeartRateCommand, MindMurmur.Domain_heartRateCommand'
EXCHANGE_COLOR = 'MindMurmur.Domain.Messages.ColorControlCommand, MindMurmur.Domain'
EXCHANGE_HEART = 'MindMurmur.Domain.Messages.HeartRateCommand, MindMurmur.Domain'
```

### Publishing Messages send.py

**Publisher** handles publishing heart rate and color commands to rabbitMQ.

This is how it is done:

```
pub = Publisher(cfg.rabbit)

credentials = pika.PlainCredentials(cfg.USER, cfg.PASS)
params = pika.ConnectionParameters(cfg.HOST, cfg.PORT, cfg.VIRTUALHOST, credentials)
connection = pika.BlockingConnection(params)
channel = connection.channel()
.
.
.
    # publish heart command
    pub.publish_heart(heartCmd)

    # publish color command
    pub.publish_color(colorCmd)

```
This was largely sourced from https://www.devmashup.com/creating-a-rabbitmq-publisher-in-python/

- - - -

## **Messages**

### **ColorControlCommand**
This command is used to send a new color to the LED controller.  
Its default ctor looks like this: _ColorControlCommand(GUID, RED, GREEN, BLUE)_

#### Filename: ColorControlCommand.py

#### Properties:
* CommandId: guid _(this is set automatically)_
* ColorRed: int 
* ColorGreen: int 
* ColorBlue: int 


### **HeartRateCommand**
This command is used to send a new heartrate to the LED controller.  
Its default ctor looks like this: _HeartRateCommand(GUID, HEART_RATE)_

#### Filename: HeartRateCommand.py

#### Properties:
* CommandId: guid _(this is set automatically)_
* HeartRate: int 


- - - - 

## **Running Test**

### runtest_commands.py
This will execute the creation of several test heart rate and color commands.  These will be subscribed by the C# code.  This should provide you with everything you need to get going.

```
1
 [x] Sent heart message <models.HeartRateCommand.HeartRateCommand object at 0x0000024FA8FCE2B0> {0} {
    "CommandId": "9a505545-4e32-42c5-8cac-cb04c2583ba5",
    "HeartRate": 73
}
 [x] Sent color message <models.ColorControlCommand.ColorControlCommand object at 0x0000024FA862B9B0> {0} {
    "ColorBlue": 134,
    "ColorGreen": 38,
    "ColorRed": 91,
    "CommandId": "e972ef7c-3fa4-4d80-b328-e338491340dd"
}
```
