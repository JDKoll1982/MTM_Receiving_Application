# XAML to Penpot Converters

Modular PowerShell scripts for converting individual XAML UI elements to Penpot JSON shapes.

## Available Converters

Each converter is a standalone PowerShell script that outputs valid Penpot JSON for a specific UI element type.

### Basic Controls

- **Convert-TextBox.ps1** - TextBox → White rectangle with border
- **Convert-Button.ps1** - Button → Rounded rectangle (supports AccentButtonStyle)
- **Convert-TextBlock.ps1** - TextBlock → Text placeholder rectangle
- **Convert-CheckBox.ps1** - CheckBox → Small rounded square
- **Convert-ProgressBar.ps1** - ProgressBar → Thin rounded rectangle

### Layout Containers

- **Convert-Grid.ps1** - Grid → Frame container
- **Convert-StackPanel.ps1** - StackPanel → Transparent frame with layout hints
- **Convert-Border.ps1** - Border → Rectangle with border/fill/corner radius

### Visual Elements

- **Convert-FontIcon.ps1** - FontIcon → Circle shape (glyph placeholder)

## Usage

### Direct Invocation

```powershell
# Import the converter
. .\Converters\Convert-Button.ps1

# Call with parameters
$buttonJson = Convert-Button `
    -Id (New-Guid).ToString() `
    -Name "SubmitButton" `
    -X 100 `
    -Y 200 `
    -Width 120 `
    -Height 32 `
    -ParentId $parentId `
    -FrameId $frameId `
    -PageId $pageId `
    -Style "AccentButtonStyle" `
    -Content "Submit"

# Save to file
[System.IO.File]::WriteAllText("button.json", $buttonJson, (New-Object System.Text.UTF8Encoding $false))
```

### Parameters

All converters share common parameters:

| Parameter | Required | Description |
|-----------|----------|-------------|
| `Id` | Yes | Unique GUID for the shape |
| `Name` | Yes | Display name in Penpot |
| `X` | No | X position (default: 0) |
| `Y` | No | Y position (default: 0) |
| `Width` | No | Width in pixels |
| `Height` | No | Height in pixels |
| `ParentId` | Yes | Parent shape ID |
| `FrameId` | Yes | Frame ID |
| `PageId` | Yes | Page ID |

### Element-Specific Parameters

#### Convert-TextBox
- `PlaceholderText` - Placeholder text hint
- `MaxWidth` - Maximum width override

#### Convert-Button
- `Content` - Button text
- `Style` - "AccentButtonStyle" for blue button
- `Background` - Custom background color

#### Convert-TextBlock
- `Text` - Display text
- `FontSize` - Font size (affects height)
- `FontWeight` - "Normal" or "Bold"
- `Foreground` - Text color

#### Convert-Border
- `Background` - Fill color
- `BorderBrush` - Border color
- `BorderThickness` - Border width in pixels
- `CornerRadius` - Rounded corner radius
- `ChildIds` - Array of child shape IDs

#### Convert-StackPanel
- `Orientation` - "Vertical" or "Horizontal"
- `Spacing` - Gap between children
- `ChildIds` - Array of child shape IDs

#### Convert-Grid
- `Background` - Fill color
- `Padding` - Inner padding
- `ChildIds` - Array of child shape IDs

#### Convert-FontIcon
- `Glyph` - Unicode glyph code
- `FontSize` - Icon size
- `Foreground` - Icon color

#### Convert-CheckBox
- `Content` - CheckBox label
- `IsChecked` - Checked state (true/false)

#### Convert-ProgressBar
- `Value` - Current progress value
- `Maximum` - Maximum value

## Color Reference

Common WinUI theme colors used:

| Color | Hex Code | Usage |
|-------|----------|-------|
| Accent | `#0078D4` | Primary actions, focus |
| Card Background | `#FFFFFF` | Content cards |
| Border | `#E0E0E0` | Dividers, borders |
| Placeholder | `#E8E8E8` | Text placeholders |
| Default Button | `#F3F3F3` | Standard buttons |
| Stroke | `#8A8A8A` | Input borders |

## Example: Creating a Form

```powershell
$pageId = (New-Guid).ToString()
$frameId = (New-Guid).ToString()
$parentId = (New-Guid).ToString()

# Title
. .\Converters\Convert-TextBlock.ps1
$title = Convert-TextBlock -Id (New-Guid) -Name "Title" -X 20 -Y 20 -Width 300 -Height 30 `
    -ParentId $parentId -FrameId $frameId -PageId $pageId -Text "User Login" -FontSize 20

# Username input
. .\Converters\Convert-TextBox.ps1
$username = Convert-TextBox -Id (New-Guid) -Name "UsernameInput" -X 20 -Y 60 -Width 300 -Height 32 `
    -ParentId $parentId -FrameId $frameId -PageId $pageId -PlaceholderText "Enter username"

# Submit button
. .\Converters\Convert-Button.ps1
$submit = Convert-Button -Id (New-Guid) -Name "SubmitButton" -X 20 -Y 100 -Width 120 -Height 32 `
    -ParentId $parentId -FrameId $frameId -PageId $pageId -Style "AccentButtonStyle" -Content "Login"
```

## Integration with Generate-Penpot.ps1

The main generator can import and use these converters:

```powershell
# Load all converters
Get-ChildItem ".\Converters\*.ps1" | ForEach-Object { . $_.FullName }

# Parse XAML and create shapes
$buttons = $xaml.SelectNodes("//Button")
foreach ($button in $buttons) {
    $id = New-UUID
    $json = Convert-Button `
        -Id $id `
        -Name $button.GetAttribute("x:Name") `
        -Content $button.Content `
        -Style $button.Style `
        # ... other parameters
    
    # Save to Penpot structure
}
```

## Notes

- All converters output UTF-8 JSON (no BOM)
- Coordinates are absolute within the parent
- IDs must be unique GUIDs
- Shapes are simplified representations (e.g., FontIcon → circle)
- Complex controls may need multiple shapes

## Extending

To add a new converter:

1. Create `Convert-ElementName.ps1`
2. Follow the parameter pattern (Id, Name, X, Y, Width, Height, ParentId, FrameId, PageId)
3. Add element-specific parameters
4. Return valid Penpot JSON
5. Document in this README
