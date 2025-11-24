@echo off
echo ????????????????????????????????????????????????????
echo ?   SNOOKER CLUB - AUTO FIX SCRIPT                ?
echo ????????????????????????????????????????????????????
echo.

set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 8.0\bin

echo [1/6] Checking MySQL Service...
sc query MySQL80 | find "RUNNING" >nul
if errorlevel 1 (
    echo       MySQL not running. Starting...
    net start MySQL80
    timeout /t 2 >nul
) else (
    echo       ? MySQL is running
)
echo.

echo [2/6] Creating database if not exists...
"%MYSQL_PATH%\mysql.exe" -u root -proot -e "CREATE DATABASE IF NOT EXISTS snooker_club_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;" 2>nul
if errorlevel 1 (
    echo       ? Failed to create database
    pause
    exit /b 1
) else (
    echo       ? Database ready
)
echo.

echo [3/6] Importing schema...
"%MYSQL_PATH%\mysql.exe" -u root -proot snooker_club_db < Database\SnookerDB_Schema.sql 2>nul
if errorlevel 1 (
    echo       ? Schema import failed
    pause
    exit /b 1
) else (
    echo       ? Schema imported
)
echo.

echo [4/6] Verifying data...
for /f "skip=1" %%a in ('"%MYSQL_PATH%\mysql.exe" -u root -proot snooker_club_db -N -e "SELECT COUNT(*) FROM app_user;" 2^>nul') do set USER_COUNT=%%a
echo       Found %USER_COUNT% user(s) in database
if "%USER_COUNT%"=="1" (
    echo       ? Admin user exists
) else (
    echo       ? Admin user missing - re-importing...
    "%MYSQL_PATH%\mysql.exe" -u root -proot snooker_club_db < Database\SnookerDB_Schema.sql 2>nul
)
echo.

echo [5/6] Cleaning and building application...
call dotnet clean >nul 2>&1
call dotnet build >nul 2>&1
if errorlevel 1 (
    echo       ? Build failed
    pause
    exit /b 1
) else (
    echo       ? Build successful
)
echo.

echo [6/6] Copying configuration...
if exist "appsettings.json" (
    copy /Y appsettings.json bin\Debug\net10.0-windows\ >nul 2>&1
    echo       ? Config copied
) else (
    echo       ? appsettings.json not found!
)
echo.

echo ????????????????????????????????????????????????????
echo ?   SETUP COMPLETE!                               ?
echo ????????????????????????????????????????????????????
echo.
echo Your login credentials:
echo   Username: admin
echo   Password: admin123
echo.
echo Press any key to start the application...
pause >nul

echo.
echo Starting application...
start "" "bin\Debug\net10.0-windows\SnookerGameManagementSystem.exe"

echo.
echo Application should open in a few seconds.
echo If login doesn't work, check the Output window in Visual Studio.
echo.
pause
