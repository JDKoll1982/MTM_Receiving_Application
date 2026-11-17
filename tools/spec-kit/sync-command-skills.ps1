#Requires -Version 7.0
<#
.SYNOPSIS
Regenerates the .github/Skills entry points for every Spec Kit command in the repo.

.DESCRIPTION
This repository keeps both Copilot layouts on purpose:

  - .github/agents/speckit.*.agent.md and .github/prompts/speckit.*.prompt.md carry the
    real command bodies that `specify integration upgrade copilot` manages.
  - .github/Skills/speckit-*/SKILL.md are thin wrappers that give the same commands a
    /speckit-* entry point without duplicating the body, so a spec-kit or extension
    update keeps working without touching the skills.

Run this after any `specify integration upgrade` or `specify extension add/update` so the
wrappers match what is actually installed. Wrappers for commands that no longer exist are
left alone unless -RemoveStale is used.

.PARAMETER Root
Repository root. Defaults to the folder two levels above this script.

.PARAMETER RemoveStale
Also removes speckit-* skill folders whose command is no longer installed.

.EXAMPLE
pwsh -File tools\spec-kit\sync-command-skills.ps1

.EXAMPLE
pwsh -File tools\spec-kit\sync-command-skills.ps1 -RemoveStale -WhatIf
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [string]$Root,
    [switch]$RemoveStale
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $Root) {
    $Root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
}

$agentsDir = Join-Path $Root '.github/agents'
$skillsDir = Join-Path $Root '.github/Skills'
$extensionsDir = Join-Path $Root '.specify/extensions'

if (-not (Test-Path $agentsDir)) {
    throw "No agent folder at $agentsDir. Is '$Root' the repository root?"
}

$template = @'
---
name: {0}
description: {1}
disable-model-invocation: true
---

This skill runs the command defined in [`{2}`]({3}).

Read that file and follow it exactly, including every reference file it names.
It is the command; this page is only the entry point, so the command stays current
when the extension is updated.
'@

function Get-SkillName {
    param([string]$CommandName)

    return $CommandName.Replace('.', '-')
}

function Get-CommandTarget {
    <#
        Decides which file a wrapper should point at: the Copilot agent when the
        integration emitted one, otherwise the extension's own command markdown.
    #>
    param(
        [string]$CommandName,
        [string]$AgentsDir,
        [string]$ExtensionsDir
    )

    $agentPath = Join-Path $AgentsDir "$CommandName.agent.md"
    if (Test-Path $agentPath) {
        return [pscustomobject]@{
            Display = "$CommandName.agent.md"
            Link    = "../../../.github/agents/$CommandName.agent.md"
        }
    }

    if (Test-Path $ExtensionsDir) {
        foreach ($extensionDir in Get-ChildItem $ExtensionsDir -Directory) {
            $commandPath = Join-Path $extensionDir.FullName "commands/$CommandName.md"
            if (Test-Path $commandPath) {
                $relative = $commandPath.Substring($Root.Length).TrimStart('\', '/').Replace('\', '/')
                return [pscustomobject]@{
                    Display = "$CommandName.md"
                    Link    = "../../../$relative"
                }
            }
        }
    }

    return $null
}

$commands = Get-ChildItem $agentsDir -Filter 'speckit.*.agent.md' -File |
    ForEach-Object { $_.Name -replace '\.agent\.md$', '' } |
    Sort-Object

$written = 0
$skipped = 0

foreach ($command in $commands) {
    $target = Get-CommandTarget -CommandName $command -AgentsDir $agentsDir -ExtensionsDir $extensionsDir -Root $Root
    if (-not $target) {
        Write-Warning "No command file found for '$command'; skipping its skill."
        $skipped++
        continue
    }

    $skillName = Get-SkillName $command
    $skillFolder = Join-Path $skillsDir $skillName
    $skillFile = Join-Path $skillFolder 'SKILL.md'
    $content = $template -f $skillName, $command, $target.Display, $target.Link

    if (-not (Test-Path $skillFolder) -and $PSCmdlet.ShouldProcess($skillFolder, 'Create skill folder')) {
        New-Item -ItemType Directory -Path $skillFolder -Force | Out-Null
    }

    $existing = if (Test-Path $skillFile) { Get-Content $skillFile -Raw } else { $null }
    if ($existing -and $existing.Replace("`r`n", "`n") -eq $content.Replace("`r`n", "`n")) {
        continue
    }

    if ($PSCmdlet.ShouldProcess($skillFile, 'Write skill wrapper')) {
        Set-Content -Path $skillFile -Value $content -Encoding utf8NoBOM
        $written++
    }
}

if ($RemoveStale) {
    $liveNames = $commands | ForEach-Object { Get-SkillName $_ }
    foreach ($folder in Get-ChildItem $skillsDir -Directory -Filter 'speckit-*') {
        if ($liveNames -notcontains $folder.Name) {
            if ($PSCmdlet.ShouldProcess($folder.FullName, 'Remove stale skill')) {
                Remove-Item $folder.FullName -Recurse -Force
                Write-Host "Removed stale skill $($folder.Name)"
            }
        }
    }
}

Write-Host "Spec Kit skills: $($commands.Count) command(s), $written wrapper(s) written, $skipped skipped."
