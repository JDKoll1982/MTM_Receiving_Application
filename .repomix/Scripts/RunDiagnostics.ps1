$ErrorActionPreference = 'Stop';
$root = 'C:\Users\johnk\source\repos\MTM_Receiving_Application';
$diagRoot = Join-Path $root '.repomix\diagrams';
$summaryPath = Join-Path $root '.repomix\diagrams\diagnostics\validation-summary.md';
$summaryDir = Split-Path $summaryPath -Parent;
New-Item -ItemType Directory -Path $summaryDir -Force | Out-Null;

$fileIndex = @{};
Get-ChildItem -Path $root -Recurse -File | ForEach-Object {
    if (-not $fileIndex.ContainsKey($_.Name)) {
        $fileIndex[$_.Name] = @();
    }
    $fileIndex[$_.Name] += $_.FullName;
};

function Get-FirstFilePath([string[]]$names) {
    foreach ($name in $names) {
        if ($fileIndex.ContainsKey($name)) {
            return $fileIndex[$name][0];
        }
    }
    return $null;
}

$files = Get-ChildItem -Path $diagRoot -Recurse -Filter 'Workflow_*.md';
$groups = $files | Group-Object { Split-Path $_.DirectoryName -Leaf } | Sort-Object Count -Descending;

$lines = @();
$lines += '# Workflow Diagram Validation Summary';
$lines += '';
$lines += ('Generated: {0}' -f (Get-Date -Format 'yyyy-MM-dd HH:mm'));
$lines += '';

foreach ($group in $groups) {
    $lines += ('## {0} ({1})' -f $group.Name, $group.Count);
    foreach ($file in ($group.Group | Sort-Object Name)) {
        $content = Get-Content -LiteralPath $file.FullName -Raw;
        $name = [System.IO.Path]::GetFileNameWithoutExtension($file.Name) -replace '^Workflow_', '';
        $normalizedName = $name -replace '_xaml$', '' -replace '\.xaml$', '';

        $stepMatches = [regex]::Matches($content, 'Step\d+\["(.*?)"\]');
        $steps = @();
        foreach ($m in $stepMatches) { $steps += $m.Groups[1].Value; }

        $decisionMatches = [regex]::Matches($content, 'Decision\d+\{\"?(.*?)\"?\}');
        $decisions = @();
        foreach ($m in $decisionMatches) { $decisions += $m.Groups[1].Value; }

        $patterns = @();
        if ($name -like '*_xaml') {
            $patterns += ('View_' + $normalizedName + '.xaml');
            $patterns += ('View_' + $normalizedName + '.xaml.cs');
            $patterns += ($normalizedName + '.xaml');
        }
        elseif ($name -like 'ViewModel_*') {
            $patterns += ($name + '.cs');
        }
        elseif ($name -like 'View_*') {
            $patterns += ($name + '.xaml');
            $patterns += ($name + '.xaml.cs');
        }
        else {
            $patterns += ($normalizedName + '.cs');
            $patterns += ('ViewModel_' + $normalizedName + '.cs');
            $patterns += ('View_' + $normalizedName + '.xaml.cs');
            $patterns += ('View_' + $normalizedName + '.xaml');
            $patterns += ($normalizedName + '.xaml');
        }

        $sourcePath = Get-FirstFilePath $patterns;
        if (-not $sourcePath) {
            $lines += ('- [WARN] {0}: source not found' -f $file.FullName.Replace($root + '\\', ''));
            continue;
        }

        $sourceContent = Get-Content -LiteralPath $sourcePath -Raw;
        $normalizedSource = $sourceContent -replace '\s+', '';

        $missingSteps = @();
        foreach ($step in $steps) {
            if ($step -and ($sourceContent -notmatch [regex]::Escape($step))) {
                $missingSteps += $step;
            }
        }

        $missingDecisions = @();
        foreach ($decision in $decisions) {
            if ($decision) {
                $unescapedDecision = $decision -replace '\\"', '"';
                $unescapedDecision = $unescapedDecision -replace '&lt;', '<' -replace '&gt;', '>' -replace '&amp;', '&';
                $normalizedDecision = $unescapedDecision -replace '\s+', '';
                if ($normalizedSource -notmatch [regex]::Escape($normalizedDecision)) {
                    $missingDecisions += $decision;
                }
            }
        }

        if ($missingSteps.Count -eq 0 -and $missingDecisions.Count -eq 0) {
            $lines += ('- [OK] {0} -> {1}' -f $file.FullName.Replace($root + '\\', ''), $sourcePath.Replace($root + '\\', ''));
        }
        else {
            $issues = @();
            if ($missingSteps.Count -gt 0) { $issues += ('Missing steps: {0}' -f ($missingSteps -join ', ')); }
            if ($missingDecisions.Count -gt 0) { $issues += ('Missing decisions: {0}' -f ($missingDecisions -join ', ')); }
            $lines += ('- [WARN] {0} -> {1} ({2})' -f $file.FullName.Replace($root + '\\', ''), $sourcePath.Replace($root + '\\', ''), ($issues -join '; '));
        }
    }

    $lines += '';
}

Set-Content -LiteralPath $summaryPath -Value ($lines -join "`n") -Encoding UTF8;
Write-Host ('Summary written: {0}' -f $summaryPath) -ForegroundColor Green;