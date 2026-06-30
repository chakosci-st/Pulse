@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "PENDING_SCRIPT=%SCRIPT_DIR%run-pending-tasks-summary.ps1"
set "NOTIFICATIONS_SCRIPT=%SCRIPT_DIR%run-notifications-table.ps1"
set "MILESTONE_READY_SCRIPT=%SCRIPT_DIR%run-milestone-ready-notifications.ps1"
set "POWERSHELL_EXE=%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe"
set "MODE=%~1"

if "%MODE%"=="" set "MODE=both"

if /I "%MODE%"=="/?" goto :usage
if /I "%MODE%"=="-h" goto :usage
if /I "%MODE%"=="--help" goto :usage

if not exist "%POWERSHELL_EXE%" ( 
    echo PowerShell executable not found: "%POWERSHELL_EXE%"
    exit /b 1
)

if /I "%MODE%"=="both" goto :run_both
if /I "%MODE%"=="all" goto :run_all
if /I "%MODE%"=="pending" goto :run_pending
if /I "%MODE%"=="notifications" goto :run_notifications
if /I "%MODE%"=="milestone-ready" goto :run_milestone_ready

echo Invalid mode: %MODE%
echo.
goto :usage

:run_both
call :run_script "%PENDING_SCRIPT%" "pending-tasks-summary"
if errorlevel 1 exit /b 1
call :run_script "%NOTIFICATIONS_SCRIPT%" "notifications-table"
if errorlevel 1 exit /b 1
echo Completed both Pulse jobs.
exit /b 0

:run_all
call :run_script "%PENDING_SCRIPT%" "pending-tasks-summary"
if errorlevel 1 exit /b 1
call :run_script "%NOTIFICATIONS_SCRIPT%" "notifications-table"
if errorlevel 1 exit /b 1
call :run_script "%MILESTONE_READY_SCRIPT%" "milestone-ready-notifications"
if errorlevel 1 exit /b 1
echo Completed all Pulse jobs.
exit /b 0

:run_pending
call :run_script "%PENDING_SCRIPT%" "pending-tasks-summary"
exit /b %errorlevel%

:run_notifications
call :run_script "%NOTIFICATIONS_SCRIPT%" "notifications-table"
exit /b %errorlevel%

:run_milestone_ready
call :run_script "%MILESTONE_READY_SCRIPT%" "milestone-ready-notifications"
exit /b %errorlevel%

:run_script
set "TARGET_SCRIPT=%~1"
set "TARGET_NAME=%~2"

if not exist "%TARGET_SCRIPT%" ( 
    echo Script not found: "%TARGET_SCRIPT%"
    exit /b 1
)

echo Running %TARGET_NAME%...
"%POWERSHELL_EXE%" -NoProfile -ExecutionPolicy Bypass -File "%TARGET_SCRIPT%"
if errorlevel 1 ( 
    echo %TARGET_NAME% failed.
    exit /b 1
)

echo %TARGET_NAME% completed.
exit /b 0

:usage
echo Usage:
echo   run-pulse-jobs.bat [both^|all^|pending^|notifications^|milestone-ready]
echo.
echo Examples:
echo   run-pulse-jobs.bat
echo   run-pulse-jobs.bat both
echo   run-pulse-jobs.bat all
echo   run-pulse-jobs.bat pending
echo   run-pulse-jobs.bat notifications
echo   run-pulse-jobs.bat milestone-ready
exit /b 1