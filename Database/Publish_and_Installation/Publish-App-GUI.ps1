# MTM Receiving Application - Publish Tool (WPF GUI)
# Select a publish option; notes are shown alongside; click Publish to run dotnet publish.

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

    return Join-Path $script:OutputRootPath $Option.Folder
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
            $script:OutputRootPath = $dialog.SelectedPath
            Update-OutputPathDisplay
            Update-PublishStatus "Publish output folder updated."
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
        Height="740" Width="940"
        MinHeight="640" MinWidth="800"
        WindowStartupLocation="CenterScreen"
        ResizeMode="CanResizeWithGrip"
        Background="#F5F5F5">
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="310"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Margin="0,0,0,14">
            <TextBlock Text="MTM Receiving Application"
                       FontSize="22" FontWeight="Bold" Foreground="#2196F3"/>
            <TextBlock Text="Publish Tool - select an option on the left, review the notes, then click Publish."
                       FontSize="13" Foreground="#666"/>
        </StackPanel>

        <!-- Options list + Notes side-by-side -->
        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="295"/>
                <ColumnDefinition Width="12"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>

            <!-- Left: option list -->
            <Border Grid.Column="0" Background="White" BorderBrush="#DDD"
                    BorderThickness="1" CornerRadius="5">
                <DockPanel>
                    <TextBlock DockPanel.Dock="Top" Text="Publish Options"
                               FontWeight="Bold" Foreground="#444" Margin="12,10,12,6"/>
                    <ListBox Name="OptionList" BorderThickness="0" Background="Transparent"
                             ScrollViewer.HorizontalScrollBarVisibility="Disabled"
                             VirtualizingPanel.IsVirtualizing="False"/>
                </DockPanel>
            </Border>

            <!-- Right: notes + output path -->
            <Border Grid.Column="2" Background="White" BorderBrush="#DDD"
                    BorderThickness="1" CornerRadius="5" Padding="15">
                <DockPanel>
                    <TextBlock DockPanel.Dock="Top" Text="Option Notes"
                               FontWeight="Bold" Foreground="#444" Margin="0,0,0,8"/>
                    <StackPanel DockPanel.Dock="Bottom" Margin="0,10,0,0">
                        <Separator Margin="0,0,0,10"/>
                        <TextBlock Text="Output Path" FontWeight="Bold" Foreground="#444" Margin="0,0,0,4"/>
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <TextBox Grid.Column="0"
                                     Name="OutputPathText"
                                     IsReadOnly="True"
                                     FontFamily="Consolas"
                                     FontSize="11"
                                     Padding="5,4"
                                     BorderBrush="#CCC"
                                     BorderThickness="1"
                                     VerticalContentAlignment="Center"
                                     Text="(select an option)"/>
                            <Button Grid.Column="1"
                                    Name="BrowseOutputPathButton"
                                    Content="Browse..."
                                    Margin="8,0,0,0"
                                    MinWidth="90"/>
                        </Grid>
                    </StackPanel>
                    <ScrollViewer VerticalScrollBarVisibility="Auto">
                        <TextBlock Name="NotesText" TextWrapping="Wrap"
                                   Foreground="#333" LineHeight="19"
                                   Text="Select a publish option on the left to see notes here."/>
                    </ScrollViewer>
                </DockPanel>
            </Border>
        </Grid>

        <!-- Project path -->
        <Border Grid.Row="2" Background="White" BorderBrush="#DDD"
                BorderThickness="1" CornerRadius="5" Padding="12" Margin="0,10,0,0">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="62"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>
                <TextBlock Grid.Row="0" Grid.Column="0" Text="Project:" FontWeight="Bold" VerticalAlignment="Center"/>
                <TextBox Grid.Row="0" Grid.Column="1" Name="ProjectPathText"
                         FontFamily="Consolas" FontSize="11" Padding="5,4"
                         BorderBrush="#CCC" BorderThickness="1" VerticalContentAlignment="Center"/>
                <TextBlock Grid.Row="1" Grid.Column="0" Text="Lang:" FontWeight="Bold" VerticalAlignment="Center" Margin="0,10,0,0"/>
                <ComboBox Grid.Row="1" Grid.Column="1" Name="SatelliteLanguagesComboBox"
                          Margin="0,10,0,0"
                          FontSize="11"
                          Padding="5,4"
                          BorderBrush="#CCC"
                          BorderThickness="1"
                          IsEditable="False"
                          IsTextSearchEnabled="True"
                          MaxDropDownHeight="320"
                          ToolTip="Optional. Pick one language to limit satellite resources, or leave the default option selected to publish all available languages."/>

                <TextBlock Grid.Row="2" Grid.Column="0" Text="Extras:" FontWeight="Bold" VerticalAlignment="Center" Margin="0,10,0,0"/>
                <StackPanel Grid.Row="2" Grid.Column="1" Orientation="Horizontal" Margin="0,10,0,0">
                    <CheckBox Name="DetailedLoggingCheckBox"
                              Content="Detailed logging"
                              ToolTip="Enable verbose publish output only when troubleshooting."/>
                    <CheckBox Name="ShowExperimentalOptionsCheckBox"
                              Content="Show experimental options"
                              Margin="18,0,0,0"
                              ToolTip="Displays advanced publish options such as ReadyToRun that are not part of the normal supported path."/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- Status / build output -->
        <Border Grid.Row="3" Background="White" BorderBrush="#DDD"
                BorderThickness="1" CornerRadius="5" Padding="12" Margin="0,8,0,0">
            <StackPanel>
                <Grid Margin="0,0,0,6">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="Auto"/>
                    </Grid.ColumnDefinitions>
                    <TextBlock Grid.Column="0" Name="StatusText"
                               Text="Ready. Select an option above and click Publish."
                               FontWeight="Bold" FontSize="13" Foreground="#555"
                               VerticalAlignment="Center"/>
                    <ProgressBar Grid.Column="1" Name="PublishProgress"
                                 Width="130" Height="8"
                                 IsIndeterminate="True" Visibility="Collapsed"/>
                </Grid>

                <!-- Build output (dark terminal style) -->
                <Border Name="OutputBorder" Background="#1E1E1E" CornerRadius="3"
                        Margin="0,4,0,0" Visibility="Collapsed">
                    <ScrollViewer Name="OutputScrollViewer" Height="130"
                                  HorizontalScrollBarVisibility="Disabled"
                                  VerticalScrollBarVisibility="Auto">
                        <TextBlock Name="OutputText" FontFamily="Consolas" FontSize="10"
                                   Foreground="#D4D4D4" TextWrapping="Wrap" Margin="8"/>
                    </ScrollViewer>
                </Border>

                <!-- Success -->
                <Border Name="SuccessBorder" Background="#E8F5E9" BorderBrush="#4CAF50"
                        BorderThickness="1" CornerRadius="3" Padding="12"
                        Margin="0,6,0,0" Visibility="Collapsed">
                    <TextBlock Name="SuccessText" Foreground="#2E7D32"
                               FontWeight="Bold" TextWrapping="Wrap"/>
                </Border>

                <!-- Error -->
                <Border Name="ErrorBorder" Background="#FFEBEE" BorderBrush="#F44336"
                        BorderThickness="1" CornerRadius="3" Padding="12"
                        Margin="0,6,0,0" Visibility="Collapsed">
                    <TextBlock Name="ErrorText" Foreground="#C62828" TextWrapping="Wrap"/>
                </Border>
            </StackPanel>
        </Border>

        <!-- Buttons -->
        <StackPanel Grid.Row="4" Orientation="Horizontal"
                    HorizontalAlignment="Right" Margin="0,12,0,0">
            <Button Name="PublishButton" Content="Publish"
                    Width="130" Height="36"
                    Background="#2196F3" Foreground="White"
                    BorderThickness="0" FontWeight="Bold" FontSize="14"
                    Cursor="Hand" IsEnabled="False"/>
            <Button Name="CloseButton" Content="Close"
                    Width="100" Height="36" Margin="10,0,0,0"
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
$satelliteLanguagesComboBox.Items.Clear()
foreach ($languageItem in (Get-SatelliteLanguageOptions)) {
    [void]$satelliteLanguagesComboBox.Items.Add($languageItem)
}
Set-SelectedSatelliteLanguage -LanguageCode $script:SatelliteResourceLanguages
Update-OutputPathDisplay

if (-not (Test-Path -LiteralPath $script:ProjectFile)) {
    Update-PublishStatus "Project file not found. Review the Project path before publishing."
}

# ---------------------------------------------------------------------------
# Populate option ListBox
# ---------------------------------------------------------------------------
$brushConverter = New-Object System.Windows.Media.BrushConverter
Update-PublishOptionList

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
    })

$browseOutputPathButton.Add_Click({
        Select-OutputRootFolder
    })

$showExperimentalOptionsCheckBox.Add_Click({
        Update-PublishOptionList
    })

# ---------------------------------------------------------------------------
# Publish button - run dotnet publish, show live output, show result
# ---------------------------------------------------------------------------
$publishButton.Add_Click({
        if ($null -eq $script:selectedOption) { return }

        Remove-PublishStagingDirectory

        $opt = $script:selectedOption
        $script:currentOutputPath = Get-EffectiveOutputPath -Option $opt
        $projectPath = $projectPathText.Text

        if ([string]::IsNullOrWhiteSpace($projectPath) -or -not (Test-Path -LiteralPath $projectPath)) {
            $errorBorder.Visibility = [System.Windows.Visibility]::Visible
            $errorText.Text = "The selected project file does not exist. Update the Project path to the current MTM_Receiving_Application.csproj before publishing."
            $successBorder.Visibility = [System.Windows.Visibility]::Collapsed
            $outputBorder.Visibility = [System.Windows.Visibility]::Collapsed
            Update-PublishStatus "Publish blocked - project file path is invalid."
            return
        }

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
        $publishCommand = "dotnet publish `"$projectPath`" $($opt.Args)$satelliteLanguagesArg -v $selectedPublishVerbosity -o `"$publishOutputPath`""

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
Command: $publishCommand
LogFile: $($script:PublishLogFile)

"@
        Add-PublishLogText $outputText.Text
    Set-PublishProgressState -Visible $true -IsIndeterminate $true
        $publishButton.IsEnabled = $false
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
                Stop-PublishSession

                $outputScrollViewer.ScrollToEnd()

                $stagingPath = $script:PublishStagingPath
                Stop-PublishSession

                $mergeSummaryText = ''
                $syncCompleted = $false
                if ($publishExitCode -eq 0 -and -not [string]::IsNullOrWhiteSpace($stagingPath)) {
                    if (Confirm-SyncStagedPublishOutput -DestinationPath $script:currentOutputPath -StagingPath $stagingPath -OptionLabel $opt.Label) {
                        try {
                            Add-PublishLogText "`r`nSync Started`r`nDeployment target: $($script:currentOutputPath)`r`n"
                            $script:LastSyncProgressRender = [datetime]::MinValue
                            $mergeResult = Sync-PublishOutputDirectory -SourcePath $stagingPath -DestinationPath $script:currentOutputPath -PreserveNames @('_PublishLogs') -ProgressAction {
                                param($completedSteps, $totalSteps, $phase)

                                Update-SyncProgress -CompletedSteps $completedSteps -TotalSteps $totalSteps -Phase $phase
                            }
                            $mergeSummaryText = "`nSync completed to: $($script:currentOutputPath)`nReused unchanged files: $($mergeResult.SkippedFiles)`nCopied new or changed files: $($mergeResult.CopiedFiles)`nRemoved stale files/folders: $($mergeResult.RemovedItems)"
                            Add-PublishLogText "`r`nSync Summary`r`nReused unchanged files: $($mergeResult.SkippedFiles)`r`nCopied new or changed files: $($mergeResult.CopiedFiles)`r`nRemoved stale files/folders: $($mergeResult.RemovedItems)`r`n"
                            Update-PublishStatus "Publish and sync completed successfully."
                            $syncCompleted = $true
                            Remove-Item -LiteralPath $stagingPath -Recurse -Force -ErrorAction SilentlyContinue
                            $script:PublishStagingPath = $null
                        }
                        catch {
                            $script:PublishStagingPath = $null
                            Set-PublishProgressState -Visible $false
                            $publishButton.IsEnabled = $true
                            $errorBorder.Visibility = [System.Windows.Visibility]::Visible
                            $errorText.Text = "Publish succeeded, but syncing the staged output failed. Staged output: $stagingPath`nLog file: $script:PublishLogFile`n$($_.Exception.Message)"
                            Update-PublishStatus "Publish sync failed - check build output."
                            Add-PublishLogText "`r`nSync Failed: $($_.Exception.Message)`r`n"
                            return
                        }
                    }
                    else {
                        $mergeSummaryText = "`nStaged output retained locally: $stagingPath`nDeployment target not updated yet."
                        Add-PublishLogText "`r`nSync Deferred`r`nStaged output retained locally: $stagingPath`r`n"
                        Update-PublishStatus "Publish completed successfully. Sync to the deployment target was deferred."
                        $script:PublishStagingPath = $null
                    }
                }

                if (-not $syncCompleted) {
                    Set-PublishProgressState -Visible $false
                }

                $publishButton.IsEnabled = $true

                if ($publishExitCode -eq 0) {
                    $successBorder.Visibility = [System.Windows.Visibility]::Visible
                    $successText.Text = "Publish succeeded!`nOutput folder: $script:currentOutputPath`nLog file: $script:PublishLogFile$mergeSummaryText"
                    Update-PublishStatus "Publish completed successfully!"
                }
                else {
                    $errorBorder.Visibility = [System.Windows.Visibility]::Visible
                    $errorText.Text = "Publish failed (exit code $publishExitCode). See the build output above for details.`nLog file: $script:PublishLogFile"
                    Update-PublishStatus "Publish failed - check build output."
                }

                Add-PublishLogText "`r`nCompleted: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`r`nExitCode: $publishExitCode`r`n"
            })
        $script:pollTimer.Start()
    })

# ---------------------------------------------------------------------------
# Close button
# ---------------------------------------------------------------------------
$closeButton.Add_Click({
        Stop-PublishSession
        Remove-PublishStagingDirectory
        $window.Close()
    })

$window.Add_Closing({
        Stop-PublishSession
        Remove-PublishStagingDirectory
    })

# ---------------------------------------------------------------------------
# Show window
# ---------------------------------------------------------------------------
$window.ShowDialog() | Out-Null
