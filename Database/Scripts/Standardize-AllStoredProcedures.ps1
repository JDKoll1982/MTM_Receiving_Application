# ============================================================================
# Script: Standardize-AllStoredProcedures.ps1
# Purpose: Master script to standardize SP naming and organization
# Steps:
#   1. Reorganize SPs to correct module folders (by table usage)
#   2. Rename SPs to follow naming convention
#   3. Update all references in codebase
# ============================================================================

param(
    [switch]$WhatIf = $false,
    [switch]$SkipReorganize = $false,
    [switch]$SkipRename = $false
)

$scriptDir = $PSScriptRoot
$repoRoot = "c:\Users\johnk\source\repos\MTM_Receiving_Application"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "   Stored Procedure Standardization" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

if ($WhatIf) {
    Write-Host "[PREVIEW MODE] No changes will be made" -ForegroundColor Yellow
    Write-Host ""
}

# Step 1: Reorganize to correct folders
if (-not $SkipReorganize) {
    Write-Host "[STEP 1] Reorganizing SPs by table usage..." -ForegroundColor Green
    Write-Host ""

    $reorganizeScript = Join-Path $scriptDir "Reorganize-StoredProcedures.ps1"
    if (Test-Path $reorganizeScript) {
        if ($WhatIf) {
            & $reorganizeScript -WhatIf
        } else {
            & $reorganizeScript
        }
    } else {
        Write-Host "[!] Reorganize script not found: $reorganizeScript" -ForegroundColor Red
        exit 1
    }

    Write-Host ""
    Write-Host "Press any key to continue to Step 2..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    Write-Host ""
}

# Step 2: Rename to standard convention
if (-not $SkipRename) {
    Write-Host "[STEP 2] Renaming SPs to standard convention..." -ForegroundColor Green
    Write-Host ""

    $renameScript = Join-Path $scriptDir "Rename-StoredProcedures.ps1"
    if (Test-Path $renameScript) {
        if ($WhatIf) {
            & $renameScript -WhatIf
        } else {
            & $renameScript
        }
    } else {
        Write-Host "[!] Rename script not found: $renameScript" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "   Standardization Complete!" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

if (-not $WhatIf) {
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Review the log files in Database\Scripts\" -ForegroundColor Gray
    Write-Host "2. Test the database deployment" -ForegroundColor Gray
    Write-Host "3. Build and test the application" -ForegroundColor Gray
    Write-Host "4. Commit changes to git" -ForegroundColor Gray
}
