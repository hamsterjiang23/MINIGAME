@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0UnityProject\Tools\Open-Project.ps1" %*
if errorlevel 1 pause
