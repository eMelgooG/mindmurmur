:loop
cmd /c ".\env3\scripts\activate && python heart_sensor_service\heartrate_sensor_service.py"
goto loop
pause
