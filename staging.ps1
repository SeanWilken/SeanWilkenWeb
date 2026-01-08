$Root    = Split-Path -Parent $MyInvocation.MyCommand.Path
$Infra   = Join-Path $Root "infrastructure/docker/App/Staging"
$EnvFile = Join-Path $Infra "staging.env"

Write-Host "===> Building and running staging environment with Podman Compose" -ForegroundColor Cyan

Push-Location $Infra

try {
    podman compose `
        -f docker-compose.staging.yml `
        --env-file $EnvFile `
        up --build
    }
    finally {
        
    podman compose `
        -f docker-compose.staging.yml `
        --env-file $EnvFile `
        down

    Pop-Location
}
