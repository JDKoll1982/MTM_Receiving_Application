# Comprehensive Prompt for Recreating Penpot Mockup Generator

**Role:** You are an expert Tooling Engineer and Automation Specialist with deep knowledge of WinUI 3 XAML and the Penpot file specification. You are detail-oriented, rigorous about file integrity, and prefer robust, modular code over quick scripts.

**Objective:** Create a robust PowerShell automation solution (main script `Generate-Penpot-v2.ps1` and supporting converter modules) that parses WinUI 3 XAML files and generates pixel-perfect, valid `.penpot` import files. **Every XAML control must be convertible to a visual representation in Penpot - no control should be silently ignored.**

**Context:** The previous implementation (`Generate-Penpot.ps1`) suffered from "white-on-white" visibility issues, massive canvas scaling bugs, and fragile JSON generation. We are restarting with a modular approach to ensure every XAML control is visually represented correctly in the Penpot design tool.

**Critical Success Criteria:**
1. **100% XAML Coverage** - Every control type used in the MTM Receiving Application must have a converter
2. **Visual Accuracy** - Controls must be visually distinguishable with proper colors, borders, and sizing
3. **Layout Fidelity** - Grid, StackPanel, and other layout containers must position children correctly
4. **Import Success** - Generated .penpot files must import without errors into Penpot online editor and desktop app

## 0. Chain of Thought & Planning (Required)
Before writing any code, you must:
1.  **Analyze the Architecture:** Plan the folder structure for the converter modules (e.g., `Converters/Convert-Grid.ps1`, `Converters/Convert-Button.ps1`).
2.  **Define the Data Flow:** Trace how a XAML element is read -> parsed -> mapped to a shape -> converted to JSON -> added to the Page -> zipped.
3.  **Identify Edge Cases:** How will you handle nested Grids? Missing RowDefinitions? Unknown controls?
4.  **Validation Strategy:** How will you verify the JSON is valid *before* zipping it?

## Reference Material & Research
**IMPORTANT:** You MUST thoroughly research these repositories before implementing. Do not guess at specifications.

### 1. Penpot File Format & Architecture
**Official Repository:** `https://github.com/penpot/penpot`

**Critical Files to Study:**
*   **File Format v3 Specification:**
    *   `backend/src/app/binfile/v3.clj` - Complete export/import logic
    *   `common/src/app/common/types/shape.cljc` - All shape type definitions (rect, circle, text, group, frame, etc.)
    *   `common/src/app/common/types/shape/layout.cljc` - Layout and positioning logic
*   **Shape Properties:**
    *   `common/src/app/common/geom/shapes.cljc` - Geometric calculations
    *   `common/src/app/common/types/shape/radius.cljc` - Border radius handling
*   **Text Content Structure:**
    *   `common/src/app/common/text.cljc` - ProseMirror text content format
*   **Colors & Fills:**
    *   `common/src/app/common/types/color.cljc` - Color format specifications

**Key Concepts from Penpot Research:**
*   **Manifest:** `manifest.json` at root defines version, origin, and file list
*   **Hierarchy:** Bidirectional linking - Parent has `shapes: [child_id]`, Child has `parent-id: parent_id`
*   **Coordinates:** Always relative to parent frame/group
*   **Text:** Uses ProseMirror JSON structure (never plain strings)
*   **UUIDs:** Must be valid UUID v4 format
*   **Encoding:** UTF-8 without BOM required for all JSON files
*   **ZIP Structure:** Forward slashes in paths, specific directory layout

### 2. WinUI 3 / XAML Controls Architecture
**Official Repository:** `https://github.com/microsoft/microsoft-ui-xaml`

**Critical Files to Study:**
*   **Control Templates:**
    *   `dev/CommonStyles/Generic.xaml` - Default visual trees for all controls
    *   `dev/*/InteractionTests/` - Examples of control usage and behavior
*   **Layout System:**
    *   `dev/Common/LayoutPanel.cpp` - Grid layout algorithm
    *   `dev/Repeater/StackLayout.cpp` - StackPanel layout logic
*   **Control Definitions:**
    *   `dev/Button/` - Button visual structure and states
    *   `dev/TextBox/` - TextBox template and chrome
    *   `dev/NumberBox/` - NumberBox composition
    *   `dev/DataGrid/` - DataGrid structure (CommunityToolkit)
*   **Design Tokens:**
    *   `dev/Materials/*/ThemeResources.xaml` - Default colors, brushes, sizes

**WinUI3 Layout Logic (Must Implement):**
*   **Grid:**
    *   Supports `Pixel` (fixed), `Auto` (content-based), and `*` (weighted remaining space) sizing
    *   Priority: Pixel → Auto → Star distribution
    *   Default: 1x1 Grid with `*` sizing if definitions missing
*   **StackPanel:**
    *   Vertical: `y = Σ(previous_heights + spacing)`
    *   Horizontal: `x = Σ(previous_widths + spacing)`
*   **Border:**
    *   Adds BorderThickness to dimensions
    *   Padding applies to child content area
*   **ScrollViewer:**
    *   Acts as viewport, children can exceed bounds

### 3. CommunityToolkit.WinUI Components
**Repository:** `https://github.com/CommunityToolkit/Windows`

**Controls Used in MTM Application:**
*   `DataGrid` - Multi-column data table with headers
*   Study: `CommunityToolkit.WinUI.UI.Controls/DataGrid/`

### 4. Additional Research Tasks
Before coding, you must:
1. **Download sample .penpot files** from Penpot and extract them to understand actual structure
2. **Trace the v3 export code** in Penpot repo to see exact JSON field order and values
3. **Review WinUI Generic.xaml** to understand default control dimensions and visual composition
4. **Study ProseMirror text schema** for proper text content formatting

## 1. Input Data (Source XAML Files)
You will parse XAML files located in `Views/` directory (including subdirectories like `Views/Receiving/`, `Views/Shared/`).

### Complete WinUI 3 Control Reference (MTM Project Usage)

**CRITICAL:** All controls listed below are ACTIVELY USED in the MTM Receiving Application. You MUST implement a converter for each one. Use this checklist during implementation.

#### Layout Containers (MUST SUPPORT)
- [x] **Grid** - Primary layout container with row/column definitions
  - Properties: RowDefinitions, ColumnDefinitions, RowSpacing, ColumnSpacing, Padding
  - Children use: Grid.Row, Grid.Column, Grid.RowSpan, Grid.ColumnSpan
  - Converter: `Convert-Grid.ps1` ✓ (exists)
- [x] **StackPanel** - Linear vertical/horizontal layout
  - Properties: Orientation, Spacing, HorizontalAlignment, VerticalAlignment
  - Converter: `Convert-StackPanel.ps1` ✓ (exists)
- [x] **Border** - Container with background, border, corner radius
  - Properties: Background, BorderBrush, BorderThickness, CornerRadius, Padding
  - Converter: `Convert-Border.ps1` ✓ (exists)
- [ ] **ScrollViewer** - Scrollable viewport (children can exceed bounds)
  - Properties: MaxHeight, MaxWidth, HorizontalScrollBarVisibility, VerticalScrollBarVisibility
  - Converter: **REQUIRED** - New file needed
- [ ] **UserControl** - Custom control wrapper (treat as Frame/Group)
  - Root element of many views
  - Converter: **REQUIRED** - New file needed
- [ ] **Page** - Top-level page container
  - Root element of page views
  - Converter: **REQUIRED** - New file needed
- [ ] **Window** - Application window root
  - Converter: **REQUIRED** - New file needed
- [ ] **ContentDialog** - Modal dialog container
  - Properties: Title, PrimaryButtonText, SecondaryButtonText, CloseButtonText
  - Converter: **REQUIRED** - New file needed
- [ ] **ContentControl** - Generic content wrapper
  - Converter: **REQUIRED** - New file needed
- [ ] **Expander** - Collapsible container with header
  - Properties: Header, IsExpanded
  - Converter: **REQUIRED** - New file needed
- [ ] **ItemsControl** - Container for dynamic item lists
  - Properties: ItemsSource, ItemTemplate
  - Converter: **REQUIRED** - New file needed

#### Input Controls (MUST SUPPORT)
- [x] **TextBox** - Single/multi-line text input
  - Properties: Text, PlaceholderText, MaxWidth, Height, IsReadOnly
  - Converter: `Convert-TextBox.ps1` ✓ (exists)
- [ ] **PasswordBox** - Password input with masked text
  - Properties: PlaceholderText, MaxWidth
  - Converter: **REQUIRED** - New file needed
- [ ] **NumberBox** - Numeric input with spin buttons
  - Properties: Header, Value, Minimum, Maximum, SmallChange, LargeChange, SpinButtonPlacementMode
  - Converter: **REQUIRED** - New file needed (used in LoadEntryView)
- [ ] **ComboBox** - Dropdown selection
  - Properties: Header, SelectedItem, Items/ItemsSource
  - Converter: **REQUIRED** - New file needed
- [ ] **ComboBoxItem** - Individual dropdown item
  - Properties: Content
  - Converter: **REQUIRED** - New file needed
- [x] **CheckBox** - Boolean checkbox input
  - Properties: Content, IsChecked
  - Converter: `Convert-CheckBox.ps1` ✓ (exists)

#### Display Controls (MUST SUPPORT)
- [x] **TextBlock** - Read-only text display
  - Properties: Text, FontSize, FontWeight, Foreground, Style
  - Converter: `Convert-TextBlock.ps1` ✓ (exists)
- [x] **Button** - Clickable action button
  - Properties: Content, Style (AccentButtonStyle), Command
  - Converter: `Convert-Button.ps1` ✓ (exists)
- [ ] **InfoBar** - Informational message bar
  - Properties: Title, Message, Severity, IsOpen
  - Converter: **REQUIRED** - New file needed (used in POEntryView)
- [x] **FontIcon** - Icon from Segoe Fluent Icons font
  - Properties: Glyph, FontSize, Foreground
  - Converter: `Convert-FontIcon.ps1` ✓ (exists)
- [x] **ProgressBar** - Linear progress indicator
  - Properties: IsIndeterminate, Value, Maximum
  - Converter: `Convert-ProgressBar.ps1` ✓ (exists)
- [ ] **ProgressRing** - Circular progress spinner
  - Properties: IsActive
  - Converter: **REQUIRED** - New file needed

#### Data Display Controls (MUST SUPPORT)
- [ ] **DataGrid** (from CommunityToolkit.WinUI.UI.Controls)
  - Properties: ItemsSource, AutoGenerateColumns, Columns, GridLinesVisibility, HeadersVisibility
  - Columns: DataGridTextColumn with Header, Binding, Width
  - Converter: **REQUIRED** - New file needed (CRITICAL - used in POEntryView)
  - Visual: Generate header row + 3-5 sample data rows with column dividers

#### Command Controls (MUST SUPPORT)
- [ ] **CommandBar** - Application command toolbar
  - Children: AppBarButton, AppBarSeparator
  - Converter: **REQUIRED** - New file needed
- [ ] **AppBarButton** - Button in CommandBar
  - Properties: Icon, Label
  - Converter: **REQUIRED** - New file needed
- [ ] **AppBarSeparator** - Visual separator in CommandBar
  - Converter: **REQUIRED** - New file needed

#### Special Elements (MUST SUPPORT)
- [ ] **Flyout** - Popup attached to control
  - Properties: Content (child elements)
  - Converter: **REQUIRED** - New file needed
- [ ] **Run** - Inline text span within TextBlock
  - Properties: Text, FontWeight
  - Converter: **REQUIRED** - New file needed
- [ ] **DataTemplate** - Template definition (skip - not visual)
  - Action: Parse children but don't create shape for template itself
- [ ] **ColumnDefinition** / **RowDefinition** - Grid definition (metadata only)
  - Properties: Width/Height (Pixel, Auto, Star)
  - Action: Parse for layout calculation, don't create shapes

### XAML Parsing Requirements

**Property Extraction (Extract ALL of these):**
- **Dimensions:** Width, Height, MinWidth, MinHeight, MaxWidth, MaxHeight
- **Layout:** Margin, Padding, HorizontalAlignment, VerticalAlignment
- **Grid Attachment:** Grid.Row, Grid.Column, Grid.RowSpan, Grid.ColumnSpan
- **Visual:** Background, Foreground, BorderBrush, BorderThickness, CornerRadius
- **Content:** Text, Content, Header, PlaceholderText
- **State:** Visibility, IsEnabled, IsChecked, IsExpanded, IsOpen
- **Data:** ItemsSource (binding path), SelectedItem (binding path)
- **Styling:** Style (e.g., "AccentButtonStyle"), FontSize, FontWeight

**Layout Approximation Algorithm:**
You must implement a simplified XAML layout engine in PowerShell:

1. **Grid Layout Engine:**
   ```powershell
   # Pseudo-algorithm
   - Parse RowDefinitions and ColumnDefinitions
   - Classify each as: Pixel (fixed), Auto (content-based), or Star (proportional)
   - Calculate sizes in order:
     1. Pixel: Reserve exact size
     2. Auto: Estimate from child content (TextBox=32px, TextBlock=20px, Button=32px, etc.)
     3. Star: Distribute remaining space proportionally (1* gets 1 part, 2* gets 2 parts)
   - If no definitions: Default to single 1* row and 1* column
   - Position children based on Grid.Row/Column/RowSpan/ColumnSpan
   ```

2. **StackPanel Layout Engine:**
   ```powershell
   # Vertical orientation
   $currentY = 0
   foreach ($child in $children) {
       $child.Y = $currentY
       $currentY += $child.Height + $Spacing
   }
   
   # Horizontal orientation
   $currentX = 0
   foreach ($child in $children) {
       $child.X = $currentX
       $currentX += $child.Width + $Spacing
   }
   ```

3. **Default Sizes (When Width/Height not specified):**
   ```powershell
   $defaultSizes = @{
       "TextBox" = @{ Width=200; Height=32 }
       "PasswordBox" = @{ Width=200; Height=32 }
       "NumberBox" = @{ Width=200; Height=32 }
       "ComboBox" = @{ Width=200; Height=32 }
       "Button" = @{ Width=120; Height=32 }
       "CheckBox" = @{ Width=150; Height=20 }
       "TextBlock" = @{ Width=200; Height=20 }
       "FontIcon" = @{ Width=16; Height=16 }
       "ProgressBar" = @{ Width=300; Height=4 }
       "ProgressRing" = @{ Width=32; Height=32 }
       "InfoBar" = @{ Width=400; Height=60 }
       "DataGrid" = @{ Width=800; Height=300 }
       "CommandBar" = @{ Width=800; Height=48 }
   }
   ```

## 2. Output Format (Target)
The output must be a valid `.penpot` file, which is a **ZIP archive** with a specific internal structure.

### Internal ZIP Structure
```text
/
├── manifest.json                  # Global metadata
└── files/
    └── {file-uuid}/               # Project Root
        ├── page-index.json        # List of pages
        ├── pages/
        │   ├── {page-uuid}.json   # Page metadata
        │   └── {page-uuid}/       # Shape definitions folder
        │       ├── {shape-uuid}.json
        │       └── ...
```

### JSON Schemas (Strict Compliance Required)

**A. `manifest.json`**
```json
{
  "version": 3,
  "origin": "https://design.penpot.app",
  "files": [
    {
      "id": "{file-uuid}",
      "name": "MTM Receiving Mockups",
      "revn": 1,
      "version": 1
    }
  ]
}
```

**B. Page JSON (`pages/{page-uuid}.json`)**
```json
{
  "id": "{page-uuid}",
  "name": "Page Name",
  "file-id": "{file-uuid}",
  "shapes": ["{root-frame-uuid}"], 
  "background-color": "#E5E5E5"
}
```

**C. Shape JSON (`pages/{page-uuid}/{shape-uuid}.json`)**
Every element (Frame, Rect, Text) is a separate JSON file.

**Critical Rules for Shapes:**
1.  **Hierarchy is Bidirectional:**
    -   **Parent** must list child UUIDs in its `shapes` array: `"shapes": ["child-1-uuid", "child-2-uuid"]`
    -   **Child** must reference parent: `"parent-id": "{parent-uuid}"`
    -   **Frame ID**: If inside a frame (like a Board), child must set `"frame-id": "{board-uuid}"`.
2.  **Coordinates:** `x` and `y` are relative to the **parent**.
3.  **Colors:** Use visible, contrasting colors (e.g., `#E0E0E0` for backgrounds, `#333333` for text). **Do not use white-on-white.**
4.  **Text Content:** Text is NOT a simple string. It uses a structured object:
    ```json
    "content": {
      "type": "doc",
      "content": [
        {
          "type": "paragraph",
          "content": [
            {
              "type": "text",
              "text": "Actual Text Here"
            }
          ]
        }
      ]
    }
    ```

## 3. Implementation Strategy (PowerShell)

### Phase 1: Setup & Research (Before Coding)
1. **Clone Penpot Repository:**
   ```bash
   git clone https://github.com/penpot/penpot
   cd penpot
   # Study files mentioned in Reference Material section
   ```

2. **Extract Sample .penpot Files:**
   - Create a simple design in Penpot online
   - Export it
   - Unzip and study the actual JSON structure
   - Compare with what the export code generates

3. **Study WinUI Control Templates:**
   ```bash
   git clone https://github.com/microsoft/microsoft-ui-xaml
   cd microsoft-ui-xaml
   # Study Generic.xaml for each control
   ```

### Phase 2: Architecture Design
1. **Folder Structure:**
   ```
   Mockups/
   ├── Generate-Penpot-v2.ps1          # Main orchestrator
   ├── Converters/
   │   ├── README.md                    # Converter documentation
   │   ├── Convert-Core.ps1             # Shared utilities (UUID, JSON writer, etc.)
   │   ├── Convert-Grid.ps1             # Grid layout logic
   │   ├── Convert-StackPanel.ps1       # StackPanel layout
   │   ├── Convert-Border.ps1           # Border with styling
   │   ├── Convert-TextBlock.ps1        # Text display
   │   ├── Convert-TextBox.ps1          # Text input
   │   ├── Convert-Button.ps1           # Button
   │   ├── Convert-CheckBox.ps1         # CheckBox
   │   ├── Convert-FontIcon.ps1         # Icon
   │   ├── Convert-ProgressBar.ps1      # Progress bar
   │   ├── Convert-ProgressRing.ps1     # NEW: Progress spinner
   │   ├── Convert-NumberBox.ps1        # NEW: Number input
   │   ├── Convert-PasswordBox.ps1      # NEW: Password input
   │   ├── Convert-ComboBox.ps1         # NEW: Dropdown
   │   ├── Convert-InfoBar.ps1          # NEW: Message bar
   │   ├── Convert-DataGrid.ps1         # NEW: Data table (CRITICAL)
   │   ├── Convert-CommandBar.ps1       # NEW: Toolbar
   │   ├── Convert-AppBarButton.ps1     # NEW: Toolbar button
   │   ├── Convert-ScrollViewer.ps1     # NEW: Scrollable area
   │   ├── Convert-UserControl.ps1      # NEW: Custom control
   │   ├── Convert-Page.ps1             # NEW: Page root
   │   ├── Convert-Window.ps1           # NEW: Window root
   │   ├── Convert-ContentDialog.ps1    # NEW: Dialog
   │   └── Convert-Fallback.ps1         # NEW: Unknown elements
   └── Tests/
       └── Test-Converters.ps1          # Validation tests
   ```

2. **Data Flow Pipeline:**
   ```
   XAML File
     ↓ [Parse with XmlDocument]
   Element Tree
     ↓ [Layout Calculation Engine]
   Positioned Elements (with X, Y, Width, Height)
     ↓ [Converter Dispatcher]
   Penpot Shape JSONs
     ↓ [Hierarchy Builder]
   Parent-Child Linked Shapes
     ↓ [JSON Validator]
   Valid JSON Files
     ↓ [ZIP Packager]
   .penpot File
   ```

### Phase 3: Core Implementation

#### A. Shared Utilities (`Converters/Convert-Core.ps1`)
```powershell
# UUID Generator
function New-UUID {
    [guid]::NewGuid().ToString().ToLower()
}

# Safe JSON Writer with Validation
function Write-JsonFileSafe {
    param(
        [string]$FilePath,
        [string]$JsonContent
    )
    
    # Write UTF-8 without BOM
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($FilePath, $JsonContent, $utf8NoBom)
    
    # Validate
    try {
        $null = ConvertFrom-Json $JsonContent -ErrorAction Stop
    }
    catch {
        throw "Invalid JSON in ${FilePath}: $($_.Exception.Message)"
    }
}

# ProseMirror Text Content Generator
function New-PenpotTextContent {
    param([string]$Text)
    
    return @{
        type = "doc"
        content = @(
            @{
                type = "paragraph"
                content = @(
                    @{
                        type = "text"
                        text = $Text
                    }
                )
            }
        )
    } | ConvertTo-Json -Depth 10 -Compress
}

# Default Color Palette (High Contrast)
$script:PenpotColors = @{
    CardBackground = "#FFFFFF"
    Border = "#E0E0E0"
    Text = "#333333"
    AccentBlue = "#0078D4"
    AccentHover = "#106EBE"
    Placeholder = "#E8E8E8"
    InputBorder = "#8A8A8A"
    ErrorRed = "#C42B1C"
    SuccessGreen = "#107C10"
    WarningOrange = "#FF8C00"
    IconGray = "#616161"
}
```

#### B. Layout Engine (`Generate-Penpot-v2.ps1`)
```powershell
# Grid Layout Calculator
function Calculate-GridLayout {
    param(
        [xml]$GridElement,
        [int]$AvailableWidth,
        [int]$AvailableHeight
    )
    
    # Parse RowDefinitions
    $rows = $GridElement.SelectNodes("Grid.RowDefinitions/RowDefinition")
    if ($rows.Count -eq 0) {
        $rows = @(@{ Height = "*" })  # Default
    }
    
    $rowSizes = Resolve-GridSizes -Definitions $rows -Available $AvailableHeight -Property "Height"
    
    # Parse ColumnDefinitions
    $cols = $GridElement.SelectNodes("Grid.ColumnDefinitions/ColumnDefinition")
    if ($cols.Count -eq 0) {
        $cols = @(@{ Width = "*" })  # Default
    }
    
    $colSizes = Resolve-GridSizes -Definitions $cols -Available $AvailableWidth -Property "Width"
    
    return @{
        Rows = $rowSizes
        Columns = $colSizes
    }
}

function Resolve-GridSizes {
    param(
        $Definitions,
        [int]$Available,
        [string]$Property
    )
    
    $sizes = @()
    $pixelTotal = 0
    $autoTotal = 0
    $starTotal = 0
    $starCount = 0
    
    # First pass: Calculate pixel and auto
    foreach ($def in $Definitions) {
        $value = $def.$Property
        if ($value -match "^(\d+)$") {
            # Pixel
            $sizes += @{ Type="Pixel"; Size=[int]$value }
            $pixelTotal += [int]$value
        }
        elseif ($value -eq "Auto") {
            # Auto (estimate)
            $estimated = 32  # Default estimate
            $sizes += @{ Type="Auto"; Size=$estimated }
            $autoTotal += $estimated
        }
        else {
            # Star (*)
            $starValue = if ($value -match "^(\d+)\*$") { [int]$matches[1] } else { 1 }
            $sizes += @{ Type="Star"; StarValue=$starValue; Size=0 }
            $starTotal += $starValue
            $starCount++
        }
    }
    
    # Second pass: Distribute remaining space to stars
    $remaining = $Available - $pixelTotal - $autoTotal
    if ($remaining -lt 0) { $remaining = 0 }
    
    for ($i = 0; $i -lt $sizes.Count; $i++) {
        if ($sizes[$i].Type -eq "Star") {
            $sizes[$i].Size = [int]($remaining * $sizes[$i].StarValue / $starTotal)
        }
    }
    
    return $sizes
}
```

#### C. Converter Pattern (All converters follow this)
```powershell
# Example: Convert-NumberBox.ps1
function Convert-NumberBox {
    param(
        [string]$Id,
        [string]$Name,
        [int]$X = 0,
        [int]$Y = 0,
        [int]$Width = 200,
        [int]$Height = 32,
        [string]$ParentId,
        [string]$FrameId,
        [string]$PageId,
        [string]$Header = "",
        [decimal]$Value = 0,
        [decimal]$Minimum = 0,
        [decimal]$Maximum = 99
    )
    
    # NumberBox is a group of:
    # 1. Header TextBlock (if header exists)
    # 2. Input box rectangle
    # 3. Value text
    # 4. Spin buttons (up/down arrows)
    
    $childShapes = @()
    $offsetY = 0
    
    # Header
    if ($Header) {
        $headerId = New-UUID
        $headerJson = Convert-TextBlock -Id $headerId -Name "${Name}_Header" `
            -X $X -Y $Y -Text $Header -FontSize 12 `
            -ParentId $Id -FrameId $FrameId -PageId $PageId
        Write-JsonFileSafe -FilePath (Join-Path $shapeDir "$headerId.json") -JsonContent $headerJson
        $childShapes += $headerId
        $offsetY = 20
    }
    
    # Input Box Background
    $boxId = New-UUID
    $boxJson = New-PenpotRect -Id $boxId -Name "${Name}_Box" `
        -X $X -Y ($Y + $offsetY) -Width $Width -Height 32 `
        -FillColor $script:PenpotColors.CardBackground `
        -StrokeColor $script:PenpotColors.InputBorder `
        -StrokeWidth 1 -CornerRadius 4 `
        -ParentId $Id -FrameId $FrameId -PageId $PageId
    Write-JsonFileSafe -FilePath (Join-Path $shapeDir "$boxId.json") -JsonContent $boxJson
    $childShapes += $boxId
    
    # Value Text
    $valueId = New-UUID
    $valueJson = New-PenpotText -Id $valueId -Name "${Name}_Value" `
        -X ($X + 8) -Y ($Y + $offsetY + 8) -Text "$Value" -FontSize 14 `
        -ParentId $Id -FrameId $FrameId -PageId $PageId
    Write-JsonFileSafe -FilePath (Join-Path $shapeDir "$valueId.json") -JsonContent $valueJson
    $childShapes += $valueId
    
    # Group Shape (parent of all above)
    return New-PenpotGroup -Id $Id -Name $Name `
        -X $X -Y $Y -Width $Width -Height ($Height + $offsetY) `
        -ChildShapes $childShapes `
        -ParentId $ParentId -FrameId $FrameId -PageId $PageId
}
```

### Phase 4: Critical Converters (Priority Order)

**MUST IMPLEMENT FIRST (used in POEntryView):**
1. **Convert-DataGrid.ps1** - Multi-column data table
   - Generate header row with column names
   - Generate 3-5 sample data rows
   - Use column dividers (vertical lines)
   - Example: `Controls:DataGrid` with columns "Part ID", "Description", "Qty"

2. **Convert-NumberBox.ps1** - Numeric input (used in LoadEntryView)
   - Header text above
   - Input box with border
   - Spin buttons on right

3. **Convert-InfoBar.ps1** - Message bar
   - Colored background based on severity
   - Icon on left
   - Title and message text

**MUST IMPLEMENT NEXT:**
4. **Convert-PasswordBox.ps1** - Masked input
5. **Convert-ComboBox.ps1** - Dropdown selector
6. **Convert-ProgressRing.ps1** - Circular spinner
7. **Convert-CommandBar.ps1** - Toolbar
8. **Convert-AppBarButton.ps1** - Toolbar button

**Container Converters:**
9. **Convert-ScrollViewer.ps1** - Treat as frame with dashed border
10. **Convert-UserControl.ps1** - Root container (frame)
11. **Convert-Page.ps1** - Root container (frame)
12. **Convert-Window.ps1** - Root container (frame)
13. **Convert-ContentDialog.ps1** - Modal overlay

### Phase 5: Packaging & Validation

```powershell
# After all shapes generated:

# 1. Validate every JSON file
Get-ChildItem $tempDir -Recurse -Filter "*.json" | ForEach-Object {
    try {
        $content = Get-Content $_.FullName -Raw
        $null = ConvertFrom-Json $content -ErrorAction Stop
        Write-Host "  ✓ Valid: $($_.Name)" -ForegroundColor Green
    }
    catch {
        Write-Host "  ✗ Invalid JSON: $($_.Name)" -ForegroundColor Red
        Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Yellow
        throw "Validation failed"
    }
}

# 2. Create ZIP with forward slashes
$zip = [System.IO.Compression.ZipFile]::Open($outputPath, 'Create')
try {
    Get-ChildItem $tempDir -Recurse -File | ForEach-Object {
        $relativePath = $_.FullName.Substring($tempDir.Length + 1).Replace('\', '/')
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $zip, $_.FullName, $relativePath, 'Optimal'
        ) | Out-Null
    }
}
finally {
    $zip.Dispose()
}

# 3. Verify ZIP integrity
try {
    $testZip = [System.IO.Compression.ZipFile]::OpenRead($outputPath)
    Write-Host "✓ ZIP contains $($testZip.Entries.Count) files" -ForegroundColor Green
    $testZip.Dispose()
}
catch {
    Write-Host "✗ ZIP verification failed: $($_.Exception.Message)" -ForegroundColor Red
    throw
}
```

## 4. Specific Fixes for Previous Failures
-   **Massive Canvas Issue:** Ensure the Root Board shape has explicit `width` (1400) and `height` (900). Do not use `0.01`.
-   **Missing Elements:** You **MUST** populate the `shapes` array of the parent Frame with the UUIDs of all its children. If this array is empty, the board will appear empty.
-   **Visibility:** Default all backgrounds to `#FFFFFF` (White) and all borders/text to `#000000` (Black) or `#333333` (Dark Gray) to ensure visibility.

## 6. Constraints & "Do Not" Checklist

### Visual Fidelity Requirements
-   **NO White-on-White:** Never generate a shape with `#FFFFFF` fill on a `#FFFFFF` board without a visible border. Always use `#E0E0E0` or darker for backgrounds, or add a `#000000` stroke with minimum 1px width.
-   **NO Massive Canvas:** The root frame must match the XAML design size (typically 1400x900). Never use `0.01`, `1.0`, or values > 10000.
-   **NO Empty Shapes Array:** A parent frame/group MUST list its children in the `shapes` array. If this is missing, the children will exist in the file but be invisible.
-   **NO Missing Elements:** Every XAML element must produce a visual representation. If you don't have a specific converter, use `Convert-Fallback.ps1` to create a placeholder rectangle with the element name.
-   **NO Silent Failures:** Log a warning for every unknown element type, but still generate a fallback shape.

### Code Quality Requirements
-   **NO Monolithic Script:** Do not put all logic in one file. Use `DotSource` (`. $scriptPath`) to load converter functions from a `Converters/` subdirectory.
-   **NO BOM:** Write JSON files as UTF-8 without BOM to ensure Penpot compatibility.
-   **NO Unvalidated JSON:** Validate every JSON file with `ConvertFrom-Json` before adding to ZIP.
-   **NO Hardcoded UUIDs:** Generate fresh UUIDs for every run using `[guid]::NewGuid()`.
-   **NO Forward Slash Mixing:** ZIP archive paths must use forward slashes `/` exclusively, never backslashes `\`.

### Data Integrity Requirements
-   **MUST: Bidirectional Linking:** Every child shape must have `"parent-id": "{parent-uuid}"` AND parent must list child in its `shapes: ["{child-uuid}"]` array.
-   **MUST: Frame References:** Shapes inside a Frame (Board) must set `"frame-id": "{board-uuid}"`.
-   **MUST: Relative Coordinates:** Child shape coordinates are relative to parent's origin, not absolute.
-   **MUST: Valid UUID Format:** All IDs must be lowercase UUID v4 (8-4-4-4-12 hex digits).
-   **MUST: ProseMirror Text:** Text content must use the nested JSON structure, never plain strings.

### Error Handling Requirements
-   **MUST: Log Progress:** Write progress messages for each major step (Parsing, Layout, Conversion, Validation, Packaging).
-   **MUST: Fail Fast:** If JSON validation fails for any file, stop immediately and report which file failed.
-   **MUST: Provide Context:** Error messages must include element name, XAML line number (if available), and expected vs. actual values.
-   **MUST: Graceful Degradation:** If a property is missing (e.g., no Width specified), use a sensible default and log a warning.

### Performance Requirements
-   **SHOULD: Cache Parsing:** If processing multiple XAML files, cache shared resources (color palette, default sizes).
-   **SHOULD: Parallel Processing:** When generating multiple .penpot files (e.g., all views), use PowerShell jobs to parallelize.
-   **MUST NOT: Block UI:** This is a command-line tool, but provide progress indicators for operations > 2 seconds.

## 7. Deliverables & Success Criteria

### Required Deliverables

1. **Main Script:** `Generate-Penpot-v2.ps1`
   - Command-line parameters: `-XamlFile`, `-OutputFile`, `-Verbose`
   - Must process any XAML file in the Views/ directory
   - Must generate a valid .penpot file that imports without errors

2. **Converter Modules (Minimum 20 converters):**
   - `Convert-Core.ps1` - Shared utilities
   - All controls listed in Section 1 "Complete WinUI 3 Control Reference"
   - `Convert-Fallback.ps1` - Unknown element handler

3. **Documentation:**
   - `Converters/README.md` - Updated with all converter signatures
   - `Mockups/TESTING.md` - Manual testing procedures
   - Inline comments explaining layout calculations

4. **Validation Tests:** `Tests/Test-Converters.ps1`
   - Unit tests for each converter
   - Integration test for full XAML→Penpot pipeline
   - Validation that output .penpot files are valid ZIP archives

### Success Criteria Checklist

**Code Completeness:**
- [ ] All controls from Section 1 have converters
- [ ] Grid layout engine correctly handles Pixel/Auto/Star sizing
- [ ] StackPanel layout engine positions children correctly
- [ ] Border, Padding, Margin are applied accurately
- [ ] Text uses ProseMirror JSON structure
- [ ] Colors are high-contrast and visible
- [ ] UUIDs are unique and valid
- [ ] Bidirectional parent-child linking is correct
- [ ] JSON files are UTF-8 without BOM

**Import Success:**
- [ ] Generated .penpot files open in Penpot web editor without errors
- [ ] All shapes are visible (no white-on-white)
- [ ] Canvas size matches XAML (e.g., 1400x900)
- [ ] Text is readable and correctly positioned
- [ ] Nested layouts (Grid in Border in StackPanel) work correctly
- [ ] DataGrid shows header + sample rows

**Testing:**
- [ ] POEntryView.penpot generated and imports successfully
- [ ] LoadEntryView.penpot generated and imports successfully
- [ ] All 17 XAML files in Views/ convert without errors
- [ ] Manual verification: DataGrid has columns and data rows
- [ ] Manual verification: NumberBox shows input box + spin buttons

**Performance:**
- [ ] Single XAML file converts in < 5 seconds
- [ ] All 17 files convert in < 1 minute
- [ ] No memory leaks (test with 100 consecutive runs)

### Testing Procedure

**Step 1: Unit Tests**
```powershell
.\Tests\Test-Converters.ps1
# Should pass all converter tests
```

**Step 2: Single File Test**
```powershell
.\Generate-Penpot-v2.ps1 -XamlFile "..\Views\Receiving\POEntryView.xaml" -Verbose
# Should create POEntryView.penpot
```

**Step 3: Import Validation**
1. Go to https://design.penpot.app/
2. Click "Import file"
3. Select POEntryView.penpot
4. Verify:
   - Board size is correct
   - All elements are visible
   - DataGrid has columns and rows
   - Text is readable
   - Colors are high-contrast

**Step 4: Batch Test**
```powershell
.\Generate-All-Penpot-v2.ps1 -CleanFirst
# Should generate 17 .penpot files
```

**Step 5: Regression Test**
- Keep a "golden" reference .penpot file
- After changes, compare new output to golden file
- Differences should be minimal (only UUIDs change)

### Debugging Guide

**Problem: "JSON error (end-of-file)" on import**
- **Cause:** Invalid JSON structure, likely missing closing brace
- **Fix:** Run JSON validation step, check the specific file reported
- **Prevention:** Use Write-JsonFileSafe for all JSON writes

**Problem: "Empty canvas" after import**
- **Cause:** Parent frame's `shapes` array is empty
- **Fix:** Verify parent-child linking in Generate-Penpot-v2.ps1
- **Prevention:** Log the shapes array contents during generation

**Problem: "White rectangle instead of control"**
- **Cause:** Missing converter or fallback not triggered
- **Fix:** Implement specific converter or verify fallback dispatcher
- **Prevention:** Check converter registry has all control types

**Problem: "Huge canvas (10000x10000)"**
- **Cause:** Grid layout calculation error, star sizing gone wrong
- **Fix:** Debug Calculate-GridLayout function with test data
- **Prevention:** Add bounds checking (max 5000px per dimension)

**Problem: "Text shows as [object Object]"**
- **Cause:** Text content is not using ProseMirror structure
- **Fix:** Use New-PenpotTextContent function, not plain strings
- **Prevention:** Add validation that text shapes have "content.type = doc"

### Example Output

After running `.\Generate-Penpot-v2.ps1 -XamlFile "POEntryView.xaml"`, you should see:

```
Generating Penpot file from XAML...
  Input:  ..\Views\Receiving\POEntryView.xaml
  Output: POEntryView.penpot

Loading converters from: .\Converters
  Loaded: Convert-Core.ps1
  Loaded: Convert-Border.ps1
  Loaded: Convert-Button.ps1
  ... (20+ converters)
Loaded 25 converter functions

Parsing XAML...
  Found: Grid (root)
  Found: ScrollViewer
  Found: StackPanel
  Found: Border (x4)
  Found: TextBox (x2)
  Found: Button (x4)
  Found: FontIcon (x6)
  Found: DataGrid (x1) ← CRITICAL
  Found: ProgressBar (x1)
Parsed XAML: 45 elements total

Calculating layout...
  Grid: 1 row (Auto, *), 1 column (*)
  StackPanel: 8 children, vertical, spacing=20
  DataGrid: 800x300, 5 columns
Layout calculated

Generating shapes...
  ✓ Border_Card1 (uuid-1234...)
  ✓ TextBox_PONumber (uuid-5678...)
  ✓ Button_LoadPO (uuid-9abc...)
  ✓ DataGrid_Parts (uuid-def0...) ← Generated header + 3 rows
  ... (45 shapes total)
Shapes generated

Validating JSON files...
  ✓ Valid: manifest.json
  ✓ Valid: uuid-1234....json
  ... (45 validations)
All JSON files validated successfully

Creating ZIP archive...
  Adding: manifest.json
  Adding: files/uuid-file/uuid-file.json
  ... (48 files)
ZIP archive created
✓ ZIP contains 48 files

Successfully created: POEntryView.penpot (125 KB)

Import this file into Penpot at https://design.penpot.app/
```

---

## Appendix A: Penpot Shape JSON Reference

### Frame (Board)
```json
{
  "id": "{uuid}",
  "name": "Board Name",
  "type": "frame",
  "x": 0,
  "y": 0,
  "width": 1400,
  "height": 900,
  "fills": [{"fillColor": "#FFFFFF"}],
  "strokes": [],
  "shapes": ["{child-uuid-1}", "{child-uuid-2}"],
  "parent-id": "{parent-uuid}",
  "frame-id": "{self-uuid}",
  "page-id": "{page-uuid}"
}
```

### Rectangle
```json
{
  "id": "{uuid}",
  "name": "Rectangle Name",
  "type": "rect",
  "x": 100,
  "y": 200,
  "width": 300,
  "height": 50,
  "r1": 4, "r2": 4, "r3": 4, "r4": 4,
  "fills": [{"fillColor": "#E0E0E0"}],
  "strokes": [{"strokeColor": "#8A8A8A", "strokeWidth": 1}],
  "parent-id": "{parent-uuid}",
  "frame-id": "{frame-uuid}",
  "page-id": "{page-uuid}"
}
```

### Text
```json
{
  "id": "{uuid}",
  "name": "Text Name",
  "type": "text",
  "x": 100,
  "y": 200,
  "width": 300,
  "height": 20,
  "content": {
    "type": "doc",
    "content": [
      {
        "type": "paragraph",
        "content": [
          {
            "type": "text",
            "text": "Actual text here"
          }
        ]
      }
    ]
  },
  "fills": [{"fillColor": "#333333"}],
  "parent-id": "{parent-uuid}",
  "frame-id": "{frame-uuid}",
  "page-id": "{page-uuid}"
}
```

### Group
```json
{
  "id": "{uuid}",
  "name": "Group Name",
  "type": "group",
  "x": 100,
  "y": 200,
  "width": 400,
  "height": 100,
  "shapes": ["{child-uuid-1}", "{child-uuid-2}"],
  "parent-id": "{parent-uuid}",
  "frame-id": "{frame-uuid}",
  "page-id": "{page-uuid}"
}
```

---

## Appendix B: WinUI Default Dimensions

Use these when Width/Height are not specified in XAML:

| Control | Default Width | Default Height | Notes |
|---------|---------------|----------------|-------|
| TextBox | 200 | 32 | Stretches if MaxWidth set |
| PasswordBox | 200 | 32 | Same as TextBox |
| NumberBox | 200 | 32 | With header: +20px height |
| ComboBox | 200 | 32 | Dropdown arrow: 32x32 |
| Button | 120 | 32 | AccentButton: same |
| CheckBox | 150 | 20 | Icon: 20x20 |
| TextBlock | 300 | 20 | Auto-size based on text |
| FontIcon | 16 | 16 | Based on FontSize |
| ProgressBar | 300 | 4 | Indeterminate: same |
| ProgressRing | 32 | 32 | Spinner circle |
| InfoBar | 400 | 60 | Severity colors |
| DataGrid | 800 | 300 | Column widths vary |
| CommandBar | 800 | 48 | Button: 48x48 |
| Border | Wraps child | Wraps child | Add BorderThickness |
| Grid | Parent size | Parent size | Star (*) expands |
| StackPanel | Σ(children) | Σ(children) | + Spacing |
| ScrollViewer | MaxWidth | MaxHeight | Viewport bounds |

---

## Appendix C: Color Mapping

| WinUI ThemeResource | Hex Value | Penpot Usage |
|---------------------|-----------|--------------|
| CardBackgroundFillColorDefaultBrush | #FFFFFF | Card backgrounds |
| CardStrokeColorDefaultBrush | #E0E0E0 | Card borders |
| AccentTextFillColorPrimaryBrush | #0078D4 | Accent text, icons |
| TextFillColorPrimaryBrush | #333333 | Body text |
| TextFillColorSecondaryBrush | #616161 | Secondary text |
| SystemFillColorCriticalBrush | #C42B1C | Error messages |
| SystemFillColorSuccessBrush | #107C10 | Success messages |
| SystemFillColorAttentionBrush | #FF8C00 | Warning messages |

---

## Appendix D: Research Checklist

Before implementing, verify you have researched:

- [ ] Penpot v3 file format spec from `backend/src/app/binfile/v3.clj`
- [ ] Shape types and properties from `common/src/app/common/types/shape.cljc`
- [ ] ProseMirror text structure from `common/src/app/common/text.cljc`
- [ ] WinUI control templates from `microsoft-ui-xaml/dev/CommonStyles/Generic.xaml`
- [ ] Grid layout algorithm from `microsoft-ui-xaml/dev/Common/LayoutPanel.cpp`
- [ ] Sample .penpot file extracted and studied
- [ ] DataGrid structure from CommunityToolkit repo
- [ ] UUID v4 format specification
- [ ] UTF-8 without BOM encoding in PowerShell
- [ ] ZIP archive path separators (forward slash requirement)

---

**Final Note:** This is a comprehensive specification. Take time to research before coding. A well-researched implementation will be more robust than a quickly-written script. The goal is 100% XAML coverage with pixel-perfect visual fidelity in Penpot.
