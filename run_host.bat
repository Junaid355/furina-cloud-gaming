@echo off
title Furina Cloud Gaming — Windows Host
cd /d "%~dp0FurinaHost"

echo =====================================================================
echo                FURINA CLOUD GAMING — HOST LAUNCHER
echo =====================================================================
echo Starting host server on port 8080...
echo Make sure your iPhone / Android device is on the same local network.
echo Open Safari or Chrome on your mobile phone and browse to:
echo    http://^<YOUR_PC_IP^>:8080
echo =====================================================================

dotnet run --configuration Release
pause
