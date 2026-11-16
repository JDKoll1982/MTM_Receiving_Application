<#
.SYNOPSIS
Reports the MTM application version declared in every known source.

.DESCRIPTION
Read-only. Reads the three-part version from appsettings.json and the software_version seed script,
the four-part version from the project file and both appx manifests, reports whether they agree,
and lists every other tracked file that contains either version literal so an unlisted source can
be found before a release is committed.

.PARAMETER Path
Repository root to inspect. Defaults to the current directory.

.PARAMETER AsJson
Emit one JSON object instead of a formatted report.

.EXAMPLE
pwsh -File ./Get-MtmVersionSnapshot.ps1 -Path .

.EXAMPLE
pwsh -File ./Get-MtmVersionSnapshot.ps1 -Path . -AsJson

.NOTES
Last Updated: 2026-11-16
#>
[CmdletBinding()]
param(
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string]$Path = '.',

    [Parameter()]
    [switch]$AsJson
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'MtmVersionShared.ps1')

$rootPath = (Resolve-Path -LiteralPath $Path).Path
$threePart = Get-MtmVersionApplicationValue -RootPath $rootPath
$fourPart = $null
if (-not [string]::IsNullOrEmpty($threePart)) {
    $fourPart = '{0}.0' -f $threePart
}

$sourceResults = [System.Collections.Generic.List[object]]::new()
$isConsistent = $true

foreach ($definition in Get-MtmVersionSourceDefinition) {
    $content = Read-MtmVersionText -LiteralPath (Join-Path $rootPath $definition.RelativePath)
    $value = Get-MtmVersionValue -Content $content -Pattern $definition.ReadPattern
    $expected = if ($definition.Shape -eq '3-part') { $threePart } else { $fourPart }

    $status = 'Unknown'
    if ([string]::IsNullOrEmpty($expected)) {
        $status = 'Unknown'
    }
    elseif ([string]::IsNullOrEmpty($value)) {
        $status = if ($null -eq $content) { 'Missing file' } else { 'Missing value' }
    }
    elseif ($value -eq $expected) {
        $status = 'Ok'
    }
    else {
        $status = 'Mismatch'
    }

    if ($status -ne 'Ok') {
        $isConsistent = $false
    }

    $sourceResults.Add([pscustomobject]@{
            Name         = $definition.Name
            RelativePath = $definition.RelativePath
            Shape        = $definition.Shape
            Value        = $value
            Expected     = $expected
            Status       = $status
        })
}

$unlistedHits = @(Find-MtmVersionUnlistedHit -RootPath $rootPath -Literal @($threePart, $fourPart))

$snapshot = [pscustomobject]@{
    RootPath     = $rootPath
    Version      = $threePart
    Version4Part = $fourPart
    Consistent   = $isConsistent
    Sources      = @($sourceResults)
    UnlistedHits = @($unlistedHits)
}

if ($AsJson.IsPresent) {
    $snapshot | ConvertTo-Json -Depth 6
    return
}

$versionText = if ($snapshot.Version) { $snapshot.Version } else { '<not found>' }
$packagingText = if ($snapshot.Version4Part) { $snapshot.Version4Part } else { '<not found>' }
$consistentText = if ($snapshot.Consistent) { 'YES' } else { 'NO' }

Write-Output ("Repository:                    {0}" -f $snapshot.RootPath)
Write-Output ("Application version (3-part):  {0}" -f $versionText)
Write-Output ("Packaging version (4-part):    {0}" -f $packagingText)
Write-Output ("Sources consistent:            {0}" -f $consistentText)
Write-Output ''
Write-Output 'KNOWN SOURCES'

foreach ($source in $sourceResults) {
    $valueText = if ($source.Value) { $source.Value } else { '<none>' }
    Write-Output ("[{0,-12}] {1,-7} {2,-12} {3}" -f $source.Status, $source.Shape, $valueText, $source.Name)
}

Write-Output ''
if ($unlistedHits.Count -eq 0) {
    Write-Output 'OTHER FILES CONTAINING THE VERSION'
    Write-Output '[none]'
}
else {
    Write-Output 'OTHER FILES CONTAINING THE VERSION (review: unlisted source or historical record)'
    foreach ($hit in $unlistedHits) {
        Write-Output ("{0}:{1} ({2})" -f $hit.RelativePath, $hit.LineNumber, $hit.Literal)
    }
}
