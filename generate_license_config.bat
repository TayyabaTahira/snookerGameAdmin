@echo off
REM =====================================================
REM License Configuration Generator
REM Snooker Game Management System
REM =====================================================

echo.
echo ============================================
echo  Snooker Game - License Generator
echo ============================================
echo.

REM Get MAC Address
echo Detecting MAC Addresses on this machine...
echo.

powershell -Command "Get-NetAdapter | Where-Object {$_.Status -eq 'Up'} | Select-Object Name, MacAddress | Format-Table -AutoSize"

echo.
echo ============================================
echo Copy one of the MAC addresses above
echo ============================================
echo.

set /p MAC_ADDRESS="Enter MAC Address for License (or press Enter to skip): "

if "%MAC_ADDRESS%"=="" (
    echo No MAC address entered. Skipping license configuration.
    goto DATABASE_SETUP
)

echo.
echo MAC Address: %MAC_ADDRESS%
echo.

:DATABASE_SETUP
echo.
echo ============================================
echo  Database Configuration
echo ============================================
echo.

REM Local Database Settings
set /p LOCAL_SERVER="Local MySQL Server (default: localhost): "
if "%LOCAL_SERVER%"=="" set LOCAL_SERVER=localhost

set /p LOCAL_USER="Local MySQL User (default: root): "
if "%LOCAL_USER%"=="" set LOCAL_USER=root

set /p LOCAL_PASSWORD="Local MySQL Password: "
if "%LOCAL_PASSWORD%"=="" (
    echo Warning: No password set for local database
    set LOCAL_PASSWORD=
)

set /p LOCAL_DB="Local Database Name (default: snooker_club_db): "
if "%LOCAL_DB%"=="" set LOCAL_DB=snooker_club_db

echo.
set /p USE_REMOTE="Configure Remote Database? (Y/N): "

if /i "%USE_REMOTE%"=="Y" (
    set /p REMOTE_SERVER="Remote MySQL Server (IP or domain): "
    set /p REMOTE_USER="Remote MySQL User: "
    set /p REMOTE_PASSWORD="Remote MySQL Password: "
    set /p REMOTE_DB="Remote Database Name (default: snooker_club_db): "
    if "!REMOTE_DB!"=="" set REMOTE_DB=snooker_club_db
) else (
    set REMOTE_SERVER=%LOCAL_SERVER%
    set REMOTE_USER=%LOCAL_USER%
    set REMOTE_PASSWORD=%LOCAL_PASSWORD%
    set REMOTE_DB=%LOCAL_DB%
)

REM Generate appsettings.json
echo.
echo Generating appsettings.json...
echo.

(
echo {
echo   "ConnectionStrings": {
echo     "SnookerDb": "Server=%LOCAL_SERVER%;Database=%LOCAL_DB%;User=%LOCAL_USER%;Password=%LOCAL_PASSWORD%;AllowUserVariables=true;UseAffectedRows=false",
echo     "RemoteDb": "Server=%REMOTE_SERVER%;Database=%REMOTE_DB%;User=%REMOTE_USER%;Password=%REMOTE_PASSWORD%;AllowUserVariables=true;UseAffectedRows=false"
echo   },
echo   "License": {
echo     "MacAddress": "%MAC_ADDRESS%"
echo   },
echo   "Logging": {
echo     "LogLevel": {
echo       "Default": "Information",
echo       "Microsoft.EntityFrameworkCore": "Warning"
echo     }
echo   }
echo }
) > appsettings_generated.json

echo ============================================
echo  Configuration Generated Successfully!
echo ============================================
echo.
echo File created: appsettings_generated.json
echo.
echo Next Steps:
echo 1. Review the generated file
echo 2. Copy it to your build output folder
echo 3. Rename to appsettings.json
echo 4. Build and distribute to client
echo.
echo ============================================
echo.

notepad appsettings_generated.json

pause
