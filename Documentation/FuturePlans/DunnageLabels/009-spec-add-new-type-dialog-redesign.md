# Feature Specification: Add New Dunnage Type Dialog UI/UX Redesign

**Feature Branch**: `009-dunnage-add-type-dialog-redesign`  
**Created**: 2025-12-28  
**Status**: Draft  
**Input**: Comprehensive UI/UX redesign for Add New Dunnage Type Dialog with focus on eliminating scrolling, consistent styling, real-time validation, visual icon picker, and custom field preview

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Eliminate Scrolling for Standard Workflows (Priority: P1)

Maria Torres (Receiving Dock Supervisor) needs to create new dunnage types quickly (under 60 seconds) when vendors change packaging. The current dialog requires scrolling which breaks her flow and causes her to lose context of what was entered above.

**Why this priority**: This is the most critical usability issue blocking efficient workflows. 90% of dunnage types have ≤5 custom fields, and eliminating scrolling for this majority use case will reduce configuration time by ~40% and decrease entry errors.

**Independent Test**: Can be fully tested by creating a new dunnage type with 5 or fewer custom fields on a 1920x1080 monitor and verifying no vertical scrollbar appears and all elements are visible without scrolling.

**Acceptance Scenarios**:

1. **Given** a 1920x1080 monitor with standard taskbar (880px available height), **When** Maria opens the Add New Dunnage Type dialog, **Then** all form sections (Basic Information, Icon Selection, Custom Specifications) are visible without scrolling
2. **Given** Maria has entered Type Name and selected an icon, **When** she adds up to 5 custom fields, **Then** no vertical scrollbar appears and the dialog height remains ≤750px
3. **Given** Maria is tabbing through form fields, **When** she presses Tab, **Then** focus flows logically top-to-bottom without jumping between sections
4. **Given** Maria has added 6 or more custom fields, **When** the custom fields section exceeds available space, **Then** only the custom fields section displays a scrollbar (not the entire dialog)

---

### User Story 2 - Consistent Button Styling with Clear Visual Hierarchy (Priority: P1)

Maria Torres is confused by the current mix of button styles (rectangular "Add Type"/"Cancel" buttons vs rounded "Click to select icon" button), causing hesitation about which actions to take.

**Why this priority**: Inconsistent UI patterns create cognitive load and slow decision-making. This affects every dialog interaction and reduces user confidence. Standard button styling is a foundational UX principle.

**Independent Test**: Can be fully tested by opening the dialog and verifying all buttons use CornerRadius="4" and have clear visual hierarchy (primary > secondary > tertiary) through color and placement.

**Acceptance Scenarios**:

1. **Given** Maria opens the Add New Dunnage Type dialog, **When** she views all buttons, **Then** all buttons display with CornerRadius="4" (no mixed rectangular/rounded styles)
2. **Given** Maria is deciding what action to take, **When** she scans the button bar, **Then** she sees the primary action ("Add Type") in accent blue, right-aligned, with 140px minimum width
3. **Given** Maria wants to cancel, **When** she looks for the cancel action, **Then** she sees "Cancel" in neutral style, 120px minimum width, positioned left of the primary button
4. **Given** Maria needs to add a custom field, **When** she looks in the Custom Specifications section, **Then** she sees "Add Field" button with subtle styling and consistent 4px corner radius
5. **Given** Maria presses Enter while in the Type Name field, **When** the form is valid, **Then** the primary "Add Type" action is triggered (keyboard shortcut)

---

### User Story 3 - Custom Field Preview with Edit/Delete/Reorder (Priority: P1)

James Chen (Quality Control Manager) needs to verify that custom field structures match specifications before saving, and Maria Torres can't reorder fields after creation (must delete and re-add).

**Why this priority**: Creating complex dunnage types (8+ fields for new product lines) without preview leads to errors requiring deletion and recreation. Reordering capability saves 5+ minutes per complex type configuration.

**Independent Test**: Can be fully tested by adding 3 custom fields, verifying they appear in a styled preview list, then dragging to reorder, editing a field, and deleting a field.

**Acceptance Scenarios**:

1. **Given** James adds a new custom field named "Material" (Text, Required), **When** he clicks "Add Field", **Then** the field immediately appears in a styled preview card showing icon, name, type, and required status
2. **Given** James has added 3 custom fields, **When** he hovers over a field preview card, **Then** edit button, delete button, and drag handle become visible
3. **Given** James wants to reorder fields, **When** he drags the "Weight" field above "Material", **Then** the preview updates immediately and tab order adjusts accordingly
4. **Given** James realizes a field name is incorrect, **When** he clicks the edit button on a field preview, **Then** the field details populate in the "New Field" section for modification
5. **Given** James has not added any custom fields yet, **When** he views the Custom Specifications section, **Then** he sees a helpful message: "No custom fields yet. Click + Add Field to begin."
6. **Given** James has added 10 custom fields, **When** he attempts to add an 11th, **Then** a warning appears: "You have 10 custom fields. Consider if all are necessary (max 25)."
7. **Given** James has added 25 custom fields (maximum), **When** he attempts to add another, **Then** the "Add Field" button is disabled and an InfoBar appears: "Maximum 25 custom fields per type. Please remove a field to add another."

---

### User Story 4 - Real-Time Validation with Inline Feedback (Priority: P1)

David Wong (New Receiving Clerk) only discovers validation errors after clicking submit, forcing him to scan the entire form to find problems. This wastes time and frustrates new users during training.

**Why this priority**: Real-time validation is a modern UX standard that reduces errors by 60% and accelerates training. Blocking submission until valid ensures data quality and reduces support tickets.

**Independent Test**: Can be fully tested by entering invalid data (empty Type Name, special characters in Field Name) and verifying red borders and error messages appear immediately with 300ms debounce.

**Acceptance Scenarios**:

1. **Given** David starts typing in the Type Name field, **When** he clears all text or enters only whitespace, **Then** a red border appears and error message displays: "Type name is required"
2. **Given** David enters "Pallet" as Type Name and a type named "Pallet" already exists, **When** the field loses focus, **Then** a yellow warning icon appears with message: "A dunnage type named 'Pallet' already exists. Consider using a different name or editing the existing type." (non-blocking)
3. **Given** David is entering a Field Name, **When** he types 42 characters, **Then** a character counter displays below the field: "42/100 characters"
4. **Given** David enters special characters in Field Name (e.g., "Weight<lbs>"), **When** he types the invalid character, **Then** a red border appears and error displays: "Field name cannot contain special characters: <>{}[]|/\n\t"
5. **Given** David has entered two custom fields both named "Material", **When** he tries to add the duplicate, **Then** validation blocks the addition with error: "Duplicate field name. Each field must have a unique name."
6. **Given** David has validation errors (empty Type Name), **When** he attempts to click "Add Type", **Then** the button is disabled (grayed out) until all required fields are valid
7. **Given** David is typing in a validated field, **When** he pauses typing for 300ms, **Then** validation runs (not on every keystroke to avoid jitter)

---

### User Story 5 - Visual Icon Picker with Search and Categories (Priority: P2)

Maria Torres forgets which icon was selected after scrolling to the specifications section, and James Chen wants to maintain visual consistency across related dunnage types but can't remember icon names.

**Why this priority**: Visual icon selection is 3x faster than text-based selection and improves visual consistency across dunnage types. Recent icons feature accelerates repetitive configuration tasks.

**Independent Test**: Can be fully tested by opening the icon picker, searching for "box", filtering by category "Containers", selecting an icon from recently used section, and verifying the preview updates immediately.

**Acceptance Scenarios**:

1. **Given** Maria opens the dialog, **When** she views the Icon Selection section, **Then** she sees a visual preview of the currently selected icon (default: Box &#xE7B8;) with icon name
2. **Given** Maria wants to choose a different icon, **When** she views the icon picker, **Then** she sees a 6-column × 3-row grid (18 icons visible) organized in tabs: "Containers", "Materials", "Warnings", "Tools", "All"
3. **Given** Maria has used certain icons before, **When** she opens the icon picker, **Then** the top 6 most-used icons appear in a "Recently Used" section (persistent via user preferences)
4. **Given** Maria is looking for a specific icon, **When** she types "box" in the search box, **Then** only icons matching "box" keyword are displayed in the grid
5. **Given** Maria selects a new icon from the grid, **When** she clicks it, **Then** the selected icon preview updates immediately (no additional click required) and the icon has an accent border
6. **Given** Maria clicks in the Custom Specifications section, **When** she looks back at Icon Selection, **Then** the selected icon preview remains visible (sticky header or fixed position)

---

### User Story 6 - Duplicate Existing Dunnage Type (Priority: P2)

James Chen needs to create 8 similar dunnage types for a new product line, currently requiring 5 minutes each (40 minutes total) with repetitive manual entry.

**Why this priority**: Bulk configuration scenarios are common during product launches and vendor changes. Duplication reduces configuration time by 70% (from 5 min to 90 sec per type) and ensures consistency.

**Independent Test**: Can be fully tested by right-clicking an existing dunnage type in the main grid, selecting "Duplicate Type", verifying the dialog opens with pre-populated data including " (Copy)" suffix, modifying the name, and saving.

**Acceptance Scenarios**:

1. **Given** James right-clicks a dunnage type in the Dunnage_ManualEntryView grid, **When** the context menu appears, **Then** he sees "Duplicate Type" option
2. **Given** James clicks "Duplicate Type" on "Pallet 48x40", **When** the Add New Type dialog opens, **Then** Type Name is pre-populated with "Pallet 48x40 (Copy)", icon matches source, and all custom fields are copied in same order
3. **Given** James has duplicated a type with validation rules (e.g., Weight min:1 max:9999), **When** he views the custom fields, **Then** validation rules are copied to the new type
4. **Given** James modifies the duplicated type name to "Pallet 48x48", **When** he clicks "Add Type", **Then** a new type is created with the copied structure and modified name

---

### User Story 7 - Field Validation Rules Builder (Priority: P3)

James Chen wants to enforce data quality (e.g., Weight must be 1-9999 with 2 decimals) but can't set validation rules without coding custom logic.

**Why this priority**: Advanced validation reduces bad data entry by 80% and supports James's quality control mandate. However, basic validation (required/optional) covers most use cases, making this enhancement-level priority.

**Independent Test**: Can be fully tested by creating a Number field, setting min:1 max:9999 decimals:2, saving the type, then entering data in the main receiving workflow and verifying values outside the range are rejected.

**Acceptance Scenarios**:

1. **Given** James is adding a Number field named "Weight", **When** he expands "Validation Rules", **Then** he sees input fields for Min Value, Max Value, and Decimal Places (0-4)
2. **Given** James is adding a Text field named "Part Number", **When** he expands "Validation Rules", **Then** he sees Max Length input and pattern options (Starts with, Ends with, Contains, Custom Regex)
3. **Given** James is adding a Date field named "Manufacture Date", **When** he expands "Validation Rules", **Then** he sees Min Date and Max Date pickers with presets (Today, 30/60/90 days ago, Custom)
4. **Given** James has set validation rules for "Weight" (min:1, max:9999, decimals:2), **When** he views the field preview, **Then** a human-readable summary appears: "Number (1-9999, 2 decimals, Required)"
5. **Given** James has created a type with validation rules, **When** Maria enters data in the receiving workflow, **Then** values violating the rules are rejected in real-time with specific error messages

---

### Edge Cases

- **What happens when the user clicks "Add Type" without entering a Type Name?** The primary button remains disabled (grayed out), red border appears around Type Name textbox, error message "Type name is required" displays, and focus returns to Type Name field
- **What happens when the user enters a Type Name that already exists?** A yellow warning icon appears with message: "A dunnage type named '[Name]' already exists. Consider using a different name or editing the existing type." Submission is NOT blocked (user might want variant like "Pallet-Large"), with a "View Existing Type" link to open the existing type in edit mode
- **What happens when the user adds no custom fields?** Save is allowed (some types may not need custom fields), but a confirmation dialog appears: "You haven't added any custom fields. This type will only track basic information (PO, location, quantity). Continue?" with checkbox "Don't show this again"
- **What happens when the user tries to add a 26th custom field?** "Add Field" button is disabled after field #25, InfoBar appears: "Maximum 25 custom fields per type. Please remove a field to add another." Warning appears after field #10: "You have 10 custom fields. Consider if all are necessary (max 25)."
- **What happens when the user enters special characters in a Field Name?** Validation blocks characters: < > { } [ ] | \ / \n \t with error message "Field name cannot contain special characters: <>{}[]|\n\t". Parentheses, dashes, spaces are allowed (common in manufacturing like "Weight (lbs)" or "Heat #")
- **What happens when the user deletes all custom fields after adding several?** No error (valid state), empty state appears: "No custom fields added yet. Click + Add Field to define specifications for this type." with gentle informational icon
- **What happens when the icon library fails to load?** Fallback to placeholder icons (Unicode symbols), error logged to ApplicationErrorLog.csv, InfoBar displays: "Icon library unavailable. Default icons will be used.", dialog remains functional with text-based icon names
- **What happens when the user clicks Cancel with unsaved changes?** Confirmation dialog appears: "You have unsaved changes. Are you sure you want to cancel?" with buttons "Discard Changes" (red) and "Continue Editing" (primary). If no changes made, closes immediately without confirmation
- **What happens when the user resizes the window below minimum width?** Dialog enforces MinWidth="500", cannot be resized smaller. Icon grid collapses to 4 columns instead of 6, field preview list remains readable (horizontal scroll if needed)
- **What happens when database connection is lost during save?** Loading indicator appears (spinner on button), timeout after 10 seconds, error dialog: "Unable to save. Please check your network connection and try again." Data retained in dialog (user doesn't lose work), retry button available

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Dialog MUST display all form sections (Basic Information, Icon Selection, Custom Specifications) without vertical scrolling at 1920x1080 resolution with ≤5 custom fields (MaxHeight="750")
- **FR-002**: System MUST enforce consistent button styling with CornerRadius="4" on all buttons and clear visual hierarchy (primary accent blue, secondary neutral, tertiary subtle)
- **FR-003**: System MUST provide real-time validation for Type Name (required, max 100 chars) and Field Name (required, unique, max 100 chars, no special chars <>{}[]|\) with 300ms debounce
- **FR-004**: System MUST display custom field preview cards immediately after adding fields, showing icon (based on type), name, type, required status, with hover-activated Edit/Delete/Drag buttons
- **FR-005**: System MUST support drag-and-drop reordering of custom fields with automatic tab order adjustment
- **FR-006**: System MUST provide visual icon picker with 6-column grid, category tabs (All, Containers, Materials, Warnings, Tools), search filter, and recently used section (top 6 icons persistent via user preferences)
- **FR-007**: System MUST disable "Add Type" primary button until all required fields are valid (Type Name non-empty, no duplicate field names, no validation errors)
- **FR-008**: System MUST validate for duplicate Type Names with non-blocking yellow warning and "View Existing Type" link
- **FR-009**: System MUST enforce maximum 25 custom fields per type with warning at 10 fields and hard block at 25
- **FR-010**: System MUST prompt for confirmation when closing dialog with unsaved changes (any data entered in Type Name, icon changed, or custom fields added)
- **FR-011**: System MUST support keyboard shortcuts: Tab (navigation), Enter (submit when valid), Esc (cancel), Ctrl+Enter (submit), Ctrl+F (icon search), Alt+A (add field)
- **FR-012**: System MUST provide "Duplicate Type" context menu option in Dunnage_ManualEntryView that opens dialog with pre-populated data from selected type plus " (Copy)" suffix
- **FR-013**: System MUST sanitize Field Names for database storage (display "Weight (lbs)" → column "weight_lbs") while preserving display name
- **FR-014**: System MUST handle database connection failures gracefully with 10-second timeout, error dialog, data retention, and retry option

### Key Entities

- **Dunnage Type**: Represents a category of dunnage (e.g., Pallet, Crate, Blocking) with attributes: TypeID, TypeName, IconGlyph, CreatedDate, IsActive
- **Custom Field Definition**: Represents user-defined specifications for a dunnage type with attributes: FieldID, TypeID, FieldName, FieldType (Text/Number/Date/Boolean), DisplayOrder, IsRequired, ValidationRules (JSON)
- **Icon Definition**: Represents available icons in the visual picker with attributes: IconGlyph (Unicode), IconName, Category (Containers/Materials/Warnings/Tools)
- **User Icon Preference**: Tracks recently used icons per user with attributes: UserID, IconGlyph, UsageCount, LastUsedDate

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 90% of standard dunnage type configurations (≤5 custom fields) complete in under 60 seconds without scrolling (baseline: 90 seconds with scrolling)
- **SC-002**: Zero UI consistency complaints in user feedback after redesign (baseline: 4 complaints about mixed button styles in past 3 months)
- **SC-003**: 75% reduction in validation-related support tickets (baseline: 8 tickets/month for "didn't realize field was required" or "entered wrong format")
- **SC-004**: Custom field reordering feature used in 30% of multi-field type configurations within first month (measured via telemetry)
- **SC-005**: Visual icon picker search feature used in 50% of dialog sessions (indicates discoverability and usefulness)
- **SC-006**: Type duplication feature reduces bulk configuration time by 70% (from 5 min/type to 90 sec/type for James Chen's use case)
- **SC-007**: 95% of users successfully complete dunnage type creation on first attempt without errors (baseline: 78% success rate due to validation issues)

## Assumptions *(optional)*

- Users have 1920x1080 or higher resolution monitors (minimum supported: 1366x768)
- Average dunnage type has 3-5 custom fields (90% of use cases based on existing data)
- Icon library contains 50-100 distinct icons organized in 4 categories
- MySQL 5.7.24 compatibility means validation rules stored as JSON string (no JSON column type)
- User preferences are stored per-user in `user_preferences` table with JSON field
- Icon picker loads all icons at once (not paginated) due to small dataset size
- Recently used icons persist indefinitely (no automatic cleanup)
- Custom field DisplayOrder starts at 1 and increments by 1 (no gaps allowed)
- Sanitized Field Names for database columns must be unique within a type (enforced at DAO layer)
- Dialog is modal (blocks interaction with parent window until closed)

## Dependencies *(optional)*

- **Database Schema**: `dunnage_types` table with columns: `type_id`, `type_name`, `icon_glyph`, `created_date`, `is_active`, `is_deleted`
- **Database Schema**: `custom_field_definitions` table with columns: `field_id`, `type_id`, `field_name`, `field_type`, `display_order`, `is_required`, `validation_rules`, `database_column_name`
- **Database Schema**: `user_preferences` table with columns: `user_id`, `preference_key`, `preference_value` (JSON)
- **Stored Procedures**: `sp_dunnage_types_check_duplicate`, `sp_dunnage_types_insert`, `sp_custom_fields_insert`, `sp_user_preferences_upsert`, `sp_user_icon_usage_track`
- **Existing Components**: `BaseViewModel` (provides `IsBusy`, `StatusMessage`, error handling)
- **Existing Services**: `IService_ErrorHandler` (user error dialogs, exception handling), `ILoggingService` (audit logging)
- **ViewModel**: `Dunnage_AddNewTypeDialogViewModel` (new) implementing validation, field management, icon selection logic
- **Model**: `Model_CustomFieldDefinition` (new) representing field metadata
- **Model**: `Model_IconDefinition` (new) representing icon catalog entries
- **Icon Font**: Segoe Fluent Icons (bundled with WinUI 3) or custom icon font file
- **NuGet Package**: `CommunityToolkit.WinUI.UI.Controls` for ItemsRepeater (custom field preview)

## Out of Scope *(optional)*

- Advanced validation rule builder UI (P3 priority - deferred to future sprint)
- Bulk import from CSV/Excel (P3 priority - separate feature specification)
- Live preview panel showing actual receiving form layout (P3 priority - enhancement)
- Field templates for common dunnage categories (P2 priority - may be included if time permits)
- Editing existing dunnage types (separate feature - Edit Type Dialog)
- Deleting dunnage types (handled in Dunnage_ManualEntryView grid context menu)
- Internationalization/localization (English-only for initial release)
- Mobile/tablet responsive layout (desktop-only application)
- Accessibility features beyond standard WinUI 3 defaults (narrator support, high contrast themes)
- Undo/redo functionality within dialog
- Auto-save drafts (user must explicitly save or cancel)

---

## Constitution Compliance Check *(mandatory)*

### Principle Alignment

- [x] **MVVM Architecture**: UI requirements clearly separate presentation (XAML-only Views) from logic (ViewModel with `[ObservableProperty]`, `[RelayCommand]`, validation methods)
- [x] **Database Layer**: Data persistence requirements specify entities (dunnage_types, custom_field_definitions, user_preferences) without implementation details. DAOs will use stored procedures exclusively
- [x] **Dependency Injection**: Service dependencies identified (`IService_ErrorHandler`, `ILoggingService`, future `IService_Dunnage`, `IService_UserPreferences`) - will be registered in App.xaml.cs
- [x] **Error Handling**: Error scenarios documented (database connection loss, icon library failure, duplicate names, validation errors) with specific user-facing messages and `IService_ErrorHandler` usage
- [x] **Security & Authentication**: Not applicable for this feature (internal tool, no authentication/authorization requirements)
- [x] **WinUI 3 Practices**: UI/UX requirements use WinUI 3 components (ContentDialog, ItemsRepeater, GridView, InfoBar), theme resources (AccentButtonStyle, CardBackgroundFillColorDefaultBrush), and Fluent Design principles (corner radius, spacing, typography)
- [x] **Specification-Driven**: This spec is technology-agnostic (no XAML markup in requirements section) and user-focused (3 detailed personas with workflows, pain points, environments)

### Special Constraints

- [x] **Infor Visual Integration**: Not applicable - this feature only interacts with MySQL mtm_receiving_application database (full CRUD allowed)
- [x] **MySQL 5.7.24 Compatibility**: Validation rules stored as JSON string in TEXT column (not JSON column type which requires MySQL 5.7.8+). No use of CTEs, window functions, or CHECK constraints
- [x] **Async Operations**: All database operations (duplicate check, insert type, insert custom fields, update user preferences) will be async with proper `IsBusy` flag and timeout handling

### Notes

**Dialog vs Window Pattern**: This feature uses `ContentDialog` (modal overlay) rather than `Window` because:
- User must complete or cancel the action before returning to main workflow
- No need for independent window controls (minimize, maximize, resize to arbitrary dimensions)
- Simpler focus management and keyboard navigation
- Standard WinUI 3 pattern for form-based data entry

**Window Sizing Exception**: Per `.github/instructions/window-sizing.instructions.md`, dialogs use `MaxHeight="750"` instead of fixed window size. The constitution's window sizing standards apply to `Window`-based classes, not `ContentDialog`.

**Validation Approach**: Real-time validation (300ms debounce) balances UX responsiveness with performance. Validation logic resides in ViewModel using CommunityToolkit.Mvvm's `ObservableValidator` or manual validation methods that update error properties bound to XAML.

**Field Reordering Implementation**: Drag-and-drop will use `CanDragItems="True"` and `CanReorderItems="True"` on `ItemsRepeater` with `DragItemsCompleted` event handler updating `DisplayOrder` property in ViewModel collection. No direct database update until "Add Type" is clicked.

**Icon Persistence**: Recently used icons stored per-user in `user_preferences` table with key `icon_usage_history` and JSON value `[{"glyph":"&#xE7B8;","count":15,"last_used":"2025-12-20T14:32:00Z"},...]`. Sorted by count descending, top 6 displayed.

**Sanitization Safety**: Field names like "Weight (lbs)" sanitized to "weight_lbs" for database columns. Collision detection (two fields sanitizing to same name) handled with append suffix "_2", "_3", etc. Display names preserved in `field_name` column, sanitized names in `database_column_name` column.
