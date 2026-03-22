# MTM Receiving Application — Publish Tool (WPF GUI)
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

# ---------------------------------------------------------------------------
# Notes strings (defined separately to avoid heredoc nesting issues)
# ---------------------------------------------------------------------------
$n1 = "RECOMMENDED for server-share deployment.`n`nBundles the .NET 10 runtime and Windows App SDK alongside the app. No software needs to be installed on any user PC. Users launch straight from the network share using their desktop shortcut.`n`n  WHEN TO CHOOSE THIS:`n  - Zero prerequisites on user PCs`n  - Most reliable launch from a network share`n  - Everything needed is in the output folder`n  - Safe for all WinUI 3 features`n`n  WHEN TO AVOID:`n  - Disk space on the share is severely constrained`n  - IT centrally manages runtimes via Intune/SCCM"

$n2 = "NOT RECOMMENDED unless IT manages runtimes centrally.`n`nPublishes only the app code — no runtime bundled. The .NET 10 Desktop Runtime and Windows App SDK must already be installed on every PC that runs the app.`n`n  WHY YOU MIGHT STILL CHOOSE THIS:`n  - IT manages workstations via Intune/SCCM — runtimes are always present`n  - Publish output is significantly smaller, so share updates are faster`n  - .NET security patches handled by IT — no full app republish needed`n  - Disk space on the share is tightly constrained`n`n  DO NOT USE IF:`n  - Any user PC may not have the runtimes installed`n  - You cannot confirm deployment status with IT"

$n4 = "GOOD CHOICE for server-share — faster startup over a network.`n`nPre-compiles assemblies to native code at publish time. Results in noticeably faster cold-start times, partially compensating for network file-read overhead on each launch.`n`n  WHY CHOOSE THIS:`n  - Best startup performance for a server-share deployment`n  - Reduces CPU work on the user PC at launch time`n  - No prerequisites or run-time caveats`n  - Safe choice for all WinUI 3 features`n`n  NOTE:`n  - Output folder will be larger than standard self-contained`n  - For best results, build machine RID should match target RID"

$n5 = "HIGH RISK with WinUI 3 — test thoroughly before deploying.`n`nRemoves unused assemblies and types to reduce output folder size. WinUI 3 relies heavily on reflection and dynamic type loading, which conflicts with aggressive trimming.`n`n  WHY YOU MIGHT STILL CHOOSE THIS:`n  - Server share is on a slow/VPN link and folder size affects launch time`n  - Disk space on the share is severely constrained`n  - All trim warnings resolved and a full end-to-end test pass is done`n  - Smallest possible artifact for automated deployment pipelines`n`n  RISKS:`n  - Runtime failures from missing types are common with WinUI 3`n  - ALWAYS test every screen before deploying to users`n  - Do not use as the primary production build until fully validated"

$n5arm64 = "FOR ARM64 MACHINES — publish to a separate folder with a separate shortcut.`n`nSame as Self-Contained but compiled for ARM64 machines (e.g., Surface Pro X, Snapdragon-based PCs). The .csproj already declares win-arm64 as a supported RID.`n`n  WHY CHOOSE THIS:`n  - Required for native ARM64 performance — x64 emulation uses more CPU/battery`n  - Separate output folder keeps x64 and ARM64 deployments fully independent`n  - Both folders can coexist on the same server share`n`n  HOW TO DEPLOY:`n  - Output goes to: MTM_Receiving_Application_ARM64`n  - Create a separate desktop shortcut for ARM64 users pointing to this folder`n  - Do NOT overwrite the standard x64 share folder"

# ---------------------------------------------------------------------------
# Publish option definitions — mirrors PublishAppScript.md
# ---------------------------------------------------------------------------
$script:BaseShare   = "X:\Software Development\Live Applications"
$script:ProjectFile = "c:\Users\jkoll\source\repos\MTM_Receiving_Application\MTM_Receiving_Application.csproj"

$script:Options = @(
    @{
        Index    = 0
        Label    = "1  Self-Contained  (Recommended)"
        Folder   = "MTM_Receiving_Application"
        Args     = "-c Release -r win-x64 --self-contained true"
        TagColor = "#388E3C"
        Notes    = $n1
    },
    @{
        Index    = 1
        Label    = "2  Framework-Dependent  ⚠"
        Folder   = "MTM_Receiving_Application_FD"
        Args     = "-c Release -r win-x64 --self-contained false"
        TagColor = "#E65100"
        Notes    = $n2
    },
    @{
        Index    = 2
        Label    = "3  ReadyToRun — Faster Startup"
        Folder   = "MTM_Receiving_Application_R2R"
        Args     = "-c Release -r win-x64 --self-contained true -p:PublishReadyToRun=true"
        TagColor = "#388E3C"
        Notes    = $n4
    },
    @{
        Index    = 3
        Label    = "4  Trimmed  ⚠  (High Risk)"
        Folder   = "MTM_Receiving_Application_Trimmed"
        Args     = "-c Release -r win-x64 --self-contained true -p:PublishTrimmed=true"
        TagColor = "#C62828"
        Notes    = $n5
    },
    @{
        Index    = 4
        Label    = "5  ARM64 Variant"
        Folder   = "MTM_Receiving_Application_ARM64"
        Args     = "-c Release -r win-arm64 --self-contained true"
        TagColor = "#388E3C"
        Notes    = $n5arm64
    }
)

# ---------------------------------------------------------------------------
# XAML
# ---------------------------------------------------------------------------
$xaml = @"
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="MTM Receiving Application — Publish Tool"
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
            <TextBlock Text="Publish Tool — select an option on the left, review the notes, then click Publish."
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
                        <TextBlock Name="OutputPathText"
                                   TextWrapping="Wrap" FontFamily="Consolas" FontSize="11"
                                   Foreground="#1565C0" Text="(select an option)"/>
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
                <TextBlock Grid.Column="0" Text="Project:" FontWeight="Bold" VerticalAlignment="Center"/>
                <TextBox Grid.Column="1" Name="ProjectPathText"
                         FontFamily="Consolas" FontSize="11" Padding="5,4"
                         BorderBrush="#CCC" BorderThickness="1" VerticalContentAlignment="Center"/>
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
                                  VerticalScrollBarVisibility="Auto"
                                  HorizontalScrollBarVisibility="Auto">
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
$reader = [System.Xml.XmlNodeReader]::new([xml]$xaml)
$window = [Windows.Markup.XamlReader]::Load($reader)

# ---------------------------------------------------------------------------
# Bind controls
# ---------------------------------------------------------------------------
$optionList        = $window.FindName("OptionList")
$notesText         = $window.FindName("NotesText")
$outputPathText    = $window.FindName("OutputPathText")
$projectPathText   = $window.FindName("ProjectPathText")
$statusText        = $window.FindName("StatusText")
$publishProgress   = $window.FindName("PublishProgress")
$outputBorder      = $window.FindName("OutputBorder")
$outputScrollViewer = $window.FindName("OutputScrollViewer")
$outputText        = $window.FindName("OutputText")
$successBorder     = $window.FindName("SuccessBorder")
$successText       = $window.FindName("SuccessText")
$errorBorder       = $window.FindName("ErrorBorder")
$errorText         = $window.FindName("ErrorText")
$publishButton     = $window.FindName("PublishButton")
$closeButton       = $window.FindName("CloseButton")

$projectPathText.Text = $script:ProjectFile

# ---------------------------------------------------------------------------
# Populate option ListBox
# ---------------------------------------------------------------------------
$brushConverter = New-Object System.Windows.Media.BrushConverter

foreach ($opt in $script:Options) {
    $item = New-Object System.Windows.Controls.ListBoxItem
    $item.Padding = "10,7"
    $item.Tag = $opt.Index

    $sp = New-Object System.Windows.Controls.StackPanel

    $labelBlock = New-Object System.Windows.Controls.TextBlock
    $labelBlock.Text        = $opt.Label
    $labelBlock.FontWeight  = [System.Windows.FontWeights]::SemiBold
    $labelBlock.TextWrapping = [System.Windows.TextWrapping]::Wrap
    $labelBlock.Foreground  = $brushConverter.ConvertFromString($opt.TagColor)

    $folderBlock = New-Object System.Windows.Controls.TextBlock
    $folderBlock.Text       = $opt.Folder
    $folderBlock.FontSize   = 10
    $folderBlock.FontFamily = New-Object System.Windows.Media.FontFamily("Consolas")
    $folderBlock.Foreground = $brushConverter.ConvertFromString("#888888")
    $folderBlock.Margin     = New-Object System.Windows.Thickness(0, 2, 0, 0)

    [void]$sp.Children.Add($labelBlock)
    [void]$sp.Children.Add($folderBlock)
    $item.Content = $sp
    [void]$optionList.Items.Add($item)
}

# ---------------------------------------------------------------------------
# Selection changed — update notes panel
# ---------------------------------------------------------------------------
$script:selectedOption = $null

$optionList.Add_SelectionChanged({
    $selectedItem = $optionList.SelectedItem
    if ($null -eq $selectedItem) { return }

    # Store directly so the publish handler never needs to re-derive it
    $script:selectedOption = $script:Options[[int]$selectedItem.Tag]

    $notesText.Text      = $script:selectedOption.Notes
    $outputPathText.Text = "$($script:BaseShare)\$($script:selectedOption.Folder)"

    $successBorder.Visibility = [System.Windows.Visibility]::Collapsed
    $errorBorder.Visibility   = [System.Windows.Visibility]::Collapsed
    $outputBorder.Visibility  = [System.Windows.Visibility]::Collapsed
    $outputText.Text          = ""
    $statusText.Text          = "Ready to publish: $($script:selectedOption.Label)"
    $publishButton.IsEnabled  = $true
})

# ---------------------------------------------------------------------------
# Publish button — run dotnet publish, show live output, show result
# ---------------------------------------------------------------------------
$publishButton.Add_Click({
    if ($null -eq $script:selectedOption) { return }

    $opt        = $script:selectedOption
    $script:currentOutputPath = "$($script:BaseShare)\$($opt.Folder)"
    $projectPath = $projectPathText.Text

    # Reset UI
    $successBorder.Visibility   = [System.Windows.Visibility]::Collapsed
    $errorBorder.Visibility     = [System.Windows.Visibility]::Collapsed
    $outputBorder.Visibility    = [System.Windows.Visibility]::Visible
    $outputText.Text            = ""
    $publishProgress.Visibility = [System.Windows.Visibility]::Visible
    $publishButton.IsEnabled    = $false
    $statusText.Text            = "Publishing — please wait..."

    # Redirect stdout+stderr to a temp file via cmd /c.
    # This avoids DataReceived event callbacks crossing into the PowerShell runspace
    # from a thread-pool thread, which causes the CLR crash (0xE0434352).
    # The DispatcherTimer reads and streams from the file entirely on the UI thread.
    $script:outFile  = [System.IO.Path]::GetTempFileName()
    $script:lastPos  = 0L

    $cmdArgs = "/c dotnet publish `"$projectPath`" $($opt.Args) -o `"$($script:currentOutputPath)`" > `"$($script:outFile)`" 2>&1"

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName        = "cmd.exe"
    $psi.Arguments       = $cmdArgs
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow  = $true

    $script:publishProcess = New-Object System.Diagnostics.Process
    $script:publishProcess.StartInfo = $psi
    $script:publishProcess.Start() | Out-Null

    # Poll the temp file for new lines every 200 ms — all on the UI thread, no callbacks
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
                # Show last non-empty line in the status bar while building
                $lastLine = ($newText -split "`n" | Where-Object { $_.Trim() -ne '' } | Select-Object -Last 1)
                if ($lastLine) { $statusText.Text = $lastLine.Trim() }
                $outputScrollViewer.ScrollToEnd()
            }
        }
        catch { <# file briefly locked — skip this tick #> }

        if (-not $script:publishProcess.HasExited) { return }

        # Process exited — do one final read to capture the last bytes
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
            if ($tail.Length -gt 0) { $outputText.Text += $tail }
        }
        catch { }

        try { Remove-Item $script:outFile -Force -ErrorAction SilentlyContinue } catch { }

        $outputScrollViewer.ScrollToEnd()
        $publishProgress.Visibility = [System.Windows.Visibility]::Collapsed
        $publishButton.IsEnabled    = $true

        if ($script:publishProcess.ExitCode -eq 0) {
            $successBorder.Visibility = [System.Windows.Visibility]::Visible
            $successText.Text         = "Publish succeeded!`nOutput folder: $script:currentOutputPath"
            $statusText.Text          = "Publish completed successfully!"
        }
        else {
            $errorBorder.Visibility = [System.Windows.Visibility]::Visible
            $errorText.Text         = "Publish failed (exit code $($script:publishProcess.ExitCode)). See the build output above for details."
            $statusText.Text        = "Publish failed — check build output."
        }
    })
    $script:pollTimer.Start()
})

# ---------------------------------------------------------------------------
# Close button
# ---------------------------------------------------------------------------
$closeButton.Add_Click({ $window.Close() })

# ---------------------------------------------------------------------------
# Show window
# ---------------------------------------------------------------------------
$window.ShowDialog() | Out-Null
