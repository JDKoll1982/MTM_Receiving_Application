# Comprehensive UI/UX Redesign:  Add New Dunnage Type Dialog

**Target Repository**:  `JDKoll1982/MTM_Receiving_Application`  
**Feature**:  Dunnage Management (Feature 008)  
**Target File**: `Views/Dunnage/Dunnage_AddNewTypeDialog.xaml` and associated ViewModel  
**Priority**: High - User Experience Enhancement

---

## 📸 Current State Analysis

**Current UI Issues** (from screenshot):
1. ❌ Vertical scrollbar present - breaks flow and reduces efficiency
2. ❌ Inconsistent button styling (rectangular "Add Type"/"Cancel" vs rounded "Click to select icon")
3. ❌ "Add Field" button lacks visual weight and discoverability
4. ❌ No visual preview of custom fields as they're added
5. ❌ Icon selection is button-based rather than visual picker
6. ❌ Poor spacing hierarchy - all sections have equal visual weight
7. ❌ No inline validation feedback
8. ❌ Static field type dropdown instead of modern segmented control
9. ❌ No drag-and-drop reordering for custom fields
10. ❌ Dialog lacks contextual help/tooltips

---

## 🎯 User Personas

### Primary:  **Maria Torres - Receiving Dock Supervisor**
**Demographics**:
- Age: 42, 8 years at MTM Manufacturing
- Role: Manages 5-person receiving team
- Shift: 6:00 AM - 2:30 PM (Day shift)

**Technical Profile**:
- Computer proficiency: Moderate
- Uses Windows desktop apps daily (Excel, email, ERP terminal)
- Comfortable with mouse but prefers keyboard shortcuts when available
- Often wears work gloves - needs larger click targets

**Goals**:
- ✅ Configure new dunnage types when vendors change packaging (2-3 times/month)
- ✅ Minimize data entry errors to reduce rework
- ✅ Train new employees quickly (30-minute onboarding target)
- ✅ Maintain consistent dunnage naming across receiving team

**Pain Points**:
- 😣 Current UI requires scrolling - loses context of what was entered above
- 😣 Inconsistent button styles cause hesitation about which action to take
- 😣 No visual confirmation when adding custom fields
- 😣 Can't reorder fields after creation - must delete and re-add
- 😣 Forgets which icon was selected after scrolling to specifications section

**Environment**:
- Workstation: Dell OptiPlex 7070, 24" 1920x1080 monitor
- Location: Receiving office adjacent to dock floor
- Interruptions: High (phones, dock questions, deliveries)
- Multi-tasking: Frequently switches between applications

**Typical Workflow**:
1. Vendor sends packing change notification email
2. Receives first shipment with new dunnage
3. Opens MTM Receiving app while standing next to delivery
4. Quickly creates new type (wants <60 seconds)
5. Returns to receiving workflow immediately

---

### Secondary: **James Chen - Quality Control Manager**
**Demographics**:
- Age: 35, 3 years at MTM
- Role: Oversees quality processes, manages data integrity
- Shift: 8:00 AM - 5:00 PM (salaried)

**Technical Profile**: 
- Computer proficiency: High
- Manages database configurations and reports
- Comfortable with SQL, Excel advanced functions
- Advocates for data standardization

**Goals**:
- ✅ Ensure data consistency across 15+ dunnage types
- ✅ Create comprehensive specifications with validation rules
- ✅ Audit existing dunnage types for compliance
- ✅ Implement bulk import for new product line launches

**Pain Points**:
- 😣 No way to preview existing similar types for comparison
- 😣 Can't set validation rules (min/max, required patterns)
- 😣 No templates for common dunnage categories
- 😣 Manual entry error-prone for bulk configuration

**Environment**:
- Workstation: Dell Precision 5560, dual 27" monitors (2560x1440)
- Location: Quality office
- Interruptions: Low
- Multi-tasking: Database, Excel, ERP reports

**Typical Workflow**: 
1. New product line requires 8 dunnage types
2. Creates Excel template with all specifications
3. Manually enters each type (current:  5 min each = 40 min total)
4. Wants to copy-paste or import from CSV
5. Validates entries against shipping requirements

---

### Tertiary: **David Wong - New Receiving Clerk**
**Demographics**: 
- Age: 24, 2 months at MTM
- Role: Entry-level receiving clerk
- Shift: 10:00 AM - 6:30 PM (Evening shift)

**Technical Profile**: 
- Computer proficiency: Moderate (smartphone-native generation)
- Expects UI patterns similar to mobile apps
- Learns quickly through visual cues and tooltips
- Prefers guided workflows over free-form entry

**Goals**: 
- ✅ Learn dunnage types and specifications quickly
- ✅ Avoid creating duplicate types
- ✅ Get immediate feedback when making mistakes
- ✅ Understand what each field means without asking supervisor

**Pain Points**:
- 😣 No tooltips explaining field purposes
- 😣 Unclear which fields are required vs optional
- 😣 Validation errors only appear after clicking submit
- 😣 No examples or placeholders showing expected format

**Environment**:
- Workstation:  Shared terminal (Dell OptiPlex), 22" monitor
- Location: Receiving dock floor
- Interruptions: Constant (forklift traffic, phone calls)
- Multi-tasking: Frequently switches to Infor Visual ERP

**Typical Workflow**:
1. Supervisor asks to create new dunnage type
2. Has written specification sheet from vendor
3. Enters data field-by-field while referencing paper
4. Wants real-time validation to catch typos immediately
5. Returns to main receiving tasks

---

## 📋 User Stories

### 🔴 Must-Have (MVP - Current Sprint)

#### **US-001: Eliminate Scrolling for Standard Workflows**
**As Maria**, I want all form elements visible without scrolling at 1920x1080 resolution  
**So that** I can complete entries quickly without losing context or scrolling back to verify earlier inputs  
**Acceptance Criteria**:
- [ ] Dialog height ≤ 750px at default size
- [ ] All sections visible on 1920x1080 (1080px - 200px taskbar/titlebar = 880px available)
- [ ] No vertical scrollbar for ≤5 custom fields (90% use case)
- [ ] Scrollbar only appears if >5 fields OR window resized below 768px height
- [ ] Tab order flows top-to-bottom without jumping between sections

**Technical Notes**:
- Use `MaxHeight="750"` on root Grid
- `ScrollViewer` wraps custom fields section only (not entire dialog)
- Test on 1366x768 laptop (minimum supported resolution)

---

#### **US-002: Consistent Button Styling with Clear Hierarchy**
**As Maria**, I want all buttons to have the same visual style with rounded corners  
**So that** I can quickly identify actionable elements without confusion about button types  
**Acceptance Criteria**:
- [ ] All buttons use `CornerRadius="4"` (per constitution. md)
- [ ] Primary action ("Add Type"): `AccentButtonStyle`, 140px wide, right-aligned
- [ ] Secondary action ("Cancel"): `DefaultButtonStyle`, 120px wide, left of primary
- [ ] Tertiary actions ("Add Field", icon picker): Subtle style, consistent corner radius
- [ ] All buttons:  `MinHeight="40"`, `Padding="16,8"`, `FontSize="14"`
- [ ] Button text uses sentence case (not ALL CAPS or lowercase)

**Visual Hierarchy**:

Primary (Accent Blue) > Secondary (Neutral) > Tertiary (Ghost/Subtle)

Code

**Code Pattern**:
```xml
<!-- Primary Button -->
<Button Content="Add Type" 
        Style="{StaticResource AccentButtonStyle}"
        MinWidth="140" Height="40" CornerRadius="4"
        HorizontalAlignment="Right"/>

<!-- Secondary Button -->
<Button Content="Cancel" 
        Style="{StaticResource DefaultButtonStyle}"
        MinWidth="120" Height="40" CornerRadius="4"
        HorizontalAlignment="Right"/>
US-003: Custom Field Preview with Edit/Delete/Reorder
As James, I want to see a live preview of custom fields I've added
So that I can verify the form structure matches my specifications before saving
Acceptance Criteria:

 Custom fields appear in styled list immediately after clicking "Add Field"
 Each field shows: Icon (based on type), Name, Type, Required status
 Hover reveals: Edit button, Delete button, Drag handle
 Drag-and-drop reordering updates tab order automatically
 Empty state shows helpful message ("No custom fields yet. Click + Add Field to begin.")
 Maximum 25 fields (warning at 10, hard limit at 25)
UI Pattern (from CommunityToolkit. WinUI. UI. Controls):

XML
<ItemsRepeater ItemsSource="{x:Bind ViewModel.CustomFields, Mode=OneWay}">
    <ItemsRepeater.ItemTemplate>
        <DataTemplate x:DataType="local:Model_CustomFieldDefinition">
            <Grid Background="{ThemeResource LayerFillColorDefaultBrush}"
                  CornerRadius="4" Padding="12" Margin="0,0,0,8"
                  BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}" BorderThickness="1">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto"/> <!-- Drag handle -->
                    <ColumnDefinition Width="Auto"/> <!-- Icon -->
                    <ColumnDefinition Width="*"/>    <!-- Name/Type -->
                    <ColumnDefinition Width="Auto"/> <!-- Actions -->
                </Grid.ColumnDefinitions>
                
                <!-- Drag Handle (visible on hover) -->
                <FontIcon Grid.Column="0" Glyph="&#xE76F;" FontSize="16" Opacity="0.6"
                          ToolTipService. ToolTip="Drag to reorder"/>
                
                <!-- Field Type Icon -->
                <FontIcon Grid.Column="1" Margin="12,0"
                          Glyph="{x:Bind FieldTypeIcon, Mode=OneWay}"
                          Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                
                <!-- Field Details -->
                <StackPanel Grid.Column="2" Spacing="4">
                    <TextBlock Text="{x:Bind FieldName, Mode=OneWay}" FontWeight="SemiBold"/>
                    <TextBlock Style="{StaticResource CaptionTextBlockStyle}"
                               Foreground="{ThemeResource TextFillColorSecondaryBrush}">
                        <Run Text="{x:Bind FieldType, Mode=OneWay}"/>
                        <Run Text=" • "/>
                        <Run Text="{x:Bind RequiredText, Mode=OneWay}"/>
                    </TextBlock>
                </StackPanel>
                
                <!-- Action Buttons (visible on hover) -->
                <StackPanel Grid.Column="3" Orientation="Horizontal" Spacing="8">
                    <Button Width="32" Height="32" Padding="0"
                            Command="{x: Bind EditCommand}"
                            ToolTipService.ToolTip="Edit field">
                        <FontIcon Glyph="&#xE70F;" FontSize="14"/>
                    </Button>
                    <Button Width="32" Height="32" Padding="0"
                            Command="{x: Bind DeleteCommand}"
                            ToolTipService.ToolTip="Delete field">
                        <FontIcon Glyph="&#xE74D;" FontSize="14"/>
                    </Button>
                </StackPanel>
            </Grid>
        </DataTemplate>
    </ItemsRepeater. ItemTemplate>
</ItemsRepeater>
US-004: Real-Time Validation with Inline Feedback
As David, I want immediate feedback when I make an error
So that I can correct mistakes before submitting instead of getting blocked at the end
Acceptance Criteria:

 Type Name: Red border + error message if empty/whitespace only
 Type Name: Warning icon + message if matches existing type (not blocking)
 Field Name: Red border if empty, duplicate, or contains special chars (<>{}[])
 Field Name: Character counter (e.g., "42/100 characters")
 Validation updates on TextChanged with 300ms debounce (avoid jitter)
 Primary button disabled until all required fields valid
 Validation messages use CaptionTextBlockStyle with system error color
Validation Patterns:

XML
<!-- Type Name with Validation -->
<StackPanel Spacing="8">
    <TextBlock Text="Type Name *" Style="{StaticResource BodyStrongTextBlockStyle}"/>
    <TextBox Text="{x:Bind ViewModel.TypeName, Mode=TwoWay}"
             PlaceholderText="e.g., Pallet, Crate, Blocking"
             MaxLength="100"
             BorderBrush="{x:Bind ViewModel.TypeNameBorderBrush, Mode=OneWay}"/>
    
    <!-- Error Message (visible when invalid) -->
    <StackPanel Orientation="Horizontal" Spacing="8"
                Visibility="{x:Bind ViewModel.TypeNameError, Mode=OneWay, Converter={StaticResource EmptyStringToCollapsedConverter}}">
        <FontIcon Glyph="&#xE7BA;" FontSize="12" Foreground="{ThemeResource SystemFillColorCriticalBrush}"/>
        <TextBlock Text="{x:Bind ViewModel.TypeNameError, Mode=OneWay}"
                   Style="{StaticResource CaptionTextBlockStyle}"
                   Foreground="{ThemeResource SystemFillColorCriticalBrush}"/>
    </StackPanel>
    
    <!-- Character Counter -->
    <TextBlock Text="{x:Bind ViewModel.TypeNameCharCount, Mode=OneWay}"
               Style="{StaticResource CaptionTextBlockStyle}"
               Foreground="{ThemeResource TextFillColorSecondaryBrush}"
               HorizontalAlignment="Right"/>
</StackPanel>
US-005: Visual Icon Picker with Search and Categories
As Maria, I want to select icons from a visual grid instead of clicking a button
So that I can quickly find the right icon without memorizing names
Acceptance Criteria:

 Icon picker displays 6 columns × 3 rows = 18 icons per page
 Icons organized in tabs: "Containers", "Materials", "Warnings", "Tools", "All"
 Recently used icons section (top 6, persistent via IUserPreferencesService)
 Search box filters icons by keyword (e.g., "box" shows all box-related icons)
 Selected icon highlighted with accent border
 Default icon: Box (&#xE7B8;)
 Icon preview updates immediately on selection (no additional click)
UI Layout:

XML
<StackPanel Spacing="12">
    <TextBlock Text="Icon Selection" Style="{StaticResource BodyStrongTextBlockStyle}"/>
    
    <!-- Selected Icon Preview -->
    <Border Background="{ThemeResource LayerFillColorDefaultBrush}"
            Padding="16" CornerRadius="4" MaxWidth="300">
        <StackPanel Orientation="Horizontal" Spacing="16">
            <FontIcon Glyph="{x:Bind ViewModel. SelectedIconGlyph, Mode=OneWay}"
                      FontSize="32"
                      Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
            <StackPanel VerticalAlignment="Center">
                <TextBlock Text="Selected Icon" 
                           Style="{StaticResource CaptionTextBlockStyle}"
                           Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                <TextBlock Text="{x:Bind ViewModel.SelectedIconName, Mode=OneWay}"
                           FontWeight="SemiBold"/>
            </StackPanel>
        </StackPanel>
    </Border>
    
    <!-- Icon Grid with Tabs -->
    <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
            BorderThickness="1" CornerRadius="4" Padding="12">
        <StackPanel Spacing="12">
            <!-- Search Box -->
            <AutoSuggestBox PlaceholderText="Search icons..."
                            QueryIcon="Find"
                            Text="{x:Bind ViewModel.IconSearchQuery, Mode=TwoWay}"/>
            
            <!-- Category Tabs (SegmentedControl pattern) -->
            <StackPanel Orientation="Horizontal" Spacing="4">
                <RadioButton Content="All" GroupName="IconCategory" IsChecked="True"/>
                <RadioButton Content="Containers" GroupName="IconCategory"/>
                <RadioButton Content="Materials" GroupName="IconCategory"/>
                <RadioButton Content="Warnings" GroupName="IconCategory"/>
            </StackPanel>
            
            <!-- Icon Grid (6 columns) -->
            <GridView ItemsSource="{x:Bind ViewModel.FilteredIcons, Mode=OneWay}"
                      SelectedItem="{x:Bind ViewModel.SelectedIcon, Mode=TwoWay}"
                      SelectionMode="Single"
                      MaxWidth="360">
                <GridView.ItemTemplate>
                    <DataTemplate x:DataType="local:Model_IconDefinition">
                        <Border Width="52" Height="52" CornerRadius="4"
                                Background="{ThemeResource LayerFillColorDefaultBrush}"
                                BorderBrush="{ThemeResource AccentFillColorDefaultBrush}"
                                BorderThickness="{x:Bind IsSelectedThickness, Mode=OneWay}"
                                ToolTipService.ToolTip="{x:Bind IconName}">
                            <FontIcon Glyph="{x:Bind Glyph}" FontSize="24"/>
                        </Border>
                    </DataTemplate>
                </GridView.ItemTemplate>
            </GridView>
        </StackPanel>
    </Border>
</StackPanel>
🟡 Should-Have (Enhanced Features - Next Sprint)
US-006: Duplicate Existing Dunnage Type
As James, I want to duplicate an existing dunnage type as a starting point
So that I can quickly create variations without re-entering common fields
Acceptance Criteria:

 "Create from Existing" button on parent Dunnage_ManualEntryView page
 Opens dialog pre-populated with selected type's data
 Auto-appends " (Copy)" to type name
 Custom fields copied with same order and validation rules
 Icon copied from source type
 User can modify any field before saving
Navigation Flow:

Code
Dunnage_ManualEntryView 
  → Right-click type in grid → "Duplicate Type"
  → Opens Dunnage_AddNewTypeDialog with prepopulated data
  → User modifies name (required), optionally adjusts fields
  → Save creates new type with copied structure
US-007: Field Validation Rules Builder
As James, I want to set validation rules for custom fields without coding
So that data entry is more accurate and operators can't enter invalid values
Acceptance Criteria:

 Number fields: Min/max value constraints, decimal places (0-4)
 Text fields: Max length (default 100), regex pattern (optional)
 Date fields: Min/max date range
 Validation rules displayed as human-readable summary
 Visual rule builder (no regex syntax required for common patterns)
Example Rules:

Field: "Weight" (Number) → Min: 1, Max: 9999, Decimals: 2
Field: "Part Number" (Text) → Pattern: "Starts with MTM-", Max Length: 20
Field: "Manufacture Date" (Date) → Max: Today, Min: 90 days ago
US-008: Field Templates for Common Dunnage Types
As Maria, I want pre-configured field sets for common dunnage categories
So that I can create standard types faster without remembering which fields to add
Acceptance Criteria:

 Template dropdown appears before manual field entry
 Templates:
"Standard Container" → Material (Text), Weight (Number), Length/Width/Height (Numbers)
"Returnable Package" → Material, Weight, Deposit Amount, Return Location
"Protective Blocking" → Material, Dimensions, Part Number Protected
 One-click apply, then customize if needed
 Templates stored in appsettings.json, editable by admins
US-009: Keyboard Shortcuts for Power Users
As James, I want keyboard shortcuts for common actions
So that I can work efficiently without constantly switching between mouse and keyboard
Acceptance Criteria:

 Tab navigates through all inputs in logical order
 Enter on Type Name field focuses Icon Picker
 Ctrl+Enter submits form (same as clicking "Add Type")
 Esc closes dialog (same as clicking "Cancel")
 Ctrl+F focuses icon search box
 Alt+A clicks "Add Field" button
 All shortcuts documented in tooltip on each button
🟢 Could-Have (Future Enhancements - Backlog)
US-010: Bulk Import from CSV/Excel
As James, I want to import multiple dunnage types from a CSV file
So that I can configure 10+ types in minutes instead of hours
Acceptance Criteria:

 "Import from CSV" button on parent page
 CSV template downloadable (with example data and validation rules)
 Import preview shows what will be created + validation errors
 Batch validation before committing to database
 Rollback if any type fails validation
CSV Format:

CSV
TypeName,IconName,Field1Name,Field1Type,Field1Required,Field2Name,Field2Type,Field2Required
Pallet 48x40,Box,Material,Text,true,Weight,Number,true
Crate Large,Container,Length,Number,true,Width,Number,true
US-011: Recently Used Icons Highlighted
As Maria, I want frequently used icons highlighted at the top of the picker
So that I can maintain consistency across similar types without searching
Acceptance Criteria:

 Top 6 most-used icons shown in "Recently Used" section
 Usage tracked per user (stored in user_preferences table)
 Persists across sessions
 Clear button resets recently used list
US-012: Right-Side Live Preview Panel
As James, I want to see how the dunnage entry form will look as I add fields
So that I can optimize field order for operator efficiency
Acceptance Criteria:

 Right panel (30% width) shows live preview
 Updates in real-time as fields are added/reordered
 Preview matches actual Dunnage_DetailsEntryView layout
 Collapsible panel for small screens
🐛 Edge Cases & Error Scenarios
EC-001: Empty Type Name
Scenario: User clicks "Add Type" without entering a name
Expected Behavior:

 Primary button remains disabled (grayed out)
 Red border appears around Type Name textbox
 Error message: "Type name is required" appears below field
 Focus automatically returns to Type Name field
Code:

C#
// ViewModel validation
private bool CanAddType()
{
    return !string.IsNullOrWhiteSpace(TypeName) 
           && !HasValidationErrors;
}

[RelayCommand(CanExecute = nameof(CanAddType))]
private async Task AddTypeAsync()
{
    // Validation double-check
}

partial void OnTypeNameChanged(string value)
{
    ValidateTypeName();
    AddTypeCommand.NotifyCanExecuteChanged();
}
EC-002: Duplicate Type Name
Scenario: User enters "Pallet" but type already exists
Expected Behavior:

 Warning icon (yellow) appears next to Type Name field
 Message: "A dunnage type named 'Pallet' already exists. Consider using a different name or editing the existing type."
 Does NOT block submission (user might want variant like "Pallet-Large")
 Provide link: "View Existing Type" (opens existing type in edit mode)
SQL Check (via DAO):

SQL
-- sp_dunnage_types_check_duplicate
SELECT COUNT(*) FROM dunnage_types 
WHERE type_name = @TypeName AND is_deleted = 0;
EC-003: No Custom Fields Added
Scenario: User enters Type Name, selects icon, but doesn't add any specifications
Expected Behavior:

 Allow save (some types may not need custom fields)
 Confirmation dialog: "You haven't added any custom fields. This type will only track basic information (PO, location, quantity). Continue?"
 Checkbox: "Don't show this again"
EC-004: Exceeds Maximum Fields (25)
Scenario: User tries to add 26th custom field
Expected Behavior:

 "Add Field" button disabled after field #25
 InfoBar appears: "Maximum 25 custom fields per type. Please remove a field to add another."
 Warning appears after field #10: "You have 10 custom fields. Consider if all are necessary (max 25)."
EC-005: Special Characters in Field Name
Scenario: User enters Weight (lbs) or Heat # as field name
Expected Behavior:

 Allow parentheses, dashes, spaces (common in manufacturing)
 Block only: < > { } [ ] | \ / \n \t
 Error message: "Field name cannot contain special characters: <>{}[]|/n/t"
 Sanitize for database: Store display name as-is, generate safe column name (weight_lbs, heat_num)
Sanitization Logic:

C#
private string SanitizeFieldName(string displayName)
{
    // Display:  "Weight (lbs)" → Database Column: "weight_lbs"
    return Regex.Replace(displayName, @"[^a-zA-Z0-9_]", "_")
                . ToLower()
                . Trim('_');
}
EC-006: Deleting Last Custom Field
Scenario: User deletes all custom fields after adding several
Expected Behavior:

 No error (valid state per EC-003)
 Empty state appears: "No custom fields added yet. Click + Add Field to define specifications for this type."
 Gentle icon (not error icon)
EC-007: Icon Library Fails to Load
Scenario: Icon font file missing or corrupted
Expected Behavior:

 Fallback to placeholder icons (Unicode symbols)
 Error logged to ApplicationErrorLog. csv
 User sees: InfoBar "Icon library unavailable. Default icons will be used."
 Dialog remains functional with text-based icon names
EC-008: Accidental Cancel with Unsaved Changes
Scenario: User has entered Type Name + 3 custom fields, clicks Cancel
Expected Behavior:

 Confirmation dialog: "You have unsaved changes. Are you sure you want to cancel?"
 Buttons: "Discard Changes" (red), "Continue Editing" (primary)
 If no changes made: Close immediately without confirmation
Change Detection:

C#
private bool HasUnsavedChanges()
{
    return ! string.IsNullOrWhiteSpace(TypeName) 
           || SelectedIconGlyph != DefaultIconGlyph 
           || CustomFields.Count > 0;
}
EC-009: Window Resize Below Minimum Width
Scenario: User resizes window to <500px width
Expected Behavior:

 Dialog enforces MinWidth="500", cannot be resized smaller
 Icon grid collapses to 4 columns instead of 6
 Field preview list remains readable (horizontal scroll if needed)
EC-010: Lost Database Connection During Save
Scenario: User clicks "Add Type" but MySQL connection dropped
Expected Behavior:

 Loading indicator appears (spinner on button)
 Timeout after 10 seconds
 Error dialog: "Unable to save. Please check your network connection and try again."
 Data retained in dialog (user doesn't lose work)
 Retry button available
Error Handling Pattern:

C#
[RelayCommand]
private async Task AddTypeAsync()
{
    if (IsBusy) return;

    try
    {
        IsBusy = true;
        StatusMessage = "Saving dunnage type...";

        var result = await _dunnageService. CreateTypeAsync(
            TypeName, 
            SelectedIconGlyph, 
            CustomFields
        );

        if (result. IsSuccess)
        {
            _errorHandler.ShowUserSuccess(
                $"Dunnage type '{TypeName}' created successfully.",
                "Success"
            );
            DialogResult = ContentDialogResult.Primary;
        }
        else
        {
            _errorHandler. ShowUserError(
                result.ErrorMessage,
                "Save Failed",
                nameof(AddTypeAsync)
            );
        }
    }
    catch (Exception ex)
    {
        _errorHandler.HandleException(
            ex,
            Enum_ErrorSeverity.High,
            nameof(AddTypeAsync),
            nameof(Dunnage_AddNewTypeDialogViewModel)
        );
    }
    finally
    {
        IsBusy = false;
    }
}
🎨 UI/UX Design Mockup
Dialog Layout (600px width × 750px height)
Code
┌─────────────────────────────────────────────────────────────────────────────┐
│  Add New Dunnage Type                                              [✕]     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ╔═══════════════════════════════════════════════════════════════════════╗ │
│  ║  Basic Information                                                    ║ │
│  ╠═══════════════════════════════════════════════════════════════════════╣ │
│  ║  Type Name *                                                          ║ │
│  ║  ┌─────────────────────────────────────────────────────────────────┐ ║ │
│  ║  │ e.g., Pallet, Crate, Blocking                                   │ ║ │
│  ║  └─────────────────────────────────────────────────────────────────┘ ║ │
│  ║  ✓ Valid type name                                        42/100 chars ║ │
│  ╚═══════════════════════════════════════════════════════════════════════╝ │
│                                                                             │
│  ╔═══════════════════════════════════════════════════════════════════════╗ │
│  ║  Icon Selection                                                       ║ │
│  ╠═══════════════════════════════════════════════════════════════════════╣ │
│  ║  ┌────────────────────────────────────────────┐                      ║ │
│  ║  │  📦  Selected:  Box                          │                      ║ │
│  ║  └────────────────────────────────────────────┘                      ║ │
│  ║                                                                       ║ │
│  ║  ┌────────────────────────────────────────────┐                      ║ │
│  ║  │ 🔍 Search icons...                          │                      ║ │
│  ║  └────────────────────────────────────────────┘                      ║ │
│  ║                                                                       ║ │
│  ║  [ All ]  [ Containers ]  [ Materials ]  [ Tools ]                   ║ │
│  ║                                                                       ║ │
│  ║  ┌────┬────┬────┬────┬────┬────┐                                     ║ │
│  ║  │ 📦 │ 🪵 │ 📋 │ 🎯 │ ⚠️ │ 🔧 │  ← Recently Used                    ║ │
│  ║  ├────┼────┼────┼────┼────┼────┤                                     ║ │
│  ║  │ 🏭 │ 📊 │ 🔒 │ 🌲 │ ♻️ │ 🎨 │                                     ║ │
│  ║  ├────┼────┼────┼────┼────┼────┤                                     ║ │
│  ║  │ 📐 │ 🧱 │ 🧰 │ 🪜 │ 🛢️ │ 🪛 │                                     ║ │
│  ║  └────┴────┴────┴────┴────┴────┘                                     ║ │
│  ║  [View More Icons...]                                                ║ │
│  ╚═══════════════════════════════════════════════════════════════════════╝ │
│                                                                             │
│  ╔═══════════════════════════════════════════════════════════════════════╗ │
│  ║  Custom Specifications (Optional)                                    ║ │
│  ╠═══════════════════════════════════════════════════════════════════════╣ │
│  ║  ┌─────────────────────────────────────────────────────────────────┐ ║ │
│  ║  │  ⠿ 📝 Material               Text        Required   [✎] [🗑]    │ ║ │
│  ║  │  ⠿ 🔢 Weight                 Number      Required   [✎] [🗑]    │ ║ │
│  ║  │  ⠿ ☑️ Returnable             Boolean     Optional   [✎] [🗑]    │ ║ │
│  ║  │                                                                  │ ║ │
│  ║  │  [+ Add Custom Field]                                            │ ║ │
│  ║  └─────────────────────────────────────────────────────────────────┘ ║ │
│  ║                                                                       ║ │
│  ║  ┌─────────────────────────────────────────────────────────────────┐ ║ │
│  ║  │ ➕ New Field                                                     │ ║ │
│  ║  │ ┌────────────────────────┐  ┌──┬──┬──┬──┐  ☐ Required          │ ║ │
│  ║  │ │ Field Name             │  │📝│🔢│📅│☑│                        │ ║ │
│  ║  │ └────────────────────────┘  └──┴──┴──┴──┘                        │ ║ │
│  ║  │                          Text Number Date Bool                   │ ║ │
│  ║  │                                           [Add Field ⤴]          │ ║ │
│  ║  └─────────────────────────────────────────────────────────────────┘ ║ │
│  ╚═══════════════════════════════════════════════════════════════════════╝ │
│                                                                             │
│  ⓘ Tip: Use templates for common types (Containers, Returnable Packages)   │
│                                                                             │
│                                     ┌────────────┐  ┌──────────────┐       │
│                                     │   Cancel   │  │   Add Type   │       │
│                                     └────────────┘  └──────────────┘       │
└─────────────────────────────────────────────────────────────────────────────┘
    600px width × 750px height (no scrolling for ≤5 fields)
Visual Hierarchy Breakdown
Spacing System (per . editorconfig and constitution):
Section Margins: 24px vertical between major sections
Field Spacing: 12px vertical within sections
Input Padding: 12px horizontal, 8px vertical
Card Padding: 16px all sides
Grid Gaps: 12px column, 8px row
Typography (WinUI 3 Theme Resources):
Dialog Title: TitleTextBlockStyle (28px, SemiBold)
Section Headers: SubtitleTextBlockStyle (20px, SemiBold)
Field Labels: BodyStrongTextBlockStyle (14px, SemiBold)
Help Text: CaptionTextBlockStyle (12px, Regular)
Input Text: BodyTextBlockStyle (14px, Regular)
Color Palette (Theme-Aware):
Primary Action: AccentFillColorDefaultBrush (#0078D4 Light, #60CDFF Dark)
Borders: CardStrokeColorDefaultBrush (Neutral-200 Light, Neutral-700 Dark)
Backgrounds:
Dialog: ApplicationPageBackgroundThemeBrush
Cards: CardBackgroundFillColorDefaultBrush
Inputs: LayerFillColorDefaultBrush
Text:
Primary: TextFillColorPrimaryBrush
Secondary: TextFillColorSecondaryBrush (60% opacity)
Error: SystemFillColorCriticalBrush (#E81123)
Success: SystemFillColorSuccessBrush (#107C10)
Corner Radius (WinUI 3 Fluent Design):
Buttons: 4px (per constitution)
Input Fields: 4px
Cards/Borders: 8px
Dialog: 8px (system-defined)