@echo off
powershell -NoProfile -ExecutionPolicy ByPass -File "%~d0%~p0MusketeerDeploy.ps1" -Action install

