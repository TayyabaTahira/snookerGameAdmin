@echo off
cls
echo ========================================
echo   FINAL FIX - Use Guid Instead of String
echo ========================================
echo.
echo This solution changes all model IDs from
echo string to Guid to match the database format.
echo.
echo This will:
echo 1. Force close any running instances
echo 2. Clean and rebuild the project
echo 3. Start the application
echo.
pause
echo.

echo Step 1: Force closing all instances...
echo.
taskkill /F /IM SnookerGameManagementSystem.exe 2>nul
if %ERRORLEVEL% EQU 0 (
    echo Application closed successfully
    echo Waiting 3 seconds for file handles to release...
    timeout /t 3 /nobreak >nul
) else (
    echo No running instance found
)
echo.

echo Step 2: Cleaning build artifacts...
dotnet clean --verbosity quiet
if %ERRORLEVEL% NEQ 0 (
    echo WARNING: Clean reported issues, but continuing...
)
echo Build artifacts cleaned
echo.

echo Step 3: Rebuilding application...
echo (This may take a moment...)
echo.
dotnet build --no-incremental --verbosity minimal
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ========================================
    echo ERROR: Build failed!
    echo ========================================
    echo.
    echo Check the errors above. SQL80001 errors
    echo from the schema file can be ignored.
    echo.
    pause
    exit /b 1
)
echo.

echo ========================================
echo   Build Successful!
echo ========================================
echo.
echo ========================================
echo   IMPORTANT CHANGE
echo ========================================
echo All model IDs are now Guid type instead
echo of string. This matches the database format.
echo.
echo Starting application...
echo.
echo Login credentials:
echo   Username: admin
echo   Password: admin123
echo.
echo Expected behavior:
echo   - No InvalidCastException
echo   - Login succeeds
echo   - Dashboard opens
echo.
echo ========================================
echo.

start "" "bin\Debug\net10.0-windows\SnookerGameManagementSystem.exe"

echo.
echo Application started!
echo.
echo If login STILL fails, please share:
echo   1. The exact error message
echo   2. Screenshot of the error
echo   3. Debug Output from Visual Studio
echo.
pause
