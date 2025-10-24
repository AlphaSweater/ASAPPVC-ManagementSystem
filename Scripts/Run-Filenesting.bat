@echo off
setlocal ENABLEDELAYEDEXPANSION

REM Resolve paths: this BAT lives in the scripts folder at solution root
set "SCRIPT_DIR=%~dp0"
set "PS_SCRIPT=%SCRIPT_DIR%Maintain-Filenesting.ps1"
REM Solution root is the parent of scripts\
for %%I in ("%SCRIPT_DIR%..") do set "SOLUTION_ROOT=%%~fI"

if not exist "%PS_SCRIPT%" (
  echo PowerShell script not found: %PS_SCRIPT%
  pause
  exit /b 1
)

:menu
cls
echo ==============================================
echo  File Nesting Maintenance
echo  Solution: %SOLUTION_ROOT%
echo ==============================================
echo.
echo  1^) Run once (update and exit)
echo  2^) Run in background (watch for changes)
echo  3^) Exit
echo.
set /p "CHOICE=Select an option (1-3): "

if "%CHOICE%"=="1" goto run_once
if "%CHOICE%"=="2" goto run_bg
if "%CHOICE%"=="3" goto done
goto menu

:run_once
echo Running once...
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%PS_SCRIPT%" -SolutionRoot "%SOLUTION_ROOT%"
echo.
echo Done. Press any key to return to menu.
pause >nul
goto menu

:run_bg
echo Starting background watcher (hidden window)...
start "" powershell.exe -NoLogo -NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File "%PS_SCRIPT%" -SolutionRoot "%SOLUTION_ROOT%" -Watch
echo.
echo Watcher started. It will keep running until you close that PowerShell process.
echo Tip: Use Task Manager to end "powershell.exe" if needed.
echo.
echo Press any key to return to menu.
pause >nul
goto menu

:done
endlocal
exit /b 0
