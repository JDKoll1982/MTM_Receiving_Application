Set-StrictMode -Version Latest

function Remove-CSharpComments {
    param(
        [string]$SourceText
    )

    if ([string]::IsNullOrEmpty($SourceText)) {
        return [string]::Empty
    }

    $builder = New-Object System.Text.StringBuilder
    $index = 0
    $length = $SourceText.Length
    $inLineComment = $false
    $inBlockComment = $false
    $inString = $false
    $inVerbatimString = $false
    $inChar = $false
    $escapeNext = $false

    while ($index -lt $length) {
        $current = $SourceText[$index]
        $next = if (($index + 1) -lt $length) { $SourceText[$index + 1] } else { [char]0 }

        if ($inLineComment) {
            if ($current -eq "`r" -or $current -eq "`n") {
                $inLineComment = $false
                [void]$builder.Append($current)
            }

            $index++
            continue
        }

        if ($inBlockComment) {
            if ($current -eq '*' -and $next -eq '/') {
                $inBlockComment = $false
                $index += 2
                continue
            }

            if ($current -eq "`r" -or $current -eq "`n") {
                [void]$builder.Append($current)
            }

            $index++
            continue
        }

        if ($inString) {
            [void]$builder.Append($current)

            if ($inVerbatimString) {
                if ($current -eq '"' -and $next -eq '"') {
                    [void]$builder.Append($next)
                    $index += 2
                    continue
                }

                if ($current -eq '"') {
                    $inString = $false
                    $inVerbatimString = $false
                }

                $index++
                continue
            }

            if (-not $escapeNext -and $current -eq '\\') {
                $escapeNext = $true
                $index++
                continue
            }

            if (-not $escapeNext -and $current -eq '"') {
                $inString = $false
            }

            $escapeNext = $false
            $index++
            continue
        }

        if ($inChar) {
            [void]$builder.Append($current)

            if (-not $escapeNext -and $current -eq '\\') {
                $escapeNext = $true
                $index++
                continue
            }

            if (-not $escapeNext -and $current -eq "'") {
                $inChar = $false
            }

            $escapeNext = $false
            $index++
            continue
        }

        if ($current -eq '/' -and $next -eq '/') {
            $inLineComment = $true
            $index += 2
            continue
        }

        if ($current -eq '/' -and $next -eq '*') {
            $inBlockComment = $true
            $index += 2
            continue
        }

        if ($current -eq '@' -and $next -eq '"') {
            $inString = $true
            $inVerbatimString = $true
            [void]$builder.Append($current)
            [void]$builder.Append($next)
            $index += 2
            continue
        }

        if (($current -eq '$' -and $next -eq '"') -or ($current -eq '$' -and $next -eq '@' -and ($index + 2) -lt $length -and $SourceText[$index + 2] -eq '"')) {
            $inString = $true
            $inVerbatimString = ($next -eq '@')
            [void]$builder.Append($current)
            [void]$builder.Append($next)

            if ($next -eq '@') {
                [void]$builder.Append($SourceText[$index + 2])
                $index += 3
            }
            else {
                $index += 2
            }

            continue
        }

        if ($current -eq '@' -and $next -eq '$' -and ($index + 2) -lt $length -and $SourceText[$index + 2] -eq '"') {
            $inString = $true
            $inVerbatimString = $true
            [void]$builder.Append($current)
            [void]$builder.Append($next)
            [void]$builder.Append($SourceText[$index + 2])
            $index += 3
            continue
        }

        if ($current -eq '"') {
            $inString = $true
            $inVerbatimString = $false
            [void]$builder.Append($current)
            $index++
            continue
        }

        if ($current -eq "'") {
            $inChar = $true
            [void]$builder.Append($current)
            $index++
            continue
        }

        [void]$builder.Append($current)
        $index++
    }

    return $builder.ToString()
}

function Get-CSharpStoredProcedureReferencePatterns {
    return @(
        [ordered]@{
            name        = 'Helper_Database_StoredProcedure'
            description = 'Helper_Database_StoredProcedure.Execute* call with stored procedure string literal'
            pattern     = 'Helper_Database_StoredProcedure\.[A-Za-z0-9_]+\s*\(\s*[\s\S]{0,250}?"(?<name>sp_[A-Za-z0-9_]+)"'
        },
        [ordered]@{
            name        = 'MySqlCommand_Constructor'
            description = 'new MySqlCommand("sp_name", connection) direct stored procedure constructor'
            pattern     = 'new\s+MySqlCommand\s*\(\s*"(?<name>sp_[A-Za-z0-9_]+)"\s*,'
        }
    )
}

function Get-CodebaseReferenceFiles {
    param(
        [string]$RepoRoot,
        [string[]]$IncludedExtensions,
        [string[]]$ExcludedRelativePrefixes
    )

    $normalizedExcludedPrefixes = @(
        $ExcludedRelativePrefixes |
        ForEach-Object {
            $_.Replace('\', '/').TrimStart('/').TrimEnd('/') + '/'
        }
    )

    return @(
        Get-ChildItem -Path $RepoRoot -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object {
            $extension = $_.Extension.ToLowerInvariant()
            if ($extension -notin $IncludedExtensions) {
                return $false
            }

            $relativePath = [System.IO.Path]::GetRelativePath($RepoRoot, $_.FullName).Replace('\', '/')
            foreach ($excludedPrefix in $normalizedExcludedPrefixes) {
                if ($relativePath.StartsWith($excludedPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
                    return $false
                }
            }

            return $true
        }
    )
}

function Get-CodebaseStoredProcedureReferenceIndex {
    param(
        [string]$RepoRoot,
        [string[]]$ProcedureNames,
        [string[]]$IncludedExtensions = @('.cs'),
        [string[]]$ExcludedRelativePrefixes = @('Database/Database_Deployment', 'bin', 'obj', '.git', '.vs', '.serena', 'docs/CopilotForms/outputs')
    )

    $uniqueProcedureNames = @(
        $ProcedureNames |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Sort-Object -Unique
    )

    $referenceIndex = @{}
    foreach ($procedureName in $uniqueProcedureNames) {
        $referenceIndex[$procedureName] = [ordered]@{
            totalReferenceCount = 0
            fileCount           = 0
            references          = @()
        }
    }

    if ($uniqueProcedureNames.Count -eq 0) {
        return $referenceIndex
    }

    $nameLookup = @{}
    foreach ($procedureName in $uniqueProcedureNames) {
        $nameLookup[$procedureName.ToLowerInvariant()] = $procedureName
    }

    $referencePatterns = @(
        Get-CSharpStoredProcedureReferencePatterns |
        ForEach-Object {
            [ordered]@{
                name        = $_.name
                description = $_.description
                regex       = [regex]::new($_.pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
            }
        }
    )

    foreach ($file in Get-CodebaseReferenceFiles -RepoRoot $RepoRoot -IncludedExtensions $IncludedExtensions -ExcludedRelativePrefixes $ExcludedRelativePrefixes) {
        $content = $null

        try {
            $content = Get-Content -Path $file.FullName -Raw -ErrorAction Stop
        }
        catch {
            continue
        }

        if ([string]::IsNullOrWhiteSpace($content)) {
            continue
        }

        $scanContent = Remove-CSharpComments -SourceText $content

        $relativePath = [System.IO.Path]::GetRelativePath($RepoRoot, $file.FullName).Replace('\', '/')
        $countsByProcedure = @{}
        $patternsByProcedure = @{}

        foreach ($referencePattern in $referencePatterns) {
            $matches = $referencePattern.regex.Matches($scanContent)
            foreach ($match in $matches) {
                $matchedName = $match.Groups['name'].Value
                if ([string]::IsNullOrWhiteSpace($matchedName)) {
                    continue
                }

                $lookupKey = $matchedName.ToLowerInvariant()
                if (-not $nameLookup.ContainsKey($lookupKey)) {
                    continue
                }

                $canonicalName = $nameLookup[$lookupKey]

                if (-not $countsByProcedure.ContainsKey($canonicalName)) {
                    $countsByProcedure[$canonicalName] = 0
                    $patternsByProcedure[$canonicalName] = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)
                }

                $countsByProcedure[$canonicalName]++
                [void]$patternsByProcedure[$canonicalName].Add($referencePattern.name)
            }
        }

        if ($countsByProcedure.Count -eq 0) {
            continue
        }

        foreach ($canonicalName in $countsByProcedure.Keys) {
            $referenceIndex[$canonicalName].totalReferenceCount += $countsByProcedure[$canonicalName]
            $referenceIndex[$canonicalName].fileCount += 1
            $referenceIndex[$canonicalName].references += [ordered]@{
                relativePath = $relativePath
                count        = $countsByProcedure[$canonicalName]
                patterns     = @($patternsByProcedure[$canonicalName] | Sort-Object)
            }
        }
    }

    return $referenceIndex
}