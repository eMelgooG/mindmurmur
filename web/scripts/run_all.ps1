$ScriptRoot = $PSScriptRoot -replace ' ', '` '

start powershell ($ScriptRoot + "\run_osc.ps1")
start powershell ($ScriptRoot + "\run_lights.ps1")
start powershell ($ScriptRoot + "\run_fr0st.ps1")
start powershell ($ScriptRoot + "\run_sound.ps1")