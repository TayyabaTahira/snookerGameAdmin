@echo off
echo ========================================
echo Snooker Club Database Setup
echo ========================================
echo.

REM Set MySQL path (try common locations)
set MYSQL_PATH=
if exist "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" (
    set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 8.0\bin
)
if exist "C:\Program Files (x86)\MySQL\MySQL Server 8.0\bin\mysql.exe" (
    set MYSQL_PATH=C:\Program Files (x86)\MySQL\MySQL Server 8.0\bin
)

if "%MYSQL_PATH%"=="" (
    echo ERROR: MySQL installation not found!
    echo Please install MySQL Server first.
    pause
    exit /b 1
)

echo MySQL found at: %MYSQL_PATH%
echo.

REM Check if MySQL service is running
sc query MySQL80 | find "RUNNING" >nul
if errorlevel 1 (
    echo MySQL service is not running. Starting...
    net start MySQL80
)

echo.
echo Step 1: Creating database...
echo Enter your MySQL root password when prompted.
echo.
"%MYSQL_PATH%\mysql.exe" -u root -p -e "CREATE DATABASE IF NOT EXISTS snooker_club_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"

if errorlevel 1 (
    echo.
    echo ERROR: Failed to create database!
    echo Please check your password and try again.
    pause
    exit /b 1
)

echo.
echo Step 2: Importing schema...
echo Enter your MySQL root password again when prompted.
echo.
"%MYSQL_PATH%\mysql.exe" -u root -p snooker_club_db < Database\SnookerDB_Schema.sql

if errorlevel 1 (
    echo.
    echo ERROR: Failed to import schema!
    pause
    exit /b 1
)

echo.
echo Step 3: Verifying installation...
echo.
"%MYSQL_PATH%\mysql.exe" -u root -p -e "USE snooker_club_db; SHOW TABLES; SELECT COUNT(*) as 'Table Count' FROM information_schema.tables WHERE table_schema = 'snooker_club_db';"

echo.
echo ========================================
echo Database setup complete!
echo ========================================
echo.
echo Next steps:
echo 1. Update appsettings.json with your MySQL password
echo 2. Run the application with: dotnet run
echo.
pause
