@echo off
setlocal enabledelayedexpansion

echo ========================================
echo COMPREHENSIVE LOGIN DIAGNOSTIC
echo ========================================
echo.

REM Test 1: Check MySQL Service
echo [TEST 1] Checking MySQL Service...
sc query MySQL80 | find "RUNNING" >nul
if !errorlevel! equ 0 (
    echo [PASS] MySQL service is running
) else (
    echo [FAIL] MySQL service is NOT running
    echo        Run: net start MySQL80
    pause
    exit /b 1
)
echo.

REM Test 2: Check Database Exists
echo [TEST 2] Checking if database exists...
"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -proot -e "SHOW DATABASES LIKE 'snooker_club_db';" 2>nul | find "snooker_club_db" >nul
if !errorlevel! equ 0 (
    echo [PASS] Database 'snooker_club_db' exists
) else (
    echo [FAIL] Database 'snooker_club_db' does NOT exist
    echo        Run: mysql -u root -proot -e "CREATE DATABASE snooker_club_db;"
    pause
    exit /b 1
)
echo.

REM Test 3: Check Tables Count
echo [TEST 3] Checking tables in database...
for /f "skip=1" %%a in ('"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -proot snooker_club_db -N -e "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='snooker_club_db';" 2^>nul') do set TABLE_COUNT=%%a

if "!TABLE_COUNT!"=="" set TABLE_COUNT=0

echo        Found !TABLE_COUNT! tables
if !TABLE_COUNT! geq 10 (
    echo [PASS] Tables exist ^(!TABLE_COUNT! tables found^)
) else (
    echo [FAIL] Tables NOT found or incomplete ^(expected 10+, found !TABLE_COUNT!^)
    echo.
    echo        Importing schema now...
    "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -proot snooker_club_db < Database\SnookerDB_Schema.sql 2>nul
    if !errorlevel! equ 0 (
        echo [SUCCESS] Schema imported successfully!
    ) else (
        echo [ERROR] Failed to import schema
        pause
        exit /b 1
    )
)
echo.

REM Test 4: Check Admin User
echo [TEST 4] Checking admin user...
"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -proot snooker_club_db -N -e "SELECT username FROM app_user WHERE username='admin';" 2>nul | find "admin" >nul
if !errorlevel! equ 0 (
    echo [PASS] Admin user exists
) else (
    echo [FAIL] Admin user does NOT exist
    pause
    exit /b 1
)
echo.

REM Test 5: Check Password Hash
echo [TEST 5] Verifying password hash...
for /f "delims=" %%a in ('"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -proot snooker_club_db -N -e "SELECT password_hash FROM app_user WHERE username='admin';" 2^>nul') do set HASH=%%a

if "!HASH!"=="" (
    echo [FAIL] Could not retrieve password hash
) else (
    echo [PASS] Password hash found
    echo        Hash: !HASH:~0,30!...
)
echo.

REM Test 6: Check appsettings.json
echo [TEST 6] Checking appsettings.json...
if exist "appsettings.json" (
    echo [PASS] appsettings.json exists in project root
    findstr /C:"Password=root" appsettings.json >nul
    if !errorlevel! equ 0 (
        echo [PASS] Connection string password is correct
    ) else (
        echo [WARN] Connection string password might be wrong
    )
) else (
    echo [FAIL] appsettings.json NOT found
)
echo.

REM Test 7: Check Build Output
echo [TEST 7] Checking if app is built...
if exist "bin\Debug\net10.0-windows\SnookerGameManagementSystem.exe" (
    echo [PASS] Application is built
    if exist "bin\Debug\net10.0-windows\appsettings.json" (
        echo [PASS] appsettings.json copied to output
    ) else (
        echo [WARN] appsettings.json NOT in output folder
    )
) else (
    echo [FAIL] Application not built - run 'dotnet build'
)
echo.

echo ========================================
echo DIAGNOSTIC SUMMARY
echo ========================================
echo.
echo If all tests passed, try these credentials:
echo   Username: admin
echo   Password: admin123
echo.
echo If login still fails, check Visual Studio Output window
echo for error messages while the app is running.
echo.
pause
