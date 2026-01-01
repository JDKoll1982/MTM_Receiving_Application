# Cloud Agent Task: Dunnage System Improvements

## Overview

Implement improvements to the Dunnage Receiving System including enhanced manual entry UI, database schema updates for home locations, and Settings menu integration for admin interfaces.

---

## ⚠️ MANDATORY PRE-IMPLEMENTATION STEPS

**Before implementing ANY task in this document, you MUST complete the following steps in order:**

### Step 0: Agent Onboarding & Documentation Review

**Follow the complete onboarding process described in:**
- **File**: `.github/prompts/documentation.prepare.prompt.md`
- **Purpose**: Systematic familiarization with MTM Receiving Application architecture, constitutional rules, and development standards

**Critical Actions from Onboarding Process:**

1. **Activate Serena MCP Server** (FIRST - enables 80-90% token savings)
   ```
   mcp_oraios_serena_activate_project("MTM_Receiving_Application")
   mcp_oraios_serena_check_onboarding_performed()
   # If not performed: mcp_oraios_serena_onboarding()
   ```

2. **Read ALL Serena Project Memories** (replaces reading 50+ instruction files)
   ```
   mcp_oraios_serena_list_memories()
   mcp_oraios_serena_read_memory("project_overview")
   mcp_oraios_serena_read_memory("constitution_summary")
   mcp_oraios_serena_read_memory("architectural_patterns")
   mcp_oraios_serena_read_memory("dao_best_practices")
   mcp_oraios_serena_read_memory("mvvm_guide")
   mcp_oraios_serena_read_memory("coding_standards")
   mcp_oraios_serena_read_memory("forbidden_practices")
   mcp_oraios_serena_read_memory("infor_visual_constraints")
   mcp_oraios_serena_read_memory("error_handling_guide")
   # ... read all available memories
   ```

3. **Read Task-Specific Instruction Files** (only if not covered by memories)
   - `.github/instructions/window-sizing.instructions.md` (for UI tasks)
   - `.github/instructions/xaml-troubleshooting.instructions.md` (for XAML work)
   - Other relevant files as referenced in task requirements

### Step 1: Use Serena Semantic Tools Throughout Implementation

**Reference**: `.github/instructions/serena-semantic-tools.instructions.md`

**Why Serena is Required:**
- **80-90% token reduction** when navigating 300+ file codebase
- **Symbol-level precision** - Read/edit specific methods without loading entire files
- **Relationship discovery** - Find all usages before refactoring
- **Pattern validation** - Search for architectural violations efficiently

**Key Serena Tools for This Task:**

| Tool | Use Case in This Task |
|------|----------------------|
| `find_symbol` | Locate ViewModel methods, DAO methods, Model properties |
| `get_symbols_overview` | Understand DataGrid structure without reading full XAML |
| `find_referencing_symbols` | Check where `Model_DunnagePart` is used before schema change |
| `replace_symbol_body` | Update DAO method implementations precisely |
| `search_for_pattern` | Find all PO number TextBox controls for formatting consistency |
| `insert_before_symbol` / `insert_after_symbol` | Add new properties to Models/ViewModels |

**Examples for This Task:**

```bash
# 1. Find existing Manual Entry ViewModel to understand current structure
mcp_oraios_serena_find_symbol(
    name_path_pattern="Dunnage_ManualEntryViewModel",
    include_body=false,
    depth=1  # Get all methods/properties overview
)

# 2. Read specific method without loading entire 500-line ViewModel
mcp_oraios_serena_find_symbol(
    name_path_pattern="Dunnage_ManualEntryViewModel/LoadDataAsync",
    include_body=true
)

# 3. Find all PO number TextBox controls for formatting reference
mcp_oraios_serena_search_for_pattern(
    substring_pattern='<TextBox.*?Text=.*?[Pp][Oo].*?>',
    paths_include_glob="Views/Receiving/**/*.xaml",
    context_lines_before=2,
    context_lines_after=2
)

# 4. Check impact of adding home_location to Model_DunnagePart
mcp_oraios_serena_find_referencing_symbols(
    name_path="Model_DunnagePart",
    relative_path="Models/Dunnage/Model_DunnagePart.cs"
)

# 5. Update DAO method precisely
mcp_oraios_serena_replace_symbol_body(
    name_path="Dao_DunnagePart/InsertAsync",
    relative_path="Data/Dunnage/Dao_DunnagePart.cs",
    body="<new method implementation with home_location parameter>"
)
```

**Token Efficiency Mandate:**
- ✅ **DO**: Use Serena for all code navigation and editing
- ✅ **DO**: Read memories instead of re-reading instruction files
- ❌ **DON'T**: Use `read_file` for large C# files (use `find_symbol` instead)
- ❌ **DON'T**: Load entire files when you only need one method
- ❌ **DON'T**: Skip Serena activation - this is non-negotiable

---

## Task 1: Manual Entry View Improvements

**File**: `Views/Dunnage/Dunnage_ManualEntryView.xaml`

### Requirements

#### 1.1 Load Number Column
- **Current State**: Load # column starts at 0
- **Required Change**: Load # should start at 1 and increment sequentially
- **Implementation**: Update the binding or data source to use 1-based indexing

#### 1.2 Type Column - Convert to ComboBox
- **Current State**: Type column is a text input
- **Required Change**: 
  - Convert to `ComboBox` control
  - Display format: `{Icon} {TypeName}` (e.g., "📦 Pallet", "🎁 Container")
  - Data source: All dunnage types from `dunnage_types` table
  - Use `ItemTemplate` to show icon glyph and type name
- **ViewModel Binding**: Bind to `ViewModel.AvailableDunnageTypes` collection
- **Reference**: Check how icons are displayed in type selection views for consistent formatting

#### 1.3 Part ID Column - Conditional ComboBox
- **Current State**: Part ID is always editable text input
- **Required Change**:
  - Default state: **Read-only** (disabled/grayed out)
  - Enable condition: Only when Type column has a valid Dunnage Type selected
  - When enabled: Convert to `ComboBox` populated with parts for selected type
  - Data source: Parts filtered by `type_id` from `dunnage_parts` table
  - Display format: Part ID string (e.g., "PALLET-48X40", "CONTAINER-SM")
- **ViewModel Logic**: 
  - Add `SelectedType` property that triggers `AvailablePartsForType` to update
  - Implement filtering logic similar to `Dao_DunnagePart.GetByTypeAsync()`

#### 1.4 PO Number Formatting
- **Current State**: PO number input may have inconsistent formatting
- **Required Change**: 
  - Apply same formatting as Receiving Workflow PO inputs
  - **Reference Files**:
    - `Views/Receiving/Receiving_POEntryView.xaml` - Check PO TextBox formatting
    - `ViewModels/Receiving/Receiving_POEntryViewModel.cs` - Check any PO formatting logic
  - No validation required, just consistent visual formatting
- **Apply to**: All PO-related inputs and ListView columns in Dunnage views

#### 1.5 Home Location Field (NEW)
- **Current State**: Does not exist
- **Required Change**:
  - Add new **"Home Location"** column to DataGrid
  - Input type: `TextBox` or `ComboBox` (recommend ComboBox with common locations + custom entry)
  - Required when creating new Dunnage Part ID
  - Default suggestions: "Warehouse A", "Warehouse B", "Shipping Dock", "Storage Yard"
  - Allow custom text entry
- **Database Impact**: See Task 2 below

---

## Task 2: Database Schema Update - Home Location

### Requirements

#### 2.1 Schema Migration
- **Table**: `dunnage_parts`
- **New Column**: `home_location VARCHAR(100) NULL`
- **Migration File**: Create `Database/Migrations/0XX_add_home_location_to_dunnage_parts.sql`
- **Migration Content**:
```sql
-- Migration: Add home_location to dunnage_parts table
-- Date: 2025-12-30

USE mtm_receiving_application;

ALTER TABLE dunnage_parts 
ADD COLUMN home_location VARCHAR(100) NULL 
COMMENT 'Default storage location for this dunnage part';

-- Update existing records with placeholder
UPDATE dunnage_parts 
SET home_location = 'Not Specified' 
WHERE home_location IS NULL;
```

#### 2.2 Stored Procedure Updates

**Update Required Procedures**:

1. **`sp_dunnage_parts_insert.sql`**:
   - Add `p_home_location VARCHAR(100)` parameter
   - Include `home_location` in INSERT statement
   
2. **`sp_dunnage_parts_update.sql`**:
   - Add `p_home_location VARCHAR(100)` parameter
   - Include `home_location` in UPDATE statement

3. **`sp_dunnage_parts_get_all.sql`**:
   - Include `home_location` in SELECT statement

4. **`sp_dunnage_parts_get_by_id.sql`**:
   - Include `home_location` in SELECT statement

5. **`sp_dunnage_parts_get_by_type.sql`**:
   - Include `home_location` in SELECT statement

**Critical Requirements**:
- All SQL files must use `DROP PROCEDURE IF EXISTS` for idempotency
- All SQL files must be compatible with `Database/Deploy/Deploy-Database.ps1`
- Use `DELIMITER $$` pattern for procedure definitions
- Follow existing stored procedure structure in `Database/StoredProcedures/Dunnage/`

#### 2.3 Model Update
- **File**: `Models/Dunnage/Model_DunnagePart.cs`
- **Add Property**:
```csharp
[ObservableProperty]
private string _homeLocation = string.Empty;
```

#### 2.4 DAO Update
- **File**: `Data/Dunnage/Dao_DunnagePart.cs`
- **Update Methods**:
  - `InsertAsync()` - Add `homeLocation` parameter and include in SP call
  - `UpdateAsync()` - Add `homeLocation` parameter and include in SP call
  - `MapFromReader()` - Add mapping for `home_location` column

---

## Task 3: Settings Menu Integration for Admin Views

**File**: `MainWindow.xaml`

### Requirements

#### 3.1 Add Settings Menu Item
- **Location**: `NavigationView.MenuItems` section
- **Add After**: Existing menu items (Receiving Labels, Dunnage Labels, Carrier Delivery)
- **Structure**:
```xml
<NavigationViewItem Content="Settings">
    <NavigationViewItem.Icon>
        <FontIcon Glyph="&#xE713;" />
    </NavigationViewItem.Icon>
    <NavigationViewItem.MenuItems>
        <NavigationViewItem Content="Dunnage Admin - Types" Tag="Dunnage_AdminTypesView">
            <NavigationViewItem.Icon>
                <FontIcon Glyph="&#xE8B7;" />
            </NavigationViewItem.Icon>
        </NavigationViewItem>
        <NavigationViewItem Content="Dunnage Admin - Inventory" Tag="Dunnage_AdminInventoryView">
            <NavigationViewItem.Icon>
                <FontIcon Glyph="&#xE8F1;" />
            </NavigationViewItem.Icon>
        </NavigationViewItem>
    </NavigationViewItem.MenuItems>
</NavigationViewItem>
```

#### 3.2 Navigation Handler Update
- **File**: `MainWindow.xaml.cs`
- **Method**: `NavView_SelectionChanged`
- **Add Cases**:
  - `"Dunnage_AdminTypesView"` → Navigate to `Views.Dunnage.Dunnage_AdminTypesView`
  - `"Dunnage_AdminInventoryView"` → Navigate to `Views.Dunnage.Dunnage_AdminInventoryView`

#### 3.3 Page Title Updates
- When navigating to admin views, update `PageTitleTextBlock.Text`:
  - Types: "Dunnage Admin - Types"
  - Inventory: "Dunnage Admin - Inventory"

---

## Reference Files

### Architecture References
- **Spec**: `Documentation/FuturePlans/DunnageLabels/007-spec-admin-interface.md`
  - Follow navigation patterns described in spec
  - Admin UI should navigate within MainWindow content area (no modal dialogs)

### Existing Implementations to Reference
- **PO Formatting**: 
  - `Views/Receiving/Receiving_POEntryView.xaml`
  - `ViewModels/Receiving/Receiving_POEntryViewModel.cs`
- **Type/Part Selection Pattern**:
  - `Views/Dunnage/Dunnage_TypeSelectionView.xaml`
  - `ViewModels/Dunnage/Dunnage_TypeSelectionViewModel.cs`
- **DataGrid with ComboBox**:
  - `Views/Dunnage/Dunnage_ManualEntryView.xaml` (current implementation for reference)
- **Icon Display**:
  - `Converters/Converter_IconCodeToGlyph.cs`
  - Existing type selection views for icon + name formatting

### Database Deployment
- **Deployment Script**: `Database/Deploy/Deploy-Database.ps1`
- **Pattern to Follow**: Check existing migrations in `Database/Migrations/` for structure
- **Stored Procedure Pattern**: Check `Database/StoredProcedures/Dunnage/` for DROP/CREATE patterns

---

## Acceptance Criteria

### Manual Entry View
- [ ] Load # column displays 1, 2, 3, ... (not 0, 1, 2, ...)
- [ ] Type column is a ComboBox showing icon + type name
- [ ] Part ID column is disabled by default
- [ ] Part ID column becomes enabled ComboBox when Type is selected
- [ ] Part ID ComboBox only shows parts for selected Type
- [ ] PO number formatting matches Receiving Workflow
- [ ] Home Location column added to DataGrid
- [ ] Home Location is saved when creating/editing part entries

### Database
- [ ] Migration script adds `home_location` column successfully
- [ ] Migration script is idempotent (can run multiple times)
- [ ] All 5 stored procedures updated with home_location parameter
- [ ] `Deploy-Database.ps1` successfully deploys all changes
- [ ] Model and DAO updated to handle home_location

### Settings Menu
- [ ] Settings menu item appears in left navigation panel
- [ ] Settings menu expands to show sub-items
- [ ] "Dunnage Admin - Types" navigates to Dunnage_AdminTypesView
- [ ] "Dunnage Admin - Inventory" navigates to Dunnage_AdminInventoryView
- [ ] Page title updates correctly when navigating to admin views
- [ ] Navigation works seamlessly within MainWindow content area

---

## Technical Notes

### MVVM Pattern Compliance
- All business logic in ViewModels
- Use `[ObservableProperty]` and `[RelayCommand]` attributes
- Views are XAML-only with `x:Bind` (compile-time binding)
- No code-behind logic except UI setup

### Data Binding
- Use `x:Bind` instead of `Binding` for all XAML bindings
- Specify `Mode=TwoWay` for user inputs
- Specify `Mode=OneWay` for display-only fields

### Database Standards
- MySQL 8.x syntax
- All procedures use `DELIMITER $$` pattern
- All procedures use `DROP PROCEDURE IF EXISTS` for idempotency
- Follow existing naming conventions: `sp_[table]_[operation]`
- Include meaningful comments in SQL files

### Error Handling
- Use `IService_ErrorHandler.ShowUserErrorAsync()` for user-facing errors
- Use `IService_ErrorHandler.HandleException()` for caught exceptions
- Return `Model_Dao_Result` from all DAO methods

---

## Implementation Order

1. **Database First**: Migration and stored procedures
2. **Model & DAO**: Update C# data layer
3. **ViewModel Logic**: Type/Part filtering, home location handling
4. **View Updates**: Manual Entry DataGrid improvements
5. **Navigation**: Settings menu integration
6. **Testing**: Verify all changes with `Deploy-Database.ps1`

---

## Files to Create/Modify

### Create New Files
- `Database/Migrations/0XX_add_home_location_to_dunnage_parts.sql`

### Modify Existing Files
- `Views/Dunnage/Dunnage_ManualEntryView.xaml`
- `ViewModels/Dunnage/Dunnage_ManualEntryViewModel.cs`
- `Models/Dunnage/Model_DunnagePart.cs`
- `Data/Dunnage/Dao_DunnagePart.cs`
- `Database/StoredProcedures/Dunnage/sp_dunnage_parts_insert.sql`
- `Database/StoredProcedures/Dunnage/sp_dunnage_parts_update.sql`
- `Database/StoredProcedures/Dunnage/sp_dunnage_parts_get_all.sql`
- `Database/StoredProcedures/Dunnage/sp_dunnage_parts_get_by_id.sql`
- `Database/StoredProcedures/Dunnage/sp_dunnage_parts_get_by_type.sql`
- `MainWindow.xaml`
- `MainWindow.xaml.cs`

---

## Success Validation

Run the following to verify implementation:

```powershell
# 1. Deploy database changes
cd Database/Deploy
.\Deploy-Database.ps1

# 2. Build application
dotnet build /p:Platform=x64 /p:Configuration=Debug

# 3. Manual testing checklist:
# - Open Manual Entry view, verify Load # starts at 1
# - Select Type from ComboBox, verify Part ID becomes enabled
# - Verify Part ID shows only parts for selected Type
# - Enter Home Location, save, verify it persists in database
# - Open Settings menu, verify admin views are accessible
# - Navigate to admin views, verify they display correctly
```

---

**Project**: MTM Receiving Application  
**Branch**: `copilot/dunnage-improvements`  
**Framework**: WinUI 3 / .NET 8  
**Database**: MySQL 8.x  
**Architecture**: Strict MVVM with Dependency Injection
