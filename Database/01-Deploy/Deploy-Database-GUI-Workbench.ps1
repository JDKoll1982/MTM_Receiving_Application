# MTM Receiving Application — Database Deployment (MySQL Workbench / MySQL Server)
# Targets a standard MySQL Server installation (C:\Program Files\MySQL\).
# Uses Start-Process -RedirectStandardInput to give mysql.exe a real OS file handle
# so that DELIMITER directives in stored-procedure files are processed correctly.

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
                </Grid.RowDefinitions>
                <TextBlock Grid.Row="0" Grid.Column="0" Text="Server:"   FontWeight="Bold" Margin="0,5"/>
                <TextBlock Grid.Row="0" Grid.Column="1" Name="ServerText"   Margin="10,5" Foreground="#333"/>
                <TextBlock Grid.Row="1" Grid.Column="0" Text="Database:" FontWeight="Bold" Margin="0,5"/>
                <TextBlock Grid.Row="1" Grid.Column="1" Name="DatabaseText" Margin="10,5" Foreground="#333"/>
                <TextBlock Grid.Row="2" Grid.Column="0" Text="User:"     FontWeight="Bold" Margin="0,5"/>
                <TextBlock Grid.Row="2" Grid.Column="1" Name="UserText"     Margin="10,5" Foreground="#333"/>
            </Grid>
        </Border>

        <Border Grid.Row="2" Background="White" BorderBrush="#DDD" BorderThickness="1"
                CornerRadius="5" Padding="15">
            <StackPanel>
                <TextBlock Name="OverallStatusText" Text="Ready to deploy..."
                           FontSize="14" FontWeight="Bold" Margin="0,0,0,10"/>

                <StackPanel Margin="0,5">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                        </Grid.ColumnDefinitions>
                        <TextBlock Grid.Column="0" Text="[1/4] Database Schemas" Foreground="#666"/>
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
                        <TextBlock Grid.Column="0" Text="[2/4] Stored Procedures" Foreground="#666"/>
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
                        <TextBlock Grid.Column="0" Text="[3/4] Database Migrations" Foreground="#666"/>
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
                        <TextBlock Grid.Column="0" Text="[4/4] Test Data" Foreground="#666"/>
                        <TextBlock Grid.Column="1" Name="TestDataCount" Text="0/0" Foreground="#2196F3"/>
                    </Grid>
                    <ProgressBar Name="TestDataProgress" Height="8" Margin="0,5" Maximum="100"/>
                </StackPanel>

                <TextBlock Name="CurrentFileText" Text="" Margin="0,15,0,0"
                           FontSize="11" Foreground="#999" TextTrimming="CharacterEllipsis"/>

                <Border Name="ErrorBorder" Background="#FFEBEE" BorderBrush="#F44336"
                        BorderThickness="1" CornerRadius="3" Padding="10"
                        Margin="0,10,0,0" Visibility="Collapsed">
                    <TextBlock Name="ErrorText" Foreground="#C62828" TextWrapping="Wrap"/>
                </Border>
            </StackPanel>
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
                    </Grid.RowDefinitions>
                    <TextBlock Grid.Row="0" Grid.Column="0" Text="Schemas Deployed:"  Margin="0,2"/>
                    <TextBlock Grid.Row="0" Grid.Column="1" Name="SummarySchemas"     Margin="10,2" FontWeight="Bold"/>
                    <TextBlock Grid.Row="1" Grid.Column="0" Text="Stored Procedures:" Margin="0,2"/>
                    <TextBlock Grid.Row="1" Grid.Column="1" Name="SummaryStoredProcs" Margin="10,2" FontWeight="Bold"/>
                    <TextBlock Grid.Row="2" Grid.Column="0" Text="Migrations Applied:" Margin="0,2"/>
                    <TextBlock Grid.Row="2" Grid.Column="1" Name="SummaryMigrations"  Margin="10,2" FontWeight="Bold"/>
                    <TextBlock Grid.Row="3" Grid.Column="0" Text="Test Data Files:"   Margin="0,2"/>
                    <TextBlock Grid.Row="3" Grid.Column="1" Name="SummaryTestData"    Margin="10,2" FontWeight="Bold"/>
                </Grid>
            </StackPanel>
        </Border>

        <StackPanel Grid.Row="4" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,20,0,0">
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
$storedProcProgress = $window.FindName("StoredProcProgress")
$storedProcCount = $window.FindName("StoredProcCount")
$migrationProgress = $window.FindName("MigrationProgress")
$migrationCount = $window.FindName("MigrationCount")
$testDataProgress = $window.FindName("TestDataProgress")
$testDataCount = $window.FindName("TestDataCount")
$errorBorder = $window.FindName("ErrorBorder")
$errorText = $window.FindName("ErrorText")
$summaryBorder = $window.FindName("SummaryBorder")
$summarySchemas = $window.FindName("SummarySchemas")
$summaryStoredProcs = $window.FindName("SummaryStoredProcs")
$summaryMigrations = $window.FindName("SummaryMigrations")
$summaryTestData = $window.FindName("SummaryTestData")
$deployButton = $window.FindName("DeployButton")
$closeButton = $window.FindName("CloseButton")

$serverText.Text = "$Server`:$Port"
$databaseText.Text = $Database
$userText.Text = $User

# Locate mysql.exe under the default MySQL Server install directory.
# Falls back to whatever is on the system PATH.
function Find-MySqlExe {
    $installRoot = "C:\Program Files\MySQL"
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
    throw "mysql.exe not found under 'C:\Program Files\MySQL\' and not on the system PATH."
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
        $args = @{ Path = $BasePath; Filter = '*.sql'; ErrorAction = 'SilentlyContinue' }
        if ($Recurse) { $args['Recurse'] = $true }
        Get-ChildItem @args |
        Where-Object { $n = $_.Name; -not ($ExcludedPatterns | Where-Object { $n -like $_ }) } |
        ForEach-Object { $fileMap[$_.FullName.Substring($BasePath.Length).TrimStart('\')] = $_ }
    }

    if (Test-Path $updatedPath) {
        $args = @{ Path = $updatedPath; Filter = '*.sql'; ErrorAction = 'SilentlyContinue' }
        if ($Recurse) { $args['Recurse'] = $true }
        Get-ChildItem @args |
        Where-Object { $n = $_.Name; -not ($ExcludedPatterns | Where-Object { $n -like $_ }) } |
        ForEach-Object { $fileMap[$_.FullName.Substring($updatedPath.Length).TrimStart('\')] = $_ }
    }

    return $fileMap.Values
}

# Run a single SQL statement via -e.  No DELIMITER directives involved, so
# ProcessStartInfo pipe-based stdin is fine here.
function Execute-SqlCommand {
    param(
        [string]$SqlCommand,
        [switch]$NoDatabase
    )
    try {
        $exe = Find-MySqlExe
        $dbPart = if ($NoDatabase) { "" } else { $Database }
        $argStr = "-h$Server -P$Port -u$User -p$Password --default-character-set=utf8mb4 $dbPart -e `"$SqlCommand`""

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
    catch { throw $_ }
}

# Execute a .sql file by piping its content to mysql via stdin.
# MySQL's client-side DELIMITER command is processed in batch mode (stdin not a TTY)
# the same as in interactive mode — no file-handle trick is required.
#
# Using ProcessStartInfo (same as Execute-SqlCommand) instead of Start-Process
# -PassThru because Start-Process on Windows does not reliably populate ExitCode
# on the returned Process object, resulting in a null ExitCode even on failure.
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

    try {
        if ($DisableForeignKeyChecks) {
            $tempSql = [System.IO.Path]::ChangeExtension([System.IO.Path]::GetTempFileName(), '.sql')
            $body = [System.IO.File]::ReadAllText($FilePath)
            # UTF-8 without BOM — a BOM at byte 0 corrupts mysql's client-side parser.
            [System.IO.File]::WriteAllText(
                $tempSql,
                "SET FOREIGN_KEY_CHECKS = 0;`r`n" + $body + "`r`nSET FOREIGN_KEY_CHECKS = 1;",
                [System.Text.UTF8Encoding]::new($false)
            )
            $targetFile = $tempSql
        }
        else {
            $targetFile = $FilePath
        }

        $sqlContent = [System.IO.File]::ReadAllText($targetFile, [System.Text.UTF8Encoding]::new($false))
        $mysqlArgs = "-h$Server -P$Port -u$User -p$Password --default-character-set=utf8mb4 $Database"

        $psi = New-Object System.Diagnostics.ProcessStartInfo
        $psi.FileName = $exe
        $psi.Arguments = $mysqlArgs
        $psi.RedirectStandardInput = $true
        $psi.RedirectStandardOutput = $true
        $psi.RedirectStandardError = $true
        $psi.UseShellExecute = $false
        $psi.CreateNoWindow = $true

        $proc = New-Object System.Diagnostics.Process
        $proc.StartInfo = $psi
        $proc.Start() | Out-Null

        # Write SQL to stdin then close it so mysql sees EOF and proceeds.
        $proc.StandardInput.Write($sqlContent)
        $proc.StandardInput.Close()

        # Read both streams before WaitForExit to prevent pipe-buffer deadlock.
        # MySQL batch DDL produces minimal stdout; combining both gives the full
        # error context (SQL errors go to stdout, warnings go to stderr).
        $stdout = $proc.StandardOutput.ReadToEnd()
        $stderr = $proc.StandardError.ReadToEnd()
        $proc.WaitForExit()

        if ($proc.ExitCode -ne 0) {
            $detail = (@($stdout, $stderr) | Where-Object { $_ -and $_.Trim() }) -join "`n"
            throw "MySQL error (exit $($proc.ExitCode)): $detail"
        }
        return $true
    }
    finally {
        if ($null -ne $tempSql -and (Test-Path $tempSql -ErrorAction SilentlyContinue)) {
            Remove-Item $tempSql -ErrorAction SilentlyContinue
        }
    }
}

$deployButton.Add_Click({
        try {
            $errorBorder.Visibility = "Collapsed"
            $summaryBorder.Visibility = "Collapsed"
            $deployButton.IsEnabled = $false
            $overallStatusText.Text = "Deploying database..."

            $ProjectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
            $script:DatabaseRoot = Join-Path $ProjectRoot "Database"
            $script:ExcludePatterns = @("test_*.sql", "*_test.sql", "*.test.sql")

            if (-not (Test-Path $script:DatabaseRoot)) {
                throw "Database folder not found at: $script:DatabaseRoot"
            }

            $script:SchemaCountTotal = 0
            $script:StoredProcCountTotal = 0
            $script:MigrationCountTotal = 0
            $script:TestDataCountTotal = 0

            $script:timer = New-Object System.Windows.Threading.DispatcherTimer
            $script:timer.Interval = [TimeSpan]::FromMilliseconds(10)

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
                            $spPath = Join-Path $script:DatabaseRoot "StoredProcedures"
                            Write-Host "DEBUG: Checking stored procedures path: $spPath" -ForegroundColor Yellow
                            $overallStatusText.Text = "Deploying stored procedures..."
                            $script:currentFiles = @(Get-SqlFiles -BasePath $spPath -ExcludedPatterns $script:ExcludePatterns -Recurse | Sort-Object FullName)
                            Write-Host "DEBUG: Found $($script:currentFiles.Count) stored procedure files" -ForegroundColor Yellow
                            $script:currentIndex = 0
                            $script:step = if ($script:currentFiles.Count -eq 0) { 5 } else { 4 }
                        }
                        elseif ($script:step -eq 4) {
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
                                Write-Host "DEBUG: Stored procedure deployment complete, moving to step 5" -ForegroundColor Green
                                $script:currentFiles = @()
                                $script:currentIndex = 0
                                $script:step = 5
                            }
                        }
                        elseif ($script:step -eq 5) {
                            $migPath = Join-Path $script:DatabaseRoot "Migrations"
                            Write-Host "DEBUG: Checking migrations path: $migPath" -ForegroundColor Yellow
                            $overallStatusText.Text = "Applying migrations..."
                            $script:currentFiles = @(Get-SqlFiles -BasePath $migPath -ExcludedPatterns $script:ExcludePatterns | Sort-Object Name)
                            Write-Host "DEBUG: Found $($script:currentFiles.Count) migration files" -ForegroundColor Yellow
                            $script:currentIndex = 0
                            $script:step = if ($script:currentFiles.Count -eq 0) { 7 } else { 6 }
                        }
                        elseif ($script:step -eq 6) {
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
                                Write-Host "DEBUG: Migration deployment complete, moving to step 7" -ForegroundColor Green
                                $script:currentFiles = @()
                                $script:currentIndex = 0
                                $script:step = 7
                            }
                        }
                        elseif ($script:step -eq 7) {
                            $tdPath = Join-Path $script:DatabaseRoot "TestData"
                            Write-Host "DEBUG: Checking test data path: $tdPath" -ForegroundColor Yellow
                            $overallStatusText.Text = "Loading test data..."
                            $script:currentFiles = @(Get-SqlFiles -BasePath $tdPath -ExcludedPatterns $script:ExcludePatterns | Sort-Object Name)
                            Write-Host "DEBUG: Found $($script:currentFiles.Count) test data files" -ForegroundColor Yellow
                            $script:currentIndex = 0
                            $script:step = if ($script:currentFiles.Count -eq 0) { 9 } else { 8 }
                        }
                        elseif ($script:step -eq 8) {
                            if ($script:currentIndex -lt $script:currentFiles.Count) {
                                $file = $script:currentFiles[$script:currentIndex]
                                $pct = [int]((($script:currentIndex + 1) / $script:currentFiles.Count) * 100)
                                Write-Host "DEBUG: Deploying test data: $($file.Name)" -ForegroundColor Yellow
                                $testDataProgress.Value = $pct
                                $testDataCount.Text = "$($script:currentIndex + 1)/$($script:currentFiles.Count)"
                                $currentFileText.Text = "Deploying: $($file.Name)"
                                try {
                                    Execute-SqlFile -FilePath $file.FullName
                                    $script:TestDataCountTotal++
                                }
                                catch {
                                    Write-Host "DEBUG: Test data warning (continuing): $($_.Exception.Message)" -ForegroundColor Yellow
                                }
                                $script:currentIndex++
                            }
                            else {
                                $script:step = 9
                            }
                        }
                        elseif ($script:step -eq 9) {
                            Write-Host "DEBUG: Deployment complete!" -ForegroundColor Green
                            $script:timer.Stop()
                            $overallStatusText.Text = "Deployment completed successfully!"
                            $currentFileText.Text = ""
                            $summaryBorder.Visibility = "Visible"
                            $summarySchemas.Text = "$($script:SchemaCountTotal) file(s)"
                            $summaryStoredProcs.Text = "$($script:StoredProcCountTotal) procedure(s)"
                            $summaryMigrations.Text = "$($script:MigrationCountTotal) file(s)"
                            $summaryTestData.Text = "$($script:TestDataCountTotal) file(s)"
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
            Write-Host "DEBUG: ProjectRoot = $ProjectRoot" -ForegroundColor Cyan
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

$closeButton.Add_Click({ $window.Close() })

$window.ShowDialog() | Out-Null
