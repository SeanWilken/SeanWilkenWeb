$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "=== Starting development stack (Mongo + Server + Client) ==="

# Resolve paths
$Root    = Split-Path -Parent $MyInvocation.MyCommand.Path
$Infra   = Join-Path $Root "infrastructure/docker/App/Development"
$EnvFile = Join-Path $Infra "development.env"
$DevCompose = Join-Path $Infra "docker-compose.dev.yml"

if (-not (Test-Path $DevCompose)) {
    Write-Host "ERROR: Could not find $DevCompose" -ForegroundColor Red
    exit 1
}

# 1) Start Mongo (and anything else in dev compose) in Podman
Write-Host ""
Write-Host "Starting Dev containers using Podman Compose..." -ForegroundColor Cyan
podman compose -f $DevCompose --env-file $EnvFile up -d

Write-Host ""
Write-Host "Dev containers should now be running."

# 2) Load environment variables from development.env
if (Test-Path $EnvFile) {
    Write-Host ""
    Write-Host "Loading environment variables from $EnvFile ..."

    Get-Content $EnvFile | ForEach-Object {
        if ($_ -match '^\s*#') { return }   # comment
        if ($_ -match '^\s*$') { return }   # empty line
        if ($_ -notmatch '=') { return }

        $parts = $_ -split '=', 2
        $key   = $parts[0].Trim()
        $value = $parts[1].Trim()

        # Control URLs via --urls
        if ($key -eq "ASPNETCORE_URLS") { return }

        if ($key) { Set-Item -Path "Env:$key" -Value $value }
    }
}
else {
    Write-Host ""
    Write-Host "WARNING: No env file found at $EnvFile" -ForegroundColor Yellow
}

# 3) Start the server (dotnet watch) - track process so we can kill it on exit
$ServerDir = Join-Path $Root "src/Server"
if (-not (Test-Path $ServerDir)) {
    Write-Host ""
    Write-Host "ERROR: Could not find server directory at $ServerDir" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Starting dotnet watch for the server on http://localhost:5000 ..." -ForegroundColor Cyan

$serverProc = Start-Process `
    -FilePath "dotnet" `
    -WorkingDirectory $ServerDir `
    -ArgumentList @("watch", "run", "--urls", "http://localhost:5000") `
    -PassThru

# 4) Start the client dev server (Vite/Fable) in this window
$ClientDir = Join-Path $Root "src/Client"
if (-not (Test-Path $ClientDir)) {
    Write-Host ""
    Write-Host "ERROR: Could not find client directory at $ClientDir" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Starting client dev server (Fable + Vite) ..." -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop client + server + dev containers." -ForegroundColor Gray
Write-Host ""

Push-Location $ClientDir
try {
    pnpm install
    dotnet fable watch -o output -s --run "pnpm exec vite"
}
finally {
    Pop-Location

    Write-Host ""
    Write-Host "===> Shutting down dev stack..." -ForegroundColor Yellow

    # Stop the server we launched
    if ($serverProc -and -not $serverProc.HasExited) {
        try {
            Stop-Process -Id $serverProc.Id -Force
        } catch {
            Write-Host "WARNING: Failed to stop server process: $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }

    # Stop dev containers
    try {
        podman compose -f $DevCompose --env-file $EnvFile down
    } catch {
        Write-Host "WARNING: Failed to bring dev containers down: $($_.Exception.Message)" -ForegroundColor Yellow
    }

    # Ensure we end at repo root
    Set-Location $Root
}
