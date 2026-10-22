@echo off
:: Self-elevating batch script to unblock Furina Cloud Gaming in Windows Firewall
setlocal enabledelayedexpansion

echo =====================================================================
echo   FURINA CLOUD GAMING - ONE-CLICK FIREWALL UNBLOCKER
echo =====================================================================
echo.

:: Check for Administrator privileges
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [!] Requesting Administrator privileges to update Windows Firewall...
    powershell -Command "Start-Process cmd.exe -ArgumentList '/c \"\"%~f0\"\"' -Verb RunAs"
    exit /b
)

echo [+] Running with Administrator privileges!
echo.
echo [1/3] Removing existing Windows Firewall block rules for FurinaHost...
netsh advfirewall firewall delete rule name="FurinaHost" >nul 2>&1
netsh advfirewall firewall delete rule name="Furina Cloud Gaming (Port 8080)" >nul 2>&1
netsh advfirewall firewall delete rule name="Furina Cloud Gaming (App)" >nul 2>&1

echo [2/3] Adding Inbound ALLOW rule for TCP Port 8080 (All Profiles: Public, Private, Domain)...
netsh advfirewall firewall add rule name="Furina Cloud Gaming (Port 8080)" dir=in action=allow protocol=TCP localport=8080 profile=any >nul

echo [3/3] Adding Inbound ALLOW rule for FurinaHost Application...
netsh advfirewall firewall add rule name="Furina Cloud Gaming (App)" dir=in action=allow program="d:\cloud\FurinaHost\bin\Debug\net10.0-windows\FurinaHost.exe" enable=yes profile=any >nul

echo.
echo =====================================================================
echo   [SUCCESS] Windows Firewall is now configured!
echo   Port 8080 is unblocked for iPhone, iPad, and Android devices.
echo =====================================================================
echo.
echo You can now refresh the page on your iPhone (http://192.168.1.197:8080).
echo.
pause
