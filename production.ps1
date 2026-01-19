param(
    [string]$Tag = $(git rev-parse --short HEAD),
    [switch]$SkipBuild,
    [switch]$SkipPush,
    [switch]$SkipTerraform
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "=== Starting production deployment (Server + Client) ==="
Write-Host "Flags: SkipBuild=$SkipBuild SkipPush=$SkipPush SkipTerraform=$SkipTerraform" -ForegroundColor DarkCyan

# 1) Derive image tag from Git SHA
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
# TerraForm and K8s paths
$K8S            = Join-Path $Root "infrastructure/k8s"
$Terraform      = Join-Path $Root "infrastructure/terraform"

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
$KubeConfig       = Join-Path $Root $envVars["KUBECONFIG"]

if (-not $ViteStripePk -or -not $ViteApiBaseUrl -or -not $ServerProxyPort) {
    Write-Host "ERROR: Missing required build-time variables in .env" -ForegroundColor Red
    exit 1
}


# 4) Build image
if (-not $SkipBuild) {
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
}
else {
    Write-Host "Skipping image build (flag: -SkipBuild)." -ForegroundColor Yellow
}

# 5) Push image
if (-not $SkipPush) {
    Write-Host ""
    Write-Host "Pushing image to DOCR..." -ForegroundColor Cyan

    podman push registry.digitalocean.com/wilkenweb/app:$Tag

    Write-Host ""
    Write-Host "Image pushed successfully."
}
else {
    Write-Host "Skipping image push (flag: -SkipPush)." -ForegroundColor Yellow
}

$env:KUBECONFIG = (Resolve-Path $KubeConfig)
Write-Host "Using kubeconfig: $env:KUBECONFIG" -ForegroundColor Cyan
kubectl get nodes


# 6) Terraform apply with tag
if (-not $SkipTerraform) {
    Write-Host "SWITCHING DIRECTORY TO: $Terraform"
    Set-Location $Terraform
    Write-Host "Running Terraform apply for Tag: $Tag." -ForegroundColor Cyan
    terraform apply -var="tag=$Tag" -var="kubeconfig_path=$env:KUBECONFIG" -auto-approve
    Write-Host "Applied Terraform for Tag: $Tag."
}
else {
    Write-Host "Skipping Terraform apply (flag: -SkipTerraform)." -ForegroundColor Yellow
}

# 7) Apply K8s overlays (Kustomize)
# ????
# Write-Host "CANCELING PENDING MIGRATION JOBS IF ANY..."
# kubectl delete job app-migrations -n wilkenweb-prod --ignore-not-found
# Write-Host "SWITCH TO K8S DIRECTORY: $Root"
# Set-Location $K8S
# Write-Host "Applying K8S overlays for Tag: $Tag." -ForegroundColor Cyan
# kubectl apply -k overlays/production
# Write-Host "Applied K8S overlays for Tag: $Tag."

# 7) Apply K8s overlays (Kustomize)
# Write-Host "CANCELING PENDING MIGRATION JOBS IF ANY..."
# kubectl delete job app-migrations -n wilkenweb-prod --ignore-not-found

# 7) Apply K8s overlays (Kustomize)
Write-Host "CANCELING PENDING MIGRATION JOBS IF ANY..."
kubectl delete job app-migrations -n wilkenweb-prod --ignore-not-found

$OverlayDir = Join-Path $K8S "overlays/production"
Set-Location $OverlayDir

Write-Host "Setting kustomize image tag to: $Tag" -ForegroundColor Cyan
kustomize edit set image `
  registry.digitalocean.com/wilkenweb/app=registry.digitalocean.com/wilkenweb/app:$Tag

Write-Host "Applying K8S overlays for Tag: $Tag." -ForegroundColor Cyan
kubectl apply -k .

kubectl -n wilkenweb-prod get job app-migrations -o jsonpath="{.spec.template.spec.containers[0].image}"
kubectl -n wilkenweb-prod logs -l job-name=app-migrations -f

# Write-Host "SWITCH TO K8S KUSTOMIZE DIRECTORY: $K8S"
# # Set-Location $K8S
# Set-Location (Join-Path $K8S "overlays/production")

# # Write-Host "Setting kustomize image tag to: $Tag" -ForegroundColor Cyan
# # kustomize edit set image registry.digitalocean.com/wilkenweb/app=registry.digitalocean.com/wilkenweb/app:$Tag

# # Write-Host "Applying K8S overlays for Tag: $Tag." -ForegroundColor Cyan
# # kubectl apply -k overlays/production
# # Write-Host "Applied K8S overlays for Tag: $Tag."
# kustomize edit set image registry.digitalocean.com/wilkenweb/app=registry.digitalocean.com/wilkenweb/app:$Tag

# Write-Host "Rendered migration job image:" -ForegroundColor Cyan
# kubectl kustomize . | Select-String -Pattern "kind: Job","name: app-migrations","image:"

# kubectl apply -k .


# 8) Wait for rollout
Write-Host ""
Set-Location $Root
Write-Host "Waiting for K8S Production rollout..." -ForegroundColor Cyan
kubectl rollout status deployment/app -n wilkenweb-prod
Write-Host "K8S Production rollout complete."

# 9) Health check
Write-Host ""
Write-Host "Performing health check on https://seanwilken.com/api/health ..." -ForegroundColor Cyan
$Health = Invoke-WebRequest "https://seanwilken.com/api/health" -UseBasicParsing
Write-Host "Health check response: $($Health.StatusCode)" -ForegroundColor Green