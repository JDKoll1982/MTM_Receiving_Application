# Migration Plan: Material.Icons.WinUI3 Implementation

**Feature**: Icon System Migration to Material Design Icons  
**Created**: 2025-12-29  
**Priority**: P2 (Enhancement)  
**Estimated Effort**: 4-6 hours

## Overview

Migrate all icon usage from Segoe Fluent Icons (FontIcon with Unicode glyphs) to Material.Icons.WinUI3 package for:
- **Consistent design language** across application
- **Better icon variety** (5000+ Material Design icons vs ~1000 Fluent icons)
- **Type-safe icon references** (enum-based instead of Unicode strings)
- **Easier maintenance** (named icons instead of hex codes like `&#xE8EC;`)

**Package**: [Material.Icons.WinUI3](https://www.nuget.org/packages/Material.Icons.WinUI3/)  
**Documentation**: https://github.com/SKProCH/Material.Icons

---

## Phase 1: Discovery & Analysis (Serena-Powered)

### Objective
Identify ALL icon usage locations across the codebase using Serena semantic tools for efficiency.

### Tasks

#### 1.1 Find All FontIcon XAML Usage
```bash
# Use Serena's pattern search to find all FontIcon elements in XAML
search_for_pattern(
    substring_pattern="FontIcon.*Glyph=",
    relative_path="Views/",
    restrict_search_to_code_files=false,
    paths_include_glob="**/*.xaml",
    context_lines_before=2,
    context_lines_after=2
)
```

**Expected Output**: List of all XAML files with FontIcon usage, showing:
- File path
- Line number
- Glyph code (e.g., `&#xE8EC;`)
- Surrounding context (tooltip, parent element)

**Estimated Matches**: 50-100 icon instances across:
- Navigation menu items
- Toolbar buttons
- Dialog headers
- Info panels
- Admin UI cards

---

#### 1.2 Find All Code-Behind Icon References
```bash
# Search for FontIcon creation in C# code
search_for_pattern(
    substring_pattern='new FontIcon.*Glyph',
    relative_path="",
    restrict_search_to_code_files=true,
    paths_include_glob="**/*.cs",
    context_lines_before=3,
    context_lines_after=3
)
```

**Expected Output**: C# files with dynamic icon creation (if any).

---

#### 1.3 Find All Converter Icon Usage
```bash
# Check if there are icon-related value converters
find_symbol(
    name_path_pattern="Icon",
    relative_path="Converters/",
    substring_matching=true,
    include_body=false
)
```

**Expected Output**: Converter classes that handle icon transformations:
- `Converter_IconCodeToGlyph.cs` (from database icon codes → Glyph)
- Any other icon-related converters

---

#### 1.4 Analyze IconPickerControl
```bash
# Get structure of existing icon picker
get_symbols_overview(
    relative_path="Views/Dunnage/Controls/IconPickerControl.xaml.cs",
    depth=1
)
```

**Expected Output**: Methods and properties in IconPickerControl.xaml.cs:
- Icon selection logic
- Recently used icons storage
- Search functionality

---

#### 1.5 Find Database Icon Storage
```bash
# Search for icon field usage in models
search_for_pattern(
    substring_pattern="Icon.*string",
    relative_path="Models/",
    restrict_search_to_code_files=true,
    paths_include_glob="**/*.cs",
    context_lines_before=2,
    context_lines_after=1
)
```

**Expected Output**: Model properties storing icon data:
- `Model_DunnageType.Icon` (stores icon identifier in database)
- Any other icon-related properties

---

### Phase 1 Deliverables
Create `specs/011-material-icons-migration/icon-inventory.md` with:
- Complete list of all icon usage locations
- Current icon mapping (Glyph code → Intended icon)
- Icon categories (navigation, actions, status, etc.)

**Estimated Time**: 1 hour

---

## Phase 2: Package Installation & Configuration

### Tasks

#### 2.1 Install NuGet Package
```powershell
dotnet add package Material.Icons.WinUI3
```

**Latest Version**: Check NuGet for current stable version

---

#### 2.2 Register Package in App.xaml
```xml
<!-- Add to App.xaml -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- Existing dictionaries -->
            
            <!-- Add Material Icons resources -->
            <ResourceDictionary Source="ms-appx:///Material.Icons.WinUI3/Themes/Generic.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

---

#### 2.3 Add Namespace to Common Views
Add to commonly used views (or create a base style):
```xml
xmlns:materialIcons="using:Material.Icons.WinUI3"
```

---

### Phase 2 Deliverables
- Package installed and referenced
- Material Icons available throughout application
- Build succeeds with new package

**Estimated Time**: 15 minutes

---

## Phase 3: Icon Mapping Strategy

### Objective
Create mapping from current Segoe Fluent icons to Material Design equivalents.

### Material.Icons.WinUI3 Usage Patterns

**In XAML:**
```xml
<!-- Old (Fluent) -->
<FontIcon Glyph="&#xE8EC;" />

<!-- New (Material) -->
<materialIcons:MaterialIcon Kind="Tag" />
```

**With Size/Color:**
```xml
<materialIcons:MaterialIcon 
    Kind="Package" 
    Width="24" 
    Height="24"
    Foreground="{ThemeResource AccentFillColorDefaultBrush}" />
```

**In Code-Behind:**
```csharp
// Old
var icon = new FontIcon { Glyph = "\uE8EC" };

// New
using Material.Icons;
var icon = new MaterialIcon { Kind = MaterialIconKind.Tag };
```

### Icon Mapping Reference

Create `specs/011-material-icons-migration/icon-mapping.md` with table:

| Current Glyph | Current Usage | Material Icon Kind | Notes |
|---------------|---------------|-------------------|-------|
| `&#xE8EC;` | Tag | `MaterialIconKind.Tag` | Direct match |
| `&#xE7C1;` | Flag | `MaterialIconKind.Flag` | Direct match |
| `&#xE734;` | Star | `MaterialIconKind.Star` | Direct match |
| `&#xEB51;` | Heart | `MaterialIconKind.Heart` | Direct match |
| `&#xE718;` | Pin | `MaterialIconKind.Pin` | Direct match |
| `&#xF133;` | Box | `MaterialIconKind.Package` | Close equivalent |
| `&#xE7B8;` | Package | `MaterialIconKind.PackageVariant` | More detail |
| `&#xF158;` | Cube | `MaterialIconKind.CubeOutline` | Outlined style |
| `&#xE8B7;` | Folder | `MaterialIconKind.Folder` | Direct match |
| `&#xE787;` | Calendar | `MaterialIconKind.Calendar` | Direct match |
| `&#xE715;` | Mail | `MaterialIconKind.Email` | Direct match |
| `&#xE8C9;` | Important | `MaterialIconKind.Alert` | Warning/alert |
| `&#xE7BA;` | Warning | `MaterialIconKind.AlertCircle` | Outlined alert |
| `&#xE946;` | Info | `MaterialIconKind.Information` | Info circle |
| `&#xE8A5;` | Document | `MaterialIconKind.FileDocument` | Direct match |
| `&#xEB9F;` | Image | `MaterialIconKind.Image` | Direct match |
| `&#xEC4F;` | Music | `MaterialIconKind.Music` | Direct match |
| `&#xE714;` | Video | `MaterialIconKind.Video` | Direct match |

**Reference**: https://pictogrammers.com/library/mdi/ for full Material Design icon library

---

### Phase 3 Deliverables
- Complete icon mapping table
- Visual comparison screenshots (optional)
- Decision on any icons requiring design review

**Estimated Time**: 1 hour

---

## Phase 4: IconPickerControl Refactor

### Objective
Replace IconPickerControl to use Material.Icons.WinUI3 with enum-based selection.

### Implementation Plan

#### 4.1 Update IconPickerControl.xaml

**Old Structure** (18 hardcoded FontIcon borders):
```xml
<Border Width="40" Height="40" ...>
    <FontIcon Glyph="&#xF133;" FontSize="24" />
</Border>
```

**New Structure** (ItemsRepeater with MaterialIconKind enum):
```xml
<UserControl
    x:Class="MTM_Receiving_Application.Views.Dunnage.Controls.IconPickerControl"
    xmlns:materialIcons="using:Material.Icons.WinUI3">

    <StackPanel Spacing="12">
        
        <!-- Recently Used Section -->
        <StackPanel Spacing="8">
            <TextBlock Text="Recently Used" FontWeight="SemiBold" FontSize="12" />
            <GridView
                x:Name="RecentIconsGrid"
                ItemsSource="{x:Bind RecentIcons, Mode=OneWay}"
                SelectedItem="{x:Bind SelectedIconKind, Mode=TwoWay}"
                SelectionMode="Single"
                MaxHeight="60">
                <GridView.ItemTemplate>
                    <DataTemplate x:DataType="materialIcons:MaterialIconKind">
                        <Border Width="40" Height="40" 
                                BorderThickness="1" 
                                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}" 
                                CornerRadius="4" 
                                Padding="4">
                            <materialIcons:MaterialIcon Kind="{x:Bind}" Width="24" Height="24"/>
                        </Border>
                    </DataTemplate>
                </GridView.ItemTemplate>
            </GridView>
        </StackPanel>

        <!-- Search Box -->
        <TextBox
            x:Name="SearchBox"
            PlaceholderText="Search icons..."
            Text="{x:Bind SearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />

        <!-- All Icons Grid -->
        <GridView
            x:Name="AllIconsGrid"
            ItemsSource="{x:Bind FilteredIcons, Mode=OneWay}"
            SelectedItem="{x:Bind SelectedIconKind, Mode=TwoWay}"
            SelectionMode="Single"
            MaxHeight="300">
            <GridView.ItemTemplate>
                <DataTemplate x:DataType="materialIcons:MaterialIconKind">
                    <Border Width="40" Height="40" 
                            BorderThickness="1" 
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}" 
                            CornerRadius="4" 
                            Padding="4"
                            ToolTipService.ToolTip="{x:Bind}">
                        <materialIcons:MaterialIcon Kind="{x:Bind}" Width="24" Height="24"/>
                    </Border>
                </DataTemplate>
            </GridView.ItemTemplate>
        </GridView>

    </StackPanel>
</UserControl>
```

---

#### 4.2 Update IconPickerControl.xaml.cs

```csharp
using Material.Icons;
using Material.Icons.WinUI3;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MTM_Receiving_Application.Views.Dunnage.Controls;

public sealed partial class IconPickerControl : UserControl
{
    // Observable collections for binding
    public ObservableCollection<MaterialIconKind> AllIcons { get; } = new();
    public ObservableCollection<MaterialIconKind> FilteredIcons { get; } = new();
    public ObservableCollection<MaterialIconKind> RecentIcons { get; } = new();

    // Selected icon (bindable to parent)
    private MaterialIconKind _selectedIconKind = MaterialIconKind.HelpCircle;
    public MaterialIconKind SelectedIconKind
    {
        get => _selectedIconKind;
        set
        {
            if (_selectedIconKind != value)
            {
                _selectedIconKind = value;
                OnPropertyChanged();
                AddToRecentIcons(value);
            }
        }
    }

    // Search text
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                FilterIcons();
            }
        }
    }

    public IconPickerControl()
    {
        InitializeComponent();
        LoadAllIcons();
        LoadRecentIcons();
    }

    private void LoadAllIcons()
    {
        // Get all MaterialIconKind enum values
        var allIconKinds = Enum.GetValues<MaterialIconKind>()
            .Where(k => k != MaterialIconKind.None)
            .OrderBy(k => k.ToString())
            .ToList();

        AllIcons.Clear();
        foreach (var kind in allIconKinds)
        {
            AllIcons.Add(kind);
        }

        FilteredIcons.Clear();
        foreach (var kind in allIconKinds.Take(100)) // Show first 100 by default
        {
            FilteredIcons.Add(kind);
        }
    }

    private void LoadRecentIcons()
    {
        // TODO: Load from user preferences or local storage
        // For now, use common icons
        var commonIcons = new[]
        {
            MaterialIconKind.Package,
            MaterialIconKind.Tag,
            MaterialIconKind.Alert,
            MaterialIconKind.Calendar,
            MaterialIconKind.Folder,
            MaterialIconKind.Star
        };

        RecentIcons.Clear();
        foreach (var kind in commonIcons)
        {
            RecentIcons.Add(kind);
        }
    }

    private void FilterIcons()
    {
        FilteredIcons.Clear();

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            // Show first 100 if no search
            foreach (var kind in AllIcons.Take(100))
            {
                FilteredIcons.Add(kind);
            }
        }
        else
        {
            // Filter by name (case-insensitive)
            var searchLower = SearchText.ToLowerInvariant();
            var matches = AllIcons
                .Where(k => k.ToString().ToLowerInvariant().Contains(searchLower))
                .Take(100);

            foreach (var kind in matches)
            {
                FilteredIcons.Add(kind);
            }
        }
    }

    private void AddToRecentIcons(MaterialIconKind kind)
    {
        // Remove if already exists
        RecentIcons.Remove(kind);

        // Add to front
        RecentIcons.Insert(0, kind);

        // Keep only last 6
        while (RecentIcons.Count > 6)
        {
            RecentIcons.RemoveAt(RecentIcons.Count - 1);
        }

        // TODO: Persist to user preferences
    }

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

---

#### 4.3 Update Parent Dialog/View

**Before** (string-based):
```csharp
public string SelectedIcon { get; set; } = "&#xE8EC;";
```

**After** (enum-based):
```csharp
using Material.Icons;

public MaterialIconKind SelectedIcon { get; set; } = MaterialIconKind.Tag;

// When saving to database, convert to string
string iconForDatabase = SelectedIcon.ToString(); // "Tag"
```

---

#### 4.4 Update Database Storage Strategy

**Current**: Store Unicode glyphs like `&#xE8EC;` as strings in `dunnage_types.Icon` column.

**New Strategy**: Store MaterialIconKind enum name as string:
- Database value: `"Tag"`, `"Package"`, `"Alert"`
- In code: Parse to enum with `Enum.Parse<MaterialIconKind>(dbValue)`

**Migration**: Create SQL update script to convert existing glyph codes to Material icon names.

---

### Phase 4 Deliverables
- IconPickerControl fully refactored to Material.Icons.WinUI3
- Searchable icon grid with 5000+ icons
- Recently used icons persistence
- Enum-based type safety

**Estimated Time**: 2 hours

---

## Phase 5: XAML File Migration (Bulk Update)

### Objective
Replace all FontIcon elements with MaterialIcon in XAML files.

### Migration Strategy

Use multi-file search and replace with validation.

#### 5.1 Navigation Menu Items
```bash
# Find all NavigationViewItem icons
search_for_pattern(
    substring_pattern='NavigationViewItem.*FontIcon',
    relative_path="Views/",
    restrict_search_to_code_files=false,
    paths_include_glob="**/*.xaml"
)
```

**Manual Update** (each file):
```xml
<!-- Before -->
<NavigationViewItem Content="Dunnage Labels">
    <NavigationViewItem.Icon>
        <FontIcon Glyph="&#xE8EC;" />
    </NavigationViewItem.Icon>
</NavigationViewItem>

<!-- After -->
<NavigationViewItem Content="Dunnage Labels">
    <NavigationViewItem.Icon>
        <materialIcons:MaterialIcon Kind="Tag" Width="16" Height="16" />
    </NavigationViewItem.Icon>
</NavigationViewItem>
```

---

#### 5.2 Toolbar Buttons
```bash
# Find all AppBarButton icons
search_for_pattern(
    substring_pattern='AppBarButton.*Icon=.*FontIcon',
    relative_path="Views/",
    restrict_search_to_code_files=false,
    paths_include_glob="**/*.xaml"
)
```

**Template:**
```xml
<!-- Before -->
<AppBarButton Icon="Save" Label="Save All" />

<!-- After (if using custom icon) -->
<AppBarButton Label="Save All">
    <AppBarButton.Icon>
        <materialIcons:MaterialIcon Kind="ContentSave" Width="20" Height="20" />
    </AppBarButton.Icon>
</AppBarButton>
```

---

#### 5.3 Info Panel Icons
```bash
# Find all info/warning icons in StackPanels
search_for_pattern(
    substring_pattern='FontIcon.*Glyph="&#xE946;"',
    relative_path="Views/",
    restrict_search_to_code_files=false
)
```

**Update Pattern:**
```xml
<!-- Before -->
<FontIcon Glyph="&#xE946;" FontSize="16" />

<!-- After -->
<materialIcons:MaterialIcon Kind="Information" Width="16" Height="16" />
```

---

#### 5.4 Dialog Headers
Find all ContentDialog and TeachingTip icon usage.

---

### Automation Script (Optional)

Create PowerShell script for bulk text replacement:
```powershell
# specs/011-material-icons-migration/migrate-icons.ps1

$iconMappings = @{
    '&#xE8EC;' = 'Tag'
    '&#xE7C1;' = 'Flag'
    '&#xE734;' = 'Star'
    # ... (from mapping table)
}

Get-ChildItem -Path "Views" -Filter "*.xaml" -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    
    foreach ($mapping in $iconMappings.GetEnumerator()) {
        $oldPattern = "<FontIcon Glyph=`"$($mapping.Key)`""
        $newPattern = "<materialIcons:MaterialIcon Kind=`"$($mapping.Value)`""
        
        if ($content -match [regex]::Escape($oldPattern)) {
            Write-Host "Updating $($_.Name): $($mapping.Key) -> $($mapping.Value)"
            $content = $content -replace [regex]::Escape($oldPattern), $newPattern
        }
    }
    
    Set-Content -Path $_.FullName -Value $content
}
```

**⚠️ CRITICAL**: Review all automated changes before committing. Material icon sizes may differ from Fluent icons.

---

### Phase 5 Deliverables
- All XAML files updated to MaterialIcon
- Visual regression testing completed
- No FontIcon elements remain (except legacy compatibility)

**Estimated Time**: 1.5 hours

---

## Phase 6: Code-Behind Migration

### Objective
Update any dynamic icon creation in C# code.

### Tasks

#### 6.1 Find Dynamic Icon Creation
```bash
find_symbol(
    name_path_pattern="FontIcon",
    substring_matching=true,
    include_body=true
)
```

**Update Pattern:**
```csharp
// Before
var icon = new FontIcon 
{ 
    Glyph = "\uE8EC",
    FontSize = 16 
};

// After
using Material.Icons;
using Material.Icons.WinUI3;

var icon = new MaterialIcon 
{ 
    Kind = MaterialIconKind.Tag,
    Width = 16,
    Height = 16
};
```

---

#### 6.2 Update Converter_IconCodeToGlyph

**Current**: Converts database icon string → FontIcon Glyph  
**New**: Convert database icon string → MaterialIconKind enum

```csharp
// Converters/Converter_IconCodeToMaterialKind.cs
using Material.Icons;

public class Converter_IconCodeToMaterialKind : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string iconCode && !string.IsNullOrWhiteSpace(iconCode))
        {
            // Try parse enum name
            if (Enum.TryParse<MaterialIconKind>(iconCode, out var kind))
            {
                return kind;
            }
            
            // Fallback for legacy glyph codes (migration period)
            return ConvertLegacyGlyphToMaterialKind(iconCode);
        }
        
        return MaterialIconKind.HelpCircle; // Default
    }
    
    private MaterialIconKind ConvertLegacyGlyphToMaterialKind(string glyph)
    {
        return glyph switch
        {
            "&#xE8EC;" => MaterialIconKind.Tag,
            "&#xE7C1;" => MaterialIconKind.Flag,
            // ... (from mapping table)
            _ => MaterialIconKind.HelpCircle
        };
    }
}
```

---

### Phase 6 Deliverables
- All code-behind icon references updated
- Converters refactored to MaterialIconKind
- Legacy glyph fallback support (optional, for migration period)

**Estimated Time**: 45 minutes

---

## Phase 7: Database Migration

### Objective
Convert existing icon data from Unicode glyphs to Material icon enum names.

### Tasks

#### 7.1 Create Migration Script
```sql
-- Database/Migrations/011-material-icons-migration.sql

-- Update dunnage_types icon values
UPDATE dunnage_types
SET Icon = CASE Icon
    WHEN '&#xE8EC;' THEN 'Tag'
    WHEN '&#xE7C1;' THEN 'Flag'
    WHEN '&#xE734;' THEN 'Star'
    WHEN '&#xEB51;' THEN 'Heart'
    WHEN '&#xE718;' THEN 'Pin'
    WHEN '&#xF133;' THEN 'Package'
    WHEN '&#xE7B8;' THEN 'PackageVariant'
    WHEN '&#xF158;' THEN 'CubeOutline'
    WHEN '&#xE8B7;' THEN 'Folder'
    WHEN '&#xE787;' THEN 'Calendar'
    WHEN '&#xE715;' THEN 'Email'
    WHEN '&#xE8C9;' THEN 'Alert'
    WHEN '&#xE7BA;' THEN 'AlertCircle'
    WHEN '&#xE946;' THEN 'Information'
    WHEN '&#xE8A5;' THEN 'FileDocument'
    WHEN '&#xEB9F;' THEN 'Image'
    WHEN '&#xEC4F;' THEN 'Music'
    WHEN '&#xE714;' THEN 'Video'
    ELSE 'HelpCircle' -- Default for unknown icons
END
WHERE Icon IS NOT NULL;
```

---

#### 7.2 Verify Migration
```sql
-- Check for any unmigrated glyphs (should return 0 rows)
SELECT type_name, Icon
FROM dunnage_types
WHERE Icon LIKE '&#x%';
```

---

#### 7.3 Update Seed Data
```sql
-- Database/TestData/010-dunnage-complete-seed.sql
-- Update icon values in INSERT statements

INSERT INTO dunnage_types (type_name, Icon, date_added, added_by)
VALUES
    ('Pallet', 'PackageVariant', NOW(), 'SYSTEM'),
    ('Box', 'Package', NOW(), 'SYSTEM'),
    ('Container', 'CubeOutline', NOW(), 'SYSTEM'),
    -- ...
```

---

### Phase 7 Deliverables
- Migration script tested on dev database
- All existing icons converted to Material icon names
- Seed data updated for fresh installations

**Estimated Time**: 30 minutes

---

## Phase 8: Testing & Validation

### Objective
Comprehensive testing across all icon usage scenarios.

### Test Cases

#### 8.1 Visual Regression Testing
- [ ] Navigation menu icons display correctly
- [ ] Toolbar button icons render at correct size
- [ ] Info panel icons use proper color/theme
- [ ] Dialog header icons centered and visible
- [ ] Admin UI card icons match design

#### 8.2 Functional Testing
- [ ] IconPickerControl search returns results
- [ ] Recently used icons persist across sessions
- [ ] Icon selection updates database correctly
- [ ] Legacy glyph codes (if any remain) display fallback icon

#### 8.3 Performance Testing
- [ ] Icon picker loads <100ms with 5000+ icons
- [ ] Search filtering responds in real-time (<50ms)
- [ ] No memory leaks with large icon grids

#### 8.4 Cross-Theme Testing
- [ ] Icons visible in Light theme
- [ ] Icons visible in Dark theme
- [ ] Icon colors respect theme accent color
- [ ] High contrast mode support

---

### Test Data
- Test with all 11 dunnage types (should all have Material icons)
- Test icon picker with various search terms ("package", "alert", "folder")
- Test recently used icons with 10+ selections

---

### Phase 8 Deliverables
- All test cases pass
- Screenshots of before/after comparison
- Performance metrics documented

**Estimated Time**: 1 hour

---

## Phase 9: Documentation & Cleanup

### Objective
Update documentation and remove legacy code.

### Tasks

#### 9.1 Update Developer Documentation
Create `Documentation/ICONS.md`:
```markdown
# Icon Usage Guide

## Overview
This application uses [Material.Icons.WinUI3](https://www.nuget.org/packages/Material.Icons.WinUI3/) for all icon rendering.

## Adding Icons in XAML
```xml
xmlns:materialIcons="using:Material.Icons.WinUI3"

<materialIcons:MaterialIcon Kind="Package" Width="24" Height="24" />
```

## Adding Icons in Code
```csharp
using Material.Icons;
using Material.Icons.WinUI3;

var icon = new MaterialIcon 
{ 
    Kind = MaterialIconKind.Tag,
    Width = 16,
    Height = 16
};
```

## Icon Picker Control
Use `IconPickerControl` for user-selectable icons. Returns `MaterialIconKind` enum value.

## Database Storage
Icons are stored as enum names (e.g., "Package", "Alert") in VARCHAR columns.

## Icon Browser
Browse all 5000+ icons: https://pictogrammers.com/library/mdi/
```

---

#### 9.2 Update Architecture Documentation
Add to `.github/copilot-instructions.md`:
```markdown
### Icon Standards
- **Package**: Material.Icons.WinUI3
- **Type**: MaterialIconKind enum (type-safe)
- **Storage**: Enum names as strings ("Package", "Alert")
- **Rendering**: <materialIcons:MaterialIcon Kind="..." />
- **❌ Never use**: FontIcon with Unicode glyphs
```

---

#### 9.3 Remove Legacy Code (Optional)
- Remove `Converter_IconCodeToGlyph.cs` (if fully replaced)
- Remove Unicode glyph constants/helpers (if any)
- Remove FontIcon references from templates

---

#### 9.4 Update Tasks.md
Mark completion in `specs/011-material-icons-migration/tasks.md`:
```markdown
- [X] Phase 1: Discovery & Analysis
- [X] Phase 2: Package Installation
- [X] Phase 3: Icon Mapping
- [X] Phase 4: IconPickerControl Refactor
- [X] Phase 5: XAML Migration
- [X] Phase 6: Code-Behind Migration
- [X] Phase 7: Database Migration
- [X] Phase 8: Testing & Validation
- [X] Phase 9: Documentation & Cleanup
```

---

### Phase 9 Deliverables
- Complete developer documentation
- Updated architecture guide
- Clean codebase with no legacy icon code

**Estimated Time**: 30 minutes

---

## Risks & Mitigation

### Risk 1: Icon Size Mismatch
**Impact**: Material icons may render larger/smaller than Fluent icons at same size  
**Mitigation**: 
- Test all icon locations visually
- Adjust Width/Height properties as needed
- Create standard size constants (16, 20, 24, 32)

---

### Risk 2: Missing Icon Equivalents
**Impact**: Some Fluent icons may not have direct Material equivalents  
**Mitigation**:
- Review mapping table thoroughly
- Use closest visual match
- Document any compromises in icon-mapping.md

---

### Risk 3: Theme Color Compatibility
**Impact**: Material icons may not inherit theme colors correctly  
**Mitigation**:
- Explicitly set Foreground property when needed
- Test in Light/Dark/High Contrast themes
- Use theme resources for colors

---

### Risk 4: Database Migration Failure
**Impact**: Existing icons lost if migration script fails  
**Mitigation**:
- **BACKUP DATABASE** before running migration
- Test migration on dev database first
- Keep legacy converter as fallback during transition

---

### Risk 5: Performance with Large Icon Grids
**Impact**: Icon picker may lag with 5000+ icons loaded  
**Mitigation**:
- Use virtualization (GridView auto-virtualizes)
- Lazy-load icons (show 100 at a time)
- Implement efficient search filtering

---

## Rollback Plan

If migration causes critical issues:

1. **Revert NuGet Package**
   ```powershell
   dotnet remove package Material.Icons.WinUI3
   ```

2. **Restore Git Commit**
   ```powershell
   git checkout <commit-before-migration>
   ```

3. **Restore Database Backup**
   ```sql
   -- Restore from backup taken before Phase 7
   ```

4. **Keep Legacy Converter**
   - Don't delete `Converter_IconCodeToGlyph.cs` until migration is stable

---

## Success Criteria

- [ ] All FontIcon elements replaced with MaterialIcon
- [ ] IconPickerControl uses Material.Icons.WinUI3
- [ ] Database migration successful (0 legacy glyphs)
- [ ] All test cases pass
- [ ] No visual regressions reported
- [ ] Build succeeds with no warnings
- [ ] Performance metrics acceptable (<100ms icon picker load)
- [ ] Documentation updated

---

## Timeline

| Phase | Estimated Time | Dependencies |
|-------|----------------|--------------|
| 1. Discovery | 1 hour | None |
| 2. Package Install | 15 min | Phase 1 complete |
| 3. Icon Mapping | 1 hour | Phase 1 complete |
| 4. IconPicker Refactor | 2 hours | Phases 2, 3 complete |
| 5. XAML Migration | 1.5 hours | Phases 2, 3 complete |
| 6. Code-Behind | 45 min | Phase 5 complete |
| 7. Database Migration | 30 min | Phases 4, 5, 6 complete |
| 8. Testing | 1 hour | Phase 7 complete |
| 9. Documentation | 30 min | Phase 8 complete |

**Total Estimated Time**: 4.5 - 6 hours (depending on icon count and complexity)

---

## Next Steps

1. **Review this plan** with team/stakeholders
2. **Create backup** of current codebase and database
3. **Execute Phase 1** (Discovery) to get accurate icon count
4. **Refine icon mapping table** based on discovery results
5. **Begin implementation** starting with Phase 2

---

## References

- **Material.Icons.WinUI3 NuGet**: https://www.nuget.org/packages/Material.Icons.WinUI3/
- **GitHub Repository**: https://github.com/SKProCH/Material.Icons
- **Material Design Icons Browser**: https://pictogrammers.com/library/mdi/
- **MaterialIconKind Enum Reference**: https://github.com/SKProCH/Material.Icons/blob/master/Material.Icons/MaterialIconKind.cs
- **Serena Documentation**: https://oraios.github.io/serena/

---

**Plan Created**: 2025-12-29  
**Plan Author**: GitHub Copilot (Claude Sonnet 4.5)  
**Plan Status**: Ready for Review
