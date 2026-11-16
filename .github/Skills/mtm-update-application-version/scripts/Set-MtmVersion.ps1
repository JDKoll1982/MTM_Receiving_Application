<#
.SYNOPSIS
Applies an MTM application version to every known version source.

.DESCRIPTION
Writes the three-part version into appsettings.json and the software_version seed script, and the
four-part version (trailing .0) into the project file and both appx manifests. Values are replaced
in place, file encoding and line endings are preserved, and every write is read back and verified.
Sources that do not exist or do not match are reported instead of being created.

.PARAMETER Version
Target version in three-part form, for example 4.8.0.

.PARAMETER Path
Repository root. Defaults to the current directory.

.PARAMETER PassThru
Emit one object per source instead of the formatted report.

.EXAMPLE
pwsh -File ./Set-MtmVersion.ps1 -Path . -Version 4.8.0

.EXAMPLE
pwsh -File ./Set-MtmVersion.ps1 -Path . -Version 4.8.0 -WhatIf

.NOTES
Last Updated: 2026-11-16
#>
[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'Medium')]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version,

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string]$Path = '.',

    [Parameter()]
    [switch]$PassThru
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'MtmVersionShared.ps1')

$rootPath = (Resolve-Path -LiteralPath $Path).Path
$packagingVersion = '{0}.0' -f $Version

function Read-MtmTextWithEncoding {
    param(
        [Parameter(Mandatory)]
        [string]$LiteralPath
    )

    $bytes = [System.IO.File]::ReadAllBytes($LiteralPath)
    $hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF

    return [pscustomobject]@{
        Text   = [System.IO.File]::ReadAllText($LiteralPath)
        HasBom = $hasBom
    }
}

function Write-MtmTextWithEncoding {
    param(
        [Parameter(Mandatory)]
        [string]$LiteralPath,

        [Parameter(Mandatory)]
        [string]$Text,

        [Parameter(Mandatory)]
        [bool]$HasBom
    )

    $encoding = [System.Text.UTF8Encoding]::new($HasBom)
    [System.IO.File]::WriteAllText($LiteralPath, $Text, $encoding)
}

$results = [System.Collections.Generic.List[object]]::new()

foreach ($definition in Get-MtmVersionSourceDefinition) {
    $fullPath = Join-Path $rootPath $definition.RelativePath
    $targetValue = if ($definition.Shape -eq '3-part') { $Version } else { $packagingVersion }

    $result = [pscustomobject]@{
        Name         = $definition.Name
        RelativePath = $definition.RelativePath
        Previous     = $null
        Current      = $null
        Status       = 'Unknown'
    }

    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        $result.Status = 'Missing file'
        $results.Add($result)
        continue
    }

    $file = Read-MtmTextWithEncoding -LiteralPath $fullPath
    $existingMatch = [System.Text.RegularExpressions.Regex]::Match($file.Text, $definition.ReadPattern)
    if ($existingMatch.Success) {
        $result.Previous = $existingMatch.Groups['value'].Value
    }

    $replacementText = '${1}' + $targetValue + '${2}'
    $regex = [System.Text.RegularExpressions.Regex]::new(
        $definition.WritePattern,
        [System.Text.RegularExpressions.RegexOptions]::Singleline
    )
    $updatedText = $regex.Replace($file.Text, $replacementText, 1)

    if ($updatedText -eq $file.Text) {
        if ($existingMatch.Success -and $existingMatch.Groups['value'].Value -eq $targetValue) {
            $result.Status = 'Already set'
            $result.Current = $existingMatch.Groups['value'].Value
        }
        else {
            $result.Status = 'No match'
        }

        $results.Add($result)
        continue
    }

    if (-not $PSCmdlet.ShouldProcess($definition.RelativePath, "Set $($definition.Name) to $targetValue")) {
        $result.Status = 'Skipped'
        $results.Add($result)
        continue
    }

    Write-MtmTextWithEncoding -LiteralPath $fullPath -Text $updatedText -HasBom $file.HasBom

    $verifyText = [System.IO.File]::ReadAllText($fullPath)
    $verifyMatch = [System.Text.RegularExpressions.Regex]::Match($verifyText, $definition.ReadPattern)
    if ($verifyMatch.Success -and $verifyMatch.Groups['value'].Value -eq $targetValue) {
        $result.Status = 'Updated'
        $result.Current = $verifyMatch.Groups['value'].Value
    }
    else {
        $result.Status = 'Verify failed'
        if ($verifyMatch.Success) {
            $result.Current = $verifyMatch.Groups['value'].Value
        }
    }

    $results.Add($result)
}

if ($PassThru.IsPresent) {
    $results
    return
}

Write-Output ("Target application version: {0}" -f $Version)
Write-Output ("Target packaging version:   {0}" -f $packagingVersion)
Write-Output ''

foreach ($result in $results) {
    $previousText = if ($result.Previous) { $result.Previous } else { '<none>' }
    $currentText = if ($result.Current) { $result.Current } else { '<none>' }
    Write-Output ("[{0,-12}] {1,-52} {2} -> {3}" -f $result.Status, $result.Name, $previousText, $currentText)
}

$unresolved = @($results | Where-Object { $_.Status -in @('Missing file', 'No match', 'Verify failed') })
if ($unresolved.Count -gt 0) {
    Write-Output ''
    $paths = ($unresolved | ForEach-Object { $_.RelativePath }) -join ', '
    Write-Warning ("{0} source(s) need manual attention: {1}" -f $unresolved.Count, $paths)
}
