@echo off
echo ========================================
echo Verifying Snooker Club Database Setup
echo ========================================
echo.

set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 8.0\bin

echo Checking database and tables...
echo.
"%MYSQL_PATH%\mysql.exe" -u root -proot snooker_club_db -e "SELECT 'Database exists!' as Status; SELECT COUNT(*) as 'Table Count' FROM information_schema.tables WHERE table_schema = 'snooker_club_db'; SHOW TABLES;"

if errorlevel 1 (
    echo.
    echo ERROR: Could not connect or tables don't exist!
    echo Run import command first.
) else (
    echo.
    echo ========================================
    echo Checking admin user...
    "%MYSQL_PATH%\mysql.exe" -u root -proot snooker_club_db -e "SELECT username, created_at FROM app_user;"
    
    echo.
    echo ========================================
    echo Checking game types...
    "%MYSQL_PATH%\mysql.exe" -u root -proot snooker_club_db -e "SELECT name FROM game_type;"
    
    echo.
    echo ========================================
    echo Setup verification complete!
    echo If you see 10 tables, 1 admin user, and 3 game types,
    echo you're ready to run the application!
    echo.
)

pause
