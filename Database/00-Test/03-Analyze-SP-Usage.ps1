# Script: 03-Analyze-SP-Usage.ps1
# Description: Analyzes SP usage, hardcoded SQL queries, and generates detailed reports
# Output: 03-usage-report.md, 03-hardcode-report.md, and individual SP reports in sp-reports/

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
        & $pwshCmd.Source -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Server $Server -Port $Port -Database $Database -User $User
        return
    }
    Write-Error "PowerShell 7+ (pwsh) is required"
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
        Write-Host "Using default MAMP password 'root'" -ForegroundColor Cyan
    }
}
else {
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
    $PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
}

# Setup paths
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$databaseDir = Split-Path -Parent $scriptDir
$projectRoot = Split-Path -Parent $databaseDir
$binPath = Join-Path $projectRoot "bin"
$spReportsDir = Join-Path $scriptDir "sp-reports"
$templatesDir = Join-Path $scriptDir "templates"

# Load MySQL Assembly
$mysqlDll = Get-ChildItem -Path $binPath -Filter "MySql.Data.dll" -Recurse -ErrorAction SilentlyContinue |
Where-Object { $_.FullName -match "\\bin\\.*\\net8\.0" } |
Select-Object -First 1

if (-not $mysqlDll) {
    Write-Error "MySql.Data.dll not found. Please build the project first (dotnet build)."
    exit 1
}

$mysqlDir = Split-Path -Parent $mysqlDll.FullName
$onAssemblyResolve = [System.ResolveEventHandler] {
    param($senderObj, $resolveArgs)
    $assemblyName = New-Object System.Reflection.AssemblyName($resolveArgs.Name)
    $dllPath = Join-Path $mysqlDir "$($assemblyName.Name).dll"
    if (Test-Path $dllPath) { return [System.Reflection.Assembly]::LoadFrom($dllPath) }
    return $null
}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve($onAssemblyResolve)

try {
    $dependencyDlls = Get-ChildItem -Path $mysqlDir -Filter "*.dll" -File |
    Where-Object { $_.Name -match "^(MySql\.|System\.|ZstdSharp|K4os|BouncyCastle)" }
    foreach ($dll in $dependencyDlls) {
        try { $null = [System.Reflection.Assembly]::LoadFrom($dll.FullName) } catch { }
    }
    Write-Host "✓ Loaded MySql.Data.dll" -ForegroundColor Green
}
catch {
    Write-Error "Failed to load MySql.Data.dll: $_"
    exit 1
}

# Connect to database
$connStr = "Server=$Server;Port=$Port;Database=$Database;Uid=$User;Pwd=$PlainPassword;SslMode=none;"
try {
    $conn = New-Object MySql.Data.MySqlClient.MySqlConnection($connStr)
    $conn.Open()
    Write-Host "✓ Connected to $Database" -ForegroundColor Green
}
catch {
    Write-Error "Failed to connect: $_"
    exit 1
}

# Get all stored procedures with parameters
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
    SELECT
        r.ROUTINE_NAME,
        r.ROUTINE_SCHEMA,
        GROUP_CONCAT(DISTINCT p.PARAMETER_MODE ORDER BY p.ORDINAL_POSITION) as PARAM_MODES
    FROM information_schema.ROUTINES r
    LEFT JOIN information_schema.PARAMETERS p
        ON r.SPECIFIC_NAME = p.SPECIFIC_NAME
        AND r.ROUTINE_SCHEMA = p.SPECIFIC_SCHEMA
    WHERE r.ROUTINE_SCHEMA = '$Database'
    AND r.ROUTINE_TYPE = 'PROCEDURE'
    GROUP BY r.ROUTINE_NAME, r.ROUTINE_SCHEMA
    ORDER BY r.ROUTINE_NAME
"@
$reader = $cmd.ExecuteReader()
$spList = @()
$spParameterModes = @{}
while ($reader.Read()) {
    $spName = $reader["ROUTINE_NAME"].ToString()
    $spList += $spName
    $modes = if ($reader["PARAM_MODES"] -is [DBNull]) { $null } else { $reader["PARAM_MODES"].ToString() }
    $spParameterModes[$spName] = $modes
}
$reader.Close()
$conn.Close()

Write-Host "✓ Found $($spList.Count) stored procedures" -ForegroundColor Cyan

# Determine category and parameter type for each SP
function Get-SPCategory {
    param([string]$spName)

    # Map SP prefixes to categories (matching folder structure)
    if ($spName -match "^(sp_Get|sp_Create|sp_Upsert|sp_Validate|sp_Log|sp_seed|sp_update.*default)") { return "Authentication" }
    if ($spName -match "^(sp_dunnage|sp_custom_fields|sp_dunnage_requires_inventory|sp_user_preferences)") { return "Dunnage" }
    if ($spName -match "^(sp_.*receiving|receiving_)") { return "Receiving" }
    if ($spName -match "^(sp_SystemSettings|sp_UserSettings|sp_.*Settings|sp_Scheduled)") { return "Settings" }
    if ($spName -match "^(sp_volvo)") { return "Volvo" }
    if ($spName -match "^(sp_PackageType|carrier_delivery|dunnage_line)") { return "Settings" }

    return "Other"
}

function Get-SPParameterType {
    param([string]$modes)

    if (-not $modes) { return "IN" } # No parameters = IN folder
    if ($modes -match "OUT" -and $modes -notmatch "IN") { return "OUT" }
    if ($modes -match "INOUT") { return "INOUT" }
    return "IN"
}

# Helper function to find class name
function Find-ClassName {
    param([array]$lines, [int]$currentLine)

    for ($i = $currentLine - 1; $i -ge 0; $i--) {
        if ($lines[$i] -match '\s+(public|private|protected|internal)?\s*(partial)?\s*class\s+(\w+)') {
            return $matches[3]
        }
    }
    return "Unknown"
}

# Helper function to find method name
function Find-MethodName {
    param([array]$lines, [int]$currentLine)

    for ($i = $currentLine - 1; $i -ge 0; $i--) {
        if ($lines[$i] -match '\s+(public|private|protected|internal)\s+.*\s+(\w+)\s*\(') {
            return $matches[2]
        }
    }
    return "Unknown"
}

# Helper function to determine database type
function Determine-DBType {
    param([string]$query)

    if ($query -match "\b(po_detail|po_master|part|site_ref|MTMFG|VISUAL)\b") {
        return "Infor Visual (SQL Server)"
    }
    elseif ($query -match "\b(SELECT|INSERT|UPDATE|DELETE)\b") {
        return "MySQL"
    }
    return "Unknown"
}

# Helper function to validate if string is an actual SQL query
function Test-ValidSqlQuery {
    param([string]$query)

    # Skip if it's a stored procedure name or call
    if ($query -match "^(CALL\s+|sp_|receiving_|dunnage_|carrier_)") { return $false }

    # Skip very short strings (likely UI labels)
    if ($query.Length -lt 15) { return $false }

    # Skip strings with format specifiers (error messages like "Records: %ld")
    if ($query -match "%[ldsf]") { return $false }

    # Must contain at least one SQL keyword in proper context
    $hasSqlStructure = $false

    # SELECT queries
    if ($query -match "\bSELECT\b.+\bFROM\b") { $hasSqlStructure = $true }

    # INSERT queries
    if ($query -match "\bINSERT\s+(INTO|IGNORE)\b") { $hasSqlStructure = $true }

    # UPDATE queries
    if ($query -match "\bUPDATE\b.+\bSET\b") { $hasSqlStructure = $true }

    # DELETE queries
    if ($query -match "\bDELETE\s+FROM\b") { $hasSqlStructure = $true }

    # If no SQL structure found, reject
    if (-not $hasSqlStructure) { return $false }

    # Skip UI labels that happen to start with SQL keywords
    if ($query -match "^(Select|Delete|Update|Insert)\s+(a|an|the|your|new|mode|entry|type|part|recipient|package)\b") { return $false }

    return $true
}

# Search for C# files
$csFiles = Get-ChildItem -Path $projectRoot -Filter "*.cs" -Recurse -ErrorAction SilentlyContinue |
Where-Object { $_.FullName -notmatch "\\obj\\|\\bin\\|\\Tests\\" }

Write-Host "✓ Searching $($csFiles.Count) C# files..." -ForegroundColor Cyan

# Analyze SP usage
$usageData = @{}
$hardcodedQueries = @()

foreach ($sp in $spList) {
    Write-Progress -Activity "Analyzing SPs" -Status $sp -PercentComplete (($spList.IndexOf($sp) / $spList.Count) * 100)
    $usages = @()

    foreach ($file in $csFiles) {
        $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
        if (-not $content) { continue }

        if ($content -match $sp) {
            $lines = Get-Content $file.FullName
            $lineNumber = 0

            foreach ($line in $lines) {
                $lineNumber++

                if ($line -match $sp) {
                    $methodName = "Unknown"
                    $className = "Unknown"

                    # Find containing method
                    for ($i = $lineNumber - 1; $i -ge 0; $i--) {
                        if ($lines[$i] -match '\s+(public|private|protected|internal)\s+.*\s+(\w+)\s*\(') {
                            $methodName = $matches[2]
                            break
                        }
                    }

                    # Find containing class
                    for ($i = $lineNumber - 1; $i -ge 0; $i--) {
                        if ($lines[$i] -match '\s+(public|private|protected|internal)?\s*(partial)?\s*class\s+(\w+)') {
                            $className = $matches[3]
                            break
                        }
                    }

                    $relativePath = $file.FullName.Replace($projectRoot, "").TrimStart('\')

                    $usages += [PSCustomObject]@{
                        File   = $relativePath
                        Class  = $className
                        Method = $methodName
                        Line   = $lineNumber
                        Code   = $line.Trim()
                    }
                }
            }
        }
    }

    $usageData[$sp] = $usages
}

Write-Progress -Activity "Analyzing SPs" -Completed

# Scan for hardcoded SQL queries
Write-Host "✓ Scanning for hardcoded SQL queries..." -ForegroundColor Cyan

$fileCount = 0
foreach ($file in $csFiles) {
    $fileCount++
    Write-Progress -Activity "Scanning for Hardcoded SQL" -Status $file.Name -PercentComplete (($fileCount / $csFiles.Count) * 100)

    $lines = Get-Content $file.FullName -ErrorAction SilentlyContinue
    if (-not $lines) { continue }

    $lineNumber = 0

    foreach ($line in $lines) {
        $lineNumber++

        # Skip comments
        if ($line -match '^\s*(//|/\*|\*)') { continue }

        # Pattern 1: CommandText assignments (regular and verbatim strings)
        if ($line -match 'CommandText\s*=\s*@?"([^"]+)"' -or $line -match 'CommandText\s*=\s*\$@?"([^"]+)"') {
            $query = $matches[1]
            if (Test-ValidSqlQuery -query $query) {
                $hardcodedQueries += (New-Object PSCustomObject -Property @{
                        File     = $file.FullName.Replace($projectRoot, "").TrimStart('\')
                        Class    = (Find-ClassName -lines $lines -currentLine $lineNumber)
                        Method   = (Find-MethodName -lines $lines -currentLine $lineNumber)
                        Line     = $lineNumber
                        Query    = $query.Substring(0, [Math]::Min(150, $query.Length))
                        DBType   = (Determine-DBType -query $query)
                        FullCode = $line.Trim()
                    })
            }
        }

        # Pattern 2: Variable assignments with SQL (including string interpolation and ternary)
        if ($line -match '(var|string)\s+\w+\s*=\s*[\$]?@?"([^"]+)"') {
            $query = $matches[2]
            if (Test-ValidSqlQuery -query $query) {
                $hardcodedQueries += (New-Object PSCustomObject -Property @{
                        File     = $file.FullName.Replace($projectRoot, "").TrimStart('\')
                        Class    = (Find-ClassName -lines $lines -currentLine $lineNumber)
                        Method   = (Find-MethodName -lines $lines -currentLine $lineNumber)
                        Line     = $lineNumber
                        Query    = $query.Substring(0, [Math]::Min(150, $query.Length))
                        DBType   = (Determine-DBType -query $query)
                        FullCode = $line.Trim()
                    })
            }
        }

        # Pattern 3: Ternary operators with SQL strings
        if ($line -match '\?\s*"([^"]+)"\s*:' -or $line -match ':\s*"([^"]+)"') {
            $query = $matches[1]
            if (Test-ValidSqlQuery -query $query) {
                $hardcodedQueries += (New-Object PSCustomObject -Property @{
                        File     = $file.FullName.Replace($projectRoot, "").TrimStart('\')
                        Class    = (Find-ClassName -lines $lines -currentLine $lineNumber)
                        Method   = (Find-MethodName -lines $lines -currentLine $lineNumber)
                        Line     = $lineNumber
                        Query    = $query.Substring(0, [Math]::Min(150, $query.Length))
                        DBType   = (Determine-DBType -query $query)
                        FullCode = $line.Trim()
                    })
            }
        }

        # Pattern 4: SqlCommand/MySqlCommand constructors
        if ($line -match 'new\s+(Sql|MySql)Command\s*\(\s*[\$]?@?"([^"]+)"') {
            $query = $matches[2]
            if (Test-ValidSqlQuery -query $query) {
                $hardcodedQueries += (New-Object PSCustomObject -Property @{
                        File     = $file.FullName.Replace($projectRoot, "").TrimStart('\')
                        Class    = (Find-ClassName -lines $lines -currentLine $lineNumber)
                        Method   = (Find-MethodName -lines $lines -currentLine $lineNumber)
                        Line     = $lineNumber
                        Query    = $query.Substring(0, [Math]::Min(150, $query.Length))
                        DBType   = (Determine-DBType -query $query)
                        FullCode = $line.Trim()
                    })
            }
        }
    }
}

Write-Progress -Activity "Scanning for Hardcoded SQL" -Completed

# Analyze method references
Write-Host "✓ Analyzing method references..." -ForegroundColor Cyan
$methodReferences = @{}

$uniqueMethods = @{}
foreach ($sp in $usageData.Keys) {
    foreach ($usage in $usageData[$sp]) {
        $key = "$($usage.Class).$($usage.Method)"
        if (-not $uniqueMethods.ContainsKey($key)) {
            $uniqueMethods[$key] = $usage
        }
    }
}

$methodCount = 0
$totalMethods = $uniqueMethods.Count

foreach ($methodEntry in $uniqueMethods.GetEnumerator()) {
    $methodCount++
    $key = $methodEntry.Key
    $usage = $methodEntry.Value

    Write-Progress -Activity "Analyzing Method References" -Status "$key" -PercentComplete (($methodCount / $totalMethods) * 100)

    if (-not $methodReferences.ContainsKey($key)) {
        $callers = @()
        $searchPattern = "$($usage.Method)\s*\("

        foreach ($file in $csFiles) {
            $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
            if (-not $content) { continue }

            $relativePath = $file.FullName.Replace($projectRoot, "").TrimStart('\')

            if ($relativePath -eq $usage.File) {
                $lines = Get-Content $file.FullName
                $lineNumber = 0
                $callCount = 0

                foreach ($line in $lines) {
                    $lineNumber++
                    if ($lineNumber -ne $usage.Line -and $line -match $searchPattern) {
                        $callCount++
                    }
                }

                if ($callCount -gt 0) {
                    $callers += "Same file ($callCount calls)"
                }
            }
            else {
                if ($content -match $searchPattern) {
                    $callers += $relativePath
                }
            }
        }

        $methodReferences[$key] = @{
            HasCallers  = $callers.Count -gt 0
            CallerCount = $callers.Count
            Callers     = $callers
        }
    }
}

Write-Progress -Activity "Analyzing Method References" -Completed

# Create sp-reports directory structure
Write-Host "✓ Creating individual SP reports..." -ForegroundColor Cyan

if (Test-Path $spReportsDir) {
    Remove-Item $spReportsDir -Recurse -Force
}
New-Item -ItemType Directory -Path $spReportsDir -Force | Out-Null

$categories = @("Authentication", "Dunnage", "Receiving", "Settings", "Volvo", "Other")
$paramTypes = @("IN", "OUT", "INOUT")

foreach ($cat in $categories) {
    foreach ($pType in $paramTypes) {
        $dirPath = Join-Path $spReportsDir "$cat\$pType"
        New-Item -ItemType Directory -Path $dirPath -Force | Out-Null
    }
}

# Generate individual SP reports
foreach ($sp in $spList) {
    $category = Get-SPCategory -spName $sp
    $paramType = Get-SPParameterType -modes $spParameterModes[$sp]
    $reportPath = Join-Path $spReportsDir "$category\$paramType\$sp.md"

    $usages = $usageData[$sp]

    if ($usages.Count -gt 0) {
        # Load template
        $template = Get-Content (Join-Path $templatesDir "sp-report-template.md") -Raw

        # Build usage rows
        $usageRows = @()
        foreach ($usage in $usages) {
            $methodKey = "$($usage.Class).$($usage.Method)"
            $refInfo = $methodReferences[$methodKey]

            $hasCallers = if ($refInfo -and $refInfo.HasCallers) { "✓" } else { "✗" }
            $callerCount = if ($refInfo) { $refInfo.CallerCount } else { 0 }

            $usageRows += "| $($usage.File) | $($usage.Class) | ``$($usage.Method)`` | $($usage.Line) | $hasCallers | $callerCount |"
        }

        # Build code samples
        $codeSamples = @()
        foreach ($usage in $usages) {
            $codeSamples += "// $($usage.File):$($usage.Line)"
            $codeSamples += $usage.Code
            $codeSamples += ""
        }

        # Build method references
        $methodRefs = @()
        $uniqueMethods = $usages | Select-Object -Property Class, Method -Unique
        foreach ($method in $uniqueMethods) {
            $methodKey = "$($method.Class).$($method.Method)"
            $refInfo = $methodReferences[$methodKey]

            $methodRefs += "### $($method.Class).$($method.Method)"
            if ($refInfo -and $refInfo.HasCallers) {
                $methodRefs += "**Called by ($($refInfo.CallerCount) references):**"
                foreach ($caller in $refInfo.Callers) {
                    $methodRefs += "- $caller"
                }
            }
            else {
                $methodRefs += "⚠️ **No references found** - May be dead code or called via reflection"
            }
            $methodRefs += ""
        }

        # Replace placeholders
        $content = $template `
            -replace '{{SP_NAME}}', $sp `
            -replace '{{CATEGORY}}', $category `
            -replace '{{PARAM_TYPES}}', $(if ($spParameterModes[$sp]) { $spParameterModes[$sp] } else { 'None' }) `
            -replace '{{TIMESTAMP}}', (Get-Date -Format 'yyyy-MM-dd HH:mm:ss') `
            -replace '{{USAGE_COUNT}}', $usages.Count `
            -replace '{{USAGE_ROWS}}', ($usageRows -join "`n") `
            -replace '{{CODE_SAMPLES}}', ($codeSamples -join "`n") `
            -replace '{{METHOD_REFERENCES}}', ($methodRefs -join "`n")

        $content | Out-File -FilePath $reportPath -Encoding UTF8
    }
    else {
        # Load unused template
        $template = Get-Content (Join-Path $templatesDir "sp-report-unused-template.md") -Raw

        # Replace placeholders
        $content = $template `
            -replace '{{SP_NAME}}', $sp `
            -replace '{{CATEGORY}}', $category `
            -replace '{{PARAM_TYPES}}', $(if ($spParameterModes[$sp]) { $spParameterModes[$sp] } else { 'None' }) `
            -replace '{{TIMESTAMP}}', (Get-Date -Format 'yyyy-MM-dd HH:mm:ss')

        $content | Out-File -FilePath $reportPath -Encoding UTF8
    }
}

# Generate 03-usage-report.md
Write-Host "✓ Generating usage report..." -ForegroundColor Cyan

$usageReport = @()
$usageReport += "# Stored Procedure Usage Analysis"
$usageReport += ""
$usageReport += "**Generated:** $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$usageReport += "**Database:** $Database"
$usageReport += "**Total Stored Procedures:** $($spList.Count)"
$usageReport += ""

$usedSPs = ($usageData.GetEnumerator() | Where-Object { $_.Value.Count -gt 0 }).Count
$unusedSPs = $spList.Count - $usedSPs
$totalUsages = ($usageData.Values | ForEach-Object { $_.Count } | Measure-Object -Sum).Sum

$usageReport += "## Summary"
$usageReport += ""
$usageReport += "| Metric | Count | Percentage |"
$usageReport += "|--------|-------|------------|"
$usageReport += "| **Used Stored Procedures** | $usedSPs | $(if ($spList.Count -gt 0) { [math]::Round(($usedSPs / $spList.Count) * 100, 1) } else { 0 })% |"
$usageReport += "| **Unused Stored Procedures** | $unusedSPs | $(if ($spList.Count -gt 0) { [math]::Round(($unusedSPs / $spList.Count) * 100, 1) } else { 0 })% |"
$usageReport += "| **Total Usage Locations** | $totalUsages | - |"
$usageReport += ""

$usageReport += "## Stored Procedures in Use"
$usageReport += ""
$usageReport += "| SP Name | File | Class | Method | Line | Has Callers | Caller Count |"
$usageReport += "|---------|------|-------|--------|------|-------------|--------------|"

$usedSPsList = $usageData.GetEnumerator() | Where-Object { $_.Value.Count -gt 0 } | Sort-Object Name

foreach ($spEntry in $usedSPsList) {
    $sp = $spEntry.Key
    $usages = $spEntry.Value

    foreach ($usage in $usages) {
        $methodKey = "$($usage.Class).$($usage.Method)"
        $refInfo = $methodReferences[$methodKey]

        $hasCallers = if ($refInfo -and $refInfo.HasCallers) { "✓ Yes" } else { "✗ No" }
        $callerCount = if ($refInfo) { $refInfo.CallerCount } else { 0 }

        $category = Get-SPCategory -spName $sp
        $paramType = Get-SPParameterType -modes $spParameterModes[$sp]
        $reportLink = "sp-reports/$category/$paramType/$sp.md"

        $usageReport += "| [``$sp``]($reportLink) | $($usage.File) | $($usage.Class) | ``$($usage.Method)`` | $($usage.Line) | $hasCallers | $callerCount |"
    }
}

$usageReport += ""
$usageReport += "## ⚠️ Unused Stored Procedures"
$usageReport += ""

$unusedList = $usageData.GetEnumerator() | Where-Object { $_.Value.Count -eq 0 } | Sort-Object Name

if ($unusedList.Count -gt 0) {
    $usageReport += "| SP Name | Category | Recommendation |"
    $usageReport += "|---------|----------|----------------|"

    foreach ($spEntry in $unusedList) {
        $sp = $spEntry.Key
        $category = Get-SPCategory -spName $sp
        $paramType = Get-SPParameterType -modes $spParameterModes[$sp]
        $reportLink = "sp-reports/$category/$paramType/$sp.md"

        $recommendation = "Review for removal"
        if ($sp -match "^sp_Get|^sp_.*_get_") { $recommendation = "Check if used externally" }
        elseif ($sp -match "^sp_.*_delete") { $recommendation = "May be utility function" }

        $usageReport += "| [``$sp``]($reportLink) | $category | $recommendation |"
    }
}
else {
    $usageReport += "✓ All stored procedures are in use!"
}

# Write to file with proper line breaks
$usageReport -join "`n" | Out-File -FilePath (Join-Path $scriptDir "03-usage-report.md") -Encoding UTF8

# Generate 03-hardcode-report.md
Write-Host "✓ Generating hardcoded SQL report..." -ForegroundColor Cyan

$hardcodeReport = @()
$hardcodeReport += "# Hardcoded SQL Query Analysis"
$hardcodeReport += ""
$hardcodeReport += "**Generated:** $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$hardcodeReport += "**Total Queries Found:** $($hardcodedQueries.Count)"
$hardcodeReport += ""

$mysqlQueries = ($hardcodedQueries | Where-Object { $_.DBType -eq "MySQL" }).Count
$visualQueries = ($hardcodedQueries | Where-Object { $_.DBType -eq "Infor Visual (SQL Server)" }).Count
$unknownQueries = ($hardcodedQueries | Where-Object { $_.DBType -eq "Unknown" }).Count

$hardcodeReport += "## Summary"
$hardcodeReport += ""
$hardcodeReport += "| Database Type | Count |"
$hardcodeReport += "|---------------|-------|"
$hardcodeReport += "| MySQL | $mysqlQueries |"
$hardcodeReport += "| Infor Visual (SQL Server) | $visualQueries |"
$hardcodeReport += "| Unknown | $unknownQueries |"
$hardcodeReport += ""

$hardcodeReport += "## Infor Visual (SQL Server) Queries"
$hardcodeReport += ""
$hardcodeReport += "These queries access the read-only Infor Visual ERP database."
$hardcodeReport += ""
$hardcodeReport += "| File | Class | Method | Line | Query Preview |"
$hardcodeReport += "|------|-------|--------|------|---------------|"

foreach ($query in ($hardcodedQueries | Where-Object { $_.DBType -eq "Infor Visual (SQL Server)" } | Sort-Object File)) {
    $preview = $query.Query.Replace("|", "\\|")
    $hardcodeReport += "| $($query.File) | $($query.Class) | ``$($query.Method)`` | $($query.Line) | ``$preview`` |"
}

$hardcodeReport += ""
$hardcodeReport += "## MySQL Queries"
$hardcodeReport += ""
$hardcodeReport += "⚠️ **These should ideally be converted to stored procedures.**"
$hardcodeReport += ""
$hardcodeReport += "| File | Class | Method | Line | Query Preview |"
$hardcodeReport += "|------|-------|--------|------|---------------|"

foreach ($query in ($hardcodedQueries | Where-Object { $_.DBType -eq "MySQL" } | Sort-Object File)) {
    $preview = $query.Query.Replace("|", "\\|")
    $hardcodeReport += "| $($query.File) | $($query.Class) | ``$($query.Method)`` | $($query.Line) | ``$preview`` |"
}

$hardcodeReport += ""
$hardcodeReport += "## Unknown/Other Queries"
$hardcodeReport += ""
$hardcodeReport += "| File | Class | Method | Line | Query Preview |"
$hardcodeReport += "|------|-------|--------|------|---------------|"

foreach ($query in ($hardcodedQueries | Where-Object { $_.DBType -eq "Unknown" } | Sort-Object File)) {
    $preview = $query.Query.Replace("|", "\\|")
    $hardcodeReport += "| $($query.File) | $($query.Class) | ``$($query.Method)`` | $($query.Line) | ``$preview`` |"
}

$hardcodeReport += ""
$hardcodeReport += "---"
$hardcodeReport += ""
$hardcodeReport += "**Recommendations:**"
$hardcodeReport += "1. Convert MySQL queries to stored procedures for better security and maintainability"
$hardcodeReport += "2. Ensure Infor Visual queries are read-only (SELECT statements only)"
$hardcodeReport += "3. Review unknown queries for proper database targeting"
$hardcodeReport += "4. Consider parameterized queries or ORM for complex data access patterns"

# Write to file with proper line breaks
$hardcodeReport -join "`n" | Out-File -FilePath (Join-Path $scriptDir "03-hardcode-report.md") -Encoding UTF8

Write-Host "`n✓ Analysis complete!" -ForegroundColor Green
Write-Host "  - Usage Report: 03-usage-report.md" -ForegroundColor Cyan
Write-Host "  - Hardcoded SQL Report: 03-hardcode-report.md" -ForegroundColor Cyan
Write-Host "  - Individual SP Reports: sp-reports/ ($($spList.Count) files)" -ForegroundColor Cyan
Write-Host "`n  Summary:" -ForegroundColor White
Write-Host "    Used SPs: $usedSPs" -ForegroundColor Green
Write-Host "    Unused SPs: $unusedSPs" -ForegroundColor $(if ($unusedSPs -eq 0) { 'Green' } else { 'Yellow' })
Write-Host "    Hardcoded Queries: $($hardcodedQueries.Count)" -ForegroundColor $(if ($hardcodedQueries.Count -eq 0) { 'Green' } else { 'Yellow' })
Write-Host "      - MySQL: $mysqlQueries" -ForegroundColor Yellow
Write-Host "      - Infor Visual: $visualQueries" -ForegroundColor Cyan
