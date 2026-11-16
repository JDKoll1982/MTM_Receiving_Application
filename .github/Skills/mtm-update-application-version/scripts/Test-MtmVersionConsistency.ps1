<#
.SYNOPSIS
Verifies that every MTM version source agrees and optionally that the solution builds.

.DESCRIPTION
Checks the three-part version in appsettings.json and the software_version seed script against the
expected version, checks the four-part version in the project file and both appx manifests, lists
leftover hits of the previous version, reports files that hold the new version but are not part of
the known source list, and can run a Debug x64 build. Exits non-zero when any check fails.

.PARAMETER Path
Repository root. Defaults to the current directory.

.PARAMETER ExpectedVersion
Version that every source must declare. Defaults to the value already in appsettings.json.

.PARAMETER PreviousVersion
Version the release replaces. When supplied, leftover occurrences are listed for review.

.PARAMETER RunBuild
Runs dotnet build on the solution after the version checks pass.

.PARAMETER BuildConfiguration
Build configuration used with -RunBuild. Debug by default.

.PARAMETER AsJson
Emit one JSON object instead of a formatted report.

.EXAMPLE
pwsh -File ./Test-MtmVersionConsistency.ps1 -Path . -ExpectedVersion 4.8.0 -PreviousVersion 4.7.0 -RunBuild

.NOTES
Last Updated: 2026-11-16
#>
[CmdletBinding()]
param(
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string]$Path = '.',

    [Parameter()]
    [string]$ExpectedVersion,

    [Parameter()]
    [string]$PreviousVersion,

    [Parameter()]
    [switch]$RunBuild,

    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$BuildConfiguration = 'Debug',

    [Parameter()]
    [switch]$AsJson
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'MtmVersionShared.ps1')

$rootPath = (Resolve-Path -LiteralPath $Path).Path
$checks = [System.Collections.Generic.List[object]]::new()

function Add-Check {
    param(
        [Parameter(Mandatory)]
        [string]$Name,

        [Parameter(Mandatory)]
        [ValidateSet('Pass', 'Fail', 'Warn', 'Skip')]
        [string]$Status,

        [Parameter()]
        [string]$Detail = ''
    )

    $checks.Add([pscustomobject]@{
            Name   = $Name
            Status = $Status
            Detail = $Detail
        })
}

$targetVersion = $ExpectedVersion
if ([string]::IsNullOrWhiteSpace($targetVersion)) {
    $targetVersion = Get-MtmVersionApplicationValue -RootPath $rootPath
}

if ([string]::IsNullOrWhiteSpace($targetVersion) -or $targetVersion -notmatch '^\d+\.\d+\.\d+$') {
    Add-Check -Name 'Target version resolved' -Status 'Fail' -Detail 'Pass -ExpectedVersion or make appsettings.json declare a three-part version.'
}
else {
    Add-Check -Name 'Target version resolved' -Status 'Pass' -Detail $targetVersion
}

$packagingVersion = $null
if (-not [string]::IsNullOrWhiteSpace($targetVersion) -and $targetVersion -match '^\d+\.\d+\.\d+$') {
    $packagingVersion = '{0}.0' -f $targetVersion

    foreach ($definition in Get-MtmVersionSourceDefinition) {
        $content = Read-MtmVersionText -LiteralPath (Join-Path $rootPath $definition.RelativePath)
        $value = Get-MtmVersionValue -Content $content -Pattern $definition.ReadPattern
        $expected = if ($definition.Shape -eq '3-part') { $targetVersion } else { $packagingVersion }
        $name = "Version source: $($definition.Name)"

        if ([string]::IsNullOrEmpty($value)) {
            $detail = if ($null -eq $content) { 'File not found.' } else { 'No version value matched.' }
            Add-Check -Name $name -Status 'Fail' -Detail $detail
        }
        elseif ($value -eq $expected) {
            Add-Check -Name $name -Status 'Pass' -Detail $value
        }
        else {
            Add-Check -Name $name -Status 'Fail' -Detail ("Found {0}, expected {1}." -f $value, $expected)
        }
    }

    $unlisted = @(Find-MtmVersionUnlistedHit -RootPath $rootPath -Literal @($targetVersion, $packagingVersion))
    if ($unlisted.Count -eq 0) {
        Add-Check -Name 'No unlisted file declares the version' -Status 'Pass' -Detail ''
    }
    else {
        $paths = ($unlisted | Select-Object -ExpandProperty RelativePath -Unique) -join ', '
        Add-Check -Name 'No unlisted file declares the version' -Status 'Warn' -Detail ("Review: {0}" -f $paths)
    }

    if ([string]::IsNullOrWhiteSpace($PreviousVersion)) {
        Add-Check -Name 'Previous version fully replaced' -Status 'Skip' -Detail 'Pass -PreviousVersion to enable this scan.'
    }
    elseif ($PreviousVersion -eq $targetVersion) {
        Add-Check -Name 'Previous version fully replaced' -Status 'Warn' -Detail 'Previous and target version are identical.'
    }
    elseif ($PreviousVersion -notmatch '^\d+\.\d+\.\d+$') {
        Add-Check -Name 'Previous version fully replaced' -Status 'Fail' -Detail 'PreviousVersion must be three-part.'
    }
    else {
        $previousPackaging = '{0}.0' -f $PreviousVersion
        $residue = @(Find-MtmVersionUnlistedHit -RootPath $rootPath -Literal @($PreviousVersion, $previousPackaging))
        if ($residue.Count -eq 0) {
            Add-Check -Name 'Previous version fully replaced' -Status 'Pass' -Detail $PreviousVersion
        }
        else {
            $paths = ($residue | Select-Object -ExpandProperty RelativePath -Unique) -join ', '
            Add-Check -Name 'Previous version fully replaced' -Status 'Warn' -Detail ("Leftover {0} occurrences: {1}" -f $PreviousVersion, $paths)
        }
    }
}

if ($RunBuild.IsPresent) {
    $solutionPath = Join-Path $rootPath 'MTM_Receiving_Application.slnx'
    if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) {
        $solutionPath = Join-Path $rootPath 'MTM_Receiving_Application.csproj'
    }

    $buildName = "Build succeeds ($BuildConfiguration, x64)"

    if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) {
        Add-Check -Name $buildName -Status 'Fail' -Detail 'No solution or project file found.'
    }
    else {
        Push-Location -LiteralPath $rootPath
        try {
            $buildOutput = & dotnet build $solutionPath -c $BuildConfiguration -p:Platform=x64 --nologo -v:minimal 2>&1 | Out-String
            $buildExitCode = $LASTEXITCODE
        }
        finally {
            Pop-Location
        }

        if ($buildExitCode -eq 0) {
            Add-Check -Name $buildName -Status 'Pass' -Detail 'dotnet build returned 0.'
        }
        else {
            $tail = ($buildOutput -split "`r?`n" | Where-Object { $_.Trim().Length -gt 0 } | Select-Object -Last 12) -join ' | '
            Add-Check -Name $buildName -Status 'Fail' -Detail ("dotnet build returned {0}. {1}" -f $buildExitCode, $tail)
        }
    }
}
else {
    Add-Check -Name 'Build succeeds' -Status 'Skip' -Detail 'Pass -RunBuild to enable.'
}

$failCount = @($checks | Where-Object { $_.Status -eq 'Fail' }).Count
$warnCount = @($checks | Where-Object { $_.Status -eq 'Warn' }).Count

if ($AsJson.IsPresent) {
    [pscustomobject]@{
        RootPath        = $rootPath
        ExpectedVersion = $targetVersion
        PreviousVersion = $PreviousVersion
        Passed          = ($failCount -eq 0)
        FailCount       = $failCount
        WarnCount       = $warnCount
        Checks          = @($checks)
    } | ConvertTo-Json -Depth 6
}
else {
    Write-Output ("Version consistency: {0}" -f $(if ($failCount -eq 0) { 'PASS' } else { 'FAIL' }))
    Write-Output ("Repository: {0}" -f $rootPath)
    Write-Output ("Expected version: {0}" -f $(if ($targetVersion) { $targetVersion } else { '<unresolved>' }))
    Write-Output ''

    foreach ($check in $checks) {
        $detail = if ([string]::IsNullOrWhiteSpace($check.Detail)) { '' } else { ' - ' + $check.Detail }
        Write-Output ("[{0,-4}] {1}{2}" -f $check.Status.ToUpperInvariant(), $check.Name, $detail)
    }

    Write-Output ''
    Write-Output ("Summary: {0} failed, {1} warning(s), {2} check(s)." -f $failCount, $warnCount, $checks.Count)
}

if ($failCount -gt 0) {
    exit 1
}

exit 0
