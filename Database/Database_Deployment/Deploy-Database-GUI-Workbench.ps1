# MTM Receiving Application — Database Deployment (MySQL Workbench / MySQL Server)
# Supports both the standard MySQL Server installation and MAMP-managed MySQL.
# MAMP's MySQL 5.7 client does not reliably process DELIMITER directives in batch
# mode, so delimiter-based scripts are normalized before execution in that mode.

param(
    [string]$Server = "localhost",
    [string]$Port = "3306",
    [string]$Database = "mtm_receiving_application",
    [string]$User = "root",
    [string]$Password = "root"
)

$script:Config = [ordered]@{
    Window    = [ordered]@{
        Height    = 900
        Width     = 1100
        MinHeight = 860
        MinWidth  = 1040
    }
    Paths     = [ordered]@{
        SqlObjectCatalogTools = 'SqlObjectCatalogTools.ps1'
        DatabaseRootFolder    = 'Sql_Files'
        CatalogFileName       = 'sql-object-catalog.json'
        OutputsFolder         = 'outputs'
        WorkbenchInstallRoot  = 'C:\Program Files\MySQL'
        WorkbenchProgramData  = 'C:\ProgramData\MySQL'
        DefaultWorkbenchMyIni = 'C:\ProgramData\MySQL\MySQL Server 9.6\my.ini'
        MampRoot              = 'C:\MAMP'
        MampConfigPath        = 'C:\MAMP\conf\mysql\my.ini'
        MampExecutablePaths   = @(
            'C:\MAMP\MAMP.exe',
            'C:\MAMP\LauncherMAMP.exe'
        )
        MampMysqlAdminPaths   = @(
            'C:\MAMP\bin\mysql\bin\mysqladmin.exe',
            'C:\MAMP\bin\mysql8.0.31\bin\mysqladmin.exe',
            'C:\MAMP\bin\mysql5.7.39\bin\mysqladmin.exe',
            'C:\MAMP\bin\mysql5.7.24\bin\mysqladmin.exe'
        )
    }
    Providers = [ordered]@{
        Mamp = [ordered]@{
            Server   = 'localhost'
            Port     = '3306'
            User     = 'root'
            Password = 'root'
        }
    }
    Sql       = [ordered]@{
        ExcludePatterns = @('test_*.sql', '*_test.sql', '*.test.sql')
    }
}

Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName WindowsBase

. (Join-Path $PSScriptRoot $script:Config.Paths.SqlObjectCatalogTools)

$xaml = @"
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="MTM Database Deployment"
    Height="$($script:Config.Window.Height)" Width="$($script:Config.Window.Width)" MinHeight="$($script:Config.Window.MinHeight)" MinWidth="$($script:Config.Window.MinWidth)"
        WindowStartupLocation="CenterScreen"
    ResizeMode="CanResize"
        Background="#F5F5F5">
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <StackPanel Grid.Row="0" Margin="0,0,0,20">
            <TextBlock Text="MTM Receiving Application"
                       FontSize="24" FontWeight="Bold" Foreground="#2196F3"/>
            <TextBlock Text="Database Deployment Tool"
                       FontSize="16" Foreground="#666"/>
        </StackPanel>

        <Border Grid.Row="1" Background="White" BorderBrush="#DDD" BorderThickness="1"
                CornerRadius="5" Padding="15" Margin="0,0,0,20">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>
                <TextBlock Grid.Row="0" Grid.Column="0" Text="Server:"   FontWeight="Bold" Margin="0,5"/>
                <TextBlock Grid.Row="0" Grid.Column="1" Name="ServerText"   Margin="10,5" Foreground="#333"/>
                <TextBlock Grid.Row="1" Grid.Column="0" Text="Database:" FontWeight="Bold" Margin="0,5"/>
                <TextBlock Grid.Row="1" Grid.Column="1" Name="DatabaseText" Margin="10,5" Foreground="#333"/>
                <TextBlock Grid.Row="2" Grid.Column="0" Text="User:"     FontWeight="Bold" Margin="0,5"/>
                <TextBlock Grid.Row="2" Grid.Column="1" Name="UserText"     Margin="10,5" Foreground="#333"/>
                <TextBlock Grid.Row="3" Grid.Column="0" Text="Provider:" FontWeight="Bold" Margin="0,5"/>
                <StackPanel Grid.Row="3" Grid.Column="1" Orientation="Horizontal" Margin="10,5">
                    <Border Background="#E3F2FD" BorderBrush="#90CAF9" BorderThickness="1" CornerRadius="14" Padding="10,4">
                        <TextBlock Name="ProviderValueText" Text="MySQL Workbench" Foreground="#1565C0" FontWeight="SemiBold"/>
                    </Border>
                    <ToggleButton Name="ProviderToggleButton"
                                  Width="170"
                                  Height="30"
                                  Margin="12,0,0,0"
                                  Background="#1976D2"
                                  Foreground="White"
                                  BorderThickness="0"
                                  FontWeight="SemiBold"
                                  Cursor="Hand"
                                  Content="Switch to MAMP"/>
                </StackPanel>
            </Grid>
        </Border>

        <Border Grid.Row="2" Background="White" BorderBrush="#DDD" BorderThickness="1"
                CornerRadius="5" Padding="15">
            <Grid>
                <StackPanel Name="DeploymentPanel">
                    <TextBlock Name="OverallStatusText" Text="Ready to deploy..."
                               FontSize="14" FontWeight="Bold" Margin="0,0,0,10"/>

                    <StackPanel Margin="0,5">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <TextBlock Grid.Column="0" Text="[1/6] Database Schemas" Foreground="#666"/>
                            <TextBlock Grid.Column="1" Name="SchemaCount" Text="0/0" Foreground="#2196F3"/>
                        </Grid>
                        <ProgressBar Name="SchemaProgress" Height="8" Margin="0,5" Maximum="100"/>
                    </StackPanel>

                    <StackPanel Margin="0,5">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <TextBlock Grid.Column="0" Text="[2/6] Database Migrations" Foreground="#666"/>
                            <TextBlock Grid.Column="1" Name="MigrationCount" Text="0/0" Foreground="#2196F3"/>
                        </Grid>
                        <ProgressBar Name="MigrationProgress" Height="8" Margin="0,5" Maximum="100"/>
                    </StackPanel>

                    <StackPanel Margin="0,5">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <TextBlock Grid.Column="0" Text="[3/6] Database Views" Foreground="#666"/>
                            <TextBlock Grid.Column="1" Name="ViewCount" Text="0/0" Foreground="#2196F3"/>
                        </Grid>
                        <ProgressBar Name="ViewProgress" Height="8" Margin="0,5" Maximum="100"/>
                    </StackPanel>

                    <StackPanel Margin="0,5">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <TextBlock Grid.Column="0" Text="[4/6] Stored Procedures" Foreground="#666"/>
                            <TextBlock Grid.Column="1" Name="StoredProcCount" Text="0/0" Foreground="#2196F3"/>
                        </Grid>
                        <ProgressBar Name="StoredProcProgress" Height="8" Margin="0,5" Maximum="100"/>
                    </StackPanel>

                    <StackPanel Margin="0,5">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <TextBlock Grid.Column="0" Text="[5/6] Database Triggers" Foreground="#666"/>
                            <TextBlock Grid.Column="1" Name="TriggerCount" Text="0/0" Foreground="#2196F3"/>
                        </Grid>
                        <ProgressBar Name="TriggerProgress" Height="8" Margin="0,5" Maximum="100"/>
                    </StackPanel>

                    <StackPanel Margin="0,5">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <TextBlock Grid.Column="0" Text="[6/6] Seed Data" Foreground="#666"/>
                            <TextBlock Grid.Column="1" Name="SeedDataCount" Text="0/0" Foreground="#2196F3"/>
                        </Grid>
                        <ProgressBar Name="SeedDataProgress" Height="8" Margin="0,5" Maximum="100"/>
                    </StackPanel>

                    <TextBlock Name="CurrentFileText" Text="" Margin="0,15,0,0"
                               FontSize="11" Foreground="#999" TextTrimming="CharacterEllipsis"/>
                </StackPanel>

                <Grid Name="CatalogPanel" Visibility="Collapsed">
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="*"/>
                    </Grid.RowDefinitions>

                    <StackPanel Grid.Row="0" Margin="0,0,0,12">
                        <TextBlock Name="CatalogStatusText" Text="Ready to build SQL object catalog..."
                                   FontSize="14" FontWeight="Bold" Margin="0,0,0,10"/>
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <Grid.RowDefinitions>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                            </Grid.RowDefinitions>
                            <TextBlock Grid.Row="0" Grid.Column="0" Text="Catalog File:" FontWeight="Bold" Margin="0,3,8,3"/>
                            <TextBlock Grid.Row="0" Grid.Column="1" Name="CatalogPathText" Foreground="#333" TextWrapping="Wrap"/>
                            <TextBlock Grid.Row="1" Grid.Column="0" Text="Generated:" FontWeight="Bold" Margin="0,3,8,3"/>
                            <TextBlock Grid.Row="1" Grid.Column="1" Name="CatalogGeneratedText" Foreground="#333"/>
                            <TextBlock Grid.Row="2" Grid.Column="0" Text="Entries:" FontWeight="Bold" Margin="0,3,8,3"/>
                            <TextBlock Grid.Row="2" Grid.Column="1" Name="CatalogEntryCountText" Foreground="#333"/>
                        </Grid>
                    </StackPanel>

                    <TextBlock Grid.Row="1" Text="Catalog entries from the current Sql_Files deployment tree"
                               Foreground="#666" Margin="0,0,0,10"/>

                    <DataGrid Grid.Row="2"
                              Name="CatalogDataGrid"
                              AutoGenerateColumns="False"
                              IsReadOnly="True"
                              CanUserAddRows="False"
                              CanUserDeleteRows="False"
                              HeadersVisibility="Column"
                              GridLinesVisibility="Horizontal"
                              RowBackground="White"
                              AlternatingRowBackground="#F7F9FC"
                              BorderThickness="1"
                              BorderBrush="#DADCE0">
                        <DataGrid.Columns>
                            <DataGridTextColumn Header="Path" Binding="{Binding RelativePath}" Width="2.5*"/>
                            <DataGridTextColumn Header="Category" Binding="{Binding Category}" Width="*"/>
                            <DataGridTextColumn Header="Type" Binding="{Binding ObjectType}" Width="*"/>
                            <DataGridTextColumn Header="Name" Binding="{Binding ObjectName}" Width="1.5*"/>
                            <DataGridTextColumn Header="Params" Binding="{Binding ParameterCount}" Width="0.7*"/>
                            <DataGridTextColumn Header="Refs" Binding="{Binding ReferenceCount}" Width="0.7*"/>
                            <DataGridTextColumn Header="Writes" Binding="{Binding WriteTargetCount}" Width="0.8*"/>
                            <DataGridTextColumn Header="Calls" Binding="{Binding CalledRoutineCount}" Width="0.8*"/>
                            <DataGridTextColumn Header="Codebase Calls" Binding="{Binding CodebaseCallCount}" Width="1.0*"/>
                        </DataGrid.Columns>
                    </DataGrid>
                </Grid>

                <Border Name="ErrorBorder" Background="#FFEBEE" BorderBrush="#F44336"
                        BorderThickness="1" CornerRadius="3" Padding="10"
                        Margin="0,10,0,0" VerticalAlignment="Bottom" Visibility="Collapsed">
                    <TextBlock Name="ErrorText" Foreground="#C62828" TextWrapping="Wrap"/>
                </Border>
            </Grid>
        </Border>

        <Border Grid.Row="3" Name="SummaryBorder" Background="#E8F5E9" BorderBrush="#4CAF50"
                BorderThickness="1" CornerRadius="5" Padding="15"
                Margin="0,20,0,0" Visibility="Collapsed">
            <StackPanel>
                <TextBlock Text="Deployment Successful!" FontSize="16" FontWeight="Bold"
                           Foreground="#2E7D32" Margin="0,0,0,10"/>
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="Auto"/>
                    </Grid.ColumnDefinitions>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                    </Grid.RowDefinitions>
                    <TextBlock Grid.Row="0" Grid.Column="0" Text="Schemas Deployed:"  Margin="0,2"/>
                    <TextBlock Grid.Row="0" Grid.Column="1" Name="SummarySchemas"     Margin="10,2" FontWeight="Bold"/>
                    <TextBlock Grid.Row="1" Grid.Column="0" Text="Migrations Applied:" Margin="0,2"/>
                    <TextBlock Grid.Row="1" Grid.Column="1" Name="SummaryMigrations"  Margin="10,2" FontWeight="Bold"/>
                    <TextBlock Grid.Row="2" Grid.Column="0" Text="Views Deployed:" Margin="0,2"/>
                    <TextBlock Grid.Row="2" Grid.Column="1" Name="SummaryViews" Margin="10,2" FontWeight="Bold"/>
                    <TextBlock Grid.Row="3" Grid.Column="0" Text="Stored Procedures:" Margin="0,2"/>
                    <TextBlock Grid.Row="3" Grid.Column="1" Name="SummaryStoredProcs" Margin="10,2" FontWeight="Bold"/>
                    <TextBlock Grid.Row="4" Grid.Column="0" Text="Triggers Deployed:" Margin="0,2"/>
                    <TextBlock Grid.Row="4" Grid.Column="1" Name="SummaryTriggers" Margin="10,2" FontWeight="Bold"/>
                    <TextBlock Grid.Row="5" Grid.Column="0" Text="Seed Data Files:"   Margin="0,2"/>
                    <TextBlock Grid.Row="5" Grid.Column="1" Name="SummarySeedData"    Margin="10,2" FontWeight="Bold"/>
                </Grid>
            </StackPanel>
        </Border>

        <Border Grid.Row="4" Name="ProviderSwitchBorder" Background="#E3F2FD" BorderBrush="#64B5F6"
                BorderThickness="1" CornerRadius="5" Padding="12" Margin="0,16,0,0" Visibility="Collapsed">
            <StackPanel>
                <TextBlock Name="ProviderSwitchStatusText" Text="Switching database provider..."
                           FontWeight="SemiBold" Foreground="#1565C0" Margin="0,0,0,8"/>
                <ProgressBar Name="ProviderSwitchProgress" Height="10" Minimum="0" Maximum="100" Value="0"/>
            </StackPanel>
        </Border>

        <StackPanel Grid.Row="5" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,20,0,0">
            <Button Name="ModeSwitchButton" Content="Object Catalog Mode"
                Width="170" Height="35" Margin="0,0,10,0"
                Background="#37474F" Foreground="White"
                BorderThickness="0" FontWeight="Bold" Cursor="Hand"/>
            <Button Name="DeployButton" Content="Start Deployment"
                    Width="140" Height="35"
                    Background="#2196F3" Foreground="White"
                    BorderThickness="0" FontWeight="Bold" Cursor="Hand"/>
            <Button Name="CloseButton" Content="Close"
                    Width="100" Height="35" Margin="10,0,0,0"
                    Background="#9E9E9E" Foreground="White"
                    BorderThickness="0" Cursor="Hand"/>
        </StackPanel>
    </Grid>
</Window>
"@

$reader = [System.Xml.XmlNodeReader]::new([xml]$xaml)
$window = [Windows.Markup.XamlReader]::Load($reader)

$serverText = $window.FindName("ServerText")
$databaseText = $window.FindName("DatabaseText")
$userText = $window.FindName("UserText")
$overallStatusText = $window.FindName("OverallStatusText")
$currentFileText = $window.FindName("CurrentFileText")
$schemaProgress = $window.FindName("SchemaProgress")
$schemaCount = $window.FindName("SchemaCount")
$migrationProgress = $window.FindName("MigrationProgress")
$migrationCount = $window.FindName("MigrationCount")
$viewProgress = $window.FindName("ViewProgress")
$viewCount = $window.FindName("ViewCount")
$storedProcProgress = $window.FindName("StoredProcProgress")
$storedProcCount = $window.FindName("StoredProcCount")
$triggerProgress = $window.FindName("TriggerProgress")
$triggerCount = $window.FindName("TriggerCount")
$seedDataProgress = $window.FindName("SeedDataProgress")
$seedDataCount = $window.FindName("SeedDataCount")
$errorBorder = $window.FindName("ErrorBorder")
$errorText = $window.FindName("ErrorText")
$summaryBorder = $window.FindName("SummaryBorder")
$summarySchemas = $window.FindName("SummarySchemas")
$summaryMigrations = $window.FindName("SummaryMigrations")
$summaryViews = $window.FindName("SummaryViews")
$summaryStoredProcs = $window.FindName("SummaryStoredProcs")
$summaryTriggers = $window.FindName("SummaryTriggers")
$summarySeedData = $window.FindName("SummarySeedData")
$providerSwitchBorder = $window.FindName("ProviderSwitchBorder")
$providerSwitchStatusText = $window.FindName("ProviderSwitchStatusText")
$providerSwitchProgress = $window.FindName("ProviderSwitchProgress")
$providerValueText = $window.FindName("ProviderValueText")
$providerToggleButton = $window.FindName("ProviderToggleButton")
$deploymentPanel = $window.FindName("DeploymentPanel")
$catalogPanel = $window.FindName("CatalogPanel")
$catalogStatusText = $window.FindName("CatalogStatusText")
$catalogPathText = $window.FindName("CatalogPathText")
$catalogGeneratedText = $window.FindName("CatalogGeneratedText")
$catalogEntryCountText = $window.FindName("CatalogEntryCountText")
$catalogDataGrid = $window.FindName("CatalogDataGrid")
$modeSwitchButton = $window.FindName("ModeSwitchButton")
$deployButton = $window.FindName("DeployButton")
$closeButton = $window.FindName("CloseButton")

$script:WorkbenchProfile = [ordered]@{
    Server   = $Server
    Port     = $Port
    User     = $User
    Password = $Password
}

$script:MampConfigPath = $script:Config.Paths.MampConfigPath
$script:MampProfile = [ordered]@{
    Server   = $script:Config.Providers.Mamp.Server
    Port     = $script:Config.Providers.Mamp.Port
    User     = $script:Config.Providers.Mamp.User
    Password = $script:Config.Providers.Mamp.Password
}

if (Test-Path $script:MampConfigPath) {
    $mampConfigLines = Get-Content -Path $script:MampConfigPath -ErrorAction SilentlyContinue
    $currentSection = ''

    foreach ($line in $mampConfigLines) {
        $trimmedLine = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmedLine) -or $trimmedLine.StartsWith('#') -or $trimmedLine.StartsWith(';')) {
            continue
        }

        if ($trimmedLine -match '^\[(.+)\]$') {
            $currentSection = $matches[1].ToLowerInvariant()
            continue
        }

        if ($currentSection -eq 'client' -and $trimmedLine -match '^password\s*=\s*(.+)$') {
            $script:MampProfile.Password = $matches[1].Trim().Trim('"')
            continue
        }

        if ($currentSection -eq 'client' -and $trimmedLine -match '^port\s*=\s*(\d+)$') {
            $script:MampProfile.Port = $matches[1]
            continue
        }

        if ($currentSection -eq 'mysqld' -and $trimmedLine -match '^port\s*=\s*(\d+)$') {
            $script:MampProfile.Port = $matches[1]
            continue
        }
    }
}

$script:CurrentMode = 'deployment'
$script:CurrentProvider = 'workbench'
$script:CurrentServer = $script:WorkbenchProfile.Server
$script:CurrentPort = $script:WorkbenchProfile.Port
$script:CurrentUser = $script:WorkbenchProfile.User
$script:CurrentPassword = $script:WorkbenchProfile.Password
$script:DatabaseRoot = Join-Path $PSScriptRoot $script:Config.Paths.DatabaseRootFolder
$script:CatalogPath = Join-Path $PSScriptRoot $script:Config.Paths.CatalogFileName
$script:ValidationOutputsPath = Join-Path $PSScriptRoot $script:Config.Paths.OutputsFolder
$script:ExcludePatterns = $script:Config.Sql.ExcludePatterns
$script:MySqlClientInfoCache = @{}

function Sync-UiRender {
    $window.Dispatcher.Invoke([System.Action] {}, [System.Windows.Threading.DispatcherPriority]::Render)
}

function Show-ProviderSwitchProgress {
    param(
        [string]$StatusText,
        [double]$ProgressValue
    )

    $providerSwitchBorder.Visibility = 'Visible'
    $providerSwitchStatusText.Text = $StatusText
    $providerSwitchProgress.Value = [Math]::Max(0, [Math]::Min(100, $ProgressValue))
    Sync-UiRender
}

function Hide-ProviderSwitchProgress {
    $providerSwitchProgress.Value = 0
    $providerSwitchStatusText.Text = 'Switching database provider...'
    $providerSwitchBorder.Visibility = 'Collapsed'
    Sync-UiRender
}

function Update-ConnectionDisplay {
    $serverText.Text = "$($script:CurrentServer)`:$($script:CurrentPort)"
    $databaseText.Text = $Database
    $userText.Text = $script:CurrentUser
}

function Test-IsMampListener {
    $mampPort = [int]$script:MampProfile.Port
    $listener = Get-ListeningProcessInfo -HostName $script:MampProfile.Server -Port $mampPort

    if ($null -eq $listener) {
        return $false
    }

    if ([string]::IsNullOrWhiteSpace($listener.Path)) {
        return $false
    }

    return $listener.Path.StartsWith($script:Config.Paths.MampRoot, [System.StringComparison]::OrdinalIgnoreCase)
}

function Get-InitialProvider {
    if (Test-IsMampListener) {
        return 'mamp'
    }

    $mampProcess = Get-Process -Name 'MAMP', 'LauncherMAMP' -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($null -ne $mampProcess) {
        return 'mamp'
    }

    return 'workbench'
}

function Set-ProviderState {
    param(
        [ValidateSet('workbench', 'mamp')]
        [string]$Provider
    )

    $script:CurrentProvider = $Provider

    if ($Provider -eq 'mamp') {
        $script:CurrentServer = $script:MampProfile.Server
        $script:CurrentPort = $script:MampProfile.Port
        $script:CurrentUser = $script:MampProfile.User
        $script:CurrentPassword = $script:MampProfile.Password
        $providerValueText.Text = 'MAMP'
        $providerValueText.Foreground = '#E65100'
        $providerToggleButton.Content = 'Switch to Workbench'
        $providerToggleButton.Background = '#FB8C00'
        $providerToggleButton.IsChecked = $true
    }
    else {
        $script:CurrentServer = $script:WorkbenchProfile.Server
        $script:CurrentPort = $script:WorkbenchProfile.Port
        $script:CurrentUser = $script:WorkbenchProfile.User
        $script:CurrentPassword = $script:WorkbenchProfile.Password
        $providerValueText.Text = 'MySQL Workbench'
        $providerValueText.Foreground = '#1565C0'
        $providerToggleButton.Content = 'Switch to MAMP'
        $providerToggleButton.Background = '#1976D2'
        $providerToggleButton.IsChecked = $false
    }

    Update-ConnectionDisplay
}

function Test-TcpPortOpen {
    param(
        [string]$HostName,
        [int]$Port,
        [int]$TimeoutMilliseconds = 1000
    )

    $client = New-Object System.Net.Sockets.TcpClient

    try {
        $asyncResult = $client.BeginConnect($HostName, $Port, $null, $null)
        if (-not $asyncResult.AsyncWaitHandle.WaitOne($TimeoutMilliseconds, $false)) {
            return $false
        }

        $client.EndConnect($asyncResult)
        return $true
    }
    catch {
        return $false
    }
    finally {
        $client.Dispose()
    }
}

function Wait-ForTcpPortState {
    param(
        [string]$HostName,
        [int]$Port,
        [bool]$ExpectedOpen,
        [int]$TimeoutSeconds = 30,
        [string]$ProgressStatusText = '',
        [int]$ProgressStart = 0,
        [int]$ProgressEnd = 100
    )

    $startTime = Get-Date
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if (-not [string]::IsNullOrWhiteSpace($ProgressStatusText)) {
            $elapsedSeconds = ((Get-Date) - $startTime).TotalSeconds
            $ratio = [Math]::Min(1, $elapsedSeconds / [Math]::Max($TimeoutSeconds, 1))
            $currentProgress = $ProgressStart + (($ProgressEnd - $ProgressStart) * $ratio)
            Show-ProviderSwitchProgress -StatusText $ProgressStatusText -ProgressValue $currentProgress
        }

        if ((Test-TcpPortOpen -HostName $HostName -Port $Port) -eq $ExpectedOpen) {
            if (-not [string]::IsNullOrWhiteSpace($ProgressStatusText)) {
                Show-ProviderSwitchProgress -StatusText $ProgressStatusText -ProgressValue $ProgressEnd
            }
            return $true
        }

        Start-Sleep -Milliseconds 500
    }

    return $false
}

function Get-WorkbenchService {
    $serviceRecord = Get-CimInstance Win32_Service -ErrorAction SilentlyContinue |
    Where-Object {
        $_.PathName -match 'mysqld\.exe' -or
        $_.Name -match 'mysql|mariadb' -or
        $_.DisplayName -match 'mysql|mariadb'
    } |
    Sort-Object @{ Expression = { if ($_.PathName -match 'mysqld\.exe') { 0 } else { 1 } } }, Name |
    Select-Object -First 1

    if ($null -eq $serviceRecord) {
        return $null
    }

    return Get-Service -Name $serviceRecord.Name -ErrorAction SilentlyContinue
}

function Test-IsCurrentProcessElevated {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Invoke-WorkbenchServiceAction {
    param(
        [ValidateSet('Start', 'Stop')]
        [string]$Action,
        [string]$ServiceName
    )

    if (Test-IsCurrentProcessElevated) {
        if ($Action -eq 'Start') {
            Start-Service -Name $ServiceName -ErrorAction Stop
            return
        }

        Stop-Service -Name $ServiceName -Force -ErrorAction Stop
        return
    }

    $serviceCommand = if ($Action -eq 'Start') {
        "Start-Service -Name '$ServiceName' -ErrorAction Stop"
    }
    else {
        "Stop-Service -Name '$ServiceName' -Force -ErrorAction Stop"
    }

    Show-ProviderSwitchProgress -StatusText "Waiting for Windows elevation to $($Action.ToLowerInvariant()) Workbench MySQL service..." -ProgressValue 58

    $elevatedProcess = Start-Process -FilePath 'powershell.exe' `
        -ArgumentList @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-Command', $serviceCommand) `
        -Verb RunAs `
        -WindowStyle Hidden `
        -PassThru

    $elevatedProcess.WaitForExit()

    if ($elevatedProcess.ExitCode -ne 0) {
        throw "Elevated request to $($Action.ToLowerInvariant()) Workbench MySQL service '$ServiceName' failed or was canceled."
    }
}

function Get-ListeningProcessInfo {
    param(
        [string]$HostName,
        [int]$Port
    )

    $connection = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue |
    Select-Object -First 1

    if ($null -eq $connection) {
        return $null
    }

    $process = Get-Process -Id $connection.OwningProcess -ErrorAction SilentlyContinue
    if ($null -eq $process) {
        return [PSCustomObject]@{
            ProcessId   = $connection.OwningProcess
            ProcessName = $null
            Path        = $null
        }
    }

    return [PSCustomObject]@{
        ProcessId   = $connection.OwningProcess
        ProcessName = $process.ProcessName
        Path        = $process.Path
    }
}

function Get-WorkbenchMysqlAdminExePath {
    $candidate = Get-ChildItem $script:Config.Paths.WorkbenchInstallRoot -Recurse -Filter 'mysqladmin.exe' -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -match '\\bin\\mysqladmin\.exe$' } |
    Sort-Object FullName -Descending |
    Select-Object -First 1

    if ($candidate) {
        return $candidate.FullName
    }

    try {
        $command = Get-Command mysqladmin -ErrorAction Stop
        return $command.Source
    }
    catch {
    }

    throw "Unable to locate mysqladmin.exe for Workbench mode under $($script:Config.Paths.WorkbenchInstallRoot)."
}

function Get-WorkbenchMysqldExePath {
    $candidate = Get-ChildItem $script:Config.Paths.WorkbenchInstallRoot -Recurse -Filter 'mysqld.exe' -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -match '\\bin\\mysqld\.exe$' } |
    Sort-Object FullName -Descending |
    Select-Object -First 1

    if ($candidate) {
        return $candidate.FullName
    }

    throw "Unable to locate mysqld.exe for Workbench mode under $($script:Config.Paths.WorkbenchInstallRoot)."
}

function Get-WorkbenchMyIniPath {
    $mysqldExe = Get-WorkbenchMysqldExePath
    $versionFolder = Split-Path (Split-Path $mysqldExe -Parent) -Parent | Split-Path -Leaf
    $candidatePaths = @(
        (Join-Path $script:Config.Paths.WorkbenchProgramData (Join-Path $versionFolder 'my.ini')),
        $script:Config.Paths.DefaultWorkbenchMyIni
    )

    foreach ($candidate in $candidatePaths) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    throw "Unable to locate Workbench my.ini under $($script:Config.Paths.WorkbenchProgramData)."
}

function Get-WorkbenchConfigInfo {
    $myIniPath = Get-WorkbenchMyIniPath
    $configLines = Get-Content -Path $myIniPath -ErrorAction Stop

    $dataDirLine = $configLines | Where-Object { $_ -match '^\s*datadir\s*=\s*(.+)$' } | Select-Object -First 1
    $errorLogLine = $configLines | Where-Object { $_ -match '^\s*log-error\s*=\s*(.+)$' } | Select-Object -First 1

    $dataDirectory = $null
    if ($dataDirLine -and $dataDirLine -match '^\s*datadir\s*=\s*(.+)$') {
        $dataDirectory = $matches[1].Trim().Trim('"').Replace('/', '\\')
    }

    $errorLogPath = $null
    if ($errorLogLine -and $errorLogLine -match '^\s*log-error\s*=\s*(.+)$') {
        $rawErrorLogValue = $matches[1].Trim().Trim('"').Replace('/', '\\')
        if ([System.IO.Path]::IsPathRooted($rawErrorLogValue)) {
            $errorLogPath = $rawErrorLogValue
        }
        elseif (-not [string]::IsNullOrWhiteSpace($dataDirectory)) {
            $errorLogPath = Join-Path $dataDirectory $rawErrorLogValue
        }
    }

    return [PSCustomObject]@{
        MyIniPath     = $myIniPath
        DataDirectory = $dataDirectory
        ErrorLogPath  = $errorLogPath
    }
}

function Stop-WorkbenchStandaloneServer {
    $mysqlAdminExe = Get-WorkbenchMysqlAdminExePath
    $workbenchPort = [int]$script:WorkbenchProfile.Port
    $currentFileText.Text = 'Stopping Workbench standalone MySQL server...'

    $arguments = @(
        "--host=$($script:WorkbenchProfile.Server)",
        "--port=$workbenchPort",
        "--user=$($script:WorkbenchProfile.User)",
        "--password=$($script:WorkbenchProfile.Password)",
        'shutdown'
    )

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $mysqlAdminExe
    foreach ($argument in $arguments) {
        [void]$psi.ArgumentList.Add($argument)
    }
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $psi
    $process.Start() | Out-Null
    [void]$process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    if ($process.ExitCode -ne 0) {
        $detail = (@($stderr) | Where-Object { $_ -and $_.Trim() }) -join "`n"
        throw "Unable to stop Workbench standalone MySQL server: $detail"
    }

    if (-not (Wait-ForTcpPortState -HostName $script:WorkbenchProfile.Server -Port $workbenchPort -ExpectedOpen $false -TimeoutSeconds 30 -ProgressStatusText 'Waiting for standalone Workbench MySQL to stop...' -ProgressStart 20 -ProgressEnd 42)) {
        throw "Workbench standalone MySQL server did not stop listening on port $workbenchPort within 30 seconds."
    }
}

function Start-WorkbenchStandaloneServer {
    $mysqldExe = Get-WorkbenchMysqldExePath
    $configInfo = Get-WorkbenchConfigInfo
    $workbenchPort = [int]$script:WorkbenchProfile.Port
    $currentFileText.Text = 'Starting Workbench standalone MySQL server...'
    Show-ProviderSwitchProgress -StatusText 'Starting Workbench standalone MySQL server...' -ProgressValue 60

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $mysqldExe
    [void]$psi.ArgumentList.Add("--defaults-file=$($configInfo.MyIniPath)")
    [void]$psi.ArgumentList.Add('--console')
    $psi.WorkingDirectory = Split-Path $mysqldExe -Parent
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $psi
    $process.Start() | Out-Null
    Start-Sleep -Seconds 2

    if ($process.HasExited) {
        $stdout = $process.StandardOutput.ReadToEnd()
        $stderr = $process.StandardError.ReadToEnd()
        $logTail = $null

        if (-not [string]::IsNullOrWhiteSpace($configInfo.ErrorLogPath) -and (Test-Path $configInfo.ErrorLogPath)) {
            $logTail = (Get-Content -Path $configInfo.ErrorLogPath -Tail 20 -ErrorAction SilentlyContinue) -join "`n"
        }

        $detailParts = @($stderr, $stdout, $logTail) | Where-Object { $_ -and $_.Trim() }
        $detail = $detailParts -join "`n`n"
        throw "Workbench standalone MySQL exited immediately after launch using $($configInfo.MyIniPath). $detail"
    }

    if (-not (Wait-ForTcpPortState -HostName $script:WorkbenchProfile.Server -Port $workbenchPort -ExpectedOpen $true -TimeoutSeconds 30 -ProgressStatusText 'Waiting for standalone Workbench MySQL to come online...' -ProgressStart 62 -ProgressEnd 88)) {
        $logTail = $null
        if (-not [string]::IsNullOrWhiteSpace($configInfo.ErrorLogPath) -and (Test-Path $configInfo.ErrorLogPath)) {
            $logTail = (Get-Content -Path $configInfo.ErrorLogPath -Tail 20 -ErrorAction SilentlyContinue) -join "`n"
        }

        throw "Workbench standalone MySQL server did not start listening on port $workbenchPort within 30 seconds. Config: $($configInfo.MyIniPath)`n`n$logTail"
    }
}

function Get-MampExePath {
    $candidates = $script:Config.Paths.MampExecutablePaths

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    throw "Unable to locate MAMP.exe under $($script:Config.Paths.MampRoot)."
}

function Get-MampMysqlAdminExePath {
    $candidates = $script:Config.Paths.MampMysqlAdminPaths

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    $dynamicCandidate = Get-Item (Join-Path $script:Config.Paths.MampRoot 'bin\mysql*\bin\mysqladmin.exe') -ErrorAction SilentlyContinue |
    Sort-Object FullName -Descending |
    Select-Object -First 1

    if ($dynamicCandidate) {
        return $dynamicCandidate.FullName
    }

    throw "Unable to locate mysqladmin.exe under $($script:Config.Paths.MampRoot)."
}

function Get-MampManagedProcesses {
    $mampRootNormalized = $script:Config.Paths.MampRoot.TrimEnd('\').ToLowerInvariant() + '\'
    $managedExecutableNames = @('MAMP.exe', 'LauncherMAMP.exe', 'httpd.exe', 'mysqld.exe', 'ApacheMonitor.exe')

    return @(
        Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
        Where-Object {
            if ($_.Name -notin $managedExecutableNames) {
                return $false
            }

            $executablePath = $_.ExecutablePath
            if ([string]::IsNullOrWhiteSpace($executablePath)) {
                return $false
            }

            return $executablePath.ToLowerInvariant().StartsWith($mampRootNormalized)
        } |
        ForEach-Object {
            [PSCustomObject]@{
                Id          = $_.ProcessId
                ProcessName = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
                Path        = $_.ExecutablePath
            }
        }
    )
}

function Stop-MampProcessesByName {
    param(
        [string[]]$ProcessNames,
        [string]$StatusText,
        [int]$ProgressValue
    )

    $matchingProcesses = @(Get-MampManagedProcesses | Where-Object { $_.ProcessName -in $ProcessNames })
    if ($matchingProcesses.Count -eq 0) {
        return
    }

    Show-ProviderSwitchProgress -StatusText $StatusText -ProgressValue $ProgressValue
    foreach ($process in $matchingProcesses) {
        try {
            Stop-Process -Id $process.Id -Force -ErrorAction Stop
        }
        catch {
        }
    }

    Start-Sleep -Milliseconds 500
}

function Ensure-WorkbenchServerRunning {
    $service = Get-WorkbenchService
    $workbenchPort = [int]$script:WorkbenchProfile.Port

    if (Test-TcpPortOpen -HostName $script:WorkbenchProfile.Server -Port $workbenchPort) {
        return
    }

    if (-not $service) {
        Start-WorkbenchStandaloneServer
        return
    }

    if ($service.Status -ne 'Running') {
        $currentFileText.Text = "Starting Workbench server service: $($service.Name)"
        Show-ProviderSwitchProgress -StatusText 'Starting Workbench MySQL service...' -ProgressValue 60
        Invoke-WorkbenchServiceAction -Action 'Start' -ServiceName $service.Name
    }

    if (-not (Wait-ForTcpPortState -HostName $script:WorkbenchProfile.Server -Port $workbenchPort -ExpectedOpen $true -TimeoutSeconds 30 -ProgressStatusText 'Waiting for Workbench MySQL to come online...' -ProgressStart 62 -ProgressEnd 88)) {
        throw "Workbench MySQL service '$($service.Name)' did not start listening on port $workbenchPort within 30 seconds."
    }
}

function Stop-WorkbenchServer {
    $service = Get-WorkbenchService
    $workbenchPort = [int]$script:WorkbenchProfile.Port

    if (-not (Test-TcpPortOpen -HostName $script:WorkbenchProfile.Server -Port $workbenchPort)) {
        return
    }

    if (-not $service) {
        $listener = Get-ListeningProcessInfo -HostName $script:WorkbenchProfile.Server -Port $workbenchPort
        if ($null -ne $listener -and $listener.ProcessName -eq 'mysqld') {
            Stop-WorkbenchStandaloneServer
            return
        }

        throw "Workbench MySQL appears to be listening on port $workbenchPort, but no Windows service or standalone mysqld process was found to stop it."
    }

    if ($service.Status -eq 'Running') {
        $currentFileText.Text = "Stopping Workbench server service: $($service.Name)"
        Show-ProviderSwitchProgress -StatusText 'Stopping Workbench MySQL service...' -ProgressValue 18
        Invoke-WorkbenchServiceAction -Action 'Stop' -ServiceName $service.Name
    }

    if (-not (Wait-ForTcpPortState -HostName $script:WorkbenchProfile.Server -Port $workbenchPort -ExpectedOpen $false -TimeoutSeconds 30 -ProgressStatusText 'Waiting for Workbench MySQL to stop...' -ProgressStart 20 -ProgressEnd 42)) {
        throw "Workbench MySQL service '$($service.Name)' did not stop listening on port $workbenchPort within 30 seconds."
    }
}

function Ensure-MampApplicationOpen {
    $mampProcess = Get-Process -Name 'MAMP', 'LauncherMAMP' -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($mampProcess) {
        return
    }

    $mampExe = Get-MampExePath
    $currentFileText.Text = "Opening MAMP application..."
    Show-ProviderSwitchProgress -StatusText 'Opening MAMP application...' -ProgressValue 48
    Start-Process -FilePath $mampExe | Out-Null
    Start-Sleep -Seconds 2
    Show-ProviderSwitchProgress -StatusText 'MAMP application opened. Waiting for MySQL...' -ProgressValue 54
}

function Restart-MampApplication {
    Close-MampApplication -ProgressValue 70
    Ensure-MampApplicationOpen
}

function Close-MampApplication {
    param(
        [int]$ProgressValue = 82
    )

    $mampProcesses = @(Get-MampManagedProcesses | Where-Object { $_.ProcessName -in @('MAMP', 'LauncherMAMP') })
    if ($mampProcesses.Count -eq 0) {
        return
    }

    $currentFileText.Text = 'Closing MAMP application...'
    Show-ProviderSwitchProgress -StatusText 'Closing MAMP application...' -ProgressValue $ProgressValue

    foreach ($mampProcess in $mampProcesses) {
        try {
            $process = Get-Process -Id $mampProcess.Id -ErrorAction Stop
            if ($process.MainWindowHandle -ne 0) {
                [void]$process.CloseMainWindow()
            }
        }
        catch {
        }
    }

    Start-Sleep -Seconds 2

    $remainingProcesses = @(Get-MampManagedProcesses | Where-Object { $_.ProcessName -in @('MAMP', 'LauncherMAMP') })
    if ($remainingProcesses.Count -gt 0) {
        Stop-MampProcessesByName -ProcessNames @('MAMP', 'LauncherMAMP') -StatusText 'Forcing MAMP application shutdown...' -ProgressValue $ProgressValue
    }
}

function Ensure-MampServerRunning {
    $mampPort = [int]$script:MampProfile.Port

    if (Test-TcpPortOpen -HostName $script:MampProfile.Server -Port $mampPort) {
        return
    }

    Ensure-MampApplicationOpen

    if (-not (Wait-ForTcpPortState -HostName $script:MampProfile.Server -Port $mampPort -ExpectedOpen $true -TimeoutSeconds 30 -ProgressStatusText 'Waiting for MAMP MySQL to start...' -ProgressStart 56 -ProgressEnd 88)) {
        Restart-MampApplication

        if (-not (Wait-ForTcpPortState -HostName $script:MampProfile.Server -Port $mampPort -ExpectedOpen $true -TimeoutSeconds 30 -ProgressStatusText 'Waiting for MAMP MySQL to start after restart...' -ProgressStart 72 -ProgressEnd 88)) {
            throw "MAMP did not start MySQL on port $mampPort after reopening the application. Verify MAMP is configured to start MySQL automatically."
        }
    }
}

function Stop-MampServer {
    $mampPort = [int]$script:MampProfile.Port

    Close-MampApplication -ProgressValue 18

    if (-not (Wait-ForTcpPortState -HostName $script:MampProfile.Server -Port $mampPort -ExpectedOpen $false -TimeoutSeconds 30 -ProgressStatusText 'Waiting for MAMP to stop MySQL...' -ProgressStart 22 -ProgressEnd 74)) {
        throw "MAMP MySQL server did not stop listening on port $mampPort after closing the MAMP application. Verify the MAMP setting to stop servers when the app closes is enabled."
    }
}

function Switch-ProviderEnvironment {
    param(
        [ValidateSet('workbench', 'mamp')]
        [string]$TargetProvider
    )

    if ($TargetProvider -eq $script:CurrentProvider) {
        return
    }

    if ($TargetProvider -eq 'mamp') {
        Show-ProviderSwitchProgress -StatusText 'Switching provider to MAMP...' -ProgressValue 8
        $overallStatusText.Text = 'Switching provider to MAMP...'
        Stop-WorkbenchServer
        Ensure-MampServerRunning
        Show-ProviderSwitchProgress -StatusText 'Finalizing MAMP provider...' -ProgressValue 94
        Set-ProviderState -Provider 'mamp'
        $overallStatusText.Text = 'MAMP provider is ready.'
        $currentFileText.Text = 'Switched provider to MAMP and started its MySQL server.'
        return
    }

    Show-ProviderSwitchProgress -StatusText 'Switching provider to Workbench...' -ProgressValue 8
    $overallStatusText.Text = 'Switching provider to Workbench...'
    Stop-MampServer
    Ensure-WorkbenchServerRunning
    Show-ProviderSwitchProgress -StatusText 'Finalizing Workbench provider...' -ProgressValue 94
    Set-ProviderState -Provider 'workbench'
    $overallStatusText.Text = 'Workbench provider is ready.'
    $currentFileText.Text = 'Switched provider to Workbench and started its MySQL service.'
}

Update-ConnectionDisplay

# Locate mysql.exe under the default MySQL Server install directory.
# Falls back to whatever is on the system PATH.
function Find-MySqlExe {
    if ($script:CurrentProvider -eq 'mamp') {
        $mampRoot = $script:Config.Paths.MampRoot
        if (Test-Path $mampRoot) {
            $mampHit = Get-ChildItem $mampRoot -Recurse -Filter 'mysql.exe' -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match '\\mysql\\bin\\mysql\.exe$' } |
            Sort-Object FullName -Descending |
            Select-Object -First 1

            if ($mampHit) {
                return $mampHit.FullName
            }
        }

        throw "mysql.exe not found under '$mampRoot' for MAMP mode."
    }

    $installRoot = $script:Config.Paths.WorkbenchInstallRoot
    if (Test-Path $installRoot) {
        $hit = Get-ChildItem $installRoot -Recurse -Filter "mysql.exe" -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match "\\bin\\mysql\.exe$" } |
        Sort-Object FullName -Descending |
        Select-Object -First 1
        if ($hit) { return $hit.FullName }
    }
    try {
        $null = Get-Command mysql -ErrorAction Stop
        return "mysql"
    }
    catch { }
    throw "mysql.exe not found under '$installRoot' and not on the system PATH."
}

function Get-MySqlClientInfo {
    param(
        [string]$ExePath
    )

    if ($script:MySqlClientInfoCache.ContainsKey($ExePath)) {
        return $script:MySqlClientInfoCache[$ExePath]
    }

    $versionText = & $ExePath --version 2>&1 | Out-String
    $versionMatch = [regex]::Match($versionText, 'Distrib\s+(?<version>[0-9]+(?:\.[0-9]+){1,3})', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    $majorVersion = if ($versionMatch.Success) { [int]($versionMatch.Groups['version'].Value.Split('.')[0]) } else { 0 }

    $clientInfo = [ordered]@{
        exePath      = $ExePath
        versionText  = $versionText.Trim()
        version      = if ($versionMatch.Success) { $versionMatch.Groups['version'].Value } else { [string]::Empty }
        majorVersion = $majorVersion
    }

    $script:MySqlClientInfoCache[$ExePath] = $clientInfo
    return $clientInfo
}

function Split-MySqlScriptByDelimiter {
    param(
        [string]$SqlText
    )

    if ([string]::IsNullOrWhiteSpace($SqlText)) {
        return @()
    }

    $currentDelimiter = ';'
    $currentLines = New-Object System.Collections.Generic.List[string]
    $segments = New-Object System.Collections.Generic.List[object]

    $flushSegment = {
        param(
            [string]$Delimiter,
            [System.Collections.Generic.List[string]]$Lines,
            [System.Collections.Generic.List[object]]$SegmentList
        )

        $segmentText = ($Lines -join "`r`n").Trim()
        if (-not [string]::IsNullOrWhiteSpace($segmentText)) {
            $SegmentList.Add([ordered]@{
                    delimiter = $Delimiter
                    sqlText   = $segmentText
                })
        }
    }

    foreach ($line in ($SqlText -split "`r?`n")) {
        $delimiterMatch = [regex]::Match($line, '^\s*DELIMITER\s+(?<delimiter>\S+)\s*$', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if ($delimiterMatch.Success) {
            & $flushSegment $currentDelimiter $currentLines $segments
            $currentLines = New-Object System.Collections.Generic.List[string]
            $currentDelimiter = $delimiterMatch.Groups['delimiter'].Value
            continue
        }

        $currentLines.Add($line)
    }

    & $flushSegment $currentDelimiter $currentLines $segments
    return @($segments | ForEach-Object { $_ })
}

function Test-RequiresLegacyBatchCompatibility {
    param(
        [hashtable]$ClientInfo
    )

    return $script:CurrentProvider -eq 'mamp' -and $ClientInfo.majorVersion -gt 0 -and $ClientInfo.majorVersion -lt 8
}

function Invoke-MySqlBatchFile {
    param(
        [string]$Exe,
        [string]$DefaultsFile,
        [string]$TargetFile,
        [string]$StdoutFile,
        [string]$StderrFile,
        [string]$Delimiter = ';'
    )

    $mysqlArgs = @(
        "--defaults-extra-file=$DefaultsFile",
        "-h$($script:CurrentServer)",
        "-P$($script:CurrentPort)",
        '--default-character-set=utf8mb4',
        "--delimiter=$Delimiter",
        $Database
    )

    $quotedMysqlExe = '"' + $Exe.Replace('"', '""') + '"'
    $quotedTargetFile = '"' + $TargetFile.Replace('"', '""') + '"'
    $quotedStdoutFile = '"' + $StdoutFile.Replace('"', '""') + '"'
    $quotedStderrFile = '"' + $StderrFile.Replace('"', '""') + '"'
    $quotedMysqlArgs = @(
        $mysqlArgs | ForEach-Object {
            if ($_ -match '\s') {
                '"' + $_.Replace('"', '""') + '"'
            }
            else {
                $_
            }
        }
    ) -join ' '

    $cmdScript = "$quotedMysqlExe $quotedMysqlArgs < $quotedTargetFile 1> $quotedStdoutFile 2> $quotedStderrFile"
    & cmd.exe /d /c $cmdScript | Out-Null
    return $LASTEXITCODE
}

# Returns an ordered list of *.sql FileInfo objects for $BasePath.
# If a sibling folder named "Updated$FolderName" exists, files with matching
# relative paths are replaced by the Updated version and extra files are appended.
function Get-SqlFiles {
    param(
        [string]  $BasePath,
        [string[]]$ExcludedPatterns,
        [switch]  $Recurse
    )
    $folderName = Split-Path $BasePath -Leaf
    $updatedPath = Join-Path (Split-Path $BasePath -Parent) "Updated$folderName"
    $fileMap = [ordered]@{}

    if (Test-Path $BasePath) {
        $fileQueryArgs = @{ Path = $BasePath; Filter = '*.sql'; ErrorAction = 'SilentlyContinue' }
        if ($Recurse) { $fileQueryArgs['Recurse'] = $true }
        Get-ChildItem @fileQueryArgs |
        Where-Object { $n = $_.Name; -not ($ExcludedPatterns | Where-Object { $n -like $_ }) } |
        ForEach-Object { $fileMap[$_.FullName.Substring($BasePath.Length).TrimStart('\')] = $_ }
    }

    if (Test-Path $updatedPath) {
        $updatedFileQueryArgs = @{ Path = $updatedPath; Filter = '*.sql'; ErrorAction = 'SilentlyContinue' }
        if ($Recurse) { $updatedFileQueryArgs['Recurse'] = $true }
        Get-ChildItem @updatedFileQueryArgs |
        Where-Object { $n = $_.Name; -not ($ExcludedPatterns | Where-Object { $n -like $_ }) } |
        ForEach-Object { $fileMap[$_.FullName.Substring($updatedPath.Length).TrimStart('\')] = $_ }
    }

    return $fileMap.Values
}

function New-MySqlDefaultsFile {
    $tempDefaultsFile = [System.IO.Path]::ChangeExtension([System.IO.Path]::GetTempFileName(), '.cnf')
    $defaultsContent = @"
[client]
user=$($script:CurrentUser)
password=$($script:CurrentPassword)
"@

    [System.IO.File]::WriteAllText(
        $tempDefaultsFile,
        $defaultsContent,
        [System.Text.UTF8Encoding]::new($false)
    )

    return $tempDefaultsFile
}

# Run a single SQL statement via -e.  No DELIMITER directives involved, so
# ProcessStartInfo pipe-based stdin is fine here.
function Execute-SqlCommand {
    param(
        [string]$SqlCommand,
        [switch]$NoDatabase
    )
    $defaultsFile = $null

    try {
        $exe = Find-MySqlExe
        $defaultsFile = New-MySqlDefaultsFile
        $dbPart = if ($NoDatabase) { "" } else { $Database }
        $argStr = "--defaults-extra-file=`"$defaultsFile`" -h$($script:CurrentServer) -P$($script:CurrentPort) --default-character-set=utf8mb4 $dbPart -e `"$SqlCommand`""

        $psi = New-Object System.Diagnostics.ProcessStartInfo
        $psi.FileName = $exe
        $psi.Arguments = $argStr
        $psi.RedirectStandardOutput = $true
        $psi.RedirectStandardError = $true
        $psi.UseShellExecute = $false
        $psi.CreateNoWindow = $true

        $proc = New-Object System.Diagnostics.Process
        $proc.StartInfo = $psi
        $proc.Start() | Out-Null
        $stdout = $proc.StandardOutput.ReadToEnd()
        $stderr = $proc.StandardError.ReadToEnd()
        $proc.WaitForExit()

        if ($proc.ExitCode -ne 0) { throw "MySQL error: $stderr" }
        return $true
    }
    finally {
        if ($null -ne $defaultsFile -and (Test-Path $defaultsFile -ErrorAction SilentlyContinue)) {
            Remove-Item $defaultsFile -ErrorAction SilentlyContinue
        }
    }
}

# Execute a .sql file by redirecting a real file handle into mysql.exe.
# This is required for client-side DELIMITER handling to behave consistently on
# Windows across both MySQL 5.7 and 8.0. Pipe-based stdin is not reliable here
# for stored-procedure scripts.
#
# When FK checks must be disabled the file content is wrapped with the toggle
# statements so they run in the same mysql session.
function Execute-SqlFile {
    param(
        [string]$FilePath,
        [switch]$DisableForeignKeyChecks
    )
    if (-not (Test-Path $FilePath)) { throw "File not found: $FilePath" }

    $exe = Find-MySqlExe
    $tempSql = $null
    $defaultsFile = $null
    $stdoutFile = $null
    $stderrFile = $null
    $legacyBatchFiles = New-Object System.Collections.Generic.List[string]

    try {
        $defaultsFile = New-MySqlDefaultsFile

        $body = [System.IO.File]::ReadAllText($FilePath)

        if ($DisableForeignKeyChecks) {
            $tempSql = [System.IO.Path]::ChangeExtension([System.IO.Path]::GetTempFileName(), '.sql')
            # UTF-8 without BOM — a BOM at byte 0 corrupts mysql's client-side parser.
            [System.IO.File]::WriteAllText(
                $tempSql,
                "SET FOREIGN_KEY_CHECKS = 0;`r`n" + $body + "`r`nSET FOREIGN_KEY_CHECKS = 1;",
                [System.Text.UTF8Encoding]::new($false)
            )
            $targetFile = $tempSql
            $body = [System.IO.File]::ReadAllText($targetFile)
        }
        else {
            $targetFile = $FilePath
        }

        $clientInfo = Get-MySqlClientInfo -ExePath $exe
        if (Test-RequiresLegacyBatchCompatibility -ClientInfo $clientInfo) {
            $segments = @(Split-MySqlScriptByDelimiter -SqlText $body)
            foreach ($segment in $segments) {
                $segmentFile = [System.IO.Path]::ChangeExtension([System.IO.Path]::GetTempFileName(), '.sql')
                [System.IO.File]::WriteAllText($segmentFile, $segment.sqlText, [System.Text.UTF8Encoding]::new($false))
                $legacyBatchFiles.Add($segmentFile)

                $stdoutFile = [System.IO.Path]::GetTempFileName()
                $stderrFile = [System.IO.Path]::GetTempFileName()
                $exitCode = Invoke-MySqlBatchFile -Exe $exe -DefaultsFile $defaultsFile -TargetFile $segmentFile -StdoutFile $stdoutFile -StderrFile $stderrFile -Delimiter $segment.delimiter

                $stdout = if (Test-Path $stdoutFile) { [System.IO.File]::ReadAllText($stdoutFile) } else { [string]::Empty }
                $stderr = if (Test-Path $stderrFile) { [System.IO.File]::ReadAllText($stderrFile) } else { [string]::Empty }

                if ($exitCode -ne 0) {
                    $detail = (@($stdout, $stderr) | Where-Object { $_ -and $_.Trim() }) -join "`n"
                    throw "MySQL error (exit $exitCode): $detail"
                }

                if (Test-Path $stdoutFile -ErrorAction SilentlyContinue) {
                    Remove-Item $stdoutFile -ErrorAction SilentlyContinue
                }

                if (Test-Path $stderrFile -ErrorAction SilentlyContinue) {
                    Remove-Item $stderrFile -ErrorAction SilentlyContinue
                }

                $stdoutFile = $null
                $stderrFile = $null
            }

            return $true
        }

        $stdoutFile = [System.IO.Path]::GetTempFileName()
        $stderrFile = [System.IO.Path]::GetTempFileName()
        $exitCode = Invoke-MySqlBatchFile -Exe $exe -DefaultsFile $defaultsFile -TargetFile $targetFile -StdoutFile $stdoutFile -StderrFile $stderrFile

        $stdout = if (Test-Path $stdoutFile) {
            [System.IO.File]::ReadAllText($stdoutFile)
        }
        else {
            [string]::Empty
        }

        $stderr = if (Test-Path $stderrFile) {
            [System.IO.File]::ReadAllText($stderrFile)
        }
        else {
            [string]::Empty
        }

        if ($exitCode -ne 0) {
            $detail = (@($stdout, $stderr) | Where-Object { $_ -and $_.Trim() }) -join "`n"
            throw "MySQL error (exit $exitCode): $detail"
        }
        return $true
    }
    finally {
        if ($null -ne $defaultsFile -and (Test-Path $defaultsFile -ErrorAction SilentlyContinue)) {
            Remove-Item $defaultsFile -ErrorAction SilentlyContinue
        }

        if ($null -ne $tempSql -and (Test-Path $tempSql -ErrorAction SilentlyContinue)) {
            Remove-Item $tempSql -ErrorAction SilentlyContinue
        }

        foreach ($legacyBatchFile in $legacyBatchFiles) {
            if (Test-Path $legacyBatchFile -ErrorAction SilentlyContinue) {
                Remove-Item $legacyBatchFile -ErrorAction SilentlyContinue
            }
        }

        if ($null -ne $stdoutFile -and (Test-Path $stdoutFile -ErrorAction SilentlyContinue)) {
            Remove-Item $stdoutFile -ErrorAction SilentlyContinue
        }

        if ($null -ne $stderrFile -and (Test-Path $stderrFile -ErrorAction SilentlyContinue)) {
            Remove-Item $stderrFile -ErrorAction SilentlyContinue
        }
    }
}

function Convert-CatalogToDisplayRows {
    param(
        [hashtable]$Catalog
    )

    return @(
        $Catalog.entries |
        Sort-Object category, objectType, objectName |
        ForEach-Object {
            [PSCustomObject]@{
                RelativePath       = $_.relativePath
                Category           = $_.category
                ObjectType         = $_.objectType
                ObjectName         = $_.objectName
                ParameterCount     = @($_.parameters).Count
                ReferenceCount     = @($_.referencedObjects).Count
                WriteTargetCount   = @($_.insertTargets).Count + @($_.updateTargets).Count + @($_.deleteTargets).Count
                CalledRoutineCount = @($_.calledRoutines).Count
                CodebaseCallCount  = if ($null -ne $_.codebaseReferenceCount) { $_.codebaseReferenceCount } else { 0 }
            }
        }
    )
}

function Reset-DeploymentProgressUi {
    $schemaProgress.Value = 0
    $migrationProgress.Value = 0
    $viewProgress.Value = 0
    $storedProcProgress.Value = 0
    $triggerProgress.Value = 0
    $seedDataProgress.Value = 0

    $schemaCount.Text = '0/0'
    $migrationCount.Text = '0/0'
    $viewCount.Text = '0/0'
    $storedProcCount.Text = '0/0'
    $triggerCount.Text = '0/0'
    $seedDataCount.Text = '0/0'

    $currentFileText.Text = ''
}

function Run-ObjectCatalog {
    $errorBorder.Visibility = 'Collapsed'
    $summaryBorder.Visibility = 'Collapsed'
    $catalogStatusText.Text = 'Building SQL object catalog...'
    $catalogPathText.Text = ''
    $catalogGeneratedText.Text = ''
    $catalogEntryCountText.Text = ''
    $catalogDataGrid.ItemsSource = $null

    try {
        $projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
        $sqlRoot = $script:DatabaseRoot

        if (-not (Test-Path $sqlRoot)) {
            throw "SQL deployment folder not found at: $sqlRoot"
        }

        $catalog = Save-SqlObjectCatalog -RepoRoot $projectRoot -DatabaseRoot $sqlRoot -OutputPath $script:CatalogPath

        $catalogPathText.Text = $script:CatalogPath
        $catalogGeneratedText.Text = ([DateTime]::Parse($catalog.generatedAt)).ToString('yyyy-MM-dd HH:mm:ss')
        $catalogEntryCountText.Text = "$($catalog.entries.Count) entry(ies)"
        $catalogDataGrid.ItemsSource = @(Convert-CatalogToDisplayRows -Catalog $catalog)
        $catalogStatusText.Text = 'Catalog generated successfully.'
        $deployButton.Content = 'Refresh Catalog'
    }
    catch {
        $catalogStatusText.Text = 'Catalog generation failed.'
        $errorBorder.Visibility = 'Visible'
        $errorText.Text = "Catalog error:`n`n$($_.Exception.Message)"
    }
}

function Set-UIMode {
    param(
        [ValidateSet('deployment', 'catalog')]
        [string]$Mode
    )

    $script:CurrentMode = $Mode

    if ($Mode -eq 'catalog') {
        $deploymentPanel.Visibility = 'Collapsed'
        $catalogPanel.Visibility = 'Visible'
        $summaryBorder.Visibility = 'Collapsed'
        $modeSwitchButton.Content = 'Deployment Mode'
        $deployButton.Content = 'Refresh Catalog'
        Run-ObjectCatalog
    }
    else {
        $catalogPanel.Visibility = 'Collapsed'
        $deploymentPanel.Visibility = 'Visible'
        $modeSwitchButton.Content = 'Object Catalog Mode'
        $deployButton.Content = 'Start Deployment'
        $errorBorder.Visibility = 'Collapsed'
    }
}

Set-ProviderState -Provider (Get-InitialProvider)

$deployButton.Add_Click({
        if ($script:CurrentMode -eq 'catalog') {
            Run-ObjectCatalog
            return
        }

        try {
            $errorBorder.Visibility = "Collapsed"
            $summaryBorder.Visibility = "Collapsed"
            Reset-DeploymentProgressUi
            $deployButton.IsEnabled = $false
            $overallStatusText.Text = "Deploying database..."

            $script:ProjectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

            if (-not (Test-Path $script:DatabaseRoot)) {
                throw "SQL deployment folder not found at: $script:DatabaseRoot"
            }

            $script:SchemaCountTotal = 0
            $script:MigrationCountTotal = 0
            $script:ViewCountTotal = 0
            $script:StoredProcCountTotal = 0
            $script:TriggerCountTotal = 0
            $script:SeedDataCountTotal = 0

            $script:timer = New-Object System.Windows.Threading.DispatcherTimer
            $script:timer.Interval = [TimeSpan]::FromMilliseconds(1)

            $script:step = 0
            $script:currentFiles = @()
            $script:currentIndex = 0

            $script:timer.Add_Tick({
                    try {
                        Write-Host "DEBUG: Timer tick - Step $script:step" -ForegroundColor Green

                        if ($script:step -eq 0) {
                            Write-Host "DEBUG: Dropping and recreating database: $Database" -ForegroundColor Yellow
                            $overallStatusText.Text = "Recreating database..."
                            $currentFileText.Text = "Dropping existing database..."
                            Execute-SqlCommand -SqlCommand "DROP DATABASE IF EXISTS ``$Database``;" -NoDatabase
                            $currentFileText.Text = "Creating new database..."
                            Execute-SqlCommand -SqlCommand "CREATE DATABASE ``$Database`` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;" -NoDatabase
                            Write-Host "DEBUG: Database recreated successfully" -ForegroundColor Green
                            $script:step = 1
                        }
                        elseif ($script:step -eq 1) {
                            $schemasPath = Join-Path $script:DatabaseRoot "Schemas"
                            Write-Host "DEBUG: Checking schemas path: $schemasPath" -ForegroundColor Yellow
                            $overallStatusText.Text = "Deploying schemas..."
                            $script:currentFiles = @(Get-SqlFiles -BasePath $schemasPath -ExcludedPatterns $script:ExcludePatterns | Sort-Object Name)
                            Write-Host "DEBUG: Found $($script:currentFiles.Count) schema files" -ForegroundColor Yellow
                            $script:currentIndex = 0
                            $script:step = if ($script:currentFiles.Count -eq 0) { 3 } else { 2 }
                        }
                        elseif ($script:step -eq 2) {
                            if ($script:currentIndex -lt $script:currentFiles.Count) {
                                $file = $script:currentFiles[$script:currentIndex]
                                $pct = [int]((($script:currentIndex + 1) / $script:currentFiles.Count) * 100)
                                Write-Host "DEBUG: Deploying schema: $($file.Name)" -ForegroundColor Yellow
                                $schemaProgress.Value = $pct
                                $schemaCount.Text = "$($script:currentIndex + 1)/$($script:currentFiles.Count)"
                                $currentFileText.Text = "Deploying: $($file.Name)"
                                Execute-SqlFile -FilePath $file.FullName -DisableForeignKeyChecks
                                $script:SchemaCountTotal++
                                $script:currentIndex++
                            }
                            else {
                                Write-Host "DEBUG: Schema deployment complete, moving to step 3" -ForegroundColor Green
                                $script:currentFiles = @()
                                $script:currentIndex = 0
                                $script:step = 3
                            }
                        }
                        elseif ($script:step -eq 3) {
                            $migPath = Join-Path $script:DatabaseRoot "Migrations"
                            Write-Host "DEBUG: Checking migrations path: $migPath" -ForegroundColor Yellow
                            $overallStatusText.Text = "Applying migrations..."
                            $script:currentFiles = @(Get-SqlFiles -BasePath $migPath -ExcludedPatterns $script:ExcludePatterns | Sort-Object Name)
                            Write-Host "DEBUG: Found $($script:currentFiles.Count) migration files" -ForegroundColor Yellow
                            $script:currentIndex = 0
                            $script:step = if ($script:currentFiles.Count -eq 0) { 5 } else { 4 }
                        }
                        elseif ($script:step -eq 4) {
                            if ($script:currentIndex -lt $script:currentFiles.Count) {
                                $file = $script:currentFiles[$script:currentIndex]
                                $pct = [int]((($script:currentIndex + 1) / $script:currentFiles.Count) * 100)
                                Write-Host "DEBUG: Deploying migration: $($file.Name)" -ForegroundColor Yellow
                                $migrationProgress.Value = $pct
                                $migrationCount.Text = "$($script:currentIndex + 1)/$($script:currentFiles.Count)"
                                $currentFileText.Text = "Deploying: $($file.Name)"
                                try {
                                    Execute-SqlFile -FilePath $file.FullName -DisableForeignKeyChecks
                                    $script:MigrationCountTotal++
                                }
                                catch {
                                    Write-Host "DEBUG: Migration warning (continuing): $($_.Exception.Message)" -ForegroundColor Yellow
                                }
                                $script:currentIndex++
                            }
                            else {
                                Write-Host "DEBUG: Migration deployment complete, moving to step 5" -ForegroundColor Green
                                $script:currentFiles = @()
                                $script:currentIndex = 0
                                $script:step = 5
                            }
                        }
                        elseif ($script:step -eq 5) {
                            $viewPath = Join-Path $script:DatabaseRoot "Views"
                            Write-Host "DEBUG: Checking views path: $viewPath" -ForegroundColor Yellow
                            $overallStatusText.Text = "Deploying views..."
                            $script:currentFiles = @(Get-SqlFiles -BasePath $viewPath -ExcludedPatterns $script:ExcludePatterns | Sort-Object Name)
                            Write-Host "DEBUG: Found $($script:currentFiles.Count) view files" -ForegroundColor Yellow
                            $script:currentIndex = 0
                            $script:step = if ($script:currentFiles.Count -eq 0) { 7 } else { 6 }
                        }
                        elseif ($script:step -eq 6) {
                            if ($script:currentIndex -lt $script:currentFiles.Count) {
                                $file = $script:currentFiles[$script:currentIndex]
                                $pct = [int]((($script:currentIndex + 1) / $script:currentFiles.Count) * 100)
                                Write-Host "DEBUG: Deploying view: $($file.Name)" -ForegroundColor Yellow
                                $viewProgress.Value = $pct
                                $viewCount.Text = "$($script:currentIndex + 1)/$($script:currentFiles.Count)"
                                $currentFileText.Text = "Deploying: $($file.Name)"
                                Execute-SqlFile -FilePath $file.FullName
                                $script:ViewCountTotal++
                                $script:currentIndex++
                            }
                            else {
                                Write-Host "DEBUG: View deployment complete, moving to step 7" -ForegroundColor Green
                                $script:currentFiles = @()
                                $script:currentIndex = 0
                                $script:step = 7
                            }
                        }
                        elseif ($script:step -eq 7) {
                            $spPath = Join-Path $script:DatabaseRoot "StoredProcedures"
                            Write-Host "DEBUG: Checking stored procedures path: $spPath" -ForegroundColor Yellow
                            $overallStatusText.Text = "Deploying stored procedures..."
                            $script:currentFiles = @(Get-SqlFiles -BasePath $spPath -ExcludedPatterns $script:ExcludePatterns -Recurse | Sort-Object FullName)
                            Write-Host "DEBUG: Found $($script:currentFiles.Count) stored procedure files" -ForegroundColor Yellow
                            $script:currentIndex = 0
                            $script:step = if ($script:currentFiles.Count -eq 0) { 9 } else { 8 }
                        }
                        elseif ($script:step -eq 8) {
                            if ($script:currentIndex -lt $script:currentFiles.Count) {
                                $file = $script:currentFiles[$script:currentIndex]
                                $pct = [int]((($script:currentIndex + 1) / $script:currentFiles.Count) * 100)
                                Write-Host "DEBUG: Deploying SP: $($file.Name)" -ForegroundColor Yellow
                                $storedProcProgress.Value = $pct
                                $storedProcCount.Text = "$($script:currentIndex + 1)/$($script:currentFiles.Count)"
                                $currentFileText.Text = "Deploying: $($file.Name)"
                                Execute-SqlFile -FilePath $file.FullName
                                $script:StoredProcCountTotal++
                                $script:currentIndex++
                            }
                            else {
                                Write-Host "DEBUG: Stored procedure deployment complete, moving to step 9" -ForegroundColor Green
                                $script:currentFiles = @()
                                $script:currentIndex = 0
                                $script:step = 9
                            }
                        }
                        elseif ($script:step -eq 9) {
                            $triggerPath = Join-Path $script:DatabaseRoot "Triggers"
                            Write-Host "DEBUG: Checking triggers path: $triggerPath" -ForegroundColor Yellow
                            $overallStatusText.Text = "Deploying triggers..."
                            $script:currentFiles = @(Get-SqlFiles -BasePath $triggerPath -ExcludedPatterns $script:ExcludePatterns | Sort-Object Name)
                            Write-Host "DEBUG: Found $($script:currentFiles.Count) trigger files" -ForegroundColor Yellow
                            $script:currentIndex = 0
                            $script:step = if ($script:currentFiles.Count -eq 0) { 11 } else { 10 }
                        }
                        elseif ($script:step -eq 10) {
                            if ($script:currentIndex -lt $script:currentFiles.Count) {
                                $file = $script:currentFiles[$script:currentIndex]
                                $pct = [int]((($script:currentIndex + 1) / $script:currentFiles.Count) * 100)
                                Write-Host "DEBUG: Deploying trigger: $($file.Name)" -ForegroundColor Yellow
                                $triggerProgress.Value = $pct
                                $triggerCount.Text = "$($script:currentIndex + 1)/$($script:currentFiles.Count)"
                                $currentFileText.Text = "Deploying: $($file.Name)"
                                Execute-SqlFile -FilePath $file.FullName
                                $script:TriggerCountTotal++
                                $script:currentIndex++
                            }
                            else {
                                Write-Host "DEBUG: Trigger deployment complete, moving to step 11" -ForegroundColor Green
                                $script:currentFiles = @()
                                $script:currentIndex = 0
                                $script:step = 11
                            }
                        }
                        elseif ($script:step -eq 11) {
                            $tdPath = Join-Path $script:DatabaseRoot "SeedData"
                            Write-Host "DEBUG: Checking seed data path: $tdPath" -ForegroundColor Yellow
                            $overallStatusText.Text = "Loading seed data..."
                            $script:currentFiles = @(Get-SqlFiles -BasePath $tdPath -ExcludedPatterns $script:ExcludePatterns | Sort-Object Name)
                            Write-Host "DEBUG: Found $($script:currentFiles.Count) seed data files" -ForegroundColor Yellow
                            $script:currentIndex = 0
                            $script:step = if ($script:currentFiles.Count -eq 0) { 13 } else { 12 }
                        }
                        elseif ($script:step -eq 12) {
                            if ($script:currentIndex -lt $script:currentFiles.Count) {
                                $file = $script:currentFiles[$script:currentIndex]
                                $pct = [int]((($script:currentIndex + 1) / $script:currentFiles.Count) * 100)
                                Write-Host "DEBUG: Deploying seed data: $($file.Name)" -ForegroundColor Yellow
                                $seedDataProgress.Value = $pct
                                $seedDataCount.Text = "$($script:currentIndex + 1)/$($script:currentFiles.Count)"
                                $currentFileText.Text = "Deploying: $($file.Name)"
                                try {
                                    Execute-SqlFile -FilePath $file.FullName
                                    $script:SeedDataCountTotal++
                                }
                                catch {
                                    Write-Host "DEBUG: Seed data warning (continuing): $($_.Exception.Message)" -ForegroundColor Yellow
                                }
                                $script:currentIndex++
                            }
                            else {
                                $script:step = 13
                            }
                        }
                        elseif ($script:step -eq 13) {
                            Write-Host "DEBUG: Building SQL object catalog and validation report" -ForegroundColor Green
                            $overallStatusText.Text = "Validating deployed SQL objects..."
                            $currentFileText.Text = "Refreshing SQL object catalog..."

                            $reportPaths = New-ValidationOutputPaths -OutputsDirectory $script:ValidationOutputsPath

                            try {
                                $catalog = Save-SqlObjectCatalog -RepoRoot $script:ProjectRoot -DatabaseRoot $script:DatabaseRoot -OutputPath $script:CatalogPath
                                $currentFileText.Text = "Writing validation report..."
                                $validationResult = Test-SqlObjectCatalogAgainstDatabase -Catalog $catalog -MySqlExe (Find-MySqlExe) -Server $Server -Port $Port -Database $Database -User $User -Password $Password
                                Save-SqlValidationReport -ValidationResult $validationResult -MarkdownPath $reportPaths.markdown -JsonPath $reportPaths.json
                            }
                            catch {
                                $validationResult = [ordered]@{
                                    generatedAt = (Get-Date).ToString('o')
                                    database    = $Database
                                    resultCount = 1
                                    results     = @(
                                        [ordered]@{
                                            relativePath = 'validation'
                                            objectType   = 'validation'
                                            objectName   = 'catalog'
                                            issueCount   = 1
                                            issues       = @(
                                                [ordered]@{
                                                    severity = 'error'
                                                    code     = 'VALIDATION_FAILURE'
                                                    message  = $_.Exception.Message
                                                }
                                            )
                                        }
                                    )
                                }
                                Save-SqlValidationReport -ValidationResult $validationResult -MarkdownPath $reportPaths.markdown -JsonPath $reportPaths.json
                            }

                            Write-Host "DEBUG: Deployment complete!" -ForegroundColor Green
                            $script:timer.Stop()
                            $overallStatusText.Text = "Deployment completed successfully!"
                            $currentFileText.Text = "Validation report: $($reportPaths.markdown)"
                            $summaryBorder.Visibility = "Visible"
                            $summarySchemas.Text = "$($script:SchemaCountTotal) file(s)"
                            $summaryMigrations.Text = "$($script:MigrationCountTotal) file(s)"
                            $summaryViews.Text = "$($script:ViewCountTotal) file(s)"
                            $summaryStoredProcs.Text = "$($script:StoredProcCountTotal) procedure(s)"
                            $summaryTriggers.Text = "$($script:TriggerCountTotal) file(s)"
                            $summarySeedData.Text = "$($script:SeedDataCountTotal) file(s)"
                            $deployButton.Content = "Deploy Again"
                            $deployButton.IsEnabled = $true
                        }
                    }
                    catch {
                        Write-Host "DEBUG: Timer tick error!" -ForegroundColor Red
                        Write-Host "DEBUG: Error message: $($_.Exception.Message)" -ForegroundColor Red
                        Write-Host "DEBUG: Stack trace: $($_.ScriptStackTrace)" -ForegroundColor Red
                        $script:timer.Stop()
                        $overallStatusText.Text = "Deployment failed!"
                        $errorBorder.Visibility = "Visible"
                        $errorText.Text = "Error in step $($script:step)`n`nMessage: $($_.Exception.Message)`n`nFile: $($file.FullName)`n`nStack:`n$($_.ScriptStackTrace)"
                        $deployButton.Content = "Retry"
                        $deployButton.IsEnabled = $true
                    }
                })

            Write-Host "DEBUG: PSScriptRoot = $PSScriptRoot" -ForegroundColor Cyan
            Write-Host "DEBUG: ProjectRoot = $script:ProjectRoot" -ForegroundColor Cyan
            Write-Host "DEBUG: DatabaseRoot = $script:DatabaseRoot" -ForegroundColor Cyan
            Write-Host "DEBUG: DatabaseRoot exists = $(Test-Path $script:DatabaseRoot)" -ForegroundColor Cyan
            Write-Host "DEBUG: Starting timer..." -ForegroundColor Cyan
            $script:timer.Start()
            Write-Host "DEBUG: Timer started successfully" -ForegroundColor Cyan
        }
        catch {
            Write-Host "CRITICAL ERROR in click handler:" -ForegroundColor Red
            Write-Host "Message: $($_.Exception.Message)" -ForegroundColor Red
            Write-Host "Stack: $($_.ScriptStackTrace)" -ForegroundColor Red
            $overallStatusText.Text = "Deployment failed!"
            $errorBorder.Visibility = "Visible"
            $errorText.Text = "Setup error: $($_.Exception.Message)`n`n$($_.ScriptStackTrace)"
            $deployButton.IsEnabled = $true
        }
    })

$providerToggleButton.Add_Click({
        $targetProvider = if ($providerToggleButton.IsChecked) { 'mamp' } else { 'workbench' }
        $previousProvider = $script:CurrentProvider

        try {
            $errorBorder.Visibility = 'Collapsed'
            $providerToggleButton.IsEnabled = $false
            $deployButton.IsEnabled = $false
            $modeSwitchButton.IsEnabled = $false
            Switch-ProviderEnvironment -TargetProvider $targetProvider
        }
        catch {
            Set-ProviderState -Provider $previousProvider
            $errorBorder.Visibility = 'Visible'
            $errorText.Text = "Provider switch error:`n`n$($_.Exception.Message)"
            $overallStatusText.Text = 'Provider switch failed.'
        }
        finally {
            Hide-ProviderSwitchProgress
            $providerToggleButton.IsEnabled = $true
            $deployButton.IsEnabled = $true
            $modeSwitchButton.IsEnabled = $true
        }
    })

$modeSwitchButton.Add_Click({
        if ($script:CurrentMode -eq 'deployment') {
            Set-UIMode -Mode 'catalog'
        }
        else {
            Set-UIMode -Mode 'deployment'
        }
    })

$closeButton.Add_Click({ $window.Close() })

$window.ShowDialog() | Out-Null
