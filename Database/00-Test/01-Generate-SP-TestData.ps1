# Script: 01-Generate-SP-TestData.ps1
# Description: Analyzes stored procedures and generates mock data configuration and execution order
# Output: 01-mock-data.json and 01-order.json

param (
    [string]$Server = "localhost",
    [int]$Port = 3306,
    [string]$Database = "mtm_receiving_application",
    [string]$User = "root",
    [securestring]$Password
)

# PowerShell runtime guard
if ($PSVersionTable.PSEdition -eq "Desktop") {
    $pwshCmd = Get-Command pwsh -ErrorAction SilentlyContinue
    if ($pwshCmd) {
        Write-Warning "Windows PowerShell 5.1 detected. Relaunching under PowerShell 7..."
        $scriptPath = $MyInvocation.MyCommand.Path
        $portArg = if ($Port -ne 3306) { "-Port $Port" } else { "" }
        & $pwshCmd.Source -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Server $Server $portArg -Database $Database -User $User
        return
    }
    Write-Error "PowerShell 7+ (pwsh) is required. Install PowerShell 7 and run: pwsh -File $($MyInvocation.MyCommand.Path)"
    return
}

# Password handling
if (-not $Password) {
    Write-Host "Enter MySQL password (default 'root' for MAMP):" -ForegroundColor Yellow
    $securePassword = Read-Host -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    $PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)

    if ([string]::IsNullOrWhiteSpace($PlainPassword)) {
        $PlainPassword = "root"
        Write-Host "Using default password 'root'" -ForegroundColor Cyan
    }
} else {
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
    $PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
}

# Load MySQL Assembly
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$databaseDir = Split-Path -Parent $scriptDir
$projectRoot = Split-Path -Parent $databaseDir
$binPath = Join-Path $projectRoot "bin"

$mysqlDll = Get-ChildItem -Path $binPath -Filter "MySql.Data.dll" -Recurse -ErrorAction SilentlyContinue |
Where-Object { $_.FullName -match "\\bin\\.*\\net8\.0" } |
Select-Object -First 1

if (-not $mysqlDll) {
    Write-Error "MySql.Data.dll not found. Please build the project first (dotnet build)."
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
        try { $null = [System.Reflection.Assembly]::LoadFrom($dll.FullName) }
        catch { }
    }
    Write-Host "Loaded MySql.Data.dll" -ForegroundColor Green
} catch {
    Write-Error "Failed to load MySql.Data.dll: $_"
    exit 1
}

# Connect to database
$connStr = "Server=$Server;Port=$Port;Database=$Database;Uid=$User;Pwd=$PlainPassword;SslMode=none;"

try {
    $conn = New-Object MySql.Data.MySqlClient.MySqlConnection($connStr)
    $conn.Open()
    Write-Host "Connected to $Database on $Server`:$Port" -ForegroundColor Green
} catch {
    Write-Error "Failed to connect: $_"
    exit 1
}

# Get all stored procedures
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
    SELECT ROUTINE_NAME
    FROM information_schema.ROUTINES
    WHERE ROUTINE_SCHEMA = '$Database'
    AND ROUTINE_TYPE = 'PROCEDURE'
    ORDER BY ROUTINE_NAME
"@
$reader = $cmd.ExecuteReader()
$spList = @()
while ($reader.Read()) {
    $spList += $reader["ROUTINE_NAME"].ToString()
}
$reader.Close()

Write-Host "Analyzing $($spList.Count) stored procedures..." -ForegroundColor Cyan

# Analyze each SP
$spDetails = @{}

foreach ($sp in $spList) {
    # Get parameters
    $cmdParams = $conn.CreateCommand()
    $cmdParams.CommandText = @"
        SELECT PARAMETER_NAME, DATA_TYPE, PARAMETER_MODE, DTD_IDENTIFIER
        FROM information_schema.PARAMETERS
        WHERE SPECIFIC_SCHEMA = '$Database'
        AND SPECIFIC_NAME = '$sp'
        ORDER BY ORDINAL_POSITION
"@
    $reader = $cmdParams.ExecuteReader()

    $params = @()
    while ($reader.Read()) {
        $paramName = if ($reader["PARAMETER_NAME"] -is [DBNull]) { $null } else { $reader["PARAMETER_NAME"].ToString() }
        $paramMode = if ($reader["PARAMETER_MODE"] -is [DBNull]) { "IN" } else { $reader["PARAMETER_MODE"].ToString() }

        # Include ALL parameters for accurate tracking (IN, OUT, INOUT)
        if ($paramName) {
            $params += @{
                name = $paramName
                type = $reader["DATA_TYPE"].ToString()
                mode = $paramMode
                dtd  = if ($reader["DTD_IDENTIFIER"] -is [DBNull]) { $null } else { $reader["DTD_IDENTIFIER"].ToString() }
            }
        }
    }
    $reader.Close()

    $spDetails[$sp] = @{
        parameters     = $params
        category       = ""
        dependencies   = @()
        executionOrder = 999
    }
}

# Categorize SPs and determine dependencies
$tableCreationOrder = @(
    # Core/lookup tables first
    "users", "departments", "dunnage_types", "dunnage_types", "dunnage_specs", "volvo_masterdata"
    # Then reference data
    "routing_recipients", "routing_po_alternatives", "dunnage_parts"
    # Then transactional tables
    "receiving_history", "receiving_packages", "receiving_lines", "dunnage_history", "dunnage_requires_inventory"
    "routing_label_data", "routing_history", "volvo_label_data", "volvo_line_data", "volvo_part_components"
    # Finally settings/preferences
    "user_preferences", "settings_universal", "reporting_scheduled_reports"
)

foreach ($sp in $spDetails.Keys) {
    $category = "Other"
    $order = 999

    # Categorize by SP name patterns
    if ($sp -match "^(sp_)?user") {
        $category = "Users"
        $order = 10
    } elseif ($sp -match "department") {
        $category = "Departments"
        $order = 15
    } elseif ($sp -match "package_type|PackageType") {
        $category = "PackageTypes"
        $order = 20
    } elseif ($sp -match "dunnage_types") {
        $category = "DunnageTypes"
        $order = 25
    } elseif ($sp -match "dunnage_specs") {
        $category = "DunnageSpecs"
        $order = 30
    } elseif ($sp -match "dunnage_parts") {
        $category = "DunnageParts"
        $order = 35
    } elseif ($sp -match "volvo_part_master") {
        $category = "VolvoParts"
        $order = 40
    } elseif ($sp -match "routing_recipient") {
        $category = "RoutingRecipients"
        $order = 45
    } elseif ($sp -match "routing_other_reason") {
        $category = "RoutingReasons"
        $order = 50
    } elseif ($sp -match "receiving.*load|ReceivingLoad") {
        $category = "ReceivingLoads"
        $order = 100
    } elseif ($sp -match "receiving.*package") {
        $category = "ReceivingPackages"
        $order = 110
    } elseif ($sp -match "receiving.*line|receiving_line") {
        $category = "ReceivingLines"
        $order = 120
    } elseif ($sp -match "dunnage_history") {
        $category = "DunnageLoads"
        $order = 130
    } elseif ($sp -match "dunnage_requires_inventory") {
        $category = "InventoriedDunnage"
        $order = 140
    } elseif ($sp -match "routing_label" -and $sp -notmatch "history") {
        $category = "RoutingLabels"
        $order = 150
    } elseif ($sp -match "routing_history") {
        $category = "RoutingHistory"
        $order = 160
    } elseif ($sp -match "volvo_shipment" -and $sp -notmatch "line") {
        $category = "VolvoShipments"
        $order = 170
    } elseif ($sp -match "volvo_shipment_line") {
        $category = "VolvoShipmentLines"
        $order = 180
    } elseif ($sp -match "volvo_part_component") {
        $category = "VolvoComponents"
        $order = 190
    } elseif ($sp -match "preference") {
        $category = "Preferences"
        $order = 200
    } elseif ($sp -match "settings|Settings") {
        $category = "Settings"
        $order = 210
    } elseif ($sp -match "report") {
        $category = "Reports"
        $order = 220
    }

    # SELECT operations can run anytime
    if ($sp -match "get|Get|select|Select|find|Find|search|Search|count|Count|check|Check") {
        $order += 1000  # Push reads to later (they don't affect order)
    }

    # DELETE operations should be last
    if ($sp -match "delete|Delete|remove|Remove") {
        $order += 2000
    }

    $spDetails[$sp].category = $category
    $spDetails[$sp].executionOrder = $order
}

# Generate mock data based on parameter types
function Get-MockValue {
    param(
        [string]$type,
        [string]$paramName,
        [string]$spName,
        [string]$dtd
    )

    # Check if this is an ENUM type - extract first enum value from DTD
    if ($type -eq "enum" -and $dtd) {
        # DTD format: enum('value1','value2','value3')
        if ($dtd -match "enum\('([^']+)'") {
            $firstEnumValue = $Matches[1]
            Write-Verbose "ENUM detected for $paramName in $spName : using '$firstEnumValue'"
            return $firstEnumValue
        }
    }

    # Smart defaults based on parameter names
    if ($paramName -match "user.*id|p_user_id") { return 1 }
    if ($paramName -match "windows.*username|p_windows_username") { return "TEST_USER" }
    if ($paramName -match "display.*name|p_display_name") { return "Test User" }
    if ($paramName -match "pin|p_pin") { return "1234" }
    if ($paramName -match "email") { return "test@example.com" }
    if ($paramName -match "department") { return "Production" }
    if ($paramName -match "shift|p_shift") { return "1st Shift" }  # Default shift value
    if ($paramName -match "match.*type|p_match_type") { return "Part Number" }  # Default routing match type
    if ($paramName -match "package.*type|p_package_type") { return 1 }
    if ($paramName -match "dunnage.*type|type_id") { return 1 }
    if ($paramName -match "part.*number|p_part_number") { return "TEST_PART" }
    if ($paramName -match "part.*id|p_part_id") {
        # Some schemas use part_id as VARCHAR, others use INT; align with the parameter type.
        if ($type -match "char|varchar|text|longtext") { return "TEST_PART" }
        return 1
    }
    if ($paramName -match "po.*number|p_po_number") {
        # Many procedures define PO number as INT; returning a string like "PO12345" causes SQL errors.
        if ($type -match "int|integer|bigint|smallint|tinyint|decimal|numeric") { return 12345 }
        return "PO12345"
    }
    if ($paramName -match "quantity|qty") { return 10 }
    if ($paramName -match "active|is_active|p_active") { return $true }
    if ($paramName -match "deleted|is_deleted") { return $false }
    if ($paramName -match "recipient.*name|p_recipient_name") { return "John Doe" }
    if ($paramName -match "label.*number|p_label_number") { return 1000 }

    # Type-based defaults
    switch -Regex ($type) {
        'int|integer|bigint|smallint|tinyint' {
            if ($dtd -match 'unsigned') { return 1 }
            return 1
        }
        'decimal|numeric|float|double' { return 1.0 }
        'bool|bit' { return $true }
        'date' { return "2026-01-01" }
        'datetime|timestamp' { return "2026-01-01 12:00:00" }
        'char|varchar|text|longtext' {
            if ($paramName -match "description|notes|comment") { return "Test data" }
            return "TEST_VALUE"
        }
        default { return $null }
    }
}

# Build mock data structure
$mockData = @{}

foreach ($sp in $spDetails.Keys) {
    $params = $spDetails[$sp].parameters
    $mockParams = @{}

    # Only generate mock values for IN and INOUT parameters (OUT parameters don't need input values)
    foreach ($param in $params) {
        if ($param.mode -eq "IN" -or $param.mode -eq "INOUT") {
            $mockValue = Get-MockValue -type $param.type -paramName $param.name -spName $sp -dtd $param.dtd
            $mockParams[$param.name] = $mockValue
        }
    }

    $mockData[$sp] = @{
        parameters     = $mockParams
        category       = $spDetails[$sp].category
        executionOrder = $spDetails[$sp].executionOrder
        enabled        = $true
    }
}

# Create execution order groups
$executionOrder = @()
$groupedSPs = $mockData.GetEnumerator() | Group-Object { $_.Value.executionOrder } | Sort-Object Name

foreach ($group in $groupedSPs) {
    $sps = $group.Group | Sort-Object Name | ForEach-Object { $_.Key }
    $category = $group.Group[0].Value.category

    $executionOrder += @{
        order            = [int]$group.Name
        category         = $category
        storedProcedures = $sps
    }
}

# Save JSON files
$mockDataJson = $mockData | ConvertTo-Json -Depth 5
$executionOrderJson = @{
    description     = "Stored Procedure execution order for testing (avoids FK constraint failures)"
    generatedDate   = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    executionGroups = $executionOrder
} | ConvertTo-Json -Depth 5

$mockDataPath = Join-Path $scriptDir "01-mock-data.json"
$executionOrderPath = Join-Path $scriptDir "01-order.json"

$mockDataJson | Out-File -FilePath $mockDataPath -Encoding UTF8
$executionOrderJson | Out-File -FilePath $executionOrderPath -Encoding UTF8

$conn.Close()

Write-Host "`nGenerated mock data configuration: $mockDataPath" -ForegroundColor Green
Write-Host "Generated execution order: $executionOrderPath" -ForegroundColor Green

# Generate Analysis Report using template
$reportFile = Join-Path $scriptDir "01-report.md"
$templatePath = Join-Path $scriptDir "templates\01-report-template.md"

# Calculate parameter statistics
$totalParams = 0
$inParams = 0
$outParams = 0
$inoutParams = 0
$withoutParams = 0

foreach ($sp in $spDetails.Keys) {
    $params = $spDetails[$sp].parameters
    if ($params.Count -eq 0) {
        $withoutParams++
    } else {
        $totalParams += $params.Count
        foreach ($p in $params) {
            switch ($p.mode) {
                "IN" { $inParams++ }
                "OUT" { $outParams++ }
                "INOUT" { $inoutParams++ }
            }
        }
    }
}

# Build category breakdown section
$categoryBreakdown = @()
$categoryGroups = $mockData.GetEnumerator() | Group-Object { $_.Value.category } | Sort-Object Name

foreach ($catGroup in $categoryGroups) {
    $categoryBreakdown += "### $($catGroup.Name) ($($catGroup.Count) procedures)"
    $categoryBreakdown += ""

    $spsInCategory = $catGroup.Group | Sort-Object { $_.Value.executionOrder }, Name
    foreach ($spEntry in $spsInCategory) {
        $sp = $spEntry.Key
        $details = $spDetails[$sp]
        $inCount = ($details.parameters | Where-Object { $_.mode -eq "IN" }).Count
        $outCount = ($details.parameters | Where-Object { $_.mode -eq "OUT" }).Count
        $inoutCount = ($details.parameters | Where-Object { $_.mode -eq "INOUT" }).Count

        $paramInfo = ""
        if ($inCount -gt 0) { $paramInfo += "IN: $inCount " }
        if ($outCount -gt 0) { $paramInfo += "OUT: $outCount " }
        if ($inoutCount -gt 0) { $paramInfo += "INOUT: $inoutCount " }
        if ($paramInfo -eq "") { $paramInfo = "No parameters" }

        $categoryBreakdown += "- **$sp** - $($paramInfo.Trim())"
    }
    $categoryBreakdown += ""
}

# Build execution order table
$executionOrderTable = @()
foreach ($group in $executionOrder | Sort-Object order) {
    $spNames = $group.storedProcedures -join ", "
    if ($spNames.Length -gt 100) {
        $spNames = $spNames.Substring(0, 97) + "..."
    }
    $executionOrderTable += "| $($group.order) | $($group.category) | $($group.storedProcedures.Count) | $spNames |"
}

# Build complex SPs section (if any)
$complexSPsSection = ""
$complexSPs = $spDetails.GetEnumerator() |
Where-Object { $_.Value.parameters.Count -ge 8 } |
Sort-Object { $_.Value.parameters.Count } -Descending

if ($complexSPs) {
    $complexSPsLines = @()
    $complexSPsLines += "## Complex Stored Procedures (8+ parameters)"
    $complexSPsLines += ""
    $complexSPsLines += "| SP Name | Total Params | IN | OUT | INOUT |"
    $complexSPsLines += "|---------|--------------|-----|-----|-------|"

    foreach ($sp in $complexSPs) {
        $params = $sp.Value.parameters
        $inCount = ($params | Where-Object { $_.mode -eq "IN" }).Count
        $outCount = ($params | Where-Object { $_.mode -eq "OUT" }).Count
        $inoutCount = ($params | Where-Object { $_.mode -eq "INOUT" }).Count

        $complexSPsLines += "| $($sp.Key) | $($params.Count) | $inCount | $outCount | $inoutCount |"
    }
    $complexSPsSection = $complexSPsLines -join "`n"
}

# Build Fix Checklists
# 1. SP Fix Checklist - SPs with OUT parameters that need proper handling
$spFixChecklistLines = @()
$daoIssues = @{}
$projectRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)

foreach ($sp in ($spDetails.GetEnumerator() | Sort-Object Key)) {
    $params = $sp.Value.parameters
    $outCount = ($params | Where-Object { $_.mode -eq "OUT" }).Count
    $inCount = ($params | Where-Object { $_.mode -eq "IN" }).Count
    $inoutCount = ($params | Where-Object { $_.mode -eq "INOUT" }).Count

    if ($outCount -gt 0 -or $inoutCount -gt 0) {
        $spName = $sp.Key
        $currentParams = $inCount
        $expectedParams = $params.Count
        $missingParams = $outCount + $inoutCount

        # Determine SP file path
        $spFile = "Database/StoredProcedures/$spName.sql"

        # Map SP to DAO class based on naming patterns
        $daoClass = ""
        $priority = "Medium"

        if ($spName -match "^sp_(.+?)_(insert|update|delete|get|check|count|find|search)") {
            $entity = $matches[1]
            $daoClass = "Dao_" + ($entity -split '_' | ForEach-Object { (Get-Culture).TextInfo.ToTitleCase($_) }) -join ''
        } elseif ($spName -match "^(.+?)_(Insert|Update|Delete|Get)") {
            $entity = $matches[1]
            $daoClass = "Dao_" + ($entity -split '_' | ForEach-Object { (Get-Culture).TextInfo.ToTitleCase($_) }) -join ''
        }

        if ($outCount -gt 2) { $priority = "High" }
        elseif ($outCount -eq 0 -and $inoutCount -gt 0) { $priority = "High" }

        $issueType = if ($outCount -gt 0) { "OUT Parameters" } else { "INOUT Parameters" }
        $fixNotes = "Update DAO to handle $outCount OUT params"
        if ($inoutCount -gt 0) { $fixNotes += " and $inoutCount INOUT params" }

        $spFixChecklistLines += "| [ ] | $spName | $currentParams IN | $expectedParams total | $missingParams OUT/INOUT | $spFile | $daoClass | $issueType | $priority | $fixNotes |"

        # Track for DAO checklist
        if ($daoClass -ne "") {
            if (-not $daoIssues.ContainsKey($daoClass)) {
                $daoIssues[$daoClass] = @{
                    SPs             = @()
                    ParamMismatches = 0
                    Methods         = @()
                }
            }
            $daoIssues[$daoClass].SPs += $spName
            $daoIssues[$daoClass].ParamMismatches += $missingParams

            # Determine method name from SP name
            if ($spName -match "insert|Insert") { $daoIssues[$daoClass].Methods += "Insert*Async" }
            elseif ($spName -match "update|Update") { $daoIssues[$daoClass].Methods += "Update*Async" }
            elseif ($spName -match "delete|Delete") { $daoIssues[$daoClass].Methods += "Delete*Async" }
            elseif ($spName -match "get|Get") { $daoIssues[$daoClass].Methods += "Get*Async" }
        }
    }
}

if ($spFixChecklistLines.Count -eq 0) {
    $spFixChecklistLines += "| - | *No issues found* | - | - | - | - | - | - | - | - |"
}
$spFixChecklist = $spFixChecklistLines -join "`n"

# 2. DAO Fix Checklist
$daoFixChecklistLines = @()
foreach ($dao in ($daoIssues.GetEnumerator() | Sort-Object Key)) {
    $daoName = $dao.Key
    $relatedSPs = ($dao.Value.SPs | Select-Object -Unique) -join ", "
    if ($relatedSPs.Length -gt 60) { $relatedSPs = $relatedSPs.Substring(0, 57) + "..." }

    $paramMismatches = $dao.Value.ParamMismatches
    $methods = ($dao.Value.Methods | Select-Object -Unique) -join ", "

    # Find DAO file path
    $daoFile = ""
    $possiblePaths = @(
        "Module_Core/Data/$daoName.cs",
        "Module_Receiving/Data/$daoName.cs",
        "Module_Dunnage/Data/$daoName.cs",
        "Module_Routing/Data/$daoName.cs",
        "Module_Settings/Data/$daoName.cs",
        "Module_Volvo/Data/$daoName.cs"
    )

    foreach ($path in $possiblePaths) {
        $fullPath = Join-Path $projectRoot $path
        if (Test-Path $fullPath) {
            $daoFile = $path
            break
        }
    }

    if ($daoFile -eq "") { $daoFile = "**/$daoName.cs (search required)" }

    $issueSummary = "$paramMismatches OUT/INOUT params across $($dao.Value.SPs.Count) SP(s)"
    $priority = if ($paramMismatches -gt 5) { "High" } elseif ($paramMismatches -gt 2) { "Medium" } else { "Low" }

    $daoFixChecklistLines += "| [ ] | $daoName | $daoFile | $relatedSPs | $paramMismatches | $methods | $issueSummary | $priority |"
}

if ($daoFixChecklistLines.Count -eq 0) {
    $daoFixChecklistLines += "| - | *No issues found* | - | - | - | - | - | - |"
}
$daoFixChecklist = $daoFixChecklistLines -join "`n"

# 3. Schema Fix Checklist (requires loading 02-report.md if it exists to detect schema issues)
$schemaFixChecklistLines = @()
$testReportPath = Join-Path $scriptDir "02-report.md"

if (Test-Path $testReportPath) {
    $testReport = Get-Content $testReportPath -Raw

    # Parse schema broken section
    if ($testReport -match "(?s)## Critical Failures \(Schema Broken\).*?\n\|.*?\n\|.*?\n(.*?)\n\n") {
        $schemaIssues = $matches[1] -split "`n"

        foreach ($line in $schemaIssues) {
            if ($line -match '^\|\s*\*\*(.+?)\*\*\s*\|\s*\d+\s*\|\s*.*?"(.+?)"\s*') {
                $spName = $matches[1].Trim()
                $errorMsg = $matches[2].Trim()

                $missingItem = ""
                $tableName = ""
                $expectedColumn = ""

                if ($errorMsg -match "Unknown column '(.+?)' in 'field list'") {
                    $missingItem = $matches[1]
                    $expectedColumn = $missingItem
                    $tableName = "Unknown (query inspection needed)"
                } elseif ($errorMsg -match "Table '.*?\.(.+?)' doesn't exist") {
                    $tableName = $matches[1]
                    $missingItem = "Table: $tableName"
                }

                $schemaFile = "Database/Schemas/* (search for table)"
                $currentStatus = "Missing in DB"
                $sqlFix = if ($expectedColumn -ne "") {
                    "ALTER TABLE ADD COLUMN"
                } else {
                    "CREATE TABLE"
                }
                $priority = "Critical"
                $fixNotes = "Review SP definition and update schema or SP code"

                $schemaFixChecklistLines += "| [ ] | $spName | $missingItem | $schemaFile | $tableName | $expectedColumn | $currentStatus | $sqlFix | $priority | $fixNotes |"
            }
        }
    }
}

if ($schemaFixChecklistLines.Count -eq 0) {
    $schemaFixChecklistLines += "| - | *No schema issues detected - run 02-Test-StoredProcedures.ps1 first* | - | - | - | - | - | - | - | - |"
}
$schemaFixChecklist = $schemaFixChecklistLines -join "`n"

# Load template and replace placeholders
$template = Get-Content $templatePath -Raw
$content = $template `
    -replace '{{TIMESTAMP}}', (Get-Date -Format 'yyyy-MM-dd HH:mm:ss') `
    -replace '{{DATABASE}}', $Database `
    -replace '{{SERVER}}', "$Server`:$Port" `
    -replace '{{TOTAL_SPS}}', $spList.Count `
    -replace '{{EXECUTION_GROUPS}}', $executionOrder.Count `
    -replace '{{CATEGORY_COUNT}}', (($mockData.Values | Select-Object -ExpandProperty category -Unique | Measure-Object).Count) `
    -replace '{{TOTAL_PARAMS}}', $totalParams `
    -replace '{{IN_PARAMS}}', $inParams `
    -replace '{{OUT_PARAMS}}', $outParams `
    -replace '{{INOUT_PARAMS}}', $inoutParams `
    -replace '{{NO_PARAMS}}', $withoutParams `
    -replace '{{CATEGORY_BREAKDOWN}}', ($categoryBreakdown -join "`n") `
    -replace '{{EXECUTION_ORDER_TABLE}}', ($executionOrderTable -join "`n") `
    -replace '{{COMPLEX_SPS_SECTION}}', $complexSPsSection `
    -replace '{{SP_FIX_CHECKLIST}}', $spFixChecklist `
    -replace '{{DAO_FIX_CHECKLIST}}', $daoFixChecklist `
    -replace '{{SCHEMA_FIX_CHECKLIST}}', $schemaFixChecklist

$content | Out-File -FilePath $reportFile -Encoding UTF8

Write-Host "Generated analysis report: $reportFile" -ForegroundColor Green
Write-Host "`nTotal SPs analyzed: $($spList.Count)" -ForegroundColor Cyan
Write-Host "Execution groups created: $($executionOrder.Count)" -ForegroundColor Cyan
