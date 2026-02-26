# ============================================================================
# Import-ReceivingHistory.ps1
# Purpose: Import receiving history from a CSV file into MAMP MySQL
# Source:  Google Sheets "Receiving Data - History" export (CSV format)
# CSV Columns: Quantity, Material ID, PO Number, Employee, Heat, Date, Initial Location
#
# Usage:
#   .\Import-ReceivingHistory.ps1 -WhatIf          # dry run
#   .\Import-ReceivingHistory.ps1                  # live import to localhost MAMP
#   .\Import-ReceivingHistory.ps1 -Server 10.0.0.1 # remote server
# ============================================================================

param(
    [string]$Server = "localhost",
    [string]$Port = "3306",
    [string]$Database = "mtm_receiving_application",
    [string]$User = "root",
    [string]$Password = "root",
    [string]$CsvPath = "$PSScriptRoot\..\..\docs\GoogleSheetsVersion\Receiving Data - History 2025.csv",
    [switch]$WhatIf,
    [switch]$SkipErrors
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# Known embedded-header keywords from Google Sheets CSV exports.
# Rows where the Employee column matches any of these are silently discarded
# (they are repeated header rows, not data).
# ---------------------------------------------------------------------------
$KnownHeaderValues = @('employee', 'employee number', 'emp', 'emp #', 'emp#', 'label #', 'label#', '#')

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

function Write-Status {
    param([string]$Message, [string]$Color = "Cyan")
    Write-Host "  $Message" -ForegroundColor $Color
}

function Write-Section {
    param([string]$Title)
    Write-Host ""
    Write-Host "-----------------------------------------------------" -ForegroundColor DarkGray
    Write-Host "  $Title" -ForegroundColor White
    Write-Host "-----------------------------------------------------" -ForegroundColor DarkGray
}

function Convert-CsvDateToMySql {
    param([string]$DateStr)
    if ([string]::IsNullOrWhiteSpace($DateStr)) { return $null }
    try {
        $parsed = [datetime]::ParseExact($DateStr.Trim(), "MM/dd/yyyy", $null)
        return $parsed.ToString("yyyy-MM-dd")
    }
    catch {
        try {
            $parsed = [datetime]::Parse($DateStr.Trim())
            return $parsed.ToString("yyyy-MM-dd")
        }
        catch { return $null }
    }
}

# Escape a value for MySQL single-quoted string literals.
# Returns the literal SQL token: NULL, or 'escaped string', or integer.
function Format-MySqlValue {
    param($Value)
    if ($null -eq $Value) { return 'NULL' }
    $s = $Value.ToString()
    # Escape backslash first, then single-quote
    $s = $s.Replace('\', '\\').Replace("'", "\'")
    return "'$s'"
}

# Find MAMP's mysql.exe. Checks common MAMP installation paths and then PATH.
function Find-MampMySql {
    $candidates = @(
        "C:\MAMP\bin\mysql\bin\mysql.exe",
        "C:\MAMP\bin\mysql8.0.31\bin\mysql.exe",
        "C:\MAMP\bin\mysql5.7.39\bin\mysql.exe",
        "C:\MAMP\bin\mysql5.7.24\bin\mysql.exe",
        "C:\MAMP\bin\mysql5.6.44\bin\mysql.exe"
    )

    # Also scan for any versioned mysql folder under C:\MAMP\bin\
    $dynamic = Get-Item "C:\MAMP\bin\mysql*\bin\mysql.exe" -ErrorAction SilentlyContinue |
    Sort-Object FullName -Descending |
    Select-Object -First 1

    if ($dynamic) { return $dynamic.FullName }

    foreach ($path in $candidates) {
        if (Test-Path $path) { return $path }
    }

    # Fallback: check PATH
    $inPath = Get-Command mysql -ErrorAction SilentlyContinue
    if ($inPath) { return $inPath.Source }

    return $null
}

# ---------------------------------------------------------------------------
# Validate prerequisites
# ---------------------------------------------------------------------------

Write-Section "MTM Receiving History Import"

$resolvedCsvPath = Resolve-Path $CsvPath -ErrorAction SilentlyContinue
if (-not $resolvedCsvPath) {
    Write-Host "  ERROR: CSV file not found at: $CsvPath" -ForegroundColor Red
    Write-Host "  Use -CsvPath to specify a different location." -ForegroundColor Yellow
    exit 1
}

Write-Status "CSV file  : $($resolvedCsvPath.Path)"
Write-Status "Server    : $Server`:$Port"
Write-Status "Database  : $Database"
if ($WhatIf) {
    Write-Status "Mode      : DRY RUN (use -WhatIf:\$false to perform actual import)" "Yellow"
}
else {
    Write-Status "Mode      : LIVE IMPORT"
}

# ---------------------------------------------------------------------------
# Parse CSV
# TextFieldParser handles quoted fields that contain commas or newlines.
# ---------------------------------------------------------------------------

Write-Section "Parsing CSV"

Add-Type -AssemblyName "Microsoft.VisualBasic"

$rawCsv = Get-Content -Path $resolvedCsvPath.Path -Encoding UTF8 -Raw
$records = [System.Collections.Generic.List[PSCustomObject]]::new()
$rowCount = 0
$headerSkipped = 0

$reader = New-Object System.IO.StringReader($rawCsv)
$csvReader = New-Object Microsoft.VisualBasic.FileIO.TextFieldParser($reader)
$csvReader.TextFieldType = [Microsoft.VisualBasic.FileIO.FieldType]::Delimited
$csvReader.SetDelimiters(",")
$csvReader.HasFieldsEnclosedInQuotes = $true
$csvReader.TrimWhiteSpace = $false

$firstRow = $true

try {
    while (-not $csvReader.EndOfData) {
        $fields = $csvReader.ReadFields()

        # Skip the very first row (column headers)
        if ($firstRow) {
            $firstRow = $false
            continue
        }

        if ($null -eq $fields -or $fields.Count -lt 6) { continue }

        $qty = $fields[0].Trim()
        $matID = $fields[1].Trim()
        $po = $fields[2].Trim()
        $emp = $fields[3].Trim()
        $heat = $fields[4].Trim()
        $date = $fields[5].Trim()
        $loc = if ($fields.Count -ge 7) { $fields[6].Trim() } else { '' }

        # Silently discard fully-blank separator rows
        if ([string]::IsNullOrWhiteSpace($qty) -and [string]::IsNullOrWhiteSpace($matID) -and
            [string]::IsNullOrWhiteSpace($date)) {
            $headerSkipped++
            continue
        }

        # Silently discard repeated Google Sheets header rows (Employee column = known keyword)
        if ($KnownHeaderValues -contains $emp.ToLower()) {
            $headerSkipped++
            continue
        }

        $records.Add([PSCustomObject]@{
                Quantity        = $qty
                MaterialID      = $matID
                PONumber        = $po
                Employee        = $emp
                Heat            = $heat
                Date            = $date
                InitialLocation = $loc
            })
        $rowCount++
    }
}
finally {
    $csvReader.Dispose()
    $reader.Dispose()
}

Write-Status "Parsed $rowCount data rows from CSV (silently skipped $headerSkipped blank/header rows)."

# ---------------------------------------------------------------------------
# Validate and transform rows
# ---------------------------------------------------------------------------

Write-Section "Validating Rows"

$validRows = [System.Collections.Generic.List[PSCustomObject]]::new()
$skippedRows = [System.Collections.Generic.List[PSCustomObject]]::new()

foreach ($row in $records) {
    $errors = @()

    # Quantity must be a positive integer
    $qty = 0
    if (-not [int]::TryParse($row.Quantity, [ref]$qty) -or $qty -le 0) {
        $errors += "Invalid quantity: '$($row.Quantity)'"
    }

    # MaterialID must not be blank
    if ([string]::IsNullOrWhiteSpace($row.MaterialID)) {
        $errors += "Blank Material ID"
    }

    # Employee must be numeric
    $empNum = 0
    if (-not [int]::TryParse($row.Employee, [ref]$empNum)) {
        $errors += "Non-numeric employee: '$($row.Employee)'"
    }

    # Date must parse
    $mySqlDate = Convert-CsvDateToMySql $row.Date
    if (-not $mySqlDate) {
        $errors += "Unparseable date: '$($row.Date)'"
    }

    if ($errors.Count -gt 0) {
        $skippedRows.Add([PSCustomObject]@{
                Row    = $row
                Reason = $errors -join "; "
            })
        continue
    }

    # Normalise heat: collapse newlines, truncate to 100 chars
    $heat = ($row.Heat -replace "`r`n|`n|`r", " / ").Trim()
    if ($heat -eq "NONE" -or $heat -eq "") { $heat = $null }
    if ($heat -and $heat.Length -gt 100) { $heat = $heat.Substring(0, 100) }

    # Normalise PartID: trim whitespace, truncate to 50 chars (VARCHAR(50) column limit).
    # Values longer than 50 are descriptions/notes that slipped into the spreadsheet;
    # truncate so the import completes rather than aborting the entire batch.
    $partID = $row.MaterialID.Trim()
    if ($partID.Length -gt 50) { $partID = $partID.Substring(0, 50) }

    # Normalise PO number: keep as-is (VARCHAR column supports 'PO-XXXXXX')
    $poNumber = $row.PONumber.Trim()
    if ($poNumber -eq "") { $poNumber = $null }

    $location = $row.InitialLocation.Trim()
    if ($location -eq "") { $location = $null }

    $validRows.Add([PSCustomObject]@{
            Quantity        = $qty
            PartID          = $partID
            PONumber        = $poNumber
            EmployeeNumber  = $empNum
            Heat            = $heat
            TransactionDate = $mySqlDate
            InitialLocation = $location
        })
}

Write-Status "Valid rows  : $($validRows.Count)"
Write-Status "Skipped rows: $($skippedRows.Count)"

if ($skippedRows.Count -gt 0) {
    Write-Host ""
    Write-Host "  Skipped row details:" -ForegroundColor Yellow
    foreach ($skip in $skippedRows | Select-Object -First 20) {
        Write-Host "    Row: $($skip.Row.MaterialID) | $($skip.Row.Date) -> $($skip.Reason)" -ForegroundColor Yellow
    }
    if ($skippedRows.Count -gt 20) {
        Write-Host "    ... and $($skippedRows.Count - 20) more." -ForegroundColor Yellow
    }
}

if ($WhatIf) {
    Write-Host ""
    Write-Host "  DRY RUN complete. $($validRows.Count) rows would be imported." -ForegroundColor Green
    exit 0
}

# ---------------------------------------------------------------------------
# Locate MAMP mysql.exe
# ---------------------------------------------------------------------------

Write-Section "Locating mysql.exe"

$mysqlExe = Find-MampMySql
if (-not $mysqlExe) {
    Write-Host "  ERROR: mysql.exe not found." -ForegroundColor Red
    Write-Host "  Install MAMP or add mysql to your PATH." -ForegroundColor Yellow
    exit 1
}
Write-Status "mysql.exe : $mysqlExe" "Green"

# ---------------------------------------------------------------------------
# Write a temp MySQL credentials file (.cnf) so we never pass --password on
# the command line (which generates the insecure-password warning and causes
# mysql.exe to return exit code 1 even on success).
# ---------------------------------------------------------------------------

$tmpCnf = [System.IO.Path]::GetTempFileName() -replace '\.tmp$', '.cnf'
[System.IO.File]::WriteAllText($tmpCnf, "[client]`nuser=$User`npassword=$Password`n")

# ---------------------------------------------------------------------------
# Test connection
# ---------------------------------------------------------------------------

Write-Section "Testing MySQL connection"

try {
    $testResult = & $mysqlExe "--defaults-extra-file=$tmpCnf" `
        --host=$Server --port=$Port `
        --connect-timeout=5 --silent --execute="SELECT 1;" `
        $Database 2>&1

    if ($LASTEXITCODE -ne 0) {
        Remove-Item $tmpCnf -Force -ErrorAction SilentlyContinue
        Write-Host "  ERROR: Could not connect to MySQL at $Server`:$Port" -ForegroundColor Red
        Write-Host "  $testResult" -ForegroundColor Red
        Write-Host ""
        Write-Host "  Tip: Make sure MAMP is running (start MySQL in the MAMP control panel)." -ForegroundColor Yellow
        exit 1
    }
    Write-Status "Connected to '$Database' on $Server`:$Port" "Green"
}
catch {
    Remove-Item $tmpCnf -Force -ErrorAction SilentlyContinue
    Write-Host "  ERROR: $_" -ForegroundColor Red
    exit 1
}

# ---------------------------------------------------------------------------
# Build bulk SQL temp file
# Each row becomes a CALL sp_Receiving_History_Import(...) statement.
# Using a single SQL file avoids per-row process overhead (7k+ rows).
# ---------------------------------------------------------------------------

Write-Section "Building SQL batch file"

$tmpSql = [System.IO.Path]::GetTempFileName() -replace '\.tmp$', '.sql'

try {
    $sw = [System.IO.StreamWriter]::new($tmpSql, $false, [System.Text.Encoding]::UTF8)

    $sw.WriteLine("SET NAMES utf8mb4;")
    $sw.WriteLine("SET SESSION sql_mode = 'STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';")
    $sw.WriteLine("")

    foreach ($row in $validRows) {
        $qty = $row.Quantity
        $partID = Format-MySqlValue $row.PartID
        $po = Format-MySqlValue $row.PONumber
        $emp = $row.EmployeeNumber
        $heat = Format-MySqlValue $row.Heat
        $txDate = Format-MySqlValue $row.TransactionDate
        $initLoc = Format-MySqlValue $row.InitialLocation

        $sw.WriteLine("CALL sp_Receiving_History_Import($qty, $partID, $po, $emp, $heat, $txDate, $initLoc, NULL, 1, NULL, NULL);")
    }

    $sw.Flush()
    $sw.Close()

    Write-Status "SQL file  : $tmpSql ($([math]::Round((Get-Item $tmpSql).Length / 1KB)) KB, $($validRows.Count) statements)"

    # ---------------------------------------------------------------------------
    # Execute the batch
    # Use cmd /c with a proper < redirect — more reliable than PowerShell stdin
    # piping for large files, and avoids the Get-Content buffering overhead.
    # ---------------------------------------------------------------------------

    Write-Section "Importing Records"
    Write-Host "  Executing batch against MySQL..." -ForegroundColor DarkCyan

    $startTime = Get-Date

    # cmd /c handles the < stdin redirect natively; capture stderr separately
    $tmpErr = [System.IO.Path]::GetTempFileName()
    $importOutput = cmd /c "`"$mysqlExe`" `"--defaults-extra-file=$tmpCnf`" --host=$Server --port=$Port --default-character-set=utf8mb4 --batch --silent $Database < `"$tmpSql`" 2>`"$tmpErr`""
    $exitCode = $LASTEXITCODE
    $stderrOutput = if (Test-Path $tmpErr) { Get-Content $tmpErr -ErrorAction SilentlyContinue } else { @() }
    Remove-Item $tmpErr -Force -ErrorAction SilentlyContinue

    $elapsed = (Get-Date) - $startTime

    # Filter stderr: ignore the benign insecure-password advisory line
    $realErrors = @($stderrOutput | Where-Object { $_ -match 'ERROR' -and $_ -notmatch 'Warning' })

    if ($exitCode -ne 0 -or $realErrors.Count -gt 0) {
        Write-Host "  ERROR: MySQL returned exit code $exitCode" -ForegroundColor Red
        foreach ($line in $stderrOutput | Select-Object -First 20) {
            Write-Host "  $line" -ForegroundColor Red
        }
        exit 1
    }

    # Report any non-fatal warnings (but not the password-on-CLI advisory)
    $warnings = @($stderrOutput | Where-Object { $_ -match 'Warning' -and $_ -notmatch 'password on the command' })
    if ($warnings.Count -gt 0) {
        Write-Host "  MySQL warnings:" -ForegroundColor Yellow
        $warnings | Select-Object -First 10 | ForEach-Object { Write-Host "    $_" -ForegroundColor Yellow }
    }

    # ---------------------------------------------------------------------------
    # Summary
    # ---------------------------------------------------------------------------

    Write-Section "Import Complete"
    Write-Host "  Rows in CSV        : $($rowCount + $headerSkipped)" -ForegroundColor White
    Write-Host "  Silently skipped   : $headerSkipped (blank / repeated header rows)" -ForegroundColor DarkGray
    Write-Host "  Skipped (bad data) : $($skippedRows.Count)" -ForegroundColor $(if ($skippedRows.Count -gt 0) { 'Yellow' } else { 'Green' })
    Write-Host "  Imported           : $($validRows.Count)" -ForegroundColor Green
    Write-Host "  Elapsed            : $([math]::Round($elapsed.TotalSeconds))s" -ForegroundColor DarkGray
    Write-Host ""
}
finally {
    Remove-Item $tmpSql  -Force -ErrorAction SilentlyContinue
    Remove-Item $tmpCnf  -Force -ErrorAction SilentlyContinue
}

exit 0
