<#
.SYNOPSIS
    File backup and restore management

.DESCRIPTION
    Creates timestamped backups and manages backup/restore operations
    for files being processed by the Mermaid tooling.
#>

function New-FileBackup {
    <#
    .SYNOPSIS
        Creates a backup of a file
    
    .PARAMETER FilePath
        Path to the file to backup
    
    .PARAMETER BackupSuffix
        Suffix to add to backup filename (default: .backup)
    
    .PARAMETER UseTimestamp
        Add timestamp to backup filename
    
    .OUTPUTS
        Path to the backup file
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,
        
        [Parameter(Mandatory = $false)]
        [string]$BackupSuffix = '.backup',
        
        [Parameter(Mandatory = $false)]
        [switch]$UseTimestamp
    )
    
    if (-not (Test-Path $FilePath)) {
        throw "File not found: $FilePath"
    }
    
    $fileName = Split-Path $FilePath -Leaf
    $directory = Split-Path $FilePath -Parent
    
    if ($UseTimestamp) {
        $timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
        $backupFileName = "$fileName.$timestamp$BackupSuffix"
    }
    else {
        $backupFileName = "$fileName$BackupSuffix"
    }
    
    $backupPath = Join-Path $directory $backupFileName
    
    Copy-Item -Path $FilePath -Destination $backupPath -Force
    
    Write-Verbose "Backup created: $backupPath"
    return $backupPath
}

function Restore-FileBackup {
    <#
    .SYNOPSIS
        Restores a file from its backup
    
    .PARAMETER FilePath
        Path to the original file
    
    .PARAMETER BackupSuffix
        Suffix of the backup file
    
    .PARAMETER BackupPath
        Explicit path to backup file (optional)
    
    .OUTPUTS
        Path to the restored backup file
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,
        
        [Parameter(Mandatory = $false)]
        [string]$BackupSuffix = '.backup',
        
        [Parameter(Mandatory = $false)]
        [string]$BackupPath
    )
    
    if (-not $BackupPath) {
        $fileName = Split-Path $FilePath -Leaf
        $directory = Split-Path $FilePath -Parent
        $BackupPath = Join-Path $directory "$fileName$BackupSuffix"
    }
    
    if (-not (Test-Path $BackupPath)) {
        throw "Backup file not found: $BackupPath"
    }
    
    Copy-Item -Path $BackupPath -Destination $FilePath -Force
    
    Write-Verbose "File restored from backup: $BackupPath"
    return $BackupPath
}

function Get-FileBackups {
    <#
    .SYNOPSIS
        Lists all backup files for a given file
    
    .PARAMETER FilePath
        Path to the original file
    
    .PARAMETER BackupSuffix
        Suffix pattern to search for (supports wildcards)
    
    .OUTPUTS
        Array of backup file paths
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,
        
        [Parameter(Mandatory = $false)]
        [string]$BackupSuffix = '.backup'
    )
    
    $fileName = Split-Path $FilePath -Leaf
    $directory = Split-Path $FilePath -Parent
    
    $searchPattern = "$fileName*$BackupSuffix"
    $backups = Get-ChildItem -Path $directory -Filter $searchPattern | 
    Sort-Object -Property LastWriteTime -Descending
    
    return $backups
}

function Remove-OldBackups {
    <#
    .SYNOPSIS
        Removes old backup files, keeping only the most recent N backups
    
    .PARAMETER FilePath
        Path to the original file
    
    .PARAMETER Keep
        Number of backups to keep (default: 5)
    
    .PARAMETER BackupSuffix
        Suffix pattern for backup files
    #>
    [CmdletBinding(SupportsShouldProcess)]
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,
        
        [Parameter(Mandatory = $false)]
        [int]$Keep = 5,
        
        [Parameter(Mandatory = $false)]
        [string]$BackupSuffix = '.backup'
    )
    
    $backups = Get-FileBackups -FilePath $FilePath -BackupSuffix $BackupSuffix
    
    if ($backups.Count -le $Keep) {
        Write-Verbose "Only $($backups.Count) backups exist, keeping all"
        return
    }
    
    $toRemove = $backups | Select-Object -Skip $Keep
    
    foreach ($backup in $toRemove) {
        if ($PSCmdlet.ShouldProcess($backup.FullName, "Remove old backup")) {
            Remove-Item -Path $backup.FullName -Force
            Write-Verbose "Removed old backup: $($backup.Name)"
        }
    }
}
