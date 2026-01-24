<#
.SYNOPSIS
    Generate Mermaid diagrams from WORKFLOW_DATA blocks in specification files

.DESCRIPTION
    Parses structured WORKFLOW_DATA blocks (key-value format in HTML comments) and generates
    raw Mermaid flowchart syntax. The generated diagrams use simple node IDs that will be
    namespaced by MermaidNamespacer.ps1 in the ProcessAll pipeline.

.PARAMETER FilePath
    Path to the markdown file containing WORKFLOW_DATA blocks

.PARAMETER Validate
    Only validate workflow relationships without generating diagrams

.PARAMETER DryRun
    Preview what would be generated without modifying the file

.EXAMPLE
    .\MermaidGenerator.ps1 -FilePath "specs\001-feature\spec.md"

.EXAMPLE
    .\MermaidGenerator.ps1 -FilePath "specs\001-feature\spec.md" -Validate

.NOTES
    Version: 1.0
    Author: MTM Receiving Application Development Team
    Date: 2026-01-24
    
    Format: WORKFLOW_DATA blocks use simple key-value pairs:
    <!-- WORKFLOW_START: 1.1 -->
    WORKFLOW: 1.1
    TITLE: Workflow Title
    DIRECTION: TD
    DEPENDS_ON: NONE
    CONFLICTS_WITH: NONE
    INTERACTION: Description
    NODE: NodeName
    TYPE: start|process|decision|end
    SHAPE: stadium|rect|diamond|circle
    LABEL: Display Text
    CONNECTION: From -> To [Label]
    <!-- WORKFLOW_END: 1.1 -->
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$FilePath,

    [Parameter(Mandatory = $false)]
    [switch]$Validate,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

#region Helper Functions

function Write-Log {
    param(
        [string]$Message,
        [ValidateSet('Info', 'Warning', 'Error', 'Success')]
        [string]$Level = 'Info'
    )
    
    $timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    $logMessage = "[$timestamp] [$Level] $Message"
    
    switch ($Level) {
        'Info' { Write-Host $logMessage -ForegroundColor Cyan }
        'Warning' { Write-Host $logMessage -ForegroundColor Yellow }
        'Error' { Write-Host $logMessage -ForegroundColor Red }
        'Success' { Write-Host $logMessage -ForegroundColor Green }
    }
}

function Get-WorkflowDataBlocks {
    param([string]$Content)
    
    $workflows = @()
    $pattern = '(?s)<!-- WORKFLOW_START:\s*([\d.]+)\s*-->(.+?)<!-- WORKFLOW_END:\s*\1\s*-->'
    $matches = [regex]::Matches($Content, $pattern)
    
    foreach ($match in $matches) {
        $workflowId = $match.Groups[1].Value.Trim()
        $data = $match.Groups[2].Value
        
        # Remove HTML comment wrappers if present (for hidden workflow data)
        # Format: <!-- WORKFLOW_START: X.Y --><!--DATA--><!-- WORKFLOW_END: X.Y -->
        $data = $data -replace '^\s*<!--\s*', '' -replace '\s*-->\s*$', ''
        
        $workflow = @{
            Id            = $workflowId
            Title         = ''
            Direction     = 'TD'
            DependsOn     = @()
            ConflictsWith = @()
            Interaction   = ''
            Nodes         = @()
            Connections   = @()
            RawBlock      = $match.Value
            StartIndex    = $match.Index
            EndIndex      = $match.Index + $match.Length
        }
        
        # Parse line by line
        $lines = $data -split "`n"
        $currentNode = $null
        
        foreach ($line in $lines) {
            $line = $line.Trim()
            if ([string]::IsNullOrWhiteSpace($line)) { continue }
            
            if ($line -match '^WORKFLOW:\s*(.+)$') {
                # Already captured from regex group
            }
            elseif ($line -match '^TITLE:\s*(.+)$') {
                $workflow.Title = $matches[1].Trim()
            }
            elseif ($line -match '^DIRECTION:\s*(TD|LR|RL|BT)$') {
                $workflow.Direction = $matches[1].Trim()
            }
            elseif ($line -match '^DEPENDS_ON:\s*(.+)$') {
                $value = $matches[1].Trim()
                if ($value -ne 'NONE') {
                    $workflow.DependsOn = $value -split ',' | ForEach-Object { $_.Trim() }
                }
            }
            elseif ($line -match '^CONFLICTS_WITH:\s*(.+)$') {
                $value = $matches[1].Trim()
                if ($value -ne 'NONE') {
                    $workflow.ConflictsWith = $value -split ',' | ForEach-Object { $_.Trim() }
                }
            }
            elseif ($line -match '^INTERACTION:\s*(.+)$') {
                $workflow.Interaction = $matches[1].Trim()
            }
            elseif ($line -match '^NODE:\s*(.+)$') {
                # Start new node
                if ($currentNode) {
                    $workflow.Nodes += $currentNode
                }
                $currentNode = @{
                    Name  = $matches[1].Trim()
                    Type  = ''
                    Shape = ''
                    Label = ''
                }
            }
            elseif ($line -match '^TYPE:\s*(.+)$') {
                if ($currentNode) {
                    $currentNode.Type = $matches[1].Trim()
                }
            }
            elseif ($line -match '^SHAPE:\s*(.+)$') {
                if ($currentNode) {
                    $currentNode.Shape = $matches[1].Trim()
                }
            }
            elseif ($line -match '^LABEL:\s*(.+)$') {
                if ($currentNode) {
                    $currentNode.Label = $matches[1].Trim()
                }
            }
            elseif ($line -match '^CONNECTION:\s*(.+)$') {
                $workflow.Connections += $matches[1].Trim()
            }
        }
        
        # Add last node if exists
        if ($currentNode) {
            $workflow.Nodes += $currentNode
        }
        
        $workflows += $workflow
    }
    
    return $workflows
}

function ConvertTo-MermaidNode {
    param(
        [string]$NodeName,
        [string]$Shape,
        [string]$Label
    )
    
    # Mermaid shape syntax mapping
    switch ($Shape) {
        'stadium' { return "$NodeName([`"$Label`"])" }
        'rect' { return "$NodeName[`"$Label`"]" }
        'diamond' { return "$NodeName{`"$Label`"}" }
        'circle' { return "$NodeName((`"$Label`"))" }
        'hexagon' { return "$NodeName{{`"$Label`"}}" }
        default { return "$NodeName[`"$Label`"]" }  # Default to rectangle
    }
}

function ConvertTo-MermaidSyntax {
    param([hashtable]$Workflow)
    
    $mermaid = @()
    $mermaid += "``````mermaid"
    $mermaid += "flowchart $($Workflow.Direction)"
    $mermaid += ""
    
    # Add nodes
    foreach ($node in $Workflow.Nodes) {
        $nodeDefinition = ConvertTo-MermaidNode -NodeName $node.Name -Shape $node.Shape -Label $node.Label
        $mermaid += "    $nodeDefinition"
    }
    
    if ($Workflow.Nodes.Count -gt 0 -and $Workflow.Connections.Count -gt 0) {
        $mermaid += ""
    }
    
    # Add connections
    foreach ($connection in $Workflow.Connections) {
        # Parse: "From -> To [Label]" or "From -> To"
        if ($connection -match '(.+?)\s*->\s*(.+?)\s*\[(.+)\]') {
            $from = $matches[1].Trim()
            $to = $matches[2].Trim()
            $label = $matches[3].Trim()
            $mermaid += "    $from -->|`"$label`"| $to"
        }
        elseif ($connection -match '(.+?)\s*->\s*(.+)') {
            $from = $matches[1].Trim()
            $to = $matches[2].Trim()
            $mermaid += "    $from --> $to"
        }
    }
    
    $mermaid += "``````"
    
    return $mermaid -join "`n"
}

function Test-WorkflowRelationships {
    param([array]$Workflows)
    
    $issues = @()
    $workflowIds = $Workflows | ForEach-Object { $_.Id }
    
    foreach ($workflow in $Workflows) {
        # Check DEPENDS_ON references exist
        foreach ($dep in $workflow.DependsOn) {
            if ($dep -notin $workflowIds) {
                $issues += @{
                    Severity = 'Error'
                    Workflow = $workflow.Id
                    Message  = "DEPENDS_ON references non-existent workflow: $dep"
                }
            }
        }
        
        # Check CONFLICTS_WITH references exist
        foreach ($conflict in $workflow.ConflictsWith) {
            if ($conflict -notin $workflowIds) {
                $issues += @{
                    Severity = 'Warning'
                    Workflow = $workflow.Id
                    Message  = "CONFLICTS_WITH references non-existent workflow: $conflict"
                }
            }
        }
        
        # Check for bidirectional conflicts
        foreach ($conflict in $workflow.ConflictsWith) {
            $otherWorkflow = $Workflows | Where-Object { $_.Id -eq $conflict }
            if ($otherWorkflow -and $workflow.Id -notin $otherWorkflow.ConflictsWith) {
                $issues += @{
                    Severity = 'Warning'
                    Workflow = $workflow.Id
                    Message  = "CONFLICTS_WITH $conflict is not bidirectional (workflow $conflict doesn't declare conflict with $($workflow.Id))"
                }
            }
        }
        
        # Check for circular dependencies (simple detection with timeout)
        $visited = @{}
        $stack = @($workflow.Id)
        $maxIterations = 1000  # Prevent infinite loops
        $iterations = 0
        
        while ($stack.Count -gt 0 -and $iterations -lt $maxIterations) {
            $iterations++
            
            # Pop from stack
            $current = $stack[-1]
            if ($stack.Count -eq 1) {
                $stack = @()
            }
            else {
                $stack = $stack[0..($stack.Count - 2)]
            }
            
            if ($visited.ContainsKey($current)) {
                if ($current -eq $workflow.Id -and $visited.Count -gt 1) {
                    $issues += @{
                        Severity = 'Error'
                        Workflow = $workflow.Id
                        Message  = "Circular dependency detected in dependency chain"
                    }
                    break
                }
                continue
            }
            
            $visited[$current] = $true
            $currentWorkflow = $Workflows | Where-Object { $_.Id -eq $current }
            if ($currentWorkflow) {
                foreach ($dep in $currentWorkflow.DependsOn) {
                    if (-not $visited.ContainsKey($dep)) {
                        $stack += $dep
                    }
                }
            }
        }
        
        if ($iterations -ge $maxIterations) {
            $issues += @{
                Severity = 'Warning'
                Workflow = $workflow.Id
                Message  = "Circular dependency check timeout - possible infinite loop in dependencies"
            }
        }
    }
    
    return $issues
}

#endregion

#region Main Execution

try {
    Write-Log "Starting Mermaid diagram generation from WORKFLOW_DATA blocks" -Level Info
    
    # Validate file exists
    if (-not (Test-Path $FilePath)) {
        Write-Log "File not found: $FilePath" -Level Error
        exit 1
    }
    
    $absolutePath = Resolve-Path $FilePath
    Write-Log "Processing file: $absolutePath" -Level Info
    
    # Read file content
    $content = Get-Content -Path $FilePath -Raw
    
    # Extract workflow data blocks
    $workflows = Get-WorkflowDataBlocks -Content $content
    
    if ($workflows.Count -eq 0) {
        Write-Log "No WORKFLOW_DATA blocks found in file" -Level Warning
        exit 0
    }
    
    Write-Log "Found $($workflows.Count) workflow(s)" -Level Info
    
    # Log workflow IDs for debugging
    foreach ($workflow in $workflows) {
        Write-Log "  - Workflow $($workflow.Id): $($workflow.Title)" -Level Info
    }
    
    # Validate workflow relationships
    $validationIssues = Test-WorkflowRelationships -Workflows $workflows
    
    if ($validationIssues.Count -gt 0) {
        Write-Log "Workflow relationship validation found $($validationIssues.Count) issue(s):" -Level Warning
        foreach ($issue in $validationIssues) {
            Write-Log "  [$($issue.Severity)] Workflow $($issue.Workflow): $($issue.Message)" -Level $issue.Severity
        }
        
        $errors = $validationIssues | Where-Object { $_.Severity -eq 'Error' }
        if ($errors.Count -gt 0) {
            Write-Log "Cannot proceed with $($errors.Count) error(s). Please fix workflow relationships." -Level Error
            exit 1
        }
    }
    
    if ($Validate) {
        Write-Log "Validation complete. Use without -Validate to generate diagrams." -Level Success
        exit 0
    }
    
    # Generate Mermaid diagrams
    $newContent = $content
    $insertions = @()
    
    foreach ($workflow in $workflows) {
        Write-Log "Generating diagram for workflow $($workflow.Id): $($workflow.Title)" -Level Info
        
        $mermaidCode = ConvertTo-MermaidSyntax -Workflow $workflow
        Write-Log "  Generated $($mermaidCode.Length) characters of Mermaid code" -Level Info
        
        # Find and replace the entire placeholder section (### header + note + comment)
        # This removes the note and comment, replacing them with the actual diagram
        $escapedId = [regex]::Escape($workflow.Id)
        $pattern = "(###\s+Workflow\s+$escapedId\s*:[^\r\n]*[\r\n]+)(?:[\r\n]+)?>\s+\*\*Note\*\*:[^\r\n]*[\r\n]+(?:[\r\n]+)?<!--\s+Mermaid diagram will be inserted here[^\r\n]*-->"
        
        Write-Log "  Searching for placeholder section for workflow $($workflow.Id)..." -Level Info
        $match = [regex]::Match($newContent, $pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
        
        if ($match.Success) {
            # Replace the entire match (note + comment) with just the header + diagram
            $replacement = $match.Groups[1].Value + "`n$mermaidCode"
            $newContent = $newContent.Substring(0, $match.Index) + $replacement + $newContent.Substring($match.Index + $match.Length)
            Write-Log "  Replaced placeholder with diagram for workflow $($workflow.Id)" -Level Success
        }
        else {
            Write-Log "Could not find placeholder section for workflow $($workflow.Id). Looking for: ### Workflow $($workflow.Id): ... with note" -Level Warning
        }
    }
    
    Write-Log "Diagram insertion complete" -Level Info
    
    # No need for separate insertions array - we're doing inline replacements above
    
    # Write updated content
    if (-not $DryRun) {
        Write-Log "Writing updated content to file..." -Level Info
        Set-Content -Path $FilePath -Value $newContent -NoNewline
        Write-Log "Successfully updated $FilePath with $($workflows.Count) diagram(s)" -Level Success
    }
    else {
        Write-Log "[DRY RUN] Would update $FilePath with $($workflows.Count) diagram(s)" -Level Info
    }
    
    # Summary
    Write-Log "Generation complete:" -Level Success
    Write-Log "  - Workflows processed: $($workflows.Count)" -Level Info
    Write-Log "  - Diagrams generated: $($workflows.Count)" -Level Info
    Write-Log "  - Validation issues: $($validationIssues.Count)" -Level Info
    
}
catch {
    Write-Log "Error during Mermaid generation: $($_.Exception.Message)" -Level Error
    Write-Log "Stack trace: $($_.ScriptStackTrace)" -Level Error
    exit 1
}

#endregion
