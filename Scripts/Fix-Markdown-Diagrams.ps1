# Fix-MarkdownDiagrams.ps1
# Scans markdown files for code blocks missing a language tag that look like diagrams.

$TargetFiles = Get-ChildItem -Recurse -Filter "*.md"

foreach ($File in $TargetFiles) {
    Write-Host "Processing $($File.FullName)..." -ForegroundColor Cyan
    
    # Read file content as a single string
    $Content = Get-Content -Path $File.FullName -Raw
    
    # Regex Breakdown:
    # (?m)        : Multi-line mode
    # ^```\r?\n   : Matches opening backticks with NO language tag (start of line)
    # ([\s\S]*?)  : Captures block content (non-greedy)
    # (?=```)     : Lookahead for the closing backticks
    # Condition   : Only replace if content contains diagram arrows (→, ↓, etc.)
    
    $NewContent = [regex]::Replace($Content, '(?m)^```\r?\n([\s\S]*?)(?=^```)', {
            param($Match)
            $BlockBody = $Match.Groups[1].Value
        
            # Define diagram indicators (arrows, boxes, etc.)
            if ($BlockBody -match '[→↓←↑➔]|--|==>|-->') {
                return "```text`n$BlockBody"
            }
            return $Match.Value
        })

    if ($Content -ne $NewContent) {
        Set-Content -Path $File.FullName -Value $NewContent -NoNewline
        Write-Host "  Fixed diagram in $($File.Name)" -ForegroundColor Green
    }
}