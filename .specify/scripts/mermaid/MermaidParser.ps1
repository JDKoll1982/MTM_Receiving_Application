<#
.SYNOPSIS
    Parses Mermaid diagrams from markdown files

.DESCRIPTION
    Extracts Mermaid diagrams with their metadata (User Story, Workflow ID, Title)
    from markdown specification files.

.NOTES
    Expected format:
    ### Workflow {UserStory}.{WorkflowNum}: {Title}
    
    ```mermaid
    {diagram content}
    ```
#>

function Get-MermaidDiagrams {
    <#
    .SYNOPSIS
        Extracts all Mermaid diagrams from a markdown file
    
    .PARAMETER FilePath
        Path to the markdown file
    
    .OUTPUTS
        Array of hashtables containing diagram metadata and content
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath
    )
    
    $content = Get-Content -Path $FilePath -Raw
    $diagrams = @()
    
    # Regex pattern to match workflow headers and Mermaid blocks
    # Matches: ### Workflow 1.1: Title
    $workflowPattern = '###\s+Workflow\s+(\d+)\.(\d+):\s+([^\n]+)'
    
    # Split content into lines for processing
    $lines = $content -split "`n"
    $i = 0
    
    while ($i -lt $lines.Count) {
        $line = $lines[$i]
        
        # Check if line matches workflow header
        if ($line -match $workflowPattern) {
            $userStory = $Matches[1]
            $workflowNum = $Matches[2]
            $title = $Matches[3].Trim()
            
            # Look for mermaid block after header
            $j = $i + 1
            $mermaidStart = -1
            $mermaidEnd = -1
            
            # Skip empty lines and find ```mermaid
            while ($j -lt $lines.Count -and $j -lt ($i + 10)) {
                if ($lines[$j] -match '^\s*```mermaid\s*$') {
                    $mermaidStart = $j
                    break
                }
                $j++
            }
            
            # Find closing ```
            if ($mermaidStart -ne -1) {
                $j = $mermaidStart + 1
                while ($j -lt $lines.Count) {
                    if ($lines[$j] -match '^\s*```\s*$') {
                        $mermaidEnd = $j
                        break
                    }
                    $j++
                }
            }
            
            # Extract diagram content
            if ($mermaidStart -ne -1 -and $mermaidEnd -ne -1) {
                $diagramContent = $lines[($mermaidStart + 1)..($mermaidEnd - 1)] -join "`n"
                
                $diagram = @{
                    UserStory       = [int]$userStory
                    WorkflowNum     = [int]$workflowNum
                    WorkflowId      = "$userStory.$workflowNum"
                    Title           = $title
                    Content         = $diagramContent
                    LineStart       = $mermaidStart
                    LineEnd         = $mermaidEnd
                    HeaderLine      = $i
                    OriginalContent = $lines[$mermaidStart..$mermaidEnd] -join "`n"
                }
                
                $diagrams += $diagram
                
                # Skip past this diagram
                $i = $mermaidEnd
            }
        }
        
        $i++
    }
    
    Write-Verbose "Parsed $($diagrams.Count) Mermaid diagrams"
    return $diagrams
}

function Get-MermaidNodes {
    <#
    .SYNOPSIS
        Extracts all node definitions from a Mermaid diagram
    
    .PARAMETER DiagramContent
        The Mermaid diagram content
    
    .OUTPUTS
        Array of node IDs found in the diagram
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$DiagramContent
    )
    
    $nodes = @()
    
    # Patterns for node definitions
    # Matches: NodeId[Label], NodeId([Label]), NodeId{Label}, NodeId[[Label]]
    $nodePatterns = @(
        '(\w+)\s*\[',          # NodeId[
        '(\w+)\s*\(',          # NodeId(
        '(\w+)\s*\{',          # NodeId{
        '(\w+)\s*\[\[',        # NodeId[[
        '(\w+)\s*-->'          # NodeId -->
    )
    
    foreach ($pattern in $nodePatterns) {
        $matches = [regex]::Matches($DiagramContent, $pattern)
        foreach ($match in $matches) {
            $nodeId = $match.Groups[1].Value
            if ($nodeId -and $nodeId -notin $nodes) {
                $nodes += $nodeId
            }
        }
    }
    
    Write-Verbose "Found $($nodes.Count) unique nodes"
    return $nodes
}

function Test-NodeHasNamespace {
    <#
    .SYNOPSIS
        Checks if a node ID already has a namespace prefix
    
    .PARAMETER NodeId
        The node ID to check
    
    .OUTPUTS
        Boolean indicating if namespace prefix exists
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$NodeId
    )
    
    # Check if matches W{num}_{num}_ pattern
    return $NodeId -match '^W\d+_\d+_'
}

function Remove-AllMermaidDiagrams {
    <#
    .SYNOPSIS
        Removes all Mermaid diagrams from a markdown file
    
    .PARAMETER FilePath
        Path to the markdown file
    
    .PARAMETER PreserveHeaders
        If true, keeps the workflow headers (### Workflow X.Y: Title)
        If false, removes both headers and diagrams
    
    .OUTPUTS
        Updated file content with diagrams removed
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,
        
        [Parameter(Mandatory = $false)]
        [switch]$PreserveHeaders = $false
    )
    
    $content = Get-Content -Path $FilePath -Raw
    $diagrams = Get-MermaidDiagrams -FilePath $FilePath
    
    Write-Verbose "Found $($diagrams.Count) Mermaid diagrams to remove"
    
    # Process in reverse order to maintain line numbers
    $diagrams = $diagrams | Sort-Object -Property LineStart -Descending
    
    foreach ($diagram in $diagrams) {
        $lines = $content -split "`n"
        
        if ($PreserveHeaders) {
            # Remove only the mermaid code block, keep the header
            $beforeDiagram = $lines[0..($diagram.LineStart - 1)]
            $afterDiagram = $lines[($diagram.LineEnd + 1)..($lines.Count - 1)]
            
            $lines = $beforeDiagram + $afterDiagram
        }
        else {
            # Remove both header and diagram
            $beforeHeader = $lines[0..($diagram.HeaderLine - 1)]
            $afterDiagram = $lines[($diagram.LineEnd + 1)..($lines.Count - 1)]
            
            $lines = $beforeHeader + $afterDiagram
        }
        
        $content = $lines -join "`n"
    }
    
    Write-Verbose "Removed $($diagrams.Count) Mermaid diagrams"
    return $content
}
