<#!
.SYNOPSIS
  Generates a JSON map of MySQL stored procedure parameters to help implement missing DAOs.

.DESCRIPTION
  - Scans Database/StoredProcedures/Settings/**/*.sql
  - Extracts CREATE PROCEDURE name + parameter list
  - Scans Module_Settings/Data/Dao_*.cs for stored procedure usage
  - Produces JSON including:
      * storedProcedure
      * parameters (direction/name/sqlType)
      * suggestedDao (based on naming)
      * referencedInDaos (existing references)
      * missingDao (if suggested DAO file doesn't exist)

.PARAMETER OutputPath
  Output JSON path. Defaults to Scripts/outputs/settings-dao-parameter-map.json

.PARAMETER StoredProcedureRoot
  Folder containing Settings stored procedure SQL files.

.PARAMETER DaoRoot
  Folder containing DAO C# files.

.EXAMPLE
  .\Scripts\Generate-SettingsDaoParameterMap.ps1

.EXAMPLE
  .\Scripts\Generate-SettingsDaoParameterMap.ps1 -OutputPath .\_bmad-output\implementation-artifacts\settings-dao-parameter-map.json
#>

[CmdletBinding()]
param(
    [string]$OutputPath = "Scripts/outputs/settings-dao-parameter-map.json",
    [string]$StoredProcedureRoot = "Database/StoredProcedures/Settings",
    [string]$DaoRoot = "Module_Settings/Data"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-RepoPath([string]$path) {
    if ([System.IO.Path]::IsPathRooted($path)) { return $path }
    return (Join-Path -Path (Get-Location) -ChildPath $path)
}

function Remove-SqlComments([string]$sql) {
    # remove /* */ comments
    $noBlock = [regex]::Replace($sql, "/\*.*?\*/", "", [System.Text.RegularExpressions.RegexOptions]::Singleline)
    # remove -- comments
    $noLine = [regex]::Replace($noBlock, "--.*?$", "", [System.Text.RegularExpressions.RegexOptions]::Multiline)
    return $noLine
}

function Split-CommaOutsideParens([string]$text) {
    $parts = @()
    $sb = New-Object System.Text.StringBuilder
    $depth = 0
    foreach ($ch in $text.ToCharArray()) {
        if ($ch -eq '(') { $depth++ }
        elseif ($ch -eq ')') { if ($depth -gt 0) { $depth-- } }

        if ($ch -eq ',' -and $depth -eq 0) {
            $parts += $sb.ToString()
            $null = $sb.Clear()
            continue
        }

        $null = $sb.Append($ch)
    }
    if ($sb.Length -gt 0) { $parts += $sb.ToString() }
    return @($parts)
}

function Add-Unique([hashtable]$table, [string]$key) {
    if ([string]::IsNullOrWhiteSpace($key)) { return }
    if (-not $table.ContainsKey($key)) { $table[$key] = $true }
}

function Analyze-ProcedureColumns([string]$bodySql) {
    # Returns:
    #  - perTable: tableName -> @{ reads=@(); writes=@(); }
    #  - readColumns: flattened unique list of "table.column"
    #  - writeColumns: flattened unique list of "table.column"
    if ([string]::IsNullOrWhiteSpace($bodySql)) {
        return [pscustomobject]@{ perTable = @(); readColumns = @(); writeColumns = @() }
    }

    $sql = Remove-SqlComments $bodySql
    $sql = $sql -replace "\r\n", "`n"

    # build alias -> table map from FROM/JOIN clauses
    $aliasToTable = @{}
    $fromJoinPattern = '(?is)\b(?:FROM|JOIN)\s+[`"`]?(?<table>[A-Za-z0-9_]+)[`"`]?\s*(?:AS\s+)?(?<alias>[A-Za-z0-9_]+)?'
    foreach ($m in [regex]::Matches($sql, $fromJoinPattern)) {
        $table = $m.Groups['table'].Value
        $alias = $m.Groups['alias'].Value
        if ([string]::IsNullOrWhiteSpace($alias)) { $alias = $table }
        if (-not $aliasToTable.ContainsKey($alias)) { $aliasToTable[$alias] = $table }
    }

    $readsByTable = @{}
    $writesByTable = @{}

    function Add-Read([string]$table, [string]$col) {
        if ([string]::IsNullOrWhiteSpace($table) -or [string]::IsNullOrWhiteSpace($col)) { return }
        if (-not $readsByTable.ContainsKey($table)) { $readsByTable[$table] = @{} }
        Add-Unique $readsByTable[$table] $col
    }

    function Add-Write([string]$table, [string]$col) {
        if ([string]::IsNullOrWhiteSpace($table) -or [string]::IsNullOrWhiteSpace($col)) { return }
        if (-not $writesByTable.ContainsKey($table)) { $writesByTable[$table] = @{} }
        Add-Unique $writesByTable[$table] $col
    }

    # INSERT INTO table (col1, col2, ...)
    $insertPattern = '(?is)\bINSERT\s+(?:IGNORE\s+)?INTO\s+[`"`]?(?<table>[A-Za-z0-9_]+)[`"`]?\s*\((?<cols>[^\)]*)\)'
    foreach ($m in [regex]::Matches($sql, $insertPattern)) {
        $table = $m.Groups['table'].Value
        $cols = Split-CommaOutsideParens $m.Groups['cols'].Value
        foreach ($c in $cols) {
            $col = ($c -replace '[`"`]', '').Trim()
            if (-not [string]::IsNullOrWhiteSpace($col)) { Add-Write $table $col }
        }
    }

    # UPDATE table [alias] SET a=b, c=d
    $updatePattern = '(?is)\bUPDATE\s+[`"`]?(?<table>[A-Za-z0-9_]+)[`"`]?\s*(?:AS\s+)?(?<alias>[A-Za-z0-9_]+)?\s+SET\s+(?<set>.*?)(?:\bWHERE\b|;|$)'
    foreach ($m in [regex]::Matches($sql, $updatePattern)) {
        $table = $m.Groups['table'].Value
        $alias = $m.Groups['alias'].Value
        if ([string]::IsNullOrWhiteSpace($alias)) { $alias = $table }
        if (-not $aliasToTable.ContainsKey($alias)) { $aliasToTable[$alias] = $table }

        $setBlock = $m.Groups['set'].Value
        $assignments = Split-CommaOutsideParens $setBlock
        foreach ($a in $assignments) {
            $left = ($a -split '=')[0].Trim()
            $left = ($left -replace '[`"`]', '').Trim()
            $lm = [regex]::Match($left, '^(?:(?<alias>[A-Za-z0-9_]+)\.)?(?<col>[A-Za-z0-9_]+)$')
            if ($lm.Success) {
                $aAlias = $lm.Groups['alias'].Value
                $col = $lm.Groups['col'].Value
                $targetTable = $table
                if (-not [string]::IsNullOrWhiteSpace($aAlias) -and $aliasToTable.ContainsKey($aAlias)) {
                    $targetTable = $aliasToTable[$aAlias]
                }
                Add-Write $targetTable $col
            }
        }
    }

    # SELECT ... FROM ...  (extract qualified column references like ss.setting_value)
    $qualifiedColPattern = '(?i)\b(?<alias>[A-Za-z0-9_]+)\.(?<col>[A-Za-z0-9_]+)\b'
    foreach ($m in [regex]::Matches($sql, $qualifiedColPattern)) {
        $alias = $m.Groups['alias'].Value
        $col = $m.Groups['col'].Value
        if ($aliasToTable.ContainsKey($alias)) {
            Add-Read $aliasToTable[$alias] $col
        }
    }

    # Flatten to arrays
    $perTable = @()
    $allReads = @{}
    $allWrites = @{}

    $tables = @(@($readsByTable.Keys) + @($writesByTable.Keys) | Sort-Object -Unique)
    foreach ($t in $tables) {
        $readCols = @()
        $writeCols = @()
        if ($readsByTable.ContainsKey($t)) { $readCols = @($readsByTable[$t].Keys | Sort-Object) }
        if ($writesByTable.ContainsKey($t)) { $writeCols = @($writesByTable[$t].Keys | Sort-Object) }

        foreach ($rc in $readCols) { Add-Unique $allReads ("$t.$rc") }
        foreach ($wc in $writeCols) { Add-Unique $allWrites ("$t.$wc") }

        $perTable += [pscustomobject]@{
            table  = $t
            reads  = $readCols
            writes = $writeCols
        }
    }

    return [pscustomobject]@{
        perTable     = $perTable
        readColumns  = @($allReads.Keys | Sort-Object)
        writeColumns = @($allWrites.Keys | Sort-Object)
    }
}

function Parse-CreateProcedure([string]$sqlText) {
    $sql = Remove-SqlComments $sqlText
    # Normalize to LF. In PowerShell, newline escape is `n (not \n).
    $sql = $sql -replace "\r\n", "`n"

    # Match up to the opening '(' and then scan for the matching closing ')'.
    # This avoids breaking on VARCHAR(50) / DECIMAL(10,2) etc.
    $startPattern = '(?is)CREATE\s+(?:DEFINER\s*=\s*[^\s]+\s+)?PROCEDURE\s+[`"`]?(?<name>[A-Za-z0-9_]+)[`"`]?\s*\('
    $startMatches = [regex]::Matches($sql, $startPattern)

    $procs = @()
    foreach ($m in $startMatches) {
        $name = $m.Groups['name'].Value.Trim()

        $openParenIndex = $m.Index + $m.Length - 1
        $depth = 0
        $closeParenIndex = -1
        for ($i = $openParenIndex; $i -lt $sql.Length; $i++) {
            $ch = $sql[$i]
            if ($ch -eq '(') { $depth++ }
            elseif ($ch -eq ')') {
                $depth--
                if ($depth -eq 0) { $closeParenIndex = $i; break }
            }
        }

        if ($closeParenIndex -lt 0) { continue }

        $paramBlock = $sql.Substring($openParenIndex + 1, $closeParenIndex - $openParenIndex - 1).Trim()
        $params = @()

        if (-not [string]::IsNullOrWhiteSpace($paramBlock)) {
            # split by commas that are not within parentheses
            $parts = @()
            $sb = New-Object System.Text.StringBuilder
            $depth = 0
            foreach ($ch in $paramBlock.ToCharArray()) {
                if ($ch -eq '(') { $depth++ }
                elseif ($ch -eq ')') { if ($depth -gt 0) { $depth-- } }

                if ($ch -eq ',' -and $depth -eq 0) {
                    $parts += $sb.ToString()
                    $null = $sb.Clear()
                    continue
                }
                $null = $sb.Append($ch)
            }
            if ($sb.Length -gt 0) { $parts += $sb.ToString() }

            foreach ($raw in $parts) {
                $line = ($raw -replace "\s+", " ").Trim()
                if ([string]::IsNullOrWhiteSpace($line)) { continue }

                # examples:
                # IN p_user_id INT
                # OUT p_total INT
                # INOUT p_value DECIMAL(10,2)
                $pm = [regex]::Match($line, '(?i)^(?<dir>INOUT|IN|OUT)\s+(?<pname>[`"`]?(?:[^\s`"`]+)[`"`]?)\s+(?<ptype>.+?)$')
                if (-not $pm.Success) {
                    $params += [pscustomobject]@{
                        direction = "UNKNOWN"
                        name      = $line
                        sqlType   = ""
                        raw       = $raw.Trim()
                    }
                    continue
                }

                $dir = $pm.Groups['dir'].Value.ToUpperInvariant()
                $pname = $pm.Groups['pname'].Value.Trim('`', '"')
                $ptype = $pm.Groups['ptype'].Value.Trim()

                $params += [pscustomobject]@{
                    direction = $dir
                    name      = $pname
                    sqlType   = $ptype
                    raw       = $raw.Trim()
                }
            }
        }

        # Attempt to capture the body (BEGIN..END) for column usage analysis
        $beginIndex = $sql.IndexOf('BEGIN', $closeParenIndex, [System.StringComparison]::OrdinalIgnoreCase)
        $endIndex = -1
        if ($beginIndex -ge 0) {
            $end1 = $sql.IndexOf('END$$', $beginIndex, [System.StringComparison]::OrdinalIgnoreCase)
            $end2 = $sql.IndexOf('END;', $beginIndex, [System.StringComparison]::OrdinalIgnoreCase)
            if ($end1 -ge 0 -and $end2 -ge 0) { $endIndex = [Math]::Min($end1 + 5, $end2 + 4) }
            elseif ($end1 -ge 0) { $endIndex = $end1 + 5 }
            elseif ($end2 -ge 0) { $endIndex = $end2 + 4 }
        }

        $bodySql = $null
        if ($beginIndex -ge 0 -and $endIndex -gt $beginIndex) {
            $bodySql = $sql.Substring($beginIndex, $endIndex - $beginIndex)
        }

        $procs += [pscustomobject]@{
            storedProcedure = $name
            parameters      = $params
            bodySql         = $bodySql
        }
    }

    return $procs
}

function Guess-DaoName([string]$storedProcedure) {
    if ($storedProcedure -match '^sp_(?<prefix>[A-Za-z0-9]+)') {
        $prefix = $Matches['prefix']

        # normalize some known prefixes
        switch -Regex ($prefix) {
            '^SystemSettings$' { return 'Dao_SystemSettings' }
            '^UserSettings$' { return 'Dao_UserSettings' }
            '^SettingsAuditLog$' { return 'Dao_SettingsAuditLog' }
            '^PackageTypeMappings$' { return 'Dao_PackageTypeMappings' }
            '^PackageType$' { return 'Dao_PackageType' }
            '^RoutingRule$' { return 'Dao_RoutingRule' }
            '^ScheduledReport$' { return 'Dao_ScheduledReport' }
            default { return "Dao_$prefix" }
        }
    }
    return "Dao_Unknown"
}

$spRootFull = Resolve-RepoPath $StoredProcedureRoot
$daoRootFull = Resolve-RepoPath $DaoRoot
$outputFull = Resolve-RepoPath $OutputPath

if (-not (Test-Path $spRootFull)) {
    throw "Stored procedure folder not found: $spRootFull"
}

$spFiles = Get-ChildItem -Path $spRootFull -Recurse -File -Filter *.sql | Sort-Object FullName

# Map stored procedure -> list of DAO file references
$daoFiles = @()
if (Test-Path $daoRootFull) {
    $daoFiles = Get-ChildItem -Path $daoRootFull -File -Filter "Dao_*.cs" | Sort-Object FullName
}

$daoFileText = @{}
foreach ($dao in $daoFiles) {
    $daoFileText[$dao.FullName] = Get-Content -Path $dao.FullName -Raw
}

$allProcedures = @()
foreach ($f in $spFiles) {
    $sqlText = Get-Content -Path $f.FullName -Raw
    $parsed = Parse-CreateProcedure $sqlText
    foreach ($p in $parsed) {
        $columnAccess = Analyze-ProcedureColumns $p.bodySql
        $allProcedures += [pscustomobject]@{
            storedProcedure = $p.storedProcedure
            parameters      = $p.parameters
            columnAccess    = $columnAccess
            sqlFile         = (Resolve-Path $f.FullName).Path
        }
    }
}

# build reference map
$refMap = @{}
foreach ($p in $allProcedures) {
    $refMap[$p.storedProcedure] = @()
}

foreach ($dao in $daoFiles) {
    $text = $daoFileText[$dao.FullName]
    $spNames = @($refMap.Keys)
    foreach ($spName in $spNames) {
        # look for "sp_Name" within quotes
        if ($text -match [regex]::Escape('"' + $spName + '"')) {
            $refMap[$spName] += $dao.Name
        }
    }
}

# compute missing suggested DAOs (file does not exist)
$existingDaoNames = @{}
foreach ($dao in $daoFiles) { $existingDaoNames[$dao.BaseName] = $true }

$result = foreach ($p in ($allProcedures | Sort-Object storedProcedure -Unique)) {
    $suggestedDao = Guess-DaoName $p.storedProcedure
    $missingDao = -not $existingDaoNames.ContainsKey($suggestedDao)

    [pscustomobject]@{
        storedProcedure  = $p.storedProcedure
        sqlFile          = ($p.sqlFile -replace "\\", "/")
        suggestedDao     = $suggestedDao
        missingDao       = $missingDao
        referencedInDaos = @($refMap[$p.storedProcedure] | Sort-Object -Unique)
        parameters       = $p.parameters
        columnAccess     = $p.columnAccess.perTable
        readColumns      = $p.columnAccess.readColumns
        writeColumns     = $p.columnAccess.writeColumns
    }
}

# summary stats
$summary = [pscustomobject]@{
    generatedAt                = (Get-Date).ToString('o')
    storedProcedureRoot        = ($spRootFull -replace "\\", "/")
    daoRoot                    = ($daoRootFull -replace "\\", "/")
    procedureCount             = @($result).Count
    referencedProcedureCount   = @($result | Where-Object { $_.referencedInDaos.Count -gt 0 }).Count
    unreferencedProcedureCount = @($result | Where-Object { $_.referencedInDaos.Count -eq 0 }).Count
    missingDaoCount            = @($result | Where-Object { $_.missingDao -eq $true }).Count
}

$outObject = [pscustomobject]@{
    summary    = $summary
    procedures = @($result)
}

# ensure output directory exists
$outDir = Split-Path -Parent $outputFull
if (-not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}

$outJson = $outObject | ConvertTo-Json -Depth 20
Set-Content -Path $outputFull -Value $outJson -Encoding UTF8

Write-Host "Wrote: $outputFull"
Write-Host ("Procedures: {0}, Referenced: {1}, Unreferenced: {2}, Missing DAO types: {3}" -f `
        $summary.procedureCount, $summary.referencedProcedureCount, $summary.unreferencedProcedureCount, $summary.missingDaoCount)
