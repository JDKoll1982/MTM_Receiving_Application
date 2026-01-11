# ============================================================================
# Script: Preview-AllChanges.ps1
# Purpose: Preview all SP reorganization and renaming without making changes
# ============================================================================

$scriptDir = $PSScriptRoot

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "   PREVIEW MODE" -ForegroundColor Cyan
Write-Host "   (No changes will be made)" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Run full standardization in WhatIf mode
$masterScript = Join-Path $scriptDir "Standardize-AllStoredProcedures.ps1"

if (Test-Path $masterScript) {
    & $masterScript -WhatIf
} else {
    Write-Host "[!] Master script not found" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "To execute these changes, run:" -ForegroundColor Yellow
Write-Host "  .\Standardize-AllStoredProcedures.ps1" -ForegroundColor White
Write-Host ""
