param(
  [ValidateSet("dev","prod")]
  [string]$Env = "dev",
  [int]$Limit = 0
)

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$EnvFile = switch ($Env) {
  "dev"  { Join-Path $Root "infrastructure/docker/App/Development/.env" }
  "prod" { Join-Path $Root "infrastructure/docker/App/Production/.env" }
}

$Args = @("run","--project","src/Server","--","sync-runner","--env-file",$EnvFile)
if ($Limit -gt 0) { $Args += @("--limit", "$Limit") }

dotnet @Args