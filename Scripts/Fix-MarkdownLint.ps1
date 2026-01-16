# Fix-MarkdownLint.ps1
# Fixes all markdownlint violations in the repository

[CmdletBinding()]
param(
    [Parameter()]
    [string]$Path = (Get-Location).Path,
    
    [Parameter()]
    [switch]$WhatIf
)

$ErrorActionPreference = "Stop"

Write-Host "Fixing markdownlint violations in: $Path" -ForegroundColor Cyan

# Check if markdownlint-cli is available
$markdownlintInstalled = $null -ne (Get-Command npx -ErrorAction SilentlyContinue)

if (-not $markdownlintInstalled) {
    Write-Error "npx not found. Please install Node.js and npm first."
    exit 1
}

# Run markdownlint fix
$args = @(
    "markdownlint-cli",
    "--fix",
    "**/*.md"
)

if ($WhatIf) {
    Write-Host "Would run: npx $($args -join ' ')" -ForegroundColor Yellow
} else {
    Write-Host "Running: npx $($args -join ' ')" -ForegroundColor Green
    & npx @args
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nMarkdown files fixed successfully!" -ForegroundColor Green
    } else {
        Write-Warning "Some issues could not be auto-fixed. Please review manually."
    }
}
