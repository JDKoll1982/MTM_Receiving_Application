#Requires -Version 7.0
<#
.SYNOPSIS
    Publishes the MTM Receiving Application as a payload the MTM Loader can launch and update.

.DESCRIPTION
    The loader (C:\Users\jkoll\source\repos\MTM_Loader_WinUI3) reads an application payload from one
    shared folder and keeps a local copy in step with it. It needs three things this script produces:

      1. An unpackaged, self-contained publish, so the payload runs on a floor machine that has
         neither the .NET runtime nor the Windows App SDK installed. The loader only copies a
         folder; it cannot install a runtime.
      2. The launcher executable at the payload root, named exactly `MTM_Receiving_Application.exe`,
         which is what the loader catalogue names.
      3. A `build-manifest.json` at the payload root listing every file with its size and SHA-256.
         Without that manifest the loader treats the application as NOT updateable.

    Order matters and mirrors the loader's own release rule: every payload file is copied first and
    the manifest is written last, so a half-copied release is never advertised as complete.

    Nothing is deleted from the destination. The loader never mirror-deletes and neither does this
    script; stale files are reported so a person can decide about them.

.PARAMETER Version
    The 3-part application version to publish. Must match `Application.Version` in appsettings.json.

.PARAMETER DestinationRoot
    The application master folder the loader reads. Defaults to the MTM Application Store on the
    Material Handler share.

.PARAMETER StagingDirectory
    Local folder the publish is built into before it is copied. Defaults to a folder under TEMP.

.PARAMETER ApplicationId
    Catalogue id of the application. Must match `catalogue.json`.

.PARAMETER Executable
    Launcher file name, as named in the catalogue. File name only.

.PARAMETER SkipPublish
    Reuse the existing staging output instead of running `dotnet publish` again.

.PARAMETER FullVerify
    Re-read every published file from the destination and compare its SHA-256 with the manifest.
    Slower over a network share, but proves the manifest every client will verify against.

.PARAMETER Force
    Overwrite a destination that already holds a different version without asking.

.EXAMPLE
    ./Publish-App-To-Loader.ps1 -Version 4.6.0 -WhatIf

.EXAMPLE
    ./Publish-App-To-Loader.ps1 -Version 4.6.0 -FullVerify
#>
[CmdletBinding(SupportsShouldProcess, ConfirmImpact = 'High')]
param(
    [Parameter(Mandatory)][ValidatePattern('^\d+\.\d+\.\d+$')][string]$Version,
    [string]$DestinationRoot = '\\mtmanu-fs01\Expo Drive\MH_RESOURCE\Material_Handler\MTM Application Store\MTM Receiving Application',
    [string]$StagingDirectory = (Join-Path $env:TEMP 'mtm-receiving-loader-publish'),
    [string]$ApplicationId = 'receiving',
    [string]$Executable = 'MTM_Receiving_Application.exe',
    [switch]$SkipPublish,
    [switch]$FullVerify,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$projectPath = Join-Path $repoRoot 'MTM_Receiving_Application.csproj'
$settingsPath = Join-Path $repoRoot 'appsettings.json'

function Write-Step { param([string]$Message) Write-Host "[publish] $Message" -ForegroundColor Cyan }
function Write-Detail { param([string]$Message) Write-Host "          $Message" }
function Stop-WithFailure { param([string]$Message) throw "Publish refused: $Message" }

# --- Pre-flight -------------------------------------------------------------

if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    Stop-WithFailure "project not found at $projectPath"
}

$declaredVersion = (Get-Content -LiteralPath $settingsPath -Raw | ConvertFrom-Json).Application.Version
if ($declaredVersion -ne $Version) {
    Stop-WithFailure "appsettings.json declares $declaredVersion but $Version was requested. Bump the version sources first."
}

if ($Executable -match '[\\/]' -or $Executable.Contains('..')) {
    Stop-WithFailure "Executable must be a file name only, because the catalogue names it that way."
}

Write-Step "Version $Version, application id '$ApplicationId', executable '$Executable'"
Write-Detail "staging: $StagingDirectory"
Write-Detail "destination: $DestinationRoot"

# --- Stage ------------------------------------------------------------------

if ($SkipPublish) {
    if (-not (Test-Path -LiteralPath $StagingDirectory -PathType Container)) {
        Stop-WithFailure "SkipPublish was requested but $StagingDirectory does not exist."
    }

    Write-Step 'Reusing the existing staging output.'
}
else {
    if (Test-Path -LiteralPath $StagingDirectory) {
        Write-Step 'Clearing the previous staging folder.'
        Remove-Item -LiteralPath $StagingDirectory -Recurse -Force
    }

    # Unpackaged and self-contained, matching the loader's own publish shape. Single-file and
    # trimming stay off: neither is supported for a WinUI 3 desktop app.
    $publishArgs = @(
        'publish', $projectPath,
        '-c', 'Release',
        '-r', 'win-x64',
        '--self-contained', 'true',
        '-p:Platform=x64',
        '-p:WindowsPackageType=None',
        '-p:WindowsAppSDKSelfContained=true',
        '-p:PublishSingleFile=false',
        '-p:PublishTrimmed=false',
        '-o', $StagingDirectory
    )

    Write-Step "dotnet $($publishArgs -join ' ')"
    if ($PSCmdlet.ShouldProcess($StagingDirectory, 'dotnet publish')) {
        & dotnet @publishArgs
        if ($LASTEXITCODE -ne 0) {
            Stop-WithFailure "dotnet publish exited with $LASTEXITCODE."
        }
    }
    else {
        Write-Step 'WhatIf: publish skipped, so nothing further can be checked.'
        return
    }
}

$exePath = Join-Path $StagingDirectory $Executable
if (-not (Test-Path -LiteralPath $exePath -PathType Leaf)) {
    Stop-WithFailure "the staged payload has no $Executable at its root. The loader catalogue names that file."
}

$stagedFiles = Get-ChildItem -LiteralPath $StagingDirectory -Recurse -File
if ($stagedFiles.Count -eq 0) {
    Stop-WithFailure 'the staged payload is empty.'
}

Write-Step "Staged $($stagedFiles.Count) files, $('{0:N1}' -f (($stagedFiles | Measure-Object Length -Sum).Sum / 1MB)) MB."

# --- Build the manifest -----------------------------------------------------

Write-Step 'Hashing the staged files for build-manifest.json.'
$files = [System.Collections.Generic.List[object]]::new()
foreach ($file in $stagedFiles) {
    $relative = [System.IO.Path]::GetRelativePath($StagingDirectory, $file.FullName) -replace '\\', '/'
    $files.Add([ordered]@{
        path   = $relative
        size   = $file.Length
        sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    })
}

$files = @($files | Sort-Object { $_.path })
$totalBytes = [long]0
foreach ($entry in $files) {
    $totalBytes += [long]$entry.size
}

$manifest = [ordered]@{
    schemaVersion = 1
    applicationId = $ApplicationId
    version       = $Version
    publishedUtc  = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
    executable    = $Executable
    fileCount     = $files.Count
    totalBytes    = $totalBytes
    files         = $files
}

$manifestJson = $manifest | ConvertTo-Json -Depth 4 -Compress:$false
$stagedManifestPath = Join-Path $StagingDirectory 'build-manifest.json'
Set-Content -LiteralPath $stagedManifestPath -Value $manifestJson -Encoding utf8

# --- Destination checks -----------------------------------------------------

if (-not (Test-Path -LiteralPath $DestinationRoot -PathType Container)) {
    Stop-WithFailure "destination folder $DestinationRoot is not reachable."
}

$existingManifestPath = Join-Path $DestinationRoot 'build-manifest.json'
if (Test-Path -LiteralPath $existingManifestPath -PathType Leaf) {
    $existing = Get-Content -LiteralPath $existingManifestPath -Raw | ConvertFrom-Json
    Write-Step "Destination currently holds version $($existing.version)."
    if ($existing.version -eq $Version -and -not $Force) {
        $different = $existing.fileCount -ne $manifest.fileCount
        if ($different) {
            Stop-WithFailure "destination already holds $Version with a different payload. A version is never reused with different files; bump the version or pass -Force."
        }

        Write-Step "Destination already holds $Version with the same file count. Re-publishing the same payload."
    }
}

# Stale files are reported, never removed: the loader never mirror-deletes and a deletion here
# could take away a file a running client still needs.
if (Test-Path -LiteralPath $DestinationRoot -PathType Container) {
    $existingFiles = Get-ChildItem -LiteralPath $DestinationRoot -Recurse -File
    $incoming = [System.Collections.Generic.HashSet[string]]::new([string[]]@($files.path), [StringComparer]::OrdinalIgnoreCase)
    $stale = foreach ($file in $existingFiles) {
        $relative = [System.IO.Path]::GetRelativePath($DestinationRoot, $file.FullName) -replace '\\', '/'
        if ($relative -eq 'build-manifest.json') { continue }
        if (-not $incoming.Contains($relative)) { $relative }
    }

    if ($stale) {
        $staleList = @($stale)
        Write-Host "[publish] $($staleList.Count) file(s) exist at the destination and are not in this payload:" -ForegroundColor Yellow
        $staleList | Select-Object -First 15 | ForEach-Object { Write-Host "          $_" -ForegroundColor Yellow }
        if ($staleList.Count -gt 15) {
            Write-Host "          ... and $($staleList.Count - 15) more" -ForegroundColor Yellow
        }
        Write-Host '          They are left in place. Remove them by hand if the release should not carry them.' -ForegroundColor Yellow
    }
}

# --- Copy: payload first, manifest last ------------------------------------

if (-not $PSCmdlet.ShouldProcess($DestinationRoot, "Publish $($manifest.fileCount) files for version $Version")) {
    return
}

New-Item -ItemType Directory -Force -Path $DestinationRoot | Out-Null

Write-Step 'Copying payload files (the manifest is written last).'
$total = $files.Count
$index = 0
foreach ($entry in $files) {
    $index++
    $source = Join-Path $StagingDirectory ($entry.path -replace '/', [System.IO.Path]::DirectorySeparatorChar)
    $target = Join-Path $DestinationRoot ($entry.path -replace '/', [System.IO.Path]::DirectorySeparatorChar)
    $targetDirectory = Split-Path -Parent $target
    if (-not (Test-Path -LiteralPath $targetDirectory -PathType Container)) {
        New-Item -ItemType Directory -Force -Path $targetDirectory | Out-Null
    }

    Copy-Item -LiteralPath $source -Destination $target -Force
    if ($index % 100 -eq 0 -or $index -eq $total) {
        Write-Detail "copied $index / $total"
    }
}

Write-Step 'Verifying the copied payload against the manifest.'
$mismatches = [System.Collections.Generic.List[string]]::new()
foreach ($entry in $files) {
    $target = Join-Path $DestinationRoot ($entry.path -replace '/', [System.IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $target -PathType Leaf)) {
        $mismatches.Add("$($entry.path): missing")
        continue
    }

    $length = (Get-Item -LiteralPath $target).Length
    if ($length -ne $entry.size) {
        $mismatches.Add("$($entry.path): size $length, expected $($entry.size)")
        continue
    }

    if ($FullVerify) {
        $hash = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($hash -ne $entry.sha256) {
            $mismatches.Add("$($entry.path): hash differs")
        }
    }
}

if ($mismatches.Count -gt 0) {
    $preview = ($mismatches | Select-Object -First 10) -join '; '
    Stop-WithFailure "$($mismatches.Count) file(s) did not verify and the manifest was NOT written, so no client will be offered this release. $preview"
}

# Manifest last, so a half-copied release is never advertised.
Write-Step 'Writing build-manifest.json last, so the release is only advertised once it is complete.'
Copy-Item -LiteralPath $stagedManifestPath -Destination $existingManifestPath -Force

$published = Get-Content -LiteralPath $existingManifestPath -Raw | ConvertFrom-Json
Write-Step "Published $ApplicationId $($published.version): $($published.fileCount) files, $('{0:N1}' -f ($published.totalBytes / 1MB)) MB."
Write-Detail "manifest: $existingManifestPath"
Write-Detail 'The loader will now treat the application as updateable on its next check.'
