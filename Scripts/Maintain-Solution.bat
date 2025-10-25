@echo off
setlocal

:: --- Admin check (same as your old script) ---
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

set "SCRIPT_DIR=%~dp0"

echo Running PowerShell script:
echo   "%SCRIPT_DIR%maintain-solution.ps1"
echo.

:: Run via Windows PowerShell; -File avoids parameter-set weirdness
powershell.exe -ExecutionPolicy Bypass -NoLogo -NoProfile -File "%SCRIPT_DIR%maintain-solution.ps1"
set "EC=%ERRORLEVEL%"

echo.
echo Finished with exit code: %EC%
echo.
echo ====================================================
echo  Script finished. Press any key to close.
echo ====================================================
pause >nul
exit /b %EC%
