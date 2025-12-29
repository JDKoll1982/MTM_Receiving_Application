# Research: Dunnage Receiving System - Complete Implementation

**Feature**: Dunnage Receiving System  
**Date**: 2025-12-29  
**Phase**: 0 - Research & Decision Making

## Overview

This document consolidates research findings and technical decisions for implementing the complete Dunnage Receiving System. All NEEDS CLARIFICATION items from Technical Context have been resolved through codebase analysis, existing pattern evaluation, and technology research.

## Research Questions & Resolutions

### 1. DataGrid Performance with Dynamic Columns

**Question**: Can CommunityToolkit.WinUI.UI.Controls.DataGrid handle 100+ rows with dynamic spec columns generated at runtime?

**Research Findings**:
- DataGrid supports dynamic column generation via `Columns.Add()` at runtime
- Performance tested in existing Receiving module ManualEntryView with 50+ rows (acceptable performance)
- Virtualization enabled by default for rows (ItemsSource binding)
- Columns are NOT virtualized, but typical use case has <20 columns (fixed + spec columns)

**Decision**: **Use DataGrid with runtime column generation**
- Dynamically add spec columns in ViewModel after loading types/parts
- Use `DataGridTemplateColumn` with `CellTemplate` for custom editors (NumberBox, CheckBox)
- Bind columns collection to ObservableCollection in ViewModel for reactivity

**Rationale**: Existing pattern in Receiving module. Proven performance. Native WinUI 3 control with MVVM support.

**Alternatives Considered**:
- ❌ Custom ListView with Grid cells - More complex, no column headers
- ❌ Third-party grid control - Adds dependency, licensing concerns

---

### 2. CSV Export with RFC 4180 Compliance

**Question**: Does CsvHelper library support RFC 4180 escaping (commas, quotes, newlines) and dynamic column generation?

**Research Findings**:
- CsvHelper 33.0+ has full RFC 4180 support (quoting, escaping)
- Supports dynamic column headers via `WriteField()` and `WriteHeader()` methods
- Can write to stream for memory efficiency (important for 1,000+ row exports)
- Has UTF-8 BOM support for Excel/LabelView compatibility

**Decision**: **Use CsvHelper for all CSV export operations**
- Configuration: `CsvConfiguration` with `Delimiter = ","`, `Quote = '"'`, `HasHeaderRecord = true`
- Dynamic columns: Build header list from `GetAllSpecKeysAsync()`, then write rows with spec value lookup
- Escaping: Automatic via library (commas, quotes, newlines handled)

**Rationale**: Industry-standard library. Zero manual escaping logic. Proven in production environments.

**Alternatives Considered**:
- ❌ Manual CSV writing - Error-prone escaping logic, reinventing wheel
- ❌ System.Formats.Csv (new .NET 8) - Less mature, fewer features than CsvHelper

---

### 3. Icon Picker Implementation

**Question**: How to implement visual icon picker with search and categories using WinUI 3 controls?

**Research Findings**:
- Segoe Fluent Icons font bundled with WinUI 3 (500+ glyphs available)
- GridView with ItemsRepeater provides efficient grid layout with scrolling
- TextBox with TextChanged event for search filtering
- TabView or NavigationView for category tabs

**Decision**: **Use GridView with Segoe Fluent Icons and category filtering**
- Icon data: `List<Model_IconDefinition>` with Glyph (Unicode), Name, Category properties
- Layout: GridView with 6-column WrapGrid, 54x54px items
- Search: Filter IconsList based on TextBox input (matches Name)
- Categories: TabView with "All", "Containers", "Materials", "Warnings", "Tools" tabs
- Recently Used: Separate GridView section at top with 6 most-used icons (stored in user preferences)

**Rationale**: Native WinUI 3 controls. No external dependencies. Matches Fluent Design System.

**Alternatives Considered**:
- ❌ Custom icon font file - Deployment complexity, version management
- ❌ Image-based icons (PNG/SVG) - File size, scaling issues, accessibility concerns

---

### 4. Real-Time Validation Strategy

**Question**: How to implement 300ms debounced validation with inline error messages in WinUI 3?

**Research Findings**:
- CommunityToolkit.Mvvm supports `INotifyDataErrorInfo` via `ObservableValidator` base class
- DispatcherTimer can be used for debouncing (restart timer on each keystroke, validate when elapsed)
- WinUI 3 controls support `ErrorTemplate` property for custom error display
- Can also use InfoBar for non-blocking warnings (duplicate type name)

**Decision**: **Use manual validation with DispatcherTimer debouncing**
- ViewModel: Implement validation methods that update error properties (`TypeNameError`, `FieldNameError`)
- Debouncing: DispatcherTimer with 300ms interval, restart on PropertyChanged
- UI Binding: TextBox with custom ErrorTemplate or adjacent TextBlock for error message
- Visual feedback: Red border on invalid controls, error icon + message below field

**Rationale**: Flexible for complex multi-field validation. Debouncing prevents excessive validation calls. Works with existing BaseViewModel pattern.

**Alternatives Considered**:
- ❌ CommunityToolkit.Mvvm ObservableValidator - Less flexible for debouncing, harder to customize error display
- ❌ No debouncing - Too many validation calls, poor performance with database uniqueness checks

---

### 5. Drag-and-Drop Field Reordering

**Question**: How to implement drag-and-drop reordering of custom fields in the preview list?

**Research Findings**:
- ItemsRepeater supports drag-and-drop via `CanDragItems="True"` and `CanReorderItems="True"`
- DragItemsCompleted event provides old/new indexes for reordering
- ObservableCollection<T> supports Move() method for reordering without recreating collection
- DisplayOrder property on custom field models must be updated after reorder

**Decision**: **Use ItemsRepeater with drag-and-drop enabled**
- XAML: `<ItemsRepeater CanDragItems="True" CanReorderItems="True" DragItemsCompleted="OnFieldReordered">`
- ViewModel: ObservableCollection<Model_CustomFieldDefinition> for field preview
- Reorder Logic: In OnFieldReordered event, update DisplayOrder property (1, 2, 3, ...) based on new index
- Tab Order: UI tab order automatically follows visual order in ItemsRepeater

**Rationale**: Native WinUI 3 feature. Minimal code required. Matches modern UX expectations.

**Alternatives Considered**:
- ❌ Up/Down arrow buttons - Less intuitive, more clicks required, poor UX
- ❌ Manual drag-and-drop with pointer events - Complex implementation, accessibility issues

---

### 6. Dual-Path File Writing Strategy

**Question**: How to ensure local CSV write always succeeds while attempting network write as best-effort?

**Research Findings**:
- `File.WriteAllTextAsync()` is atomic on Windows (write to temp file, then rename)
- Network path reachability can be checked with `Directory.Exists()` with timeout via `Task.Run()` + `Task.WaitAsync()`
- IOException for file locking can be caught and retried with incremented filename
- User folder creation on network share requires `Directory.CreateDirectory()` with error handling

**Decision**: **Sequential write strategy with local-first approach**
1. Write to local path (`%APPDATA%\MTM_Receiving_Application\DunnageData.csv`) - MUST succeed or fail entire operation
2. Check network path reachability with 3-second timeout
3. If reachable, create user subdirectory if needed, write to network path
4. Catch network write exceptions, log as warning (don't fail operation)
5. Return `Model_CSVWriteResult` with `LocalSuccess`, `NetworkSuccess`, `ErrorMessage` properties

**Rationale**: Local-first ensures workflow continuity. Network write is optional enhancement. Clear error reporting for troubleshooting.

**Alternatives Considered**:
- ❌ Parallel writes - Risk of race conditions, complex error handling
- ❌ Network-only with local fallback - Blocks on network timeout, poor UX
- ❌ Queue-based background writing - Overengineering for this use case

---

### 7. Pagination Implementation

**Question**: Should pagination use existing IService_Pagination or implement custom logic in ViewModel?

**Research Findings**:
- Existing `IService_Pagination` in Services/Service_Pagination.cs
- Supports `SetSource()`, `GetCurrentPageItems()`, `NextPage()`, `PreviousPage()`, `GoToPage()`
- Used in Receiving EditModeViewModel successfully
- Handles TotalPages calculation, boundary checks, PageChanged event

**Decision**: **Reuse existing IService_Pagination**
- Inject into Dunnage_EditModeViewModel and Dunnage_AdminPartsViewModel
- Set ItemsPerPage to 50 (EditMode) and 20 (Admin)
- Bind UI to service properties (CurrentPage, TotalPages, HasNextPage, HasPreviousPage)
- Subscribe to PageChanged event to update ObservableCollection<T> in ViewModel

**Rationale**: DRY principle. Proven implementation. Consistent pagination UX across application.

**Alternatives Considered**:
- ❌ Custom pagination logic in ViewModel - Code duplication, maintenance burden
- ❌ Server-side pagination - Overengineering, adds complexity to stored procedures

---

### 8. Date Filtering Preset Logic

**Question**: How to calculate preset date ranges (This Week, This Month, This Quarter) with correct Monday-Sunday weeks?

**Research Findings**:
- .NET DateTime has `DayOfWeek` enum (Sunday=0, Monday=1, ...)
- Calendar week calculation: `int daysUntilMonday = ((int)DateTime.Now.DayOfWeek - 1 + 7) % 7;`
- Quarter calculation: `int quarter = (DateTime.Now.Month - 1) / 3;`
- Fiscal year alignment may differ from calendar year (MTM uses calendar year per spec)

**Decision**: **Use calendar-based calculations in ViewModel**
- This Week: Monday 00:00:00 to Sunday 23:59:59 of current week
- This Month: First day 00:00:00 to last day 23:59:59 of current month
- This Quarter: First day of quarter month 00:00:00 to last day of quarter 23:59:59
- Buttons display dynamic text: "This Week (Dec 23-29)", "This Month (December 2025)"

**Rationale**: Calendar weeks match user expectations. Dynamic button text provides clarity.

**Alternatives Considered**:
- ❌ ISO 8601 weeks (Monday-Sunday, week 1 has Jan 4th) - Confusing for US users
- ❌ Static button text - Less intuitive, requires user to remember date ranges

---

### 9. ContentDialog MaxHeight for No-Scroll Experience

**Question**: What MaxHeight ensures all content visible without scrolling at 1920x1080 with ≤5 custom fields?

**Research Findings**:
- 1920x1080 screen with Windows taskbar (40px) = 1040px available height
- Window chrome (title bar, margins) = ~80px
- ContentDialog margin from edges = ~100px each side (top/bottom) = 200px total
- Remaining for dialog content: 1040 - 80 - 200 = 760px

**Decision**: **MaxHeight="750" on ContentDialog root**
- At 750px height, all sections visible with ≤5 custom fields (tested estimation):
  - Header (title): 60px
  - Basic Information section: 120px (Type Name + Icon picker preview)
  - Icon Selection section: 200px (6x3 grid + search + tabs)
  - Custom Specifications section: 300px (5 fields @ 60px each)
  - Footer (buttons): 70px
  - **Total**: 750px
- If >5 fields, only Custom Specifications section scrolls (not entire dialog)

**Rationale**: Meets SC-001 requirement (no scrolling for ≤5 fields). Clear visual hierarchy. Scrolling isolated to variable content area.

**Alternatives Considered**:
- ❌ Fixed Height="750" - Prevents dialog from shrinking when fewer fields (wasted space)
- ❌ MaxHeight="900" - Would show scrollbar on 1080p screens, violating requirement

---

### 10. Field Name Sanitization for Database Columns

**Question**: How to sanitize user-entered field names (e.g., "Weight (lbs)") into valid database column names (e.g., "weight_lbs")?

**Research Findings**:
- MySQL column naming rules: Alphanumeric + underscore only, 64 char max, case-insensitive
- Display name stored separately from database column name in `custom_field_definitions` table
- Collision detection required (two fields like "Weight-lbs" and "Weight (lbs)" both sanitize to "weight_lbs")

**Decision**: **Sanitization logic in Service_MySQL_Dunnage.CreateTypeAsync()**
1. Convert to lowercase
2. Replace spaces/hyphens/parentheses/brackets with underscore
3. Remove special characters <>{}[]|\n\t
4. Trim leading/trailing underscores
5. Check for collision in existing fields for same type
6. If collision, append "_2", "_3", etc.
7. Store display name in `field_name` column, sanitized name in `database_column_name` column

**Rationale**: Centralized logic prevents inconsistencies. Display name preserved for UI. Database name guaranteed valid.

**Alternatives Considered**:
- ❌ User enters both display and database names - Poor UX, error-prone
- ❌ No sanitization, store display name as column - Fails for special characters

---

## Technology Decisions Summary

| Component | Technology Choice | Rationale |
|-----------|------------------|-----------|
| **DataGrid** | CommunityToolkit.WinUI.UI.Controls.DataGrid | Native WinUI 3, proven performance, MVVM support |
| **CSV Export** | CsvHelper 33.0+ | RFC 4180 compliance, dynamic columns, proven library |
| **Icon Picker** | GridView + Segoe Fluent Icons | Native controls, no dependencies, Fluent Design alignment |
| **Validation** | Manual with DispatcherTimer debouncing | Flexible, works with BaseViewModel, supports complex rules |
| **Drag-Drop** | ItemsRepeater with CanReorderItems | Native WinUI 3 feature, minimal code, good UX |
| **File Writing** | Sequential local-first, then network | Ensures local success, network as best-effort |
| **Pagination** | Existing IService_Pagination | DRY principle, proven implementation, consistency |
| **Date Filtering** | Calendar-based calculations | Matches user expectations, dynamic button text |
| **Dialog Sizing** | MaxHeight="750" | No scrolling at 1920x1080 with ≤5 fields |
| **Field Sanitization** | Lowercase + underscore replacement | Valid MySQL columns, preserves display name |

---

## Best Practices Research

### WinUI 3 DataGrid Performance Optimization
- Use virtualization (enabled by default for rows)
- Avoid complex CellTemplates (use simple TextBlock/TextBox/CheckBox)
- Bind columns collection to ObservableCollection for add/remove
- Use x:Bind for all bindings (compile-time, faster than Binding)

### CSV Export Efficiency
- Stream writing for large datasets (1,000+ rows)
- Build header list once, reuse for all rows
- Use `CsvWriter.WriteField()` in loop, `NextRecord()` after each row
- Flush and dispose StreamWriter to ensure file write completion

### ContentDialog Best Practices
- Set XamlRoot to current window for proper rendering
- Use ScrollViewer inside dialog for overflow content (not entire dialog)
- Implement Cancel handling with confirmation if unsaved changes
- Use PrimaryButton for main action, SecondaryButton for alternatives, CloseButton for cancel

### Validation UX Guidelines
- Red border + error message below field for hard errors
- Yellow InfoBar for warnings (duplicate name, non-blocking)
- Disable primary button until all required fields valid
- Focus first invalid field when validation fails

---

## Open Questions (None Remaining)

All technical context items marked "NEEDS CLARIFICATION" have been resolved through research and codebase analysis. No blocking questions remain for Phase 1 design.

---

## References

- [CommunityToolkit.WinUI.UI.Controls Documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/windows/controls/)
- [CsvHelper Documentation](https://joshclose.github.io/CsvHelper/)
- [WinUI 3 Gallery - Icons Sample](https://github.com/microsoft/WinUI-Gallery)
- [RFC 4180 - Common Format and MIME Type for CSV Files](https://www.ietf.org/rfc/rfc4180.txt)
- [.NET DateTime and Calendar Week Calculations](https://learn.microsoft.com/en-us/dotnet/api/system.datetime)
- [MySQL 5.7 Column Naming Rules](https://dev.mysql.com/doc/refman/5.7/en/identifiers.html)

---

**Phase 0 Complete** - All technology decisions documented. Ready for Phase 1 design (data-model.md, contracts/, quickstart.md).
