Set-StrictMode -Version Latest

function Split-SqlIdentifierList {
    param(
        [string]$Text
    )

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return @()
    }

    return ($Text -split ',') |
    ForEach-Object {
        $_.Trim().Trim('`').Trim()
    } |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
}

function Remove-SqlComments {
    param(
        [string]$SqlText
    )

    if ([string]::IsNullOrWhiteSpace($SqlText)) {
        return [string]::Empty
    }

    $withoutBlockComments = [regex]::Replace(
        $SqlText,
        '/\*.*?\*/',
        '',
        [System.Text.RegularExpressions.RegexOptions]::Singleline
    )

    return [regex]::Replace(
        $withoutBlockComments,
        '(?m)^\s*--.*$',
        ''
    )
}

function Remove-SqlStringLiterals {
    param(
        [string]$SqlText
    )

    if ([string]::IsNullOrWhiteSpace($SqlText)) {
        return [string]::Empty
    }

    $withoutSingleQuotedStrings = [regex]::Replace(
        $SqlText,
        "'([^']|'')*'",
        "''",
        [System.Text.RegularExpressions.RegexOptions]::Singleline
    )

    return [regex]::Replace(
        $withoutSingleQuotedStrings,
        '"([^"]|"")*"',
        '""',
        [System.Text.RegularExpressions.RegexOptions]::Singleline
    )
}

function Get-SqlFileCategory {
    param(
        [string]$FilePath
    )

    $normalizedPath = $FilePath.Replace('\', '/')

    if ($normalizedPath -match '/Database/StoredProcedures/') {
        return 'stored_procedure_file'
    }

    if ($normalizedPath -match '/Database/Schemas/') {
        return 'schema_file'
    }

    if ($normalizedPath -match '/Database/SeedData/') {
        return 'seed_file'
    }

    if ($normalizedPath -match '/Database/Scripts/') {
        return 'script_file'
    }

    return 'other_sql_file'
}

function Test-SqlReservedIdentifier {
    param(
        [string]$Name
    )

    if ([string]::IsNullOrWhiteSpace($Name)) {
        return $true
    }

    $reservedIdentifiers = @(
        'AS', 'ASC', 'AFTER', 'AND', 'BEFORE', 'BY', 'CALL', 'CASE', 'COALESCE', 'COMMENT',
        'COUNT', 'CURRENT_TIMESTAMP', 'DATABASE', 'DATE', 'DELETE', 'DESC', 'DISTINCT', 'ELSE',
        'END', 'EXISTS', 'FROM', 'GROUP', 'HAVING', 'IF', 'IN', 'INSERT', 'INTO', 'JOIN',
        'LEFT', 'LIMIT', 'MAX', 'MIN', 'NOT', 'NULL', 'ON', 'OR', 'ORDER', 'RIGHT', 'SELECT',
        'SET', 'SUM', 'THEN', 'UNION', 'UPDATE', 'VALUES', 'WHEN', 'WHERE'
    )

    return $Name.ToUpperInvariant() -in $reservedIdentifiers
}

function Test-SqlSystemObjectName {
    param(
        [string]$Name
    )

    if ([string]::IsNullOrWhiteSpace($Name)) {
        return $false
    }

    $systemObjects = @('information_schema', 'mysql', 'performance_schema', 'sys', 'NEW', 'OLD')
    return $Name -in $systemObjects -or $Name.StartsWith('__system__:', [System.StringComparison]::OrdinalIgnoreCase)
}

function Test-SqlEphemeralMigrationRoutine {
    param(
        [hashtable]$Entry
    )

    return $Entry.objectType -eq 'stored_procedure' -and $Entry.relativePath -match '^Database/Schemas/.+Migration.+\.sql$'
}

function Test-SqlDerivedAliasObjectName {
    param(
        [string]$Name
    )

    return -not [string]::IsNullOrWhiteSpace($Name) -and $Name.StartsWith('__derived__:', [System.StringComparison]::OrdinalIgnoreCase)
}

function Test-SqlTemporaryObjectName {
    param(
        [string]$Name
    )

    return -not [string]::IsNullOrWhiteSpace($Name) -and $Name.StartsWith('__temp__:', [System.StringComparison]::OrdinalIgnoreCase)
}

function Resolve-SqlObjectName {
    param(
        [string]$SchemaName,
        [string]$ObjectName
    )

    if (-not [string]::IsNullOrWhiteSpace($SchemaName) -and (Test-SqlSystemObjectName -Name $SchemaName)) {
        return "__system__:$ObjectName"
    }

    if (-not [string]::IsNullOrWhiteSpace($ObjectName)) {
        return $ObjectName
    }

    return $SchemaName
}

function Get-SqlObjectName {
    param(
        [string]$SqlText
    )

    $namePatterns = @(
        'CREATE\s+(?:OR\s+REPLACE\s+)?VIEW\s+`?(?<name>[A-Za-z0-9_]+)`?',
        'CREATE\s+PROCEDURE\s+`?(?<name>[A-Za-z0-9_]+)`?',
        'CREATE\s+TABLE\s+(?:IF\s+NOT\s+EXISTS\s+)?`?(?<name>[A-Za-z0-9_]+)`?'
    )

    foreach ($pattern in $namePatterns) {
        $match = [regex]::Match(
            $SqlText,
            $pattern,
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
        )

        if ($match.Success) {
            return $match.Groups['name'].Value
        }
    }

    return $null
}

function Get-SqlObjectType {
    param(
        [string]$SqlText,
        [string]$Category
    )

    if ($SqlText -match 'CREATE\s+(?:OR\s+REPLACE\s+)?VIEW\s+') {
        return 'view'
    }

    if ($SqlText -match 'CREATE\s+PROCEDURE\s+') {
        return 'stored_procedure'
    }

    if ($SqlText -match 'CREATE\s+TABLE\s+') {
        return 'table'
    }

    if ($Category -eq 'seed_file') {
        return 'seed'
    }

    if ($Category -eq 'script_file') {
        return 'script'
    }

    return 'sql_script'
}

function Get-SqlProcedureParameters {
    param(
        [string]$SqlText
    )

    $procedureMatch = [regex]::Match(
        $SqlText,
        'CREATE\s+PROCEDURE\s+`?[A-Za-z0-9_]+`?\s*\((?<params>.*?)\)\s*(?:[A-Za-z0-9_]+\s*:\s*)?BEGIN',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Singleline
    )

    if (-not $procedureMatch.Success) {
        return @()
    }

    $rawParameters = $procedureMatch.Groups['params'].Value -split ','
    $parameters = New-Object System.Collections.Generic.List[object]

    foreach ($rawParameter in $rawParameters) {
        $trimmedParameter = $rawParameter.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmedParameter)) {
            continue
        }

        $parameterMatch = [regex]::Match(
            $trimmedParameter,
            '^(?<direction>INOUT|IN|OUT)?\s*`?(?<name>[A-Za-z0-9_]+)`?\s+(?<type>.+)$',
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
        )

        if ($parameterMatch.Success) {
            $parameters.Add([ordered]@{
                    direction = if ([string]::IsNullOrWhiteSpace($parameterMatch.Groups['direction'].Value)) { 'IN' } else { $parameterMatch.Groups['direction'].Value.ToUpperInvariant() }
                    name      = $parameterMatch.Groups['name'].Value
                    sqlType   = $parameterMatch.Groups['type'].Value.Trim()
                })
        }
    }

    return $parameters
}

function Get-SqlDeclaredVariables {
    param(
        [string]$SqlText
    )

    return [regex]::Matches(
        $SqlText,
        'DECLARE\s+`?(?<name>[A-Za-z0-9_]+)`?\s+(?<type>[A-Za-z0-9_(), ]+)',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
    ) |
    ForEach-Object {
        [ordered]@{
            name    = $_.Groups['name'].Value
            sqlType = $_.Groups['type'].Value.Trim()
        }
    }
}

function Get-SqlAliasMap {
    param(
        [string]$SqlText
    )

    $aliasMap = [ordered]@{}
    $objectPattern = '(?:FROM|JOIN)\s+(?:`?(?<schema>[A-Za-z0-9_]+)`?\.)?`?(?<table>[A-Za-z0-9_]+)`?(?:\s+(?:AS\s+)?(?<alias>[A-Za-z0-9_]+))?'
    $matches = [regex]::Matches(
        $SqlText,
        $objectPattern,
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
    )

    foreach ($match in $matches) {
        $tableName = Resolve-SqlObjectName -SchemaName $match.Groups['schema'].Value -ObjectName $match.Groups['table'].Value
        $aliasName = $match.Groups['alias'].Value

        if (-not [string]::IsNullOrWhiteSpace($tableName) -and -not (Test-SqlReservedIdentifier -Name $tableName)) {
            $aliasMap[$tableName] = $tableName
        }

        if (-not [string]::IsNullOrWhiteSpace($aliasName) -and -not (Test-SqlReservedIdentifier -Name $aliasName)) {
            $aliasMap[$aliasName] = $tableName
        }
    }

    $derivedAliasMatches = [regex]::Matches(
        $SqlText,
        '(?:FROM|JOIN)\s*\(\s*SELECT[\s\S]*?\)\s*(?:AS\s+)?(?<alias>[A-Za-z0-9_]+)',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
    )

    foreach ($match in $derivedAliasMatches) {
        $aliasName = $match.Groups['alias'].Value
        if (-not [string]::IsNullOrWhiteSpace($aliasName) -and -not (Test-SqlReservedIdentifier -Name $aliasName)) {
            $aliasMap[$aliasName] = "__derived__:$aliasName"
        }
    }

    $cteAliasMatches = [regex]::Matches(
        $SqlText,
        '(?:WITH|,)\s*(?<alias>[A-Za-z0-9_]+)\s+AS\s*\(',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
    )

    foreach ($match in $cteAliasMatches) {
        $aliasName = $match.Groups['alias'].Value
        if (-not [string]::IsNullOrWhiteSpace($aliasName) -and -not (Test-SqlReservedIdentifier -Name $aliasName)) {
            $aliasMap[$aliasName] = "__derived__:$aliasName"
        }
    }

    return $aliasMap
}

function Get-SqlReferencedObjects {
    param(
        [string]$SqlText,
        [string]$CurrentObjectName
    )

    $matches = [regex]::Matches(
        $SqlText,
        '(?:FROM|JOIN|DELETE\s+FROM)\s+(?:`?(?<schema>[A-Za-z0-9_]+)`?\.)?`?(?<name>[A-Za-z0-9_]+)`?',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
    )

    $referencedObjects = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($match in $matches) {
        $name = Resolve-SqlObjectName -SchemaName $match.Groups['schema'].Value -ObjectName $match.Groups['name'].Value
        if (-not [string]::IsNullOrWhiteSpace($name) -and $name -ne $CurrentObjectName -and -not (Test-SqlReservedIdentifier -Name $name)) {
            [void]$referencedObjects.Add($name)
        }
    }

    return @($referencedObjects | ForEach-Object { $_ })
}

function Get-SqlQualifiedColumnReferences {
    param(
        [string]$SqlText,
        [hashtable]$AliasMap
    )

    $references = New-Object System.Collections.Generic.List[object]
    $seenKeys = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)

    $matches = [regex]::Matches(
        $SqlText,
        '(?<![@A-Za-z0-9_])(?<qualifier>[A-Za-z_][A-Za-z0-9_]*)\.(?<column>[A-Za-z_][A-Za-z0-9_]*)',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
    )

    foreach ($match in $matches) {
        $qualifier = $match.Groups['qualifier'].Value
        $column = $match.Groups['column'].Value

        if (-not $AliasMap.Contains($qualifier) -and $AliasMap.Contains($column)) {
            continue
        }

        $resolvedObject = if ($AliasMap.Contains($qualifier)) { $AliasMap[$qualifier] } else { $qualifier }

        if ((Test-SqlReservedIdentifier -Name $resolvedObject) -or (Test-SqlSystemObjectName -Name $resolvedObject) -or (Test-SqlDerivedAliasObjectName -Name $resolvedObject)) {
            continue
        }

        $key = "$resolvedObject::$column"

        if ($seenKeys.Add($key)) {
            $references.Add([ordered]@{
                    qualifier      = $qualifier
                    resolvedObject = $resolvedObject
                    column         = $column
                })
        }
    }

    return $references
}

function Get-SqlInsertTargets {
    param(
        [string]$SqlText
    )

    return [regex]::Matches(
        $SqlText,
        'INSERT\s+INTO\s+(?:`?(?<schema>[A-Za-z0-9_]+)`?\.)?`?(?<table>[A-Za-z0-9_]+)`?\s*\((?<columns>.*?)\)\s*(?:VALUES|SELECT)',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Singleline
    ) |
    ForEach-Object {
        [ordered]@{
            table   = Resolve-SqlObjectName -SchemaName $_.Groups['schema'].Value -ObjectName $_.Groups['table'].Value
            columns = @(Split-SqlIdentifierList -Text $_.Groups['columns'].Value)
        }
    }
}

function Get-SqlUpdateTargets {
    param(
        [string]$SqlText,
        [hashtable]$AliasMap
    )

    $results = New-Object System.Collections.Generic.List[object]
    $matches = [regex]::Matches(
        $SqlText,
        'UPDATE\s+(?:`?(?<schema>[A-Za-z0-9_]+)`?\.)?`?(?<table>[A-Za-z0-9_]+)`?(?:\s+(?:AS\s+)?(?<alias>[A-Za-z0-9_]+))?\s+SET\s+(?<assignments>.*?)(?:WHERE|ORDER\s+BY|LIMIT|;|$)',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Singleline
    )

    foreach ($match in $matches) {
        $tableName = Resolve-SqlObjectName -SchemaName $match.Groups['schema'].Value -ObjectName $match.Groups['table'].Value
        $assignments = $match.Groups['assignments'].Value -split ','
        $columns = New-Object System.Collections.Generic.List[string]

        foreach ($assignment in $assignments) {
            $assignmentMatch = [regex]::Match(
                $assignment,
                '^(?:`?(?<qualifier>[A-Za-z0-9_]+)`?\.)?`?(?<column>[A-Za-z0-9_]+)`?\s*=',
                [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
            )

            if ($assignmentMatch.Success) {
                $columnName = $assignmentMatch.Groups['column'].Value
                if (-not [string]::IsNullOrWhiteSpace($columnName)) {
                    $columns.Add($columnName)
                }
            }
        }

        $results.Add([ordered]@{
                table   = $tableName
                columns = @($columns | Select-Object -Unique)
            })
    }

    return $results
}

function Get-SqlDeleteTargets {
    param(
        [string]$SqlText
    )

    return [regex]::Matches(
        $SqlText,
        'DELETE\s+FROM\s+(?:`?(?<schema>[A-Za-z0-9_]+)`?\.)?`?(?<table>[A-Za-z0-9_]+)`?',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
    ) |
    ForEach-Object {
        [ordered]@{ table = Resolve-SqlObjectName -SchemaName $_.Groups['schema'].Value -ObjectName $_.Groups['table'].Value }
    }
}

function Get-SqlTemporaryTables {
    param(
        [string]$SqlText
    )

    return [regex]::Matches(
        $SqlText,
        'CREATE\s+TEMPORARY\s+TABLE\s+(?:IF\s+NOT\s+EXISTS\s+)?`?(?<name>[A-Za-z0-9_]+)`?',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
    ) |
    ForEach-Object {
        "__temp__:$($_.Groups['name'].Value)"
    }
}

function Get-SqlRoutineCalls {
    param(
        [string]$SqlText
    )

    $matches = [regex]::Matches(
        $SqlText,
        'CALL\s+`?(?<name>[A-Za-z0-9_]+)`?',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
    )

    $calledRoutines = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($match in $matches) {
        [void]$calledRoutines.Add($match.Groups['name'].Value)
    }

    return @($calledRoutines | ForEach-Object { $_ })
}

function Get-SqlTokenUsages {
    param(
        [string]$SqlText,
        [string]$Prefix
    )

    $tokens = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)
    $pattern = "\b$([regex]::Escape($Prefix))[A-Za-z0-9_]+\b"

    foreach ($match in [regex]::Matches($SqlText, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
        [void]$tokens.Add($match.Value)
    }

    return @($tokens | ForEach-Object { $_ })
}

function ConvertTo-SqlCatalogEntry {
    param(
        [string]$RepoRoot,
        [string]$FilePath
    )

    $sqlText = Get-Content -Path $FilePath -Raw
    $normalizedSqlText = Remove-SqlComments -SqlText $sqlText
    $analysisSqlText = Remove-SqlStringLiterals -SqlText $normalizedSqlText
    $category = Get-SqlFileCategory -FilePath $FilePath
    $objectName = Get-SqlObjectName -SqlText $normalizedSqlText
    $objectType = Get-SqlObjectType -SqlText $normalizedSqlText -Category $category
    $aliasMap = Get-SqlAliasMap -SqlText $analysisSqlText
    $parameters = @(Get-SqlProcedureParameters -SqlText $normalizedSqlText)
    $declaredVariables = @(Get-SqlDeclaredVariables -SqlText $normalizedSqlText)
    $temporaryTables = @(Get-SqlTemporaryTables -SqlText $analysisSqlText)
    $relativePath = [System.IO.Path]::GetRelativePath($RepoRoot, $FilePath).Replace('\', '/')

    foreach ($temporaryTable in $temporaryTables) {
        $aliasMap[$temporaryTable.Substring('__temp__:'.Length)] = $temporaryTable
    }

    if ($objectType -eq 'table') {
        return [ordered]@{
            relativePath        = $relativePath
            category            = $category
            objectType          = $objectType
            objectName          = $objectName
            parameters          = @()
            declaredVariables   = @()
            referencedObjects   = @()
            qualifiedColumns    = @()
            insertTargets       = @()
            updateTargets       = @()
            deleteTargets       = @()
            calledRoutines      = @()
            parameterTokensUsed = @()
            variableTokensUsed  = @()
            aliasMap            = @()
        }
    }

    return [ordered]@{
        relativePath        = $relativePath
        category            = $category
        objectType          = $objectType
        objectName          = $objectName
        parameters          = $parameters
        declaredVariables   = $declaredVariables
        referencedObjects   = @(Get-SqlReferencedObjects -SqlText $analysisSqlText -CurrentObjectName $objectName)
        qualifiedColumns    = @(Get-SqlQualifiedColumnReferences -SqlText $analysisSqlText -AliasMap $aliasMap)
        insertTargets       = @(Get-SqlInsertTargets -SqlText $analysisSqlText)
        updateTargets       = @(Get-SqlUpdateTargets -SqlText $analysisSqlText -AliasMap $aliasMap)
        deleteTargets       = @(Get-SqlDeleteTargets -SqlText $analysisSqlText)
        calledRoutines      = @(Get-SqlRoutineCalls -SqlText $analysisSqlText)
        parameterTokensUsed = @(Get-SqlTokenUsages -SqlText $analysisSqlText -Prefix 'p_')
        variableTokensUsed  = @(Get-SqlTokenUsages -SqlText $analysisSqlText -Prefix 'v_')
        temporaryTables     = $temporaryTables
        aliasMap            = @($aliasMap.GetEnumerator() | ForEach-Object {
                [ordered]@{
                    alias  = $_.Key
                    object = $_.Value
                }
            })
    }
}

function Get-TrackedSqlFiles {
    param(
        [string]$DatabaseRoot
    )

    $includeRoots = @(
        (Join-Path $DatabaseRoot 'Schemas'),
        (Join-Path $DatabaseRoot 'StoredProcedures'),
        (Join-Path $DatabaseRoot 'SeedData'),
        (Join-Path $DatabaseRoot 'Scripts')
    )

    $sqlFiles = New-Object System.Collections.Generic.List[string]

    foreach ($includeRoot in $includeRoots) {
        if (Test-Path $includeRoot) {
            Get-ChildItem -Path $includeRoot -Filter '*.sql' -Recurse -File | ForEach-Object {
                $sqlFiles.Add($_.FullName)
            }
        }
    }

    return @($sqlFiles | Sort-Object -Unique)
}

function New-SqlObjectCatalog {
    param(
        [string]$RepoRoot,
        [string]$DatabaseRoot
    )

    $entries = Get-TrackedSqlFiles -DatabaseRoot $DatabaseRoot |
    ForEach-Object {
        ConvertTo-SqlCatalogEntry -RepoRoot $RepoRoot -FilePath $_
    }

    return [ordered]@{
        generatedAt  = (Get-Date).ToString('o')
        databaseRoot = [System.IO.Path]::GetRelativePath($RepoRoot, $DatabaseRoot).Replace('\', '/')
        entries      = @($entries)
    }
}

function Save-SqlObjectCatalog {
    param(
        [string]$RepoRoot,
        [string]$DatabaseRoot,
        [string]$OutputPath
    )

    $catalog = New-SqlObjectCatalog -RepoRoot $RepoRoot -DatabaseRoot $DatabaseRoot
    $json = $catalog | ConvertTo-Json -Depth 100
    [System.IO.File]::WriteAllText($OutputPath, $json, [System.Text.UTF8Encoding]::new($false))
    return $catalog
}

function New-SqlDeployDefaultsFile {
    param(
        [string]$User,
        [string]$Password
    )

    $defaultsFile = [System.IO.Path]::ChangeExtension([System.IO.Path]::GetTempFileName(), '.cnf')
    $content = @"
[client]
user=$User
password=$Password
"@

    [System.IO.File]::WriteAllText($defaultsFile, $content, [System.Text.UTF8Encoding]::new($false))
    return $defaultsFile
}

function Invoke-MySqlTextQuery {
    param(
        [string]$MySqlExe,
        [string]$Server,
        [string]$Port,
        [string]$Database,
        [string]$User,
        [string]$Password,
        [string]$Query
    )

    $defaultsFile = $null

    try {
        $defaultsFile = New-SqlDeployDefaultsFile -User $User -Password $Password
        $arguments = @(
            "--defaults-extra-file=$defaultsFile",
            '--batch',
            '--raw',
            '--skip-column-names',
            "-h$Server",
            "-P$Port",
            $Database,
            '-e',
            $Query
        )

        $psi = New-Object System.Diagnostics.ProcessStartInfo
        $psi.FileName = $MySqlExe
        foreach ($argument in $arguments) {
            [void]$psi.ArgumentList.Add($argument)
        }
        $psi.RedirectStandardOutput = $true
        $psi.RedirectStandardError = $true
        $psi.UseShellExecute = $false
        $psi.CreateNoWindow = $true

        $process = New-Object System.Diagnostics.Process
        $process.StartInfo = $psi
        $process.Start() | Out-Null
        $stdout = $process.StandardOutput.ReadToEnd()
        $stderr = $process.StandardError.ReadToEnd()
        $process.WaitForExit()

        if ($process.ExitCode -ne 0) {
            throw "MySQL query failed: $stderr"
        }

        return $stdout
    }
    finally {
        if ($null -ne $defaultsFile -and (Test-Path $defaultsFile -ErrorAction SilentlyContinue)) {
            Remove-Item $defaultsFile -ErrorAction SilentlyContinue
        }
    }
}

function Get-DatabaseSchemaSnapshot {
    param(
        [string]$MySqlExe,
        [string]$Server,
        [string]$Port,
        [string]$Database,
        [string]$User,
        [string]$Password
    )

    $tableRows = Invoke-MySqlTextQuery -MySqlExe $MySqlExe -Server $Server -Port $Port -Database $Database -User $User -Password $Password -Query @"
SELECT TABLE_NAME, TABLE_TYPE
FROM information_schema.tables
WHERE table_schema = DATABASE()
ORDER BY TABLE_NAME;
"@

    $columnRows = Invoke-MySqlTextQuery -MySqlExe $MySqlExe -Server $Server -Port $Port -Database $Database -User $User -Password $Password -Query @"
SELECT TABLE_NAME, COLUMN_NAME
FROM information_schema.columns
WHERE table_schema = DATABASE()
ORDER BY TABLE_NAME, ORDINAL_POSITION;
"@

    $routineRows = Invoke-MySqlTextQuery -MySqlExe $MySqlExe -Server $Server -Port $Port -Database $Database -User $User -Password $Password -Query @"
SELECT ROUTINE_NAME, ROUTINE_TYPE
FROM information_schema.routines
WHERE routine_schema = DATABASE()
ORDER BY ROUTINE_NAME;
"@

    $tables = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)
    $views = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)
    $columnsByObject = @{}
    $routines = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($row in ($tableRows -split "`r?`n" | Where-Object { $_.Trim() })) {
        $parts = $row -split "`t"
        if ($parts.Count -ge 2) {
            if ($parts[1] -eq 'VIEW') {
                [void]$views.Add($parts[0])
            }
            else {
                [void]$tables.Add($parts[0])
            }
        }
    }

    foreach ($row in ($columnRows -split "`r?`n" | Where-Object { $_.Trim() })) {
        $parts = $row -split "`t"
        if ($parts.Count -ge 2) {
            if (-not $columnsByObject.ContainsKey($parts[0])) {
                $columnsByObject[$parts[0]] = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)
            }
            [void]$columnsByObject[$parts[0]].Add($parts[1])
        }
    }

    foreach ($row in ($routineRows -split "`r?`n" | Where-Object { $_.Trim() })) {
        $parts = $row -split "`t"
        if ($parts.Count -ge 1) {
            [void]$routines.Add($parts[0])
        }
    }

    return [ordered]@{
        tables          = $tables
        views           = $views
        columnsByObject = $columnsByObject
        routines        = $routines
    }
}

function Test-SqlCatalogEntryAgainstSnapshot {
    param(
        [hashtable]$Entry,
        [hashtable]$Snapshot,
        [hashtable]$CatalogIndex
    )

    $issues = New-Object System.Collections.Generic.List[object]

    $knownObjects = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($table in $Snapshot.tables) { [void]$knownObjects.Add($table) }
    foreach ($view in $Snapshot.views) { [void]$knownObjects.Add($view) }

    if ($Entry.objectType -eq 'table' -and -not [string]::IsNullOrWhiteSpace($Entry.objectName) -and -not $Snapshot.tables.Contains($Entry.objectName)) {
        $issues.Add([ordered]@{ severity = 'error'; code = 'TABLE_NOT_DEPLOYED'; message = "Table '$($Entry.objectName)' was not found after deployment." })
    }

    if ($Entry.objectType -eq 'view' -and -not [string]::IsNullOrWhiteSpace($Entry.objectName) -and -not $Snapshot.views.Contains($Entry.objectName)) {
        $issues.Add([ordered]@{ severity = 'error'; code = 'VIEW_NOT_DEPLOYED'; message = "View '$($Entry.objectName)' was not found after deployment." })
    }

    if ($Entry.objectType -eq 'stored_procedure' -and -not [string]::IsNullOrWhiteSpace($Entry.objectName) -and -not $Snapshot.routines.Contains($Entry.objectName) -and -not (Test-SqlEphemeralMigrationRoutine -Entry $Entry)) {
        $issues.Add([ordered]@{ severity = 'error'; code = 'ROUTINE_NOT_DEPLOYED'; message = "Stored procedure '$($Entry.objectName)' was not found after deployment." })
    }

    foreach ($referencedObject in $Entry.referencedObjects) {
        if ((Test-SqlSystemObjectName -Name $referencedObject) -or (Test-SqlDerivedAliasObjectName -Name $referencedObject) -or (("__temp__:$referencedObject") -in @($Entry.temporaryTables))) {
            continue
        }

        if (-not $knownObjects.Contains($referencedObject) -and -not $CatalogIndex.ContainsKey($referencedObject)) {
            $issues.Add([ordered]@{ severity = 'error'; code = 'OBJECT_NOT_FOUND'; message = "Referenced object '$referencedObject' was not found in the deployed schema or catalog." })
        }
    }

    foreach ($qualifiedColumn in $Entry.qualifiedColumns) {
        $resolvedObject = $qualifiedColumn.resolvedObject
        if ((Test-SqlSystemObjectName -Name $resolvedObject) -or (Test-SqlDerivedAliasObjectName -Name $resolvedObject)) {
            continue
        }

        if ($Snapshot.columnsByObject.ContainsKey($resolvedObject)) {
            if (-not $Snapshot.columnsByObject[$resolvedObject].Contains($qualifiedColumn.column)) {
                $issues.Add([ordered]@{ severity = 'error'; code = 'COLUMN_NOT_FOUND'; message = "Column '$($qualifiedColumn.column)' was not found on '$resolvedObject'." })
            }
        }
        elseif (-not $CatalogIndex.ContainsKey($resolvedObject)) {
            $issues.Add([ordered]@{ severity = 'warning'; code = 'COLUMN_OBJECT_UNKNOWN'; message = "Unable to validate column '$($qualifiedColumn.column)' because object '$resolvedObject' is unknown." })
        }
    }

    foreach ($insertTarget in $Entry.insertTargets) {
        if (("__temp__:$($insertTarget.table)") -in @($Entry.temporaryTables) -or (Test-SqlTemporaryObjectName -Name $insertTarget.table)) {
            continue
        }

        if ($Snapshot.columnsByObject.ContainsKey($insertTarget.table)) {
            foreach ($column in $insertTarget.columns) {
                if (-not $Snapshot.columnsByObject[$insertTarget.table].Contains($column)) {
                    $issues.Add([ordered]@{ severity = 'error'; code = 'INSERT_COLUMN_NOT_FOUND'; message = "Insert target column '$column' was not found on '$($insertTarget.table)'." })
                }
            }
        }
        else {
            $issues.Add([ordered]@{ severity = 'error'; code = 'INSERT_TARGET_NOT_FOUND'; message = "Insert target table '$($insertTarget.table)' was not found." })
        }
    }

    foreach ($updateTarget in $Entry.updateTargets) {
        if (("__temp__:$($updateTarget.table)") -in @($Entry.temporaryTables) -or (Test-SqlTemporaryObjectName -Name $updateTarget.table)) {
            continue
        }

        if ($Snapshot.columnsByObject.ContainsKey($updateTarget.table)) {
            foreach ($column in $updateTarget.columns) {
                if (-not $Snapshot.columnsByObject[$updateTarget.table].Contains($column)) {
                    $issues.Add([ordered]@{ severity = 'error'; code = 'UPDATE_COLUMN_NOT_FOUND'; message = "Update column '$column' was not found on '$($updateTarget.table)'." })
                }
            }
        }
        else {
            $issues.Add([ordered]@{ severity = 'error'; code = 'UPDATE_TARGET_NOT_FOUND'; message = "Update target '$($updateTarget.table)' was not found." })
        }
    }

    foreach ($deleteTarget in $Entry.deleteTargets) {
        if (("__temp__:$($deleteTarget.table)") -in @($Entry.temporaryTables) -or (Test-SqlTemporaryObjectName -Name $deleteTarget.table)) {
            continue
        }

        if (-not $knownObjects.Contains($deleteTarget.table)) {
            $issues.Add([ordered]@{ severity = 'error'; code = 'DELETE_TARGET_NOT_FOUND'; message = "Delete target '$($deleteTarget.table)' was not found." })
        }
    }

    $declaredParameterNames = @($Entry.parameters | ForEach-Object { $_.name })
    foreach ($parameterToken in $Entry.parameterTokensUsed) {
        if ($parameterToken -notin $declaredParameterNames) {
            $issues.Add([ordered]@{ severity = 'warning'; code = 'UNDECLARED_PARAMETER_TOKEN'; message = "Token '$parameterToken' is referenced but not declared as a procedure parameter." })
        }
    }

    $declaredVariableNames = @($Entry.declaredVariables | ForEach-Object { $_.name })
    foreach ($variableToken in $Entry.variableTokensUsed) {
        if ($variableToken -notin $declaredVariableNames -and $variableToken -notin $declaredParameterNames) {
            $issues.Add([ordered]@{ severity = 'warning'; code = 'UNDECLARED_VARIABLE_TOKEN'; message = "Token '$variableToken' is referenced but not declared in the routine." })
        }
    }

    foreach ($calledRoutine in $Entry.calledRoutines) {
        if (Test-SqlEphemeralMigrationRoutine -Entry $Entry) {
            continue
        }

        if (-not $Snapshot.routines.Contains($calledRoutine) -and -not $CatalogIndex.ContainsKey($calledRoutine)) {
            $issues.Add([ordered]@{ severity = 'error'; code = 'CALLED_ROUTINE_NOT_FOUND'; message = "Called routine '$calledRoutine' was not found in the deployed schema or catalog." })
        }
    }

    return @($issues | ForEach-Object { $_ })
}

function Test-SqlObjectCatalogAgainstDatabase {
    param(
        [hashtable]$Catalog,
        [string]$MySqlExe,
        [string]$Server,
        [string]$Port,
        [string]$Database,
        [string]$User,
        [string]$Password
    )

    $snapshot = Get-DatabaseSchemaSnapshot -MySqlExe $MySqlExe -Server $Server -Port $Port -Database $Database -User $User -Password $Password
    $catalogIndex = @{}

    foreach ($entry in $Catalog.entries) {
        if (-not [string]::IsNullOrWhiteSpace($entry.objectName)) {
            $catalogIndex[$entry.objectName] = $entry
        }
    }

    $results = New-Object System.Collections.Generic.List[object]

    foreach ($entry in $Catalog.entries) {
        $issues = @(Test-SqlCatalogEntryAgainstSnapshot -Entry $entry -Snapshot $snapshot -CatalogIndex $catalogIndex)
        $results.Add([ordered]@{
                relativePath = $entry.relativePath
                objectType   = $entry.objectType
                objectName   = $entry.objectName
                issueCount   = $issues.Count
                issues       = $issues
            })
    }

    return @{
        generatedAt = (Get-Date).ToString('o')
        database    = $Database
        resultCount = $results.Count
        results     = @($results | ForEach-Object { $_ })
    }
}

function New-ValidationOutputPaths {
    param(
        [string]$OutputsDirectory
    )

    if (-not (Test-Path $OutputsDirectory)) {
        New-Item -Path $OutputsDirectory -ItemType Directory -Force | Out-Null
    }

    $timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    return [ordered]@{
        markdown = Join-Path $OutputsDirectory "sql-validation-$timestamp.md"
        json     = Join-Path $OutputsDirectory "sql-validation-$timestamp.json"
    }
}

function Save-SqlValidationReport {
    param(
        [object]$ValidationResult,
        [string]$MarkdownPath,
        [string]$JsonPath
    )

    $json = $ValidationResult | ConvertTo-Json -Depth 100
    [System.IO.File]::WriteAllText($JsonPath, $json, [System.Text.UTF8Encoding]::new($false))

    $markdown = New-Object System.Text.StringBuilder
    [void]$markdown.AppendLine('# SQL Validation Report')
    [void]$markdown.AppendLine()
    [void]$markdown.AppendLine("Generated: $($ValidationResult.generatedAt)")
    [void]$markdown.AppendLine("Database: $($ValidationResult.database)")
    [void]$markdown.AppendLine()

    $errorCount = ($ValidationResult.results | ForEach-Object { @($_.issues | Where-Object { $_.severity -eq 'error' }).Count } | Measure-Object -Sum).Sum
    $warningCount = ($ValidationResult.results | ForEach-Object { @($_.issues | Where-Object { $_.severity -eq 'warning' }).Count } | Measure-Object -Sum).Sum

    [void]$markdown.AppendLine("- Files validated: $($ValidationResult.resultCount)")
    [void]$markdown.AppendLine("- Errors: $errorCount")
    [void]$markdown.AppendLine("- Warnings: $warningCount")
    [void]$markdown.AppendLine()

    foreach ($result in $ValidationResult.results | Where-Object { $_.issueCount -gt 0 }) {
        [void]$markdown.AppendLine("## $($result.relativePath)")
        [void]$markdown.AppendLine()
        [void]$markdown.AppendLine("- Object: $($result.objectName)")
        [void]$markdown.AppendLine("- Type: $($result.objectType)")
        [void]$markdown.AppendLine("- Issues: $($result.issueCount)")
        [void]$markdown.AppendLine()

        foreach ($issue in $result.issues) {
            [void]$markdown.AppendLine("- [$($issue.severity.ToUpperInvariant())] $($issue.code): $($issue.message)")
        }

        [void]$markdown.AppendLine()
    }

    if (($ValidationResult.results | Where-Object { $_.issueCount -gt 0 }).Count -eq 0) {
        [void]$markdown.AppendLine('No validation issues were found.')
        [void]$markdown.AppendLine()
    }

    [System.IO.File]::WriteAllText($MarkdownPath, $markdown.ToString(), [System.Text.UTF8Encoding]::new($false))
}
