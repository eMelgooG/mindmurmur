cmd /c ".\env3\scripts\activate && cd web && start python webapp.py"
timeout 1
start http://localhost:8080
exit