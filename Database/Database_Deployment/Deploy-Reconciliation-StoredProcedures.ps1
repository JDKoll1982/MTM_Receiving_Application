<#
.SYNOPSIS
    Deploys the Receiving Validator stored procedures under the R-SQL-06 safety workflow.

.DESCRIPTION
    Applies 19_Migration_reconciliation_stored_procedures.sql, read-only by default.

    The workflow is the one .github/instructions/database/db-script-execution-standards.instructions.md
    requires for any script that can modify a database:

      * read-only unless -Execute is supplied
      * -WhatIf logs the intended statements and changes nothing
      * -GenerateSql writes the ordered SQL for review and executes nothing
      * a mysqldump backup of core before an -Execute against core, with the path printed
      * a typed confirmation before core is written
      * abort on the first error and restore from that backup
      * validation against mtm_receiving_application_test before core is touched
      * idempotent, because every object is dropped and recreated

    Secrets are prompted for and never echoed. Nothing is hardcoded.

.EXAMPLE
    ./Deploy-Reconciliation-StoredProcedures.ps1 -WhatIf

.EXAMPLE
    ./Deploy-Reconciliation-StoredProcedures.ps1 -GenerateSql ./reconciliation-sp.sql

.EXAMPLE
    ./Deploy-Reconciliation-StoredProcedures.ps1 -Execute
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [ValidateSet('Test', 'Core')]
    [string]$Target = 'Test',

    [string]$DatabaseHost = $(if ($env:MTM_DB_HOST) { $env:MTM_DB_HOST } else { 'localhost' }),

    [int]$DatabasePort = $(if ($env:MTM_DB_PORT) { [int]$env:MTM_DB_PORT } else { 3306 }),

    [string]$DatabaseUser = $env:MTM_DB_USER,

    [string]$GenerateSql,

    [string]$BackupDirectory = (Get-Location).Path,

    [switch]$Execute
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$CoreDatabase = 'mtm_receiving_application'
$TestDatabase = 'mtm_receiving_application_test'
$MigrationPath = Join-Path $PSScriptRoot 'Sql_Files/Migrations/19_Migration_reconciliation_stored_procedures.sql'

if (-not (Test-Path $MigrationPath)) {
    throw "Migration file not found: $MigrationPath"
}

if ([string]::IsNullOrWhiteSpace($DatabaseUser)) {
    $DatabaseUser = Read-Host 'MySQL user'
}

if ([string]::IsNullOrWhiteSpace($DatabaseUser)) {
    throw 'A MySQL user is required. Set MTM_DB_USER or answer the prompt.'
}

$TargetDatabase = if ($Target -eq 'Core') { $CoreDatabase } else { $TestDatabase }
$MigrationSql = Get-Content -Path $MigrationPath -Raw

if ($GenerateSql) {
    $resolved = [System.IO.Path]::GetFullPath($GenerateSql, (Get-Location).Path)
    Set-Content -Path $resolved -Value $MigrationSql -NoNewline
    Write-Host "Wrote reviewed SQL to $resolved. Nothing was executed."
    return
}

Write-Host "Target database : $TargetDatabase"
Write-Host "Migration       : $MigrationPath"

if (-not $Execute -or $WhatIfPreference) {
    Write-Host ''
    Write-Host 'Read-only mode. The statements below would run:'
    Write-Host '  DROP PROCEDURE IF EXISTS sp_Reconciliation_ReceivingRows_GetDay'
    Write-Host '  CREATE PROCEDURE sp_Reconciliation_ReceivingRows_GetDay'
    Write-Host '  DROP PROCEDURE IF EXISTS sp_Reconciliation_DayActivity_GetRange'
    Write-Host '  CREATE PROCEDURE sp_Reconciliation_DayActivity_GetRange'
    Write-Host '  DROP PROCEDURE IF EXISTS sp_Reconciliation_ReceivingLocation_Update'
    Write-Host '  CREATE PROCEDURE sp_Reconciliation_ReceivingLocation_Update'
    Write-Host ''
    Write-Host 'Re-run with -GenerateSql <file> to review the SQL, or -Execute to apply it.'
    return
}

function Invoke-MySqlFile {
    param(
        [Parameter(Mandatory)][string]$DatabaseName,
        [Parameter(Mandatory)][string]$FilePath
    )

    $exitCode = Invoke-MySqlCli -Arguments @("--database=$DatabaseName") -StandardInputPath $FilePath

    return $exitCode
}

function Invoke-MySqlCli {
    param(
        [Parameter(Mandatory)][string[]]$Arguments,
        [string]$StandardInputPath
    )

    $allArguments = @(
        "--host=$DatabaseHost",
        "--port=$DatabasePort",
        "--user=$DatabaseUser",
        '--protocol=TCP'
    ) + $Arguments

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new('mysql')
    foreach ($argument in $allArguments) {
        $startInfo.ArgumentList.Add($argument)
    }

    $startInfo.RedirectStandardInput = [bool]$StandardInputPath
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.UseShellExecute = $false

    $process = [System.Diagnostics.Process]::Start($startInfo)
    if ($StandardInputPath) {
        $process.StandardInput.Write((Get-Content -Path $StandardInputPath -Raw))
        $process.StandardInput.Close()
    }

    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    if ($stdout) {
        Write-Verbose $stdout
    }

    if ($process.ExitCode -ne 0) {
        throw "mysql exited with $($process.ExitCode): $stderr"
    }

    return $process.ExitCode
}

$securePassword = Read-Host -Prompt "Password for $DatabaseUser" -AsSecureString
$passwordPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
$plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPointer)

[Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPointer)
$env:MYSQL_PWD = $plainPassword
$plainPassword = $null

try {
    Write-Host ''
    Write-Host "Step 1/2: validating against $TestDatabase"
    Invoke-MySqlFile -DatabaseName $TestDatabase -FilePath $MigrationPath

    if ($Target -ne 'Core') {
        Write-Host 'Test target applied. Nothing was written to core.'
        return
    }

    Write-Host ''
    Write-Host "Step 2/2: applying to $CoreDatabase"

    $timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $backupPath = Join-Path $BackupDirectory "before_19_reconciliation_$timestamp.sql"

    & mysqldump --single-transaction --routines "--host=$DatabaseHost" "--port=$DatabasePort" "--user=$DatabaseUser" $CoreDatabase 2>$null |
        Set-Content -Path $backupPath

    if (-not (Test-Path $backupPath)) {
        throw 'The pre-write backup was not created, so core was not touched.'
    }

    Write-Host "Backup written to $backupPath"

    $confirmation = Read-Host "Type APPLY TO CORE to write $CoreDatabase"
    if ($confirmation -cne 'APPLY TO CORE') {
        Write-Host 'Confirmation did not match. Nothing was written.'
        return
    }

    try {
        Invoke-MySqlFile -DatabaseName $CoreDatabase -FilePath $MigrationPath
    }
    catch {
        Write-Warning "Apply failed: $($_.Exception.Message)"
        Write-Warning "Restoring $CoreDatabase from $backupPath"
        Get-Content -Path $backupPath -Raw | & mysql "--host=$DatabaseHost" "--port=$DatabasePort" "--user=$DatabaseUser" $CoreDatabase
        throw
    }

    Write-Host "Applied to $CoreDatabase. Re-running is safe: every object is dropped and recreated."
}
finally {
    Remove-Item Env:MYSQL_PWD -ErrorAction SilentlyContinue
}
