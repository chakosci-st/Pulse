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
$logFile = Join-Path $logDir "notifications-table.log"

if (-not $pythonExe) {
    throw "Python executable not found. Checked: $($pythonCandidates -join '; ')"
}

New-Item -ItemType Directory -Path $logDir -Force | Out-Null

$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
Add-Content -Path $logFile -Value "[$timestamp] START notifications-table"

Push-Location $projectRoot
try {
    & $pythonExe -m pulse_console notifications-table *>> $logFile
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Add-Content -Path $logFile -Value "[$timestamp] END notifications-table"
}
catch {
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Add-Content -Path $logFile -Value "[$timestamp] ERROR notifications-table $($_.Exception.Message)"
    throw
}
finally {
    Pop-Location
}
