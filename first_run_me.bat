REM Python 2 setup...

pip2 install virtualenv
py -2 -m virtualenv env2
cmd /c ".\env2\scripts\activate && pip install -e . && pip install -r requirements.txt"

REM Python 2 setup completed.


REM Python 3 setup...

pip3 install virtualenv
py -3 -m venv env3
cmd /c ".\env3\scripts\activate && pip install -e . && pip install -r requirements.txt"

REM Python 3 setup completed.


pause