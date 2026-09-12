@echo off
echo ===================================
echo   Talkift Official Server
echo ===================================
echo.

cd /d "%~dp0"

if not exist "data" mkdir data
if not exist "data\user" mkdir data\user
if not exist "data\logs" mkdir data\logs
if not exist "data\version" mkdir data\version
if not exist "data\config" mkdir data\config

echo Usage: run.bat [--port 8081]
echo.

go run ./cmd/main.go %*

pause
