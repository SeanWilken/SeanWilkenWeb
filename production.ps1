$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "=== Starting production deployment (Server + Client) ==="

# 1) Derive image tag from Git SHA
$Tag = git rev-parse --short HEAD
if (-not $Tag) {
    Write-Host "ERROR: Could not determine Git SHA (is this a Git repo?)." -ForegroundColor Red
    exit 1
}
Write-Host "Using image tag: $Tag" -ForegroundColor Cyan

# 2) Resolve paths
$Root       = Split-Path -Parent $MyInvocation.MyCommand.Path
$Infra      = Join-Path $Root "infrastructure/docker/App"
$EnvFile    = Join-Path $Infra "Production/.env"
$Dockerfile = Join-Path $Infra "dockerfile.app"

if (-not (Test-Path $Dockerfile)) {
    Write-Host "ERROR: Could not find $Dockerfile" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $EnvFile)) {
    Write-Host "ERROR: Could not find $EnvFile" -ForegroundColor Red
    exit 1
}

# 3) Load build-time env vars from .env
Write-Host ""
Write-Host "Loading environment variables from $EnvFile ..." -ForegroundColor Cyan

$envVars = @{}
Get-Content $EnvFile | ForEach-Object {
    if ($_ -match '^\s*#') { return }
    if ($_ -match '^\s*$') { return }
    if ($_ -notmatch '=') { return }

    $parts = $_ -split '=', 2
    $key   = $parts[0].Trim()
    $value = $parts[1].Trim()

    $envVars[$key] = $value
}

$ViteStripePk     = $envVars["VITE_STRIPE_API_PK"]
$ViteApiBaseUrl   = $envVars["VITE_API_BASE_URL"]
$ServerProxyPort  = $envVars["SERVER_PROXY_PORT"]

if (-not $ViteStripePk -or -not $ViteApiBaseUrl -or -not $ServerProxyPort) {
    Write-Host "ERROR: Missing required build-time variables in .env" -ForegroundColor Red
    exit 1
}

# 4) Build image
Write-Host ""
Write-Host "Building production image with Podman..." -ForegroundColor Cyan

podman build `
    -f $Dockerfile `
    --build-arg VITE_STRIPE_API_PK=$ViteStripePk `
    --build-arg VITE_API_BASE_URL=$ViteApiBaseUrl `
    --build-arg SERVER_PROXY_PORT=$ServerProxyPort `
    -t registry.digitalocean.com/wilkenweb/app:$Tag `
    $Root

Write-Host ""
Write-Host "Production image built successfully."

# 5) Push image
Write-Host ""
Write-Host "Pushing image to DOCR..." -ForegroundColor Cyan

podman push registry.digitalocean.com/wilkenweb/app:$Tag

Write-Host ""
Write-Host "Image pushed successfully."

# 6) Terraform apply with tag
Write-Host ""
Write-Host "Running Terraform apply for Tag: $Tag." -ForegroundColor Cyan
terraform apply -var="tag=$Tag" -auto-approve
Write-Host "Applied Terraform for Tag: $Tag."

# 7) Apply K8s overlays (Kustomize)
Write-Host ""
Write-Host "Applying K8S overlays for Tag: $Tag." -ForegroundColor Cyan
kubectl apply -k infrastructure/k8s/overlays/prod
Write-Host "Applied K8S overlays for Tag: $Tag."

# 8) Wait for rollout
Write-Host ""
Write-Host "Waiting for K8S Production rollout..." -ForegroundColor Cyan
kubectl rollout status deployment/app -n store-prod
Write-Host "K8S Production rollout complete."

# 9) Health check
Write-Host ""
Write-Host "Performing health check on https://seanwilken.com/api/health ..." -ForegroundColor Cyan
$Health = Invoke-WebRequest "https://seanwilken.com/api/health" -UseBasicParsing
Write-Host "Health check response: $($Health.StatusCode)" -ForegroundColor Green