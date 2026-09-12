@echo off
echo ===================================
echo   Talkift Chat Server
echo ===================================
echo.

cd /d "%~dp0"

if not exist "data" mkdir data

echo Usage: run.bat [--port 8080]
echo.

go run ./cmd/server/main.go %*

pause
