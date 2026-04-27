# Embed-JSONData.ps1
# Embeds all JSON question data directly into the HTML file to avoid CORS issues

$htmlPath = ".\generated\questionnaire.html"
$questionsPath = ".\questions"

# Read all JSON files
$coreJson = Get-Content "$questionsPath\core.json" -Raw
$productionJson = Get-Content "$questionsPath\production.json" -Raw
$materialHandlingJson = Get-Content "$questionsPath\material-handling.json" -Raw
$itDepartmentJson = Get-Content "$questionsPath\it-department.json" -Raw
$managementJson = Get-Content "$questionsPath\management.json" -Raw
$maintenanceJson = Get-Content "$questionsPath\maintenance.json" -Raw
$qualityControlJson = Get-Content "$questionsPath\quality-control.json" -Raw
$dieShopJson = Get-Content "$questionsPath\die-shop.json" -Raw
$fabricationWeldingJson = Get-Content "$questionsPath\fabrication-welding.json" -Raw
$assemblyJson = Get-Content "$questionsPath\assembly.json" -Raw
$setupTechniciansJson = Get-Content "$questionsPath\setup-technicians.json" -Raw

# Read the HTML file
$html = Get-Content $htmlPath -Raw

# Create the embedded data block
$embeddedData = @"
    <script>
        // Embedded question data (no fetch needed - avoids CORS issues)
        const EMBEDDED_DATA = {
            core: $coreJson,
            "production": $productionJson,
            "material-handling": $materialHandlingJson,
            "it-department": $itDepartmentJson,
            "management": $managementJson,
            "maintenance": $maintenanceJson,
            "quality-control": $qualityControlJson,
            "die-shop": $dieShopJson,
            "fabrication-welding": $fabricationWeldingJson,
            "assembly": $assemblyJson,
            "setup-technicians": $setupTechniciansJson
        };
    </script>
    <script>
"@

# Replace the opening script tag with embedded data + script tag
$html = $html -replace '<script>', $embeddedData

# Save the updated HTML
$html | Set-Content $htmlPath -NoNewline

Write-Host "✅ JSON data embedded successfully!" -ForegroundColor Green
Write-Host "HTML file updated: $htmlPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "You can now open the HTML file directly without a web server!" -ForegroundColor Yellow
