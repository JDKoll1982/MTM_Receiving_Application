<#
.SYNOPSIS
Validates an MTM end-user update page and its entry in docs/updates/index.html.

.DESCRIPTION
Checks docs/updates/{version}/index.html for the required page anatomy, shared asset references,
logo path, interaction hooks, release date, and accessibility basics. Then checks
docs/updates/index.html for a newest-first row, the latest-release badge, the copy value, and the
at-a-glance heading. Exits non-zero when any check fails.

.PARAMETER Version
Version whose update page should be validated, for example 4.8.0.

.PARAMETER Path
Repository root. Defaults to the current directory.

.PARAMETER AsJson
Emit one JSON object instead of a formatted report.

.EXAMPLE
pwsh -File ./Test-MtmUpdatePage.ps1 -Path . -Version 4.8.0

.NOTES
Last Updated: 2026-11-16
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version,

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
$checks = [System.Collections.Generic.List[object]]::new()
$singleLineOptions = [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
$multiLineOptions = [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Singleline

$versionLiteral = [System.Text.RegularExpressions.Regex]::Escape($Version)
$versionFolder = Join-Path $rootPath ("docs/updates/{0}" -f $Version)
$pagePath = Join-Path $versionFolder 'index.html'
$indexPath = Join-Path $rootPath 'docs/updates/index.html'

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

function Test-PagePattern {
    param(
        [Parameter(Mandatory)]
        [string]$Name,

        [AllowNull()]
        [string]$Content,

        [Parameter(Mandatory)]
        [string]$Pattern,

        [Parameter()]
        [string]$MissingDetail = 'Page content unavailable.',

        [Parameter()]
        [switch]$MustNotMatch,

        [Parameter()]
        [switch]$IgnoreComments
    )

    if ([string]::IsNullOrEmpty($Content)) {
        Add-Check -Name $Name -Status 'Skip' -Detail $MissingDetail
        return
    }

    $candidate = $Content
    if ($IgnoreComments.IsPresent) {
        $candidate = [System.Text.RegularExpressions.Regex]::Replace($candidate, '<!--.*?-->', '', $script:multiLineOptions)
    }

    $isMatch = [System.Text.RegularExpressions.Regex]::IsMatch($candidate, $Pattern, $script:singleLineOptions)
    $passed = if ($MustNotMatch.IsPresent) { -not $isMatch } else { $isMatch }

    if ($passed) {
        Add-Check -Name $Name -Status 'Pass' -Detail ''
    }
    elseif ($MustNotMatch.IsPresent) {
        Add-Check -Name $Name -Status 'Fail' -Detail ("Unexpected pattern found: {0}" -f $Pattern)
    }
    else {
        Add-Check -Name $Name -Status 'Fail' -Detail ("Missing pattern: {0}" -f $Pattern)
    }
}

$pageContent = $null
if (Test-Path -LiteralPath $pagePath -PathType Leaf) {
    Add-Check -Name 'Version page exists' -Status 'Pass' -Detail $pagePath
    $pageContent = Read-MtmVersionText -LiteralPath $pagePath
}
else {
    Add-Check -Name 'Version page exists' -Status 'Fail' -Detail ("Create {0}." -f $pagePath)
}

Test-PagePattern -Name 'Page title names the version' -Content $pageContent -Pattern ("<title>[^<]*{0}" -f $versionLiteral)
Test-PagePattern -Name 'Page heading names the version' -Content $pageContent -Pattern ("<h1[^>]*>[^<]*{0}" -f $versionLiteral)
Test-PagePattern -Name 'Page uses the shared stylesheet' -Content $pageContent -Pattern 'href="\.\./styles\.css"'
Test-PagePattern -Name 'Page uses the shared script' -Content $pageContent -Pattern 'src="\.\./app\.js"'
Test-PagePattern -Name 'Page has no inline style block' -Content $pageContent -Pattern '<style\b' -MustNotMatch -IgnoreComments
Test-PagePattern -Name 'Page has no inline script block' -Content $pageContent -Pattern '<script(?![^>]*\bsrc=)' -MustNotMatch -IgnoreComments
Test-PagePattern -Name 'Page uses the shared MTM logo' -Content $pageContent -Pattern 'src="\.\./\.\./\.\./Assets/MTMLogo\.jpg"'
Test-PagePattern -Name 'Copy button carries the version' -Content $pageContent -Pattern ("id=`"copy-version`"[^>]*data-copy-text=`"{0}`"" -f $versionLiteral)
Test-PagePattern -Name 'Toggle details hook present' -Content $pageContent -Pattern 'id="toggle-details"'
Test-PagePattern -Name 'Back-to-top control present' -Content $pageContent -Pattern 'id="back-to-top"'
Test-PagePattern -Name 'Skip link present' -Content $pageContent -Pattern 'class="skip-link"'
Test-PagePattern -Name 'Main landmark present' -Content $pageContent -Pattern '<main\b'
Test-PagePattern -Name 'Navigation landmark present' -Content $pageContent -Pattern '<nav\b'
Test-PagePattern -Name 'Page language declared' -Content $pageContent -Pattern '<html[^>]*lang="en"'
Test-PagePattern -Name 'Viewport meta present' -Content $pageContent -Pattern 'name="viewport"'
Test-PagePattern -Name 'Release date stated' -Content $pageContent -Pattern 'Released\s+[A-Za-z]+\s+\d{1,2},\s+\d{4}'
Test-PagePattern -Name 'Detail items carry a filter category' -Content $pageContent -Pattern 'class="change-item"[^>]*data-category="'
Test-PagePattern -Name 'Filter controls present' -Content $pageContent -Pattern 'data-filter="all"'
Test-PagePattern -Name 'Action section present' -Content $pageContent -Pattern 'class="action-state'
Test-PagePattern -Name 'No ambiguous link text' -Content $pageContent -Pattern '>\s*(here|click here|this link)\s*<' -MustNotMatch -IgnoreComments

if ([string]::IsNullOrEmpty($pageContent)) {
    Add-Check -Name 'Single h1 heading' -Status 'Skip' -Detail 'Page content unavailable.'
}
else {
    $headingCount = [System.Text.RegularExpressions.Regex]::Matches($pageContent, '<h1\b', $singleLineOptions).Count
    if ($headingCount -eq 1) {
        Add-Check -Name 'Single h1 heading' -Status 'Pass' -Detail ''
    }
    else {
        Add-Check -Name 'Single h1 heading' -Status 'Fail' -Detail ("Found {0} h1 elements." -f $headingCount)
    }

    $requiredIds = @('highlights', 'changes', 'impact', 'action', 'support')
    $missingIds = @($requiredIds | Where-Object {
            -not [System.Text.RegularExpressions.Regex]::IsMatch($pageContent, ('id="{0}"' -f $_), $singleLineOptions)
        })

    if ($missingIds.Count -eq 0) {
        Add-Check -Name 'Required sections present' -Status 'Pass' -Detail ($requiredIds -join ', ')
    }
    else {
        Add-Check -Name 'Required sections present' -Status 'Fail' -Detail ("Missing ids: {0}" -f ($missingIds -join ', '))
    }

    $imageTags = [System.Text.RegularExpressions.Regex]::Matches($pageContent, '<img\b[^>]*>', $singleLineOptions)
    $imagesWithoutAlt = @($imageTags | Where-Object { $_.Value -notmatch 'alt="[^"]+"' })
    if ($imagesWithoutAlt.Count -eq 0) {
        Add-Check -Name 'Images have alt text' -Status 'Pass' -Detail ("{0} image(s) checked." -f $imageTags.Count)
    }
    else {
        Add-Check -Name 'Images have alt text' -Status 'Fail' -Detail ("{0} image(s) without alt text." -f $imagesWithoutAlt.Count)
    }
}

$localStyleSheet = Join-Path $versionFolder 'styles.css'
$localScript = Join-Path $versionFolder 'app.js'
if ((Test-Path -LiteralPath $localStyleSheet) -or (Test-Path -LiteralPath $localScript)) {
    Add-Check -Name 'No version-local assets' -Status 'Fail' -Detail 'Remove styles.css or app.js from the version folder; use the shared assets.'
}
else {
    Add-Check -Name 'No version-local assets' -Status 'Pass' -Detail ''
}

if (Test-Path -LiteralPath $versionFolder -PathType Container) {
    $extraFiles = @(Get-ChildItem -LiteralPath $versionFolder -File | Where-Object { $_.Name -ne 'index.html' })
    if ($extraFiles.Count -eq 0) {
        Add-Check -Name 'Version folder holds only index.html' -Status 'Pass' -Detail ''
    }
    else {
        Add-Check -Name 'Version folder holds only index.html' -Status 'Warn' -Detail (($extraFiles | ForEach-Object { $_.Name }) -join ', ')
    }
}
else {
    Add-Check -Name 'Version folder holds only index.html' -Status 'Skip' -Detail 'Version folder does not exist.'
}

$indexContent = $null
if (Test-Path -LiteralPath $indexPath -PathType Leaf) {
    Add-Check -Name 'Updates index exists' -Status 'Pass' -Detail $indexPath
    $indexContent = Read-MtmVersionText -LiteralPath $indexPath
}
else {
    Add-Check -Name 'Updates index exists' -Status 'Fail' -Detail ("Create {0}." -f $indexPath)
}

Test-PagePattern -Name 'Index links to the version page' -Content $indexContent -Pattern ("href=`"{0}/index\.html`"" -f $versionLiteral) -MissingDetail 'Index content unavailable.'
Test-PagePattern -Name 'Index badge shows the latest release' -Content $indexContent -Pattern ("Latest release\s+{0}" -f $versionLiteral) -MissingDetail 'Index content unavailable.'
Test-PagePattern -Name 'Index copy value updated' -Content $indexContent -Pattern ("data-copy-text=`"{0}`"" -f $versionLiteral) -MissingDetail 'Index content unavailable.'
Test-PagePattern -Name 'Index at-a-glance names the version' -Content $indexContent -Pattern ("Version\s+{0}\s+at a glance" -f $versionLiteral) -MissingDetail 'Index content unavailable.'

if ([string]::IsNullOrEmpty($indexContent)) {
    Add-Check -Name 'Index lists the newest release first' -Status 'Skip' -Detail 'Index content unavailable.'
}
else {
    $tbodyMatch = [System.Text.RegularExpressions.Regex]::Match($indexContent, '<tbody>(?<body>.*?)</tbody>', $multiLineOptions)
    if (-not $tbodyMatch.Success) {
        Add-Check -Name 'Index lists the newest release first' -Status 'Fail' -Detail 'No release table body found.'
    }
    else {
        $firstRowMatch = [System.Text.RegularExpressions.Regex]::Match($tbodyMatch.Groups['body'].Value, '<tr\b.*?</tr>', $multiLineOptions)
        if (-not $firstRowMatch.Success) {
            Add-Check -Name 'Index lists the newest release first' -Status 'Fail' -Detail 'Release table body has no rows.'
        }
        elseif ($firstRowMatch.Value -match ('href="' + $versionLiteral + '/index\.html"')) {
            Add-Check -Name 'Index lists the newest release first' -Status 'Pass' -Detail $Version
        }
        else {
            Add-Check -Name 'Index lists the newest release first' -Status 'Fail' -Detail ("The first row is not {0}." -f $Version)
        }
    }
}

$failCount = @($checks | Where-Object { $_.Status -eq 'Fail' }).Count
$warnCount = @($checks | Where-Object { $_.Status -eq 'Warn' }).Count

if ($AsJson.IsPresent) {
    [pscustomobject]@{
        RootPath  = $rootPath
        Version   = $Version
        PagePath  = $pagePath
        Passed    = ($failCount -eq 0)
        FailCount = $failCount
        WarnCount = $warnCount
        Checks    = @($checks)
    } | ConvertTo-Json -Depth 6
}
else {
    Write-Output ("Update page validation: {0}" -f $(if ($failCount -eq 0) { 'PASS' } else { 'FAIL' }))
    Write-Output ("Version: {0}" -f $Version)
    Write-Output ("Page: {0}" -f $pagePath)
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
