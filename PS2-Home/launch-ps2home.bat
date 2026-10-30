@echo off
setlocal

set "ROOT_DIR=%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%ROOT_DIR%launch-ps2home.ps1" %*
exit /b %ERRORLEVEL%
