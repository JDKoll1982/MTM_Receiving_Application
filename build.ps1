#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build script for MTM Receiving Application

.DESCRIPTION
    Cleans, restores, and builds the MTM Receiving Application WinUI 3 project.
    Must be run with a specific platform (x64, x86, or ARM64).

.PARAMETER Configuration
    Build configuration (Debug or Release). Default is Debug.

.PARAMETER Platform
    Target platform (x64, x86, ARM64). Default is x64.

.PARAMETER Clean
    If specified, performs a clean before building.

.EXAMPLE
    .\build.ps1
    Builds the project in Debug configuration for x64 platform

.EXAMPLE
    .\build.ps1 -Configuration Release -Platform x64 -Clean
    Cleans, restores, and builds in Release configuration for x64
#>

param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [Parameter()]
    [ValidateSet('x64', 'x86', 'ARM64')]
    [string]$Platform = 'x64',

    [Parameter()]
    [switch]$Clean
)

$ErrorActionPreference = 'Stop'
$projectFile = 'MTM_Receiving_Application.csproj'

Write-Host "=== MTM Receiving Application Build Script ===" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Platform: $Platform" -ForegroundColor Yellow
Write-Host ""

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning project..." -ForegroundColor Green
    dotnet clean $projectFile -c $Configuration -p:Platform=$Platform
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Clean failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }
    Write-Host "Clean completed successfully." -ForegroundColor Green
    Write-Host ""
}

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Green
dotnet restore $projectFile
if ($LASTEXITCODE -ne 0) {
    Write-Error "Restore failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}
Write-Host "Restore completed successfully." -ForegroundColor Green
Write-Host ""

# Build project
Write-Host "Building project..." -ForegroundColor Green
dotnet build $projectFile -c $Configuration -p:Platform=$Platform --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "=== Build Completed Successfully ===" -ForegroundColor Green
Write-Host "Output: bin\$Platform\$Configuration\net8.0-windows10.0.22621.0\" -ForegroundColor Cyan
