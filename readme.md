# Mind Murmur #

## About ##

[Mind Murmur](http://dinafisher.net/mind-murmur/) is an art installation originally exhibited at Burning Man 2018. It will return in 2019.

[Demo video of Mind Murmur in action!](https://youtu.be/ldOr4BrbeaU)

As you meditate on a podium, you wear an EEG headset (Muse headset from 2014), and a machine learning model predicts based on your brainwaves how deeply you are meditating. This gets translated into one of 5 meditation levels which are sent over a RabbitMQ bus on the single laptop that runs the installation. Soundscapes for each of the 5 levels play all around you, drowning out the chaos of life. Your heart rate is fed in and LEDs pulsate in sync. Finally, fractal visuals are played either side of you on the sides of a pyramid.

The software in this repo controls all aspects of the installation. The hardware you will need is:

**Heart Rate Sensor**

- Bluetooth LE dongle with Bluegiga BLED112 or BLED113 chipset, or any PyGATT compatible dongle
  - (Windows' default Bluetooth LE does not work with Python)
- Bluetooth LE Pulse Oximeter, finger clip heart rate sensor (+ batteries)
- [Muse headset](https://choosemuse.com/) from 2014 (Bluetooth LE edition, you may need to source it second hand)

**EEG Reader**

- Android phone
- [Muse Monitor app](https://musemonitor.com/) for Android
- [Velleman K8062 USB DMX controller](https://www.velleman.eu/products/view/?country=nl&lang=en&id=353412)

**LEDs**

- DMX decoders x8 (we use the [Elation ELAR Driver 1 Pro](https://www.elationlighting.com/elar-driver1-pro))
  - The DMX decoders are all daisy-chained off the DMX controller
- RGB LED strips x8, spliced into the terminal block connectors on the DMX decoders.

4 of the LED strips will be for the vertices of the pyramid, and the other 4 are the rings of the chandelier.

**Software**

This repo. Visuals code is based on [Fractal Fr0st](https://code.launchpad.net/fr0st).

## Environment setup ##

This app is Windows only due to:

- flam4 library, which uses CUDA for GPU-accelerated fractal rendering, being closed-source
- Nvidia CUDA being closed-source and nowhere near as fast on linux
- Lights application being currently in C#
- The DMX libs (for controlling the lights) are closed-source and windows-only

The app uses both **Python 2.7** and **Python 3.x** as well as **Visual Studio and C#**. The instructions below will get you a working environment. If not please update the docs!

**WARNING:** You *need* to install the outdated wxPython 3.0.2.0 - do not install wxPython 4 it is very hard to uninstall and DLLs etc will conflict, it is painful.

We recommend using

- Visual Studio for C# development
- Visual Studio Code for Python development

To get the phone to connect you need to configure the laptop you are using as an adhoc wifi hotspot, then you will need to set up Muse Monitor OSC streaming to go to port 7000 so it arrives at the OSC server.

### Python 2 install - 32-bit ###

32-bit python is required because the flam3 and flam4 libraries on which we depend are both 32-bit. Flam4 is closed source, so we cannot rebuild for 64-bit. Flam3's codebase is so old that it's impossible to figure out what they thought would be appropriate lengths for variables when moving to 64-bit, so it cannot be rebuild for 64-bit without a LOT of head-scratching.

- Install a **32-bit** Python 2 to `C:\Python27` (`libflam3.dll` is 32-bit, if you get an error like `%s is not a valid Windows application` it means you are using a 64-bit python)
- Install **32-bit** wxPython 3.0.2.0 (available in the `installers` directory or [Sourceforge](https://sourceforge.net/projects/wxpython/files/wxPython/3.0.2.0/)) to `C:\Python27`
- Install Microsoft Visual C++ 9.0 from [Microsoft](http://aka.ms/vcpython27)

### Python 3 install ###

- Install 64-bit Python 3 in the default location.

### Environment Variables ###

- System environment variables
  - `PYTHON_PATH` `C:\Python27`
  - Add `C:\Python27\Scripts` to PATH

### Python Dependencies ###
All dependencies needed are in requirements.txt.

- Download get-pip.py from [PyPA](https://pip.pypa.io/en/stable/installing/)
- Install pip into both python 2 and 3:
  - `py -2 get-pip.py`
  - `py -3 get-pip.py`
- Run `run_me_first.bat` to set up virtual environments and install dependencies.

### Visual C# ###

The lights application is built in C#.

- Install Visual Studio
- Open `lights\MindMurmur.Lights.sln`
- Build in **Debug** mode. No need to build in Release mode.

### GPU processing with Nvidia CUDA ###

Read documentation at NVIDIA's website: [GPU Accelerated Computing with Python](https://developer.nvidia.com/how-to-cuda-python)

- Download and install [CUDA toolkit](https://developer.nvidia.com/cuda-toolkit) (1.5GB)
- To setup CUDA Python, install [Anaconda python distribution](https://www.continuum.io/downloads)
- Install the latest version of the [Numba package](https://docs.continuum.io/anaconda/packages/pkg-docs)

## Running the Application ##

To start everything, run `run_all.bat`.

To start just one component at a time, there are batch files for each component.

- `run_visuals.bat` runs the fractal renderer, which renders fractals for the given meditation level
- `run_sound.bat` runs the soundscape which plays the sounds for a given meditation level 1-5
- `run_heart_sensor.bat` runs the heart sensor, Bluetooth LE app which sends heart rate from a Bluetooth LE device over the RabbitMQ bus
- `run_lights.bat` runs the .NET binary that controls the LEDs via a USB DMX controller.
- `run_osc.bat` runs the OSC UDP server that receives OSC data over UDP from a phone running either Muse Direct or Muse Monitor with streaming enabled. In 2019 we will be using Muse Monitor.
- `run_web.bat` runs the web server, which can monitor the current heart rate and meditation level from another phone connected via wifi.
- `run_fr0st.bat` runs the now-deprecated fractal fr0st application.

## Optional steps - Streamlining Your Environment ##

- To take away another step from getting the laptop logged in and ready, you can disable the lock screen. Follow steps at https://www.windowscentral.com/how-disable-windows-10-lock-screen
- You can also allow run_fr0st.bat to start without a user prompt by auto-accepting UAC prompts. Follow the steps at https://mywindowshub.com/how-to-stop-windows-10-from-asking-for-administrator-rights-to-run-unknown-apps/ and then drag the slider *all the way down*, like so:

![UAC notification slider set to lowest setting](https://trello-attachments.s3.amazonaws.com/5ae22bee8b00b68c6056b5e5/5d360e73957eb41fb3a35732/cd8d4c5f496a4651e1d9b38adcaa4433/CleanShot_2019-07-23_at_18.53.38%402x.png)

## Appendix: `fr0st-master` Directory

This is not used any more, now that we have a standalone renderer in the visuals directory. Therefore the docs below are here mainly for posterity and your edumacation.

Under the fr0st folder, are stored
- **parameters**: library of flames, used as shapes for generating the fractals
- **scripts**: the scripts to run to animate and render the flames
- **config.cfg**: configuration file. Delete it to restore defaults.

This folder should be the default folder used by the app to load config from.
Depending on your folder location or operating system, you might want to **allow access to this folder** for python to run.

What we actually use is `playa.flame` and `fr0stlib` which are both duplicated under the visuals directory.

## Appendix: Troubleshooting ##

- If you get an error like `%s is not a valid Win32 application` you are using a 64-bit python 2 to run the renderer or fractal fr0st. Uninstall your 64-bit Python 2 and replace with a 32-bit then go through the steps again.