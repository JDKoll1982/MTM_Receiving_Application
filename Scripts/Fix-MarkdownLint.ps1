# Fix-MarkdownLint.ps1
# Fixes all markdownlint violations in the repository
# Follows PowerShell best practices per .github/instructions/powershell-scripting-ai.instructions.md

[CmdletBinding()]
param(
    [Parameter(ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
    [ValidateScript({ Test-Path $_ -PathType Container })]
    [string]$Path = (Get-Location).Path,
    
    [Parameter()]
    [ValidateSet('fix', 'check', 'list')]
    [string]$Mode = 'fix',
    
    [Parameter()]
    [string[]]$ExcludePatterns = @('.git', 'node_modules', '.vs', 'obj', 'bin')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Helper function to check if a tool is available
function Test-CommandAvailable {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$CommandName)
    $null -ne (Get-Command $CommandName -ErrorAction SilentlyContinue)
}

# Helper function to resolve repository root
function Get-RepositoryRoot {
    [CmdletBinding()]
    param([Parameter()][string]$StartPath = (Get-Location).Path)
    
    $current = $StartPath
    $maxDepth = 10
    $depth = 0
    
    while ($depth -lt $maxDepth) {
        if (Test-Path (Join-Path $current '.git' -Resolve -ErrorAction SilentlyContinue)) {
            return $current
        }
        $parent = Split-Path $current -Parent
        if ($parent -eq $current) { return $StartPath }
        $current = $parent
        $depth++
    }
    return $StartPath
}

# Helper function to build markdownlint arguments
function Get-MarkdownlintArgs {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][ValidateSet('fix', 'check', 'list')][string]$Mode,
        [Parameter()][string[]]$ExcludePatterns,
        [Parameter()][string]$ConfigPath
    )
    
    $args = @('markdownlint-cli')
    
    # Add configuration file if available
    if ($ConfigPath -and (Test-Path $ConfigPath)) {
        $args += '--config', $ConfigPath
        Write-Verbose "Using config file: $ConfigPath"
    }
    else {
        Write-Verbose "No config file found. Using comprehensive default rules."
    }
    
    # Add mode-specific argumentsd mode-specific arguments
    switch ($Mode) {
        'fix' { $args += '--fix'; Write-Verbose "Mode: Auto-fix all supported violations" }rite-Verbose "Mode: Auto-fix all supported violations" 
    }
    'check' { Write-Verbose "Mode: Check only" }'check' { Write-Verbose "Mode: Check only" }
    'list' { $args += '--list'; Write-Verbose "Mode: List violations" }s += '--list'; Write-Verbose "Mode: List violations" 
}
}
    
# Exclude directories# Exclude directories
foreach ($pattern in $ExcludePatterns) {
    Patterns) {
    $args += "--ignore=$pattern/**"    $args += "--ignore=$pattern/**"
}
    
# Include all markdown files
$args += '**/*.md'
return $argseturn $args
}

# Main execution
$exitCode = 1Code = 1
$output = @()put = @()
$errors = @()
$statusMessage = ""

try {
    # Validate npx
    if (-not (Test-CommandAvailable 'npx')) {
        not (Test-CommandAvailable 'npx')) {
        throw 'npx not found. Please install Node.js and npm.'x not found. Please install Node.js and npm.'
    }
    
    # Resolve pathsths
    $repoRoot = Get-RepositoryRoot -StartPath $PathyRoot -StartPath $Path
    $fullPath = (Resolve-Path $Path).Path
    
    Write-Verbose "Repository root: $repoRoot"rite-Verbose "Repository root: $repoRoot"
    Write-Verbose "Target path: $fullPath"Write-Verbose "Target path: $fullPath"
    Write-Verbose "Mode: $Mode"
    
    # Find config file
    $configPath = $null
    foreach ($candidate in @(oreach ($candidate in @(
        (Join-Path $repoRoot '.markdownlint.json'),    (Join-Path $repoRoot '.markdownlint.json'),
        (Join-Path $repoRoot '.markdownlint.jsonc'),jsonc'),
    (Join-Path $repoRoot '.markdownlint.yaml')poRoot '.markdownlint.yaml')
)) {
    if (Test-Path $candidate) {
        if (Test-Path $candidate) {
            $configPath = $candidatefigPath = $candidate
            break           break
        } }
}
    
# Count files before
$filesBeforeCount = @(Get-ChildItem -Path $fullPath -Filter '*.md' -Recurse -ErrorAction SilentlyContinue).Count $fullPath -Filter '*.md' -Recurse -ErrorAction SilentlyContinue).Count
    
# Get markdownlint args
$markdownlintArgs = Get-MarkdownlintArgs -Mode $Mode -ExcludePatterns $ExcludePatterns -ConfigPath $configPaths -Mode $Mode -ExcludePatterns $ExcludePatterns -ConfigPath $configPath
    
# Display header
Write-Host "`n" -ForegroundColor Cyanrite-Host "`n" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor CyanWrite-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "  Markdown Linting: $Mode" -ForegroundColor Cyanarkdown Linting: $Mode" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "  Path:       $fullPath" -ForegroundColor Gray-ForegroundColor Gray
Write-Host "  Mode:       $Mode" -ForegroundColor GrayWrite-Host "  Mode:       $Mode" -ForegroundColor Gray
Write-Host "  Files:      $filesBeforeCount markdown files found" -ForegroundColor GrayeCount markdown files found" -ForegroundColor Gray
    if ($configPath) {
        Write-Host "  Config:     $(Split-Path $configPath -Leaf)" -ForegroundColor Greenig:     $(Split-Path $configPath -Leaf)" -ForegroundColor Green
}
else {
    Write-Host "  Config:     Built-in rules" -ForegroundColor YellowForegroundColor Yellow
}
Write-Host "  Fixable:    Whitespace, headings, lists, code, urls, formatting" -ForegroundColor Cyanrite-Host "  Fixable:    Whitespace, headings, lists, code, urls, formatting" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor CyanWrite-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
    
# Execute markdownlint
Write-Verbose "Executing: npx $($markdownlintArgs -join ' ')"$markdownlintArgs -join ' ')"
    
    Push-Location $fullPathLocation $fullPath
    try {ry {
        & npx @markdownlintArgs 2>&1 | ForEach-Object {    & npx @markdownlintArgs 2>&1 | ForEach-Object {
            $output += $_
            if ($_ -match 'error|Error|ERROR') {
                $errors += $_
            }
        }    }
        $exitCode = $LASTEXITCODE
    }
    finally {finally {
        Pop-Location
    }
    
    # Count files after
    $filesAfterCount = @(Get-ChildItem -Path $fullPath -Filter '*.md' -Recurse -ErrorAction SilentlyContinue).CountlentlyContinue).Count
}
catch {
    $statusMessage = "Error: $($_.Exception.Message)"
    Write-Host "`n  ✗ $statusMessage" -ForegroundColor Red
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Cyan Cyan
    exit 1exit 1
}

# Display outputplay output
if ($output -and $Mode -ne 'check') {t -and $Mode -ne 'check') {
    Write-Host "`nProcessing Details:" -ForegroundColor Cyan
    Write-Host ($output -join "`n") -ForegroundColor Grayrite-Host ($output -join "`n") -ForegroundColor Gray
}

# Summary
Write-Host "`n" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyane-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan

if ($exitCode -eq 0) {
    $statusMessage = switch ($Mode) {$statusMessage = switch ($Mode) {
        'fix' { "All supported markdown violations fixed successfully!" } markdown violations fixed successfully!" }
'check' { "All markdown files pass validation!" } "All markdown files pass validation!" }
'list' { "Markdown violations listed successfully!" }"Markdown violations listed successfully!" }
}
Write-Host "  ✓ $statusMessage" -ForegroundColor GreentusMessage" -ForegroundColor Green
    Write-Host "  Files processed: $filesAfterCount" -ForegroundColor GreenWrite-Host "  Files processed: $filesAfterCount" -ForegroundColor Green
    Write-Host "  Rules applied: Whitespace, formatting, headings, lists, fences, code, urls" -ForegroundColor Greenlied: Whitespace, formatting, headings, lists, fences, code, urls" -ForegroundColor Green
}
else {
    if ($Mode -eq 'check') {
        Write-Host "  ⚠ Markdown validation issues found." -ForegroundColor Yellowarkdown validation issues found." -ForegroundColor Yellow
        Write-Host "  Total issues: $($errors.Count)" -ForegroundColor Yellowunt)" -ForegroundColor Yellow
        Write-Host "`n  To auto-fix all supported issues, run:" -ForegroundColor Yellowto-fix all supported issues, run:" -ForegroundColor Yellow
        Write-Host "  .\Fix-MarkdownLint.ps1 -Mode 'fix'" -ForegroundColor Yellow-Host "  .\Fix-MarkdownLint.ps1 -Mode 'fix'" -ForegroundColor Yellow
        Write-Host "`n  Fixable violations:" -ForegroundColor Cyanrite-Host "`n  Fixable violations:" -ForegroundColor Cyan
        Write-Host "  • Whitespace and trailing spaces" -ForegroundColor GrayWrite-Host "  • Whitespace and trailing spaces" -ForegroundColor Gray
        Write-Host "  • Multiple blank lines" -ForegroundColor Grayblank lines" -ForegroundColor Gray
        Write-Host "  • Heading formatting and spacing" -ForegroundColor GrayWrite-Host "  • Heading formatting and spacing" -ForegroundColor Gray
        Write-Host "  • List marker spacing and indentation" -ForegroundColor Grayacing and indentation" -ForegroundColor Gray
        Write-Host "  • Code fence formatting" -ForegroundColor Gray
        Write-Host "  • Hard tabs" -ForegroundColor Gray   Write-Host "  • Hard tabs" -ForegroundColor Gray
    }
    else {
        Write-Host "  ✗ Some issues could not be auto-fixed." -ForegroundColor Yellow   Write-Host "  ✗ Some issues could not be auto-fixed." -ForegroundColor Yellow
        Write-Host "  Auto-fixed: $($output.Count) | Manual review: $($errors.Count)" -ForegroundColor Yellow       Write-Host "  Auto-fixed: $($output.Count) | Manual review: $($errors.Count)" -ForegroundColor Yellow
        if ($errors) { if ($errors) {
            Write-Host "`n  Issues requiring manual intervention:" -ForegroundColor YellowHost "`n  Issues requiring manual intervention:" -ForegroundColor Yellow
            $errors | Select-Object -First 10 | ForEach-Object {ject -First 10 | ForEach-Object {
                Write-Host "    $_" -ForegroundColor Yellow
            }
            if ($errors.Count -gt 10) {
                Write-Host "    ... and $($errors.Count - 10) more" -ForegroundColor Yellow           Write-Host "    ... and $($errors.Count - 10) more" -ForegroundColor Yellow
            }        }
        }
    }
}

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Cyan━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Cyan

        exit $exitCode
