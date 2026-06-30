$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptRoot
$workspaceRoot = Split-Path -Parent $projectRoot
$pythonCandidates = @(
    (Join-Path $workspaceRoot ".venv\Scripts\python.exe"),
    (Join-Path $projectRoot ".venv\Scripts\python.exe")
)
$pythonExe = $pythonCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
$logDir = Join-Path $projectRoot "logs"
$logFile = Join-Path $logDir "pending-tasks-summary.log"

if (-not $pythonExe) {
    throw "Python executable not found. Checked: $($pythonCandidates -join '; ')"
}

New-Item -ItemType Directory -Path $logDir -Force | Out-Null

$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
Add-Content -Path $logFile -Value "[$timestamp] START pending-tasks-summary"

Push-Location $projectRoot
try {
    & $pythonExe -m pulse_console pending-tasks-summary *>> $logFile
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Add-Content -Path $logFile -Value "[$timestamp] END pending-tasks-summary"
}
catch {
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Add-Content -Path $logFile -Value "[$timestamp] ERROR pending-tasks-summary $($_.Exception.Message)"
    throw
}
finally {
    Pop-Location
}
