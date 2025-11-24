@echo off
echo ========================================
echo Rebuilding Snooker Management System
echo ========================================
echo.
echo This script will:
echo 1. Clean previous build artifacts
echo 2. Rebuild the application
echo 3. Start the application
echo.

echo [1/3] Cleaning previous build...
dotnet clean --verbosity quiet > nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Clean failed!
    pause
    exit /b 1
)
echo SUCCESS: Build artifacts cleaned
echo.

echo [2/3] Rebuilding application...
dotnet build --no-incremental --verbosity minimal
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ========================================
    echo ERROR: Build failed!
    echo ========================================
    echo Please check the error messages above.
    pause
    exit /b 1
)
echo.
echo ========================================
echo SUCCESS: Build completed!
echo ========================================
echo.

echo [3/3] Starting application...
echo.
echo Login credentials:
echo   Username: admin
echo   Password: admin123
echo.
echo ========================================
echo.

dotnet run

pause
