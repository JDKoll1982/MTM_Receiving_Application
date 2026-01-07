<#
.SYNOPSIS
    Searches a specific PO in Infor Visual ERP for lines containing a specific string in the specification fields.

.DESCRIPTION
    This script connects to the Infor Visual ERP database (MTMFG) and searches through all lines
    of a specified PO to find those where the "PO Line Specs" (stored in PURC_LINE_BINARY.BITS) 
    contain the specified search string.
    
    The script searches the binary/Unicode text data stored in the BITS column of PURC_LINE_BINARY table.

.PARAMETER PONumber
    REQUIRED. The PO number to search within.

.PARAMETER SearchString
    REQUIRED. The text to search for in the PO line specification fields. Case-insensitive.
    Wildcards (%) are automatically added before and after the search string.

.PARAMETER OutputPath
    Optional. Path to save results as CSV file. If not specified, displays results in console.

.PARAMETER LogPath
    Optional. Path to save log file. If not specified, creates log in Scripts folder with timestamp.

.EXAMPLE
    .\Search-POLineSpecs.ps1 -PONumber "12345" -SearchString "PICK TICKET"
    
    Searches PURC_LINE_BINARY in PO 12345 for lines containing "PICK TICKET".

.EXAMPLE
    .\Search-POLineSpecs.ps1 -PONumber "67890" -SearchString "GALV" -OutputPath "C:\Temp\po_results.csv"
    
    Searches PURC_LINE_BINARY in PO 67890 for "GALV" and saves results to CSV.

.NOTES
    Server: VISUAL
    Database: MTMFG
    Authentication: SQL Server Authentication
    User: JKOLL
    
    This script uses READ-ONLY access to the Infor Visual database.
    DO NOT attempt to modify data through this connection.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, HelpMessage = "PO number to search within", Position = 0)]
    [string]$PONumber,
    
    [Parameter(Mandatory = $true, HelpMessage = "Text to search for in PO line specs", Position = 1)]
    [string]$SearchString,
    
    [Parameter(Mandatory = $false, HelpMessage = "Path to save results as CSV")]
    [string]$OutputPath,
    
    [Parameter(Mandatory = $false, HelpMessage = "Path to save log file")]
    [string]$LogPath
)

# Setup logging
if (-not $LogPath) {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $LogPath = Join-Path $PSScriptRoot "Search-POLineSpecs_$timestamp.log"
}

function Write-Log {
    param(
        [string]$Message,
        [ValidateSet('INFO', 'WARNING', 'ERROR', 'SUCCESS')]
        [string]$Level = 'INFO'
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    
    # Write to log file
    Add-Content -Path $LogPath -Value $logMessage
    
    # Also write to console with appropriate color
    switch ($Level) {
        'INFO' { Write-Host $Message -ForegroundColor Cyan }
        'SUCCESS' { Write-Host $Message -ForegroundColor Green }
        'WARNING' { Write-Host $Message -ForegroundColor Yellow }
        'ERROR' { Write-Host $Message -ForegroundColor Red }
    }
}

Write-Log "=== PO Line Specs Search Started ===" -Level INFO
Write-Log "Log file: $LogPath" -Level INFO

# Connection settings
$serverName = "VISUAL"
$databaseName = "MTMFG"
$username = "JKOLL"
$password = "KOLL"

# Build connection string with READ-ONLY intent
$connectionString = "Server=$serverName;Database=$databaseName;User Id=$username;Password=$password;ApplicationIntent=ReadOnly;TrustServerCertificate=True;Connection Timeout=30;"

Write-Log "Connection String: Server=$serverName;Database=$databaseName;ApplicationIntent=ReadOnly" -Level INFO

Write-Log "=== Search Parameters ===" -Level INFO
Write-Log "PO Number: $PONumber" -Level INFO
Write-Log "Search String: '$SearchString'" -Level INFO
Write-Log "Searching PURC_LINE_BINARY.BITS column for Unicode text" -Level INFO

$sqlQuery = @"
SELECT 
    po.ID AS PO_Number,
    pol.LINE_NO AS Line_Number,
    pol.PART_ID AS Part_Number,
    pol.VENDOR_PART_ID AS Vendor_Part_Number,
    pol.USER_ORDER_QTY AS Ordered_Qty,
    pol.PURCHASE_UM AS Unit_Of_Measure,
    pol.TOTAL_RECEIVED_QTY AS Received_Qty,
    pol.LINE_STATUS AS Line_Status,
    pol.DESIRED_RECV_DATE AS Desired_Recv_Date,
    pol.LAST_RECEIVED_DATE AS Last_Recv_Date,
    CONVERT(NVARCHAR(MAX), CONVERT(VARBINARY(MAX), plb.BITS)) AS Spec_Text,
    po.VENDOR_ID AS Vendor_ID,
    po.STATUS AS PO_Status,
    po.ORDER_DATE AS PO_Date,
    po.SITE_ID AS Site_ID
FROM 
    dbo.PURC_ORDER_LINE pol
    INNER JOIN dbo.PURCHASE_ORDER po ON pol.PURC_ORDER_ID = po.ID
    INNER JOIN dbo.PURC_LINE_BINARY plb ON pol.PURC_ORDER_ID = plb.PURC_ORDER_ID 
        AND pol.LINE_NO = plb.PURC_ORDER_LINE_NO
        AND plb.TYPE = 'D'
WHERE 
    po.ID = @PONumber
    AND CONVERT(NVARCHAR(MAX), CONVERT(VARBINARY(MAX), plb.BITS)) LIKE @SearchPattern
ORDER BY 
    pol.LINE_NO
"@

Write-Log "=== SQL Query Generated ===" -Level INFO
Write-Log "Query: $sqlQuery" -Level INFO

Write-Host "Connecting to SQL Server: $serverName" -ForegroundColor Cyan
Write-Host "Database: $databaseName" -ForegroundColor Cyan
Write-Host "PO Number: $PONumber" -ForegroundColor Yellow
Write-Host "Search String: '$SearchString'" -ForegroundColor Yellow
Write-Host "Search Location: PURC_LINE_BINARY.BITS (Unicode text specifications)" -ForegroundColor Yellow
Write-Host ""

try {
    Write-Log "=== Database Connection Attempt ===" -Level INFO
    Write-Log "Server: $serverName, Database: $databaseName" -Level INFO
    
    # Create connection
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connection established successfully" -ForegroundColor Green
    Write-Log "Connection established successfully" -Level SUCCESS
    Write-Host "Executing query..." -ForegroundColor Cyan
    Write-Log "Executing query..." -Level INFO
    Write-Host ""
    
    # Create command
    $command = $connection.CreateCommand()
    $command.CommandText = $sqlQuery
    $command.CommandTimeout = 60
    
    # Add parameters
    $command.Parameters.AddWithValue("@PONumber", $PONumber) | Out-Null
    $command.Parameters.AddWithValue("@SearchPattern", "%$SearchString%") | Out-Null
    
    Write-Log "Parameters: PONumber='$PONumber', SearchPattern='%$SearchString%'" -Level INFO
    
    # Execute query
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataSet = New-Object System.Data.DataSet
    
    Write-Log "Executing SQL query..." -Level INFO
    $rowCount = $adapter.Fill($dataSet)
    
    Write-Host "Query executed successfully" -ForegroundColor Green
    Write-Host "Results found: $rowCount" -ForegroundColor Yellow
    Write-Log "Query executed successfully" -Level SUCCESS
    Write-Log "Return Value: $rowCount row(s) returned" -Level SUCCESS
    Write-Host ""
    
    if ($rowCount -gt 0) {
        $results = $dataSet.Tables[0]
        
        Write-Log "=== Results Summary ===" -Level INFO
        Write-Log "Total matching rows returned: $rowCount" -Level INFO
        
        # Log PO numbers found and get total line counts for each PO
        $poNumbers = $results | Select-Object -ExpandProperty PO_Number -Unique
        Write-Log "Unique PO Numbers found: $($poNumbers -join ', ')" -Level INFO
        
        # Get total line count vs matching line count for the PO
        Write-Log "=== PO Line Counts (Total vs Matching) ===" -Level INFO
        $matchingLinesCount = $rowCount
        
        # Query total lines for this PO
        $totalLinesQuery = "SELECT COUNT(*) AS TotalLines FROM dbo.PURC_ORDER_LINE pol INNER JOIN dbo.PURCHASE_ORDER po ON pol.PURC_ORDER_ID = po.ID WHERE po.ID = @PONumber"
        $totalLinesCmd = $connection.CreateCommand()
        $totalLinesCmd.CommandText = $totalLinesQuery
        $totalLinesCmd.Parameters.AddWithValue("@PONumber", $PONumber) | Out-Null
        
        $totalLinesReader = $totalLinesCmd.ExecuteReader()
        $totalLines = 0
        if ($totalLinesReader.Read()) {
            $totalLines = $totalLinesReader["TotalLines"]
        }
        $totalLinesReader.Close()
        
        $percentage = if ($totalLines -gt 0) { [math]::Round(($matchingLinesCount / $totalLines) * 100, 1) } else { 0 }
        
        Write-Log "PO $PONumber : $matchingLinesCount of $totalLines lines match ($percentage%)" -Level INFO
        Write-Host "PO $PONumber : $matchingLinesCount of $totalLines total lines contain '$SearchString' ($percentage%)" -ForegroundColor Cyan
        
        Write-Host ""
        
        # Save to CSV if OutputPath specified
        if ($OutputPath) {
            $results | Export-Csv -Path $OutputPath -NoTypeInformation -Encoding UTF8
            Write-Host "Results saved to: $OutputPath" -ForegroundColor Green
            Write-Log "Results saved to CSV: $OutputPath" -Level SUCCESS
            Write-Host ""
        }
        
        # Display summary in console
        Write-Host "=== SEARCH RESULTS SUMMARY ===" -ForegroundColor Cyan
        Write-Host ""
        
        $results | Format-Table -Property `
            PO_Number, 
        Line_Number, 
        Part_Number, 
        Ordered_Qty, 
        Received_Qty, 
        Line_Status, 
        @{Label = 'Spec_Text_Preview'; Expression = {
                $text = $_.Spec_Text
                if ($text.Length -gt 100) {
                    $text.Substring(0, 100) + "..."
                }
                else {
                    $text
                }
            }
        } `
            -AutoSize
        
        Write-Host ""
        Write-Host "=== DETAILED RESULTS ===" -ForegroundColor Cyan
        
        Write-Log "=== Detailed Results ===" -Level INFO
        
        foreach ($row in $results) {
            Write-Host ""
            Write-Host "PO Number: $($row.PO_Number) | Line: $($row.Line_Number)" -ForegroundColor Yellow
            
            # Log each result
            Write-Log "PO: $($row.PO_Number), Line: $($row.Line_Number), Part: $($row.Part_Number), Status: $($row.Line_Status)" -Level INFO
            
            Write-Host "Part: $($row.Part_Number) | Vendor Part: $($row.Vendor_Part_Number)"
            Write-Host "Vendor: $($row.Vendor_ID) | Status: $($row.PO_Status)"
            Write-Host "Ordered: $($row.Ordered_Qty) $($row.Unit_Of_Measure) | Received: $($row.Received_Qty)"
            Write-Host "Desired Recv Date: $($row.Desired_Recv_Date) | Last Recv: $($row.Last_Recv_Date)"
            
            Write-Host "Specification Text:" -ForegroundColor Cyan
            Write-Host "  $($row.Spec_Text)" -ForegroundColor Green
            
            Write-Log "  Spec Text: $($row.Spec_Text)" -Level INFO
            Write-Host ("-" * 80) -ForegroundColor DarkGray
        }
    }
    else {
        Write-Host "No PO lines found matching the search criteria." -ForegroundColor Yellow
        Write-Log "No results found matching search criteria" -Level WARNING
        Write-Host ""
        Write-Host "Suggestions:" -ForegroundColor Cyan
        Write-Host "  - Try a different search string"
        Write-Host "  - Verify the PO number is correct: $PONumber"
        Write-Host "  - Check if the PO has any specification text in PURC_LINE_BINARY"
        Write-Host "  - Note: PO line specs are stored in PURC_LINE_BINARY.BITS, not USER fields"
    }
}
catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Log "=== DATABASE ERROR ===" -Level ERROR
    Write-Log "Error Message: $($_.Exception.Message)" -Level ERROR
    Write-Log "Error Type: $($_.Exception.GetType().FullName)" -Level ERROR
    
    if ($_.Exception.InnerException) {
        Write-Log "Inner Exception: $($_.Exception.InnerException.Message)" -Level ERROR
    }
    
    Write-Host ""
    Write-Host "Stack Trace:" -ForegroundColor DarkRed
    Write-Host $_.Exception.StackTrace -ForegroundColor DarkRed
    Write-Log "Stack Trace: $($_.Exception.StackTrace)" -Level ERROR
    
    # Log SQL-specific errors
    if ($_.Exception -is [System.Data.SqlClient.SqlException]) {
        $sqlEx = $_.Exception
        Write-Log "SQL Error Number: $($sqlEx.Number)" -Level ERROR
        Write-Log "SQL Error State: $($sqlEx.State)" -Level ERROR
        Write-Log "SQL Error Class: $($sqlEx.Class)" -Level ERROR
        Write-Log "SQL Server: $($sqlEx.Server)" -Level ERROR
        Write-Log "SQL Procedure: $($sqlEx.Procedure)" -Level ERROR
        Write-Log "SQL Line Number: $($sqlEx.LineNumber)" -Level ERROR
    }
}
finally {
    # Clean up
    if ($connection -and $connection.State -eq 'Open') {
        $connection.Close()
        Write-Host ""
        Write-Host "Connection closed" -ForegroundColor Cyan
        Write-Log "Connection closed" -Level INFO
    }
    
    Write-Log "=== PO Line Specs Search Completed ===" -Level INFO
    Write-Host ""
    Write-Host "Log file saved to: $LogPath" -ForegroundColor Cyan
}
