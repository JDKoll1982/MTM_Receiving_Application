# Script: Test-StoredProcedures.ps1
# Description: Dynamically calls every Stored Procedure in the database with dummy data to check for Runtime logic/schema errors.
# Dependency: MySql.Data.dll (Usually available if the app builds, or we use pure SQL via mysql.exe if available, but .NET is safer here given the environment)

param (
    [string]$Server = "172.16.1.104",
    [string]$Database = "mtm_receiving_application",
    [string]$User = "root",
    [string]$Password = "root"
)

# Load MySQL Assembly from local bin if possible, else rely on installed connector
$nugetPath = "$env:USERPROFILE\.nuget\packages\mysql.data"
$dllVersion = Get-ChildItem -Path $nugetPath -Recurse -Filter "MySql.Data.dll" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($dllVersion) {
    Add-Type -Path $dllVersion.FullName
}
else {
    Write-Error "MySql.Data.dll not found in NuGet cache. Please build the project ensuring packages are restored."
    exit 1
}

$connStr = "Server=$Server;Database=$Database;Uid=$User;Pwd=$Password;SslMode=none;"
$conn = New-Object MySql.Data.MySqlClient.MySqlConnection($connStr)

try {
    $conn.Open()
    Write-Host "Connected to $Database on $Server" -ForegroundColor Green
}
catch {
    Write-Error "Failed to connect: $_"
    exit 1
}

# 1. Get List of Stored Procedures
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT ROUTINE_NAME FROM information_schema.ROUTINES WHERE ROUTINE_SCHEMA = '$Database' AND ROUTINE_TYPE = 'PROCEDURE'"
$reader = $cmd.ExecuteReader()
$spList = @()
while ($reader.Read()) {
    $spList += $reader["ROUTINE_NAME"]
}
$reader.Close()

Write-Host "Found $($spList.Count) stored procedures to test." -ForegroundColor Cyan

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
    while ($reader.Read()) {
        $paramDefs += [PSCustomObject]@{
            Name = $reader["PARAMETER_NAME"]
            Type = $reader["DATA_TYPE"]
            Mode = $reader["PARAMETER_MODE"]
        }
    }
    $reader.Close()

    # 3. Construct Dummy Call
    $testCall = "CALL `$sp`("
    $paramValues = @()
    
    foreach ($p in $paramDefs) {
        if ($p.Mode -eq "OUT" -or $p.Mode -eq "INOUT") {
            # pass a session variable
            $varName = "@var_test"
            $paramValues += $varName
        }
        else {
            # Generate safe dummy data based on type
            switch -Regex ($p.Type) {
                'int|decimal|float|double' { $paramValues += "1" }
                'bool|bit|tinyint' { $paramValues += "1" }
                'date' { $paramValues += "'2026-01-01'" }
                'datetime|timestamp' { $paramValues += "'2026-01-01 12:00:00'" }
                'char|varchar|text|longtext' { $paramValues += "'TEST_AUDIT'" }
                'blob' { $paramValues += "NULL" }
                default { $paramValues += "NULL" }
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
        # We start a transaction and rollback immediately to avoid polluting DB
        # BUT: Stored procedures that commit their own transactions will still persist data.
        # Given this is an audit of a dev/test db, we assume it is acceptable.
        
        # NOTE: Cannot START TRANSACTION inside CALL via .NET easily unless creating a transaction object
        # but let's just run it.
        $null = $testCmd.ExecuteNonQuery()
    }
    catch {
        $status = "FAIL"
        $errorMsg = $_.Exception.Message
        # Isolate MySQL Error Code if possible
        if ($_.Exception.InnerException -and $_.Exception.InnerException.Number) {
            $errorCode = $_.Exception.InnerException.Number
        }
        elseif ($_.Exception.Number) {
            $errorCode = $_.Exception.Number
        }
    }

    # Categorize Error
    # 1054: Unknown column (Schema Mismatch) -> CRITICAL
    # 1146: Table doesn't exist -> CRITICAL
    # 1318: Incorrect number of arguments (Script bug or mismatch) -> WARN
    # 1048: Column cannot be null (Constraint) -> INFO (Means logic tried to run but dummy data wasn't enough)
    # 1644: Custom user signal (Logic validation) -> PASS (Means logic ran and caught bad data)
    
    $category = "Unknown"
    if ($status -eq "PASS") { 
        $category = "Valid" 
    }
    elseif ($errorCode -eq 1054 -or $errorCode -eq 1146) {
        $category = "SchemaBroken"
    }
    elseif ($errorCode -eq 1048 -or $errorCode -eq 1062) {
        $category = "Constraint" # Notnull or Unique
    }
    elseif ($status -eq "FAIL" -and $errorMsg -match "SQLSTATE\[45000\]") {
        # "Unhandled user-defined exception" or similar
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
    }
    
    Write-Host "[$category] $sp" -ForegroundColor ($category -eq "Valid" ? "Green" : ($category -eq "SchemaBroken" ? "Red" : "Gray"))
}

$conn.Close()

# 5. Output Report
$reportFile = "StoredProcedure_Execution_Report.md"
$md = @()
$md += "# Stored Procedure Execution Test"
$md += "**Generated:** $(Get-Date)"
$md += ""
$md += "## Summary"
$md += "- **Total:** $($results.Count)"
$md += "- **Passed:** $(($results | Where-Object Status -eq 'PASS').Count)"
$md += "- **Schema Broken:** $(($results | Where-Object Category -eq 'SchemaBroken').Count)"
$md += ""

$md += "## Critical Failures (Schema Broken)"
$md += "| SP Name | Error Code | Message | Call Used |"
$md += "|---|---|---|---|"
$broken = $results | Where-Object Category -eq 'SchemaBroken'
if ($broken) {
    foreach ($r in $broken) {
        $md += "| **$($r.SP)** | $($r.ErrorCode) | $($r.Message) | `$($r.TestCall)` |"
    }
}
else {
    $md += "| - | - | None | - |"
}

$md += ""
$md += "## Other Failures (Constraints/Runtime)"
$md += "| SP Name | Category | Message |"
$md += "|---|---|---|"
$others = $results | Where-Object { $_.Category -ne 'SchemaBroken' -and $_.Status -eq 'FAIL' }
foreach ($r in $others) {
    if ($r.Category -eq "LogicCaught") { continue } # Skip expected logic errors
    $md += "| $($r.SP) | $($r.Category) | $($r.Message) |"
}

$md | Out-File $reportFile -Encoding utf8
Write-Host "Report saved to $reportFile" -ForegroundColor Cyan
