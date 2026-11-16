<#
.SYNOPSIS
Shared helpers for the MTM application version release scripts.

.DESCRIPTION
Dot-source this file from the other scripts in this folder:
. (Join-Path $PSScriptRoot 'MtmVersionShared.ps1')
It is a helper library, not a command.

.NOTES
Last Updated: 2026-11-16
#>

function Get-MtmVersionSourceDefinition {
    [OutputType([hashtable[]])]
    param()

    return @(
        @{
            Name         = 'appsettings.json | Application.Version'
            RelativePath = 'appsettings.json'
            Shape        = '3-part'
            ReadPattern  = '"Application"\s*:\s*\{[^}]*?"Version"\s*:\s*"(?<value>\d+\.\d+\.\d+)"'
            WritePattern = '("Application"\s*:\s*\{[^}]*?"Version"\s*:\s*")\d+\.\d+\.\d+(?:\.\d+)?(")'
        }
        @{
            Name         = '05_seed_software_version.sql | required_version'
            RelativePath = 'Database/Database_Deployment/Sql_Files/SeedData/05_seed_software_version.sql'
            Shape        = '3-part'
            ReadPattern  = "VALUES\s*\(\s*\d+\s*,\s*'(?<value>\d+\.\d+\.\d+)'"
            WritePattern = "(VALUES\s*\(\s*\d+\s*,\s*')\d+\.\d+\.\d+(?:\.\d+)?(')"
        }
        @{
            Name         = 'MTM_Receiving_Application.csproj | <Version>'
            RelativePath = 'MTM_Receiving_Application.csproj'
            Shape        = '4-part'
            ReadPattern  = '<Version>\s*(?<value>\d+\.\d+\.\d+\.\d+)\s*</Version>'
            WritePattern = '(<Version>)\d+\.\d+\.\d+(?:\.\d+)?(</Version>)'
        }
        @{
            Name         = 'MTM_Receiving_Application.csproj | <AssemblyVersion>'
            RelativePath = 'MTM_Receiving_Application.csproj'
            Shape        = '4-part'
            ReadPattern  = '<AssemblyVersion>\s*(?<value>\d+\.\d+\.\d+\.\d+)\s*</AssemblyVersion>'
            WritePattern = '(<AssemblyVersion>)\d+\.\d+\.\d+(?:\.\d+)?(</AssemblyVersion>)'
        }
        @{
            Name         = 'MTM_Receiving_Application.csproj | <FileVersion>'
            RelativePath = 'MTM_Receiving_Application.csproj'
            Shape        = '4-part'
            ReadPattern  = '<FileVersion>\s*(?<value>\d+\.\d+\.\d+\.\d+)\s*</FileVersion>'
            WritePattern = '(<FileVersion>)\d+\.\d+\.\d+(?:\.\d+)?(</FileVersion>)'
        }
        @{
            Name         = 'appxmanifest.xml | <Identity Version>'
            RelativePath = 'appxmanifest.xml'
            Shape        = '4-part'
            ReadPattern  = '<Identity\b[^>]*?\bVersion\s*=\s*"(?<value>\d+\.\d+\.\d+\.\d+)"'
            WritePattern = '(<Identity\b[^>]*?\bVersion\s*=\s*")\d+\.\d+\.\d+(?:\.\d+)?(")'
        }
        @{
            Name         = 'Package.appxmanifest | <Identity Version>'
            RelativePath = 'Package.appxmanifest'
            Shape        = '4-part'
            ReadPattern  = '<Identity\b[^>]*?\bVersion\s*=\s*"(?<value>\d+\.\d+\.\d+\.\d+)"'
            WritePattern = '(<Identity\b[^>]*?\bVersion\s*=\s*")\d+\.\d+\.\d+(?:\.\d+)?(")'
        }
    )
}

function Get-MtmVersionKnownPath {
    [OutputType([string[]])]
    param()

    return @(
        'appsettings.json'
        'Database/Database_Deployment/Sql_Files/SeedData/05_seed_software_version.sql'
        'MTM_Receiving_Application.csproj'
        'appxmanifest.xml'
        'Package.appxmanifest'
    )
}

function Get-MtmVersionExcludePattern {
    [OutputType([string[]])]
    param()

    return @(
        '^\.git/'
        '^\.github/'
        '^docs/updates/'
        '^bin/'
        '^obj/'
        '^TestResults/'
        '(^|/)(bin|obj|\.vs|TestResults)/'
    )
}

function Read-MtmVersionText {
    param(
        [Parameter(Mandatory)]
        [string]$LiteralPath
    )

    if (-not (Test-Path -LiteralPath $LiteralPath -PathType Leaf)) {
        return $null
    }

    return [System.IO.File]::ReadAllText($LiteralPath)
}

function Get-MtmVersionValue {
    param(
        [AllowNull()]
        [string]$Content,

        [Parameter(Mandatory)]
        [string]$Pattern
    )

    if ([string]::IsNullOrEmpty($Content)) {
        return $null
    }

    $match = [System.Text.RegularExpressions.Regex]::Match($Content, $Pattern)
    if (-not $match.Success) {
        return $null
    }

    return $match.Groups['value'].Value
}

function Get-MtmVersionApplicationValue {
    param(
        [Parameter(Mandatory)]
        [string]$RootPath
    )

    $content = Read-MtmVersionText -LiteralPath (Join-Path $RootPath 'appsettings.json')
    if ([string]::IsNullOrEmpty($content)) {
        return $null
    }

    $blockMatch = [System.Text.RegularExpressions.Regex]::Match($content, '"Application"\s*:\s*\{(?<block>[^}]*)\}')
    if ($blockMatch.Success) {
        $value = Get-MtmVersionValue -Content $blockMatch.Groups['block'].Value -Pattern '"Version"\s*:\s*"(?<value>\d+\.\d+\.\d+)"'
        if (-not [string]::IsNullOrEmpty($value)) {
            return $value
        }
    }

    return Get-MtmVersionValue -Content $content -Pattern '"Version"\s*:\s*"(?<value>\d+\.\d+\.\d+)"'
}

function Test-MtmVersionPathExcluded {
    param(
        [Parameter(Mandatory)]
        [string]$RelativePath,

        [Parameter(Mandatory)]
        [string[]]$ExcludePattern
    )

    foreach ($pattern in $ExcludePattern) {
        if ($RelativePath -match $pattern) {
            return $true
        }
    }

    return $false
}

function Find-MtmVersionLiteralHit {
    param(
        [Parameter(Mandatory)]
        [string]$RootPath,

        [Parameter(Mandatory)]
        [string]$Literal,

        [Parameter(Mandatory)]
        [string[]]$ExcludePattern
    )

    $hits = [System.Collections.Generic.List[object]]::new()
    $gitSucceeded = $false
    $raw = @()

    Push-Location -LiteralPath $RootPath
    try {
        $raw = & git grep --no-color -n -I -F -e $Literal 2>$null
        if ($LASTEXITCODE -le 1) {
            $gitSucceeded = $true
        }
    }
    catch {
        $gitSucceeded = $false
    }
    finally {
        Pop-Location
    }

    if ($gitSucceeded) {
        foreach ($line in @($raw)) {
            if ([string]::IsNullOrEmpty($line)) {
                continue
            }

            $parts = $line -split ':', 3
            if ($parts.Count -lt 3) {
                continue
            }

            $relativePath = $parts[0].Replace('\', '/')
            if (Test-MtmVersionPathExcluded -RelativePath $relativePath -ExcludePattern $ExcludePattern) {
                continue
            }

            $hits.Add([pscustomobject]@{
                    RelativePath = $relativePath
                    LineNumber   = [int]$parts[1]
                    Literal      = $Literal
                })
        }

        return $hits
    }

    $textExtensions = @(
        '.json', '.csproj', '.props', '.targets', '.xml', '.manifest', '.appxmanifest',
        '.sql', '.yml', '.yaml', '.ps1', '.psm1', '.iss', '.wxs', '.nuspec', '.cs', '.config'
    )

    $files = Get-ChildItem -LiteralPath $RootPath -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object { $textExtensions -contains $_.Extension.ToLowerInvariant() }

    foreach ($file in $files) {
        $relativePath = $file.FullName.Substring($RootPath.Length).TrimStart('\', '/').Replace('\', '/')
        if (Test-MtmVersionPathExcluded -RelativePath $relativePath -ExcludePattern $ExcludePattern) {
            continue
        }

        $fileMatches = Select-String -LiteralPath $file.FullName -SimpleMatch -Pattern $Literal -ErrorAction SilentlyContinue
        foreach ($fileMatch in @($fileMatches)) {
            $hits.Add([pscustomobject]@{
                    RelativePath = $relativePath
                    LineNumber   = $fileMatch.LineNumber
                    Literal      = $Literal
                })
        }
    }

    return $hits
}

function Find-MtmVersionUnlistedHit {
    param(
        [Parameter(Mandatory)]
        [string]$RootPath,

        [Parameter(Mandatory)]
        [string[]]$Literal
    )

    $knownPaths = Get-MtmVersionKnownPath
    $excludePattern = Get-MtmVersionExcludePattern
    $unlisted = [System.Collections.Generic.List[object]]::new()

    foreach ($currentLiteral in @($Literal | Where-Object { -not [string]::IsNullOrEmpty($_) })) {
        $hits = Find-MtmVersionLiteralHit -RootPath $RootPath -Literal $currentLiteral -ExcludePattern $excludePattern
        foreach ($hit in @($hits)) {
            if ($knownPaths -contains $hit.RelativePath) {
                continue
            }

            $existing = $unlisted | Where-Object {
                $_.RelativePath -eq $hit.RelativePath -and $_.LineNumber -eq $hit.LineNumber
            }

            if ($null -eq $existing) {
                $unlisted.Add($hit)
            }
        }
    }

    return $unlisted
}
