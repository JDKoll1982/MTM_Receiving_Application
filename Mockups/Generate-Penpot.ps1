# XAML-to-Penpot.ps1
# Generates a valid .penpot file from XAML UI definitions

param(
    [Parameter(Mandatory=$false)]
    [string]$XamlFile,
    
    [Parameter(Mandatory=$false)]
    [string]$OutputFile
)

# Load required assemblies
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

# If no XAML file specified, show available XAML files
if (-not $XamlFile) {
    Write-Host "Available XAML files:" -ForegroundColor Cyan
    $xamlFiles = Get-ChildItem -Path "..\Views" -Filter "*.xaml" -Recurse -File
    $xamlFiles | ForEach-Object { Write-Host "  $($_.FullName)" }
    
    if ($xamlFiles.Count -eq 0) {
        Write-Host "No XAML files found in Views folder." -ForegroundColor Yellow
        exit
    }
    
    Write-Host "`nUsage: .\Generate-Penpot.ps1 -XamlFile <path> [-OutputFile <name>]" -ForegroundColor Yellow
    exit
}

# Validate XAML file exists
if (-not (Test-Path $XamlFile)) {
    Write-Host "Error: XAML file not found: $XamlFile" -ForegroundColor Red
    exit
}

# Auto-generate output file name if not specified
if (-not $OutputFile) {
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($XamlFile)
    $OutputFile = "$baseName.penpot"
}

Write-Host "Generating Penpot file from XAML..." -ForegroundColor Cyan
Write-Host "  Input:  $XamlFile" -ForegroundColor Gray
Write-Host "  Output: $OutputFile" -ForegroundColor Gray
Write-Host ""

# Generate UUIDs
function New-UUID {
    [guid]::NewGuid().ToString()
}

# Safe JSON file writer with validation
function Write-JsonFileSafe {
    param(
        [string]$FilePath,
        [string]$JsonContent
    )
    
    try {
        # Write with UTF-8 encoding (no BOM)
        $utf8NoBom = New-Object System.Text.UTF8Encoding $false
        [System.IO.File]::WriteAllText($FilePath, $JsonContent, $utf8NoBom)
        
        # Verify file was written
        if (-not (Test-Path $FilePath)) {
            throw "File was not created: $FilePath"
        }
        
        $fileSize = (Get-Item $FilePath).Length
        if ($fileSize -eq 0) {
            Write-Host "ERROR: Generated JSON is empty for $FilePath" -ForegroundColor Red
            Write-Host "JSON Content (first 500 chars): $($JsonContent.Substring(0, [Math]::Min(500, $JsonContent.Length)))" -ForegroundColor Yellow
            throw "File is empty: $FilePath"
        }
        
        # Validate JSON after writing
        try {
            $content = Get-Content $FilePath -Raw
            $null = ConvertFrom-Json $content -ErrorAction Stop
        }
        catch {
            Write-Host "ERROR: Invalid JSON generated for $FilePath" -ForegroundColor Red
            Write-Host "Parse Error: $($_.Exception.Message)" -ForegroundColor Yellow
            throw "JSON validation failed: $($_.Exception.Message)"
        }
    }
    catch {
        Write-Host "ERROR writing JSON file $FilePath : $($_.Exception.Message)" -ForegroundColor Red
        throw
    }
}

# Import all converters
$converterPath = Join-Path $PSScriptRoot "Converters"
if (Test-Path $converterPath) {
    Write-Host "Loading converters from: $converterPath" -ForegroundColor Gray
    $loadedConverters = 0
    Get-ChildItem "$converterPath\*.ps1" | ForEach-Object {
        . $_.FullName
        $loadedConverters++
        Write-Host "  Loaded: $($_.Name)" -ForegroundColor DarkGray
    }
    Write-Host "Loaded $loadedConverters converter functions" -ForegroundColor Green
}
else {
    Write-Host "WARNING: Converters folder not found at $converterPath" -ForegroundColor Yellow
}

# Parse XAML file and extract UI elements
function Get-XamlElements {
    param([string]$xamlPath)
    
    try {
        [xml]$xaml = Get-Content $xamlPath -Raw
        
        function Get-SafeInt {
            param($val, $default = 0)
            if ([string]::IsNullOrEmpty($val)) { return $default }
            if ($val -match "^\{") { return $default } # Binding
            if ($val -as [int]) { return [int]$val }
            return $default
        }

        # Extract root element properties
        $root = $xaml.DocumentElement
        $windowWidth = 1400
        $windowHeight = 900
        
        # Try to get actual dimensions
        $windowWidth = Get-SafeInt $root.Width 1400
        $windowHeight = Get-SafeInt $root.Height 900
        
        $elements = @{
            Width = $windowWidth
            Height = $windowHeight
            Elements = @()
        }
        
        # Parse all UI elements with layout estimation
        $currentY = 40
        $currentX = 40
        $spacing = 12
        
        # Parse Buttons
        $buttons = $root.SelectNodes("//*[local-name()='Button']")
        foreach ($button in $buttons) {
            $btnData = @{
                Type = "Button"
                Name = if ($button.GetAttribute("x:Name")) { $button.GetAttribute("x:Name") } else { "Button" }
                Content = if ($button.Content) { $button.Content } else { "Button" }
                Style = if ($button.Style) { $button.Style } else { "Default" }
                X = $currentX
                Y = $currentY
                Width = Get-SafeInt $button.Width 120
                Height = Get-SafeInt $button.Height 32
            }
            $elements.Elements += $btnData
            $currentY += $btnData.Height + $spacing
        }
        
        # Parse TextBoxes
        $textBoxes = $root.SelectNodes("//*[local-name()='TextBox']")
        foreach ($textBox in $textBoxes) {
            $tbData = @{
                Type = "TextBox"
                Name = if ($textBox.GetAttribute("x:Name")) { $textBox.GetAttribute("x:Name") } else { "TextBox" }
                PlaceholderText = if ($textBox.PlaceholderText) { $textBox.PlaceholderText } else { "" }
                X = $currentX
                Y = $currentY
                Width = if ($textBox.Width) { Get-SafeInt $textBox.Width 200 } else { if ($textBox.MaxWidth) { Get-SafeInt $textBox.MaxWidth 200 } else { 200 } }
                Height = Get-SafeInt $textBox.Height 32
            }
            $elements.Elements += $tbData
            $currentY += $tbData.Height + $spacing
        }
        
        # Parse TextBlocks
        $textBlocks = $root.SelectNodes("//*[local-name()='TextBlock']")
        foreach ($text in $textBlocks) {
            $textData = @{
                Type = "TextBlock"
                Name = if ($text.GetAttribute("x:Name")) { $text.GetAttribute("x:Name") } else { "TextBlock" }
                Text = if ($text.Text) { $text.Text } else { if ($text.InnerText) { $text.InnerText } else { "Text" } }
                X = $currentX
                Y = $currentY
                FontSize = Get-SafeInt $text.FontSize 14
                Width = 300
                Height = 20
            }
            $elements.Elements += $textData
            $currentY += $textData.Height + $spacing
        }
        
        # Parse Borders
        $borders = $root.SelectNodes("//*[local-name()='Border']")
        foreach ($border in $borders) {
            $borderData = @{
                Type = "Border"
                Name = if ($border.GetAttribute("x:Name")) { $border.GetAttribute("x:Name") } else { "Border" }
                Background = if ($border.Background) { $border.Background } else { "#FFFFFF" }
                BorderBrush = if ($border.BorderBrush) { $border.BorderBrush } else { "#E0E0E0" }
                CornerRadius = Get-SafeInt $border.CornerRadius 0
                X = $currentX
                Y = $currentY
                Width = Get-SafeInt $border.Width 400
                Height = Get-SafeInt $border.Height 100
            }
            $elements.Elements += $borderData
            $currentY += $borderData.Height + $spacing
        }
        
        # Parse CheckBoxes
        $checkBoxes = $root.SelectNodes("//*[local-name()='CheckBox']")
        foreach ($checkBox in $checkBoxes) {
            $cbData = @{
                Type = "CheckBox"
                Name = if ($checkBox.GetAttribute("x:Name")) { $checkBox.GetAttribute("x:Name") } else { "CheckBox" }
                Content = if ($checkBox.Content) { $checkBox.Content } else { "CheckBox" }
                X = $currentX
                Y = $currentY
                Width = 150
                Height = 20
            }
            $elements.Elements += $cbData
            $currentY += $cbData.Height + $spacing
        }
        
        # Parse FontIcons
        $fontIcons = $root.SelectNodes("//*[local-name()='FontIcon']")
        foreach ($icon in $fontIcons) {
            $iconData = @{
                Type = "FontIcon"
                Name = if ($icon.GetAttribute("x:Name")) { $icon.GetAttribute("x:Name") } else { "Icon" }
                Glyph = if ($icon.Glyph) { $icon.Glyph } else { "" }
                FontSize = Get-SafeInt $icon.FontSize 16
                Foreground = if ($icon.Foreground) { $icon.Foreground } else { "#0078D4" }
                X = $currentX
                Y = $currentY
            }
            $elements.Elements += $iconData
            $currentY += 32 + $spacing
        }
        
        # Parse ProgressBars
        $progressBars = $root.SelectNodes("//*[local-name()='ProgressBar']")
        foreach ($progress in $progressBars) {
            $pbData = @{
                Type = "ProgressBar"
                Name = if ($progress.GetAttribute("x:Name")) { $progress.GetAttribute("x:Name") } else { "ProgressBar" }
                X = $currentX
                Y = $currentY
                Width = Get-SafeInt $progress.Width 300
                Height = Get-SafeInt $progress.Height 4
                Value = Get-SafeInt $progress.Value 0
                Maximum = Get-SafeInt $progress.Maximum 100
            }
            $elements.Elements += $pbData
            $currentY += 20 + $spacing
        }
        
        Write-Host "Parsed XAML: Found $($elements.Elements.Count) elements. Size: $($elements.Width)x$($elements.Height)" -ForegroundColor Cyan
        return $elements
    }
    catch {
        Write-Host "Warning: Could not parse XAML file. Using default layout. Error: $_" -ForegroundColor Yellow
        return @{
            Width = 1400
            Height = 900
            Elements = @()
        }
    }
}

# Create Penpot shape JSON for a rectangle
function New-PenpotRect {
    param(
        [string]$id,
        [string]$name,
        [int]$x,
        [int]$y,
        [int]$width,
        [int]$height,
        [string]$fillColor,
        [string]$parentId,
        [string]$frameId,
        [string]$pageId,
        [array]$childShapes = @(),
        [string]$strokeColor = "#E0E0E0",
        [int]$strokeWidth = 2
    )
    
    $shapes = if ($childShapes.Count -eq 0) { "[]" } else { 
        '["' + ($childShapes -join '", "') + '"]'
    }
    
    return @"
{
  "id": "$id",
  "name": "$name",
  "type": "rect",
  "x": $x,
  "y": $y,
  "width": $width,
  "height": $height,
  "rotation": null,
  "selrect": {
    "x": $x,
    "y": $y,
    "width": $width,
    "height": $height,
    "x1": $x,
    "y1": $y,
    "x2": $($x + $width),
    "y2": $($y + $height)
  },
  "points": [
    {"x": $x, "y": $y},
    {"x": $($x + $width), "y": $y},
    {"x": $($x + $width), "y": $($y + $height)},
    {"x": $x, "y": $($y + $height)}
  ],
  "transform": {
    "a": 1.0,
    "b": 0.0,
    "c": 0.0,
    "d": 1.0,
    "e": 0.0,
    "f": 0.0
  },
  "transformInverse": {
    "a": 1.0,
    "b": 0.0,
    "c": 0.0,
    "d": 1.0,
    "e": 0.0,
    "f": 0.0
  },
  "parentId": "$parentId",
  "frameId": "$frameId",
  "flipX": null,
  "flipY": null,
  "proportionLock": false,
  "pageId": "$pageId",
  "proportion": 1,
  "r1": 4,
  "r2": 4,
  "r3": 4,
  "r4": 4,
  "fills": [
    {"fillColor": "$fillColor"}
  ],
  "strokes": [
    {"strokeColor": "$strokeColor", "strokeWidth": $strokeWidth, "strokeStyle": "solid", "strokeAlignment": "inner"}
  ]$(if ($childShapes.Count -gt 0) { ",`n  `"shapes`": $shapes" })
}
"@
}

$timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ")
$fileId = New-UUID
$pageId = New-UUID
$boardId = New-UUID

# Parse XAML to get dimensions and elements
$xamlData = Get-XamlElements -xamlPath $XamlFile
$boardWidth = $xamlData.Width
$boardHeight = $xamlData.Height

# Create temp structure
$tempDir = Join-Path $env:TEMP "penpot_gen_$(Get-Random)"
$filesDir = Join-Path $tempDir "files"
$fileSubDir = Join-Path $filesDir $fileId
$pagesDir = Join-Path $fileSubDir "pages"
$pageSubDir = Join-Path $pagesDir $pageId
$objectsDir = Join-Path $tempDir "objects"

New-Item -ItemType Directory -Path $pageSubDir -Force | Out-Null
New-Item -ItemType Directory -Path $objectsDir -Force | Out-Null

try {
    # Manifest - Manual JSON to ensure correct field order
    $manifestJson = @"
{
  "type": "penpot/export-files",
  "version": 1,
  "generatedBy": "XAML-to-Penpot/1.0",
  "refer": "penpot",
  "files": [
    {
      "id": "$fileId",
      "name": "$([System.IO.Path]::GetFileNameWithoutExtension($OutputFile))",
      "features": [
        "fdata/path-data",
        "design-tokens/v1",
        "variants/v1",
        "layout/grid",
        "components/v2",
        "fdata/shape-data-type"
      ]
    }
  ],
  "relations": []
}
"@
    # Write UTF-8 without BOM
    [System.IO.File]::WriteAllText((Join-Path $tempDir "manifest.json"), $manifestJson, (New-Object System.Text.UTF8Encoding $false))

    # File metadata - Manual JSON to ensure correct field order
    $fileMetadataJson = @"
{
  "features": [
    "fdata/path-data",
    "design-tokens/v1",
    "variants/v1",
    "layout/grid",
    "components/v2",
    "fdata/shape-data-type"
  ],
  "teamId": "$(New-UUID)",
  "hasMediaTrimmed": false,
  "name": "$([System.IO.Path]::GetFileNameWithoutExtension($OutputFile))",
  "revn": 1,
  "modifiedAt": "$timestamp",
  "vern": 0,
  "id": "$fileId",
  "isShared": false,
  "options": {
    "componentsV2": true,
    "baseFontSize": "16px"
  },
  "migrations": [
    "0001-remove-tokens-from-groups",
    "0002-normalize-bool-content-v2"
  ],
  "version": 67,
  "projectId": "$(New-UUID)",
  "createdAt": "$timestamp"
}
"@
    Write-JsonFileSafe -FilePath (Join-Path $filesDir "$fileId.json") -JsonContent $fileMetadataJson

    # Page metadata - Manual JSON to ensure correct field order
    $pageMetadataJson = @"
{
  "options": {
    "guides": {}
  },
  "id": "$pageId",
  "name": "$([System.IO.Path]::GetFileNameWithoutExtension($XamlFile))",
  "guides": {},
  "index": 0
}
"@
    Write-JsonFileSafe -FilePath (Join-Path $pagesDir "$pageId.json") -JsonContent $pageMetadataJson

    # Root shape - Manual JSON with proper field order
    $rootId = "00000000-0000-0000-0000-000000000000"
    $rootJson = @"
{
  "id": "$rootId",
  "name": "Root Frame",
  "type": "frame",
  "x": 0,
  "y": 0,
  "width": $boardWidth,
  "height": $boardHeight,
  "rotation": 0,
  "selrect": {
    "x": 0,
    "y": 0,
    "width": $boardWidth,
    "height": $boardHeight,
    "x1": 0,
    "y1": 0,
    "x2": $boardWidth,
    "y2": $boardHeight
  },
  "points": [
    {"x": 0, "y": 0},
    {"x": $boardWidth, "y": 0},
    {"x": $boardWidth, "y": $boardHeight},
    {"x": 0, "y": $boardHeight}
  ],
  "transform": {
    "a": 1.0,
    "b": 0.0,
    "c": 0.0,
    "d": 1.0,
    "e": 0.0,
    "f": 0.0
  },
  "transformInverse": {
    "a": 1.0,
    "b": 0.0,
    "c": 0.0,
    "d": 1.0,
    "e": 0.0,
    "f": 0.0
  },
  "parentId": "$rootId",
  "frameId": "$rootId",
  "flipX": null,
  "flipY": null,
  "hideFillOnExport": false,
  "r2": 0,
  "proportionLock": false,
  "pageId": "$pageId",
  "r3": 0,
  "r1": 0,
  "strokes": [],
  "proportion": 1.0,
  "r4": 0,
  "fills": [
    {"fillColor": "#FFFFFF", "fillOpacity": 1}
  ],
  "shapes": ["$boardId"]
}
"@
    # Root shape is always written
    Write-JsonFileSafe -FilePath (Join-Path $pageSubDir "$rootId.json") -JsonContent $rootJson
    
    # Create the Board (Main Frame)
    $boardJson = @"
{
  "id": "$boardId",
  "name": "$([System.IO.Path]::GetFileNameWithoutExtension($XamlFile)) Board",
  "type": "frame",
  "x": 100,
  "y": 100,
  "width": $boardWidth,
  "height": $boardHeight,
  "rotation": 0,
  "selrect": {
    "x": 100,
    "y": 100,
    "width": $boardWidth,
    "height": $boardHeight,
    "x1": 100,
    "y1": 100,
    "x2": $(100 + $boardWidth),
    "y2": $(100 + $boardHeight)
  },
  "points": [
    {"x": 100, "y": 100},
    {"x": $(100 + $boardWidth), "y": 100},
    {"x": $(100 + $boardWidth), "y": $(100 + $boardHeight)},
    {"x": 100, "y": $(100 + $boardHeight)}
  ],
  "transform": { "a": 1.0, "b": 0.0, "c": 0.0, "d": 1.0, "e": 0.0, "f": 0.0 },
  "transformInverse": { "a": 1.0, "b": 0.0, "c": 0.0, "d": 1.0, "e": 0.0, "f": 0.0 },
  "parentId": "$rootId",
  "frameId": "$rootId",
  "pageId": "$pageId",
  "fills": [{"fillColor": "#FFFFFF", "fillOpacity": 1}],
  "strokes": [{"strokeColor": "#000000", "strokeWidth": 1, "strokeOpacity": 0.1}],
  "r1": 0, "r2": 0, "r3": 0, "r4": 0,
  "shapes": [
    $(
        $shapeRefs = @()
        foreach ($sid in $generatedShapeIds) {
            $shapeRefs += "`"$sid`""
        }
        $shapeRefs -join ",`n    "
    )
  ]
}
"@
    Write-JsonFileSafe -FilePath (Join-Path $pageSubDir "$boardId.json") -JsonContent $boardJson

    # Generate shapes based on XAML content
    $generatedShapeIds = @()
    if ($xamlData.Elements.Count -gt 0) {
        Write-Host "Generating shapes from XAML elements..." -ForegroundColor Gray
        foreach ($element in $xamlData.Elements) {
            $shapeId = New-UUID
            $generatedShapeIds += $shapeId
            
            $shapeJson = switch ($element.Type) {
                "Button" {
                    Convert-Button -Id $shapeId -Name $element.Name `
                        -X $element.X -Y $element.Y -Width $element.Width -Height $element.Height `
                        -ParentId $boardId -FrameId $boardId -PageId $pageId `
                        -Content $element.Content -Style $element.Style
                }
                "TextBox" {
                    Convert-TextBox -Id $shapeId -Name $element.Name `
                        -X $element.X -Y $element.Y -Width $element.Width -Height $element.Height `
                        -ParentId $boardId -FrameId $boardId -PageId $pageId `
                        -PlaceholderText $element.PlaceholderText
                }
                "TextBlock" {
                    Convert-TextBlock -Id $shapeId -Name $element.Name `
                        -X $element.X -Y $element.Y -Width $element.Width -Height $element.Height `
                        -ParentId $boardId -FrameId $boardId -PageId $pageId `
                        -Text $element.Text -FontSize $element.FontSize
                }
                "Border" {
                    Convert-Border -Id $shapeId -Name $element.Name `
                        -X $element.X -Y $element.Y -Width $element.Width -Height $element.Height `
                        -ParentId $boardId -FrameId $boardId -PageId $pageId `
                        -Background $element.Background -BorderBrush $element.BorderBrush `
                        -CornerRadius $element.CornerRadius -BorderThickness 1
                }
                "CheckBox" {
                    Convert-CheckBox -Id $shapeId -Name $element.Name `
                        -X $element.X -Y $element.Y -Width $element.Width -Height $element.Height `
                        -ParentId $boardId -FrameId $boardId -PageId $pageId `
                        -Content $element.Content
                }
                "FontIcon" {
                    Convert-FontIcon -Id $shapeId -Name $element.Name `
                        -X $element.X -Y $element.Y -FontSize $element.FontSize `
                        -ParentId $boardId -FrameId $boardId -PageId $pageId `
                        -Glyph $element.Glyph -Foreground $element.Foreground
                }
                "ProgressBar" {
                    Convert-ProgressBar -Id $shapeId -Name $element.Name `
                        -X $element.X -Y $element.Y -Width $element.Width -Height $element.Height `
                        -ParentId $boardId -FrameId $boardId -PageId $pageId `
                        -Value $element.Value -Maximum $element.Maximum
                }
                default {
                    # Unknown element - create basic rectangle
                    New-PenpotRect -id $shapeId -name $element.Name `
                        -x $element.X -y $element.Y -width $element.Width -height $element.Height `
                        -fillColor "#CCCCCC" -parentId $boardId -frameId $boardId -pageId $pageId
                }
            }
            
            Write-JsonFileSafe -FilePath (Join-Path $pageSubDir "$shapeId.json") -JsonContent $shapeJson
        }
    }
    else {
        # Enhanced default layout with common UI patterns
        Write-Host "  Note: Using enhanced template layout (XAML parsing found no positioned elements)" -ForegroundColor Yellow
        
        $navId = New-UUID
        $contentId = New-UUID
        $titleId = New-UUID
        $titleTextId = New-UUID
        $mainAreaId = New-UUID
        $buttonAreaId = New-UUID
        $button1Id = New-UUID
        $button2Id = New-UUID
        $generatedShapeIds = @($navId, $contentId, $titleId, $titleTextId, $mainAreaId, $buttonAreaId, $button1Id, $button2Id)
        
        # Navigation pane (left side - dark)
        $navJson = New-PenpotRect -id $navId -name "NavigationPane" -x 0 -y 0 -width 320 -height $boardHeight `
            -fillColor "#1A1A1A" -strokeColor "#000000" -strokeWidth 0 -parentId $boardId -frameId $boardId -pageId $pageId
        Write-JsonFileSafe -FilePath (Join-Path $pageSubDir "$navId.json") -JsonContent $navJson
        
        # Content area background (right side)
        $contentJson = New-PenpotRect -id $contentId -name "ContentArea" -x 320 -y 0 -width ($boardWidth - 320) -height $boardHeight `
            -fillColor "#E0E0E0" -strokeColor "#AAAAAA" -strokeWidth 2 -parentId $boardId -frameId $boardId -pageId $pageId
        Write-JsonFileSafe -FilePath (Join-Path $pageSubDir "$contentId.json") -JsonContent $contentJson
        
        # Title/Header bar
        $titleJson = New-PenpotRect -id $titleId -name "HeaderBar" -x 320 -y 0 -width ($boardWidth - 320) -height 80 `
            -fillColor "#0078D4" -strokeColor "#005A9E" -strokeWidth 2 -parentId $boardId -frameId $boardId -pageId $pageId
        Write-JsonFileSafe -FilePath (Join-Path $pageSubDir "$titleId.json") -JsonContent $titleJson
        
        # Title text placeholder
        $titleTextJson = New-PenpotRect -id $titleTextId -name "TitlePlaceholder" -x 340 -y 20 -width 300 -height 40 `
            -fillColor "#1E6FBF" -strokeColor "#FFFFFF" -strokeWidth 2 -parentId $boardId -frameId $boardId -pageId $pageId
        Write-JsonFileSafe -FilePath (Join-Path $pageSubDir "$titleTextId.json") -JsonContent $titleTextJson
        
        # Main content area (white card)
        $mainAreaJson = New-PenpotRect -id $mainAreaId -name "MainContentCard" -x 360 -y 120 -width ($boardWidth - 400) -height ($boardHeight - 220) `
            -fillColor "#F5F5F5" -strokeColor "#999999" -strokeWidth 4 -parentId $boardId -frameId $boardId -pageId $pageId
        Write-JsonFileSafe -FilePath (Join-Path $pageSubDir "$mainAreaId.json") -JsonContent $mainAreaJson
        
        # Button area background
        $buttonAreaJson = New-PenpotRect -id $buttonAreaId -name "ActionButtonArea" -x 360 -y ($boardHeight - 90) -width ($boardWidth - 400) -height 60 `
            -fillColor "#D0D0D0" -strokeColor "#888888" -strokeWidth 3 -parentId $boardId -frameId $boardId -pageId $pageId
        Write-JsonFileSafe -FilePath (Join-Path $pageSubDir "$buttonAreaId.json") -JsonContent $buttonAreaJson
        
        # Primary action button
        $button1Json = New-PenpotRect -id $button1Id -name "PrimaryButton" -x ($boardWidth - 280) -y ($boardHeight - 75) -width 120 -height 32 `
            -fillColor "#0078D4" -strokeColor "#005A9E" -strokeWidth 2 -parentId $boardId -frameId $boardId -pageId $pageId
        Write-JsonFileSafe -FilePath (Join-Path $pageSubDir "$button1Id.json") -JsonContent $button1Json
        
        # Secondary action button
        $button2Json = New-PenpotRect -id $button2Id -name "SecondaryButton" -x ($boardWidth - 420) -y ($boardHeight - 75) -width 120 -height 32 `
            -fillColor "#BBBBBB" -strokeColor "#666666" -strokeWidth 3 -parentId $boardId -frameId $boardId -pageId $pageId
        Write-JsonFileSafe -FilePath (Join-Path $pageSubDir "$button2Id.json") -JsonContent $button2Json
    }
    
    # Board - Manual JSON with proper field order
    $shapesArray = '["' + ($generatedShapeIds -join '", "') + '"]'
    
    $boardJson = @"
{
  "id": "$boardId",
  "name": "$([System.IO.Path]::GetFileNameWithoutExtension($XamlFile)) Board",
  "type": "frame",
  "x": 0,
  "y": 0,
  "width": $boardWidth,
  "height": $boardHeight,
  "rotation": 0,
  "selrect": {
    "x": 0,
    "y": 0,
    "width": $boardWidth,
    "height": $boardHeight,
    "x1": 0,
    "y1": 0,
    "x2": $boardWidth,
    "y2": $boardHeight
  },
  "points": [
    {"x": 0, "y": 0},
    {"x": $boardWidth, "y": 0},
    {"x": $boardWidth, "y": $boardHeight},
    {"x": 0, "y": $boardHeight}
  ],
  "transform": {
    "a": 1.0,
    "b": 0.0,
    "c": 0.0,
    "d": 1.0,
    "e": 0.0,
    "f": 0.0
  },
  "transformInverse": {
    "a": 1.0,
    "b": 0.0,
    "c": 0.0,
    "d": 1.0,
    "e": 0.0,
    "f": 0.0
  },
  "parentId": "$rootId",
  "frameId": "$rootId",
  "flipX": null,
  "flipY": null,
  "hideFillOnExport": false,
  "growType": "fixed",
  "hideInViewer": false,
  "proportionLock": false,
  "pageId": "$pageId",
  "r1": 0,
  "r2": 0,
  "r3": 0,
  "r4": 0,
  "proportion": 1,
  "fills": [
    {"fillColor": "#FFFFFF"}
  ],
  "strokes": [],
  "shapes": $shapesArray
}
"@
    Write-JsonFileSafe -FilePath (Join-Path $pageSubDir "$boardId.json") -JsonContent $boardJson

    Write-Host "Created Penpot structure" -ForegroundColor Green

    # Create ZIP with forward slashes (required by Penpot)
    $output = Join-Path (Get-Location) $OutputFile
    if (Test-Path $output) { Remove-Item $output -Force }

    Add-Type -Assembly System.IO.Compression.FileSystem
    
    # Validate all JSON files before zipping
    Write-Host "Validating JSON files..." -ForegroundColor Gray
    $jsonFiles = Get-ChildItem $tempDir -Recurse -File -Filter "*.json"
    foreach ($jsonFile in $jsonFiles) {
        try {
            $content = Get-Content $jsonFile.FullName -Raw
            $null = ConvertFrom-Json $content -ErrorAction Stop
        }
        catch {
            Write-Host "ERROR: Invalid JSON in $($jsonFile.Name): $($_.Exception.Message)" -ForegroundColor Red
            throw "JSON validation failed for $($jsonFile.Name)"
        }
    }
    Write-Host "All JSON files validated successfully" -ForegroundColor Green
    
    # Create ZIP archive with proper disposal
    $zip = $null
    try {
        $zip = [System.IO.Compression.ZipFile]::Open($output, [System.IO.Compression.ZipArchiveMode]::Create)
        
        # Add all files with forward slash paths
        Get-ChildItem $tempDir -Recurse -File | ForEach-Object {
            $relativePath = $_.FullName.Substring($tempDir.Length + 1).Replace('\', '/')
            Write-Host "  Adding: $relativePath" -ForegroundColor DarkGray
            [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $_.FullName, $relativePath, [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
        }
        
        Write-Host "ZIP archive created" -ForegroundColor Green
    }
    catch {
        Write-Host "ERROR creating ZIP: $($_.Exception.Message)" -ForegroundColor Red
        throw
    }
    finally {
        if ($zip -ne $null) {
            $zip.Dispose()
            # Force flush to disk
            [System.GC]::Collect()
            [System.GC]::WaitForPendingFinalizers()
        }
    }
    
    # Verify the output file was created and is valid
    if (Test-Path $output) {
        $fileSize = (Get-Item $output).Length
        if ($fileSize -gt 0) {
            Write-Host "Successfully created: $output ($fileSize bytes)" -ForegroundColor Green
            
            # Verify ZIP integrity
            try {
                $testZip = [System.IO.Compression.ZipFile]::OpenRead($output)
                $entryCount = $testZip.Entries.Count
                $testZip.Dispose()
                Write-Host "ZIP verification: $entryCount files" -ForegroundColor Green
            }
            catch {
                Write-Host "WARNING: ZIP file may be corrupted: $($_.Exception.Message)" -ForegroundColor Yellow
            }
        }
        else {
            Write-Host "ERROR: Output file is empty" -ForegroundColor Red
            throw "Generated file is 0 bytes"
        }
    }
    else {
        Write-Host "ERROR: Output file was not created" -ForegroundColor Red
        throw "File creation failed"
    }
    
    Write-Host ""
    Write-Host "Import this file into Penpot at https://design.penpot.app/" -ForegroundColor Yellow
}
finally {
    if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force }
}
