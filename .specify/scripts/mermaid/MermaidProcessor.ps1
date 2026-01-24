<#
.SYNOPSIS
    Master orchestration script for Mermaid diagram processing

.DESCRIPTION
    Coordinates multiple scripts to parse, process, validate, and update Mermaid diagrams
    in specification files following the namespace naming convention.

.PARAMETER FilePath
    Path to the markdown file containing Mermaid diagrams

.PARAMETER Action
    Action to perform: Parse, ApplyNamespaces, Validate, ProcessAll

.PARAMETER BackupFirst
    Create backup before processing (default: $true)

.PARAMETER DryRun
    Preview changes without applying them

.EXAMPLE
    .\MermaidProcessor.ps1 -FilePath "specs\001-workflow-consolidation\spec.md" -Action ProcessAll

.NOTES
    Version: 1.0
    Author: MTM Receiving Application Development Team
    Date: 2026-01-24
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$FilePath,

    [Parameter(Mandatory = $false)]
    [ValidateSet('Parse', 'ApplyNamespaces', 'Validate', 'ProcessAll', 'Restore', 'RemoveDiagrams', 'Generate')]
    [string]$Action = 'ProcessAll',

    [Parameter(Mandatory = $false)]
    [switch]$BackupFirst = $true,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$PreserveHeaders
)

# Import helper modules
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
. "$ScriptDir\MermaidParser.ps1"
. "$ScriptDir\MermaidNamespacer.ps1"
. "$ScriptDir\MermaidValidator.ps1"
. "$ScriptDir\FileBackupManager.ps1"

# Configuration
$Config = @{
    NamespaceFormat = 'W{UserStory}_{Workflow}_{NodeName}'
    BackupSuffix    = '.backup'
    LogFile         = "$ScriptDir\mermaid-processing.log"
    ValidationRules = @{
        RequireUniqueNodeIds   = $true
        RequireNamespacePrefix = $true
        RequireDecisionLabels  = $true
        RequireEndNodeSuffixes = $true
    }
}

#region Logging Functions

function Write-Log {
    param(
        [string]$Message,
        [ValidateSet('Info', 'Warning', 'Error', 'Success')]
        [string]$Level = 'Info'
    )
    
    $timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    $logMessage = "[$timestamp] [$Level] $Message"
    
    # Console output with color
    switch ($Level) {
        'Info' { Write-Host $logMessage -ForegroundColor Cyan }
        'Warning' { Write-Host $logMessage -ForegroundColor Yellow }
        'Error' { Write-Host $logMessage -ForegroundColor Red }
        'Success' { Write-Host $logMessage -ForegroundColor Green }
    }
    
    # Log to file
    Add-Content -Path $Config.LogFile -Value $logMessage
}

#endregion

#region Main Processing Functions

function Invoke-MermaidParsing {
    param([string]$Path)
    
    Write-Log "Parsing Mermaid diagrams from: $Path" -Level Info
    
    try {
        $diagrams = Get-MermaidDiagrams -FilePath $Path
        Write-Log "Found $($diagrams.Count) Mermaid diagrams" -Level Success
        return $diagrams
    }
    catch {
        Write-Log "Error parsing diagrams: $_" -Level Error
        throw
    }
}

function Invoke-NamespaceApplication {
    param(
        [string]$Path,
        [switch]$DryRun
    )
    
    Write-Log "Applying namespace prefixes to diagrams in: $Path" -Level Info
    
    try {
        $diagrams = Get-MermaidDiagrams -FilePath $Path
        $updatedContent = Apply-MermaidNamespaces -Diagrams $diagrams -SourceFile $Path
        
        if ($DryRun) {
            Write-Log "DRY RUN - Would update $($diagrams.Count) diagrams" -Level Warning
            return $updatedContent
        }
        
        Set-Content -Path $Path -Value $updatedContent -Encoding UTF8
        Write-Log "Successfully applied namespaces to $($diagrams.Count) diagrams" -Level Success
        return $updatedContent
    }
    catch {
        Write-Log "Error applying namespaces: $_" -Level Error
        throw
    }
}

function Invoke-MermaidValidation {
    param([string]$Path)
    
    Write-Log "Validating Mermaid diagrams in: $Path" -Level Info
    
    try {
        $diagrams = Get-MermaidDiagrams -FilePath $Path
        $results = Test-MermaidDiagrams -Diagrams $diagrams -Rules $Config.ValidationRules
        
        $failCount = ($results | Where-Object { -not $_.IsValid }).Count
        
        if ($failCount -eq 0) {
            Write-Log "All $($diagrams.Count) diagrams passed validation" -Level Success
        }
        else {
            Write-Log "$failCount of $($diagrams.Count) diagrams failed validation" -Level Warning
            foreach ($result in $results | Where-Object { -not $_.IsValid }) {
                Write-Log "  - Workflow $($result.WorkflowId): $($result.Errors -join ', ')" -Level Error
            }
        }
        
        return $results
    }
    catch {
        Write-Log "Error validating diagrams: $_" -Level Error
        throw
    }
}

function Invoke-MermaidGeneration {
    param(
        [string]$Path,
        [switch]$DryRun
    )
    
    Write-Log "=== Starting Mermaid Generation from WORKFLOW_DATA ===" -Level Info
    
    # Call MermaidGenerator.ps1
    $generatorScript = Join-Path $ScriptDir "MermaidGenerator.ps1"
    
    if (-not (Test-Path $generatorScript)) {
        Write-Log "MermaidGenerator.ps1 not found at: $generatorScript" -Level Error
        throw "MermaidGenerator.ps1 not found"
    }
    
    try {
        if ($DryRun) {
            & $generatorScript -FilePath $Path -DryRun
        }
        else {
            & $generatorScript -FilePath $Path
            
            Write-Log "Generation complete. Now applying namespaces and validating..." -Level Info
            
            # After generation, apply namespaces (don't run full ProcessAll to avoid recursion)
            Write-Log "Step 1/2: Applying namespace prefixes..." -Level Info
            Invoke-NamespaceApplication -Path $Path -DryRun:$false
            
            # Validate the final result
            Write-Log "Step 2/2: Validating diagrams..." -Level Info
            Invoke-MermaidValidation -Path $Path
            
            Write-Log "=== Generation Pipeline Complete ===" -Level Success
        }
    }
    catch {
        Write-Log "Error during Mermaid generation: $_" -Level Error
        throw
    }
}

function Invoke-ProcessAll {
    param(
        [string]$Path,
        [switch]$DryRun
    )
    
    Write-Log "=== Starting Full Mermaid Processing Pipeline ===" -Level Info
    
    # Step 1: Backup
    if ($BackupFirst -and -not $DryRun) {
        Write-Log "Creating backup..." -Level Info
        $backupPath = New-FileBackup -FilePath $Path -BackupSuffix $Config.BackupSuffix
        Write-Log "Backup created: $backupPath" -Level Success
    }
    
    # Step 2: Parse
    Write-Log "Step 1/3: Parsing diagrams..." -Level Info
    $diagrams = Invoke-MermaidParsing -Path $Path
    
    # Step 3: Apply Namespaces
    Write-Log "Step 2/3: Applying namespace prefixes..." -Level Info
    $updatedContent = Invoke-NamespaceApplication -Path $Path -DryRun:$DryRun
    
    # Step 4: Validate
    Write-Log "Step 3/3: Validating updated diagrams..." -Level Info
    $validationResults = Invoke-MermaidValidation -Path $Path
    
    # Summary
    Write-Log "=== Processing Complete ===" -Level Success
    Write-Log "Total diagrams processed: $($diagrams.Count)" -Level Info
    $validCount = ($validationResults | Where-Object { $_.IsValid }).Count
    Write-Log "Valid diagrams: $validCount / $($diagrams.Count)" -Level Info
    
    if ($DryRun) {
        Write-Log "DRY RUN MODE - No changes were written to disk" -Level Warning
    }
    
    return @{
        Diagrams          = $diagrams
        ValidationResults = $validationResults
        UpdatedContent    = $updatedContent
    }
}

#endregion

#region Script Execution

try {
    # Validate file exists
    if (-not (Test-Path $FilePath)) {
        throw "File not found: $FilePath"
    }
    
    # Execute requested action
    switch ($Action) {
        'Generate' {
            $result = Invoke-MermaidGeneration -Path $FilePath -DryRun:$DryRun
        }
        'Parse' {
            $result = Invoke-MermaidParsing -Path $FilePath
            $result | Format-Table -AutoSize
        }
        'ApplyNamespaces' {
            $result = Invoke-NamespaceApplication -Path $FilePath -DryRun:$DryRun
            if ($DryRun) {
                Write-Host "`n=== PREVIEW OF CHANGES ===`n" -ForegroundColor Yellow
                Write-Host $result
            }
        }
        'Validate' {
            $result = Invoke-MermaidValidation -Path $FilePath
            $result | Format-Table -AutoSize
        }
        'ProcessAll' {
            $result = Invoke-ProcessAll -Path $FilePath -DryRun:$DryRun
        }
        'Restore' {
            Write-Log "Restoring from backup..." -Level Info
            $restored = Restore-FileBackup -FilePath $FilePath -BackupSuffix $Config.BackupSuffix
            Write-Log "File restored from: $restored" -Level Success
        }
        'RemoveDiagrams' {
            Write-Log "Removing all Mermaid diagrams from: $FilePath" -Level Info
            
            if ($BackupFirst -and -not $DryRun) {
                Write-Log "Creating backup..." -Level Info
                $backupPath = New-FileBackup -FilePath $FilePath -BackupSuffix $Config.BackupSuffix
                Write-Log "Backup created: $backupPath" -Level Success
            }
            
            $diagrams = Get-MermaidDiagrams -FilePath $FilePath
            $diagramCount = $diagrams.Count
            
            if ($diagramCount -eq 0) {
                Write-Log "No Mermaid diagrams found in file" -Level Warning
            }
            else {
                $updatedContent = Remove-AllMermaidDiagrams -FilePath $FilePath -PreserveHeaders:$PreserveHeaders
                
                if ($DryRun) {
                    Write-Log "DRY RUN - Would remove $diagramCount diagrams" -Level Warning
                    if ($PreserveHeaders) {
                        Write-Log "Headers would be preserved" -Level Info
                    }
                    else {
                        Write-Log "Headers would be removed" -Level Info
                    }
                    Write-Host "`n=== PREVIEW OF CLEANED FILE ===`n" -ForegroundColor Yellow
                    Write-Host $updatedContent
                }
                else {
                    Set-Content -Path $FilePath -Value $updatedContent -Encoding UTF8
                    if ($PreserveHeaders) {
                        Write-Log "Successfully removed $diagramCount diagrams (headers preserved)" -Level Success
                    }
                    else {
                        Write-Log "Successfully removed $diagramCount diagrams and headers" -Level Success
                    }
                }
            }
        }
    }
    
    exit 0
}
catch {
    Write-Log "Fatal error: $_" -Level Error
    Write-Log $_.ScriptStackTrace -Level Error
    exit 1
}

#endregion
