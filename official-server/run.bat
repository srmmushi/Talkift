@echo off
echo ===================================
echo   Talkift Official Server
echo ===================================
echo.

cd /d "%~dp0"

if not exist "data" mkdir data
if not exist "data\logs" mkdir data\logs

echo Starting official server...
go run ./cmd/main.go

pause
