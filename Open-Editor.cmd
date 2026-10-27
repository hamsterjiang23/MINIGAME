@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0UnityProject\Tools\Open-Editor.ps1" %*
if errorlevel 1 pause
