[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$Path,

    [Parameter()]
    [ValidateSet('Enable', 'Disable', 'Toggle')]
    [string]$State = 'Toggle',

    [Parameter()]
    [string[]]$BlockName
)

Set-StrictMode -Version Latest

$startPattern = '^\s*--\s*TEST BLOCK START:\s*(?<Name>.+?)\s*$'
$endPattern = '^\s*--\s*TEST BLOCK END:\s*(?<Name>.+?)\s*$'
$commentPrefixPattern = '^(?<Indent>\s*)--\s?(?<Body>.*)$'

$resolvedPath = (Resolve-Path -Path $Path).Path
$lines = [System.IO.File]::ReadAllLines($resolvedPath)
$targetBlockNames = [string[]]@()

if ($PSBoundParameters.ContainsKey('BlockName')) {
    $targetBlockNames = [string[]]@($BlockName)
}

$insideBlock = $false
$currentBlock = ''
$modifiedLines = [System.Collections.Generic.List[string]]::new()
$changedLineCount = 0
$processedBlockCount = 0

foreach ($line in $lines) {
    if ($line -match $startPattern) {
        $insideBlock = $true
        $currentBlock = $Matches.Name
        $processedBlockCount++
        $modifiedLines.Add($line)
        continue
    }

    if ($line -match $endPattern) {
        $insideBlock = $false
        $currentBlock = ''
        $modifiedLines.Add($line)
        continue
    }

    if (-not $insideBlock) {
        $modifiedLines.Add($line)
        continue
    }

    if ($targetBlockNames.Count -gt 0 -and $currentBlock -notin $targetBlockNames) {
        $modifiedLines.Add($line)
        continue
    }

    if ([string]::IsNullOrWhiteSpace($line)) {
        $modifiedLines.Add($line)
        continue
    }

    $updatedLine = $line

    switch ($State) {
        'Enable' {
            if ($line -match $commentPrefixPattern) {
                $updatedLine = "$($Matches.Indent)$($Matches.Body)"
            }
        }
        'Disable' {
            if ($line -notmatch '^\s*--') {
                $updatedLine = "-- $line"
            }
        }
        'Toggle' {
            if ($line -match $commentPrefixPattern) {
                $updatedLine = "$($Matches.Indent)$($Matches.Body)"
            }
            elseif ($line -notmatch '^\s*--') {
                $updatedLine = "-- $line"
            }
        }
    }

    if ($updatedLine -ne $line) {
        $changedLineCount++
    }

    $modifiedLines.Add($updatedLine)
}

if ($insideBlock) {
    throw "Unclosed test block found in '$resolvedPath'."
}

if ($processedBlockCount -eq 0) {
    throw "No test blocks were found in '$resolvedPath'."
}

if ($PSCmdlet.ShouldProcess($resolvedPath, "$State SQL test blocks")) {
    [System.IO.File]::WriteAllLines(
        $resolvedPath,
        $modifiedLines,
        [System.Text.UTF8Encoding]::new($false)
    )
}

[PSCustomObject]@{
    Path            = $resolvedPath
    State           = $State
    ProcessedBlocks = $processedBlockCount
    ChangedLines    = $changedLineCount
    TargetedBlocks  = if ($targetBlockNames.Count -gt 0) { $targetBlockNames -join ', ' } else { '<all>' }
}