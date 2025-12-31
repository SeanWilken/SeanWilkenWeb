$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "=== Starting development stack (Mongo + ES + Server + Client) ==="

# Resolve paths
$Root    = Split-Path -Parent $MyInvocation.MyCommand.Path
$Infra   = Join-Path $Root "infrastructure"
$EnvFile = Join-Path $Infra ".env"

# 1) Start Mongo + Elasticsearch in Podman
$DevCompose = Join-Path $Infra "docker-compose.dev.yml"
if (-not (Test-Path $DevCompose)) {
    Write-Host "ERROR: Could not find $DevCompose" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Starting Mongo + Elasticsearch using Podman Compose..."
Push-Location $Infra
podman compose -f "docker-compose.dev.yml" up -d
Pop-Location

Write-Host ""
Write-Host "Mongo and Elasticsearch should now be running."
Write-Host "  Mongo:         mongodb://localhost:27017"
Write-Host "  Elasticsearch: http://localhost:9200"

# 2) Load environment variables from infrastructure/.env
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

        # Optional: skip ASPNETCORE_URLS here and control it via --urls
        if ($key -eq "ASPNETCORE_URLS") {
            return
        }

        if ($key) {
            Set-Item -Path "Env:$key" -Value $value
        }
    }
} else {
    Write-Host ""
    Write-Host "WARNING: No .env file found at $EnvFile"
}

# 3) Start the server (dotnet watch) on port 5000 in a separate window
$ServerDir = Join-Path $Root "src/Server"
if (-not (Test-Path $ServerDir)) {
    Write-Host ""
    Write-Host "ERROR: Could not find server directory at $ServerDir" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Starting dotnet watch for the server on http://localhost:5000 ..."

Start-Process `
    -FilePath "dotnet" `
    -WorkingDirectory $ServerDir `
    -ArgumentList @(
        "watch"
        "run"
        "--urls"
        "http://localhost:5000"
    )

# 4) Start the client dev server (Vite/Fable) in this window
$ClientDir = Join-Path $Root "src/Client"
if (-not (Test-Path $ClientDir)) {
    Write-Host ""
    Write-Host "ERROR: Could not find client directory at $ClientDir" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Starting client dev server (pnpm dev) ..."
Write-Host "Press Ctrl+C in this window to stop the client."
Write-Host "Stop the server window separately when you are done."
Write-Host ""

Push-Location $ClientDir
pnpm install
dotnet fable watch -o output -s --run "pnpm exec vite"
Pop-Location

Push-Location $Root
Pop-Location
