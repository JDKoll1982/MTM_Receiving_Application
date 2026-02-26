# ============================================================================
# Import-ReceivingHistory.ps1
# Purpose: Import receiving history from a CSV file into MAMP MySQL
# Source:  Google Sheets "Receiving Data - History" export (CSV format)
# CSV Columns: Quantity, Material ID, PO Number, Employee, Heat, Date, Initial Location
#
# Usage:
#   .\Import-ReceivingHistory.ps1 -WhatIf          # dry run
#   .\Import-ReceivingHistory.ps1                  # live import to 172.16.1.104 MAMP
#   .\Import-ReceivingHistory.ps1 -Server 10.0.0.1 # remote server
# ============================================================================

param(
    [string]$Server = "172.16.1.104",
    [string]$Port = "3306",
    [string]$Database = "mtm_receiving_application",
    [string]$User = "root",
    [string]$Password = "root",
    [string]$CsvPath = "$PSScriptRoot\..\..\docs\GoogleSheetsVersion\Receiving Data - History 2025.csv",
    [switch]$WhatIf,
    [switch]$SkipErrors,
    # ---------------------------------------------------------------------------
    # Infor Visual (SQL Server) enrichment. Defaults match appsettings.json.
    # Pass -SkipInforVisualLookup to run without querying Infor Visual.
    # ---------------------------------------------------------------------------
    [string]$InforVisualServer = "VISUAL",
    [string]$InforVisualDatabase = "MTMFG",
    [string]$InforVisualUser = "SHOP2",
    [string]$InforVisualPassword = "SHOP",
    [switch]$SkipInforVisualLookup
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

# Formats a PO number to the canonical PO-NNNNNN (6-digit zero-padded) form.
# Mirrors the FormatPONumber() logic in View_Receiving_ManualEntry.xaml.cs and
# ViewModel_Receiving_POEntry.cs:
#   "66868"     -> "PO-066868"
#   "64489B"    -> "PO-064489B"  (numeric + B suffix)
#   "PO-66868"  -> "PO-066868"
#   "PO-064489B"-> "PO-064489B"  (alpha suffix preserved as-is)
#   ""          -> $null
function Format-PoNumber {
    param([string]$PoNumber)

    $trimmed = $PoNumber.Trim()
    if ($trimmed -eq '') { return $null }

    if ($trimmed -match '^[Pp][Oo]-(.+)$') {
        $numberPart = $Matches[1]
        # Pure digits up to 6 chars: zero-pad
        if ($numberPart -match '^\d{1,6}$') {
            return 'PO-' + $numberPart.PadLeft(6, '0')
        }
        # Digits + B suffix (e.g. PO-64489B): pad the numeric portion, keep B
        if ($numberPart -match '^(\d{1,6})([Bb])$') {
            return 'PO-' + $Matches[1].PadLeft(6, '0') + $Matches[2].ToUpper()
        }
        # Anything else (longer codes, other suffixes): normalise prefix casing only
        return 'PO-' + $numberPart
    }

    # No prefix at all - add PO- if the value is a bare number or number+B
    if ($trimmed -match '^\d{1,6}$') {
        return 'PO-' + $trimmed.PadLeft(6, '0')
    }
    if ($trimmed -match '^(\d{1,6})([Bb])$') {
        return 'PO-' + $Matches[1].PadLeft(6, '0') + $Matches[2].ToUpper()
    }

    # Unrecognised format - return as-is so the record still imports
    return $trimmed
}

# ---------------------------------------------------------------------------
# Validate prerequisites
# ----------------------------------------------------------------------JKO-----

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

    # Quantity: accept decimals by rounding up to the nearest integer
    $qtyRaw = 0.0
    $qty    = 0
    if (-not [double]::TryParse($row.Quantity, [ref]$qtyRaw) -or $qtyRaw -le 0) {
        $errors += "Invalid quantity: '$($row.Quantity)'"
    } else {
        $qty = [int][Math]::Ceiling($qtyRaw)
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

    # Normalise PO number: apply canonical PO-NNNNNN zero-padding (matches app workflow)
    $poNumber = Format-PoNumber $row.PONumber

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
            PartDescription = $null    # enriched from Infor Visual
            VendorName      = $null    # enriched from Infor Visual
            IsNonPOItem     = [int]0   # 1 when part not in IV and no PO
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

# ---------------------------------------------------------------------------
# Enrich rows from Infor Visual (SQL Server)
#   Has PO  + found     -> PartDescription + VendorName populated from IV
#   No PO   + found     -> PartDescription populated from IV
#   Has PO  + NOT found -> added to $correctionRows (imported; needs review)
#   No PO   + NOT found -> IsNonPOItem = 1  (expected for non-catalogue parts)
# ---------------------------------------------------------------------------
Write-Section "Enriching from Infor Visual"

$correctionRows = [System.Collections.Generic.List[PSCustomObject]]::new()
$ivEnriched = 0
$ivNonPO = 0

if ($SkipInforVisualLookup) {
    Write-Status "IV lookup SKIPPED (-SkipInforVisualLookup specified)." "Yellow"
}
elseif ($validRows.Count -eq 0) {
    Write-Status "No valid rows to enrich." "DarkGray"
}
else {
    $ivConn = $null
    try {
        $ivCs = "Server=$InforVisualServer;Database=$InforVisualDatabase;" +
        "User Id=$InforVisualUser;Password=$InforVisualPassword;" +
        "TrustServerCertificate=True;ApplicationIntent=ReadOnly;Connect Timeout=10;"
        Write-Status "Connecting to $InforVisualServer\$InforVisualDatabase..."
        $ivConn = New-Object System.Data.SqlClient.SqlConnection($ivCs)
        $ivConn.Open()
        Write-Status "Connected to Infor Visual." "Green"

        # Reusable parameterised commands (avoids re-parsing SQL per row)

        # Primary: full PO line lookup - gets both description and vendor in one shot
        $cmdPOPart = $ivConn.CreateCommand()
        $cmdPOPart.CommandText = @"
SELECT TOP 1
    p.DESCRIPTION AS PartDescription,
    v.NAME        AS VendorName
FROM       dbo.PURCHASE_ORDER    po
INNER JOIN dbo.PURC_ORDER_LINE   pol ON po.ID       = pol.PURC_ORDER_ID
LEFT  JOIN dbo.PART              p   ON pol.PART_ID  = p.ID
LEFT  JOIN dbo.VENDOR            v   ON po.VENDOR_ID = v.ID
WHERE po.ID       = @PO
  AND pol.PART_ID = @Part
"@
        $pPO     = $cmdPOPart.Parameters.Add("@PO",   [System.Data.SqlDbType]::NVarChar, 20)
        $pPartPO = $cmdPOPart.Parameters.Add("@Part", [System.Data.SqlDbType]::NVarChar, 50)

        # Fallback A: PO header only - vendor even when the exact part line is absent
        # (covers old/closed POs where the line was purged or the part ID drifted)
        $cmdPOHeader = $ivConn.CreateCommand()
        $cmdPOHeader.CommandText = @"
SELECT TOP 1 v.NAME AS VendorName
FROM  dbo.PURCHASE_ORDER po
LEFT  JOIN dbo.VENDOR    v ON po.VENDOR_ID = v.ID
WHERE po.ID = @PO
"@
        $pPOHeader = $cmdPOHeader.Parameters.Add("@PO", [System.Data.SqlDbType]::NVarChar, 20)

        # Fallback B / No-PO path: part description from PART master
        $cmdPart = $ivConn.CreateCommand()
        $cmdPart.CommandText = "SELECT TOP 1 DESCRIPTION AS PartDescription FROM dbo.PART WHERE ID = @Part"
        $pPartOnly = $cmdPart.Parameters.Add("@Part", [System.Data.SqlDbType]::NVarChar, 50)

        $ivFallback = 0   # rows enriched via fallback (partial match)

        foreach ($row in $validRows) {
            try {
                if (![string]::IsNullOrWhiteSpace($row.PONumber)) {
                    # IV stores PO IDs as 'PO-NNNNNN' - pass the formatted number directly
                    $pPO.Value     = $row.PONumber
                    $pPartPO.Value = $row.PartID
                    $rdr = $cmdPOPart.ExecuteReader()
                    if ($rdr.Read()) {
                        # Full match: PO line found
                        $row.PartDescription = $rdr["PartDescription"].ToString().Trim()
                        $row.VendorName      = $rdr["VendorName"].ToString().Trim()
                        $rdr.Dispose()
                        $ivEnriched++
                    }
                    else {
                        # PO line not found (old/closed PO or part-ID mismatch) -
                        # attempt two independent fallbacks before flagging for correction.
                        $rdr.Dispose()   # must close before opening new readers on same connection

                        # Fallback A: vendor from PO header
                        $pPOHeader.Value = $row.PONumber
                        $rdrH = $cmdPOHeader.ExecuteReader()
                        if ($rdrH.Read()) { $row.VendorName = $rdrH["VendorName"].ToString().Trim() }
                        $rdrH.Dispose()

                        # Fallback B: description from PART master
                        $pPartOnly.Value = $row.PartID
                        $rdrP = $cmdPart.ExecuteReader()
                        if ($rdrP.Read()) { $row.PartDescription = $rdrP["PartDescription"].ToString().Trim() }
                        $rdrP.Dispose()

                        if ($row.VendorName -or $row.PartDescription) { $ivFallback++ }
                        $correctionRows.Add($row)
                    }
                }
                else {
                    # No PO – look up the part description directly
                    $pPartOnly.Value = $row.PartID
                    $rdr = $cmdPart.ExecuteReader()
                    if ($rdr.Read()) {
                        $row.PartDescription = $rdr["PartDescription"].ToString().Trim()
                        $ivEnriched++
                    }
                    else {
                        # Part not in IV and no PO -> Non-PO item
                        $row.IsNonPOItem = 1
                        $ivNonPO++
                    }
                    $rdr.Dispose()
                }
            }
            catch {
                Write-Host "    Warning: IV lookup skipped for '$($row.PartID)': $_" -ForegroundColor Yellow
            }
        }

        $cmdPOPart.Dispose()
        $cmdPOHeader.Dispose()
        $cmdPart.Dispose()
    }
    catch {
        Write-Host "  WARNING: Could not connect to Infor Visual ($InforVisualServer\$InforVisualDatabase):" -ForegroundColor Yellow
        Write-Host "  $_"                                                                                    -ForegroundColor Yellow
        Write-Host "  Continuing without IV enrichment. Use -SkipInforVisualLookup to suppress."            -ForegroundColor Yellow
    }
    finally {
        if ($null -ne $ivConn) { $ivConn.Dispose() }
    }

    $ivNotFound = $correctionRows.Count
    Write-Status "IV enriched     : $ivEnriched row(s) (full PO line match)"       $(if ($ivEnriched -gt 0)   { 'Green' }    else { 'DarkGray' })
    if ($ivFallback  -gt 0) { Write-Status "IV fallback     : $ivFallback row(s) (partial - PO header / PART master)" "Cyan" }
    if ($ivNonPO     -gt 0) { Write-Status "Non-PO flagged  : $ivNonPO row(s) (part not in IV, no PO)"              "Yellow" }
    if ($ivNotFound  -gt 0) { Write-Status "Needs correction: $ivNotFound row(s) (PO line not in IV - partially enriched where possible)" "Red" }
}

if ($WhatIf) {
    Write-Host ""
    Write-Host "  DRY RUN complete. $($validRows.Count) rows would be imported." -ForegroundColor Green
    if ($correctionRows.Count -gt 0) {
        Write-Host "  $($correctionRows.Count) row(s) will be imported but need correction (PO line not found in IV)." -ForegroundColor Yellow
    }
    if ($ivFallback -gt 0) {
        Write-Host "  $ivFallback row(s) partially enriched via PO header / PART master (PO line not found)." -ForegroundColor Cyan
    }
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
        $vendor = Format-MySqlValue $row.VendorName
        $partDesc = Format-MySqlValue $row.PartDescription
        $isNonPO = [int]$row.IsNonPOItem

        $sw.WriteLine("CALL sp_Receiving_History_Import($qty, $partID, $po, $emp, $heat, $txDate, $initLoc, NULL, 1, $vendor, $partDesc, $isNonPO);")
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

    if ($correctionRows.Count -gt 0) {
        Write-Host "  CORRECTION REQUIRED — PO + Part combination not found in Infor Visual:" -ForegroundColor Red
        Write-Host ("  {0,-22} {1,-15} {2,-11} {3}" -f "PartID", "PO Number", "Date", "Emp") -ForegroundColor Red
        Write-Host "  $('-' * 64)" -ForegroundColor DarkGray
        foreach ($cr in $correctionRows) {
            $crPO = if ($null -ne $cr.PONumber) { $cr.PONumber } else { '' }
            Write-Host ("  {0,-22} {1,-15} {2,-11} {3}" -f $cr.PartID, $crPO, $cr.TransactionDate, $cr.EmployeeNumber) -ForegroundColor Yellow
        }
        Write-Host ""
        Write-Host "  Verify the PO number and Part ID in Infor Visual for each row above, then re-import." -ForegroundColor Yellow
    }
}
finally {
    Remove-Item $tmpSql  -Force -ErrorAction SilentlyContinue
    Remove-Item $tmpCnf  -Force -ErrorAction SilentlyContinue
}

exit 0
