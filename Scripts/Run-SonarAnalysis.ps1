param(
    [Parameter(Mandatory = $false)]
    [string]$Token = "f70ebe79196eabccba3ae751c9374a182d01ec71",
    
    [string]$ProjectKey = "JDKoll1982_MTM_Receiving_Application",
    [string]$Organization = "john-koll",
    [string]$HostUrl = "https://sonarcloud.io"
)

$ErrorActionPreference = "Stop"

# 1. Check for dotnet-sonarscanner
if (-not (Get-Command "dotnet-sonarscanner" -ErrorAction SilentlyContinue)) {
    Write-Host "Installing dotnet-sonarscanner tool globally..." -ForegroundColor Cyan
    dotnet tool install --global dotnet-sonarscanner
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to install dotnet-sonarscanner."
    }
}

# 2. Get current branch name
$branchName = git rev-parse --abbrev-ref HEAD
Write-Host "Current Branch: $branchName" -ForegroundColor Cyan

# 3. Begin Analysis
Write-Host "Starting SonarQube Analysis..." -ForegroundColor Cyan
Write-Host "Project: $ProjectKey" -ForegroundColor Gray
Write-Host "Org: $Organization" -ForegroundColor Gray
Write-Host "Branch: $branchName" -ForegroundColor Gray

dotnet sonarscanner begin `
    /k:"$ProjectKey" `
    /o:"$Organization" `
    /d:sonar.host.url="$HostUrl" `
    /d:sonar.token="$Token" `
    /d:sonar.branch.name="$branchName" `
    /d:sonar.cs.vscoveragexml.reportsPaths=coverage.xml

if ($LASTEXITCODE -ne 0) {
    Write-Error "SonarScanner Begin failed. Please check the error message above."
}

# 4. Build
Write-Host "Building Solution..." -ForegroundColor Cyan
dotnet build /p:Platform=x64 /p:Configuration=Debug

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed."
}

# 5. End Analysis
Write-Host "Finishing Analysis..." -ForegroundColor Cyan
dotnet sonarscanner end /d:sonar.token="$Token"

if ($LASTEXITCODE -ne 0) {
    Write-Error "SonarScanner End failed."
}

Write-Host "Analysis Complete!" -ForegroundColor Green
