param(
    [string]$MysqlExe = "mysql",
    [string]$DbHost = "127.0.0.1",
    [int]$Port = 3306,
    [string]$User = "root",
    [string]$Password = "",
    [string]$Database = "mtm_waitlist",
    [string]$RootDir = (Split-Path -Parent $PSCommandPath),
    [bool]$IncludeSeed = $true
)

$ErrorActionPreference = "Stop"

function Get-SqlFilesInOrder {
    param(
        [string]$BaseDir,
        [bool]$IncludeSeed
    )

    $paths = @(
        Join-Path $BaseDir "schema\000_create_database.sql"
        (Join-Path $BaseDir "schema\tables\*.sql")
        (Join-Path $BaseDir "schema\views\*.sql")
        (Join-Path $BaseDir "routines\procedures\*.sql")
        (Join-Path $BaseDir "routines\functions\*.sql")
        (Join-Path $BaseDir "triggers\*.sql")
    )

    if ($IncludeSeed) {
        $paths += (Join-Path $BaseDir "seed\*.sql")
    }

    $files = @()
    foreach ($pattern in $paths) {
        if (Test-Path $pattern) {
            $files += Get-ChildItem -Path $pattern -File | Sort-Object Name
        }
    }

    $files | Select-Object -ExpandProperty FullName
}

function Build-MySqlArgs {
    param(
        [string]$DbName,
        [string]$FilePath,
        [bool]$SkipDatabase
    )

    $args = @("-h", $DbHost, "-P", $Port, "-u", $User)

    if ($Password -ne "") {
        $args += "-p$Password"
    }

    if (-not $SkipDatabase) {
        $args += $DbName
    }

    $args += @("-e", "source $FilePath")

    return $args
}

$mysqlExeResolved = $MysqlExe
if (-not (Get-Command $mysqlExeResolved -ErrorAction SilentlyContinue)) {
    $candidatePaths = @(
        "C:\\MAMP\\bin\\mysql\\bin\\mysql.exe"
    )

    foreach ($candidate in $candidatePaths) {
        if (Test-Path $candidate) {
            $mysqlExeResolved = $candidate
            break
        }
    }
}

if (-not (Get-Command $mysqlExeResolved -ErrorAction SilentlyContinue)) {
    throw "mysql executable not found. Set -MysqlExe to the full path (e.g., C:\\MAMP\\bin\\mysql\\bin\\mysql.exe)."
}

$sqlFiles = Get-SqlFilesInOrder -BaseDir $RootDir -IncludeSeed:$IncludeSeed
if ($sqlFiles.Count -eq 0) {
    throw "No SQL files found under $RootDir."
}

Write-Host "Applying SQL scripts to $Database on ${DbHost}:$Port as $User" -ForegroundColor Cyan

foreach ($file in $sqlFiles) {
    $fileName = Split-Path -Leaf $file
    Write-Host "Running: $fileName" -ForegroundColor Gray

    $skipDb = $fileName -eq "000_create_database.sql"
    $args = Build-MySqlArgs -DbName $Database -FilePath $file -SkipDatabase:$skipDb
    & $mysqlExeResolved @args
}

Write-Host "Done." -ForegroundColor Green
