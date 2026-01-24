<#
.SYNOPSIS
    Applies namespace prefixes to Mermaid diagram nodes

.DESCRIPTION
    Adds W{UserStory}_{Workflow}_ prefix to all node IDs in Mermaid diagrams
    to prevent conflicts when multiple diagrams exist in the same file.
#>

function Apply-MermaidNamespaces {
    <#
    .SYNOPSIS
        Applies namespace prefixes to all diagrams in a file
    
    .PARAMETER Diagrams
        Array of diagram objects from Get-MermaidDiagrams
    
    .PARAMETER SourceFile
        Path to the source markdown file
    
    .OUTPUTS
        Updated file content with namespaced diagrams
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [array]$Diagrams,
        
        [Parameter(Mandatory = $true)]
        [string]$SourceFile
    )
    
    $content = Get-Content -Path $SourceFile -Raw
    
    foreach ($diagram in $Diagrams) {
        $prefix = "W$($diagram.UserStory)_$($diagram.WorkflowNum)_"
        
        Write-Verbose "Processing Workflow $($diagram.WorkflowId): $($diagram.Title)"
        
        # Get all nodes in the diagram
        $nodes = Get-MermaidNodes -DiagramContent $diagram.Content
        
        # Filter out nodes that already have namespace
        $nodesToUpdate = $nodes | Where-Object { -not (Test-NodeHasNamespace -NodeId $_) }
        
        if ($nodesToUpdate.Count -eq 0) {
            Write-Verbose "  Skipping - all nodes already namespaced"
            continue
        }
        
        Write-Verbose "  Applying prefix '$prefix' to $($nodesToUpdate.Count) nodes"
        
        # Apply namespace to diagram content
        $updatedDiagram = $diagram.Content
        
        # Sort nodes by length (longest first) to avoid partial replacements
        $sortedNodes = $nodesToUpdate | Sort-Object -Property Length -Descending
        
        foreach ($node in $sortedNodes) {
            $newNode = "$prefix$node"
            
            # Replace all occurrences of the node ID
            # Use word boundaries to avoid partial matches
            $patterns = @(
                # Node definitions: NodeId[, NodeId(, NodeId{, NodeId[[
                "(\b)$node(\s*[\[\(\{])",
                # Arrows: NodeId -->, --> NodeId
                "(\b)$node(\s*-->)",
                "(-->\s*)$node(\b)"
            )
            
            foreach ($pattern in $patterns) {
                $updatedDiagram = $updatedDiagram -replace $pattern, "`$1$newNode`$2"
            }
        }
        
        # Replace the diagram in the content
        $content = $content.Replace($diagram.Content, $updatedDiagram)
    }
    
    return $content
}

function Get-NamespacePrefix {
    <#
    .SYNOPSIS
        Generates namespace prefix for a workflow
    
    .PARAMETER UserStory
        User story number
    
    .PARAMETER WorkflowNum
        Workflow number within the user story
    
    .OUTPUTS
        Namespace prefix string (e.g., "W1_2_")
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [int]$UserStory,
        
        [Parameter(Mandatory = $true)]
        [int]$WorkflowNum
    )
    
    return "W${UserStory}_${WorkflowNum}_"
}

function Remove-MermaidNamespaces {
    <#
    .SYNOPSIS
        Removes namespace prefixes from Mermaid diagrams (rollback operation)
    
    .PARAMETER Diagrams
        Array of diagram objects
    
    .PARAMETER SourceFile
        Path to the source markdown file
    
    .OUTPUTS
        Updated file content with namespaces removed
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [array]$Diagrams,
        
        [Parameter(Mandatory = $true)]
        [string]$SourceFile
    )
    
    $content = Get-Content -Path $SourceFile -Raw
    
    foreach ($diagram in $Diagrams) {
        $prefix = "W$($diagram.UserStory)_$($diagram.WorkflowNum)_"
        
        Write-Verbose "Removing prefix '$prefix' from Workflow $($diagram.WorkflowId)"
        
        # Simple find and replace - remove all instances of the prefix
        $updatedDiagram = $diagram.Content -replace $prefix, ''
        
        # Replace in content
        $content = $content.Replace($diagram.Content, $updatedDiagram)
    }
    
    return $content
}
