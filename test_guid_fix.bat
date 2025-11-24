@echo off
cls
echo ========================================
echo   GUID to String Fix - Quick Test
echo ========================================
echo.
echo This will rebuild and test the application
echo with the new GUID to String converter.
echo.
echo Press any key to continue...
pause > nul
echo.

echo [1/3] Cleaning build artifacts...
dotnet clean --verbosity quiet > nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Clean failed!
    pause
    exit /b 1
)
echo      SUCCESS
echo.

echo [2/3] Building application...
dotnet build --no-incremental --verbosity minimal
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ========================================
    echo ERROR: Build failed!
    echo ========================================
    pause
    exit /b 1
)
echo.

echo ========================================
echo   Build Successful!
echo ========================================
echo.
echo [3/3] Starting application...
echo.
echo Test credentials:
echo   Username: admin
echo   Password: admin123
echo.
echo Expected result:
echo   - Login window appears
echo   - No "InvalidCastException" error
echo   - Login succeeds
echo   - Dashboard opens
echo.
echo ========================================
echo.

dotnet run

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Application exited with error code: %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)

pause
