<#
.SYNOPSIS
    Runs the build and generates a DOT graph of warnings/errors (Pure PowerShell).
.DESCRIPTION
    This script runs 'dotnet build', parses the output, 
    and generates a 'build-issues.dot' file.
    If Graphviz 'dot' is installed, it also renders a PNG.
#>

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$DotFile = Join-Path $ScriptDir "..\build-issues.dot"
$PngFile = Join-Path $ScriptDir "..\build-issues.png"

Write-Host "Running dotnet build..." -ForegroundColor Cyan
$BuildOutput = dotnet build -clp:NoSummary 2>&1

# Parse Output
$Issues = @()
# Regex for MSBuild warning/error: File(Line,Col): Level Code: Message [Project]
$Pattern = "^\s*(?<file>.*)\((?<line>\d+),(?<col>\d+)\):\s+(?<level>warning|error)\s+(?<code>\w+):\s+(?<msg>.*)\s+\[.*\]"

foreach ($Line in $BuildOutput) {
    if ($Line -match $Pattern) {
        $Issues += [PSCustomObject]@{
            File  = $Matches.file
            Level = $Matches.level
            Code  = $Matches.code
            Msg   = $Matches.msg
        }
    }
}

Write-Host "Found $($Issues.Count) issues." -ForegroundColor Cyan

# Generate DOT
$DotContent = @("digraph BuildIssues {")
$DotContent += "  rankdir=LR;"
$DotContent += '  node [shape=box, style=filled, fontname="Segoe UI"];'
$DotContent += '  edge [fontname="Segoe UI", fontsize=10];'
$DotContent += "  ROOT [label=`"Build Issues\nTotal: $($Issues.Count)`", fillcolor=`"lightblue`", shape=doubleoctagon];"

if ($Issues.Count -eq 0) {
    $DotContent += '  "Good Job!" [shape=box, style=filled, fillcolor=green];'
} else {
    # Group by File
    $FileGroups = $Issues | Group-Object File
    
    foreach ($Group in $FileGroups) {
        $FileName = Split-Path $Group.Name -Leaf
        $FileId = """$FileName"""
        $Total = $Group.Count
        
        # Determine color
        $HasError = $Group.Group | Where-Object { $_.Level -eq 'error' }
        $Color = if ($HasError) { "lightpink" } else { "lightyellow" }
        
        $DotContent += "  $FileId [label=`"$FileName\n($Total)`", fillcolor=`"$Color`"];"
        $DotContent += "  ROOT -> $FileId;"
        
        # Group by Code within File
        $CodeGroups = $Group.Group | Group-Object Code
        foreach ($CodeGroup in $CodeGroups) {
            $Code = $CodeGroup.Name
            $Count = $CodeGroup.Count
            $Level = $CodeGroup.Group[0].Level
            
            $CodeId = """${FileName}_${Code}"""
            $CodeColor = if ($Level -eq 'warning') { "orange" } else { "red" }
            
            $DotContent += "  $CodeId [label=`"$Code`", fillcolor=`"$CodeColor`", shape=ellipse];"
            $DotContent += "  $FileId -> $CodeId [label=`"$Count`"];"
        }
    }
}

$DotContent += "}"
$DotContent | Set-Content -Path $DotFile -Encoding UTF8

Write-Host "DOT file generated at: $DotFile" -ForegroundColor Green

# Render if dot is available
if (Get-Command "dot" -ErrorAction SilentlyContinue) {
    Write-Host "Rendering to PNG..." -ForegroundColor Cyan
    dot -Tpng $DotFile -o $PngFile
    Write-Host "Graph image generated at: $PngFile" -ForegroundColor Green
    Invoke-Item $PngFile
} else {
    Write-Warning "Graphviz 'dot' tool not found. Install it to render PNG images."
    Write-Host "You can view the DOT file online at https://dreampuf.github.io/GraphvizOnline/" -ForegroundColor Gray
}
