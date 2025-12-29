# Database Deployment Script for MTM Receiving Application
# This script deploys all schemas, stored procedures, and initial data

param(
    [string]$Server = "localhost",
    [string]$Port = "3306",
    [string]$Database = "mtm_receiving_application",
    [string]$User = "root",
    [string]$Password = "root"
)

$ErrorActionPreference = "Stop"

# Connection string
$ConnectionString = "Server=$Server;Port=$Port;Database=$Database;Uid=$User;Pwd=$Password;"

Write-Host "=" * 80
Write-Host "MTM Receiving Application - Database Deployment"
Write-Host "=" * 80
Write-Host ""
Write-Host "Connection: $Server`:$Port"
Write-Host "Database: $Database"
Write-Host ""

# Function to execute SQL file using MAMP MySQL
function Execute-SqlFile {
    param(
        [string]$FilePath,
        [string]$Description
    )
    
    Write-Host "Executing: $Description" -ForegroundColor Cyan
    Write-Host "File: $FilePath"
    
    if (-not (Test-Path $FilePath)) {
        Write-Host "  ERROR: File not found!" -ForegroundColor Red
        return $false
    }
    
    try {
        # Try common MAMP MySQL paths
        $mampPaths = @(
            "C:\MAMP\bin\mysql\bin\mysql.exe",
            "C:\MAMP\bin\mysql\mysql8\bin\mysql.exe",
            "mysql"  # Fallback to PATH
        )
        
        $mysqlCmd = $null
        foreach ($path in $mampPaths) {
            if (Test-Path $path -ErrorAction SilentlyContinue) {
                $mysqlCmd = $path
                break
            }
            if ($path -eq "mysql") {
                try {
                    $null = Get-Command mysql -ErrorAction Stop
                    $mysqlCmd = "mysql"
                    break
                }
                catch { }
            }
        }
        
        if (-not $mysqlCmd) {
            Write-Host "  ERROR: MySQL command not found. Please check MAMP installation." -ForegroundColor Red
            Write-Host "  Tried paths: $($mampPaths -join ', ')" -ForegroundColor Yellow
            return $false
        }
        
        # Execute using mysql command with source
        # Note: SQL Server stored procedures (Infor Visual) are not deployed to MySQL
        # We only deploy MySQL-compatible scripts here

        $arguments = "-h$Server -P$Port -u$User -p$Password $Database -e `"source $FilePath`""
        
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
        
        if ($process.ExitCode -eq 0) {
            Write-Host "  SUCCESS" -ForegroundColor Green
            if ($stdout) { Write-Host "  Output: $stdout" -ForegroundColor Gray }
            Write-Host ""
            return $true
        }
        else {
            Write-Host "  ERROR: MySQL returned exit code $($process.ExitCode)" -ForegroundColor Red
            if ($stderr) { Write-Host "  Error: $stderr" -ForegroundColor Red }
            Write-Host ""
            return $false
        }
    }
    catch {
        Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        return $false
    }
}

# Get project root
$ProjectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

# Step 1: Deploy Schema Files
Write-Host "STEP 1: Deploying Database Schemas" -ForegroundColor Yellow
Write-Host "-" * 80

$schemaFiles = @(
    "$ProjectRoot\Database\Schemas\01_create_receiving_tables.sql",
    "$ProjectRoot\Database\Schemas\02_create_authentication_tables.sql",
    "$ProjectRoot\Database\Schemas\03_create_receiving_tables.sql",
    "$ProjectRoot\Database\Schemas\04_create_package_preferences.sql",
    "$ProjectRoot\Database\Schemas\05_add_default_mode_to_users.sql",
    "$ProjectRoot\Database\Schemas\07_create_dunnage_tables_v2.sql",
    "$ProjectRoot\Database\Schemas\08_add_icon_to_dunnage_types.sql",
    "$ProjectRoot\Database\Schemas\09_fix_bad_icon_data.sql"
)

foreach ($file in $schemaFiles) {
    $fileName = Split-Path $file -Leaf
    if (-not (Execute-SqlFile -FilePath $file -Description $fileName)) {
        Write-Host "Schema deployment failed. Stopping." -ForegroundColor Red
        exit 1
    }
}

# Step 1.5: Deploy Migration Files
Write-Host "STEP 1.5: Deploying Database Migrations" -ForegroundColor Yellow
Write-Host "-" * 80

$migrationFiles = Get-ChildItem -Path "$ProjectRoot\Database\Migrations" -Filter "*.sql" | Sort-Object Name
foreach ($file in $migrationFiles) {
    $description = "Migration: $($file.Name)"
    if (-not (Execute-SqlFile -FilePath $file.FullName -Description $description)) {
        Write-Host "Migration deployment failed. Skipping." -ForegroundColor Yellow
    }
}

# Step 2: Deploy Stored Procedures
Write-Host "STEP 2: Deploying Stored Procedures" -ForegroundColor Yellow
Write-Host "-" * 80

$storedProcDirs = @(
    "$ProjectRoot\Database\StoredProcedures\Authentication",
    "$ProjectRoot\Database\StoredProcedures\Receiving",
    "$ProjectRoot\Database\StoredProcedures\Dunnage"
)

foreach ($dir in $storedProcDirs) {
    if (Test-Path $dir) {
        $spFiles = Get-ChildItem -Path $dir -Filter "*.sql" | Sort-Object Name
        foreach ($file in $spFiles) {
            $description = "SP: $($file.Name)"
            if (-not (Execute-SqlFile -FilePath $file.FullName -Description $description)) {
                Write-Host "Stored procedure deployment failed. Stopping." -ForegroundColor Red
                exit 1
            }
        }
    }
}

# Step 3: Deploy Test Data
Write-Host "STEP 3: Deploying Test Data" -ForegroundColor Yellow
Write-Host "-" * 80

$testDataFiles = @(
    "$ProjectRoot\Database\TestData\insert_test_data.sql",
    "$ProjectRoot\Database\TestData\010_seed_dunnage_complete.sql"
)

foreach ($file in $testDataFiles) {
    if (Test-Path $file) {
        $fileName = Split-Path $file -Leaf
        if (-not (Execute-SqlFile -FilePath $file -Description $fileName)) {
            Write-Host "Test data deployment failed. Skipping." -ForegroundColor Yellow
        }
    }
}

Write-Host "=" * 80
Write-Host "Database Deployment Complete!" -ForegroundColor Green
Write-Host "=" * 80
Write-Host ""
Write-Host "Next steps:"
Write-Host "1. Verify tables exist in your database"
Write-Host "2. Insert test user data if needed"
Write-Host "3. Run the application"
Write-Host ""
