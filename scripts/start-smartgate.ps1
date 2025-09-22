<#
.SYNOPSIS
    Starts SmartGate API with environment-specific configuration
.PARAMETER Env
    Environment: dev, uat, or production
#>

param(
  [ValidateSet("dev", "uat", "production")]
  [string]$Env = "dev"
)

$ErrorActionPreference = "Stop"

# Configuration
$config = @{
  ComposeFile = "docker-compose.yml"
  InfraProj = "src/SmartGate.Infrastructure/SmartGate.Infrastructure.csproj"
  ApiProj = "src/SmartGate.Api/SmartGate.Api.csproj"
  DbService = "smartgate-db"
  HealthTimeoutSec = 90
}

$envConfig = @{
  dev = @{
    Database = "smartgate"
    ConnString = "Host=localhost;Port=15432;Database=smartgate;Username=postgres;Password=postgres"
  }
  uat = @{
    Database = "smartgate_uat"
    ConnString = "Host=localhost;Port=15432;Database=smartgate_uat;Username=postgres;Password=postgres"
    Authority = "SmartGate"
    SigningKey = "SmartGate-Solutions-256-bit-secret-key-here-must-be-at-least-32-chars"
  }
  production = @{
    Database = "smartgate_prod"
    ConnString = "Host=localhost;Port=15432;Database=smartgate_prod;Username=postgres;Password=postgres"
    Authority = "SmartGate"
    SigningKey = "SmartGate-Solutions-256-bit-secret-key-here-must-be-at-least-32-chars"
  }
}

function Write-Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }
function Test-Command($cmd) { Get-Command $cmd -ErrorAction SilentlyContinue }
function Wait-DatabaseHealth {
  $start = Get-Date
  while ((Get-Date) - $start -lt [TimeSpan]::FromSeconds($config.HealthTimeoutSec)) {
    $status = (docker inspect --format='{{json .State.Health.Status}}' $config.DbService 2>$null) -replace '"',''
    if ($status -eq 'healthy') { return $true }
    Start-Sleep -Seconds 2
  }
  return $false
}

try {
  Write-Step "Checking prerequisites..."
  if (-not (Test-Command docker)) { throw "Docker not found" }
  if (-not (Test-Command dotnet)) { throw ".NET SDK not found" }
  
  if (-not (dotnet tool list -g | Select-String -Quiet 'dotnet-ef')) {
    Write-Step "Installing dotnet-ef..."
    dotnet tool install --global dotnet-ef | Out-Null
  }

  $currentEnv = $envConfig[$Env]
  $isDev = $Env -eq "dev"
  
  Write-Step "Configuring $Env environment..."
  $env:ASPNETCORE_ENVIRONMENT = if ($Env -eq "uat") { "Uat" } else { $Env }

  Write-Host "Set ASPNETCORE_ENVIRONMENT to: $($env:ASPNETCORE_ENVIRONMENT)" -ForegroundColor Yellow
  $env:SMARTGATE_PG = $currentEnv.ConnString
  $env:ConnectionStrings__Postgres = $currentEnv.ConnString
  
  if (-not $isDev) {
    $env:Jwt__Authority = $currentEnv.Authority
    $env:Jwt__SigningKey = $currentEnv.SigningKey
  }

  Write-Step "Running tests..."
  ./scripts/test-coverage.ps1

  Write-Step "Starting database..."
  docker compose -f $config.ComposeFile up -d

  Write-Step "Waiting for database..."
  if (-not (Wait-DatabaseHealth)) {
    Write-Warning "Database health check failed, continuing..."
  }

  Write-Step "Applying migrations..."
  dotnet ef database update --project $config.InfraProj

  Write-Step "Starting API in $Env mode..."
  if ($isDev) {
    dotnet watch --project $config.ApiProj run --no-launch-profile
  } else {
    dotnet run --project $config.ApiProj --no-launch-profile
  }
}
catch {
  Write-Error $_.Exception.Message
  exit 1
}
