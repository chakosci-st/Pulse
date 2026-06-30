@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "DEPLOY_SCRIPT=%SCRIPT_DIR%deploy-pulse-console.ps1"
set "POWERSHELL_EXE=%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe"
set "MODE=%~1"

if "%MODE%"=="" set "MODE=deploy"

if /I "%MODE%"=="/?" goto :usage
if /I "%MODE%"=="-h" goto :usage
if /I "%MODE%"=="--help" goto :usage

if not exist "%POWERSHELL_EXE%" (
    echo PowerShell executable not found: "%POWERSHELL_EXE%"
    exit /b 1
)

if not exist "%DEPLOY_SCRIPT%" (
    echo Deploy script not found: "%DEPLOY_SCRIPT%"
    exit /b 1
)

if /I "%MODE%"=="deploy" goto :deploy
if /I "%MODE%"=="deploy-and-register" goto :deploy_and_register

echo Invalid mode: %MODE%
echo.
goto :usage

:deploy
echo Running deployment...
"%POWERSHELL_EXE%" -NoProfile -ExecutionPolicy Bypass -File "%DEPLOY_SCRIPT%"
if errorlevel 1 (
    echo Deployment failed.
    exit /b 1
)

echo Deployment completed.
exit /b 0

:deploy_and_register
echo Running deployment and scheduled-task registration...
"%POWERSHELL_EXE%" -NoProfile -ExecutionPolicy Bypass -File "%DEPLOY_SCRIPT%" -RegisterTasks
if errorlevel 1 (
    echo Deployment failed.
    exit /b 1
)

echo Deployment and task registration completed.
exit /b 0

:usage
echo Usage:
echo   deploy-pulse-console.bat [deploy^|deploy-and-register]
echo.
echo Examples:
echo   deploy-pulse-console.bat
echo   deploy-pulse-console.bat deploy
echo   deploy-pulse-console.bat deploy-and-register
exit /b 1