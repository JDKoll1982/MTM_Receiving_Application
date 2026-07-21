# MTM Receiving Application - Publish Tool (WPF GUI)
# Select a publish option; notes are shown alongside; click Publish to run dotnet publish.

# Publishing from this tool requires elevated privileges in this environment.
# If not elevated, re-launch immediately as Administrator and exit this process.
$windowsIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
$windowsPrincipal = [Security.Principal.WindowsPrincipal]::new($windowsIdentity)
$isAdministrator = $windowsPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdministrator) {
    $psExe = (Get-Process -Id $PID).MainModule.FileName
    $elevatedArgs = @('-STA', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $PSCommandPath) + $args

    try {
        $elevatedProcess = Start-Process -FilePath $psExe -ArgumentList $elevatedArgs -Verb RunAs -PassThru -Wait
        if ($null -ne $elevatedProcess) {
            exit $elevatedProcess.ExitCode
        }

        exit 0
    }
    catch {
        Write-Error "Administrator privileges are required to run the Publish GUI. Elevation was cancelled or failed."
        exit 1
    }
}

# WPF requires an STA thread. The VS Code PowerShell Extension REPL runs MTA and will
# crash with exit code 0xE0434352 if WPF is loaded there. This guard re-launches the
# script in a dedicated pwsh -STA process automatically.
if ([System.Threading.Thread]::CurrentThread.ApartmentState -ne 'STA') {
    $psExe = (Get-Process -Id $PID).MainModule.FileName
    & $psExe -STA -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath @args
    exit $LASTEXITCODE
}

Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName WindowsBase
Add-Type -AssemblyName System.Windows.Forms

# ---------------------------------------------------------------------------
# Notes strings (defined separately to avoid heredoc nesting issues)
# ---------------------------------------------------------------------------
$n1 = "RECOMMENDED for server-share deployment.`n`nBundles the .NET 10 runtime and Windows App SDK alongside the app. No software needs to be installed on any user PC. Users launch straight from the network share using their desktop shortcut.`n`n  WHEN TO CHOOSE THIS:`n  - Zero prerequisites on user PCs`n  - Most reliable launch from a network share`n  - Everything needed is in the output folder`n  - Safe for all WinUI 3 features`n`n  WHEN TO AVOID:`n  - Disk space on the share is severely constrained`n  - IT centrally manages runtimes via Intune/SCCM"

$n2 = "NOT RECOMMENDED unless IT manages runtimes centrally.`n`nPublishes only the app code - no runtime bundled. The .NET 10 Desktop Runtime and Windows App SDK must already be installed on every PC that runs the app.`n`n  WHY YOU MIGHT STILL CHOOSE THIS:`n  - IT manages workstations via Intune/SCCM - runtimes are always present`n  - Publish output is significantly smaller, so share updates are faster`n  - .NET security patches handled by IT - no full app republish needed`n  - Disk space on the share is tightly constrained`n`n  DO NOT USE IF:`n  - Any user PC may not have the runtimes installed`n  - You cannot confirm deployment status with IT"

$n4 = "EXPERIMENTAL for WinUI 3 - not recommended for routine production publishes.`n`nReadyToRun pre-compiles assemblies to native code at publish time, but this project currently treats the option as unstable because WinUI 3 COM interop issues have caused runtime problems in prior validation.`n`n  WHEN TO CONSIDER THIS:`n  - You are doing an intentional performance experiment`n  - You can fully validate startup and core workflows before deployment`n  - You understand this option is not the normal supported publish path`n`n  RISKS:`n  - May reintroduce WinUI 3 COM interop failures`n  - Output folder is larger than standard self-contained`n  - Should be treated as an advanced troubleshooting or benchmarking option only"

$n5 = "HIGH RISK with WinUI 3 - test thoroughly before deploying.`n`nRemoves unused assemblies and types to reduce output folder size. WinUI 3 relies heavily on reflection and dynamic type loading, which conflicts with aggressive trimming.`n`n  WHY YOU MIGHT STILL CHOOSE THIS:`n  - Server share is on a slow/VPN link and folder size affects launch time`n  - Disk space on the share is severely constrained`n  - All trim warnings resolved and a full end-to-end test pass is done`n  - Smallest possible artifact for automated deployment pipelines`n`n  RISKS:`n  - Runtime failures from missing types are common with WinUI 3`n  - ALWAYS test every screen before deploying to users`n  - Do not use as the primary production build until fully validated"

$repoRoot = Split-Path -Path (Split-Path -Path $PSScriptRoot -Parent) -Parent
$defaultProjectFile = Join-Path $repoRoot "MTM_Receiving_Application.csproj"

# ---------------------------------------------------------------------------
# Publish option definitions - mirrors PublishAppScript.md
# ---------------------------------------------------------------------------
$script:BaseShare = "X:\MH_RESOURCE\Material_Handler"
$script:OutputRootPath = $script:BaseShare
$script:ProjectFile = $defaultProjectFile
$script:PublishVerbosity = 'normal'
$script:SatelliteResourceLanguages = 'en-US'
$script:PublishLogFile = $null
$script:LastStatusMessage = ''
$script:PublishLogBuffer = $null
$script:PublishProcess = $null
$script:PollTimer = $null
$script:OutFile = $null
$script:PublishStagingPath = $null
$script:selectedOption = $null
$script:LastSyncProgressRender = [datetime]::MinValue
$script:PublishCompletionHandled = $false
$script:IsPublishInProgress = $false
$script:IsSyncInProgress = $false
$script:SyncProcess = $null
$script:SyncPollTimer = $null
$script:SyncOutFile = $null
$script:SyncLastPos = 0L
$script:ExistingBuildAvailability = $null
$script:ProtectedDeploymentRelativePaths = @(
    'Assets\DunnageImages',
    'Assets/DunnageImages'
)
$script:OutputRootPathStartupMessage = ''

function Resolve-StartupOutputRootPath {
    param(
        [string]$PreferredPath
    )

    $fallbackRoot = Join-Path $env:USERPROFILE 'Documents\MTM Receiving Application\Publish Output'

    if ([string]::IsNullOrWhiteSpace($PreferredPath)) {
        return [pscustomobject]@{
            Path = [System.IO.Path]::GetFullPath($fallbackRoot)
            Message = "Default publish share path is empty. Using local fallback: $fallbackRoot"
        }
    }

    if (Test-Path -LiteralPath $PreferredPath) {
        return [pscustomobject]@{
            Path = [System.IO.Path]::GetFullPath($PreferredPath)
            Message = ''
        }
    }

    if ($PreferredPath -match '^(?<Drive>[A-Za-z]):\\(?<Rest>.*)$') {
        $driveLetter = $matches.Drive.ToUpperInvariant()
        $relativeRest = $matches.Rest
        $networkKey = "HKCU:\Network\$driveLetter"

        if (Test-Path -LiteralPath $networkKey) {
            $remotePath = (Get-ItemProperty -LiteralPath $networkKey -Name RemotePath -ErrorAction SilentlyContinue).RemotePath
            if (-not [string]::IsNullOrWhiteSpace($remotePath)) {
                $uncPath = if ([string]::IsNullOrWhiteSpace($relativeRest)) {
                    $remotePath
                }
                else {
                    Join-Path $remotePath $relativeRest
                }

                return [pscustomobject]@{
                    Path = $uncPath
                    Message = "Mapped drive $driveLetter`: is not available in this elevated session. Using UNC path instead: $uncPath"
                }
            }
        }
    }

    return [pscustomobject]@{
        Path = [System.IO.Path]::GetFullPath($fallbackRoot)
        Message = "Mapped publish share path '$PreferredPath' is not available. Using local fallback: $fallbackRoot"
    }
}

function Get-SatelliteLanguageOptions {
    $languageOptions = New-Object System.Collections.Generic.List[System.Windows.Controls.ComboBoxItem]

    $allLanguagesItem = New-Object System.Windows.Controls.ComboBoxItem
    $allLanguagesItem.Content = 'All available languages'
    $allLanguagesItem.Tag = ''
    [void]$languageOptions.Add($allLanguagesItem)

    $cultureTypes = [System.Globalization.CultureTypes]::NeutralCultures -bor [System.Globalization.CultureTypes]::SpecificCultures
    $seenCultures = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($culture in ([System.Globalization.CultureInfo]::GetCultures($cultureTypes) | Where-Object { -not [string]::IsNullOrWhiteSpace($_.Name) } | Sort-Object EnglishName, Name)) {
        if (-not $seenCultures.Add($culture.Name)) {
            continue
        }

        $cultureItem = New-Object System.Windows.Controls.ComboBoxItem
        $cultureItem.Content = '{0} [{1}]' -f $culture.EnglishName, $culture.Name
        $cultureItem.Tag = $culture.Name
        [void]$languageOptions.Add($cultureItem)
    }

    return $languageOptions
}

function Get-SelectedSatelliteLanguageCode {
    if ($null -eq $satelliteLanguagesComboBox) {
        return ''
    }

    if ($satelliteLanguagesComboBox.SelectedItem -isnot [System.Windows.Controls.ComboBoxItem]) {
        return ''
    }

    return [string]$satelliteLanguagesComboBox.SelectedItem.Tag
}

function Set-SelectedSatelliteLanguage {
    param(
        [string]$LanguageCode
    )

    if ($null -eq $satelliteLanguagesComboBox) {
        return
    }

    $normalizedLanguageCode = if ([string]::IsNullOrWhiteSpace($LanguageCode)) {
        ''
    }
    else {
        $LanguageCode.Trim()
    }

    foreach ($item in $satelliteLanguagesComboBox.Items) {
        if ($item -is [System.Windows.Controls.ComboBoxItem] -and [string]::Equals([string]$item.Tag, $normalizedLanguageCode, [System.StringComparison]::OrdinalIgnoreCase)) {
            $satelliteLanguagesComboBox.SelectedItem = $item
            return
        }
    }

    $satelliteLanguagesComboBox.SelectedIndex = 0
}

function Test-PathIsEqualOrChild {
    param(
        [string]$BasePath,
        [string]$CandidatePath
    )

    if ([string]::IsNullOrWhiteSpace($BasePath) -or [string]::IsNullOrWhiteSpace($CandidatePath)) {
        return $false
    }

    try {
        $normalizedBase = [System.IO.Path]::GetFullPath($BasePath).TrimEnd('\', '/')
        $normalizedCandidate = [System.IO.Path]::GetFullPath($CandidatePath).TrimEnd('\', '/')
    }
    catch {
        return $false
    }

    if ([string]::Equals($normalizedBase, $normalizedCandidate, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }

    $candidateWithSeparator = "$normalizedCandidate\"
    $baseWithSeparator = "$normalizedBase\"
    return $candidateWithSeparator.StartsWith($baseWithSeparator, [System.StringComparison]::OrdinalIgnoreCase)
}

function Get-OutputPathValidationResult {
    param(
        [string]$OutputRootPath,
        [hashtable]$Option
    )

    if ([string]::IsNullOrWhiteSpace($OutputRootPath)) {
        return [pscustomobject]@{
            IsValid = $false
            Message = 'Choose a publish output folder before publishing.'
        }
    }

    try {
        $resolvedRoot = [System.IO.Path]::GetFullPath($OutputRootPath)
    }
    catch {
        return [pscustomobject]@{
            IsValid = $false
            Message = 'The selected publish output folder is not a valid path.'
        }
    }

    try {
        $resolvedDestination = if ($null -eq $Option) {
            $resolvedRoot
        }
        else {
            [System.IO.Path]::GetFullPath((Join-Path $resolvedRoot $Option.Folder))
        }
    }
    catch {
        return [pscustomobject]@{
            IsValid = $false
            Message = 'The selected publish output folder could not be combined with the publish option folder. Verify the output path and drive availability.'
        }
    }

    if (Test-PathIsEqualOrChild -BasePath $repoRoot -CandidatePath $resolvedRoot) {
        return [pscustomobject]@{
            IsValid = $false
            Message = 'The publish output folder cannot be inside the source-code repository.'
        }
    }

    $stagingRoot = Join-Path $env:LOCALAPPDATA 'MTM Receiving Application\PublishTool\Staging'
    if (Test-PathIsEqualOrChild -BasePath $stagingRoot -CandidatePath $resolvedRoot) {
        return [pscustomobject]@{
            IsValid = $false
            Message = 'The publish output folder cannot be the tool''s temporary staging folder.'
        }
    }

    return [pscustomobject]@{
        IsValid = $true
        Message = ''
        OutputRootPath = $resolvedRoot
        DestinationPath = $resolvedDestination
    }
}

function Test-PublishOutputDirectoryHasFiles {
    param(
        [string]$Path,
        [string[]]$IgnoreNames = @()
    )

    if ([string]::IsNullOrWhiteSpace($Path) -or -not (Test-Path -LiteralPath $Path)) {
        return $false
    }

    foreach ($item in (Get-ChildItem -LiteralPath $Path -Force -ErrorAction SilentlyContinue)) {
        if ($IgnoreNames -contains $item.Name) {
            continue
        }

        return $true
    }

    return $false
}

function Confirm-SelfContainedOverwrite {
    param(
        [string]$Path,
        [string]$OptionLabel
    )

    $message = @"
The publish output folder already contains files:

$Path

The selected option is self-contained: $OptionLabel

Select Yes to clear the folder first and republish every self-contained file.
Select No to keep existing matching self-contained files and publish only new or changed output.
"@

    $result = [System.Windows.MessageBox]::Show(
        $message,
        'Existing Self-Contained Publish Detected',
        [System.Windows.MessageBoxButton]::YesNo,
        [System.Windows.MessageBoxImage]::Warning)

    return $result -eq [System.Windows.MessageBoxResult]::Yes
}

function New-PublishStagingDirectory {
    $workingRoot = Join-Path $env:LOCALAPPDATA "MTM Receiving Application\PublishTool\Staging"
    New-Item -ItemType Directory -Path $workingRoot -Force | Out-Null
    $stagingPath = Join-Path $workingRoot ((Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $stagingPath -Force | Out-Null
    return $stagingPath
}

function Get-PublishLogDirectory {
    $logDirectory = Join-Path $env:LOCALAPPDATA "MTM Receiving Application\PublishTool\Logs"
    New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
    return $logDirectory
}

function Remove-PublishStagingDirectory {
    if ([string]::IsNullOrWhiteSpace($script:PublishStagingPath)) {
        return
    }

    if (Test-Path -LiteralPath $script:PublishStagingPath) {
        Remove-Item -LiteralPath $script:PublishStagingPath -Recurse -Force -ErrorAction SilentlyContinue
    }

    $script:PublishStagingPath = $null
}

function Test-FilesMatch {
    param(
        [string]$SourcePath,
        [string]$DestinationPath
    )

    if (-not (Test-Path -LiteralPath $SourcePath) -or -not (Test-Path -LiteralPath $DestinationPath)) {
        return $false
    }

    $sourceItem = Get-Item -LiteralPath $SourcePath -ErrorAction Stop
    $destinationItem = Get-Item -LiteralPath $DestinationPath -ErrorAction Stop

    if ($sourceItem.Length -ne $destinationItem.Length) {
        return $false
    }

    $sourceHash = Get-FileHash -LiteralPath $SourcePath -Algorithm SHA256 -ErrorAction Stop
    $destinationHash = Get-FileHash -LiteralPath $DestinationPath -Algorithm SHA256 -ErrorAction Stop
    return $sourceHash.Hash -eq $destinationHash.Hash
}

function Sync-PublishOutputDirectory {
    param(
        [string]$SourcePath,
        [string]$DestinationPath,
        [string[]]$PreserveNames = @(),
        [scriptblock]$ProgressAction = $null
    )

    if ([string]::IsNullOrWhiteSpace($SourcePath) -or -not (Test-Path -LiteralPath $SourcePath)) {
        throw "The staged publish output path does not exist: $SourcePath"
    }

    New-Item -ItemType Directory -Path $DestinationPath -Force | Out-Null

    $sourceDirectories = @(Get-ChildItem -LiteralPath $SourcePath -Directory -Force -Recurse -ErrorAction SilentlyContinue)
    $sourceFiles = @(Get-ChildItem -LiteralPath $SourcePath -File -Force -Recurse -ErrorAction SilentlyContinue)
    $sourceRelativePaths = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $targetItems = @(Get-ChildItem -LiteralPath $DestinationPath -Force -Recurse -ErrorAction SilentlyContinue | Sort-Object FullName -Descending)
    $totalSteps = $targetItems.Count + $sourceDirectories.Count + $sourceFiles.Count
    if ($totalSteps -le 0) {
        $totalSteps = 1
    }

    $currentStep = 0

    if ($null -ne $ProgressAction) {
        & $ProgressAction $currentStep $totalSteps 'Preparing deployment target'
    }

    foreach ($directory in $sourceDirectories) {
        [void]$sourceRelativePaths.Add([System.IO.Path]::GetRelativePath($SourcePath, $directory.FullName))
    }

    foreach ($file in $sourceFiles) {
        [void]$sourceRelativePaths.Add([System.IO.Path]::GetRelativePath($SourcePath, $file.FullName))
    }

    $removedItems = 0
    foreach ($targetItem in $targetItems) {
        $relativePath = [System.IO.Path]::GetRelativePath($DestinationPath, $targetItem.FullName)
        $topLevelName = ($relativePath -split '[\\/]', 2)[0]

        if ($PreserveNames -contains $topLevelName) {
            $currentStep++
            if ($null -ne $ProgressAction) {
                & $ProgressAction $currentStep $totalSteps 'Reviewing deployment target'
            }

            continue
        }

        if (-not $sourceRelativePaths.Contains($relativePath)) {
            Remove-Item -LiteralPath $targetItem.FullName -Recurse -Force -ErrorAction Stop
            $removedItems++
        }

        $currentStep++
        if ($null -ne $ProgressAction) {
            & $ProgressAction $currentStep $totalSteps 'Removing stale files'
        }
    }

    foreach ($directory in ($sourceDirectories | Sort-Object FullName)) {
        $relativePath = [System.IO.Path]::GetRelativePath($SourcePath, $directory.FullName)
        $destinationDirectory = Join-Path $DestinationPath $relativePath
        New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null

        $currentStep++
        if ($null -ne $ProgressAction) {
            & $ProgressAction $currentStep $totalSteps 'Preparing folders'
        }
    }

    $copiedFiles = 0
    $skippedFiles = 0
    foreach ($file in $sourceFiles) {
        $relativePath = [System.IO.Path]::GetRelativePath($SourcePath, $file.FullName)
        $destinationFile = Join-Path $DestinationPath $relativePath
        $destinationDirectory = Split-Path -Path $destinationFile -Parent

        if (-not [string]::IsNullOrWhiteSpace($destinationDirectory)) {
            New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null
        }

        if (Test-FilesMatch -SourcePath $file.FullName -DestinationPath $destinationFile) {
            $skippedFiles++
            $currentStep++
            if ($null -ne $ProgressAction) {
                & $ProgressAction $currentStep $totalSteps 'Checking unchanged files'
            }

            continue
        }

        Copy-Item -LiteralPath $file.FullName -Destination $destinationFile -Force
        $copiedFiles++

        $currentStep++
        if ($null -ne $ProgressAction) {
            & $ProgressAction $currentStep $totalSteps 'Copying publish files'
        }
    }

    if ($currentStep -lt $totalSteps -and $null -ne $ProgressAction) {
        $currentStep = $totalSteps
        & $ProgressAction $currentStep $totalSteps 'Finalizing sync'
    }

    return [pscustomobject]@{
        CopiedFiles  = $copiedFiles
        SkippedFiles = $skippedFiles
        RemovedItems = $removedItems
    }
}

function Get-Utf8Encoding {
    return [System.Text.UTF8Encoding]::new($false)
}

function Get-SelectedPublishVerbosity {
    if ($null -eq $detailedLoggingCheckBox -or $true -ne [bool]$detailedLoggingCheckBox.IsChecked) {
        return $script:PublishVerbosity
    }

    return 'detailed'
}

function Get-ShowExperimentalOptions {
    if ($null -eq $showExperimentalOptionsCheckBox) {
        return $false
    }

    return $true -eq [bool]$showExperimentalOptionsCheckBox.IsChecked
}

function Get-AutoMigrateToSharedDrive {
    if ($null -eq $autoMigrateCheckBox) {
        return $true
    }

    return $true -eq [bool]$autoMigrateCheckBox.IsChecked
}

function Get-UseExistingBuildOutput {
    if ($null -eq $useExistingBuildCheckBox) {
        return $false
    }

    return $true -eq [bool]$useExistingBuildCheckBox.IsChecked
}

function Get-WindowsSdkAvailability {
    $registryCandidates = @(
        'HKLM:\SOFTWARE\Microsoft\Windows Kits\Installed Roots',
        'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows Kits\Installed Roots',
        'HKLM:\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v10.0'
    )

    foreach ($registryPath in $registryCandidates) {
        if (-not (Test-Path -LiteralPath $registryPath)) {
            continue
        }

        try {
            $registryValues = Get-ItemProperty -LiteralPath $registryPath -ErrorAction Stop
        }
        catch {
            continue
        }

        $kitsRootCandidate = if (-not [string]::IsNullOrWhiteSpace([string]$registryValues.KitsRoot10)) {
            [string]$registryValues.KitsRoot10
        }
        elseif (-not [string]::IsNullOrWhiteSpace([string]$registryValues.InstallationFolder)) {
            [string]$registryValues.InstallationFolder
        }
        else {
            $null
        }

        if ([string]::IsNullOrWhiteSpace($kitsRootCandidate)) {
            continue
        }

        try {
            $kitsRoot = [System.IO.Path]::GetFullPath($kitsRootCandidate)
        }
        catch {
            continue
        }

        $includeRoot = Join-Path $kitsRoot 'Include'
        $libRoot = Join-Path $kitsRoot 'Lib'
        $hasIncludes = Test-Path -LiteralPath $includeRoot
        $hasLibs = Test-Path -LiteralPath $libRoot

        if ($hasIncludes -and $hasLibs) {
            return [pscustomobject]@{
                IsAvailable = $true
                RegistryPath = $registryPath
                KitsRoot = $kitsRoot
                Reason = ''
            }
        }
    }

    return [pscustomobject]@{
        IsAvailable = $false
        RegistryPath = ''
        KitsRoot = ''
        Reason = 'Windows SDK 10/11 was not found in the expected registry locations.'
    }
}

function Get-TargetPlatformVersionFromTargetFramework {
    param(
        [string]$TargetFramework
    )

    if ([string]::IsNullOrWhiteSpace($TargetFramework)) {
        return ''
    }

    if ($TargetFramework -match 'windows(?<Version>\d+(?:\.\d+){1,3})$') {
        return $matches.Version
    }

    return ''
}

function Get-WindowsSdkPreflightResult {
    param(
        [bool]$UseExistingBuild,
        [string]$ProjectPath
    )

    $availability = Get-WindowsSdkAvailability
    $targetPlatformVersion = ''

    if (-not [string]::IsNullOrWhiteSpace($ProjectPath) -and (Test-Path -LiteralPath $ProjectPath)) {
        try {
            $projectXml = [xml](Get-Content -LiteralPath $ProjectPath -Raw -ErrorAction Stop)
            $propertyGroups = @($projectXml.Project.PropertyGroup)
            $targetFramework = ($propertyGroups | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_.TargetFramework) } | Select-Object -First 1).TargetFramework
            if ([string]::IsNullOrWhiteSpace($targetFramework)) {
                $targetFrameworks = ($propertyGroups | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_.TargetFrameworks) } | Select-Object -First 1).TargetFrameworks
                if (-not [string]::IsNullOrWhiteSpace($targetFrameworks)) {
                    $targetFramework = ($targetFrameworks -split ';' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1)
                }
            }

            $targetPlatformVersion = Get-TargetPlatformVersionFromTargetFramework -TargetFramework $targetFramework
        }
        catch {
            $targetPlatformVersion = ''
        }
    }

    if ($availability.IsAvailable) {
        if (-not [string]::IsNullOrWhiteSpace($targetPlatformVersion)) {
            $platformXmlPath = Join-Path $availability.KitsRoot (Join-Path 'Platforms\UAP' (Join-Path $targetPlatformVersion 'Platform.xml'))
            if (-not (Test-Path -LiteralPath $platformXmlPath)) {
                $availableVersions = @()
                $uapRoot = Join-Path $availability.KitsRoot 'Platforms\UAP'
                if (Test-Path -LiteralPath $uapRoot) {
                    $availableVersions = @(Get-ChildItem -LiteralPath $uapRoot -Directory -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Name | Sort-Object -Descending)
                }

                $availablePreview = if ($availableVersions.Count -gt 0) {
                    ($availableVersions | Select-Object -First 5) -join ', '
                }
                else {
                    'none detected'
                }

                $message = "The project targets Windows SDK $targetPlatformVersion, but Platform.xml was not found at:`n$platformXmlPath`n`nDetected UAP platform versions: $availablePreview`n`nInstall Windows SDK 10.0.$targetPlatformVersion (or change the project target to a supported installed SDK version)."

                if ($UseExistingBuild) {
                    return [pscustomobject]@{
                        IsReady = $true
                        IsWarning = $true
                        Message = "$message`n`nContinuing because 'Reuse existing build output' is enabled (--no-build)."
                    }
                }

                return [pscustomobject]@{
                    IsReady = $false
                    IsWarning = $false
                    Message = $message
                }
            }
        }

        return [pscustomobject]@{
            IsReady = $true
            IsWarning = $false
            Message = ''
        }
    }

    if ($UseExistingBuild) {
        return [pscustomobject]@{
            IsReady = $true
            IsWarning = $true
            Message = "Windows SDK registry entries were not found. Continuing because 'Reuse existing build output' is enabled (--no-build). If publish still fails, install the Windows 10/11 SDK from Visual Studio Installer."
        }
    }

    return [pscustomobject]@{
        IsReady = $false
        IsWarning = $false
        Message = "Publish requires the Windows 10/11 SDK, but it was not found on this machine.`n`nFix options:`n  1) Install 'Windows 10/11 SDK' from Visual Studio Installer (Individual components), then retry.`n  2) Build the project successfully in Visual Studio, enable 'Reuse existing build output', and publish with --no-build."
    }
}

function Get-ProjectPublishMetadata {
    param(
        [string]$ProjectPath
    )

    if ([string]::IsNullOrWhiteSpace($ProjectPath) -or -not (Test-Path -LiteralPath $ProjectPath)) {
        return $null
    }

    try {
        $projectXml = [xml](Get-Content -LiteralPath $ProjectPath -Raw -ErrorAction Stop)
        $propertyGroups = @($projectXml.Project.PropertyGroup)

        $targetFramework = ($propertyGroups | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_.TargetFramework) } | Select-Object -First 1).TargetFramework
        if ([string]::IsNullOrWhiteSpace($targetFramework)) {
            $targetFrameworks = ($propertyGroups | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_.TargetFrameworks) } | Select-Object -First 1).TargetFrameworks
            if (-not [string]::IsNullOrWhiteSpace($targetFrameworks)) {
                $targetFramework = ($targetFrameworks -split ';' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1)
            }
        }

        $platforms = ($propertyGroups | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_.Platforms) } | Select-Object -First 1).Platforms
        $platformTarget = ($propertyGroups | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_.PlatformTarget) } | Select-Object -First 1).PlatformTarget
        $runtimeIdentifiers = ($propertyGroups | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_.RuntimeIdentifiers) } | Select-Object -First 1).RuntimeIdentifiers

        $platform = $null
        if (-not [string]::IsNullOrWhiteSpace($platforms)) {
            $platform = ($platforms -split ';' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1)
        }

        if ([string]::IsNullOrWhiteSpace($platform)) {
            $platform = $platformTarget
        }

        $runtimeIdentifier = $null
        if (-not [string]::IsNullOrWhiteSpace($runtimeIdentifiers)) {
            $runtimeIdentifier = ($runtimeIdentifiers -split ';' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1)
        }

        return [pscustomobject]@{
            TargetFramework  = [string]$targetFramework
            Platform         = [string]$platform
            RuntimeIdentifier = [string]$runtimeIdentifier
            ProjectDirectory = Split-Path -Path $ProjectPath -Parent
            ProjectName      = [System.IO.Path]::GetFileNameWithoutExtension($ProjectPath)
        }
    }
    catch {
        return $null
    }
}

function Get-PublishOptionBuildSettings {
    param(
        [hashtable]$Option,
        [pscustomobject]$ProjectMetadata
    )

    if ($null -eq $Option) {
        return $null
    }

    $configuration = 'Release'
    $runtimeIdentifier = if ($null -ne $ProjectMetadata -and -not [string]::IsNullOrWhiteSpace($ProjectMetadata.RuntimeIdentifier)) {
        $ProjectMetadata.RuntimeIdentifier
    }
    else {
        'win-x64'
    }

    if ($Option.Args -match '(?<!\S)-c\s+(?<Configuration>[^\s]+)') {
        $configuration = $matches.Configuration
    }

    if ($Option.Args -match '(?<!\S)-r\s+(?<RuntimeIdentifier>[^\s]+)') {
        $runtimeIdentifier = $matches.RuntimeIdentifier
    }

    return [pscustomobject]@{
        Configuration    = [string]$configuration
        RuntimeIdentifier = [string]$runtimeIdentifier
        RequiresPublishBuild = ($Option.Args -match 'PublishReadyToRun=true|PublishTrimmed=true')
    }
}

function Test-DirectoryHasFilesRecursively {
    param(
        [string]$Path
    )

    if ([string]::IsNullOrWhiteSpace($Path) -or -not (Test-Path -LiteralPath $Path)) {
        return $false
    }

    return $null -ne (Get-ChildItem -LiteralPath $Path -File -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1)
}

function Get-CompatibleExistingBuildAvailability {
    param(
        [string]$ProjectPath,
        [hashtable]$Option
    )

    if ($null -eq $Option) {
        return [pscustomobject]@{
            IsAvailable = $false
            BuildOutputPath = ''
            Reason = 'Select a publish option to check whether an existing compatible build can be reused.'
        }
    }

    $projectMetadata = Get-ProjectPublishMetadata -ProjectPath $ProjectPath
    if ($null -eq $projectMetadata) {
        return [pscustomobject]@{
            IsAvailable = $false
            BuildOutputPath = ''
            Reason = 'Enter a valid project path before checking for an existing compatible build.'
        }
    }

    $buildSettings = Get-PublishOptionBuildSettings -Option $Option -ProjectMetadata $projectMetadata
    if ($buildSettings.RequiresPublishBuild) {
        return [pscustomobject]@{
            IsAvailable = $false
            BuildOutputPath = ''
            Reason = 'This publish option changes publish-time compilation, so it must build during publish and cannot safely reuse the current bin output.'
        }
    }

    if ([string]::IsNullOrWhiteSpace($projectMetadata.TargetFramework) -or [string]::IsNullOrWhiteSpace($projectMetadata.Platform)) {
        return [pscustomobject]@{
            IsAvailable = $false
            BuildOutputPath = ''
            Reason = 'Could not determine the target framework and platform from the selected project file.'
        }
    }

    $binCandidates = @(
        (Join-Path $projectMetadata.ProjectDirectory (Join-Path 'bin' (Join-Path $projectMetadata.Platform (Join-Path $buildSettings.Configuration (Join-Path $projectMetadata.TargetFramework $buildSettings.RuntimeIdentifier))))),
        (Join-Path $projectMetadata.ProjectDirectory (Join-Path 'bin' (Join-Path $projectMetadata.Platform (Join-Path $buildSettings.Configuration $projectMetadata.TargetFramework))))
    ) | Select-Object -Unique

    $objCandidates = @(
        (Join-Path $projectMetadata.ProjectDirectory (Join-Path 'obj' (Join-Path $projectMetadata.Platform (Join-Path $buildSettings.Configuration (Join-Path $projectMetadata.TargetFramework $buildSettings.RuntimeIdentifier))))),
        (Join-Path $projectMetadata.ProjectDirectory (Join-Path 'obj' (Join-Path $projectMetadata.Platform (Join-Path $buildSettings.Configuration $projectMetadata.TargetFramework))))
    ) | Select-Object -Unique

    foreach ($binCandidate in $binCandidates) {
        if (-not (Test-DirectoryHasFilesRecursively -Path $binCandidate)) {
            continue
        }

        foreach ($objCandidate in $objCandidates) {
            if (-not (Test-DirectoryHasFilesRecursively -Path $objCandidate)) {
                continue
            }

            return [pscustomobject]@{
                IsAvailable = $true
                BuildOutputPath = $binCandidate
                Reason = "Compatible existing build found under $binCandidate"
            }
        }
    }

    $expectedPath = $binCandidates | Select-Object -First 1
    return [pscustomobject]@{
        IsAvailable = $false
        BuildOutputPath = ''
        Reason = "No compatible existing build was found for $($buildSettings.Configuration) / $($projectMetadata.TargetFramework) / $($buildSettings.RuntimeIdentifier). Build the project first so $expectedPath contains the matching output."
    }
}

function Update-ExistingBuildCheckboxState {
    if ($null -eq $useExistingBuildCheckBox -or $null -eq $existingBuildStatusText) {
        return
    }

    $availability = Get-CompatibleExistingBuildAvailability -ProjectPath $projectPathText.Text -Option $script:selectedOption
    $script:ExistingBuildAvailability = $availability

    if ($availability.IsAvailable) {
        $wasDisabled = -not $useExistingBuildCheckBox.IsEnabled
        $useExistingBuildCheckBox.IsEnabled = $true
        if ($wasDisabled -or $null -eq $useExistingBuildCheckBox.IsChecked) {
            $useExistingBuildCheckBox.IsChecked = $false
        }

        $useExistingBuildCheckBox.ToolTip = "Reuse the compatible existing build output from $($availability.BuildOutputPath) by adding --no-build to dotnet publish."
        $existingBuildStatusText.Text = "Compatible existing build found: $($availability.BuildOutputPath)"
        $existingBuildStatusText.Foreground = $brushConverter.ConvertFromString('#2E7D32')
        return
    }

    $useExistingBuildCheckBox.IsChecked = $false
    $useExistingBuildCheckBox.IsEnabled = $false
    $useExistingBuildCheckBox.ToolTip = $availability.Reason
    $existingBuildStatusText.Text = $availability.Reason
    $existingBuildStatusText.Foreground = $brushConverter.ConvertFromString('#8A6D3B')
}

function Add-PublishLogText {
    param(
        [string]$Text
    )

    if ($null -eq $script:PublishLogBuffer -or [string]::IsNullOrEmpty($Text)) {
        return
    }

    [void]$script:PublishLogBuffer.Append($Text)
}

function Save-PublishLog {
    if ([string]::IsNullOrEmpty($script:PublishLogFile) -or $null -eq $script:PublishLogBuffer) {
        return
    }

    $logDirectory = Split-Path -Path $script:PublishLogFile -Parent
    if (-not [string]::IsNullOrWhiteSpace($logDirectory)) {
        New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
    }

    [System.IO.File]::WriteAllText($script:PublishLogFile, $script:PublishLogBuffer.ToString(), (Get-Utf8Encoding))
}

function Get-EffectiveOutputPath {
    param(
        [hashtable]$Option
    )

    if ($null -eq $Option) {
        return $script:OutputRootPath
    }

    try {
        return Join-Path $script:OutputRootPath $Option.Folder
    }
    catch {
        return $script:OutputRootPath
    }
}

function Clear-PublishOutputDirectory {
    param(
        [string]$Path,
        [string[]]$PreserveNames = @()
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return
    }

    New-Item -ItemType Directory -Path $Path -Force | Out-Null

    $items = Get-ChildItem -LiteralPath $Path -Force -ErrorAction SilentlyContinue
    foreach ($item in $items) {
        if ($PreserveNames -contains $item.Name) {
            continue
        }

        Remove-Item -LiteralPath $item.FullName -Recurse -Force -ErrorAction Stop
    }
}

function Update-OutputPathDisplay {
    if ($null -eq $outputPathText) {
        return
    }

    $outputPathText.Text = Get-EffectiveOutputPath -Option $script:selectedOption
}

function Confirm-SyncStagedPublishOutput {
    param(
        [string]$DestinationPath,
        [string]$StagingPath,
        [string]$OptionLabel
    )

    $message = @"
The publish completed successfully in the local staging folder:

$StagingPath

Selected option: $OptionLabel
Deployment target: $DestinationPath

Select Yes to sync the staged output to the deployment target now.
Select No to keep the staged output locally and sync it later.
"@

    $result = [System.Windows.MessageBox]::Show(
        $message,
        'Sync Staged Publish Output',
        [System.Windows.MessageBoxButton]::YesNo,
        [System.Windows.MessageBoxImage]::Question)

    return $result -eq [System.Windows.MessageBoxResult]::Yes
}

function Update-PublishOptionList {
    if ($null -eq $optionList) {
        return
    }

    $selectedOptionIndex = if ($null -ne $script:selectedOption) {
        $script:selectedOption.Index
    }
    else {
        $null
    }

    $optionList.Items.Clear()

    foreach ($opt in ($script:Options | Where-Object { -not $_.Experimental -or (Get-ShowExperimentalOptions) })) {
        $item = New-Object System.Windows.Controls.ListBoxItem
        $item.Padding = "10,7"
        $item.Tag = $opt.Index

        $sp = New-Object System.Windows.Controls.StackPanel

        $labelBlock = New-Object System.Windows.Controls.TextBlock
        $labelBlock.Text = $opt.Label
        $labelBlock.FontWeight = [System.Windows.FontWeights]::SemiBold
        $labelBlock.TextWrapping = [System.Windows.TextWrapping]::Wrap
        $labelBlock.Foreground = $brushConverter.ConvertFromString($opt.TagColor)

        $folderBlock = New-Object System.Windows.Controls.TextBlock
        $folderBlock.Text = $opt.Folder
        $folderBlock.FontSize = 10
        $folderBlock.FontFamily = New-Object System.Windows.Media.FontFamily("Consolas")
        $folderBlock.Foreground = $brushConverter.ConvertFromString("#888888")
        $folderBlock.Margin = New-Object System.Windows.Thickness(0, 2, 0, 0)

        [void]$sp.Children.Add($labelBlock)
        [void]$sp.Children.Add($folderBlock)
        $item.Content = $sp
        [void]$optionList.Items.Add($item)
    }

    if ($null -ne $selectedOptionIndex) {
        foreach ($item in $optionList.Items) {
            if ([int]$item.Tag -eq $selectedOptionIndex) {
                $optionList.SelectedItem = $item
                return
            }
        }
    }

    $script:selectedOption = $null

    if ($null -ne $notesText) {
        $notesText.Text = 'Select a publish option on the left to see notes here.'
    }

    if ($null -ne $publishButton) {
        $publishButton.IsEnabled = $false
    }

    Set-PublishProgressState -Visible $false
    Update-OutputPathDisplay
    Update-PublishStatus 'Ready. Select a publish option above and click Publish.'
}

function Select-OutputRootFolder {
    $dialog = New-Object System.Windows.Forms.FolderBrowserDialog
    $dialog.Description = 'Choose the folder where the publish output should be created.'
    $dialog.ShowNewFolderButton = $true

    if (Test-Path -LiteralPath $script:OutputRootPath) {
        $dialog.SelectedPath = $script:OutputRootPath
    }

    try {
        if ($dialog.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK -and -not [string]::IsNullOrWhiteSpace($dialog.SelectedPath)) {
            $validationResult = Get-OutputPathValidationResult -OutputRootPath $dialog.SelectedPath -Option $script:selectedOption
            if (-not $validationResult.IsValid) {
                [System.Windows.MessageBox]::Show(
                    $validationResult.Message,
                    'Unsafe Publish Output Folder',
                    [System.Windows.MessageBoxButton]::OK,
                    [System.Windows.MessageBoxImage]::Warning) | Out-Null
                Update-PublishStatus $validationResult.Message
                return
            }

            $script:OutputRootPath = $validationResult.OutputRootPath
            Update-OutputPathDisplay
            Update-PublishStatus 'Publish output folder updated.'
        }
    }
    finally {
        $dialog.Dispose()
    }
}

function Stop-PublishSession {
    if ($null -ne $script:PollTimer) {
        try {
            $script:PollTimer.Stop()
        }
        catch {
        }

        $script:PollTimer = $null
    }

    if ($null -ne $script:PublishProcess) {
        try {
            $script:PublishProcess.Dispose()
        }
        catch {
        }

        $script:PublishProcess = $null
    }

    if (-not [string]::IsNullOrWhiteSpace($script:OutFile)) {
        try {
            Remove-Item -LiteralPath $script:OutFile -Force -ErrorAction SilentlyContinue
        }
        catch {
        }

        $script:OutFile = $null
    }
}

function Reset-SyncWorker {
    if ($null -ne $script:SyncPollTimer) {
        try {
            $script:SyncPollTimer.Stop()
        }
        catch {
        }

        $script:SyncPollTimer = $null
    }

    if ($null -ne $script:SyncProcess) {
        try {
            $script:SyncProcess.Dispose()
        }
        catch {
        }

        $script:SyncProcess = $null
    }

    if (-not [string]::IsNullOrWhiteSpace($script:SyncOutFile)) {
        try {
            Remove-Item -LiteralPath $script:SyncOutFile -Force -ErrorAction SilentlyContinue
        }
        catch {
        }

        $script:SyncOutFile = $null
    }

    $script:SyncLastPos = 0L
}

function Set-OperationUiState {
    param(
        [bool]$IsBusy
    )

    foreach ($control in @(
            $optionList,
            $browseOutputPathButton,
            $projectPathText,
            $satelliteLanguagesComboBox,
            $detailedLoggingCheckBox,
            $showExperimentalOptionsCheckBox,
            $useExistingBuildCheckBox,
            $autoMigrateCheckBox)) {
        if ($null -ne $control) {
            $control.IsEnabled = -not $IsBusy
        }
    }

    if ($null -ne $publishButton) {
        $publishButton.IsEnabled = (-not $IsBusy) -and ($null -ne $script:selectedOption)
    }

    if ($null -ne $closeButton) {
        $closeButton.IsEnabled = -not $IsBusy
    }
}

function Update-PublishStatus {
    param(
        [string]$Message,
        [switch]$SkipLog
    )

    $statusText.Text = $Message

    if ([string]::IsNullOrWhiteSpace($Message) -or $Message -eq $script:LastStatusMessage) {
        return
    }

    $script:LastStatusMessage = $Message

    if ($SkipLog) {
        return
    }

    Add-PublishLogText "[STATUS $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $Message`r`n"
}

function Set-PublishProgressState {
    param(
        [bool]$Visible,
        [bool]$IsIndeterminate = $true,
        [double]$Value = 0,
        [double]$Maximum = 100
    )

    if ($null -eq $publishProgress) {
        return
    }

    $publishProgress.Minimum = 0
    $publishProgress.Maximum = if ($Maximum -gt 0) {
        $Maximum
    }
    else {
        100
    }
    $publishProgress.IsIndeterminate = $IsIndeterminate
    $publishProgress.Value = if ($IsIndeterminate) {
        0
    }
    else {
        [Math]::Min([Math]::Max($Value, 0), $publishProgress.Maximum)
    }
    $publishProgress.Visibility = if ($Visible) {
        [System.Windows.Visibility]::Visible
    }
    else {
        [System.Windows.Visibility]::Collapsed
    }
}

function Invoke-PublishUiRefresh {
    if ($null -eq $window -or $null -eq $window.Dispatcher) {
        return
    }

    $frame = New-Object System.Windows.Threading.DispatcherFrame
    $callback = [System.Windows.Threading.DispatcherOperationCallback]{
        param($state)

        $state.Continue = $false
        return $null
    }

    $null = $window.Dispatcher.BeginInvoke(
        [System.Windows.Threading.DispatcherPriority]::Background,
        $callback,
        $frame)
    [System.Windows.Threading.Dispatcher]::PushFrame($frame)
}

function Update-SyncProgress {
    param(
        [int]$CompletedSteps,
        [int]$TotalSteps,
        [string]$Phase,
        [switch]$Force
    )

    $safeTotal = [Math]::Max($TotalSteps, 1)
    $safeCompleted = [Math]::Min([Math]::Max($CompletedSteps, 0), $safeTotal)
    $now = Get-Date

    if (-not $Force -and $safeCompleted -lt $safeTotal -and (($now - $script:LastSyncProgressRender).TotalMilliseconds -lt 125)) {
        return
    }

    $script:LastSyncProgressRender = $now
    $percentComplete = [int][Math]::Floor(($safeCompleted / [double]$safeTotal) * 100)

    Set-PublishProgressState -Visible $true -IsIndeterminate $false -Value $safeCompleted -Maximum $safeTotal
    Update-PublishStatus -Message "Syncing staged publish output to the deployment target... $Phase ($safeCompleted of $safeTotal, $percentComplete%)" -SkipLog
    Invoke-PublishUiRefresh
}

function Start-PublishOutputSync {
    param(
        [string]$StagingPath,
        [string]$DestinationPath,
        [hashtable]$Option,
        [int]$PublishExitCode
    )

    if ([string]::IsNullOrWhiteSpace($StagingPath) -or -not (Test-Path -LiteralPath $StagingPath)) {
        throw "The staged publish output path is not available for sync: $StagingPath"
    }

    Reset-SyncWorker

    $script:IsSyncInProgress = $true
    Set-OperationUiState -IsBusy $true
    $script:LastSyncProgressRender = [datetime]::MinValue

    Set-PublishProgressState -Visible $true -IsIndeterminate $true
    Update-PublishStatus "Starting migration of the staged publish output to the shared drive..."
    Add-PublishLogText "`r`nSync Started`r`nDeployment target: $DestinationPath`r`n"

    $syncStagingPath = $StagingPath
    $syncDestinationPath = $DestinationPath
    $syncPublishExitCode = $PublishExitCode

    $script:SyncOutFile = [System.IO.Path]::GetTempFileName()
    $script:SyncLastPos = 0L
    $protectedDirectories = @('_PublishLogs') + $script:ProtectedDeploymentRelativePaths
    $quotedProtectedDirectories = @($protectedDirectories | ForEach-Object { "`"$_`"" }) -join ' '
    $syncCommand = "robocopy `"$syncStagingPath`" `"$syncDestinationPath`" /MIR /R:1 /W:1 /XJ /XD $quotedProtectedDirectories"
    $syncCmdArgs = "/c $syncCommand > `"$($script:SyncOutFile)`" 2>&1"

    $syncPsi = New-Object System.Diagnostics.ProcessStartInfo
    $syncPsi.FileName = 'cmd.exe'
    $syncPsi.Arguments = $syncCmdArgs
    $syncPsi.UseShellExecute = $false
    $syncPsi.CreateNoWindow = $true

    $script:SyncProcess = New-Object System.Diagnostics.Process
    $script:SyncProcess.StartInfo = $syncPsi
    $script:SyncProcess.Start() | Out-Null

    $script:SyncPollTimer = New-Object System.Windows.Threading.DispatcherTimer
    $script:SyncPollTimer.Interval = [TimeSpan]::FromMilliseconds(200)
    $script:SyncPollTimer.Add_Tick({
            try {
                $fs = [System.IO.File]::Open($script:SyncOutFile,
                    [System.IO.FileMode]::Open,
                    [System.IO.FileAccess]::Read,
                    [System.IO.FileShare]::ReadWrite)
                $fs.Seek($script:SyncLastPos, [System.IO.SeekOrigin]::Begin) | Out-Null
                $reader = New-Object System.IO.StreamReader($fs, [System.Text.Encoding]::UTF8)
                $newText = $reader.ReadToEnd()
                $script:SyncLastPos = $fs.Position
                $reader.Dispose()
                $fs.Dispose()

                if ($newText.Length -gt 0) {
                    $outputText.Text += $newText
                    Add-PublishLogText $newText
                    $lastLine = ($newText -split "`n" | Where-Object { $_.Trim() -ne '' } | Select-Object -Last 1)
                    if ($lastLine) {
                        Update-PublishStatus $lastLine.Trim()
                    }

                    $outputScrollViewer.ScrollToEnd()
                }
            }
            catch {
            }

            if (-not $script:SyncProcess.HasExited) {
                return
            }

            $script:SyncPollTimer.Stop()

            try {
                $fs = [System.IO.File]::Open($script:SyncOutFile,
                    [System.IO.FileMode]::Open,
                    [System.IO.FileAccess]::Read,
                    [System.IO.FileShare]::ReadWrite)
                $fs.Seek($script:SyncLastPos, [System.IO.SeekOrigin]::Begin) | Out-Null
                $reader = New-Object System.IO.StreamReader($fs, [System.Text.Encoding]::UTF8)
                $tail = $reader.ReadToEnd()
                $reader.Dispose()
                $fs.Dispose()

                if ($tail.Length -gt 0) {
                    $outputText.Text += $tail
                    Add-PublishLogText $tail
                }
            }
            catch {
            }

            $syncExitCode = $script:SyncProcess.ExitCode
            $script:IsSyncInProgress = $false
            Reset-SyncWorker
            Set-PublishProgressState -Visible $false
            Set-OperationUiState -IsBusy $false
            $outputScrollViewer.ScrollToEnd()

            if ($syncExitCode -ge 8) {
                $script:PublishStagingPath = $null
                $successBorder.Visibility = [System.Windows.Visibility]::Collapsed
                $errorBorder.Visibility = [System.Windows.Visibility]::Visible
                $errorText.Text = "Publish succeeded, but migrating the staged output failed. Staged output: $syncStagingPath`nLog file: $script:PublishLogFile`nRobocopy exit code: $syncExitCode"
                Update-PublishStatus 'Publish migration failed - check build output.'
                Add-PublishLogText "`r`nSync Failed: Robocopy exit code $syncExitCode`r`n"
                Add-PublishLogText "`r`nCompleted: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`r`nExitCode: $syncPublishExitCode`r`n"

                try {
                    Save-PublishLog
                }
                catch {
                }

                return
            }

            if (-not [string]::IsNullOrWhiteSpace($syncStagingPath) -and (Test-Path -LiteralPath $syncStagingPath)) {
                Remove-Item -LiteralPath $syncStagingPath -Recurse -Force -ErrorAction SilentlyContinue
            }

            $script:PublishStagingPath = $null
            $mergeSummaryText = "`nMigration completed to: $syncDestinationPath`nRobocopy exit code: $syncExitCode"
            Add-PublishLogText "`r`nSync Summary`r`nRobocopy exit code: $syncExitCode`r`n"
            $errorBorder.Visibility = [System.Windows.Visibility]::Collapsed
            $successBorder.Visibility = [System.Windows.Visibility]::Visible
            $successText.Text = "Publish succeeded!`nOutput folder: $syncDestinationPath`nLog file: $script:PublishLogFile$mergeSummaryText"
            $outputText.Text += "`r`nMigration completed to: $syncDestinationPath`r`n"
            Update-PublishStatus 'Publish and migration completed successfully.'
            Add-PublishLogText "`r`nCompleted: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`r`nExitCode: $syncPublishExitCode`r`n"

            try {
                Save-PublishLog
            }
            catch {
            }
        }.GetNewClosure())
    $script:SyncPollTimer.Start()
}

$script:Options = @(
    @{
        Index         = 0
        Label         = "1  Self-Contained  (Recommended)"
        Folder        = "MTM Receiving Application"
        Args          = "-c Release -r win-x64 --self-contained true"
        SelfContained = $true
        TagColor      = "#388E3C"
        Notes         = $n1
    },
    @{
        Index         = 1
        Label         = "2  Framework-Dependent  [Caution]"
        Folder        = "MTM Receiving Application FD"
        Args          = "-c Release -r win-x64 --self-contained false"
        SelfContained = $false
        TagColor      = "#E65100"
        Notes         = $n2
    },
    @{
        Index         = 2
        Label         = "3  ReadyToRun - Experimental"
        Folder        = "MTM Receiving Application R2R"
        Args          = "-c Release -r win-x64 --self-contained true -p:PublishReadyToRun=true"
        SelfContained = $true
        TagColor      = "#E65100"
        Experimental  = $true
        Notes         = $n4
    },
    @{
        Index         = 3
        Label         = "4  Trimmed  [Caution]  (High Risk)"
        Folder        = "MTM Receiving Application Trimmed"
        Args          = "-c Release -r win-x64 --self-contained true -p:PublishTrimmed=true"
        SelfContained = $true
        TagColor      = "#C62828"
        Experimental  = $false
        Notes         = $n5
    }
)

# ---------------------------------------------------------------------------
# XAML
# ---------------------------------------------------------------------------
$xaml = @"
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="MTM Receiving Application - Publish Tool"
        Height="860" Width="1180"
        MinHeight="760" MinWidth="1040"
        WindowStartupLocation="CenterScreen"
        ResizeMode="CanResizeWithGrip"
        Background="#F5F5F5">
    <Grid Margin="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="365"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Margin="0,0,0,18">
            <TextBlock Text="MTM Receiving Application"
                       FontSize="24" FontWeight="Bold" Foreground="#2196F3"/>
            <TextBlock Text="Publish Tool - select an option on the left, review the notes, then click Publish."
                       FontSize="14" Foreground="#666" Margin="0,4,0,0"/>
        </StackPanel>

        <!-- Options list + Notes side-by-side -->
        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="340"/>
                <ColumnDefinition Width="18"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>

            <!-- Left: option list -->
            <Border Grid.Column="0" Background="White" BorderBrush="#DDD"
                    BorderThickness="1" CornerRadius="6" Padding="0,4,0,4">
                <DockPanel>
                    <TextBlock DockPanel.Dock="Top" Text="Publish Options"
                               FontWeight="Bold" FontSize="14" Foreground="#444" Margin="14,12,14,8"/>
                    <ListBox Name="OptionList" BorderThickness="0" Background="Transparent"
                             ScrollViewer.HorizontalScrollBarVisibility="Disabled"
                             VirtualizingPanel.IsVirtualizing="False"/>
                </DockPanel>
            </Border>

            <!-- Right: notes + output path -->
            <Border Grid.Column="2" Background="White" BorderBrush="#DDD"
                    BorderThickness="1" CornerRadius="6" Padding="18">
                <DockPanel>
                    <TextBlock DockPanel.Dock="Top" Text="Option Notes"
                               FontWeight="Bold" FontSize="14" Foreground="#444" Margin="0,0,0,10"/>
                    <StackPanel DockPanel.Dock="Bottom" Margin="0,14,0,0">
                        <Separator Margin="0,0,0,12"/>
                        <TextBlock Text="Output Path" FontWeight="Bold" FontSize="13" Foreground="#444" Margin="0,0,0,6"/>
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <TextBox Grid.Column="0"
                                     Name="OutputPathText"
                                     IsReadOnly="True"
                                     FontFamily="Consolas"
                                     FontSize="12"
                                     Padding="7,6"
                                     BorderBrush="#CCC"
                                     BorderThickness="1"
                                     VerticalContentAlignment="Center"
                                     Text="(select an option)"/>
                            <Button Grid.Column="1"
                                    Name="BrowseOutputPathButton"
                                    Content="Browse..."
                                    Margin="10,0,0,0"
                                    MinWidth="108"
                                    Height="34"
                                    Padding="12,6"/>
                        </Grid>
                    </StackPanel>
                    <ScrollViewer VerticalScrollBarVisibility="Auto">
                        <TextBlock Name="NotesText" TextWrapping="Wrap"
                                   Foreground="#333" FontSize="13" LineHeight="21"
                                   Text="Select a publish option on the left to see notes here."/>
                    </ScrollViewer>
                </DockPanel>
            </Border>
        </Grid>

        <Grid Grid.Row="2" Margin="0,12,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="430"/>
                <ColumnDefinition Width="18"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>

            <!-- Project path / settings -->
            <Border Grid.Column="0" Background="White" BorderBrush="#DDD"
                    BorderThickness="1" CornerRadius="6" Padding="16">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="74"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                    </Grid.RowDefinitions>

                    <TextBlock Grid.Row="0" Grid.Column="0" Text="Project:" FontWeight="Bold" FontSize="13" VerticalAlignment="Center"/>
                    <TextBox Grid.Row="0" Grid.Column="1" Name="ProjectPathText"
                             FontFamily="Consolas" FontSize="12" Padding="7,6"
                             BorderBrush="#CCC" BorderThickness="1" VerticalContentAlignment="Center"/>

                    <TextBlock Grid.Row="1" Grid.Column="0" Text="Lang:" FontWeight="Bold" FontSize="13" VerticalAlignment="Center" Margin="0,12,0,0"/>
                    <ComboBox Grid.Row="1" Grid.Column="1" Name="SatelliteLanguagesComboBox"
                              Margin="0,12,0,0"
                              FontSize="12"
                              Padding="7,6"
                              BorderBrush="#CCC"
                              BorderThickness="1"
                              IsEditable="False"
                              IsTextSearchEnabled="True"
                              MaxDropDownHeight="320"
                              ToolTip="Optional. Pick one language to limit satellite resources, or leave the default option selected to publish all available languages."/>

                    <TextBlock Grid.Row="2" Grid.Column="0" Text="Extras:" FontWeight="Bold" FontSize="13" VerticalAlignment="Top" Margin="0,14,0,0"/>
                    <StackPanel Grid.Row="2" Grid.Column="1" Margin="0,14,0,0">
                        <CheckBox Name="AutoMigrateCheckBox"
                                  Content="Automatically migrate the staged publish output to the shared drive after publish"
                                  IsChecked="True"
                                  FontSize="12"
                                  ToolTip="When enabled, a successful publish immediately migrates the locally staged output to the selected shared-drive folder without prompting."/>
                        <CheckBox Name="UseExistingBuildCheckBox"
                                  Content="Use the existing compatible bin build when available (publish with --no-build)"
                                  Margin="0,10,0,0"
                                  IsChecked="False"
                                  IsEnabled="False"
                                  FontSize="12"
                                  ToolTip="Disabled until the script finds a compatible existing build for the selected publish option."/>
                        <TextBlock Name="ExistingBuildStatusText"
                                   Margin="24,5,0,0"
                                   FontSize="11"
                                   Foreground="#8A6D3B"
                                   TextWrapping="Wrap"
                                   LineHeight="18"
                                   Text="Select a publish option to check whether an existing compatible build can be reused."/>
                        <WrapPanel Margin="0,12,0,0" ItemHeight="28" ItemWidth="190">
                            <CheckBox Name="DetailedLoggingCheckBox"
                                      Content="Detailed logging"
                                      FontSize="12"
                                      Margin="0,0,14,6"
                                      ToolTip="Enable verbose publish output only when troubleshooting."/>
                            <CheckBox Name="ShowExperimentalOptionsCheckBox"
                                      Content="Show experimental options"
                                      FontSize="12"
                                      Margin="0,0,0,6"
                                      ToolTip="Displays advanced publish options such as ReadyToRun that are not part of the normal supported path."/>
                        </WrapPanel>
                    </StackPanel>
                </Grid>
            </Border>

            <!-- Status / build output -->
            <Border Grid.Column="2" Background="White" BorderBrush="#DDD"
                    BorderThickness="1" CornerRadius="6" Padding="16">
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="*"/>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                    </Grid.RowDefinitions>

                    <Grid Grid.Row="0" Margin="0,0,0,8">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                        </Grid.ColumnDefinitions>
                        <TextBlock Grid.Column="0" Name="StatusText"
                                   Text="Ready. Select an option above and click Publish."
                                   FontWeight="Bold" FontSize="14" Foreground="#555"
                                   VerticalAlignment="Center" TextWrapping="Wrap"/>
                        <ProgressBar Grid.Column="1" Name="PublishProgress"
                                     Width="170" Height="10"
                                     Margin="14,0,0,0"
                                     IsIndeterminate="True" Visibility="Collapsed"/>
                    </Grid>

                    <Border Grid.Row="1" Name="OutputBorder" Background="#1E1E1E" CornerRadius="4"
                            Margin="0,4,0,0" Visibility="Collapsed">
                        <ScrollViewer Name="OutputScrollViewer"
                                      MinHeight="260"
                                      HorizontalScrollBarVisibility="Disabled"
                                      VerticalScrollBarVisibility="Auto">
                            <TextBlock Name="OutputText" FontFamily="Consolas" FontSize="11"
                                       Foreground="#D4D4D4" TextWrapping="Wrap" Margin="10"/>
                        </ScrollViewer>
                    </Border>

                    <Border Grid.Row="2" Name="SuccessBorder" Background="#E8F5E9" BorderBrush="#4CAF50"
                            BorderThickness="1" CornerRadius="4" Padding="14"
                            Margin="0,10,0,0" Visibility="Collapsed">
                        <TextBlock Name="SuccessText" Foreground="#2E7D32"
                                   FontWeight="Bold" FontSize="13" TextWrapping="Wrap" LineHeight="20"/>
                    </Border>

                    <Border Grid.Row="3" Name="ErrorBorder" Background="#FFEBEE" BorderBrush="#F44336"
                            BorderThickness="1" CornerRadius="4" Padding="14"
                            Margin="0,10,0,0" Visibility="Collapsed">
                        <TextBlock Name="ErrorText" Foreground="#C62828" FontSize="13" TextWrapping="Wrap" LineHeight="20"/>
                    </Border>
                </Grid>
            </Border>
        </Grid>

        <!-- Buttons -->
        <StackPanel Grid.Row="3" Orientation="Horizontal"
                    HorizontalAlignment="Right" Margin="0,16,0,0">
            <Button Name="PublishButton" Content="Publish"
                    Width="144" Height="40"
                    Background="#2196F3" Foreground="White"
                    BorderThickness="0" FontWeight="Bold" FontSize="14"
                    Cursor="Hand" IsEnabled="False"/>
            <Button Name="CloseButton" Content="Close"
                    Width="110" Height="40" Margin="12,0,0,0"
                    Background="#9E9E9E" Foreground="White"
                    BorderThickness="0" Cursor="Hand"/>
        </StackPanel>
    </Grid>
</Window>
"@

# ---------------------------------------------------------------------------
# Load XAML
# ---------------------------------------------------------------------------
try {
    $reader = [System.Xml.XmlNodeReader]::new([xml]$xaml)
    $window = [Windows.Markup.XamlReader]::Load($reader)
}
catch {
    throw "Failed to load the publish window XAML. $($_.Exception.Message)"
}

if ($null -eq $window) {
    throw 'Failed to load the publish window XAML.'
}

# ---------------------------------------------------------------------------
# Bind controls
# ---------------------------------------------------------------------------
$optionList = $window.FindName("OptionList")
$notesText = $window.FindName("NotesText")
$outputPathText = $window.FindName("OutputPathText")
$browseOutputPathButton = $window.FindName("BrowseOutputPathButton")
$projectPathText = $window.FindName("ProjectPathText")
$satelliteLanguagesComboBox = $window.FindName("SatelliteLanguagesComboBox")
if ($null -eq $satelliteLanguagesComboBox -and $null -ne $window) {
    $satelliteLanguagesComboBox = [System.Windows.LogicalTreeHelper]::FindLogicalNode($window, "SatelliteLanguagesComboBox")
}
$autoMigrateCheckBox = $window.FindName("AutoMigrateCheckBox")
$useExistingBuildCheckBox = $window.FindName("UseExistingBuildCheckBox")
$existingBuildStatusText = $window.FindName("ExistingBuildStatusText")
$detailedLoggingCheckBox = $window.FindName("DetailedLoggingCheckBox")
$showExperimentalOptionsCheckBox = $window.FindName("ShowExperimentalOptionsCheckBox")
$statusText = $window.FindName("StatusText")
$publishProgress = $window.FindName("PublishProgress")
$outputBorder = $window.FindName("OutputBorder")
$outputScrollViewer = $window.FindName("OutputScrollViewer")
$outputText = $window.FindName("OutputText")
$successBorder = $window.FindName("SuccessBorder")
$successText = $window.FindName("SuccessText")
$errorBorder = $window.FindName("ErrorBorder")
$errorText = $window.FindName("ErrorText")
$publishButton = $window.FindName("PublishButton")
$closeButton = $window.FindName("CloseButton")

if ($null -eq $satelliteLanguagesComboBox) {
    throw "Could not find the SatelliteLanguagesComboBox control in the loaded publish window XAML."
}

$projectPathText.Text = $script:ProjectFile
$startupOutputRoot = Resolve-StartupOutputRootPath -PreferredPath $script:OutputRootPath
$script:OutputRootPath = $startupOutputRoot.Path
$script:OutputRootPathStartupMessage = $startupOutputRoot.Message
$satelliteLanguagesComboBox.Items.Clear()
foreach ($languageItem in (Get-SatelliteLanguageOptions)) {
    [void]$satelliteLanguagesComboBox.Items.Add($languageItem)
}
Set-SelectedSatelliteLanguage -LanguageCode $script:SatelliteResourceLanguages
Update-OutputPathDisplay

if (-not (Test-Path -LiteralPath $script:ProjectFile)) {
    Update-PublishStatus "Project file not found. Review the Project path before publishing."
}
elseif (-not [string]::IsNullOrWhiteSpace($script:OutputRootPathStartupMessage)) {
    Update-PublishStatus $script:OutputRootPathStartupMessage
}

# ---------------------------------------------------------------------------
# Populate option ListBox
# ---------------------------------------------------------------------------
$brushConverter = New-Object System.Windows.Media.BrushConverter
Update-PublishOptionList
Update-ExistingBuildCheckboxState
Set-OperationUiState -IsBusy $false

# ---------------------------------------------------------------------------
# Selection changed - update notes panel
# ---------------------------------------------------------------------------
$script:selectedOption = $null

$optionList.Add_SelectionChanged({
        $selectedItem = $optionList.SelectedItem
        if ($null -eq $selectedItem) { return }

        # Store directly so the publish handler never needs to re-derive it
        $script:selectedOption = $script:Options[[int]$selectedItem.Tag]

        $notesText.Text = $script:selectedOption.Notes
        Update-OutputPathDisplay

        $successBorder.Visibility = [System.Windows.Visibility]::Collapsed
        $errorBorder.Visibility = [System.Windows.Visibility]::Collapsed
        $outputBorder.Visibility = [System.Windows.Visibility]::Collapsed
        $outputText.Text = ""
        Set-PublishProgressState -Visible $false
        Update-PublishStatus "Ready to publish: $($script:selectedOption.Label)"
        $publishButton.IsEnabled = $true
        Update-ExistingBuildCheckboxState
    })

$browseOutputPathButton.Add_Click({
        Select-OutputRootFolder
    })

$showExperimentalOptionsCheckBox.Add_Click({
        Update-PublishOptionList
    Update-ExistingBuildCheckboxState
    })

$projectPathText.Add_TextChanged({
    Update-ExistingBuildCheckboxState
    })

# ---------------------------------------------------------------------------
# Publish button - run dotnet publish, show live output, show result
# ---------------------------------------------------------------------------
$publishButton.Add_Click({
        if ($null -eq $script:selectedOption) { return }

        Remove-PublishStagingDirectory

        $opt = $script:selectedOption
    $outputPathValidation = Get-OutputPathValidationResult -OutputRootPath $script:OutputRootPath -Option $opt
    if (-not $outputPathValidation.IsValid) {
        $script:PublishCompletionHandled = $false
        $script:IsPublishInProgress = $false
        $script:IsSyncInProgress = $false
        $errorBorder.Visibility = [System.Windows.Visibility]::Visible
        $errorText.Text = $outputPathValidation.Message
        $successBorder.Visibility = [System.Windows.Visibility]::Collapsed
        $outputBorder.Visibility = [System.Windows.Visibility]::Collapsed
        Update-PublishStatus 'Publish blocked - choose a safer output folder.'
        return
    }

    $script:OutputRootPath = $outputPathValidation.OutputRootPath
    $script:currentOutputPath = $outputPathValidation.DestinationPath
        $projectPath = $projectPathText.Text

        if ([string]::IsNullOrWhiteSpace($projectPath) -or -not (Test-Path -LiteralPath $projectPath)) {
            $script:PublishCompletionHandled = $false
            $script:IsPublishInProgress = $false
            $script:IsSyncInProgress = $false
            $errorBorder.Visibility = [System.Windows.Visibility]::Visible
            $errorText.Text = "The selected project file does not exist. Update the Project path to the current MTM_Receiving_Application.csproj before publishing."
            $successBorder.Visibility = [System.Windows.Visibility]::Collapsed
            $outputBorder.Visibility = [System.Windows.Visibility]::Collapsed
            Update-PublishStatus "Publish blocked - project file path is invalid."
            return
        }

        Update-ExistingBuildCheckboxState
        $useExistingBuild = $script:ExistingBuildAvailability.IsAvailable -and (Get-UseExistingBuildOutput)
        $windowsSdkPreflight = Get-WindowsSdkPreflightResult -UseExistingBuild:$useExistingBuild -ProjectPath $projectPath
        if (-not $windowsSdkPreflight.IsReady) {
            $script:PublishCompletionHandled = $false
            $script:IsPublishInProgress = $false
            $script:IsSyncInProgress = $false
            $errorBorder.Visibility = [System.Windows.Visibility]::Visible
            $errorText.Text = $windowsSdkPreflight.Message
            $successBorder.Visibility = [System.Windows.Visibility]::Collapsed
            $outputBorder.Visibility = [System.Windows.Visibility]::Collapsed
            Update-PublishStatus 'Publish blocked - Windows SDK prerequisites are missing.'
            return
        }

        if ($windowsSdkPreflight.IsWarning) {
            Update-PublishStatus $windowsSdkPreflight.Message
        }

        $script:PublishCompletionHandled = $false
        $script:IsPublishInProgress = $true
        $script:IsSyncInProgress = $false

        $satelliteLanguages = if ([string]::IsNullOrWhiteSpace((Get-SelectedSatelliteLanguageCode))) {
            ''
        }
        else {
            (Get-SelectedSatelliteLanguageCode).Trim()
        }
        Set-SelectedSatelliteLanguage -LanguageCode $satelliteLanguages
        $satelliteLanguagesArg = if ([string]::IsNullOrWhiteSpace($satelliteLanguages)) {
            ''
        }
        else {
            " -p:SatelliteResourceLanguages=`"$satelliteLanguages`""
        }

        $script:PublishStagingPath = New-PublishStagingDirectory
        $publishOutputPath = $script:PublishStagingPath
        $selectedPublishVerbosity = Get-SelectedPublishVerbosity
        $noBuildArg = if ($useExistingBuild) {
            ' --no-build'
        }
        else {
            ''
        }
        $publishCommand = "dotnet publish `"$projectPath`" $($opt.Args)$satelliteLanguagesArg$noBuildArg -v $selectedPublishVerbosity -o `"$publishOutputPath`""

        Update-PublishStatus "Preparing local staged publish output..."

        $logDirectory = Get-PublishLogDirectory
        New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
        $script:PublishLogFile = Join-Path $logDirectory ("publish-{0}.log" -f (Get-Date -Format 'yyyyMMdd-HHmmss'))
        $script:LastStatusMessage = ''
        $script:PublishLogBuffer = [System.Text.StringBuilder]::new()

        # Reset UI
        $successBorder.Visibility = [System.Windows.Visibility]::Collapsed
        $errorBorder.Visibility = [System.Windows.Visibility]::Collapsed
        $outputBorder.Visibility = [System.Windows.Visibility]::Visible
        $outputText.Text = @"
MTM Receiving Application Publish Tool
Started: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Option: $($opt.Label)
Project: $projectPath
Output: $($script:currentOutputPath)
PublishMode: Local staging first
PublishCommandOutput: $publishOutputPath
Verbosity: $selectedPublishVerbosity
SatelliteResourceLanguages: $(if ([string]::IsNullOrWhiteSpace($satelliteLanguages)) { 'all' } else { $satelliteLanguages })
ReuseExistingBuildOutput: $(if ($useExistingBuild) { 'enabled' } else { 'disabled' })
ExistingBuildPath: $(if ($useExistingBuild) { $script:ExistingBuildAvailability.BuildOutputPath } else { 'n/a' })
Command: $publishCommand
LogFile: $($script:PublishLogFile)

"@
        Add-PublishLogText $outputText.Text
    Set-PublishProgressState -Visible $true -IsIndeterminate $true
    Set-OperationUiState -IsBusy $true
        Update-PublishStatus "Publishing - please wait..."

        # Redirect stdout+stderr to a temp file via cmd /c.
        # This avoids DataReceived event callbacks crossing into the PowerShell runspace
        # from a thread-pool thread, which causes the CLR crash (0xE0434352).
        # The DispatcherTimer reads and streams from the file entirely on the UI thread.
        $script:outFile = [System.IO.Path]::GetTempFileName()
        $script:lastPos = 0L

        $cmdArgs = "/c $publishCommand > `"$($script:outFile)`" 2>&1"

        $psi = New-Object System.Diagnostics.ProcessStartInfo
        $psi.FileName = "cmd.exe"
        $psi.Arguments = $cmdArgs
        $psi.UseShellExecute = $false
        $psi.CreateNoWindow = $true

        $script:publishProcess = New-Object System.Diagnostics.Process
        $script:publishProcess.StartInfo = $psi
        $script:publishProcess.Start() | Out-Null

        # Poll the temp file for new lines every 200 ms - all on the UI thread, no callbacks
        $script:pollTimer = New-Object System.Windows.Threading.DispatcherTimer
        $script:pollTimer.Interval = [TimeSpan]::FromMilliseconds(200)
        $script:pollTimer.Add_Tick({
                # Read any new bytes written to the temp file since last tick
                try {
                    $fs = [System.IO.File]::Open($script:outFile,
                        [System.IO.FileMode]::Open,
                        [System.IO.FileAccess]::Read,
                        [System.IO.FileShare]::ReadWrite)
                    $fs.Seek($script:lastPos, [System.IO.SeekOrigin]::Begin) | Out-Null
                    $reader = New-Object System.IO.StreamReader($fs, [System.Text.Encoding]::UTF8)
                    $newText = $reader.ReadToEnd()
                    $script:lastPos = $fs.Position
                    $reader.Dispose()
                    $fs.Dispose()

                    if ($newText.Length -gt 0) {
                        $outputText.Text += $newText
                        Add-PublishLogText $newText
                        # Show last non-empty line in the status bar while building
                        $lastLine = ($newText -split "`n" | Where-Object { $_.Trim() -ne '' } | Select-Object -Last 1)
                        if ($lastLine) { Update-PublishStatus $lastLine.Trim() }
                        $outputScrollViewer.ScrollToEnd()
                    }
                }
                catch { <# file briefly locked - skip this tick #> }

                if (-not $script:publishProcess.HasExited) { return }

                if ($script:PublishCompletionHandled) {
                    return
                }

                $script:PublishCompletionHandled = $true

                # Process exited - do one final read to capture the last bytes
                $script:pollTimer.Stop()
                try {
                    $fs = [System.IO.File]::Open($script:outFile,
                        [System.IO.FileMode]::Open,
                        [System.IO.FileAccess]::Read,
                        [System.IO.FileShare]::ReadWrite)
                    $fs.Seek($script:lastPos, [System.IO.SeekOrigin]::Begin) | Out-Null
                    $reader = New-Object System.IO.StreamReader($fs, [System.Text.Encoding]::UTF8)
                    $tail = $reader.ReadToEnd()
                    $reader.Dispose()
                    $fs.Dispose()
                    if ($tail.Length -gt 0) {
                        $outputText.Text += $tail
                        Add-PublishLogText $tail
                    }
                }
                catch { }

                try {
                    Save-PublishLog
                }
                catch {
                    $errorBorder.Visibility = [System.Windows.Visibility]::Visible
                    $errorText.Text = "Publish completed, but the log file could not be written to $($script:PublishLogFile). $($_.Exception.Message)"
                }

                $publishExitCode = $script:publishProcess.ExitCode
                $stagingPath = $script:PublishStagingPath
                Stop-PublishSession
                $script:IsPublishInProgress = $false

                $outputScrollViewer.ScrollToEnd()

                $mergeSummaryText = ''
                if ($publishExitCode -eq 0 -and -not [string]::IsNullOrWhiteSpace($stagingPath)) {
                    if (Get-AutoMigrateToSharedDrive) {
                        try {
                            Start-PublishOutputSync -StagingPath $stagingPath -DestinationPath $script:currentOutputPath -Option $opt -PublishExitCode $publishExitCode
                            return
                        }
                        catch {
                            $script:PublishStagingPath = $null
                            $script:IsSyncInProgress = $false
                            Set-PublishProgressState -Visible $false
                            Set-OperationUiState -IsBusy $false
                            $successBorder.Visibility = [System.Windows.Visibility]::Collapsed
                            $errorBorder.Visibility = [System.Windows.Visibility]::Visible
                            $errorText.Text = "Publish succeeded, but migration to the shared drive could not be started. Staged output: $stagingPath`nLog file: $script:PublishLogFile`n$($_.Exception.Message)"
                            Update-PublishStatus "Publish migration failed to start - check build output."
                            Add-PublishLogText "`r`nSync Failed To Start: $($_.Exception.Message)`r`n"
                            Add-PublishLogText "`r`nCompleted: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`r`nExitCode: $publishExitCode`r`n"
                            try {
                                Save-PublishLog
                            }
                            catch {
                            }
                            return
                        }
                    }

                    $mergeSummaryText = "`nStaged output retained locally: $stagingPath`nAutomatic migration to the shared drive was disabled for this publish."
                    Add-PublishLogText "`r`nSync Deferred`r`nStaged output retained locally: $stagingPath`r`n"
                    Update-PublishStatus "Publish completed successfully. Automatic migration was disabled, so the staged output was retained locally."
                    $script:PublishStagingPath = $null
                }

                Set-PublishProgressState -Visible $false
                Set-OperationUiState -IsBusy $false

                if ($publishExitCode -eq 0) {
                    $errorBorder.Visibility = [System.Windows.Visibility]::Collapsed
                    $successBorder.Visibility = [System.Windows.Visibility]::Visible
                    $successText.Text = "Publish succeeded!`nOutput folder: $script:currentOutputPath`nLog file: $script:PublishLogFile$mergeSummaryText"
                    Update-PublishStatus "Publish completed successfully!"
                }
                else {
                    $successBorder.Visibility = [System.Windows.Visibility]::Collapsed
                    $errorBorder.Visibility = [System.Windows.Visibility]::Visible
                    $errorText.Text = "Publish failed (exit code $publishExitCode). See the build output above for details.`nLog file: $script:PublishLogFile"
                    Update-PublishStatus "Publish failed - check build output."
                }

                Add-PublishLogText "`r`nCompleted: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`r`nExitCode: $publishExitCode`r`n"
                try {
                    Save-PublishLog
                }
                catch {
                }
            })
        $script:pollTimer.Start()
    })

# ---------------------------------------------------------------------------
# Close button
# ---------------------------------------------------------------------------
$closeButton.Add_Click({
        if ($script:IsPublishInProgress -or $script:IsSyncInProgress) {
            Update-PublishStatus 'Wait for the current publish or migration operation to finish before closing the tool.'
            return
        }

        Stop-PublishSession
        Reset-SyncWorker
        Remove-PublishStagingDirectory
        $window.Close()
    })

$window.Add_Closing({
        param($closingSender, $closingEventArgs)

        if ($script:IsPublishInProgress -or $script:IsSyncInProgress) {
            $closingEventArgs.Cancel = $true
            Update-PublishStatus 'Wait for the current publish or migration operation to finish before closing the tool.'
            return
        }

        Stop-PublishSession
        Reset-SyncWorker
        Remove-PublishStagingDirectory
    })

# ---------------------------------------------------------------------------
# Show window
# ---------------------------------------------------------------------------
$window.ShowDialog() | Out-Null
