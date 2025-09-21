<#
  generate-token.ps1
  Usage:
    pwsh ./generate-token.ps1 -Permission read -Principal "john.doe@company.com"
    pwsh ./generate-token.ps1 -Permission write -Principal "admin@company.com"
    pwsh ./generate-token.ps1 -Permission admin -Principal "super.admin@company.com"
#>

param(
  [Parameter(Mandatory=$true)]
  [ValidateSet("read", "write", "admin")]
  [string]$Permission,
  
  [Parameter(Mandatory=$true)]
  [string]$Principal,
  
  [string]$SigningKey = "your-256-bit-secret-key-here-must-be-at-least-32-chars",
  [string]$Issuer = "SmartGate",
  [string]$Audience = "SmartGate.Api",
  [int]$ExpiryHours = 24
)

$ErrorActionPreference = "Stop"

function Write-Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }

function New-JwtToken {
  param(
    [string]$Subject,
    [string]$Scope,
    [string]$SigningKey,
    [string]$Issuer,
    [string]$Audience,
    [DateTime]$Expiry
  )
  
  # JWT Header
  $header = @{
    alg = "HS256"
    typ = "JWT"
  } | ConvertTo-Json -Compress
  
  # JWT Payload
  $payload = @{
    sub = $Subject
    scope = $Scope
    iss = $Issuer
    aud = $Audience
    iat = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    exp = [DateTimeOffset]::new($Expiry.ToUniversalTime()).ToUnixTimeSeconds()
  } | ConvertTo-Json -Compress
  
  # Base64Url encode
  $headerEncoded = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($header)).TrimEnd('=').Replace('+', '-').Replace('/', '_')
  $payloadEncoded = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($payload)).TrimEnd('=').Replace('+', '-').Replace('/', '_')
  
  # Create signature
  $message = "$headerEncoded.$payloadEncoded"
  $keyBytes = [Text.Encoding]::UTF8.GetBytes($SigningKey)
  $hmac = New-Object System.Security.Cryptography.HMACSHA256
  $hmac.Key = $keyBytes
  $signatureBytes = $hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($message))
  $signatureEncoded = [Convert]::ToBase64String($signatureBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
  $hmac.Dispose()
  
  return "$message.$signatureEncoded"
}

try {
  Write-Step "Generating JWT token for '$Permission' permission..."
  
  $scopes = switch ($Permission) {
    "read"  { "visits:read" }
    "write" { "visits:read visits:write" }
    "admin" { "visits:read visits:write admin:manage" }
  }
  
  $expiry = (Get-Date).AddHours($ExpiryHours)
  $token = New-JwtToken -Subject $Principal -Scope $scopes -SigningKey $SigningKey -Issuer $Issuer -Audience $Audience -Expiry $expiry
  
  Write-Step "Token details:"
  Write-Host "  Principal: $Principal" -ForegroundColor Yellow
  Write-Host "  Scopes: $scopes" -ForegroundColor Yellow
  Write-Host "  Expires: $expiry" -ForegroundColor Yellow
  
  Write-Step "JWT Bearer Token:"
  Write-Host $token -ForegroundColor Green
  
  Write-Step "Use this Authorization header:"
  Write-Host "Authorization: Bearer $token" -ForegroundColor Cyan
  
  Write-Step "Configuration required:"
  Write-Host "Set Auth:UseDevAuth=false in appsettings.json" -ForegroundColor Yellow
  Write-Host "Set Jwt:SigningKey=$SigningKey in appsettings.json" -ForegroundColor Yellow
  Write-Host "Set Jwt:Authority=$Issuer in appsettings.json" -ForegroundColor Yellow
  Write-Host "Set Jwt:Audience=$Audience in appsettings.json" -ForegroundColor Yellow
  
  Write-Step "Example curl command:"
  Write-Host "curl -H `"Authorization: Bearer $token`" http://localhost:5000/v1/visits" -ForegroundColor Magenta
}
catch {
  Write-Error $_.Exception.Message
  exit 1
}