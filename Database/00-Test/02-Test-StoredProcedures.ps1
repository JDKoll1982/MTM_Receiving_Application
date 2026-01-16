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
        $args = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $scriptPath, '-Server', $Server, '-Database', $Database, '-User', $User)
        if ($Port -ne 3306) {
            $args += @('-Port', $Port)
        }
        if ($UseExecutionOrder) {
            $args += '-UseExecutionOrder'
        }
        & $pwshCmd.Source @args
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

# Build connection string with port and options for stored procedure testing
# NOTE: OUT parameter testing uses multi-statement batches (SET @out...; CALL ...; SELECT @out...;)
# which requires AllowBatch/Allow User Variables.
$connStr = "Server=$Server;Port=$Port;Database=$Database;Uid=$User;Pwd=$PlainPassword;SslMode=none;AllowUserVariables=true;Allow User Variables=true;AllowBatch=true;"

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
    Write-Host "Loaded mock data configuration from 01-mock-data.json" -ForegroundColor Green
}
else {
    Write-Warning "Mock data file not found: $mockDataPath"
    Write-Host "Run 01-Generate-SP-TestData.ps1 first to create configuration files." -ForegroundColor Yellow
}

# Make insert-style test values unique per run to avoid duplicate-key validations
$runToken = (Get-Date).ToString("yyyyMMddHHmmss")
$shortToken = (Get-Date).ToString("MMddHHmmss")
try {
    if ($mockData.'sp_Dunnage_Types_Insert') {
        $mockData.'sp_Dunnage_Types_Insert'.parameters.p_type_name = "TEST_DUNNAGE_TYPE_INSERT_$runToken"
    }
    if ($mockData.'sp_Dunnage_Parts_Insert') {
        $mockData.'sp_Dunnage_Parts_Insert'.parameters.p_part_id = "TEST_DUNNAGE_PART_$runToken"
    }
    if ($mockData.'sp_Settings_RoutingRule_Insert') {
        $mockData.'sp_Settings_RoutingRule_Insert'.parameters.p_pattern = "TEST_PATTERN_INSERT_$runToken"
        $mockData.'sp_Settings_RoutingRule_Insert'.parameters.p_destination_location = "LOC_TEST_INSERT_$runToken"
    }
    if ($mockData.'sp_Volvo_PartMaster_Insert') {
        # VARCHAR(20) constraint: keep this short
        $mockData.'sp_Volvo_PartMaster_Insert'.parameters.p_part_number = "TP$shortToken"
    }
}
catch { }

if ($UseExecutionOrder -and (Test-Path $executionOrderPath)) {
    $executionOrderConfig = Get-Content $executionOrderPath -Raw | ConvertFrom-Json
    Write-Host "Using execution order from 01-order.json" -ForegroundColor Green
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
    # Optional: allow disabling individual SP tests via 01-mock-data.json
    if ($mockData -and $mockData.$sp -and $null -ne $mockData.$sp.enabled -and $mockData.$sp.enabled -eq $false) {
        Write-Host "[Skipped] $sp" -ForegroundColor DarkGray
        continue
    }

    # Pre-test cleanup for settings SPs: ensure the target setting isn't locked.
    if ($sp -in @('sp_Settings_System_UpdateValue', 'sp_Settings_System_ResetToDefault')) {
        try {
            $settingId = 0
            if ($mockData -and $mockData.$sp -and $mockData.$sp.parameters -and $mockData.$sp.parameters.p_setting_id) {
                $settingId = [int]$mockData.$sp.parameters.p_setting_id
            }
            if ($settingId -gt 0) {
                $unlockCmd = $conn.CreateCommand()
                $unlockCmd.CommandText = "UPDATE settings_universal SET is_locked = 0 WHERE id = $settingId;"
                $null = $unlockCmd.ExecuteNonQuery()
            }
        }
        catch { }
    }

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
                'bool|bit|tinyint' { $paramValues += "1"; break }
                'int|decimal|float|double|bigint|smallint' { $paramValues += "1"; break }
                'date' { $paramValues += "'2026-01-01'"; break }
                'datetime|timestamp' { $paramValues += "'2026-01-01 12:00:00'"; break }
                'char|varchar|text|longtext' { $paramValues += "'TEST_VALUE'"; break }
                'blob' { $paramValues += "NULL"; break }
                default { $paramValues += "NULL" }
            }
        }
        else {
            # Format based on type
            switch -Regex ($p.Type) {
                'bool|bit|tinyint' {
                    $converted = if ($mockValue) { "1" } else { "0" }
                    $paramValues += $converted
                    break
                }
                'int|decimal|float|double|bigint|smallint' {
                    # Ensure numeric types never receive unquoted non-numeric strings (e.g., "PO12345"),
                    # which MySQL interprets as an identifier and can raise "Unknown column ...".
                    $text = $mockValue.ToString()
                    if ($text -match '^-?\d+(\.\d+)?$') {
                        $paramValues += $text
                    }
                    else {
                        $paramValues += "1"
                    }
                    break
                }
                'date' {
                    if ($mockValue -match '^\d{4}-\d{2}-\d{2}$') {
                        $paramValues += "'$mockValue'"
                    }
                    else {
                        $paramValues += "'2026-01-01'"
                    }
                    break
                }
                'datetime|timestamp' {
                    if ($mockValue -match '^\d{4}-\d{2}-\d{2}') {
                        $paramValues += "'$mockValue'"
                    }
                    else {
                        $paramValues += "'2026-01-01 12:00:00'"
                    }
                    break
                }
                default {
                    # String types
                    $escaped = $mockValue.ToString().Replace("'", "\'")
                    $paramValues += "'$escaped'"
                }
            }
        }
    }

    # Prepare OUT parameter variables and add to parameter list
    # NOTE: We intentionally do NOT pre-initialize user variables with SET here.
    # Some MySql.Data configurations can choke on the leading SET statement for certain routines;
    # MySQL will create/assign the user variable when it is used as an OUT argument.
    $outVarSelects = @()
    foreach ($outParam in $outParams) {
        $varName = "@out_$($outParam.Name)"
        $paramValues += $varName
        $outVarSelects += $varName
    }

    $testCall += ($paramValues -join ", ") + ");"

    # Build complete SQL batch
    $sqlBatch = "" + $testCall
    if ($outVarSelects.Count -gt 0) {
        $sqlBatch += " SELECT " + ($outVarSelects -join ", ") + ";"
    }

    # Known business-rule trigger: only one pending Volvo shipment allowed.
    # For test execution, archive any existing pending shipment before attempting an insert.
    if ($sp -eq 'sp_Volvo_Shipment_Insert') {
        $cleanupCmd = $conn.CreateCommand()
        $cleanupCmd.CommandText = "UPDATE volvo_label_data SET is_archived = 1 WHERE status = 'pending_po' AND is_archived = 0;"
        try { $null = $cleanupCmd.ExecuteNonQuery() } catch { }
    }

    # 4. Execute
    $testCmd = $conn.CreateCommand()
    $testCmd.CommandText = $sqlBatch

    $status = "PASS"
    $errorMsg = ""
    $errorCode = 0

    try {
        $result = $testCmd.ExecuteReader()
        # Read all result sets (including OUT parameter values)
        while ($result.Read()) {
            # Successfully read result
        }
        while ($result.NextResult()) {
            while ($result.Read()) {
                # Read subsequent result sets
            }
        }
        $result.Close()
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

# 5. Generate Comprehensive Test Report using template
$reportFile = Join-Path $scriptDir "02-report.md"
$templatePath = Join-Path $scriptDir "templates\02-report-template.md"

# Calculate statistics
$totalTests = $results.Count
$passed = ($results | Where-Object Status -eq 'PASS').Count
$failed = ($results | Where-Object Status -eq 'FAIL').Count
$schemaBroken = ($results | Where-Object Category -eq 'SchemaBroken').Count
$runtimeErrors = ($results | Where-Object Category -eq 'RuntimeError').Count
$constraints = ($results | Where-Object Category -eq 'Constraint').Count
$logicCaught = ($results | Where-Object Category -eq 'LogicCaught').Count

$successRate = if ($totalTests -gt 0) { [math]::Round(($passed / $totalTests) * 100, 1) } else { 0 }
$failRate = if ($totalTests -gt 0) { [math]::Round(($failed / $totalTests) * 100, 1) } else { 0 }
$schemaRate = if ($totalTests -gt 0) { [math]::Round(($schemaBroken / $totalTests) * 100, 1) } else { 0 }
$runtimeRate = if ($totalTests -gt 0) { [math]::Round(($runtimeErrors / $totalTests) * 100, 1) } else { 0 }
$constraintRate = if ($totalTests -gt 0) { [math]::Round(($constraints / $totalTests) * 100, 1) } else { 0 }
$logicRate = if ($totalTests -gt 0) { [math]::Round(($logicCaught / $totalTests) * 100, 1) } else { 0 }

# Build Results by Category section
$resultsByCategory = ""
if ($mockData) {
    $categoryLines = @()
    $categoryLines += "## Results by Category"
    $categoryLines += ""

    $categoryStats = $results | Group-Object {
        if ($mockData.($_.SP)) { $mockData.($_.SP).category }
        else { "Uncategorized" }
    } | Sort-Object Name

    $categoryLines += "| Category | Total | Passed | Failed | Success Rate |"
    $categoryLines += "|----------|-------|--------|--------|--------------|"

    foreach ($cat in $categoryStats) {
        $catTotal = $cat.Count
        $catPassed = ($cat.Group | Where-Object Status -eq 'PASS').Count
        $catFailed = ($cat.Group | Where-Object Status -eq 'FAIL').Count
        $catRate = if ($catTotal -gt 0) { [math]::Round(($catPassed / $catTotal) * 100, 1) } else { 0 }

        $categoryLines += "| $($cat.Name) | $catTotal | $catPassed | $catFailed | $catRate% |"
    }
    $categoryLines += ""
    $resultsByCategory = $categoryLines -join "`n"
}

# Build schema broken table
$schemaBrokenTable = ""
$broken = $results | Where-Object Category -eq 'SchemaBroken'
if ($broken) {
    $brokenLines = @()
    foreach ($r in $broken) {
        $brokenLines += "| **$($r.SP)** | $($r.ErrorCode) | $($r.Message) | $($r.InParams) | $($r.OutParams) |"
    }
    $schemaBrokenTable = $brokenLines -join "`n"
}
else {
    $schemaBrokenTable = "| - | - | No schema errors | - | - |"
}

# Build parameter mismatches table
$paramMismatchesTable = ""
$paramMismatches = $results | Where-Object { $_.Status -eq 'FAIL' -and $_.Message -match "Incorrect number of arguments" }
if ($paramMismatches) {
    $pmLines = @()
    $pmLines += "These stored procedures have parameter count mismatches between the database definition and mock data."
    $pmLines += ""
    $pmLines += "| SP Name | Message | IN Params | OUT Params |"
    $pmLines += "|---------|---------|-----------|------------|"
    foreach ($r in $paramMismatches) {
        $pmLines += "| $($r.SP) | $($r.Message) | $($r.InParams) | $($r.OutParams) |"
    }
    $paramMismatchesTable = $pmLines -join "`n"
}
else {
    $paramMismatchesTable = "No parameter mismatches detected"
}

# Build constraint violations table
$constraintViolationsTable = ""
$constraintErrors = $results | Where-Object { $_.Status -eq 'FAIL' -and $_.Message -match "foreign key constraint|Cannot add or update a child row|Cannot delete or update a parent row" }
if ($constraintErrors) {
    $cvLines = @()
    $cvLines += "These stored procedures failed due to foreign key constraints (missing prerequisite data)."
    $cvLines += ""
    $cvLines += "| SP Name | Message |"
    $cvLines += "|---------|---------|"
    foreach ($r in $constraintErrors) {
        $cvLines += "| $($r.SP) | $($r.Message) |"
    }
    $cvLines += ""
    $cvLines += "**Recommendation:** Run with ``-UseExecutionOrder`` flag or add prerequisite test data."
    $constraintViolationsTable = $cvLines -join "`n"
}
else {
    $constraintViolationsTable = "No constraint violations"
}

# Build validation errors table
$validationErrorsTable = ""
$validationErrors = $results | Where-Object { $_.Status -eq 'FAIL' -and $_.Message -match "Data truncated|already exists|Illegal mix of collations" }
if ($validationErrors) {
    $veLines = @()
    $veLines += "These stored procedures failed due to data validation issues."
    $veLines += ""
    $veLines += "| SP Name | Message |"
    $veLines += "|---------|---------|"
    foreach ($r in $validationErrors) {
        $veLines += "| $($r.SP) | $($r.Message) |"
    }
    $veLines += ""
    $veLines += "**Recommendation:** Review mock data values in ``01-mock-data.json``."
    $validationErrorsTable = $veLines -join "`n"
}
else {
    $validationErrorsTable = "No data validation errors"
}

# Build other errors table
$otherErrorsTable = ""
$otherErrors = $results | Where-Object {
    $_.Status -eq 'FAIL' -and $_.Category -eq 'RuntimeError' -and
    $_.Message -notmatch "Incorrect number of arguments" -and
    $_.Message -notmatch "foreign key constraint" -and
    $_.Message -notmatch "Cannot add or update" -and
    $_.Message -notmatch "Cannot delete or update" -and
    $_.Message -notmatch "Data truncated" -and
    $_.Message -notmatch "already exists" -and
    $_.Message -notmatch "Illegal mix of collations"
}
if ($otherErrors) {
    $oeLines = @()
    $oeLines += "| SP Name | Category | Message |"
    $oeLines += "|---------|----------|---------|"
    foreach ($r in $otherErrors) {
        $oeLines += "| $($r.SP) | $($r.Category) | $($r.Message) |"
    }
    $otherErrorsTable = $oeLines -join "`n"
}
else {
    $otherErrorsTable = "No other runtime errors"
}

# Build recommendations section
$recommendations = @()
if ($schemaBroken -gt 0) {
    $recommendations += "### Critical: Fix Schema Issues"
    $recommendations += "- $schemaBroken stored procedure(s) reference non-existent columns or tables"
    $recommendations += "- Review and update stored procedure SQL or database schema"
    $recommendations += ""
}

if ($paramMismatches.Count -gt 0) {
    $recommendations += "### High Priority: Fix Parameter Mismatches"
    $recommendations += "- $($paramMismatches.Count) stored procedure(s) have parameter count issues"
    $recommendations += "- Regenerate mock data: ``pwsh -File .\Database\00-Test\01-Generate-SP-TestData.ps1``"
    $recommendations += ""
}

if ($constraintErrors.Count -gt 0) {
    $recommendations += "### Improve Test Data"
    $recommendations += "- $($constraintErrors.Count) stored procedure(s) failed due to missing FK references"
    $recommendations += "- Use dependency-aware execution: ``pwsh -File .\Database\00-Test\02-Test-StoredProcedures.ps1 -UseExecutionOrder``"
    $recommendations += "- Or add prerequisite test data to the database"
    $recommendations += ""
}

if ($successRate -eq 100) {
    $recommendations += "### All Tests Passed!"
    $recommendations += "- All $totalTests stored procedures executed successfully"
    $recommendations += "- Database schema and stored procedures are in sync"
    $recommendations += ""
}
elseif ($successRate -ge 80) {
    $recommendations += "### Good Progress"
    $recommendations += "- $successRate% success rate"
    $recommendations += "- Focus on fixing the $failed remaining issues"
    $recommendations += ""
}
else {
    $recommendations += "### Needs Attention"
    $recommendations += "- Only $successRate% success rate"
    $recommendations += "- Review error categories above and prioritize fixes"
    $recommendations += ""
}

# Load template and replace placeholders
$template = Get-Content $templatePath -Raw
$content = $template `
    -replace '{{TIMESTAMP}}', (Get-Date -Format 'yyyy-MM-dd HH:mm:ss') `
    -replace '{{DATABASE}}', $Database `
    -replace '{{SERVER}}', "$Server`:$Port" `
    -replace '{{EXECUTION_MODE}}', $(if ($UseExecutionOrder) { 'Dependency Order' } else { 'Alphabetical' }) `
    -replace '{{TOTAL_TESTS}}', $totalTests `
    -replace '{{PASSED}}', $passed `
    -replace '{{SUCCESS_RATE}}', $successRate `
    -replace '{{FAILED}}', $failed `
    -replace '{{FAIL_RATE}}', $failRate `
    -replace '{{SCHEMA_BROKEN}}', $schemaBroken `
    -replace '{{SCHEMA_RATE}}', $schemaRate `
    -replace '{{RUNTIME_ERRORS}}', $runtimeErrors `
    -replace '{{RUNTIME_RATE}}', $runtimeRate `
    -replace '{{CONSTRAINTS}}', $constraints `
    -replace '{{CONSTRAINT_RATE}}', $constraintRate `
    -replace '{{LOGIC_CAUGHT}}', $logicCaught `
    -replace '{{LOGIC_RATE}}', $logicRate `
    -replace '{{RESULTS_BY_CATEGORY}}', $resultsByCategory `
    -replace '{{SCHEMA_BROKEN_TABLE}}', $schemaBrokenTable `
    -replace '{{PARAM_MISMATCHES_TABLE}}', $paramMismatchesTable `
    -replace '{{CONSTRAINT_VIOLATIONS_TABLE}}', $constraintViolationsTable `
    -replace '{{VALIDATION_ERRORS_TABLE}}', $validationErrorsTable `
    -replace '{{OTHER_ERRORS_TABLE}}', $otherErrorsTable `
    -replace '{{RECOMMENDATIONS}}', ($recommendations -join "`n") `
    -replace '{{MOCK_DATA_STATUS}}', $(if (Test-Path $mockDataPath) { 'Loaded' } else { 'Not found' }) `
    -replace '{{EXEC_ORDER_STATUS}}', $(if (Test-Path $executionOrderPath) { 'Loaded' } else { 'Not found' }) `
    -replace '{{USING_EXEC_ORDER}}', $(if ($UseExecutionOrder) { 'Yes' } else { 'No' })

$content | Out-File -FilePath $reportFile -Encoding UTF8

Write-Host "`nGenerated test report: $reportFile" -ForegroundColor Green
Write-Host "  - Total Tests: $totalTests" -ForegroundColor Cyan
Write-Host "  - Passed: $passed ($successRate%)" -ForegroundColor $(if ($successRate -ge 80) { 'Green' } elseif ($successRate -ge 50) { 'Yellow' } else { 'Red' })
Write-Host "  - Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { 'Green' } else { 'Red' })
