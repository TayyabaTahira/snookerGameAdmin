@echo off
echo ========================================
echo Quick Database Check
echo ========================================
echo.

set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 8.0\bin

echo Checking if admin user exists...
"%MYSQL_PATH%\mysql.exe" -u root -proot snooker_club_db -e "SELECT username, SUBSTRING(password_hash, 1, 30) as hash_preview FROM app_user WHERE username='admin';"

if errorlevel 1 (
    echo.
    echo ERROR: Cannot find admin user!
    echo.
    echo Importing schema now...
    cd /d "%~dp0"
    "%MYSQL_PATH%\mysql.exe" -u root -proot snooker_club_db < Database\SnookerDB_Schema.sql
    echo.
    echo Schema imported! Try again.
) else (
    echo.
    echo Admin user found! ?
    echo.
    echo Correct credentials:
    echo Username: admin
    echo Password: admin123
    echo.
)

echo.
echo Table count:
"%MYSQL_PATH%\mysql.exe" -u root -proot snooker_club_db -e "SELECT COUNT(*) as Tables FROM information_schema.tables WHERE table_schema = 'snooker_club_db';"

echo.
pause
