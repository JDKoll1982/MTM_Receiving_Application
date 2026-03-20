# Database Deployment Script for MTM Receiving Application - GUI Version
# This script deploys all schemas, stored procedures, and initial data using a WPF GUI.
# Requires a MySQL Server installation (8.0, 8.4, 9.x). MySQL Workbench is the
# recommended client for managing the server; this script uses the mysql.exe CLI
# that ships with MySQL Server.

param(
    [string]$Server = "172.16.1.104",
    [string]$Port = "3306",
    [string]$Database = "mtm_receiving_application",
    [string]$User = "root",
    [string]$Password = "root"
)

Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName WindowsBase

# XAML for the deployment window  
$xaml = @"
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="MTM Database Deployment" 
        Height="600" Width="800"
        WindowStartupLocation="CenterScreen"
        ResizeMode="NoResize"
        Background="#F5F5F5">
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Margin="0,0,0,20">
            <TextBlock Text="MTM Receiving Application" 
                       FontSize="24" 
                       FontWeight="Bold" 
                       Foreground="#2196F3"/>
            <TextBlock Text="Database Deployment Tool" 
                       FontSize="16" 
                       Foreground="#666"/>
        </StackPanel>

        <!-- Connection Details -->
        <Border Grid.Row="1" 
                Background="White" 
                BorderBrush="#DDD" 
                BorderThickness="1" 
                CornerRadius="5" 
                Padding="15" 
                Margin="0,0,0,20">
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

                <TextBlock Grid.Row="0" Grid.Column="0" Text="Server:" FontWeight="Bold" Margin="0,5"/>
                <TextBlock Grid.Row="0" Grid.Column="1" Name="ServerText" Margin="10,5" Foreground="#333"/>

                <TextBlock Grid.Row="1" Grid.Column="0" Text="Database:" FontWeight="Bold" Margin="0,5"/>
                <TextBlock Grid.Row="1" Grid.Column="1" Name="DatabaseText" Margin="10,5" Foreground="#333"/>

                <TextBlock Grid.Row="2" Grid.Column="0" Text="User:" FontWeight="Bold" Margin="0,5"/>
                <TextBlock Grid.Row="2" Grid.Column="1" Name="UserText" Margin="10,5" Foreground="#333"/>
            </Grid>
        </Border>

        <!-- Progress Section -->
        <Border Grid.Row="2" 
                Background="White" 
                BorderBrush="#DDD" 
                BorderThickness="1" 
                CornerRadius="5" 
                Padding="15">
            <StackPanel>
                <!-- Overall Progress -->
                <TextBlock Name="OverallStatusText" 
                           Text="Ready to deploy..." 
                           FontSize="14" 
                           FontWeight="Bold" 
                           Margin="0,0,0,10"/>

                <!-- Step 1: Schemas -->
                <StackPanel Name="SchemaSection" Margin="0,5">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                        </Grid.ColumnDefinitions>
                        <TextBlock Grid.Column="0" Name="SchemaText" Text="[1/4] Database Schemas" Foreground="#666"/>
                        <TextBlock Grid.Column="1" Name="SchemaCount" Text="0/0" Foreground="#2196F3"/>
                    </Grid>
                    <ProgressBar Name="SchemaProgress" Height="8" Margin="0,5" Maximum="100"/>
                </StackPanel>

                <!-- Step 2: Stored Procedures -->
                <StackPanel Name="StoredProcSection" Margin="0,5">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                        </Grid.ColumnDefinitions>
                        <TextBlock Grid.Column="0" Name="StoredProcText" Text="[2/4] Stored Procedures" Foreground="#666"/>
                        <TextBlock Grid.Column="1" Name="StoredProcCount" Text="0/0" Foreground="#2196F3"/>
                    </Grid>
                    <ProgressBar Name="StoredProcProgress" Height="8" Margin="0,5" Maximum="100"/>
                </StackPanel>

                <!-- Step 3: Migrations -->
                <StackPanel Name="MigrationSection" Margin="0,5">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                        </Grid.ColumnDefinitions>
                        <TextBlock Grid.Column="0" Name="MigrationText" Text="[3/4] Database Migrations" Foreground="#666"/>
                        <TextBlock Grid.Column="1" Name="MigrationCount" Text="0/0" Foreground="#2196F3"/>
                    </Grid>
                    <ProgressBar Name="MigrationProgress" Height="8" Margin="0,5" Maximum="100"/>
                </StackPanel>



                <!-- Step 4: Test Data -->
                <StackPanel Name="TestDataSection" Margin="0,5">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                        </Grid.ColumnDefinitions>
                        <TextBlock Grid.Column="0" Name="TestDataText" Text="[4/4] Test Data" Foreground="#666"/>
                        <TextBlock Grid.Column="1" Name="TestDataCount" Text="0/0" Foreground="#2196F3"/>
                    </Grid>
                    <ProgressBar Name="TestDataProgress" Height="8" Margin="0,5" Maximum="100"/>
                </StackPanel>

                <!-- Current File -->
                <TextBlock Name="CurrentFileText" 
                           Text="" 
                           Margin="0,15,0,0" 
                           FontSize="11" 
                           Foreground="#999" 
                           TextTrimming="CharacterEllipsis"/>

                <!-- Error Display -->
                <Border Name="ErrorBorder" 
                        Background="#FFEBEE" 
                        BorderBrush="#F44336" 
                        BorderThickness="1" 
                        CornerRadius="3" 
                        Padding="10" 
                        Margin="0,10,0,0"
                        Visibility="Collapsed">
                    <TextBlock Name="ErrorText" 
                               Foreground="#C62828" 
                               TextWrapping="Wrap"/>
                </Border>
            </StackPanel>
        </Border>

        <!-- Summary Section -->
        <Border Grid.Row="3" 
                Name="SummaryBorder"
                Background="#E8F5E9" 
                BorderBrush="#4CAF50" 
                BorderThickness="1" 
                CornerRadius="5" 
                Padding="15" 
                Margin="0,20,0,0"
                Visibility="Collapsed">
            <StackPanel>
                <TextBlock Text="Deployment Successful!" 
                           FontSize="16" 
                           FontWeight="Bold" 
                           Foreground="#2E7D32" 
                           Margin="0,0,0,10"/>
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
                    </Grid.RowDefinitions>

                    <TextBlock Grid.Row="0" Grid.Column="0" Text="Schemas Deployed:" Margin="0,2"/>
                    <TextBlock Grid.Row="0" Grid.Column="1" Name="SummarySchemas" Margin="10,2" FontWeight="Bold"/>

                    <TextBlock Grid.Row="1" Grid.Column="0" Text="Migrations Applied:" Margin="0,2"/>
                    <TextBlock Grid.Row="1" Grid.Column="1" Name="SummaryMigrations" Margin="10,2" FontWeight="Bold"/>

                    <TextBlock Grid.Row="2" Grid.Column="0" Text="Stored Procedures:" Margin="0,2"/>
                    <TextBlock Grid.Row="2" Grid.Column="1" Name="SummaryStoredProcs" Margin="10,2" FontWeight="Bold"/>

                    <TextBlock Grid.Row="3" Grid.Column="0" Text="Test Data Files:" Margin="0,2"/>
                    <TextBlock Grid.Row="3" Grid.Column="1" Name="SummaryTestData" Margin="10,2" FontWeight="Bold"/>
                </Grid>
            </StackPanel>
        </Border>

        <!-- Buttons -->
        <StackPanel Grid.Row="4" 
                    Orientation="Horizontal" 
                    HorizontalAlignment="Right" 
                    Margin="0,20,0,0">
            <Button Name="DeployButton" 
                    Content="Start Deployment" 
                    Width="140" 
                    Height="35" 
                    Background="#2196F3" 
                    Foreground="White" 
                    BorderThickness="0" 
                    FontWeight="Bold"
                    Cursor="Hand"/>
            <Button Name="CloseButton" 
                    Content="Close" 
                    Width="100" 
                    Height="35" 
                    Margin="10,0,0,0"
                    Background="#9E9E9E" 
                    Foreground="White" 
                    BorderThickness="0"
                    Cursor="Hand"/>
        </StackPanel>
    </Grid>
</Window>
"@

# Load XAML
$reader = [System.Xml.XmlNodeReader]::new([xml]$xaml)
$window = [Windows.Markup.XamlReader]::Load($reader)

# Get controls
$serverText = $window.FindName("ServerText")
$databaseText = $window.FindName("DatabaseText")
$userText = $window.FindName("UserText")
$overallStatusText = $window.FindName("OverallStatusText")
$currentFileText = $window.FindName("CurrentFileText")

$schemaProgress = $window.FindName("SchemaProgress")
$schemaCount = $window.FindName("SchemaCount")
$migrationProgress = $window.FindName("MigrationProgress")
$migrationCount = $window.FindName("MigrationCount")
$storedProcProgress = $window.FindName("StoredProcProgress")
$storedProcCount = $window.FindName("StoredProcCount")
$testDataProgress = $window.FindName("TestDataProgress")
$testDataCount = $window.FindName("TestDataCount")

$errorBorder = $window.FindName("ErrorBorder")
$errorText = $window.FindName("ErrorText")
$summaryBorder = $window.FindName("SummaryBorder")
$summarySchemas = $window.FindName("SummarySchemas")
$summaryMigrations = $window.FindName("SummaryMigrations")
$summaryStoredProcs = $window.FindName("SummaryStoredProcs")
$summaryTestData = $window.FindName("SummaryTestData")

$deployButton = $window.FindName("DeployButton")
$closeButton = $window.FindName("CloseButton")

# Set connection details
$serverText.Text = "$Server`:$Port"
$databaseText.Text = $Database
$userText.Text = $User

# Returns an ordered list of *.sql FileInfo objects for a given base folder.
# If a matching 'Updated{FolderName}' sibling folder exists, any file with the
# same relative path is replaced by the Updated version, and any extra files
# in the Updated folder are appended. This lets you drop in corrected SQL
# without touching the originals.
function Get-SqlFiles {
    param(
        [string]$BasePath,
        [string[]]$ExcludedPatterns,
        [switch]$Recurse
    )

    $folderName = Split-Path $BasePath -Leaf
    $parentPath = Split-Path $BasePath -Parent
    $updatedPath = Join-Path $parentPath "Updated$folderName"

    $fileMap = [ordered]@{}

    if (Test-Path $BasePath) {
        $getArgs = @{ Path = $BasePath; Filter = '*.sql'; ErrorAction = 'SilentlyContinue' }
        if ($Recurse) { $getArgs['Recurse'] = $true }
        Get-ChildItem @getArgs |
        Where-Object { $fn = $_.Name; -not ($ExcludedPatterns | Where-Object { $fn -like $_ }) } |
        ForEach-Object {
            $rel = $_.FullName.Substring($BasePath.Length).TrimStart('\')
            $fileMap[$rel] = $_
        }
    }

    if (Test-Path $updatedPath) {
        Write-Host "DEBUG: Updated folder found: $updatedPath" -ForegroundColor Magenta
        $updArgs = @{ Path = $updatedPath; Filter = '*.sql'; ErrorAction = 'SilentlyContinue' }
        if ($Recurse) { $updArgs['Recurse'] = $true }
        Get-ChildItem @updArgs |
        Where-Object { $fn = $_.Name; -not ($ExcludedPatterns | Where-Object { $fn -like $_ }) } |
        ForEach-Object {
            $rel = $_.FullName.Substring($updatedPath.Length).TrimStart('\')
            if ($fileMap.Contains($rel)) {
                Write-Host "DEBUG: Override: $rel" -ForegroundColor Magenta
            }
            $fileMap[$rel] = $_
        }
    }

    return $fileMap.Values
}

# Function to execute SQL command (for database drop/create)
function Execute-SqlCommand {
    param(
        [string]$SqlCommand,
        [switch]$NoDatabase
    )
    
    try {
        # Search for mysql.exe — scans all MySQL Server subdirectories automatically,
        # then falls back to an explicit list and finally the system PATH.
        $mysqlCmd = $null

        $mysqlInstallRoot = "C:\Program Files\MySQL"
        if (Test-Path $mysqlInstallRoot) {
            $found = Get-ChildItem -Path $mysqlInstallRoot -Recurse -Filter "mysql.exe" -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match "\\bin\\mysql\.exe$" } |
            Sort-Object FullName -Descending |
            Select-Object -First 1
            if ($found) { $mysqlCmd = $found.FullName }
        }

        if (-not $mysqlCmd) {
            try {
                $null = Get-Command mysql -ErrorAction Stop
                $mysqlCmd = "mysql"
            }
            catch { }
        }

        if (-not $mysqlCmd) {
            throw "mysql.exe not found. Ensure MySQL Server is installed under 'C:\Program Files\MySQL\' or is available on the system PATH."
        }
        
        if ($NoDatabase) {
            $arguments = "-h$Server -P$Port -u$User -p$Password --default-character-set=utf8mb4 -e `"$SqlCommand`""
        }
        else {
            $arguments = "-h$Server -P$Port -u$User -p$Password --default-character-set=utf8mb4 $Database -e `"$SqlCommand`""
        }
        
        $psi = New-Object System.Diagnostics.ProcessStartInfo
        $psi.FileName = $mysqlCmd
        $psi.Arguments = $arguments
        $psi.RedirectStandardOutput = $true
        $psi.RedirectStandardError = $true
        $psi.UseShellExecute = $false
        $psi.CreateNoWindow = $true
        
        $process = New-Object System.Diagnostics.Process
        $process.StartInfo = $psi
        $process.Start() | Out-Null
        
        $stdout = $process.StandardOutput.ReadToEnd()
        $stderr = $process.StandardError.ReadToEnd()
        $process.WaitForExit()
        
        if ($process.ExitCode -ne 0) {
            throw "MySQL error: $stderr"
        }
        
        return $true
    }
    catch {
        throw $_
    }
}

# Function to execute SQL file
function Execute-SqlFile {
    param(
        [string]$FilePath,
        [switch]$DisableForeignKeyChecks
    )
    
    if (-not (Test-Path $FilePath)) {
        throw "File not found: $FilePath"
    }

    try {
        # Search for mysql.exe — scans all MySQL Server subdirectories automatically,
        # then falls back to an explicit list and finally the system PATH.
        $mysqlCmd = $null

        $mysqlInstallRoot = "C:\Program Files\MySQL"
        if (Test-Path $mysqlInstallRoot) {
            $found = Get-ChildItem -Path $mysqlInstallRoot -Recurse -Filter "mysql.exe" -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match "\\bin\\mysql\.exe$" } |
            Sort-Object FullName -Descending |
            Select-Object -First 1
            if ($found) { $mysqlCmd = $found.FullName }
        }

        if (-not $mysqlCmd) {
            try {
                $null = Get-Command mysql -ErrorAction Stop
                $mysqlCmd = "mysql"
            }
            catch { }
        }

        if (-not $mysqlCmd) {
            throw "mysql.exe not found. Ensure MySQL Server is installed under 'C:\Program Files\MySQL\' or is available on the system PATH."
        }

        # mysql only processes DELIMITER when stdin is a real file handle (not a pipe).
        # ProcessStartInfo.RedirectStandardInput=true creates a pipe — that breaks it.
        # Start-Process -RedirectStandardInput <path> has the OS open the file directly
        # as the stdin file descriptor, identical to running: mysql args < file.sql
        # from an actual shell. This is the only .NET/PowerShell approach that works.
        #
        # Start-Process -RedirectStandardInput <file> gives mysql a file-backed stdin
        # handle which enables full client-side DELIMITER processing. -Wait is NOT used
        # because it adds ~1-2s of PowerShell polling+async-reader overhead per file.
        # Instead we call .WaitForExit() directly on the returned Process object, which
        # is a fast native kernel wait with no extra overhead.
        # stdout is not needed so it goes to NUL; only stderrFile is created per run.
        $tempFile = $null
        $stderrFile = [System.IO.Path]::GetTempFileName()

        try {
            if ($DisableForeignKeyChecks) {
                $tempFile = [System.IO.Path]::ChangeExtension(
                    [System.IO.Path]::GetTempFileName(), '.sql')
                $sqlContent = [System.IO.File]::ReadAllText($FilePath)
                # UTF-8 without BOM — a BOM at byte 0 can corrupt mysql's client-side
                # command parser, causing DELIMITER to be treated as a SQL keyword.
                [System.IO.File]::WriteAllText(
                    $tempFile,
                    "SET FOREIGN_KEY_CHECKS = 0;`r`n" + $sqlContent + "`r`nSET FOREIGN_KEY_CHECKS = 1;",
                    [System.Text.UTF8Encoding]::new($false)
                )
                $targetFile = $tempFile
            }
            else {
                $targetFile = $FilePath
            }

            $mysqlArgs = "-h$Server -P$Port -u$User -p$Password --default-character-set=utf8mb4 $Database"

            $proc = Start-Process `
                -FilePath $mysqlCmd `
                -ArgumentList $mysqlArgs `
                -RedirectStandardInput  $targetFile `
                -RedirectStandardError  $stderrFile `
                -NoNewWindow -PassThru

            $proc.WaitForExit()

            $stderr = Get-Content $stderrFile -Raw -ErrorAction SilentlyContinue

            if ($proc.ExitCode -ne 0) {
                throw "MySQL error: $stderr"
            }

            return $true
        }
        finally {
            Remove-Item $stderrFile -ErrorAction SilentlyContinue
            if ($null -ne $tempFile -and (Test-Path $tempFile -ErrorAction SilentlyContinue)) {
                Remove-Item $tempFile -ErrorAction SilentlyContinue
            }
        }
    }
    catch {
        throw $_
    }
}

# Async deployment using Background Worker
$deployButton.Add_Click({
        try {
            $errorBorder.Visibility = "Collapsed"
            $summaryBorder.Visibility = "Collapsed"
            $deployButton.IsEnabled = $false
            $overallStatusText.Text = "Deploying database..."
        
            # Get database root path - Make it script-level so timer can access it
            $ProjectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
            $script:DatabaseRoot = Join-Path $ProjectRoot "Database"
            $script:ExcludedFilePatterns = @("test_*.sql", "*_test.sql", "*.test.sql")
        
            # Debug output
            Write-Host "DEBUG: PSScriptRoot = $PSScriptRoot" -ForegroundColor Cyan
            Write-Host "DEBUG: ProjectRoot = $ProjectRoot" -ForegroundColor Cyan
            Write-Host "DEBUG: DatabaseRoot = $script:DatabaseRoot" -ForegroundColor Cyan
            Write-Host "DEBUG: DatabaseRoot exists = $(Test-Path $script:DatabaseRoot)" -ForegroundColor Cyan
        
            if (-not (Test-Path $script:DatabaseRoot)) {
                throw "Database folder not found at: $script:DatabaseRoot"
            }
        
            # Counters
            $script:SchemaCountTotal = 0
            $script:MigrationCountTotal = 0
            $script:StoredProcCountTotal = 0
            $script:TestDataCountTotal = 0
        
            Write-Host "DEBUG: Creating timer..." -ForegroundColor Cyan
        
            # Use Dispatcher timer to run async
            $script:timer = New-Object System.Windows.Threading.DispatcherTimer
            $script:timer.Interval = [TimeSpan]::FromMilliseconds(10)
        
            $script:deploymentStep = 0
            $script:currentFiles = @()
            $script:currentIndex = 0
        
            Write-Host "DEBUG: Adding timer tick handler..." -ForegroundColor Cyan
        
            $script:timer.Add_Tick({
                    try {
                        Write-Host "DEBUG: Timer tick - Step $script:deploymentStep" -ForegroundColor Green
                
                        if ($script:deploymentStep -eq 0) {
                            # Step 0: Drop and recreate database
                            Write-Host "DEBUG: Dropping and recreating database: $Database" -ForegroundColor Yellow
                            $overallStatusText.Text = "Recreating database..."
                            $currentFileText.Text = "Dropping existing database..."
                            
                            # Drop database
                            Execute-SqlCommand -SqlCommand "DROP DATABASE IF EXISTS ``$Database``;" -NoDatabase
                            
                            # Create database
                            $currentFileText.Text = "Creating new database..."
                            Execute-SqlCommand -SqlCommand "CREATE DATABASE ``$Database`` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;" -NoDatabase
                            
                            Write-Host "DEBUG: Database recreated successfully" -ForegroundColor Green
                            $script:deploymentStep = 1
                        }
                        elseif ($script:deploymentStep -eq 1) {
                            # Step 1: Schemas (prefers UpdatedSchemas overrides when present)
                            $schemasPath = Join-Path $script:DatabaseRoot "Schemas"
                            Write-Host "DEBUG: Checking schemas path: $schemasPath" -ForegroundColor Yellow
                    
                            $script:currentFiles = @(Get-SqlFiles -BasePath $schemasPath -ExcludedPatterns $script:ExcludedFilePatterns | Sort-Object Name)
                            Write-Host "DEBUG: Found $($script:currentFiles.Count) schema files" -ForegroundColor Yellow
                        
                            if ($script:currentFiles.Count -eq 0) {
                                $script:deploymentStep = 3
                            }
                            else {
                                $script:currentIndex = 0
                                $script:deploymentStep = 2
                            }
                        }
                        elseif ($script:deploymentStep -eq 2) {
                            if ($script:currentIndex -lt $script:currentFiles.Count) {
                                $file = $script:currentFiles[$script:currentIndex]
                                $percent = [int]((($script:currentIndex + 1) / $script:currentFiles.Count) * 100)
                        
                                Write-Host "DEBUG: Deploying schema: $($file.Name)" -ForegroundColor Yellow
                        
                                $schemaProgress.Value = $percent
                                $schemaCount.Text = "$($script:currentIndex + 1)/$($script:currentFiles.Count)"
                                $currentFileText.Text = "Deploying: $($file.Name)"
                        
                                Execute-SqlFile -FilePath $file.FullName -DisableForeignKeyChecks
                                $script:SchemaCountTotal++
                                $script:currentIndex++
                            }
                            else {
                                # Move to next step
                                Write-Host "DEBUG: Schema deployment complete, moving to step 3" -ForegroundColor Yellow
                                $script:deploymentStep = 3
                                $script:currentFiles = @()
                                $script:currentIndex = 0
                            }
                        }
                        elseif ($script:deploymentStep -eq 3) {
                            # Step 2: Stored Procedures (prefers UpdatedStoredProcedures overrides when present)
                            $storedProcsPath = Join-Path $script:DatabaseRoot "StoredProcedures"
                            Write-Host "DEBUG: Checking stored procedures path: $storedProcsPath" -ForegroundColor Yellow
                    
                            $script:currentFiles = @(Get-SqlFiles -BasePath $storedProcsPath -ExcludedPatterns $script:ExcludedFilePatterns -Recurse | Sort-Object FullName)
                            Write-Host "DEBUG: Found $($script:currentFiles.Count) stored procedure files" -ForegroundColor Yellow
                        
                            if ($script:currentFiles.Count -eq 0) {
                                $script:deploymentStep = 5
                            }
                            else {
                                $script:currentIndex = 0
                                $script:deploymentStep = 4
                            }
                        }
                        elseif ($script:deploymentStep -eq 4) {
                            if ($script:currentIndex -lt $script:currentFiles.Count) {
                                $file = $script:currentFiles[$script:currentIndex]
                                $percent = [int]((($script:currentIndex + 1) / $script:currentFiles.Count) * 100)
                        
                                $storedProcProgress.Value = $percent
                                $storedProcCount.Text = "$($script:currentIndex + 1)/$($script:currentFiles.Count)"
                                $currentFileText.Text = "Deploying: $($file.Name)"
                        
                                Execute-SqlFile -FilePath $file.FullName
                                $script:StoredProcCountTotal++
                                $script:currentIndex++
                            }
                            else {
                                $script:deploymentStep = 5
                                $script:currentFiles = @()
                                $script:currentIndex = 0
                            }
                        }
                        elseif ($script:deploymentStep -eq 5) {
                            # Step 3: Migrations (prefers UpdatedMigrations overrides when present)
                            $migrationsPath = Join-Path $script:DatabaseRoot "Migrations"
                            Write-Host "DEBUG: Checking migrations path: $migrationsPath" -ForegroundColor Yellow
                    
                            $script:currentFiles = @(Get-SqlFiles -BasePath $migrationsPath -ExcludedPatterns $script:ExcludedFilePatterns | Sort-Object Name)
                            Write-Host "DEBUG: Found $($script:currentFiles.Count) migration files" -ForegroundColor Yellow
                        
                            if ($script:currentFiles.Count -eq 0) {
                                $script:deploymentStep = 7
                            }
                            else {
                                $script:currentIndex = 0
                                $script:deploymentStep = 6
                            }
                        }
                        elseif ($script:deploymentStep -eq 6) {
                            if ($script:currentIndex -lt $script:currentFiles.Count) {
                                $file = $script:currentFiles[$script:currentIndex]
                                $percent = [int]((($script:currentIndex + 1) / $script:currentFiles.Count) * 100)
                        
                                $migrationProgress.Value = $percent
                                $migrationCount.Text = "$($script:currentIndex + 1)/$($script:currentFiles.Count)"
                                $currentFileText.Text = "Deploying: $($file.Name)"
                        
                                try {
                                    Execute-SqlFile -FilePath $file.FullName -DisableForeignKeyChecks
                                    $script:MigrationCountTotal++
                                }
                                catch {
                                    Write-Host "DEBUG: Migration error: $($_.Exception.Message)" -ForegroundColor Red
                                    # Continue on migration errors
                                }
                                $script:currentIndex++
                            }
                            else {
                                $script:deploymentStep = 7
                                $script:currentFiles = @()
                                $script:currentIndex = 0
                            }
                        }
                        elseif ($script:deploymentStep -eq 7) {
                            # Step 4: Test Data (prefers UpdatedTestData overrides when present)
                            $testDataPath = Join-Path $script:DatabaseRoot "TestData"
                            Write-Host "DEBUG: Checking test data path: $testDataPath" -ForegroundColor Yellow
                    
                            $script:currentFiles = @(Get-SqlFiles -BasePath $testDataPath -ExcludedPatterns $script:ExcludedFilePatterns | Sort-Object Name)
                            Write-Host "DEBUG: Found $($script:currentFiles.Count) test data files" -ForegroundColor Yellow
                        
                            if ($script:currentFiles.Count -eq 0) {
                                $script:deploymentStep = 9
                            }
                            else {
                                $script:currentIndex = 0
                                $script:deploymentStep = 8
                            }
                        }
                        elseif ($script:deploymentStep -eq 8) {
                            if ($script:currentIndex -lt $script:currentFiles.Count) {
                                $file = $script:currentFiles[$script:currentIndex]
                                $percent = [int]((($script:currentIndex + 1) / $script:currentFiles.Count) * 100)
                        
                                $testDataProgress.Value = $percent
                                $testDataCount.Text = "$($script:currentIndex + 1)/$($script:currentFiles.Count)"
                                $currentFileText.Text = "Deploying: $($file.Name)"
                        
                                try {
                                    Execute-SqlFile -FilePath $file.FullName
                                    $script:TestDataCountTotal++
                                }
                                catch {
                                    Write-Host "DEBUG: Test data error: $($_.Exception.Message)" -ForegroundColor Red
                                    # Continue on test data errors
                                }
                                $script:currentIndex++
                            }
                            else {
                                $script:deploymentStep = 9
                            }
                        }
                        elseif ($script:deploymentStep -eq 9) {
                            # Done!
                            Write-Host "DEBUG: Deployment complete!" -ForegroundColor Green
                    
                            if ($script:timer) {
                                $script:timer.Stop()
                            }
                    
                            $overallStatusText.Text = "Deployment completed successfully!"
                            $currentFileText.Text = ""
                            $summaryBorder.Visibility = "Visible"
                            $summarySchemas.Text = "$script:SchemaCountTotal file(s)"
                            $summaryMigrations.Text = "$script:MigrationCountTotal file(s)"
                            $summaryStoredProcs.Text = "$script:StoredProcCountTotal procedure(s)"
                            $summaryTestData.Text = "$script:TestDataCountTotal file(s)"
                            $deployButton.Content = "Deploy Again"
                            $deployButton.IsEnabled = $true
                        }
                    }
                    catch {
                        Write-Host "DEBUG: Timer tick error!" -ForegroundColor Red
                        Write-Host "DEBUG: Error message: $($_.Exception.Message)" -ForegroundColor Red
                        Write-Host "DEBUG: Stack trace: $($_.ScriptStackTrace)" -ForegroundColor Red
                
                        if ($script:timer) {
                            $script:timer.Stop()
                        }
                
                        # Build detailed error message
                        $errorDetails = @"
Error in deployment step $script:deploymentStep

Message: $($_.Exception.Message)

File: $($file.FullName)

Stack Trace:
$($_.ScriptStackTrace)
"@
                
                        $overallStatusText.Text = "Deployment failed!"
                        $errorBorder.Visibility = "Visible"
                        $errorText.Text = $errorDetails
                        $deployButton.Content = "Retry"
                        $deployButton.IsEnabled = $true
                    }
                })
        
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

$closeButton.Add_Click({
        $window.Close()
    })

# Show window
$window.ShowDialog() | Out-Null
