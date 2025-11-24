@echo off
echo ========================================
echo Testing Database Connection
echo ========================================
echo.

set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 8.0\bin

echo Testing connection to snooker_club_db...
echo.

REM Test if database exists and has tables
"%MYSQL_PATH%\mysql.exe" -u root -proot -e "USE snooker_club_db; SELECT 'Connection successful!' as Status; SELECT COUNT(*) as 'Tables' FROM information_schema.tables WHERE table_schema = 'snooker_club_db';"

if errorlevel 1 (
    echo.
    echo ERROR: Connection failed or database doesn't exist!
    echo.
    pause
    exit /b 1
)

echo.
echo Checking if app_user table exists...
"%MYSQL_PATH%\mysql.exe" -u root -proot snooker_club_db -e "DESCRIBE app_user;"

if errorlevel 1 (
    echo.
    echo ERROR: app_user table doesn't exist!
    echo You need to import the schema first.
    echo.
    echo Run this command:
    echo mysql -u root -proot snooker_club_db ^< Database\SnookerDB_Schema.sql
    echo.
    pause
    exit /b 1
)

echo.
echo Checking admin user...
"%MYSQL_PATH%\mysql.exe" -u root -proot snooker_club_db -e "SELECT username, SUBSTRING(password_hash, 1, 20) as 'password_hash_preview' FROM app_user WHERE username='admin';"

if errorlevel 1 (
    echo.
    echo ERROR: Admin user doesn't exist!
    pause
    exit /b 1
)

echo.
echo ========================================
echo Database setup looks good!
echo ========================================
echo.
echo You can now run the application.
echo.
echo Login credentials:
echo Username: admin
echo Password: admin123
echo.
pause
