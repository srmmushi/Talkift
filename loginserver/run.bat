@echo off
echo ===================================
echo   Talkift Login Server
echo ===================================
echo.

cd /d "%~dp0"

if not exist "data" mkdir data
if not exist "data\logs" mkdir data\logs

echo Starting login server...
go run ./cmd/main.go

pause
