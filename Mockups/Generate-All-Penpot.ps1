# Generate-All-Penpot.ps1
# Generates Penpot files for all XAML views in the project

param(
    [string]$ViewsPath = "..\Views",
    [switch]$CleanFirst
)

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "  Penpot Batch Generator" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Ensure Penpot-Before folder exists
$outputFolder = "Penpot-Before"
if (-not (Test-Path $outputFolder)) {
    New-Item -ItemType Directory -Path $outputFolder -Force | Out-Null
    Write-Host "Created output folder: $outputFolder" -ForegroundColor Green
}

# Clean existing penpot files if requested
if ($CleanFirst) {
    Write-Host "Cleaning existing .penpot files in $outputFolder..." -ForegroundColor Yellow
    Remove-Item "$outputFolder\*.penpot" -Force -ErrorAction SilentlyContinue
    Write-Host "Done." -ForegroundColor Green
    Write-Host ""
}

# Find all XAML files
$xamlFiles = Get-ChildItem -Path $ViewsPath -Filter "*.xaml" -Recurse -File
$totalFiles = $xamlFiles.Count
$successCount = 0
$failCount = 0

Write-Host "Found $totalFiles XAML files" -ForegroundColor Cyan
Write-Host ""

foreach ($xamlFile in $xamlFiles) {
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($xamlFile.Name)
    $outputFile = Join-Path $outputFolder "$baseName.penpot"
    
    Write-Host "[$($successCount + $failCount + 1)/$totalFiles] Processing: $($xamlFile.Name)..." -NoNewline
    
    try {
        .\Generate-Penpot.ps1 -XamlFile $xamlFile.FullName -OutputFile $outputFile 2>&1 | Out-Null
        
        if (Test-Path $outputFile) {
            Write-Host " OK" -ForegroundColor Green
            $successCount++
        }
        else {
            Write-Host " FAILED (File not created)" -ForegroundColor Red
            $failCount++
        }
    }
    catch {
        Write-Host " ERROR: $($_.Exception.Message)" -ForegroundColor Red
        $failCount++
    }
}

Write-Host ""
Write-Host "====================================" -ForegroundColor Cyan
Write-Host "  Summary" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host "Total files:    $totalFiles" -ForegroundColor White
Write-Host "Successful:     $successCount" -ForegroundColor Green
if ($failCount -eq 0) {
    Write-Host "Failed:         $failCount" -ForegroundColor Green
} else {
    Write-Host "Failed:         $failCount" -ForegroundColor Red
}
Write-Host ""

if ($successCount -gt 0) {
    Write-Host "Generated .penpot files are ready to import!" -ForegroundColor Yellow
}
