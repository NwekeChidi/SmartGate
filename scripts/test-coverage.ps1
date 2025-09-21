param(
  [string]$ResultsDir = "TestResults", 
  [string]$CoverageOutDir = "coverage"
)

$ErrorActionPreference = "Stop"

function Write-Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }

try {
  # Install reportgenerator if needed
  if (-not (dotnet tool list -g | Select-String -Quiet 'reportgenerator')) {
    Write-Step "Installing reportgenerator..."
    dotnet tool install -g dotnet-reportgenerator-globaltool | Out-Null
  }

  # Clean and create directories
  Remove-Item $ResultsDir, $CoverageOutDir -Recurse -Force -ErrorAction SilentlyContinue
  New-Item -ItemType Directory -Force -Path $ResultsDir, $CoverageOutDir | Out-Null

  Write-Step "Running tests with coverage..."
  dotnet test --collect:"XPlat Code Coverage" --results-directory "$ResultsDir"

  Write-Step "Generating coverage report..."
  $reports = Get-ChildItem -Path $ResultsDir -Recurse -Filter "coverage.cobertura.xml" -File
  if (-not $reports) { throw "No coverage files found" }

  & reportgenerator -reports:($reports.FullName -join ";") -targetdir:"$CoverageOutDir" -reporttypes:"HtmlInline;TextSummary" -assemblyfilters:"-SmartGate.Infrastructure;-SmartGate.Domain.Tests" -filefilters:"-**/obj/**;-**/bin/**;-**/*Generator*.g.cs;-**/Program.cs"

  $summary = Join-Path $CoverageOutDir "Summary.txt"
  if (Test-Path $summary) { Get-Content $summary }

  Write-Step "Report: $(Join-Path $CoverageOutDir 'index.html')"
}
catch {
  Write-Error $_.Exception.Message
  exit 1
}
