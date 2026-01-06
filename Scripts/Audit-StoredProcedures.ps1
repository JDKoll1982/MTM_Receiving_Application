
# Script: Audit-StoredProcedures.ps1
# Description: Comprehensive audit of Stored Procedures including Schema Validation (Column Mismatches)

$ErrorActionPreference = "Continue" 

Write-Host "Initializing Deep Audit..." -ForegroundColor Cyan

$workspaceRoot = Get-Location
$reportFile = Join-Path $workspaceRoot "StoredProcedure_Audit.md"

# Clear previous report
if (Test-Path $reportFile) {
    Remove-Item $reportFile -Force
}

function Get-RelativePath($path) {
    if ([string]::IsNullOrEmpty($path)) { return "" }
    return $path.Replace($workspaceRoot.Path, "").Trim("\")
}

# ==============================================================================
# PHASE 1: Parse Database Schema
# ==============================================================================
Write-Host "Phase 1: Parsing Database Schema..." -ForegroundColor Yellow
$schemaDefinitions = @{}

$schemaFiles = Get-ChildItem -Path (Join-Path $workspaceRoot "Database\Schemas") -Recurse -Filter "*.sql"

foreach ($file in $schemaFiles) {
    $content = Get-Content $file.FullName -Raw
    $tableMatches = [regex]::Matches($content, 'CREATE\s+TABLE\s+[`""]?(\w+)[`""]?\s*\(([\s\S]*?)\)(?=\s*(?:ENGINE|;))')

    foreach ($tm in $tableMatches) {
        $tableName = $tm.Groups[1].Value
        $body = $tm.Groups[2].Value
        $columns = @()
        $lines = $body -split "`n"
        foreach ($line in $lines) {
            $line = $line.Trim()
            if ($line -match '^[`""](\w+)[`""]') {
                $columns += $matches[1]
            }
        }
        if (-not $schemaDefinitions.ContainsKey($tableName)) {
            $schemaDefinitions[$tableName] = $columns
        }
    }
}
Write-Host "   -> Found $($schemaDefinitions.Keys.Count) tables defined in schema." -ForegroundColor Gray

# ==============================================================================
# PHASE 2: Parse Stored Procedures & Check Columns
# ==============================================================================
Write-Host "Phase 2: analyzing Stored Procedures..." -ForegroundColor Yellow
$sqlDefinitions = @{}
$spColumnErrors = @()

$spPath = Join-Path $workspaceRoot "Database\StoredProcedures"
$sqlFiles = Get-ChildItem -Path $spPath -Recurse -Filter "*.sql"

foreach ($file in $sqlFiles) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match 'CREATE\s+PROCEDURE\s+[`""]?(\w+)[`""]?') {
        $spName = $matches[1]
        $params = @()
        $paramRegex = [regex]'(?:IN|OUT|INOUT)\s+(\w+)\s+'
        foreach ($m in $paramRegex.Matches($content)) {
            $params += $m.Groups[1].Value
        }
        $sqlDefinitions[$spName] = @{
            File       = Get-RelativePath $file.FullName
            Parameters = $params
            Module     = $file.Directory.Name
        }

        $selectMatches = [regex]::Matches($content, 'SELECT\s+([\s\S]+?)\s+FROM\s+[`""]?(\w+)[`""]?')
        foreach ($sm in $selectMatches) {
            $colBlock = $sm.Groups[1].Value
            $tableName = $sm.Groups[2].Value

            # Handle SELECT ... INTO variables ... FROM
            if ($colBlock -match 'INTO\s+') {
                $colBlock = $colBlock -split 'INTO\s+' | Select-Object -First 1
            }

            if ($schemaDefinitions.ContainsKey($tableName)) {
                $validCols = $schemaDefinitions[$tableName]
                
                # Robust splitting: Ignore commas inside parentheses
                # e.g., "COALESCE(a,b), c" -> ["COALESCE(a,b)", "c"]
                $colsToCheck = @()
                $parenDepth = 0
                $buffer = ""
                
                for ($i = 0; $i -lt $colBlock.Length; $i++) {
                    $char = $colBlock[$i]
                    if ($char -eq '(') { $parenDepth++ }
                    elseif ($char -eq ')') { $parenDepth-- }
                    
                    if ($char -eq ',' -and $parenDepth -eq 0) {
                        $colsToCheck += $buffer.Trim()
                        $buffer = ""
                    }
                    else {
                        $buffer += $char
                    }
                }
                if ($buffer.Trim().Length -gt 0) { $colsToCheck += $buffer.Trim() }

                foreach ($rawCol in $colsToCheck) {
                    $col = $rawCol.Trim()
                    
                    # Skip if looks like function call or expression
                    if ($col.Contains('(')) { continue }
                    
                    # Handle Aliases: "l.id AS label_id" -> "l.id", "col alias" -> "col"
                    # Regex explanation:
                    #  ^                Start
                    #  ([\w\.]+)        Capture group 1: Word chars or dots (column or table.column)
                    #  \s+              Whitespace
                    #  (?:AS\s+)?       Non-capturing optional group for "AS "
                    #  \w+              Alias name
                    #  $                End
                    if ($col -match '^([\w\.]+)\s+(?:AS\s+)?\w+$') { 
                        $col = $matches[1] 
                    }
                    
                    $col = $col -replace '[`""]', ''
                    
                    # Handle table aliases: "l.id" -> "id"
                    if ($col -match '\.') {
                        $parts = $col.Split('.')
                        $col = $parts[$parts.Length - 1].Trim()
                    }
                    
                    if ($col -eq '*' -or [string]::IsNullOrWhiteSpace($col)) { continue }

                    if ($validCols -notcontains $col) {
                        $spColumnErrors += [PSCustomObject]@{
                            SpName    = $spName
                            Table     = $tableName
                            BadColumn = $col
                            File      = Get-RelativePath $file.FullName
                        }
                    }
                }
            }
        }
    }
}

# ==============================================================================
# PHASE 3: Audit DAO Usage
# ==============================================================================
Write-Host "Phase 3: Scanning Code Usage (DAOs)..." -ForegroundColor Yellow
$daoCalls = @()
$daoFiles = Get-ChildItem -Path $workspaceRoot -Recurse -Include "Dao_*.cs"

foreach ($file in $daoFiles) {
    $content = Get-Content $file.FullName -Raw
    $module = $file.Directory.Parent.Name 
    $matches = [regex]::Matches($content, '"(sp_\w+|carrier_delivery_label_Insert|receiving_line_Insert|dunnage_line_Insert)"')

    foreach ($m in $matches) {
        $spName = $m.Groups[1].Value
        $paramKeys = @()
        $paramMatches = [regex]::Matches($content, '{\s*"(@?\w+)"\s*,|\.AddWithValue\s*\(\s*"(@?\w+)"')
        foreach ($pm in $paramMatches) {
            $p = $pm.Groups[1].Value
            if ([string]::IsNullOrEmpty($p)) { $p = $pm.Groups[2].Value }
            if (-not [string]::IsNullOrEmpty($p)) { $paramKeys += $p }
        }

        $daoCalls += [PSCustomObject]@{
            SpName     = $spName
            DaoFile    = $file.Name
            Module     = $module
            CodeParams = ($paramKeys | Select-Object -Unique)
        }
    }
}

# ==============================================================================
# PHASE 4: Categorize & Report
# ==============================================================================
$missingSps = @()
$paramMismatches = @()
$orphanedSps = @()
$validSps = @()

foreach ($call in $daoCalls) {
    if (-not $sqlDefinitions.ContainsKey($call.SpName)) {
        $missingSps += [PSCustomObject]@{
            Module  = $call.Module
            DaoFile = $call.DaoFile
            SpName  = $call.SpName
        }
    }
    else {
        $def = $sqlDefinitions[$call.SpName]
        $validSps += [PSCustomObject]@{
            Module    = $call.Module
            DaoFile   = $call.DaoFile
            SpName    = $call.SpName
            SqlParams = ($def.Parameters -join ", ")
            SqlFile   = $def.File
        }
    }
}

$md = @()
$md += "# Stored Procedure Master Audit Report"
$md += ""
$md += "**Generated:** $(Get-Date)"
$md += ""
$md += "## Executive Summary"
$md += "- **Tables Parsed:** $($schemaDefinitions.Keys.Count)"
$md += "- **Column Mismatches (Logic Errors):** $($spColumnErrors.Count)"
$md += ""

$md += "## 1. CRITICAL: Column Mismatches (Schema Validation)"
$md += "Stored procedures attempting to select columns that **do not exist** in the table definition."
$md += ""
$md += "| SP Name | Target Table | Unknown Column | File |"
$md += "|---|---|---|---|"
if ($spColumnErrors.Count -eq 0) { $md += "| - | - | None | - |" }
foreach ($err in $spColumnErrors) {
    $md += "| **$($err.SpName)** | ``$($err.Table)`` | **``$($err.BadColumn)``** | ``$($err.File)`` |"
}
$md += ""

$md += "## 3. Validated Workflow (Inventory)"
$md += "| Module | DAO File | Stored Procedure | SQL File |"
$md += "|---|---|---|---|"
foreach ($x in ($validSps | Sort-Object Module, DaoFile)) {
    $md += "| $($x.Module) | ``$($x.DaoFile)`` | ``$($x.SpName)`` | ``$($x.SqlFile)`` |"
}

$md | Out-File $reportFile -Encoding utf8 -Force
Write-Host "Audit Complete. Report saved to: $reportFile" -ForegroundColor Green
