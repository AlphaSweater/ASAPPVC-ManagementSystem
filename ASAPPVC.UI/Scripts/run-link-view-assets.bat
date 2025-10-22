@echo off
setlocal

:: --- Admin check ---
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo ====================================================
    echo   ERROR: This script must be run as Administrator!
    echo   Right-click this file and choose:
    echo        "Run as administrator"
    echo ====================================================
    pause
    exit /b 1
)

set SCRIPT_DIR=%~dp0

:: Append dot to avoid trailing-backslash-quote issue
powershell -ExecutionPolicy Bypass -NoLogo -NoProfile -File "%SCRIPT_DIR%link-view-assets.ps1" -StartDir "%SCRIPT_DIR%."

echo.
echo ====================================================
echo  Script finished. Press any key to close.
echo ====================================================
pause >nul
