#Requires -Version 7.0
<#
.SYNOPSIS
Checks that this repository is wired for spec-kit and the SpecKit Companion.

.DESCRIPTION
Verifies the CLI version, the Copilot integration manifest, the installed extensions and preset,
the hook registration, the VS Code settings the Companion reads, and that every installed command
has a matching .github/Skills wrapper. Everything except the Companion status probe is a file read.

Exit code is 0 when every check passes, 1 when any check fails.

.PARAMETER Root
Repository root. Defaults to the folder two levels above this script.

.EXAMPLE
pwsh -File tools\spec-kit\verify-setup.ps1
#>
[CmdletBinding()]
param(
    [string]$Root
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $Root) {
    $Root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
}

$script:failures = 0
$script:checks = 0

function Test-Check {
    param(
        [string]$Name,
        [scriptblock]$Condition,
        [string]$Fix
    )

    $script:checks++
    try {
        $result = & $Condition
    }
    catch {
        $result = $false
    }

    if ($result) {
        Write-Host "  PASS  $Name" -ForegroundColor Green
        return
    }

    $script:failures++
    Write-Host "  FAIL  $Name" -ForegroundColor Red
    if ($Fix) {
        Write-Host "        fix: $Fix" -ForegroundColor Yellow
    }
}

function Read-JsonFile {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        return $null
    }

    return Get-Content $Path -Raw | ConvertFrom-Json
}

Write-Host "Spec Kit setup check - $Root"
Write-Host ''

Write-Host 'CLI'
$versionText = (specify --version 2>&1 | Out-String).Trim()
Test-Check -Name "specify CLI is 1.2.5 or newer (found '$versionText')" -Condition {
    if ($versionText -notmatch '(\d+)\.(\d+)\.(\d+)') { return $false }
    $major = [int]$Matches[1]; $minor = [int]$Matches[2]; $patch = [int]$Matches[3]
    ($major -gt 1) -or ($major -eq 1 -and $minor -gt 2) -or ($major -eq 1 -and $minor -eq 2 -and $patch -ge 5)
} -Fix 'specify self check, then uv tool install specify-cli --force --from git+https://github.com/github/spec-kit.git@v1.4.1'

Write-Host ''
Write-Host 'Integration'

$statusJson = $null
try {
    $rawStatus = specify integration status --json 2>&1 | Out-String
    $statusJson = $rawStatus.Substring($rawStatus.IndexOf('{')) | ConvertFrom-Json
}
catch {
    $statusJson = $null
}

Test-Check -Name 'integration status is readable' -Condition { $null -ne $statusJson } -Fix 'specify integration status'

# constitution-sync overrides the constitution command on purpose, and the Companion settings in
# .vscode/settings.json are repo-specific, so those three files always read as modified.
$expectedModified = @(
    '.github\agents\speckit.constitution.agent.md',
    '.github\prompts\speckit.constitution.prompt.md',
    '.vscode\settings.json'
)

Test-Check -Name 'no managed files are missing' -Condition {
    $statusJson.missing_managed_files -eq 0
} -Fix 'specify integration upgrade copilot --integration-options="--commands"'

Test-Check -Name 'only the preset override and Companion settings read as modified' -Condition {
    $actual = @()
    foreach ($manifest in $statusJson.manifests.PSObject.Properties) {
        $actual += $manifest.Value.modified_files
    }
    foreach ($file in $actual) {
        $normalized = $file.Replace('/', '\')
        if ($expectedModified -notcontains $normalized) { return $false }
    }
    return $true
} -Fix 'Review git diff on .github/agents and .github/prompts; drift there means something edited a managed file'

Write-Host ''
Write-Host 'Extensions and preset'

$extensionsYml = if (Test-Path (Join-Path $Root '.specify/extensions.yml')) {
    Get-Content (Join-Path $Root '.specify/extensions.yml') -Raw
}
else {
    ''
}

foreach ($extension in 'companion', 'assess', 'bug', 'git', 'github', 'agent-context') {
    Test-Check -Name "extension '$extension' is installed" -Condition {
        $extensionsYml -match "(?m)^-\s*$([regex]::Escape($extension))\s*$"
    } -Fix "specify extension add $extension"
}

Test-Check -Name 'constitution-sync preset is installed' -Condition {
    Test-Path (Join-Path $Root '.specify/presets/constitution-sync')
} -Fix 'specify preset add constitution-sync'

Write-Host ''
Write-Host 'Hooks'

$companionHooks = [regex]::Matches($extensionsYml, '(?m)^\s*-\s*extension:\s*companion\s*$').Count
Test-Check -Name "extensions.yml registers the companion hooks (found $companionHooks, expect 10)" -Condition {
    $companionHooks -ge 10
} -Fix 'specify extension add companion --from https://github.com/alfredoperez/speckit-companion/releases/download/companion-latest/companion.zip --force'

$hooksFile = Read-JsonFile (Join-Path $Root '.github/hooks/speckit.json')
Test-Check -Name 'Copilot runtime events are wired (userPromptSubmitted + agentStop)' -Condition {
    $null -ne $hooksFile -and
    $null -ne $hooksFile.hooks.userPromptSubmitted -and
    $null -ne $hooksFile.hooks.agentStop
} -Fix 'specify integration upgrade copilot --integration-options="--commands"'

Test-Check -Name 'the event dispatcher is present' -Condition {
    Test-Path (Join-Path $Root '.specify/events.py')
} -Fix 'specify integration upgrade copilot --integration-options="--commands"'

Write-Host ''
Write-Host 'VS Code settings the Companion reads'

$vscodeSettings = Get-Content (Join-Path $Root '.vscode/settings.json') -Raw
# Comments are legal in VS Code settings and ConvertFrom-Json does not accept them.
$vscodeSettingsJson = ($vscodeSettings -split "`n" | Where-Object { $_ -notmatch '^\s*//' }) -join "`n"
$settings = $vscodeSettingsJson | ConvertFrom-Json

Test-Check -Name 'speckit.aiProvider is copilot (the extension defaults to claude)' -Condition {
    $settings.'speckit.aiProvider' -eq 'copilot'
} -Fix 'Add "speckit.aiProvider": "copilot" to .vscode/settings.json'

Test-Check -Name 'speckit.specDirectories includes specs' -Condition {
    @($settings.'speckit.specDirectories') -contains 'specs'
} -Fix 'Add "speckit.specDirectories": ["specs"] to .vscode/settings.json'

Test-Check -Name 'speckit.defaultWorkflow names an installed workflow' -Condition {
    $workflows = (specify workflow list 2>&1 | Out-String)
    $settings.'speckit.defaultWorkflow' -and $workflows -match [regex]::Escape($settings.'speckit.defaultWorkflow')
} -Fix 'Add "speckit.defaultWorkflow": "speckit-companion" to .vscode/settings.json'

Write-Host ''
Write-Host 'Command entry points'

$agents = Get-ChildItem (Join-Path $Root '.github/agents') -Filter 'speckit.*.agent.md' -File
$missingSkills = @()
foreach ($agent in $agents) {
    $command = $agent.Name -replace '\.agent\.md$', ''
    $skillPath = Join-Path $Root ".github/Skills/$($command.Replace('.', '-'))/SKILL.md"
    if (-not (Test-Path $skillPath)) {
        $missingSkills += $command
    }
}

Test-Check -Name "every command has a skill wrapper ($($agents.Count) command(s))" -Condition {
    $missingSkills.Count -eq 0
} -Fix 'pwsh -File tools\spec-kit\sync-command-skills.ps1 -RemoveStale'

$promptRoot = Get-ChildItem (Join-Path $Root '.github/prompts') -Filter 'speckit.*.prompt.md' -File
Test-Check -Name "prompt selectors exist for the installed commands (found $($promptRoot.Count))" -Condition {
    $promptRoot.Count -ge 40
} -Fix 'specify integration upgrade copilot --integration-options="--commands"'

Write-Host ''
Write-Host 'Companion'

$statusScript = Join-Path $Root '.specify/extensions/companion/scripts/status-context.py'
Test-Check -Name 'the Companion can read the active spec' -Condition {
    if (-not (Test-Path $statusScript)) { return $false }
    $probe = python $statusScript --plain 2>&1 | Out-String
    $LASTEXITCODE -eq 0 -and $probe -match 'Spec:'
} -Fix 'python .specify/extensions/companion/scripts/status-context.py --plain'

Write-Host ''
if ($script:failures -eq 0) {
    Write-Host "All $script:checks checks passed." -ForegroundColor Green
    exit 0
}

Write-Host "$script:failures of $script:checks checks failed." -ForegroundColor Red
exit 1
