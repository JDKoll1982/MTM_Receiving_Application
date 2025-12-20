# XAML to Penpot Generator

Scripts to generate `.penpot` files from XAML UI definitions for easy import into [Penpot](https://penpot.app/).

## Quick Start

### Generate a Single File

```powershell
.\Generate-Penpot.ps1 -XamlFile "..\Views\Receiving\POEntryView.xaml"
```

This creates `POEntryView.penpot` ready to import.

### Generate All XAML Files

```powershell
.\Generate-All-Penpot.ps1
```

Processes all XAML files in `Views/` and creates corresponding `.penpot` files.

### Clean and Regenerate All

```powershell
.\Generate-All-Penpot.ps1 -CleanFirst
```

Removes existing `.penpot` files first, then regenerates all.

## Usage

### Single File Generation

```powershell
.\Generate-Penpot.ps1 [-XamlFile <path>] [-OutputFile <name>]
```

**Parameters:**
- `-XamlFile` - Path to XAML file (required)
- `-OutputFile` - Custom output filename (optional, defaults to `<XamlName>.penpot`)

**Examples:**
```powershell
# Auto-detect output name
.\Generate-Penpot.ps1 -XamlFile "..\MainWindow.xaml"

# Custom output name
.\Generate-Penpot.ps1 -XamlFile "..\MainWindow.xaml" -OutputFile "MyDesign.penpot"

# List available XAML files
.\Generate-Penpot.ps1
```

### Batch Generation

```powershell
.\Generate-All-Penpot.ps1 [-ViewsPath <path>] [-CleanFirst]
```

**Parameters:**
- `-ViewsPath` - Path to Views folder (default: `"..\Views"`)
- `-CleanFirst` - Remove existing `.penpot` files before generating

**Examples:**
```powershell
# Generate all Views
.\Generate-All-Penpot.ps1

# Clean and regenerate
.\Generate-All-Penpot.ps1 -CleanFirst

# Custom Views path
.\Generate-All-Penpot.ps1 -ViewsPath "C:\MyProject\Views"
```

## What Gets Generated

The script parses XAML and creates Penpot shapes for:

- **Grids** → Frames with background colors
- **NavigationView** → Side panels (320px wide)
- **Buttons** → Rectangles with labels
- **TextBlocks** → Text elements

### Fallback Layout

If XAML parsing fails or no elements are found:
- Creates default 1400x900 board
- Adds NavigationPane (320px) + ContentArea (1080px)

## Importing to Penpot

1. Go to [https://penpot.app/](https://penpot.app/)
2. Click **"Import file"**
3. Select the generated `.penpot` file
4. Your UI mockup appears ready to edit!

## Technical Details

### File Format
- **ZIP archive** with `.penpot` extension
- Contains JSON files describing shapes, pages, and metadata
- Uses **UTF-8 without BOM** encoding
- **Forward slashes** (`/`) in paths (required by Penpot)

### Supported XAML Attributes
- `Width`, `Height` - Dimensions
- `Background` - Fill colors
- `x:Name`, `Name` - Element names
- `Canvas.Left`, `Canvas.Top` - Positioning
- `Content`, `Text` - Labels
- `FontSize` - Text sizing

### Generated Structure
```
MyView.penpot (ZIP)
├── manifest.json
├── files/
│   └── <file-id>/
│       ├── <file-id>.json
│       └── pages/
│           └── <page-id>/
│               ├── <page-id>.json
│               ├── 00000000-0000-0000-0000-000000000000.json (root)
│               ├── <board-id>.json
│               └── <shape-id>.json (one per UI element)
```

## Troubleshooting

### "XAML file not found"
- Check the path is correct
- Use relative paths from `Mockups/` folder

### "Import failed" in Penpot
- Verify `.penpot` file was created
- Check file size > 0 bytes
- Try regenerating with `-CleanFirst`

### Missing UI elements
- Script currently supports limited XAML elements
- Complex layouts may need manual adjustment in Penpot
- Check console for parsing warnings

## Examples

```powershell
# Generate MainWindow
.\Generate-Penpot.ps1 -XamlFile "..\MainWindow.xaml"

# Generate all receiving views
Get-ChildItem "..\Views\Receiving\*.xaml" | ForEach-Object {
    .\Generate-Penpot.ps1 -XamlFile $_.FullName
}

# Generate with custom names
.\Generate-Penpot.ps1 -XamlFile "..\Views\Receiving\POEntryView.xaml" -OutputFile "PO-Entry-Mockup.penpot"
```

## Notes

- Generated files are **mockups** - not pixel-perfect conversions
- Use Penpot to refine layouts, add interactions, and polish designs
- XAML → Penpot is **one-way** - changes in Penpot don't sync back to XAML
- Default dimensions: 1400x900 (if not specified in XAML)
