# Feature Specification:  Dunnage Admin Interface

**Feature Branch**: `007-admin-interface`  
**Created**: 2025-12-26  
**Status**: Ready for Implementation  
**Parent Feature**: Dunnage Receiving System V2  
**Depends On**: 004-services-layer

## Overview

Create the administrative user interface for managing dunnage metadata:  Types, Specification Schemas, Parts, and Inventoried Parts List.  This interface is accessed via Settings > Dunnage Management and provides full CRUD operations with impact analysis for safe deletion.

**Architecture Principle**: Admin UI navigates within MainWindow content area (no modal dialogs or separate windows). Follows existing Settings page navigation pattern.

## User Scenarios & Testing

### User Story 1 - Admin Main Navigation Hub (Priority: P1)

As a **system administrator**, I need a main navigation page with 4 management areas (Types, Specs, Parts, Inventoried List) so that I can access all dunnage configuration features from one central location.

**Why this priority**:  Navigation hub is the entry point for all admin functionality. Without it, no configuration is accessible.

**Independent Test**:  Can be tested by navigating to Settings > Dunnage Management, verifying 4 navigation cards display, clicking each card, and confirming correct management view opens.

**Acceptance Scenarios**:

1. **Given** Settings page loads, **When** user clicks "Dunnage Management" expander, **Then** "Launch Dunnage Admin" button appears
2. **Given** Dunnage Admin button clicked, **When** admin main view loads, **Then** 4 cards display:  Manage Dunnage Types, Manage Specs per Type, Manage Dunnage Parts, Inventoried Parts List
3. **Given** admin main view, **When** user clicks "Manage Dunnage Types", **Then** type management view loads in same content area
4. **Given** any management view, **When** user clicks "Back to Management", **Then** main navigation hub reappears
5. **Given** main navigation cards, **When** displayed, **Then** each card shows icon, title, and description of functionality

---

### User Story 2 - Dunnage Type Management (Priority: P1)

As an **administrator**, I need to view, add, edit, and delete dunnage types with impact analysis so that I can maintain the type taxonomy safely.

**Why this priority**: Types are the foundation of the dunnage hierarchy. Type management must exist before specs and parts can be managed.

**Independent Test**: Can be tested by adding a new type "Container", editing its name to "Container Box", attempting to delete a type with parts (should block with impact count), and deleting an unused type (should succeed).

**Acceptance Scenarios**:

1. **Given** type management view loads, **When** displaying types list, **Then** DataGrid shows all types with columns: TypeName, DateAdded, AddedBy, LastModified, Actions (Edit/Delete buttons)
2. **Given** type management view, **When** user clicks "+ Add New Type", **Then** 3-step add type form appears (Step 1: Name, Step 2: Select default specs, Step 3: Optional custom specs)
3. **Given** add type form Step 1, **When** user enters "Container" and clicks Next, **Then** Step 2 displays checkboxes for default specs (Width, Height, Depth, IsInventoriedToVisual, Material, Color, etc.)
4. **Given** add type form Step 2 with specs selected, **When** user clicks "Save Type", **Then** type is created, spec schema is inserted, success message displays
5. **Given** type with 15 dependent parts, **When** user clicks Delete, **Then** confirmation dialog displays "⚠️ Warning: 15 parts use this type.  Deleting will remove all parts and transaction history. Type DELETE to confirm."

---

### User Story 3 - Specification Schema Management (Priority: P1)

*Implemented in Quick Add Type Dialog*

---

### User Story 4 - Dunnage Part Management (Priority:  P1)

As an **administrator**, I need to view, add, edit, and delete dunnage parts with filtering and search so that I can maintain the master parts catalog. 

**Why this priority**: Parts are the primary receiving entities. Part management is critical for system operation.

**Independent Test**: Can be tested by adding part "PALLET-60X48" with spec values, filtering by type "Pallet", searching for "60X48", editing spec values, and deleting unused part.

**Acceptance Scenarios**:

1. **Given** part management view loads, **When** displaying parts, **Then** DataGrid shows PartID, Type, spec columns (Width, Height, Depth, etc.), DateAdded, Actions with pagination (20 per page)
2. **Given** part management toolbar, **When** user selects "Filter Type:  Pallet", **Then** grid refreshes to show only Pallet parts
3. **Given** search box, **When** user enters "60X48", **Then** grid filters to parts with matching PartID or spec values
4. **Given** user clicks "+ Add New Part", **When** form loads, **Then** 3-step form appears (Step 1: Select Type, Step 2: Enter PartID, Step 3: Enter spec values with dynamic inputs based on type's schema)
5. **Given** part "PALLET-48X40" with 127 transaction records, **When** user clicks Delete, **Then** confirmation shows "⚠️ 127 transaction records reference this part. Deleting will orphan those records. Type DELETE to confirm."

---

### User Story 5 - Inventoried Parts List Management (Priority: P2)

As an **administrator**, I need to manage the list of parts requiring Visual ERP inventory tracking so that users receive appropriate notifications during data entry.

**Why this priority**:  Inventoried list drives data quality notifications. Priority P2 because core receiving works without it, but data quality suffers.

**Independent Test**: Can be tested by adding "PALLET-48X40" to inventoried list with method "Both", editing method to "Adjust In", and removing from list.

**Acceptance Scenarios**:

1. **Given** inventoried list view loads, **When** displaying list, **Then** DataGrid shows PartID, Type, InventoryMethod, Notes, DateAdded, AddedBy, Actions
2. **Given** inventoried list, **When** user clicks "+ Add Part to List", **Then** dialog opens with PartID ComboBox (searchable), InventoryMethod dropdown (Adjust In, Receive In, Both), Notes TextBox
3. **Given** add dialog, **When** user selects method "Both" and saves, **Then** part is added to inventoried_dunnage_list table
4. **Given** inventoried part listed, **When** user clicks Edit, **Then** dialog allows changing InventoryMethod and Notes only (PartID readonly)
5. **Given** inventoried part selected, **When** user clicks Remove, **Then** confirmation dialog displays "ℹ️ Part will no longer trigger inventory notifications. Continue?"

---

### Edge Cases

- What happens when trying to add duplicate type name?  (Validation error:  "Type name already exists")
- What happens when deleting type with specs but no parts? (Specs cascade delete automatically, allow deletion)
- What happens when adding spec field with duplicate key for same type? (Validation error:  "Spec key already exists for this type")
- What happens when editing part to change PartID that already exists? (Validation error: "PartID must be unique")
- What happens when adding part to inventoried list that's already listed? (Validation error: "Part already in inventoried list")

## Requirements

### Functional Requirements - Main Navigation

#### DunnageAdminMainView.xaml + DunnageAdminMainViewModel
- **FR-001**: View MUST display 4 navigation cards in 2x2 grid layout
- **FR-002**: Cards MUST show: Manage Dunnage Types, Manage Specs per Type, Manage Dunnage Parts, Inventoried Parts List
- **FR-003**:  Each card MUST display icon, title, description, and clickable button
- **FR-004**: ViewModel MUST manage visibility flags:  IsMainNavigationVisible, IsManageTypesVisible, IsManageSpecsVisible, IsManagePartsVisible, IsInventoriedListVisible
- **FR-005**:  ViewModel MUST provide navigation commands: NavigateToManageTypes, NavigateToManageSpecs, NavigateToManageParts, NavigateToInventoriedList, ReturnToMainNavigation
- **FR-006**:  Navigation MUST hide main hub and show selected management view (mutual exclusion)

### Functional Requirements - Type Management

#### DunnageAdminTypesView.xaml + DunnageAdminTypesViewModel
- **FR-007**: View MUST display DataGrid with types list (columns: TypeName, DateAdded, AddedBy, LastModified, Edit/Delete buttons)
- **FR-008**: View MUST provide toolbar:  "+ Add New Type" button, Back to Management button
- **FR-009**: ViewModel MUST load types from GetAllTypesAsync on initialization
- **FR-010**: ViewModel MUST provide ShowAddTypeCommand that displays add type multi-step form
- **FR-011**: ViewModel MUST provide ShowEditTypeCommand that opens edit dialog with pre-populated values
- **FR-012**: ViewModel MUST provide ShowDeleteConfirmationCommand that calls impact analysis (GetPartCountByTypeAsync, GetTransactionCountByTypeAsync)
- **FR-013**: Delete confirmation MUST display impact counts and require typing "DELETE" to confirm
- **FR-014**: ViewModel MUST provide SaveNewTypeCommand that calls InsertTypeAsync and refreshes grid
- **FR-015**: ViewModel MUST provide DeleteTypeCommand that calls DeleteTypeAsync after confirmation

#### DunnageAddTypeView (Multi-Step Form)
- **FR-016**: Step 1 MUST provide TextBox for TypeName with validation (not empty, unique)
- **FR-017**: Step 2 MUST provide checkboxes for default spec fields (Width, Height, Depth, IsInventoriedToVisual, Material, Color, MaxWeight, Stackable)
- **FR-018**: Step 3 MUST provide "+ Add Custom Spec" button and grid for additional specs (optional)
- **FR-019**: Form MUST create type record AND spec schema record in single transaction on Save
- **FR-020**:  Form MUST validate type name uniqueness before allowing save

### Functional Requirements - Spec Management

#### DunnageAdminSpecsView.xaml + DunnageAdminSpecsViewModel
- **FR-021**: View MUST display type selection ComboBox and "View Specs" button
- **FR-022**: View MUST display DataGrid with spec fields for selected type (columns: SpecKey, DataType, Required, DefaultValue, Edit/Delete buttons)
- **FR-023**: View MUST provide toolbar: "+ Add Spec Field", "Delete All Specs" buttons
- **FR-024**: ViewModel MUST load specs for selected type via GetSpecsForTypeAsync
- **FR-025**: ViewModel MUST provide ShowAddSpecCommand that opens spec field dialog
- **FR-026**:  ViewModel MUST provide ShowEditSpecCommand with pre-populated spec definition
- **FR-027**: ViewModel MUST provide DeleteSpecCommand that calls GetPartCountUsingSpecAsync for impact analysis
- **FR-028**:  ViewModel MUST provide DeleteAllSpecsForTypeCommand with confirmation
- **FR-029**:  Spec dialog MUST generate validation inputs based on DataType (MinValue/MaxValue for number, MaxLength for text)

### Functional Requirements - Part Management

#### DunnageAdminPartsView.xaml + DunnageAdminPartsViewModel
- **FR-030**: View MUST display DataGrid with parts (columns: PartID, Type, dynamic spec columns, DateAdded, Edit/Delete buttons)
- **FR-031**: View MUST provide toolbar: "+ Add New Part", Type filter ComboBox, Search TextBox
- **FR-032**:  View MUST provide pagination controls (20 items per page)
- **FR-033**: ViewModel MUST load parts from GetAllPartsAsync with optional filtering
- **FR-034**: ViewModel MUST provide FilterByTypeCommand that calls GetPartsByTypeAsync
- **FR-035**: ViewModel MUST provide SearchPartsCommand that calls SearchPartsAsync
- **FR-036**: ViewModel MUST provide ShowAddPartCommand that displays 3-step form
- **FR-037**: ViewModel MUST provide ShowDeleteConfirmationCommand that calls GetTransactionCountByPartAsync
- **FR-038**: Add part form Step 3 MUST dynamically generate spec input controls based on selected type's schema

#### DunnageAddPartView (Multi-Step Form)
- **FR-039**: Step 1 MUST provide type selection ComboBox
- **FR-040**: Step 2 MUST provide PartID TextBox with uniqueness validation
- **FR-041**: Step 3 MUST dynamically generate spec input controls (NumberBox for number, TextBox for text, CheckBox for boolean)
- **FR-042**: Step 3 MUST provide checkbox "Add to Inventoried Parts List" with method and notes inputs
- **FR-043**: Form MUST create part record AND optionally inventoried list record in transaction on Save

### Functional Requirements - Inventoried Parts List

#### DunnageAdminInventoriedView.xaml + DunnageAdminInventoriedViewModel
- **FR-044**: View MUST display DataGrid with inventoried parts (columns: PartID, Type, InventoryMethod, Notes, DateAdded, AddedBy, Edit/Remove buttons)
- **FR-045**: View MUST provide toolbar: "+ Add Part to List" button
- **FR-046**: ViewModel MUST load inventoried parts from GetAllInventoriedPartsAsync
- **FR-047**: ViewModel MUST provide ShowAddToListCommand that opens add dialog
- **FR-048**: ViewModel MUST provide ShowEditEntryCommand that opens edit dialog
- **FR-049**: ViewModel MUST provide ShowRemoveConfirmationCommand with soft warning (informational, not blocking)
- **FR-050**: Add dialog MUST provide searchable PartID ComboBox, InventoryMethod dropdown (Adjust In, Receive In, Both), Notes TextBox
- **FR-051**: Edit dialog MUST make PartID readonly (cannot change which part, only method/notes)

### Impact Analysis Requirements

- **FR-052**: Type delete MUST show counts:  dependent parts, dependent transactions, dependent specs
- **FR-053**:  Spec delete MUST show count: parts with this spec value
- **FR-054**: Part delete MUST show count: transaction records referencing this part
- **FR-055**: All delete confirmations with impact MUST require typing "DELETE" to confirm
- **FR-056**: Inventoried list remove MUST show informational warning only (no blocking)

### Validation Requirements

- **FR-057**:  Type name MUST be unique (case-insensitive check)
- **FR-058**: Spec key MUST be unique per type
- **FR-059**:  PartID MUST be unique globally
- **FR-060**: PartID MUST match pattern validation if specified (alphanumeric, hyphens, underscores)
- **FR-061**: Required spec fields MUST be filled before saving part
- **FR-062**: Default spec fields (Width, Height, Depth, IsInventoriedToVisual) CANNOT be deleted

## Success Criteria

### Measurable Outcomes

- **SC-001**: Administrator can add new type with specs in under 1 minute
- **SC-002**: Impact analysis displays accurate counts before all delete operations
- **SC-003**: Type/Spec/Part filtering and search return results in under 500ms
- **SC-004**: Part management pagination handles 1000+ parts efficiently
- **SC-005**:  All CRUD operations successfully persist to database and refresh UI
- **SC-006**: Validation errors display user-friendly messages and prevent invalid data
- **SC-007**: Multi-step forms maintain state when navigating between steps
- **SC-008**:  Inventoried list changes immediately affect receiving workflow notifications

## Non-Functional Requirements

- **NFR-001**: Admin views MUST navigate within MainWindow content area (no separate windows)
- **NFR-002**:  All forms MUST use x:Bind for compile-time binding
- **NFR-003**:  Delete confirmations MUST use ContentDialog for modal blocking
- **NFR-004**:  All DataGrids MUST support sorting by clicking column headers
- **NFR-005**: Multi-step forms MUST show progress indicator (Step X of Y)
- **NFR-006**: All admin operations MUST log to ILoggingService with user context

## Out of Scope

- ❌ Bulk import of types/specs/parts from CSV (manual CRUD only)
- ❌ Audit trail of who changed what when (basic metadata only:  EntryUser, AlterUser)
- ❌ Role-based access control (all users with Settings access can admin)
- ❌ Spec schema versioning (immediate update, no version history)
- ❌ Part duplication/cloning feature (manual copy only)
- ❌ Advanced search with multiple filters (simple text search only)

## Dependencies

- 004-services-layer (IService_MySQL_Dunnage for all CRUD operations)
- Project:  Settings page (add Dunnage Management section)
- NuGet: CommunityToolkit.WinUI.UI. Controls (DataGrid)
- NuGet: CommunityToolkit. Mvvm (commands, properties)

## Files to be Created

### Main Navigation
- `Views/Receiving/Admin/DunnageAdminMainView.xaml`
- `Views/Receiving/Admin/DunnageAdminMainView.xaml.cs`
- `ViewModels/Receiving/Admin/DunnageAdminMainViewModel. cs`

### Type Management
- `Views/Receiving/Admin/DunnageAdminTypesView. xaml`
- `Views/Receiving/Admin/DunnageAdminTypesView.xaml.cs`
- `ViewModels/Receiving/Admin/DunnageAdminTypesViewModel. cs`
- `Views/Receiving/Admin/DunnageAddTypeView.xaml`
- `Views/Receiving/Admin/DunnageAddTypeView.xaml.cs`
- `Views/Receiving/Admin/DunnageEditTypeDialog.xaml` (ContentDialog)
- `Views/Receiving/Admin/DunnageDeleteTypeConfirmationDialog.xaml` (ContentDialog)

### Spec Management
- `Views/Receiving/Admin/DunnageAdminSpecsView.xaml`
- `Views/Receiving/Admin/DunnageAdminSpecsView.xaml. cs`
- `ViewModels/Receiving/Admin/DunnageAdminSpecsViewModel.cs`
- `Views/Receiving/Admin/DunnageAddSpecFieldDialog.xaml` (ContentDialog)
- `Views/Receiving/Admin/DunnageEditSpecFieldDialog.xaml` (ContentDialog)
- `Views/Receiving/Admin/DunnageDeleteSpecConfirmationDialog.xaml` (ContentDialog)

### Part Management
- `Views/Receiving/Admin/DunnageAdminPartsView.xaml`
- `Views/Receiving/Admin/DunnageAdminPartsView.xaml.cs`
- `ViewModels/Receiving/Admin/DunnageAdminPartsViewModel.cs`
- `Views/Receiving/Admin/DunnageAddPartView.xaml`
- `Views/Receiving/Admin/DunnageAddPartView.xaml. cs`
- `Views/Receiving/Admin/DunnageEditPartDialog.xaml` (ContentDialog)
- `Views/Receiving/Admin/DunnageDeletePartConfirmationDialog.xaml` (ContentDialog)

### Inventoried Parts Management
- `Views/Receiving/Admin/DunnageAdminInventoriedView.xaml`
- `Views/Receiving/Admin/DunnageAdminInventoriedView.xaml.cs`
- `ViewModels/Receiving/Admin/DunnageAdminInventoriedViewModel.cs`
- `Views/Receiving/Admin/DunnageEditInventoriedEntryDialog.xaml` (ContentDialog)
- `Views/Receiving/Admin/DunnageRemoveInventoriedConfirmationDialog.xaml` (ContentDialog)

### Modified Files
- `Views/Shared/SettingsPage.xaml` - Add Dunnage Management section

## Review & Acceptance Checklist

### Requirement Completeness
- [x] All 4 management areas are fully specified (Types, Specs, Parts, Inventoried)
- [x] CRUD operations are defined for each entity
- [x] Impact analysis requirements are explicit for all delete operations
- [x] Multi-step forms are detailed (steps, inputs, validation)
- [x] Navigation flow is clearly mapped (hub → management view → hub)

### Clarity & Unambiguity
- [x] DataGrid columns are specified for each management view
- [x] Dialog input controls are defined (ComboBox, TextBox, CheckBox, NumberBox)
- [x] Validation rules are enumerated per form
- [x] Delete confirmation messages are verbatim specified
- [x] Impact analysis metrics are defined (count parts, count transactions)

### Testability
- [x] Each management area can be tested independently
- [x] CRUD operations can be verified by database queries
- [x] Impact analysis can be verified by creating dependencies and attempting delete
- [x] Success criteria are measurable (timing, accuracy, UI behavior)

### UX Quality
- [x] Navigation is intuitive (hub-based, clear back buttons)
- [x] Impact warnings prevent accidental data loss
- [x] Multi-step forms guide user through complex operations
- [x] Validation provides immediate feedback
- [x] Confirmations require explicit action ("DELETE" typing) for destructive operations

## Clarifications

### Resolved Questions

**Q1**: Should admin UI be modal dialog or navigate within MainWindow?   
**A1**: Navigate within MainWindow content area.  Matches existing Settings navigation pattern.  No separate windows. 

**Q2**: Should default specs (Width, Height, Depth, IsInventoriedToVisual) be deletable?  
**A2**:  No.  These are core specs.  Validation must prevent deletion.  Other specs can be deleted.

**Q3**: Should type/spec/part changes immediately affect receiving workflow?  
**A3**: Yes. No caching. ViewModels load fresh data each time.  Changes are immediate.

**Q4**:  Should delete confirmations be soft (Yes/No) or hard (type DELETE)?  
**A4**: Hard confirmation (type DELETE) for operations with impact counts. Soft confirmation (Yes/No) for inventoried list removal.

**Q5**: Should add part form allow creating type/specs inline?  
**A5**: No. Use Quick Add buttons that open separate dialogs without leaving form.  Same pattern as wizard workflow. 