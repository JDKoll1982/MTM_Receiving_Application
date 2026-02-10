# ============================================================================
# Script: Reorganize-StoredProcedures.ps1
# Purpose: Move SPs to correct module folders based on table interactions
# Logic: Analyzes which tables each SP uses and moves to appropriate folder
# ============================================================================

param(
    [switch]$WhatIf = $false,
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"
$repoRoot = "c:\Users\johnk\source\repos\MTM_Receiving_Application"
$spFolder = Join-Path $repoRoot "Database\StoredProcedures"

# Define table prefix to module mappings
$tableToModule = @{
    "auth_"               = "Authentication"
    "workstation_"        = "Authentication"
    "dunnage_"            = "Dunnage"
    "inventoried_dunnage" = "Dunnage"
    "receiving_"          = "Receiving"
    "settings_"           = "Settings"
    "scheduled_reports"   = "Settings"
    "system_settings"     = "Settings"
    "user_settings"       = "Settings"
    "settings_audit_log"  = "Settings"
    "volvo_"              = "Volvo"
}

function Get-TablesFromSP {
    param([string]$FilePath)

    $content = Get-Content $FilePath -Raw
    $tables = @()

    # Patterns to match table references
    $patterns = @(
        'FROM\s+`?(\w+)`?',
        'JOIN\s+`?(\w+)`?',
        'INSERT\s+INTO\s+`?(\w+)`?',
        'UPDATE\s+`?(\w+)`?',
        'DELETE\s+FROM\s+`?(\w+)`?',
        'EXISTS\s*\(\s*SELECT.*?FROM\s+`?(\w+)`?'
    )

    foreach ($pattern in $patterns) {
        $matches = [regex]::Matches($content, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        foreach ($match in $matches) {
            if ($match.Groups.Count -gt 1) {
                $tableName = $match.Groups[1].Value.ToLower()
                # Filter out subqueries and common SQL keywords
                if ($tableName -notmatch '^(select|where|and|or|on|as|by|set|values|into)$') {
                    $tables += $tableName
                }
            }
        }
    }

    return $tables | Select-Object -Unique
}

function Get-ModuleForTables {
    param([string[]]$Tables)

    $moduleCounts = @{}

    foreach ($table in $Tables) {
        $matched = $false
        foreach ($prefix in $tableToModule.Keys) {
            if ($table -like "$prefix*" -or $table -eq $prefix) {
                $module = $tableToModule[$prefix]
                if (-not $moduleCounts.ContainsKey($module)) {
                    $moduleCounts[$module] = 0
                }
                $moduleCounts[$module]++
                $matched = $true
                break
            }
        }

        if (-not $matched -and $Verbose) {
            Write-Host "        [?] Unknown table: $table" -ForegroundColor DarkGray
        }
    }

    # Return the module with most table matches
    if ($moduleCounts.Count -gt 0) {
        $topModule = $moduleCounts.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First 1
        return $topModule.Name
    }

    return $null
}

function Get-CurrentModule {
    param([string]$FilePath)

    $relativePath = $FilePath.Replace($spFolder, "").TrimStart('\')
    $parts = $relativePath.Split('\')
    if ($parts.Count -gt 0) {
        return $parts[0]
    }
    return $null
}

# ============================================================================
# MAIN EXECUTION
# ============================================================================

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Stored Procedure Reorganization Tool" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

if ($WhatIf) {
    Write-Host "[WHATIF MODE] No changes will be made" -ForegroundColor Yellow
    Write-Host ""
}

$moveLog = @()
$allSPs = Get-ChildItem -Path $spFolder -Filter "*.sql" -Recurse -File

Write-Host "[*] Analyzing $($allSPs.Count) stored procedures..." -ForegroundColor Cyan
Write-Host ""

foreach ($spFile in $allSPs) {
    $currentModule = Get-CurrentModule $spFile.FullName

    if (-not $currentModule) {
        Write-Host "[!] Could not determine current module for: $($spFile.Name)" -ForegroundColor Yellow
        continue
    }

    Write-Host "[*] Analyzing: $($spFile.Name)" -ForegroundColor Gray
    Write-Host "    Current module: $currentModule" -ForegroundColor Gray

    # Get tables used by this SP
    $tables = Get-TablesFromSP $spFile.FullName

    if ($tables.Count -eq 0) {
        Write-Host "    [!] No tables found - keeping in $currentModule" -ForegroundColor Yellow
        Write-Host ""
        continue
    }

    if ($Verbose) {
        Write-Host "    Tables used: $($tables -join ', ')" -ForegroundColor DarkGray
    }

    # Determine correct module
    $correctModule = Get-ModuleForTables $tables

    if (-not $correctModule) {
        Write-Host "    [!] Could not determine module - keeping in $currentModule" -ForegroundColor Yellow
        Write-Host ""
        continue
    }

    Write-Host "    Suggested module: $correctModule" -ForegroundColor Gray

    # Check if move is needed
    if ($currentModule -ne $correctModule) {
        Write-Host "    [!] MISMATCH - Should move to $correctModule" -ForegroundColor Yellow

        $targetFolder = Join-Path $spFolder $correctModule
        $targetPath = Join-Path $targetFolder $spFile.Name

        # Check if target already exists
        if (Test-Path $targetPath) {
            Write-Host "    [!] Target file already exists - SKIPPING" -ForegroundColor Red
        }
        else {
            if (-not $WhatIf) {
                # Ensure target folder exists
                if (-not (Test-Path $targetFolder)) {
                    New-Item -ItemType Directory -Path $targetFolder -Force | Out-Null
                }

                # Move file
                Move-Item $spFile.FullName $targetPath -Force
                Write-Host "    [✓] MOVED to $correctModule" -ForegroundColor Green
            }
            else {
                Write-Host "    [WHATIF] Would move to $correctModule" -ForegroundColor Cyan
            }

            # Log the move
            $moveLog += [PSCustomObject]@{
                FileName   = $spFile.Name
                FromModule = $currentModule
                ToModule   = $correctModule
                Tables     = ($tables -join ', ')
            }
        }
    }
    else {
        Write-Host "    [✓] Already in correct module" -ForegroundColor Green
    }

    Write-Host ""
}

# ============================================================================
# SUMMARY
# ============================================================================

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Total SPs analyzed: $($allSPs.Count)" -ForegroundColor Cyan
Write-Host "SPs to move: $($moveLog.Count)" -ForegroundColor Yellow
Write-Host ""

if ($moveLog.Count -gt 0) {
    Write-Host "Moves by module:" -ForegroundColor Cyan
    $moveLog | Group-Object ToModule | ForEach-Object {
        Write-Host "  $($_.Name): $($_.Count) file(s)" -ForegroundColor Gray
    }
    Write-Host ""

    # Export detailed log
    $logPath = Join-Path $repoRoot "Database\Scripts\sp-reorganize-log.csv"
    $moveLog | Export-Csv $logPath -NoTypeInformation
    Write-Host "[+] Detailed log exported to: $logPath" -ForegroundColor Green

    # Show specific moves
    Write-Host ""
    Write-Host "Detailed moves:" -ForegroundColor Cyan
    $moveLog | Format-Table -Property FileName, FromModule, ToModule -AutoSize
}
else {
    Write-Host "[✓] All stored procedures are in the correct modules!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
