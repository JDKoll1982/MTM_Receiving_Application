#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build script for MTM Receiving Application Tests

.DESCRIPTION
    Cleans, restores, and builds the test project.
    Automatically builds the main application first to ensure dependencies are available.

.PARAMETER Configuration
    Build configuration (Debug or Release). Default is Debug.

.PARAMETER Clean
    If specified, performs a clean before building.

.PARAMETER SkipMainBuild
    If specified, skips building the main application (use only if already built).

.EXAMPLE
    .\build.ps1
    Builds the main app and test project in Debug configuration

.EXAMPLE
    .\build.ps1 -Configuration Release -Clean
    Cleans and builds both projects in Release configuration

.EXAMPLE
    .\build.ps1 -SkipMainBuild
    Builds only the test project (assumes main app is already built)
#>

param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [Parameter()]
    [switch]$Clean,

    [Parameter()]
    [switch]$SkipMainBuild
)

$ErrorActionPreference = 'Stop'
$testProjectFile = 'MTM_Receiving_Application.Tests.csproj'
$mainProjectFile = '..\MTM_Receiving_Application.csproj'

Write-Host "=== MTM Receiving Application Tests Build Script ===" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host ""

# Build main application first (unless skipped)
if (-not $SkipMainBuild) {
    Write-Host "Building main application (required dependency)..." -ForegroundColor Green

    if ($Clean) {
        Write-Host "Cleaning main application..." -ForegroundColor Yellow
        dotnet clean $mainProjectFile -c $Configuration -p:Platform=x64
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Main project clean failed with exit code $LASTEXITCODE"
            exit $LASTEXITCODE
        }
    }

    Write-Host "Restoring main application packages..." -ForegroundColor Yellow
    dotnet restore $mainProjectFile
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Main project restore failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }

    Write-Host "Building main application..." -ForegroundColor Yellow
    dotnet build $mainProjectFile -c $Configuration -p:Platform=x64 --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Main project build failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }

    Write-Host "Main application built successfully." -ForegroundColor Green
    Write-Host ""
}

# Clean test project if requested
if ($Clean) {
    Write-Host "Cleaning test project..." -ForegroundColor Green
    dotnet clean $testProjectFile -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Test project clean failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }
    Write-Host "Clean completed successfully." -ForegroundColor Green
    Write-Host ""
}

# Restore test project packages
Write-Host "Restoring test project packages..." -ForegroundColor Green
dotnet restore $testProjectFile
if ($LASTEXITCODE -ne 0) {
    Write-Error "Test project restore failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}
Write-Host "Restore completed successfully." -ForegroundColor Green
Write-Host ""

# Build test project
Write-Host "Building test project..." -ForegroundColor Green
dotnet build $testProjectFile -c $Configuration --no-restore --no-dependencies
if ($LASTEXITCODE -ne 0) {
    Write-Error "Test project build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "=== Build Completed Successfully ===" -ForegroundColor Green
Write-Host "Output: bin\$Configuration\net8.0-windows10.0.22621.0\" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run tests, execute: dotnet test" -ForegroundColor Yellow
