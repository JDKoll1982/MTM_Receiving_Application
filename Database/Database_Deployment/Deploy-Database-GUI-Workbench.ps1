# MTM Receiving Application — Database Deployment (MySQL Workbench / MySQL Server)
# Targets the standard MySQL Server installation. A MySQL 5.7 client does not
# reliably process DELIMITER directives in batch mode, so delimiter-based scripts
# are normalized before execution when an older client is detected.
#
# LEGACY: the supported deployment tool is now the WinUI app in MTM_Database_Deployment/.
# This script is kept for reference and for scripted/no-UI runs only. It deploys the same
# Sql_Files tree, so the two tools stay in sync as long as the tree stays idempotent.

param(
    [string]$Server = "172.16.1.104",
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
    }
    Connection = [ordered]@{
        # Used when the configured server is unreachable so the tool still works on this PC.
        FallbackServer            = '127.0.0.1'
        ProbeTimeoutMilliseconds  = 2000
        ClientConnectTimeoutSeconds = 5
    }
    # AUTH-SECRET-LOGIC-BEGIN
    Security  = [ordered]@{
        EnvironmentVariableName = 'MTM_AUTH_USER_SECRET_KEY'
        SharedServerIp          = '172.16.1.104'
        LocalSecretDirectory    = Join-Path $env:ProgramData 'MTM Receiving Application\Security'
        LocalSecretFileName     = 'MTM_AUTH_USER_SECRET_KEY.txt'
        SharedSecretDirectory   = '\\MTMANU-FS01\Expo Drive\Software Development\Live Applications\MTM_Application_Keys'
        SharedSecretFileName    = 'MTM_AUTH_USER_SECRET_KEY.txt'
    }
    # AUTH-SECRET-LOGIC-END
    HostSwap  = [ordered]@{
        ShowButton          = $true
        localValue          = '172.16.1.104'
        SharedHostValue     = '172.16.1.104'
        ExcludedDirectories = @('.git', '.vs', 'bin', 'obj', 'TestResults')
        IncludedExtensions  = @(
            '.bat', '.cmd', '.config', '.cs', '.csproj', '.css', '.fs', '.go', '.htm', '.html',
            '.ini', '.java', '.js', '.json', '.jsx', '.md', '.mmd', '.props', '.ps1', '.psd1',
            '.psm1', '.py', '.razor', '.rb', '.rs', '.scss', '.sh', '.skill', '.sln', '.slnx',
            '.sql', '.targets', '.toml', '.ts', '.tsx', '.txt', '.vb', '.xml', '.xaml', '.yaml', '.yml'
        )
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
                        <TextBlock Name="ProviderValueText" Text="MySQL Server" Foreground="#1565C0" FontWeight="SemiBold"/>
                    </Border>
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
                <TextBlock Name="SummaryTitleText" Text="Deployment Successful!" FontSize="16" FontWeight="Bold"
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

        <Border Grid.Row="4" Name="OperationProgressBorder" Background="#E3F2FD" BorderBrush="#64B5F6"
                BorderThickness="1" CornerRadius="5" Padding="12" Margin="0,16,0,0" Visibility="Collapsed">
            <StackPanel>
                <TextBlock Name="OperationProgressStatusText" Text="Working..."
                           FontWeight="SemiBold" Foreground="#1565C0" Margin="0,0,0,8"/>
                <ProgressBar Name="OperationProgressBar" Height="10" Minimum="0" Maximum="100" Value="0"/>
            </StackPanel>
        </Border>

        <Grid Grid.Row="5" Margin="0,20,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <CheckBox Grid.Column="0"
                      Name="RunValidationCheckBox"
                      VerticalAlignment="Center"
                      IsChecked="False"
                      Margin="0,0,16,0">
                <TextBlock Text="Run SQL object catalog and validation report after deployment"
                           TextWrapping="Wrap"
                           MaxWidth="360"
                           Foreground="#333"/>
            </CheckBox>

            <StackPanel Grid.Column="1" Orientation="Horizontal" HorizontalAlignment="Right">
                <Button Name="SwapHostsButton" Content="Swap Host References"
                    Width="180" Height="35" Margin="0,0,10,0"
                    Background="#6A1B9A" Foreground="White"
                    BorderThickness="0" FontWeight="Bold" Cursor="Hand"/>
                <Button Name="ModeSwitchButton" Content="Object Catalog Mode"
                    Width="170" Height="35" Margin="0,0,10,0"
                    Background="#37474F" Foreground="White"
                    BorderThickness="0" FontWeight="Bold" Cursor="Hand"/>
                <Button Name="DeployButton" Content="Start Deployment"
                        Width="140" Height="35"
                        Background="#2196F3" Foreground="White"
                        BorderThickness="0" FontWeight="Bold" Cursor="Hand"/>
                <Button Name="MigrateButton" Content="Migrate Existing"
                    Width="140" Height="35" Margin="10,0,0,0"
                    Background="#2E7D32" Foreground="White"
                    BorderThickness="0" FontWeight="Bold" Cursor="Hand"/>
                <Button Name="CloseButton" Content="Close"
                    Width="100" Height="35" Margin="10,0,0,0"
                        Background="#9E9E9E" Foreground="White"
                        BorderThickness="0" Cursor="Hand"/>
            </StackPanel>
        </Grid>
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
$summaryTitleText = $window.FindName("SummaryTitleText")
$summarySchemas = $window.FindName("SummarySchemas")
$summaryMigrations = $window.FindName("SummaryMigrations")
$summaryViews = $window.FindName("SummaryViews")
$summaryStoredProcs = $window.FindName("SummaryStoredProcs")
$summaryTriggers = $window.FindName("SummaryTriggers")
$summarySeedData = $window.FindName("SummarySeedData")
$operationProgressBorder = $window.FindName("OperationProgressBorder")
$operationProgressStatusText = $window.FindName("OperationProgressStatusText")
$operationProgressBar = $window.FindName("OperationProgressBar")
$deploymentPanel = $window.FindName("DeploymentPanel")
$catalogPanel = $window.FindName("CatalogPanel")
$catalogStatusText = $window.FindName("CatalogStatusText")
$catalogPathText = $window.FindName("CatalogPathText")
$catalogGeneratedText = $window.FindName("CatalogGeneratedText")
$catalogEntryCountText = $window.FindName("CatalogEntryCountText")
$catalogDataGrid = $window.FindName("CatalogDataGrid")
$runValidationCheckBox = $window.FindName("RunValidationCheckBox")
$swapHostsButton = $window.FindName("SwapHostsButton")
$modeSwitchButton = $window.FindName("ModeSwitchButton")
$deployButton = $window.FindName("DeployButton")
$migrateButton = $window.FindName("MigrateButton")
$closeButton = $window.FindName("CloseButton")

if ($null -ne $swapHostsButton) {
    $swapHostsButton.Visibility = if ($script:Config.HostSwap.ShowButton) { 'Visible' } else { 'Collapsed' }
}

$script:WorkbenchProfile = [ordered]@{
    Server   = $Server
    Port     = $Port
    User     = $User
    Password = $Password
}

$script:CurrentMode = 'deployment'
$script:PreferredServer = $script:WorkbenchProfile.Server
$script:CurrentServer = $script:WorkbenchProfile.Server
$script:CurrentPort = $script:WorkbenchProfile.Port
$script:CurrentUser = $script:WorkbenchProfile.User
$script:CurrentPassword = $script:WorkbenchProfile.Password
$script:DatabaseRoot = Join-Path $PSScriptRoot $script:Config.Paths.DatabaseRootFolder
$script:CatalogPath = Join-Path $PSScriptRoot $script:Config.Paths.CatalogFileName
$script:ValidationOutputsPath = Join-Path $PSScriptRoot $script:Config.Paths.OutputsFolder
$script:ExcludePatterns = $script:Config.Sql.ExcludePatterns
$script:MySqlClientInfoCache = @{}
$script:MySqlExePath = $null
$script:SharedMySqlDefaultsFile = $null
$script:SharedMySqlDefaultsFileKey = $null

function Sync-UiRender {
    $window.Dispatcher.Invoke([System.Action] {}, [System.Windows.Threading.DispatcherPriority]::Render)
}

function Show-OperationProgress {
    param(
        [string]$StatusText,
        [double]$ProgressValue
    )

    $operationProgressBorder.Visibility = 'Visible'
    $operationProgressStatusText.Text = $StatusText
    $operationProgressBar.Value = [Math]::Max(0, [Math]::Min(100, $ProgressValue))
    Sync-UiRender
}

function Hide-OperationProgress {
    $operationProgressBar.Value = 0
    $operationProgressStatusText.Text = 'Working...'
    $operationProgressBorder.Visibility = 'Collapsed'
    Sync-UiRender
}

function Show-HostSwapProgress {
    param(
        [int]$CurrentFileIndex,
        [int]$TotalFileCount,
        [string]$CurrentFilePath
    )

    $safeTotalFileCount = [Math]::Max(1, $TotalFileCount)
    $currentProgress = if ($TotalFileCount -le 0) {
        100
    }
    else {
        [Math]::Round(($CurrentFileIndex / $safeTotalFileCount) * 100, 2)
    }

    Show-OperationProgress -StatusText "Swapping host references ($CurrentFileIndex/$TotalFileCount)..." -ProgressValue $currentProgress
    $currentFileText.Text = "Scanning file $CurrentFileIndex of ${TotalFileCount}: $CurrentFilePath"
    Sync-UiRender
}

function Update-ConnectionDisplay {
    $serverText.Text = "$($script:CurrentServer)`:$($script:CurrentPort)"
    $databaseText.Text = $Database
    $userText.Text = $script:CurrentUser

    if ($null -ne $swapHostsButton) {
        $swapTarget = if (Test-IslocalValue -HostName $script:CurrentServer) {
            $script:Config.HostSwap.SharedHostValue
        }
        else {
            $script:Config.HostSwap.localValue
        }

        $swapHostsButton.Content = "Swap Repo To $swapTarget"
    }
}

function Get-MySqlExecutionCacheKey {
    return "$($script:CurrentServer)|$($script:CurrentPort)|$($script:CurrentUser)|$Database"
}

function Clear-SharedMySqlDefaultsFile {
    if ($null -ne $script:SharedMySqlDefaultsFile -and (Test-Path $script:SharedMySqlDefaultsFile -ErrorAction SilentlyContinue)) {
        Remove-Item $script:SharedMySqlDefaultsFile -ErrorAction SilentlyContinue
    }

    $script:SharedMySqlDefaultsFile = $null
    $script:SharedMySqlDefaultsFileKey = $null
}

# AUTH-SECRET-LOGIC-BEGIN
function Test-IslocalValue {
    param(
        [string]$HostName
    )

    if ([string]::IsNullOrWhiteSpace($HostName)) {
        return $false
    }

    return $HostName.Trim().ToLowerInvariant() -in @('172.16.1.104', '127.0.0.1', '::1')
}

function New-AuthSecretValue {
    $bytes = New-Object byte[] 32
    $randomNumberGenerator = [System.Security.Cryptography.RandomNumberGenerator]::Create()

    try {
        $randomNumberGenerator.GetBytes($bytes)
    }
    finally {
        $randomNumberGenerator.Dispose()
    }

    return [Convert]::ToBase64String($bytes)
}

function Get-AuthSecretSourcePath {
    $securityConfig = $script:Config.Security

    if (Test-IslocalValue -HostName $script:CurrentServer) {
        return Join-Path $securityConfig.LocalSecretDirectory $securityConfig.LocalSecretFileName
    }

    return Join-Path $securityConfig.SharedSecretDirectory $securityConfig.SharedSecretFileName
}

function Get-OrCreate-AuthSecretValue {
    $secretPath = Get-AuthSecretSourcePath
    $secretDirectory = Split-Path -Parent $secretPath

    if (-not (Test-Path $secretDirectory)) {
        New-Item -Path $secretDirectory -ItemType Directory -Force | Out-Null
    }

    if (Test-Path $secretPath) {
        $existingSecret = (Get-Content -Path $secretPath -Raw -ErrorAction Stop).Trim()
        if (-not [string]::IsNullOrWhiteSpace($existingSecret)) {
            return [PSCustomObject]@{
                SecretValue = $existingSecret
                SecretPath  = $secretPath
                WasCreated  = $false
            }
        }
    }

    $newSecret = New-AuthSecretValue
    [System.IO.File]::WriteAllText($secretPath, $newSecret, [System.Text.UTF8Encoding]::new($false))

    return [PSCustomObject]@{
        SecretValue = $newSecret
        SecretPath  = $secretPath
        WasCreated  = $true
    }
}

function Set-AuthSecretEnvironmentVariable {
    param(
        [string]$SecretValue
    )

    $variableName = $script:Config.Security.EnvironmentVariableName
    [Environment]::SetEnvironmentVariable($variableName, $SecretValue, 'Process')

    $machineEnvironmentPath = 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Environment'
    $userEnvironmentPath = 'HKCU:\Environment'

    try {
        Set-ItemProperty -Path $machineEnvironmentPath -Name $variableName -Value $SecretValue -Type String -ErrorAction Stop
        return 'Machine'
    }
    catch {
        if (-not (Test-Path $userEnvironmentPath)) {
            New-Item -Path $userEnvironmentPath -Force | Out-Null
        }

        Set-ItemProperty -Path $userEnvironmentPath -Name $variableName -Value $SecretValue -Type String -ErrorAction Stop
        return 'User'
    }
}

function Ensure-AuthSecretConfigured {
    $secretDetails = Get-OrCreate-AuthSecretValue
    $scope = Set-AuthSecretEnvironmentVariable -SecretValue $secretDetails.SecretValue

    $currentFileText.Text = "Auth secret source: $($secretDetails.SecretPath)"

    if ($secretDetails.WasCreated) {
        Write-Host "Created auth secret file at $($secretDetails.SecretPath) and set environment variable scope $scope." -ForegroundColor Green
    }
    else {
        Write-Host "Loaded auth secret from $($secretDetails.SecretPath) and set environment variable scope $scope." -ForegroundColor Cyan
    }

    return [PSCustomObject]@{
        SecretPath = $secretDetails.SecretPath
        Scope      = $scope
        WasCreated = $secretDetails.WasCreated
    }
}
# AUTH-SECRET-LOGIC-END

function Protect-AuthSecretLogicBlocks {
    param(
        [string]$Content
    )

    $protectedBlocks = [ordered]@{}
    $updatedContent = $Content
    $protectedBlockList = [regex]::Matches($Content, '(?s)# AUTH-SECRET-LOGIC-BEGIN.*?# AUTH-SECRET-LOGIC-END')

    for ($index = 0; $index -lt $protectedBlockList.Count; $index++) {
        $token = "__MTM_AUTH_SECRET_BLOCK_$($index)__"
        $protectedBlocks[$token] = $protectedBlockList[$index].Value
        $updatedContent = $updatedContent.Replace($protectedBlockList[$index].Value, $token)
    }

    return [PSCustomObject]@{
        Content = $updatedContent
        Blocks  = $protectedBlocks
    }
}

function Restore-AuthSecretLogicBlocks {
    param(
        [string]$Content,
        [System.Collections.IDictionary]$Blocks
    )

    $restoredContent = $Content
    foreach ($entry in $Blocks.GetEnumerator()) {
        $restoredContent = $restoredContent.Replace($entry.Key, $entry.Value)
    }

    return $restoredContent
}

function Convert-HostReferenceText {
    param(
        [string]$Content,
        [string]$FromHost,
        [string]$ToHost
    )

    $placeholder = '172.16.1.104'
    return $Content.Replace($FromHost, $placeholder).Replace($ToHost, $FromHost).Replace($placeholder, $ToHost)
}

function Get-RepoFilesForHostSwap {
    param(
        [string]$RepoRoot
    )

    $includedExtensions = $script:Config.HostSwap.IncludedExtensions
    $excludedDirectories = $script:Config.HostSwap.ExcludedDirectories

    $candidateFiles = @()

    try {
        $gitFileList = git -C $RepoRoot ls-files --cached --others --exclude-standard 2>$null

        if ($LASTEXITCODE -eq 0 -and $null -ne $gitFileList) {
            $candidateFiles = @(
                $gitFileList |
                Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
                ForEach-Object {
                    $relativePath = $_.Trim()
                    $absolutePath = Join-Path $RepoRoot $relativePath

                    if (Test-Path $absolutePath -PathType Leaf) {
                        Get-Item -LiteralPath $absolutePath -ErrorAction SilentlyContinue
                    }
                } |
                Where-Object { $null -ne $_ }
            )
        }
    }
    catch {
    }

    if ($candidateFiles.Count -eq 0) {
        $candidateFiles = @(
            Get-ChildItem -Path $RepoRoot -Recurse -File -ErrorAction SilentlyContinue
        )
    }

    return @(
        $candidateFiles |
        Where-Object {
            if ($_.Extension.ToLowerInvariant() -notin $includedExtensions) {
                return $false
            }

            $relativePath = $_.FullName.Substring($RepoRoot.Length).TrimStart('\')
            $pathSegments = $relativePath.Split([System.IO.Path]::DirectorySeparatorChar, [System.StringSplitOptions]::RemoveEmptyEntries)
            return -not ($pathSegments | Where-Object { $_ -in $excludedDirectories })
        }
    )
}

function Update-HostProfilesAfterSwap {
    param(
        [string]$FromHost,
        [string]$ToHost
    )

    $profile = $script:WorkbenchProfile
    if ($profile['Server'] -eq $FromHost) {
        $profile['Server'] = $ToHost
    }
    elseif ($profile['Server'] -eq $ToHost) {
        $profile['Server'] = $FromHost
    }

    if ($script:CurrentServer -eq $FromHost) {
        $script:CurrentServer = $ToHost
    }
    elseif ($script:CurrentServer -eq $ToHost) {
        $script:CurrentServer = $FromHost
    }

    if ($script:PreferredServer -eq $FromHost) {
        $script:PreferredServer = $ToHost
    }
    elseif ($script:PreferredServer -eq $ToHost) {
        $script:PreferredServer = $FromHost
    }

    Update-ConnectionDisplay
}

function Invoke-RepoHostReferenceSwap {
    $repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
    $172.16.1.104 = $script:Config.HostSwap.localValue
    $sharedHost = $script:Config.HostSwap.SharedHostValue
    $fromHost = if (Test-IslocalValue -HostName $script:CurrentServer) { $172.16.1.104 } else { $sharedHost }
    $toHost = if ($fromHost -eq $172.16.1.104) { $sharedHost } else { $172.16.1.104 }
    $filesChanged = 0
    $repoFiles = @(Get-RepoFilesForHostSwap -RepoRoot $repoRoot)
    $totalFiles = $repoFiles.Count

    if ($totalFiles -eq 0) {
        Show-OperationProgress -StatusText 'No eligible repository files found for host swap.' -ProgressValue 100
    }

    for ($fileIndex = 0; $fileIndex -lt $totalFiles; $fileIndex++) {
        $file = $repoFiles[$fileIndex]
        $relativePath = $file.FullName.Substring($repoRoot.Length).TrimStart('\')
        Show-HostSwapProgress -CurrentFileIndex ($fileIndex + 1) -TotalFileCount $totalFiles -CurrentFilePath $relativePath

        $originalContent = [System.IO.File]::ReadAllText($file.FullName)
        $protectedContent = Protect-AuthSecretLogicBlocks -Content $originalContent
        $updatedContent = Convert-HostReferenceText -Content $protectedContent.Content -FromHost $fromHost -ToHost $toHost
        $updatedContent = Restore-AuthSecretLogicBlocks -Content $updatedContent -Blocks $protectedContent.Blocks

        if ($updatedContent -ne $originalContent) {
            [System.IO.File]::WriteAllText($file.FullName, $updatedContent, [System.Text.UTF8Encoding]::new($false))
            $filesChanged++
        }
    }

    Update-HostProfilesAfterSwap -FromHost $fromHost -ToHost $toHost

    Show-OperationProgress -StatusText "Host reference swap complete ($totalFiles/$totalFiles)." -ProgressValue 100

    return [PSCustomObject]@{
        FromHost     = $fromHost
        ToHost       = $toHost
        FilesChanged = $filesChanged
        FilesScanned = $totalFiles
        RepoRoot     = $repoRoot
    }
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
            Show-OperationProgress -StatusText $ProgressStatusText -ProgressValue $currentProgress
        }

        if ((Test-TcpPortOpen -HostName $HostName -Port $Port) -eq $ExpectedOpen) {
            if (-not [string]::IsNullOrWhiteSpace($ProgressStatusText)) {
                Show-OperationProgress -StatusText $ProgressStatusText -ProgressValue $ProgressEnd
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

    Show-OperationProgress -StatusText "Waiting for Windows elevation to $($Action.ToLowerInvariant()) Workbench MySQL service..." -ProgressValue 58

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
    Show-OperationProgress -StatusText 'Starting Workbench standalone MySQL server...' -ProgressValue 60

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
        Show-OperationProgress -StatusText 'Starting Workbench MySQL service...' -ProgressValue 60
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
        Show-OperationProgress -StatusText 'Stopping Workbench MySQL service...' -ProgressValue 18
        Invoke-WorkbenchServiceAction -Action 'Stop' -ServiceName $service.Name
    }

    if (-not (Wait-ForTcpPortState -HostName $script:WorkbenchProfile.Server -Port $workbenchPort -ExpectedOpen $false -TimeoutSeconds 30 -ProgressStatusText 'Waiting for Workbench MySQL to stop...' -ProgressStart 20 -ProgressEnd 42)) {
        throw "Workbench MySQL service '$($service.Name)' did not stop listening on port $workbenchPort within 30 seconds."
    }
}

# Picks the server to run against: the configured server when it answers,
# otherwise the local MySQL instance, so a missing VPN/remote server does not
# stall every script in the deployment.
function Resolve-MySqlServer {
    param(
        [int]$TimeoutMilliseconds = $script:Config.Connection.ProbeTimeoutMilliseconds
    )

    $port = [int]$script:CurrentPort
    $preferredServer = $script:PreferredServer
    $fallbackServer = $script:Config.Connection.FallbackServer

    if (Test-TcpPortOpen -HostName $preferredServer -Port $port -TimeoutMilliseconds $TimeoutMilliseconds) {
        if ($script:CurrentServer -ne $preferredServer) {
            $script:WorkbenchProfile['Server'] = $preferredServer
            $script:CurrentServer = $preferredServer
            Clear-SharedMySqlDefaultsFile
            Update-ConnectionDisplay
        }

        return [pscustomobject]@{
            Server          = $preferredServer
            UsedFallback    = $false
            StatusMessage   = "MySQL server $preferredServer`:$port is available."
        }
    }

    if ($preferredServer -eq $fallbackServer) {
        throw "MySQL server $preferredServer`:$port is not reachable. Start MySQL Server (or connect to the network) and retry."
    }

    if (-not (Test-TcpPortOpen -HostName $fallbackServer -Port $port -TimeoutMilliseconds $TimeoutMilliseconds)) {
        throw "Neither $preferredServer`:$port nor the local fallback $fallbackServer`:$port is reachable. Start MySQL Server and retry."
    }

    $script:WorkbenchProfile['Server'] = $fallbackServer
    $script:CurrentServer = $fallbackServer
    Clear-SharedMySqlDefaultsFile
    Update-ConnectionDisplay

    return [pscustomobject]@{
        Server        = $fallbackServer
        UsedFallback  = $true
        StatusMessage = "$preferredServer`:$port is unavailable. Using local MySQL server $fallbackServer`:$port."
    }
}

Update-ConnectionDisplay

try {
    $script:StartupServerStatus = (Resolve-MySqlServer).StatusMessage
    $currentFileText.Text = $script:StartupServerStatus
}
catch {
    $script:StartupServerStatus = $_.Exception.Message
    $currentFileText.Text = $script:StartupServerStatus
}

# Locate mysql.exe under the default MySQL Server install directory.
# Falls back to whatever is on the system PATH.
function Find-MySqlExe {
    if (-not [string]::IsNullOrWhiteSpace($script:MySqlExePath)) {
        return $script:MySqlExePath
    }

    $installRoot = $script:Config.Paths.WorkbenchInstallRoot
    if (Test-Path $installRoot) {
        $hit = Get-ChildItem $installRoot -Recurse -Filter "mysql.exe" -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match "\\bin\\mysql\.exe$" } |
        Sort-Object FullName -Descending |
        Select-Object -First 1
        if ($hit) {
            $script:MySqlExePath = $hit.FullName
            return $hit.FullName
        }
    }
    try {
        $null = Get-Command mysql -ErrorAction Stop
        $script:MySqlExePath = 'mysql'
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

function Normalize-MySqlLegacyRoutineDelimiterLayout {
    param(
        [string]$SqlText
    )

    if ([string]::IsNullOrWhiteSpace($SqlText)) {
        return $SqlText
    }

    $pattern = '(?m)^(?<dropIndent>\s*)DROP\s+(?<routineType>PROCEDURE|FUNCTION|TRIGGER)\s+IF\s+EXISTS\s+(?<routineName>.+?);\s*$\r?\n(?:\s*\r?\n)*(?<delimiterIndent>\s*)DELIMITER\s+(?<delimiter>\S+)\s*$'

    return [regex]::Replace(
        $SqlText,
        $pattern,
        {
            param($match)

            $newline = if ($match.Value.Contains("`r`n")) { "`r`n" } else { "`n" }
            $dropIndent = $match.Groups['dropIndent'].Value
            $delimiterIndent = $match.Groups['delimiterIndent'].Value
            $routineType = $match.Groups['routineType'].Value
            $routineName = $match.Groups['routineName'].Value.Trim()
            $delimiter = $match.Groups['delimiter'].Value

            return @(
                ($delimiterIndent + "DELIMITER $delimiter"),
                '',
                ($dropIndent + "DROP $routineType IF EXISTS $routineName$delimiter")
            ) -join $newline
        }
    )
}

function Test-RequiresLegacyBatchCompatibility {
    param(
        [hashtable]$ClientInfo
    )

    return $ClientInfo.majorVersion -gt 0 -and $ClientInfo.majorVersion -lt 8
}

function Invoke-MySqlBatchFile {
    param(
        [string]$Exe,
        [string]$DefaultsFile,
        [string]$TargetFile,
        [string]$StdoutFile,
        [string]$StderrFile,
        [string]$Delimiter = ';',
        [string]$DatabaseName
    )

    $targetDatabase = if (-not [string]::IsNullOrWhiteSpace($DatabaseName)) { $DatabaseName } else { $Database }
    $mysqlArgs = @(
        "--defaults-extra-file=$DefaultsFile",
        "-h$($script:CurrentServer)",
        "-P$($script:CurrentPort)",
        "--connect-timeout=$($script:Config.Connection.ClientConnectTimeoutSeconds)",
        '--default-character-set=utf8mb4',
        "--delimiter=$Delimiter",
        $targetDatabase
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

function Expand-MySqlSourceReferences {
    param(
        [string]$SqlText,
        [string]$BaseDirectory
    )

    $outputLines = New-Object System.Collections.Generic.List[string]

    foreach ($line in ($SqlText -split "`r?`n")) {
        if ($line -match '^\s*SOURCE\s+(.+?);\s*$') {
            $includePath = $matches[1].Trim().Trim('"').Trim("'")
            $resolvedPath = if ([System.IO.Path]::IsPathRooted($includePath)) {
                $includePath
            }
            else {
                Join-Path $BaseDirectory $includePath
            }

            if (-not (Test-Path $resolvedPath)) {
                throw "Included SQL file not found: $includePath"
            }

            $includedText = [System.IO.File]::ReadAllText($resolvedPath)
            $expandedText = Expand-MySqlSourceReferences -SqlText $includedText -BaseDirectory $BaseDirectory
            $trimmedExpandedText = $expandedText.TrimEnd("`r", "`n")

            if ($trimmedExpandedText.Length -gt 0) {
                $outputLines.Add($trimmedExpandedText)
            }
        }
        else {
            $outputLines.Add($line)
        }
    }

    return ($outputLines -join "`r`n")
}

# Rewrites a script so it can run against a database that already holds live data:
# destructive DDL is removed and object creation becomes repeatable.
function ConvertTo-MigrationSafeSqlText {
    param(
        [string]$SqlText
    )

    if ([string]::IsNullOrWhiteSpace($SqlText)) {
        return $SqlText
    }

    $migrationSafeText = $SqlText

    $destructivePatterns = @(
        '(?im)^[ \t]*DROP[ \t]+(?:TEMPORARY[ \t]+)?TABLE[ \t]+(?:IF[ \t]+EXISTS[ \t]+)?[^;]*;[ \t]*\r?\n?',
        '(?im)^[ \t]*DROP[ \t]+(?:DATABASE|SCHEMA)[ \t]+(?:IF[ \t]+EXISTS[ \t]+)?[^;]*;[ \t]*\r?\n?',
        '(?im)^[ \t]*TRUNCATE[ \t]+(?:TABLE[ \t]+)?[^;]*;[ \t]*\r?\n?',
        '(?im)^[ \t]*DELETE[ \t]+FROM[ \t]+[^;]*;[ \t]*\r?\n?'
    )

    foreach ($pattern in $destructivePatterns) {
        $migrationSafeText = [regex]::Replace($migrationSafeText, $pattern, [string]::Empty)
    }

    $migrationSafeText = [regex]::Replace(
        $migrationSafeText,
        '(?im)^(?<indent>[ \t]*)CREATE[ \t]+TABLE[ \t]+(?!IF[ \t]+NOT[ \t]+EXISTS)',
        '${indent}CREATE TABLE IF NOT EXISTS '
    )

    $migrationSafeText = [regex]::Replace(
        $migrationSafeText,
        '(?im)^(?<indent>[ \t]*)CREATE[ \t]+VIEW[ \t]+',
        '${indent}CREATE OR REPLACE VIEW '
    )

    return $migrationSafeText
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

function Get-SharedMySqlDefaultsFile {
    $cacheKey = Get-MySqlExecutionCacheKey

    if (
        $script:SharedMySqlDefaultsFileKey -eq $cacheKey -and
        $null -ne $script:SharedMySqlDefaultsFile -and
        (Test-Path $script:SharedMySqlDefaultsFile -ErrorAction SilentlyContinue)
    ) {
        return $script:SharedMySqlDefaultsFile
    }

    Clear-SharedMySqlDefaultsFile
    $script:SharedMySqlDefaultsFile = New-MySqlDefaultsFile
    $script:SharedMySqlDefaultsFileKey = $cacheKey

    return $script:SharedMySqlDefaultsFile
}

# Run a single SQL statement via -e.  No DELIMITER directives involved, so
# ProcessStartInfo pipe-based stdin is fine here.
function Execute-SqlCommand {
    param(
        [string]$SqlCommand,
        [switch]$NoDatabase,
        [string]$DatabaseName
    )
    $exe = Find-MySqlExe
    $defaultsFile = Get-SharedMySqlDefaultsFile
    $targetDatabase = if (-not [string]::IsNullOrWhiteSpace($DatabaseName)) { $DatabaseName } else { $Database }
    $dbPart = if ($NoDatabase) { "" } else { $targetDatabase }
    $argStr = "--defaults-extra-file=`"$defaultsFile`" -h$($script:CurrentServer) -P$($script:CurrentPort) --connect-timeout=$($script:Config.Connection.ClientConnectTimeoutSeconds) --default-character-set=utf8mb4 $dbPart -e `"$SqlCommand`""

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

# Runs a query and returns the raw batch output so callers can parse metadata.
function Invoke-MySqlQuery {
    param(
        [string]$SqlCommand,
        [string]$DatabaseName,
        [switch]$NoDatabase
    )

    $exe = Find-MySqlExe
    $defaultsFile = Get-SharedMySqlDefaultsFile
    $targetDatabase = if (-not [string]::IsNullOrWhiteSpace($DatabaseName)) { $DatabaseName } else { $Database }
    $dbPart = if ($NoDatabase) { "" } else { $targetDatabase }
    $singleLineCommand = ($SqlCommand -split "\r?\n" | ForEach-Object { $_.Trim() } | Where-Object { $_ }) -join ' '
    $argStr = "--defaults-extra-file=`"$defaultsFile`" -h$($script:CurrentServer) -P$($script:CurrentPort) --connect-timeout=$($script:Config.Connection.ClientConnectTimeoutSeconds) --default-character-set=utf8mb4 --batch --raw $dbPart -e `"$singleLineCommand`""

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

    return @($stdout -split "`r?`n")
}

# Execute a .sql file by redirecting a real file handle into mysql.exe.
# This is required for client-side DELIMITER handling to behave consistently on
# Windows across both MySQL 5.7 and 8.0. Pipe-based stdin is not reliable here
# for stored-procedure scripts.
#
# When FK checks must be disabled the file content is wrapped with the toggle
# statements so they run in the same mysql session.
#
# MigrationSafe keeps an existing database intact: destructive DDL is stripped
# out and object creation is made repeatable before the script is executed.
function Execute-SqlFile {
    param(
        [string]$FilePath,
        [switch]$DisableForeignKeyChecks,
        [switch]$MigrationSafe,
        [string]$DatabaseName
    )
    if (-not (Test-Path $FilePath)) { throw "File not found: $FilePath" }

    $exe = Find-MySqlExe
    $tempSql = $null
    $defaultsFile = $null
    $stdoutFile = $null
    $stderrFile = $null
    $legacyBatchFiles = New-Object System.Collections.Generic.List[string]

    try {
        $defaultsFile = Get-SharedMySqlDefaultsFile
        $repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

        $body = [System.IO.File]::ReadAllText($FilePath)
        $body = Expand-MySqlSourceReferences -SqlText $body -BaseDirectory $repoRoot

        if ($MigrationSafe) {
            $body = ConvertTo-MigrationSafeSqlText -SqlText $body
        }

        if ($DisableForeignKeyChecks -or $MigrationSafe) {
            $tempSql = [System.IO.Path]::ChangeExtension([System.IO.Path]::GetTempFileName(), '.sql')
            $prefix = if ($DisableForeignKeyChecks) { "SET FOREIGN_KEY_CHECKS = 0;`r`n" } else { [string]::Empty }
            $suffix = if ($DisableForeignKeyChecks) { "`r`nSET FOREIGN_KEY_CHECKS = 1;" } else { [string]::Empty }

            # UTF-8 without BOM — a BOM at byte 0 corrupts mysql's client-side parser.
            [System.IO.File]::WriteAllText(
                $tempSql,
                $prefix + $body + $suffix,
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
            $body = Normalize-MySqlLegacyRoutineDelimiterLayout -SqlText $body
            $segments = @(Split-MySqlScriptByDelimiter -SqlText $body)
            foreach ($segment in $segments) {
                $segmentFile = [System.IO.Path]::ChangeExtension([System.IO.Path]::GetTempFileName(), '.sql')
                [System.IO.File]::WriteAllText($segmentFile, $segment.sqlText, [System.Text.UTF8Encoding]::new($false))
                $legacyBatchFiles.Add($segmentFile)

                $stdoutFile = [System.IO.Path]::GetTempFileName()
                $stderrFile = [System.IO.Path]::GetTempFileName()
                $exitCode = Invoke-MySqlBatchFile -Exe $exe -DefaultsFile $defaultsFile -TargetFile $segmentFile -StdoutFile $stdoutFile -StderrFile $stderrFile -Delimiter $segment.delimiter -DatabaseName $DatabaseName

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
        $exitCode = Invoke-MySqlBatchFile -Exe $exe -DefaultsFile $defaultsFile -TargetFile $targetFile -StdoutFile $stdoutFile -StderrFile $stderrFile -DatabaseName $DatabaseName

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

# Reads the column definitions that exist in the schema files but not yet in the
# live database, so new columns can be added without touching existing rows.
function Get-MissingSchemaColumnDefinition {
    param(
        [string]$SourceDatabase,
        [string]$TargetDatabase
    )

    $missingColumnQuery = "SELECT e.TABLE_NAME, e.COLUMN_NAME FROM information_schema.COLUMNS e WHERE e.TABLE_SCHEMA = '$SourceDatabase' AND EXISTS (SELECT 1 FROM information_schema.TABLES t WHERE t.TABLE_SCHEMA = '$TargetDatabase' AND t.TABLE_NAME = e.TABLE_NAME AND t.TABLE_TYPE = 'BASE TABLE') AND e.COLUMN_NAME NOT IN (SELECT l.COLUMN_NAME FROM information_schema.COLUMNS l WHERE l.TABLE_SCHEMA = '$TargetDatabase' AND l.TABLE_NAME = e.TABLE_NAME) ORDER BY e.TABLE_NAME, e.ORDINAL_POSITION"

    $rows = @(Invoke-MySqlQuery -SqlCommand $missingColumnQuery -NoDatabase)
    $missingByTable = [ordered]@{}

    for ($index = 1; $index -lt $rows.Count; $index++) {
        $row = [string]$rows[$index]
        if ([string]::IsNullOrWhiteSpace($row)) { continue }

        $fields = $row -split "`t"
        if ($fields.Count -lt 2) { continue }

        $tableName = $fields[0].Trim()
        $columnName = $fields[1].Trim()
        if (-not $missingByTable.Contains($tableName)) {
            $missingByTable[$tableName] = New-Object System.Collections.Generic.List[string]
        }

        $missingByTable[$tableName].Add($columnName)
    }

    $definitions = New-Object System.Collections.Generic.List[object]

    foreach ($tableName in @($missingByTable.Keys)) {
        $createRows = @(Invoke-MySqlQuery -SqlCommand "SHOW CREATE TABLE ``$SourceDatabase``.``$tableName``" -NoDatabase)
        if ($createRows.Count -le 1) { continue }

        $definitionText = ($createRows[1..($createRows.Count - 1)] -join "`n")
        $tablePrefix = "$tableName`t"
        if ($definitionText.StartsWith($tablePrefix)) {
            $definitionText = $definitionText.Substring($tablePrefix.Length)
        }

        foreach ($missingColumn in $missingByTable[$tableName]) {
            foreach ($definitionLine in ($definitionText -split "`r?`n")) {
                $columnMatch = [regex]::Match($definitionLine, '^\s+`(?<name>[^`]+)`\s+(?<definition>.+?),?\s*$')
                if (-not $columnMatch.Success) { continue }
                if ($columnMatch.Groups['name'].Value -ne $missingColumn) { continue }

                $definitions.Add([pscustomobject]@{
                        Table      = $tableName
                        Column     = $missingColumn
                        Definition = $columnMatch.Groups['definition'].Value.TrimEnd(',', ' ')
                    })
                break
            }
        }
    }

    return $definitions
}

# Adds columns that exist in Sql_Files/Schemas but are missing from the live
# database, so 'Migrate Existing' brings an older schema up to date in place.
function Sync-MySqlSchemaColumns {
    param(
        [string]$SchemasPath,
        [string[]]$ExcludedPatterns
    )

    $liveDatabase = $Database
    $syncDatabase = "mtm_schema_sync_$([guid]::NewGuid().ToString('N').Substring(0, 8))"
    $addedCount = 0
    $messages = New-Object System.Collections.Generic.List[string]

    try {
        Execute-SqlCommand -SqlCommand "CREATE DATABASE ``$syncDatabase`` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;" -NoDatabase

        $schemaFiles = @(Get-SqlFiles -BasePath $SchemasPath -ExcludedPatterns $ExcludedPatterns | Sort-Object Name)
        foreach ($file in $schemaFiles) {
            try {
                Execute-SqlFile -FilePath $file.FullName -DisableForeignKeyChecks -DatabaseName $syncDatabase
            }
            catch {
                Write-Host "DEBUG: Schema sync reference skipped $($file.Name): $($_.Exception.Message)" -ForegroundColor DarkYellow
            }
        }

        $missingColumns = @(Get-MissingSchemaColumnDefinition -SourceDatabase $syncDatabase -TargetDatabase $liveDatabase)
        Write-Host "DEBUG: Schema sync found $($missingColumns.Count) missing column(s) in $liveDatabase" -ForegroundColor Cyan

        foreach ($missingColumn in $missingColumns) {
            try {
                Execute-SqlCommand -SqlCommand "ALTER TABLE ``$($missingColumn.Table)`` ADD COLUMN ``$($missingColumn.Column)`` $($missingColumn.Definition);"
                $addedCount++
            }
            catch {
                $messages.Add("Column $($missingColumn.Table).$($missingColumn.Column): $($_.Exception.Message)")
            }
        }
    }
    finally {
        try {
            Execute-SqlCommand -SqlCommand "DROP DATABASE IF EXISTS ``$syncDatabase``;" -NoDatabase
        }
        catch {
        }
    }

    return [pscustomobject]@{
        AddedCount = $addedCount
        Messages   = $messages
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

function Test-CanIgnoreMigrationSqlError {
    param(
        [ValidateSet('schema', 'migration', 'seed')]
        [string]$Stage,

        [string]$ErrorMessage
    )

    if ([string]::IsNullOrWhiteSpace($ErrorMessage)) {
        return $false
    }

    $normalizedError = $ErrorMessage.ToLowerInvariant()
    $sharedPatterns = @(
        'already exists',
        'duplicate column name',
        'duplicate key name',
        'duplicate foreign key constraint name',
        'multiple primary key defined',
        "check that column/key exists"
    )

    foreach ($pattern in $sharedPatterns) {
        if ($normalizedError.Contains($pattern)) {
            return $true
        }
    }

    if ($Stage -eq 'seed' -or $Stage -eq 'migration') {
        return $normalizedError.Contains('duplicate entry')
    }

    return $false
}

function Set-DeploymentButtonsEnabled {
    param(
        [bool]$IsEnabled
    )

    $deployButton.IsEnabled = $IsEnabled

    if ($null -ne $migrateButton) {
        $migrateButton.IsEnabled = $IsEnabled -and $script:CurrentMode -eq 'deployment'
    }
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

function Start-DatabaseExecution {
    param(
        [ValidateSet('deployment', 'migration')]
        [string]$ExecutionMode
    )

    if ($script:CurrentMode -eq 'catalog') {
        Run-ObjectCatalog
        return
    }

    try {
        $errorBorder.Visibility = 'Collapsed'
        $summaryBorder.Visibility = 'Collapsed'
        Reset-DeploymentProgressUi
        Clear-SharedMySqlDefaultsFile
        Set-DeploymentButtonsEnabled -IsEnabled $false
        $script:CurrentExecutionMode = $ExecutionMode
        $overallStatusText.Text = if ($ExecutionMode -eq 'migration') { 'Migrating existing database...' } else { 'Deploying database...' }

        $serverStatus = Resolve-MySqlServer
        Write-Host "DEBUG: $($serverStatus.StatusMessage)" -ForegroundColor Cyan
        $currentFileText.Text = $serverStatus.StatusMessage
        $overallStatusText.Text = $serverStatus.StatusMessage

        $script:ProjectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

        $secretSetup = Ensure-AuthSecretConfigured
        Write-Host "DEBUG: Auth secret scope = $($secretSetup.Scope), path = $($secretSetup.SecretPath)" -ForegroundColor Cyan

        if (-not (Test-Path $script:DatabaseRoot)) {
            throw "SQL deployment folder not found at: $script:DatabaseRoot"
        }

        $script:SchemaCountTotal = 0
        $script:MigrationCountTotal = 0
        $script:ViewCountTotal = 0
        $script:StoredProcCountTotal = 0
        $script:TriggerCountTotal = 0
        $script:SeedDataCountTotal = 0
        $script:SchemaSyncAddedTotal = 0
        $script:WarningMessages = New-Object System.Collections.Generic.List[string]

        $script:timer = New-Object System.Windows.Threading.DispatcherTimer
        $script:timer.Interval = [TimeSpan]::FromMilliseconds(1)

        $script:step = if ($ExecutionMode -eq 'migration') { 1 } else { 0 }
        $script:currentFiles = @()
        $script:currentIndex = 0

        $script:timer.Add_Tick({
                try {
                    Write-Host "DEBUG: Timer tick - Step $script:step ($script:CurrentExecutionMode)" -ForegroundColor Green

                    if ($script:step -eq 0) {
                        Write-Host "DEBUG: Dropping and recreating database: $Database" -ForegroundColor Yellow
                        $overallStatusText.Text = 'Recreating database...'
                        $currentFileText.Text = 'Dropping existing database...'
                        Execute-SqlCommand -SqlCommand "DROP DATABASE IF EXISTS ``$Database``;" -NoDatabase
                        $currentFileText.Text = 'Creating new database...'
                        Execute-SqlCommand -SqlCommand "CREATE DATABASE ``$Database`` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;" -NoDatabase
                        Write-Host 'DEBUG: Database recreated successfully' -ForegroundColor Green
                        $script:step = 1
                    }
                    elseif ($script:step -eq 1) {
                        $schemasPath = Join-Path $script:DatabaseRoot 'Schemas'
                        Write-Host "DEBUG: Checking schemas path: $schemasPath" -ForegroundColor Yellow
                        $overallStatusText.Text = if ($script:CurrentExecutionMode -eq 'migration') { 'Checking base schemas...' } else { 'Deploying schemas...' }
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

                            try {
                                Execute-SqlFile -FilePath $file.FullName -DisableForeignKeyChecks -MigrationSafe:($script:CurrentExecutionMode -eq 'migration')
                                $script:SchemaCountTotal++
                            }
                            catch {
                                if (
                                    $script:CurrentExecutionMode -eq 'migration' -and
                                    (Test-CanIgnoreMigrationSqlError -Stage 'schema' -ErrorMessage $_.Exception.Message)
                                ) {
                                    Write-Host "DEBUG: Schema already present during migration, continuing: $($file.Name)" -ForegroundColor DarkYellow
                                    $currentFileText.Text = "Skipping existing schema: $($file.Name)"
                                    $script:SchemaCountTotal++
                                }
                                else {
                                    throw
                                }
                            }

                            $script:currentIndex++
                        }
                        else {
                            Write-Host 'DEBUG: Schema deployment complete, moving to step 3' -ForegroundColor Green

                            if ($script:CurrentExecutionMode -eq 'migration') {
                                $overallStatusText.Text = 'Adding new columns to existing tables...'
                                $currentFileText.Text = 'Comparing schema files against the live database...'
                                Sync-UiRender

                                $schemaSyncResult = Sync-MySqlSchemaColumns -SchemasPath (Join-Path $script:DatabaseRoot 'Schemas') -ExcludedPatterns $script:ExcludePatterns
                                $script:SchemaSyncAddedTotal = $schemaSyncResult.AddedCount
                                foreach ($syncMessage in $schemaSyncResult.Messages) {
                                    $script:WarningMessages.Add("Column sync $syncMessage")
                                }

                                Write-Host "DEBUG: Column sync added $($schemaSyncResult.AddedCount) column(s)" -ForegroundColor Green
                            }

                            $script:currentFiles = @()
                            $script:currentIndex = 0
                            $script:step = 3
                        }
                    }
                    elseif ($script:step -eq 3) {
                        $migPath = Join-Path $script:DatabaseRoot 'Migrations'
                        Write-Host "DEBUG: Checking migrations path: $migPath" -ForegroundColor Yellow
                        $overallStatusText.Text = 'Applying migrations...'
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
                                if (Test-CanIgnoreMigrationSqlError -Stage 'migration' -ErrorMessage $_.Exception.Message) {
                                    Write-Host "DEBUG: Migration already applied, continuing: $($file.Name)" -ForegroundColor DarkYellow
                                    $currentFileText.Text = "Skipping already-applied migration: $($file.Name)"
                                    $script:MigrationCountTotal++
                                }
                                else {
                                    Write-Host "DEBUG: Migration warning (continuing): $($file.Name) - $($_.Exception.Message)" -ForegroundColor Yellow
                                    $script:WarningMessages.Add("Migration $($file.Name): $($_.Exception.Message)")
                                }
                            }
                            $script:currentIndex++
                        }
                        else {
                            Write-Host 'DEBUG: Migration deployment complete, moving to step 5' -ForegroundColor Green
                            $script:currentFiles = @()
                            $script:currentIndex = 0
                            $script:step = 5
                        }
                    }
                    elseif ($script:step -eq 5) {
                        $viewPath = Join-Path $script:DatabaseRoot 'Views'
                        Write-Host "DEBUG: Checking views path: $viewPath" -ForegroundColor Yellow
                        $overallStatusText.Text = 'Deploying views...'
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
                            Execute-SqlFile -FilePath $file.FullName -MigrationSafe:($script:CurrentExecutionMode -eq 'migration')
                            $script:ViewCountTotal++
                            $script:currentIndex++
                        }
                        else {
                            Write-Host 'DEBUG: View deployment complete, moving to step 7' -ForegroundColor Green
                            $script:currentFiles = @()
                            $script:currentIndex = 0
                            $script:step = 7
                        }
                    }
                    elseif ($script:step -eq 7) {
                        $spPath = Join-Path $script:DatabaseRoot 'StoredProcedures'
                        Write-Host "DEBUG: Checking stored procedures path: $spPath" -ForegroundColor Yellow
                        $overallStatusText.Text = 'Deploying stored procedures...'
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
                            Write-Host 'DEBUG: Stored procedure deployment complete, moving to step 9' -ForegroundColor Green
                            $script:currentFiles = @()
                            $script:currentIndex = 0
                            $script:step = 9
                        }
                    }
                    elseif ($script:step -eq 9) {
                        $triggerPath = Join-Path $script:DatabaseRoot 'Triggers'
                        Write-Host "DEBUG: Checking triggers path: $triggerPath" -ForegroundColor Yellow
                        $overallStatusText.Text = 'Deploying triggers...'
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
                            Write-Host 'DEBUG: Trigger deployment complete, moving to step 11' -ForegroundColor Green
                            $script:currentFiles = @()
                            $script:currentIndex = 0
                            $script:step = 11
                        }
                    }
                    elseif ($script:step -eq 11) {
                        $tdPath = Join-Path $script:DatabaseRoot 'SeedData'
                        Write-Host "DEBUG: Checking seed data path: $tdPath" -ForegroundColor Yellow
                        $overallStatusText.Text = 'Loading seed data...'
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
                                if (
                                    $script:CurrentExecutionMode -eq 'migration' -and
                                    (Test-CanIgnoreMigrationSqlError -Stage 'seed' -ErrorMessage $_.Exception.Message)
                                ) {
                                    Write-Host "DEBUG: Seed data already present during migration, continuing: $($file.Name)" -ForegroundColor DarkYellow
                                    $currentFileText.Text = "Skipping duplicate seed data: $($file.Name)"
                                    $script:SeedDataCountTotal++
                                }
                                else {
                                    Write-Host "DEBUG: Seed data warning (continuing): $($_.Exception.Message)" -ForegroundColor Yellow
                                    $script:WarningMessages.Add("Seed data $($file.Name): $($_.Exception.Message)")
                                }
                            }
                            $script:currentIndex++
                        }
                        else {
                            $script:step = 13
                        }
                    }
                    elseif ($script:step -eq 13) {
                        $finalStatusMessage = 'SQL object catalog and validation report skipped.'

                        if ($runValidationCheckBox.IsChecked) {
                            Write-Host 'DEBUG: Building SQL object catalog and validation report' -ForegroundColor Green
                            $overallStatusText.Text = 'Validating deployed SQL objects...'
                            $currentFileText.Text = 'Refreshing SQL object catalog...'

                            $reportPaths = New-ValidationOutputPaths -OutputsDirectory $script:ValidationOutputsPath

                            try {
                                $catalog = Save-SqlObjectCatalog -RepoRoot $script:ProjectRoot -DatabaseRoot $script:DatabaseRoot -OutputPath $script:CatalogPath
                                $currentFileText.Text = 'Writing validation report...'
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

                            $finalStatusMessage = "Validation report: $($reportPaths.markdown)"
                        }
                        else {
                            Write-Host 'DEBUG: Skipping optional SQL object catalog and validation report' -ForegroundColor DarkYellow
                        }

                        Write-Host 'DEBUG: Deployment complete!' -ForegroundColor Green
                        $script:timer.Stop()
                        $warningCount = $script:WarningMessages.Count
                        $completionText = if ($script:CurrentExecutionMode -eq 'migration') { 'Migration completed successfully!' } else { 'Deployment completed successfully!' }

                        if ($warningCount -gt 0) {
                            $completionText = if ($script:CurrentExecutionMode -eq 'migration') {
                                "Migration completed with $warningCount skipped script(s)."
                            }
                            else {
                                "Deployment completed with $warningCount skipped script(s)."
                            }

                            $errorBorder.Visibility = 'Visible'
                            $errorText.Text = "Skipped script(s):`n`n" + ($script:WarningMessages -join "`n")
                        }

                        $overallStatusText.Text = $completionText
                        $currentFileText.Text = if ($script:SchemaSyncAddedTotal -gt 0) {
                            "$($script:SchemaSyncAddedTotal) new column(s) added to existing tables. $finalStatusMessage"
                        }
                        else {
                            $finalStatusMessage
                        }
                        $summaryTitleText.Text = if ($script:CurrentExecutionMode -eq 'migration') { 'Migration Successful!' } else { 'Deployment Successful!' }
                        $summaryBorder.Visibility = 'Visible'
                        $summarySchemas.Text = "$($script:SchemaCountTotal) file(s)"
                        $summaryMigrations.Text = "$($script:MigrationCountTotal) file(s)"
                        $summaryViews.Text = "$($script:ViewCountTotal) file(s)"
                        $summaryStoredProcs.Text = "$($script:StoredProcCountTotal) procedure(s)"
                        $summaryTriggers.Text = "$($script:TriggerCountTotal) file(s)"
                        $summarySeedData.Text = "$($script:SeedDataCountTotal) file(s)"
                        Set-DeploymentButtonsEnabled -IsEnabled $true
                        Clear-SharedMySqlDefaultsFile
                    }
                }
                catch {
                    Write-Host 'DEBUG: Timer tick error!' -ForegroundColor Red
                    Write-Host "DEBUG: Error message: $($_.Exception.Message)" -ForegroundColor Red
                    Write-Host "DEBUG: Stack trace: $($_.ScriptStackTrace)" -ForegroundColor Red
                    $script:timer.Stop()
                    $overallStatusText.Text = if ($script:CurrentExecutionMode -eq 'migration') { 'Migration failed!' } else { 'Deployment failed!' }
                    $errorBorder.Visibility = 'Visible'
                    $filePath = if ($null -ne $file) { $file.FullName } else { 'n/a' }
                    $errorText.Text = "Error in step $($script:step)`n`nMessage: $($_.Exception.Message)`n`nFile: $filePath`n`nStack:`n$($_.ScriptStackTrace)"
                    Set-DeploymentButtonsEnabled -IsEnabled $true
                    Clear-SharedMySqlDefaultsFile
                }
            })

        Write-Host "DEBUG: PSScriptRoot = $PSScriptRoot" -ForegroundColor Cyan
        Write-Host "DEBUG: ProjectRoot = $script:ProjectRoot" -ForegroundColor Cyan
        Write-Host "DEBUG: DatabaseRoot = $script:DatabaseRoot" -ForegroundColor Cyan
        Write-Host "DEBUG: DatabaseRoot exists = $(Test-Path $script:DatabaseRoot)" -ForegroundColor Cyan
        Write-Host "DEBUG: Starting timer in $ExecutionMode mode..." -ForegroundColor Cyan
        $script:timer.Start()
        Write-Host 'DEBUG: Timer started successfully' -ForegroundColor Cyan
    }
    catch {
        Write-Host 'CRITICAL ERROR in click handler:' -ForegroundColor Red
        Write-Host "Message: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Stack: $($_.ScriptStackTrace)" -ForegroundColor Red
        $overallStatusText.Text = if ($ExecutionMode -eq 'migration') { 'Migration failed!' } else { 'Deployment failed!' }
        $errorBorder.Visibility = 'Visible'
        $errorText.Text = "Setup error: $($_.Exception.Message)`n`n$($_.ScriptStackTrace)"
        Set-DeploymentButtonsEnabled -IsEnabled $true
        Clear-SharedMySqlDefaultsFile
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
        $migrateButton.Visibility = 'Collapsed'
        $migrateButton.IsEnabled = $false
        Run-ObjectCatalog
    }
    else {
        $catalogPanel.Visibility = 'Collapsed'
        $deploymentPanel.Visibility = 'Visible'
        $modeSwitchButton.Content = 'Object Catalog Mode'
        $deployButton.Content = 'Start Deployment'
        $migrateButton.Visibility = 'Visible'
        $migrateButton.Content = 'Migrate Existing'
        $migrateButton.IsEnabled = $true
        $errorBorder.Visibility = 'Collapsed'
    }
}

$deployButton.Add_Click({
        Start-DatabaseExecution -ExecutionMode 'deployment'
    })

$migrateButton.Add_Click({
        Start-DatabaseExecution -ExecutionMode 'migration'
    })

$swapHostsButton.Add_Click({
        try {
            $errorBorder.Visibility = 'Collapsed'
            $swapHostsButton.IsEnabled = $false
            Set-DeploymentButtonsEnabled -IsEnabled $false
            $modeSwitchButton.IsEnabled = $false
            $overallStatusText.Text = 'Swapping host references across repository...'
            Show-OperationProgress -StatusText 'Preparing repository host swap...' -ProgressValue 0
            $currentFileText.Text = 'Collecting eligible files under the solution folder...'

            $swapResult = Invoke-RepoHostReferenceSwap

            $overallStatusText.Text = 'Repository host swap completed.'
            $currentFileText.Text = "$($swapResult.FilesChanged) of $($swapResult.FilesScanned) file(s) updated from $($swapResult.FromHost) to $($swapResult.ToHost)."
        }
        catch {
            $errorBorder.Visibility = 'Visible'
            $errorText.Text = "Host swap error:`n`n$($_.Exception.Message)"
            $overallStatusText.Text = 'Repository host swap failed.'
        }
        finally {
            Hide-OperationProgress
            $swapHostsButton.IsEnabled = $true
            Set-DeploymentButtonsEnabled -IsEnabled $true
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

$closeButton.Add_Click({
        Clear-SharedMySqlDefaultsFile
        $window.Close()
    })

$window.ShowDialog() | Out-Null
