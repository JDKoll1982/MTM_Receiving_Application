<#
.SYNOPSIS
    Validates Mermaid diagrams against quality rules

.DESCRIPTION
    Checks Mermaid diagrams for common issues:
    - Duplicate node IDs within a diagram
    - Missing namespace prefixes (in multi-diagram files)
    - Missing decision branch labels
    - Missing unique end node suffixes
    - Syntax errors
#>

function Test-MermaidDiagrams {
    <#
    .SYNOPSIS
        Validates all diagrams against quality rules
    
    .PARAMETER Diagrams
        Array of diagram objects to validate
    
    .PARAMETER Rules
        Hashtable of validation rules to apply
    
    .OUTPUTS
        Array of validation results
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [array]$Diagrams,
        
        [Parameter(Mandatory = $false)]
        [hashtable]$Rules = @{
            RequireUniqueNodeIds   = $true
            RequireNamespacePrefix = $true
            RequireDecisionLabels  = $true
            RequireEndNodeSuffixes = $true
        }
    )
    
    $results = @()
    
    foreach ($diagram in $Diagrams) {
        $errors = @()
        $warnings = @()
        
        Write-Verbose "Validating Workflow $($diagram.WorkflowId): $($diagram.Title)"
        
        # Get all nodes
        $nodes = Get-MermaidNodes -DiagramContent $diagram.Content
        
        # Rule 1: Check for duplicate node IDs
        if ($Rules.RequireUniqueNodeIds) {
            $duplicates = $nodes | Group-Object | Where-Object { $_.Count -gt 1 } | Select-Object -ExpandProperty Name
            if ($duplicates) {
                $errors += "Duplicate node IDs: $($duplicates -join ', ')"
            }
        }
        
        # Rule 2: Check for namespace prefixes (only if multiple diagrams exist)
        if ($Rules.RequireNamespacePrefix -and $Diagrams.Count -gt 1) {
            $nodesWithoutNamespace = $nodes | Where-Object { -not (Test-NodeHasNamespace -NodeId $_) }
            if ($nodesWithoutNamespace.Count -gt 0) {
                $warnings += "$($nodesWithoutNamespace.Count) nodes missing namespace prefix"
            }
        }
        
        # Rule 3: Check for decision branch labels
        if ($Rules.RequireDecisionLabels) {
            # Look for decision nodes: {Question?}
            $decisionMatches = [regex]::Matches($diagram.Content, '\{[^}]+\}')
            foreach ($match in $decisionMatches) {
                $decisionNode = $match.Value
                # Check if there are labeled branches after this decision
                $lines = $diagram.Content -split "`n"
                $hasLabels = $false
                foreach ($line in $lines) {
                    if ($line -match [regex]::Escape($decisionNode) -and $line -match '\|\w+\|') {
                        $hasLabels = $true
                        break
                    }
                }
                if (-not $hasLabels) {
                    $warnings += "Decision node $decisionNode may be missing branch labels"
                }
            }
        }
        
        # Rule 4: Check for unique end node suffixes
        if ($Rules.RequireEndNodeSuffixes) {
            # Find end nodes (nodes with stadium shape: ([...]))
            $endNodeMatches = [regex]::Matches($diagram.Content, '(\w+)\s*\(\[')
            $endNodes = $endNodeMatches | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
            
            # Check if multiple end nodes exist without unique suffixes
            if ($endNodes.Count -gt 1) {
                $genericEndNodes = $endNodes | Where-Object { $_ -match '^(W\d+_\d+_)?End$' }
                if ($genericEndNodes.Count -gt 1) {
                    $errors += "Multiple end nodes with generic 'End' ID found - use suffixes (SuccessEnd, ErrorEnd, etc.)"
                }
            }
        }
        
        # Create validation result
        $result = @{
            WorkflowId = $diagram.WorkflowId
            Title      = $diagram.Title
            IsValid    = ($errors.Count -eq 0)
            Errors     = $errors
            Warnings   = $warnings
            NodeCount  = $nodes.Count
        }
        
        $results += New-Object PSObject -Property $result
    }
    
    return $results
}

function Test-MermaidSyntax {
    <#
    .SYNOPSIS
        Validates Mermaid syntax using basic pattern matching
    
    .PARAMETER DiagramContent
        The Mermaid diagram content
    
    .OUTPUTS
        Validation result object
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$DiagramContent
    )
    
    $errors = @()
    
    # Check for flowchart declaration
    if ($DiagramContent -notmatch '^\s*flowchart\s+(TD|LR|TB|RL)') {
        $errors += "Missing or invalid flowchart direction (TD, LR, TB, or RL)"
    }
    
    # Check for unclosed brackets
    $openBrackets = ([regex]::Matches($DiagramContent, '\[')).Count
    $closeBrackets = ([regex]::Matches($DiagramContent, '\]')).Count
    if ($openBrackets -ne $closeBrackets) {
        $errors += "Mismatched square brackets: $openBrackets open, $closeBrackets close"
    }
    
    $openParens = ([regex]::Matches($DiagramContent, '\(')).Count
    $closeParens = ([regex]::Matches($DiagramContent, '\)')).Count
    if ($openParens -ne $closeParens) {
        $errors += "Mismatched parentheses: $openParens open, $closeParens close"
    }
    
    $openBraces = ([regex]::Matches($DiagramContent, '\{')).Count
    $closeBraces = ([regex]::Matches($DiagramContent, '\}')).Count
    if ($openBraces -ne $closeBraces) {
        $errors += "Mismatched curly braces: $openBraces open, $closeBraces close"
    }
    
    # Check for invalid characters in node IDs
    $invalidNodeIds = [regex]::Matches($DiagramContent, '(\w+-\w+)\s*[\[\(\{]')
    if ($invalidNodeIds.Count -gt 0) {
        $errors += "Node IDs with hyphens detected (use underscores instead): $($invalidNodeIds.Value -join ', ')"
    }
    
    return @{
        IsValid = ($errors.Count -eq 0)
        Errors  = $errors
    }
}

