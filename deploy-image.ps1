param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("staging", "prod")]
    [string]$Environment,

    [string]$Registry = "registry.digitalocean.com/YOUR_NAMESPACE/YOUR_IMAGE",

    [string]$Tag = "",

    [switch]$AlsoTagLatest,

    # NEW: path to Dockerfile relative to repo root
    [string]$DockerfilePath = "infrastructure/docker/App/Dockerfile.app"
)

# ... compute $Tag as before ...

$image = "$Registry:$Tag"

Write-Host "===> Building image $image using $DockerfilePath" -ForegroundColor Cyan
docker build `
    -f $DockerfilePath `
    --build-arg APP_ENV=$Environment `
    -t $image .

# push + latest tagging as before
