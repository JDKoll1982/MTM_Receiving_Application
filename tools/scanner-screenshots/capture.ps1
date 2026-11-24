<#
.SYNOPSIS
    Capture the Scanner help screenshots straight from the running app.

.DESCRIPTION
    Drives the MTM Receiving Application through UI Automation (`winapp ui`) and writes the
    screenshots that docs/help/scanner/*.html reference. Each scene is a named list of steps, so
    re-running this after a UI change refreshes the images without hand-cropping anything.

    The app must already be signed in. This script only navigates and captures - it never sends a
    line to Infor Visual, never edits a quantity, and never answers a save prompt.

    Scenes marked "live" read real Infor Visual data (the RECV location and part MMC0000994). They
    need a reachable Infor Visual connection and are skipped unless you ask for them.

.PARAMETER Scene
    One or more scene names. Defaults to every scene that runs without live data.

.PARAMETER All
    Also run the scenes that need live Infor Visual data.

.PARAMETER List
    Print the scene names and what each one needs, then exit.

.PARAMETER ProcessId
    Attach to this process instead of locating the app by name.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tools\scanner-screenshots\capture.ps1

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tools\scanner-screenshots\capture.ps1 -All -Verbose
#>
[CmdletBinding()]
param(
    [string[]] $Scene,
    [switch] $All,
    [switch] $List,
    [Alias('Pid')]
    [int] $ProcessId,
    [int] $SettleSeconds = 2
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$sourceRoot = Join-Path $PSScriptRoot 'source'

# Step verbs: nav (navigation item), invoke (press), set (type into), keys (send keys), wait.
# Selectors are either an AutomationId or a visible label, whichever the app exposes.
$scenes = @(
    @{
        Name = 'workbench-main'
        File = 'Scanner-Workbench-Part.png'
        NeedsData = $false
        Steps = @(
            @{ nav = 'Scanner' }
            @{ invoke = 'ScannerWorkbenchTab' }
            @{ keys = 'esc' }
            @{ wait = 'ScannerSessionList' }
        )
    }
    @{
        Name = 'workbench-location'
        File = 'Scanner-Workbench-Location.png'
        NeedsData = $false
        Steps = @(
            @{ nav = 'Scanner' }
            @{ invoke = 'ScannerSearchByToggle' }
            @{ keys = 'esc' }
            @{ wait = 'Enter location' }
        )
    }
    @{
        Name = 'workbench-range'
        File = 'Scanner-Workbench-Location.png'
        NeedsData = $false
        Steps = @(
            @{ nav = 'Scanner' }
            @{ invoke = 'ScannerSearchByToggle' }
            @{ invoke = 'ScannerLocationRangeToggle' }
            @{ keys = 'esc' }
            @{ wait = 'ScannerRangeStartBox' }
        )
    }
    @{
        Name = 'workbench-parts-at-location'
        File = 'Scanner-Workbench-Location-List.png'
        NeedsData = $true
        Steps = @(
            @{ nav = 'Scanner' }
            @{ invoke = 'ScannerSearchByToggle' }
            @{ set = @('Enter location', 'RECV') }
            @{ keys = 'enter' }
            @{ wait = 'Parts at RECV' }
        )
    }
    @{
        Name = 'workbench-stock-picker'
        File = 'Scanner-Workbench-Location-List.png'
        NeedsData = $true
        Steps = @(
            @{ nav = 'Scanner' }
            @{ invoke = 'ScannerWorkbenchTab' }
            @{ set = @('Enter part number', 'MMC0000994') }
            @{ keys = 'enter' }
            @{ wait = 'Stock locations for MMC0000994' }
        )
    }
    @{
        Name = 'workbench-range-picker'
        File = 'Scanner-Workbench-Location-List.png'
        NeedsData = $true
        Steps = @(
            @{ nav = 'Scanner' }
            @{ invoke = 'ScannerSearchByToggle' }
            @{ invoke = 'ScannerLocationRangeToggle' }
            @{ set = @('ScannerRangeStartBox', 'V-A0-01') }
            @{ set = @('ScannerRangeStopBox', 'V-A0-05') }
            @{ invoke = 'ScannerLoadRangeButton' }
            @{ wait = 'Use Selected' }
        )
    }
    @{
        Name = 'workbench-with-rows'
        File = 'Scanner-Workbench-Location-LineData.png'
        NeedsData = $true
        Steps = @(
            @{ nav = 'Scanner' }
            @{ invoke = 'ScannerSearchByToggle' }
            @{ set = @('Enter location', 'RECV') }
            @{ keys = 'enter' }
            @{ wait = 'Parts at RECV' }
            @{ invoke = 'Select All' }
            @{ invoke = 'Use Selected' }
            @{ keys = 'esc' }
            @{ wait = 'ScannerSessionList' }
        )
    }
    @{
        Name = 'workbench-row-status'
        File = 'Scanner-Workbench-Location-LineData.png'
        NeedsData = $true
        Steps = @(
            @{ nav = 'Scanner' }
            @{ invoke = 'ScannerSearchByToggle' }
            @{ set = @('Enter location', 'RECV') }
            @{ keys = 'enter' }
            @{ wait = 'Parts at RECV' }
            @{ invoke = 'Select All' }
            @{ invoke = 'Use Selected' }
            @{ keys = 'esc' }
            @{ wait = 'ScannerSessionList' }
            @{ invoke = 'ScannerRowStatusButton' }
            @{ wait = 'Line status' }
        )
    }
    @{
        Name = 'workbench-manage-items'
        File = 'Scanner-Workbench-Location-LineData.png'
        NeedsData = $true
        Steps = @(
            @{ nav = 'Scanner' }
            @{ invoke = 'ScannerManageItemsButton' }
            @{ wait = 'Manage Items' }
        )
    }
    @{
        Name = 'workbench-help'
        File = 'Scanner-Workbench-Part.png'
        NeedsData = $false
        Steps = @(
            @{ nav = 'Scanner' }
            @{ invoke = 'ScannerHelpButton' }
            @{ wait = 'Scanner' }
        )
    }
    @{
        Name = 'history-main'
        File = 'Scanner-History.png'
        NeedsData = $false
        Steps = @(
            @{ nav = 'Scanner' }
            @{ invoke = 'ScannerHistoryTab' }
            @{ keys = 'esc' }
            @{ wait = 'ScannerHistoryHost' }
        )
    }
    @{
        Name = 'settings-main'
        File = 'Scanner-Settings.png'
        NeedsData = $false
        Steps = @(
            @{ nav = 'Scanner' }
            @{ invoke = 'ScannerSettingsTab' }
            @{ keys = 'esc' }
            @{ wait = 'ScannerSettingsHost' }
        )
    }
)

if ($List) {
    '{0,-30} {1,-6} {2}' -f 'SCENE', 'DATA', 'OUTPUT'
    '-' * 92
    foreach ($s in $scenes) {
        '{0,-30} {1,-6} {2}' -f $s.Name, $(if ($s.NeedsData) { 'live' } else { 'none' }), $s.File
    }
    'Manual only (needs a hand-driven state): View_Scanner_Workbench-DuplicateLines,'
    '-FixIssues, -PartNoStock.'
    return
}

function Invoke-WinApp {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]] $Arguments)

    Write-Verbose ('winapp ' + ($Arguments -join ' '))
    $text = ((& winapp @Arguments 2>&1) | Out-String).Trim()

    if ($LASTEXITCODE -ne 0) {
        throw "winapp failed ($LASTEXITCODE): $($Arguments -join ' ')`n$text"
    }

    Write-Verbose $text
    return $text
}

function Find-AppWindow {
    param([int] $Id)

    $process = if ($Id) {
        Get-Process -Id $Id -ErrorAction SilentlyContinue
    } else {
        Get-Process -Name 'MTM_Receiving_Application' -ErrorAction SilentlyContinue |
            Where-Object { $_.MainWindowHandle -ne 0 } |
            Select-Object -First 1
    }

    if (-not $process) {
        throw 'MTM Receiving Application is not running. Start it, sign in, and re-run this script.'
    }

    for ($attempt = 0; $attempt -lt 20; $attempt++) {
        $process.Refresh()
        if ($process.MainWindowHandle -ne 0) {
            return [pscustomobject]@{ Id = $process.Id; Handle = [int64] $process.MainWindowHandle }
        }
        Start-Sleep -Milliseconds 500
    }

    throw "Process $($process.Id) has no main window."
}

function Invoke-Step {
    param(
        [hashtable] $Step,
        [pscustomobject] $Window
    )

    $handle = "$($Window.Handle)"

    if ($Step.ContainsKey('nav')) {
        $found = Invoke-WinApp -Command @('ui','search',$Step.nav,'-w',$handle)
        $slug = ([regex]'(itm-[\w-]+)').Match($found).Value
        if (-not $slug) { throw "Could not resolve navigation item '$($Step.nav)'." }
        Invoke-WinApp -Command @('ui','invoke','-w',$handle,$slug) | Out-Null
        Start-Sleep -Seconds $SettleSeconds
    }
    elseif ($Step.ContainsKey('invoke')) {
        Invoke-WinApp -Command @('ui','invoke','-w',$handle,$Step.invoke) | Out-Null
        Start-Sleep -Seconds $SettleSeconds
    }
    elseif ($Step.ContainsKey('set')) {
        Invoke-WinApp -Command @('ui','set-value','-w',$handle,$Step.set[0],$Step.set[1]) | Out-Null
        Start-Sleep -Milliseconds 600
    }
    elseif ($Step.ContainsKey('keys')) {
        Invoke-WinApp -Command @('ui','send-keys',$Step.keys,'-w',$handle,'--via','send-input') | Out-Null
        Start-Sleep -Seconds $SettleSeconds
    }
    elseif ($Step.ContainsKey('wait')) {
        Invoke-WinApp -Command @('ui','wait-for','-w',$handle,$Step.wait,'--timeout','8000') | Out-Null
    }
}

$window = Find-AppWindow -Id $ProcessId
Write-Host "Attached to PID $($window.Id), window $($window.Handle)."

$requested = if ($Scene) {
    $Scene
} elseif ($All) {
    @($scenes | ForEach-Object { $_.Name })
} else {
    @($scenes | Where-Object { -not $_.NeedsData } | ForEach-Object { $_.Name })
}

$selected = foreach ($name in $requested) {
    $definition = $scenes | Where-Object { $_.Name -eq $name }
    if (-not $definition) { throw "Unknown scene '$name'. Run with -List to see the names." }
    $definition
}

$failures = 0
foreach ($definition in $selected) {
    $target = Join-Path $sourceRoot $definition.File
    $directory = Split-Path -Parent $target
    if (-not (Test-Path $directory)) { New-Item -ItemType Directory -Path $directory -Force | Out-Null }

    Write-Host "-> $($definition.Name)"
    try {
        foreach ($step in $definition.Steps) {
            Invoke-Step -Step $step -Window $window
        }
        Invoke-WinApp -Command @('ui','screenshot','-w',"$($window.Handle)",'-o',$target) | Out-Null
        Write-Host "   saved $($definition.File)"
    }
    catch {
        $failures++
        Write-Warning "   $($definition.Name) failed: $($_.Exception.Message)"
        # Leave the app in a known state before the next scene.
        try { Invoke-WinApp -Command @('ui','send-keys','esc','-w',"$($window.Handle)",'--via','send-input') | Out-Null } catch { }
    }
}

$captured = @($selected).Count - $failures
Write-Host ''
Write-Host "$captured of $(@($selected).Count) scene(s) captured into tools\scanner-screenshots\source."
Write-Host "Next: python tools\scanner-screenshots\annotate.py"
if ($failures) {
    Write-Warning "$failures scene(s) failed. Re-run with -Verbose to see the failing UI Automation step."
    exit 1
}
