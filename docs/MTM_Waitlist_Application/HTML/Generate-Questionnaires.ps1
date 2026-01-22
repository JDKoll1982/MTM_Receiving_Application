<#
.SYNOPSIS
    Generate standalone HTML questionnaires from JSON data files.

.DESCRIPTION
    This script reads JSON questionnaire data files and embeds them into an HTML template,
    creating self-contained HTML files that can be opened directly from disk without
    needing a web server (no file:// CORS issues).

.PARAMETER DataFile
    Path to a specific JSON file to generate HTML for. If not specified, processes all JSON files in the data folder.

.PARAMETER OutputFolder
    Output folder for generated HTML files. Defaults to 'generated' subfolder.

.EXAMPLE
    .\Generate-Questionnaires.ps1
    Generates HTML files for all JSON files in the data folder.

.EXAMPLE
    .\Generate-Questionnaires.ps1 -DataFile "data\core-all-roles.json"
    Generates HTML file for a specific questionnaire.
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$DataFile,
    
    [Parameter()]
    [string]$OutputFolder = "generated"
)

$ErrorActionPreference = 'Stop'

# Get script directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$templatePath = Join-Path $scriptPath "wizard-template.html"
$dataFolder = Join-Path $scriptPath "data"
$outputPath = Join-Path $scriptPath $OutputFolder

# Create output folder if it doesn't exist
if (-not (Test-Path $outputPath)) {
    New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
    Write-Host "Created output folder: $outputPath" -ForegroundColor Green
}

# Verify template exists
if (-not (Test-Path $templatePath)) {
    throw "Template file not found: $templatePath"
}

# Read template
Write-Host "Reading template: $templatePath" -ForegroundColor Cyan
$template = Get-Content -Path $templatePath -Raw

# Get JSON files to process
if ($DataFile) {
    $jsonFiles = @(Get-Item $DataFile)
}
else {
    if (-not (Test-Path $dataFolder)) {
        throw "Data folder not found: $dataFolder"
    }
    $jsonFiles = Get-ChildItem -Path $dataFolder -Filter "*.json"
}

if ($jsonFiles.Count -eq 0) {
    Write-Warning "No JSON files found to process."
    exit
}

Write-Host "`nProcessing $($jsonFiles.Count) file(s)...`n" -ForegroundColor Cyan

foreach ($jsonFile in $jsonFiles) {
    try {
        Write-Host "Processing: $($jsonFile.Name)" -ForegroundColor Yellow
        
        # Read and validate JSON
        $jsonContent = Get-Content -Path $jsonFile.FullName -Raw
        $jsonData = $jsonContent | ConvertFrom-Json
        
        # Validate required fields
        if (-not $jsonData.title) {
            Write-Warning "  Skipping - JSON file missing 'title' field"
            continue
        }
        
        if (-not $jsonData.questions) {
            Write-Warning "  Skipping - JSON file missing 'questions' array"
            continue
        }
        
        # Create JavaScript variable assignment
        $jsDataBlock = "const QUESTIONNAIRE_DATA = $jsonContent;"
        
        # Replace placeholder in template
        $html = $template -replace '// %%JSON_DATA_PLACEHOLDER%%', $jsDataBlock
        
        # Generate output filename
        $baseName = [System.IO.Path]::GetFileNameWithoutExtension($jsonFile.Name)
        $outputFile = Join-Path $outputPath "$baseName.html"
        
        # Write output file
        $html | Out-File -FilePath $outputFile -Encoding UTF8 -Force
        
        Write-Host "  Generated: $outputFile" -ForegroundColor Green
        Write-Host "    Title: $($jsonData.title)" -ForegroundColor Gray
        Write-Host "    Questions: $($jsonData.questions.Count)" -ForegroundColor Gray
        
    }
    catch {
        Write-Error "  Failed to process $($jsonFile.Name): $_"
    }
}

Write-Host ""
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host "Complete" -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Files in: $outputPath" -ForegroundColor White
Write-Host "Open HTML files directly in browser" -ForegroundColor White
Write-Host ""
