@echo off
echo ?????????????????????????????????????????????????????
echo ?   FINAL LOGIN TEST - RUN THIS NOW!               ?
echo ?????????????????????????????????????????????????????
echo.

echo Step 1: Database Check
echo ----------------------
& "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -proot snooker_club_db -e "SELECT 'DB Connected' as Status, (SELECT COUNT(*) FROM app_user) as Users, (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='snooker_club_db') as Tables;" 2>nul

if errorlevel 1 (
    echo ? Database check FAILED
    pause
    exit /b 1
) else (
    echo ? Database check PASSED
)

echo.
echo Step 2: Admin User Check
echo ------------------------
& "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -proot snooker_club_db -e "SELECT username, SUBSTRING(password_hash, 1, 20) as hash FROM app_user WHERE username='admin';" 2>nul

echo.
echo Step 3: Configuration Check
echo ---------------------------
if exist "bin\Debug\net10.0-windows\appsettings.json" (
    echo ? appsettings.json exists in output folder
) else (
    echo ? appsettings.json MISSING - copying now...
    copy /Y appsettings.json bin\Debug\net10.0-windows\ >nul
)

echo.
echo Step 4: Starting Application
echo ----------------------------
echo.
echo The app will now launch.
echo.
echo IMPORTANT:
echo ----------
echo 1. After you enter credentials and click LOGIN
echo 2. Open Visual Studio
echo 3. Go to: View -^> Output (Ctrl+Alt+O)
echo 4. Select "Debug" from the dropdown
echo 5. Look for these messages:
echo    - "[LoginWindow] Login button clicked!"
echo    - "[AuthService] Attempting authentication..."
echo    - "[AuthService] Password verification result: True/False"
echo.
echo LOGIN CREDENTIALS:
echo ------------------
echo Username: admin
echo Password: admin123
echo.
pause

echo.
echo Launching app...
start "" "bin\Debug\net10.0-windows\SnookerGameManagementSystem.exe"

echo.
echo App launched! Check the login window.
echo.
echo If login fails, the debug output will show exactly what's wrong.
echo.
pause
