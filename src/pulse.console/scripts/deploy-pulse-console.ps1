param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot),
    [switch]$RegisterTasks
)

$ErrorActionPreference = "Stop"

$projectRootPath = (Resolve-Path $ProjectRoot).Path
$pythonLauncher = Get-Command py -ErrorAction SilentlyContinue
$pythonCommand = if ($pythonLauncher) { "py -3.12" } else { "python" }
$venvPath = Join-Path $projectRootPath ".venv"
$pythonExe = Join-Path $venvPath "Scripts\python.exe"
$envExample = Join-Path $projectRootPath ".env.example"
$envFile = Join-Path $projectRootPath ".env"
$logsDir = Join-Path $projectRootPath "logs"
$stateDir = Join-Path $projectRootPath ".state"
$registerScript = Join-Path $PSScriptRoot "register-scheduled-tasks.ps1"

Write-Host "Deploying pulse.console from $projectRootPath"

if (-not (Test-Path $venvPath)) {
    Write-Host "Creating virtual environment..."
    Push-Location $projectRootPath
    try {
        Invoke-Expression "$pythonCommand -m venv .venv"
    }
    finally {
        Pop-Location
    }
}

if (-not (Test-Path $pythonExe)) {
    throw "Python executable not found after venv creation: $pythonExe"
}

Write-Host "Installing pulse.console package..."
Push-Location $projectRootPath
try {
    & $pythonExe -m pip install --upgrade pip
    & $pythonExe -m pip install .
}
finally {
    Pop-Location
}

New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
New-Item -ItemType Directory -Path $stateDir -Force | Out-Null

if (-not (Test-Path $envFile) -and (Test-Path $envExample)) {
    Copy-Item $envExample $envFile
    Write-Host "Created .env from .env.example. Update it before running live jobs."
}

if ($RegisterTasks) {
    if (-not (Test-Path $registerScript)) {
        throw "Task registration script not found: $registerScript"
    }

    & $registerScript -ProjectRoot $projectRootPath
}

Write-Host "Deployment complete."
