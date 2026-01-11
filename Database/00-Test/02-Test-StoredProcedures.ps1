# Script: 02-Test-StoredProcedures.ps1
# Description: Dynamically calls every Stored Procedure in the database with mock data to check for Runtime logic/schema errors.
# Dependency: MySql.Data.dll (Usually available if the app builds)
# Configuration: 01-mock-data.json (parameter values), 01-order.json (execution sequence)

param (
    [string]$Server = "localhost",
    [int]$Port = 3306,  # MAMP default is 3306, MAMP PRO uses 8889
    [string]$Database = "mtm_receiving_application",
    [string]$User = "root",
    [securestring]$Password,
    [switch]$UseExecutionOrder  # Use 01-order.json instead of alphabetical
)

# -----------------------------------------------------------------------------
# PowerShell runtime guard
# -----------------------------------------------------------------------------
if ($PSVersionTable.PSEdition -eq "Desktop") {
    $pwshCmd = Get-Command pwsh -ErrorAction SilentlyContinue
    if ($pwshCmd) {
        Write-Warning "Windows PowerShell 5.1 detected. Relaunching under PowerShell 7 (pwsh) for .NET 8 MySql.Data compatibility..."

        $scriptPath = $MyInvocation.MyCommand.Path
        $portArg = if ($Port -ne 3306) { "-Port $Port" } else { "" }
        & $pwshCmd.Source -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Server $Server $portArg -Database $Database -User $User
        return
    }

    Write-Error "PowerShell 7+ (pwsh) is required to run this script with the app's net8 MySql.Data.dll. Install PowerShell 7 and run: pwsh -File .\Database\00-Test\02-Test-StoredProcedures.ps1"
    return
}

# Convert SecureString to plain text for connection string (required by MySqlConnection)
if (-not $Password) {
    Write-Host "MAMP default password is 'root'. Press Enter to use default, or type custom password:" -ForegroundColor Yellow
    $securePassword = Read-Host -AsSecureString
    # Check if empty (Enter pressed) - use default MAMP password
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    $PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
    
    if ([string]::IsNullOrWhiteSpace($PlainPassword)) {
        $PlainPassword = "root"  # MAMP default
        Write-Host "Using default MAMP password 'root'" -ForegroundColor Cyan
    }
}
else {
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
    $PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
}

# Load MySQL Assembly from project bin directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$databaseDir = Split-Path -Parent $scriptDir
$projectRoot = Split-Path -Parent $databaseDir
$binPath = Join-Path $projectRoot "bin"

$mysqlDll = Get-ChildItem -Path $binPath -Filter "MySql.Data.dll" -Recurse -ErrorAction SilentlyContinue | 
Where-Object { $_.FullName -match "\\bin\\.*\\net8\.0" } | 
Select-Object -First 1

if (-not $mysqlDll) {
    Write-Error "MySql.Data.dll not found in project bin. Please build the project first (dotnet build). Searched in: $binPath"
    exit 1
}

$mysqlDir = Split-Path -Parent $mysqlDll.FullName

# Register assembly resolver
$onAssemblyResolve = [System.ResolveEventHandler] {
    param($senderObj, $resolveArgs)
    $assemblyName = New-Object System.Reflection.AssemblyName($resolveArgs.Name)
    $dllPath = Join-Path $mysqlDir "$($assemblyName.Name).dll"
    if (Test-Path $dllPath) {
        return [System.Reflection.Assembly]::LoadFrom($dllPath)
    }
    return $null
}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve($onAssemblyResolve)

try {
    $dependencyDlls = Get-ChildItem -Path $mysqlDir -Filter "*.dll" -File | 
    Where-Object { $_.Name -match "^(MySql\.|System\.|ZstdSharp|K4os|BouncyCastle)" }
    foreach ($dll in $dependencyDlls) {
        try {
            $null = [System.Reflection.Assembly]::LoadFrom($dll.FullName)
        }
        catch {
            # Ignore already loaded assemblies
        }
    }
    Write-Host "Loaded MySql.Data.dll from $($mysqlDll.FullName)" -ForegroundColor Green
}
catch {
    Write-Error "Failed to load MySql.Data.dll: $_"
    exit 1
}

# Build connection string with port
$connStr = "Server=$Server;Port=$Port;Database=$Database;Uid=$User;Pwd=$PlainPassword;SslMode=none;"

try {
    $conn = New-Object MySql.Data.MySqlClient.MySqlConnection($connStr)
}
catch {
    Write-Error "Failed to create MySqlConnection: $_"
    Write-Host "Connection string (without password): Server=$Server;Port=$Port;Database=$Database;Uid=$User;SslMode=none;" -ForegroundColor Yellow
    exit 1
}

try {
    $conn.Open()
    Write-Host "Connected to $Database on $Server`:$Port (MAMP)" -ForegroundColor Green
}
catch {
    Write-Error "Failed to connect: $_"
    Write-Host "Common MAMP issues:" -ForegroundColor Yellow
    Write-Host "  - MAMP MySQL service not running (Start Servers in MAMP)" -ForegroundColor Yellow
    Write-Host "  - Wrong port (Default: 3306, MAMP PRO: 8889)" -ForegroundColor Yellow
    Write-Host "  - Database '$Database' doesn't exist (Create via phpMyAdmin)" -ForegroundColor Yellow
    exit 1
}

# Load configuration files
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$mockDataPath = Join-Path $scriptDir "01-mock-data.json"
$executionOrderPath = Join-Path $scriptDir "01-order.json"

$mockData = $null
$executionOrderConfig = $null

if (Test-Path $mockDataPath) {
    $mockData = Get-Content $mockDataPath -Raw | ConvertFrom-Json
    Write-Host "✓ Loaded mock data configuration from 01-mock-data.json" -ForegroundColor Green
}
else {
    Write-Warning "Mock data file not found: $mockDataPath"
    Write-Host "Run 01-Generate-SP-TestData.ps1 first to create configuration files." -ForegroundColor Yellow
}

if ($UseExecutionOrder -and (Test-Path $executionOrderPath)) {
    $executionOrderConfig = Get-Content $executionOrderPath -Raw | ConvertFrom-Json
    Write-Host "✓ Using execution order from 01-order.json" -ForegroundColor Green
}

# 1. Get List of Stored Procedures
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT ROUTINE_NAME FROM information_schema.ROUTINES WHERE ROUTINE_SCHEMA = '$Database' AND ROUTINE_TYPE = 'PROCEDURE' ORDER BY ROUTINE_NAME"
$reader = $cmd.ExecuteReader()
$spList = @()
while ($reader.Read()) {
    $spList += $reader["ROUTINE_NAME"].ToString()
}
$reader.Close()

# Apply execution order if specified
if ($UseExecutionOrder -and $executionOrderConfig) {
    $orderedList = @()
    foreach ($group in $executionOrderConfig.executionGroups | Sort-Object order) {
        foreach ($sp in $group.storedProcedures) {
            if ($spList -contains $sp) {
                $orderedList += $sp
            }
        }
    }
    # Add any SPs not in execution order config
    foreach ($sp in $spList) {
        if ($orderedList -notcontains $sp) {
            $orderedList += $sp
        }
    }
    $spList = $orderedList
    Write-Host "Executing in dependency order ($($spList.Count) procedures)" -ForegroundColor Cyan
}
else {
    Write-Host "Found $($spList.Count) stored procedures to test (alphabetical order)" -ForegroundColor Cyan
}

$results = @()

foreach ($sp in $spList) {
    # 2. Get Parameters for this SP
    $cmdParams = $conn.CreateCommand()
    $cmdParams.CommandText = @"
        SELECT PARAMETER_NAME, DATA_TYPE, PARAMETER_MODE 
        FROM information_schema.PARAMETERS 
        WHERE SPECIFIC_SCHEMA = '$Database' 
        AND SPECIFIC_NAME = '$sp' 
        ORDER BY ORDINAL_POSITION
"@
    $reader = $cmdParams.ExecuteReader()
    
    $paramDefs = @()
    $outParams = @()
    while ($reader.Read()) {
        $paramName = $reader["PARAMETER_NAME"]
        if ($paramName -isnot [DBNull]) {
            $mode = if ($reader["PARAMETER_MODE"] -is [DBNull]) { "IN" } else { $reader["PARAMETER_MODE"].ToString() }
            $param = [PSCustomObject]@{
                Name = $paramName.ToString()
                Type = $reader["DATA_TYPE"].ToString()
                Mode = $mode
            }
            
            # IN and INOUT parameters need values, OUT does not
            if ($mode -eq "IN" -or $mode -eq "INOUT") {
                $paramDefs += $param
            }
            else {
                $outParams += $param  # Track pure OUT params for diagnostics
            }
        }
    }
    $reader.Close()

    # 3. Construct Call with Mock Data (IN and INOUT parameters)
    $testCall = "CALL $sp("
    $paramValues = @()
    
    # All paramDefs are now IN or INOUT mode
    foreach ($p in $paramDefs) {
        # Use mock data if available
        $mockValue = $null
        if ($mockData -and $mockData.$sp -and $mockData.$sp.parameters.($p.Name)) {
            $mockValue = $mockData.$sp.parameters.($p.Name)
        }
            
        # Format value based on type
        if ($null -eq $mockValue) {
            # Fallback to type-based defaults
            switch -Regex ($p.Type) {
                'int|decimal|float|double|bigint|smallint' { $paramValues += "1" }
                'bool|bit|tinyint' { $paramValues += "1" }
                'date' { $paramValues += "'2026-01-01'" }
                'datetime|timestamp' { $paramValues += "'2026-01-01 12:00:00'" }
                'char|varchar|text|longtext' { $paramValues += "'TEST_VALUE'" }
                'blob' { $paramValues += "NULL" }
                default { $paramValues += "NULL" }
            }
        }
        else {
            # Format based on type
            switch -Regex ($p.Type) {
                'int|decimal|float|double|bigint|smallint' { 
                    $paramValues += $mockValue 
                }
                'bool|bit|tinyint' { 
                    $paramValues += if ($mockValue) { "1" } else { "0" }
                }
                'date' {
                    if ($mockValue -match '^\d{4}-\d{2}-\d{2}$') {
                        $paramValues += "'$mockValue'"
                    }
                    else {
                        $paramValues += "'2026-01-01'"
                    }
                }
                'datetime|timestamp' {
                    if ($mockValue -match '^\d{4}-\d{2}-\d{2}') {
                        $paramValues += "'$mockValue'"
                    }
                    else {
                        $paramValues += "'2026-01-01 12:00:00'"
                    }
                }
                default {
                    # String types
                    $escaped = $mockValue.ToString().Replace("'", "\'")
                    $paramValues += "'$escaped'"
                }
            }
        }
    }
    
    $testCall += ($paramValues -join ", ") + ");"

    # 4. Execute
    $testCmd = $conn.CreateCommand()
    $testCmd.CommandText = $testCall
    
    $status = "PASS"
    $errorMsg = ""
    $errorCode = 0

    try {
        $null = $testCmd.ExecuteNonQuery()
    }
    catch {
        $status = "FAIL"
        $errorMsg = $_.Exception.Message
        if ($_.Exception.InnerException -and $_.Exception.InnerException.Number) {
            $errorCode = $_.Exception.InnerException.Number
        }
        elseif ($_.Exception.Number) {
            $errorCode = $_.Exception.Number
        }
    }

    # Categorize Error
    $category = "Unknown"
    if ($status -eq "PASS") { 
        $category = "Valid" 
    }
    elseif ($errorCode -eq 1054 -or $errorCode -eq 1146) {
        $category = "SchemaBroken"
    }
    elseif ($errorCode -eq 1048 -or $errorCode -eq 1062) {
        $category = "Constraint"
    }
    elseif ($status -eq "FAIL" -and $errorMsg -match "SQLSTATE\[45000\]") {
        $category = "LogicCaught"
    }
    else {
        $category = "RuntimeError"
    }

    $results += [PSCustomObject]@{
        SP        = $sp
        Status    = $status
        Category  = $category
        ErrorCode = $errorCode
        Message   = $errorMsg
        TestCall  = $testCall
        InParams  = $paramDefs.Count
        OutParams = $outParams.Count
    }
    
    $color = if ($category -eq "Valid") { "Green" } elseif ($category -eq "SchemaBroken") { "Red" } else { "Gray" }
    Write-Host "[$category] $sp" -ForegroundColor $color
}

$conn.Close()

# 5. Generate Comprehensive Test Report
$reportFile = Join-Path $scriptDir "02-report.md"
$md = @()

# Header
$md += "# Stored Procedure Execution Test Report"
$md += ""
$md += "**Generated:** $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$md += "**Database:** $Database"
$md += "**Server:** $Server`:$Port"
$md += "**Execution Mode:** $(if ($UseExecutionOrder) { 'Dependency Order' } else { 'Alphabetical' })"
$md += ""

# Summary Statistics
$totalTests = $results.Count
$passed = ($results | Where-Object Status -eq 'PASS').Count
$failed = ($results | Where-Object Status -eq 'FAIL').Count
$schemaBroken = ($results | Where-Object Category -eq 'SchemaBroken').Count
$runtimeErrors = ($results | Where-Object Category -eq 'RuntimeError').Count
$constraints = ($results | Where-Object Category -eq 'Constraint').Count
$logicCaught = ($results | Where-Object Category -eq 'LogicCaught').Count

$successRate = if ($totalTests -gt 0) { [math]::Round(($passed / $totalTests) * 100, 1) } else { 0 }

$md += "## Summary"
$md += ""
$md += "| Metric | Count | Percentage |"
$md += "|--------|-------|------------|"
$md += "| **Total Tests** | $totalTests | 100% |"
$md += "| **Passed** | $passed | $successRate% |"
$md += "| **Failed** | $failed | $([math]::Round(($failed / $totalTests) * 100, 1))% |"
$md += "| Schema Broken | $schemaBroken | $([math]::Round(($schemaBroken / $totalTests) * 100, 1))% |"
$md += "| Runtime Errors | $runtimeErrors | $([math]::Round(($runtimeErrors / $totalTests) * 100, 1))% |"
$md += "| Constraint Violations | $constraints | $([math]::Round(($constraints / $totalTests) * 100, 1))% |"
$md += "| Business Logic Validations | $logicCaught | $([math]::Round(($logicCaught / $totalTests) * 100, 1))% |"
$md += ""

# Results by Category (if mock data available)
if ($mockData) {
    $md += "## Results by Category"
    $md += ""
    
    $categoryStats = $results | Group-Object { 
        if ($mockData.($_.SP)) { 
            $mockData.($_.SP).category 
        }
        else { 
            "Uncategorized" 
        }
    } | Sort-Object Name
    
    $md += "| Category | Total | Passed | Failed | Success Rate |"
    $md += "|----------|-------|--------|--------|--------------|"
    
    foreach ($cat in $categoryStats) {
        $catTotal = $cat.Count
        $catPassed = ($cat.Group | Where-Object Status -eq 'PASS').Count
        $catFailed = ($cat.Group | Where-Object Status -eq 'FAIL').Count
        $catRate = if ($catTotal -gt 0) { [math]::Round(($catPassed / $catTotal) * 100, 1) } else { 0 }
        
        $md += "| $($cat.Name) | $catTotal | $catPassed | $catFailed | $catRate% |"
    }
    $md += ""
}

# Critical Failures (Schema Broken)
$md += "## 🔴 Critical Failures (Schema Broken)"
$md += ""
$md += "These stored procedures reference columns or tables that don't exist in the database."
$md += ""
$md += "| SP Name | Error Code | Message | IN Params | OUT Params |"
$md += "|---------|------------|---------|-----------|------------|"
$broken = $results | Where-Object Category -eq 'SchemaBroken'
if ($broken) {
    foreach ($r in $broken) {
        $md += "| **$($r.SP)** | $($r.ErrorCode) | $($r.Message) | $($r.InParams) | $($r.OutParams) |"
    }
}
else {
    $md += "| - | - | ✓ No schema errors | - | - |"
}
$md += ""

# Parameter Mismatches
$md += "## ⚠️ Parameter Mismatches"
$md += ""
$paramMismatches = $others | Where-Object { $_.Message -match "Incorrect number of arguments" }
if ($paramMismatches) {
    $md += "These stored procedures have parameter count mismatches between the database definition and mock data."
    $md += ""
    $md += "| SP Name | Message | IN Params | OUT Params |"
    $md += "|---------|---------|-----------|------------|"
    foreach ($r in $paramMismatches) {
        $md += "| $($r.SP) | $($r.Message) | $($r.InParams) | $($r.OutParams) |"
    }
}
else {
    $md += "✓ No parameter mismatches detected"
}
$md += ""

# Constraint Violations
$md += "## 🔗 Constraint Violations"
$md += ""
$constraintErrors = $others | Where-Object { $_.Message -match "foreign key constraint|Cannot add or update a child row|Cannot delete or update a parent row" }
if ($constraintErrors) {
    $md += "These stored procedures failed due to foreign key constraints (missing prerequisite data)."
    $md += ""
    $md += "| SP Name | Message |"
    $md += "|---------|---------|"
    foreach ($r in $constraintErrors) {
        $md += "| $($r.SP) | $($r.Message) |"
    }
    $md += ""
    $md += "**Recommendation:** Run with ``-UseExecutionOrder`` flag or add prerequisite test data."
}
else {
    $md += "✓ No constraint violations"
}
$md += ""

# Data Validation Errors
$md += "## 📋 Data Validation Errors"
$md += ""
$validationErrors = $others | Where-Object { $_.Message -match "Data truncated|already exists|Illegal mix of collations" }
if ($validationErrors) {
    $md += "These stored procedures failed due to data validation issues."
    $md += ""
    $md += "| SP Name | Message |"
    $md += "|---------|---------|"
    foreach ($r in $validationErrors) {
        $md += "| $($r.SP) | $($r.Message) |"
    }
    $md += ""
    $md += "**Recommendation:** Review mock data values in ``01-mock-data.json``."
}
else {
    $md += "✓ No data validation errors"
}
$md += ""

# Other Runtime Errors
$md += "## 🔧 Other Runtime Errors"
$md += ""
$otherErrors = $others | Where-Object { 
    $_.Category -eq 'RuntimeError' -and 
    $_.Message -notmatch "Incorrect number of arguments" -and
    $_.Message -notmatch "foreign key constraint" -and
    $_.Message -notmatch "Cannot add or update" -and
    $_.Message -notmatch "Cannot delete or update" -and
    $_.Message -notmatch "Data truncated" -and
    $_.Message -notmatch "already exists" -and
    $_.Message -notmatch "Illegal mix of collations"
}
if ($otherErrors) {
    $md += "| SP Name | Category | Message |"
    $md += "|---------|----------|---------|"
    foreach ($r in $otherErrors) {
        $md += "| $($r.SP) | $($r.Category) | $($r.Message) |"
    }
}
else {
    $md += "✓ No other runtime errors"
}
$md += ""

# Recommendations
$md += "## 💡 Recommendations"
$md += ""

if ($schemaBroken -gt 0) {
    $md += "### Critical: Fix Schema Issues"
    $md += "- $schemaBroken stored procedure(s) reference non-existent columns or tables"
    $md += "- Review and update stored procedure SQL or database schema"
    $md += ""
}

if ($paramMismatches.Count -gt 0) {
    $md += "### High Priority: Fix Parameter Mismatches"
    $md += "- $($paramMismatches.Count) stored procedure(s) have parameter count issues"
    $md += "- Regenerate mock data: ``pwsh -File .\Database\00-Test\Generate-SP-TestData.ps1``"
    $md += ""
}

if ($constraintErrors.Count -gt 0) {
    $md += "### Improve Test Data"
    $md += "- $($constraintErrors.Count) stored procedure(s) failed due to missing FK references"
    $md += "- Use dependency-aware execution: ``pwsh -File .\Database\00-Test\02-Test-StoredProcedures.ps1 -UseExecutionOrder``"
    $md += "- Or add prerequisite test data to the database"
    $md += ""
}

if ($successRate -eq 100) {
    $md += "### ✅ All Tests Passed!"
    $md += "- All $totalTests stored procedures executed successfully"
    $md += "- Database schema and stored procedures are in sync"
    $md += ""
}
elseif ($successRate -ge 80) {
    $md += "### Good Progress"
    $md += "- $successRate% success rate"
    $md += "- Focus on fixing the $failed remaining issues"
    $md += ""
}
else {
    $md += "### Needs Attention"
    $md += "- Only $successRate% success rate"
    $md += "- Review error categories above and prioritize fixes"
    $md += ""
}

# Test Configuration
$md += "## Test Configuration"
$md += ""
$md += "- **Mock Data File:** $(if (Test-Path $mockDataPath) { '✓ Loaded' } else { '✗ Not found' })"
$md += "- **Execution Order File:** $(if (Test-Path $executionOrderPath) { '✓ Loaded' } else { '✗ Not found' })"
$md += "- **Using Execution Order:** $(if ($UseExecutionOrder) { 'Yes' } else { 'No' })"
$md += ""

# Footer
$md += "---"
$md += ""
$md += "**Report Generated By:** 02-Test-StoredProcedures.ps1"
$md += ""
$md += "**Next Steps:**"
$md += "1. Fix critical schema issues first (red flags above)"
$md += "2. Regenerate mock data if parameter mismatches exist"
$md += "3. Run with ``-UseExecutionOrder`` flag to reduce FK constraint errors"
$md += "4. Review and customize mock data values in ``01-mock-data.json``"

$md | Out-File $reportFile -Encoding utf8
Write-Host "`n✓ Generated test report: $reportFile" -ForegroundColor Green
Write-Host "  - Total Tests: $totalTests" -ForegroundColor Cyan
Write-Host "  - Passed: $passed ($successRate%)" -ForegroundColor $(if ($successRate -ge 80) { 'Green' } elseif ($successRate -ge 50) { 'Yellow' } else { 'Red' })
Write-Host "  - Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { 'Green' } else { 'Red' })
