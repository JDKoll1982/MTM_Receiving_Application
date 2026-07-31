# Shared Part Number Validation Workflow Refactor Prompt

Use this prompt to implement a shared, reusable part-number validation workflow in Module_Shared and migrate all Infor Visual part-number textboxes to use it.

## Goal

Standardize every part-number textbox that validates against Infor Visual so the logic is implemented once and reused everywhere.

## Required Architecture

- Preserve MVVM boundaries: View -> ViewModel -> Service -> DAO -> Database.
- Do not call DAO classes directly from ViewModels.
- Place shared validation workflow code in Module_Shared.
- Place the custom part-number textbox user control in Module_Shared.
- Avoid per-screen duplicated validation pipelines.

## Required Shared Workflow

Implement a shared workflow service in Module_Shared that executes this exact sequence for every part-number input:

1. Run parameter validation on the raw entered value.
2. Check whether a formatting rule exists for the entered value.
3. If a rule exists and formatting is required, apply formatting.
4. Check Infor Visual for an exact match on the formatted value.
5. If exact match exists, keep the formatted value.
6. If no exact match exists, run fuzzy search.
7. Return the best valid fuzzy result using existing business rules.
8. Return structured result metadata for UI and ViewModel usage.

The shared workflow must be implemented as a generic typed lookup pipeline that supports multiple Infor Visual lookup domains.
It must be configured with Part Number and Location support by default.

## Required Shared User Control

Create a custom part-number textbox user control in Module_Shared and migrate all relevant textboxes to use it.

Control responsibilities:

- Accept raw user input.
- Trigger shared workflow validation.
- Display validation state and messaging consistently.
- Surface final resolved part number.
- Expose bindable properties and events for ViewModels.
- Avoid business-rule logic in code-behind.

The control and backing service must support lookup of any Infor Visual data type, not only part numbers.
It must ship with these defaults enabled:

- Part Number
- Location

The control must support a configurable lookup type input (for example: PartNumber, Location, and future types).

## Required Generic Lookup Design

Implement a reusable typed lookup architecture in Module_Shared so future data types can be added without refactoring the control internals.

Minimum requirements:

- A shared lookup type key (enum or equivalent strongly typed identifier).
- A lookup strategy/provider contract per type.
- Type-specific formatting-rule hook.
- Type-specific exact-match query hook.
- Type-specific fuzzy-search hook.
- Unified result contract consumed by the control.
- DI registration pattern for adding new lookup types.

## Instructions For Adding New Lookup Types

Include implementation instructions in the delivery for extending the control/service to additional Infor Visual lookup types.

Required extension steps:

1. Add a new lookup type key.
2. Implement and register a type provider/strategy for:
    - parameter validation
    - optional formatting
    - exact-match lookup
    - fuzzy fallback lookup
3. Add or map UI defaults (placeholder, header text, and optional formatting hints).
4. Wire the new type into the control configuration/binding surface.
5. Add tests for valid, invalid, exact-match, and fuzzy-fallback paths for the new type.

Expected outputs from the control:

- Raw input
- Formatted value
- Exact-match status
- Fuzzy-fallback status
- Final resolved value
- Validation errors and warnings

## Refactor Scope

- Identify every textbox in the app that validates part numbers against Infor Visual.
- Inventory current behavior for each textbox.
- Replace duplicated validation paths with the shared Module_Shared workflow.
- Replace plain textbox usage with the shared custom user control where applicable.
- Keep behaviorally unrelated fields out of scope.

## MCP Server Requirements

Use all three MCP servers during discovery, design validation, and implementation.

### 1. Serena MCP

- Use symbol discovery and usage tracing to identify all impacted textboxes and code paths.
- Use symbol-aware refactor targeting to minimize risk.
- Validate impacted call sites after changes.

### 2. Context7 MCP

- Retrieve third-party library documentation relevant to validation and fuzzy matching.
- Confirm API usage patterns for parsing, normalization, and matching helpers.

### 3. Microsoft Learn MCP

- Retrieve official Microsoft guidance for WinUI custom controls, dependency properties, bindings, and validation UX.
- Confirm MVVM-compatible design choices for reusable controls in WinUI 3.

If any MCP server is unavailable during execution, explicitly record that gap and continue with available tools.

## Deliverables

1. Affected textbox inventory:
    - file path
    - owning view and viewmodel
    - current validation path summary
2. Shared implementation summary:
    - new Module_Shared workflow components
    - new Module_Shared custom control components
    - new or updated interfaces/contracts
3. Migration summary:
    - each textbox migrated to shared workflow and control
    - deferred edge cases with rationale
4. Validation proof:
    - parameter-check case
    - format-rule-applied case
    - exact-match case
    - fuzzy-fallback case
    - invalid or unresolved case
5. Regression safeguards:
    - tests added or updated
    - verification that every migrated textbox follows the required sequence

## Constraints

- Do not change unrelated business logic.
- Do not introduce direct database calls from views or viewmodels.
- Keep the refactor minimal, focused, and reusable.
- Prefer extending existing abstractions over broad rewrites when compatible.

## Completion Criteria

The task is complete only when all part-number textboxes that validate against Infor Visual use the shared Module_Shared workflow and the shared Module_Shared custom user control, with verified consistent behavior.

The task is also complete only when the shared control/service can handle typed lookup domains generically, is configured with Part Number and Location by default, and includes extension instructions for additional Infor Visual lookup types.

## Serena Discovery Inventory (2026-07-31)

Use this inventory as the initial migration checklist. Confirm final inclusion by checking each control's actual validation path in ViewModel and Service layers.

### Part-Number Textboxes (Primary Conversion Targets)

- Module_Receiving/Views/View_Receiving_POEntry.xaml: `PartIDTextBox`
- Module_Receiving/Views/View_Receiving_ManualEntry.xaml: Part ID editable textbox in grid (`LostFocus="PartIDTextBox_LostFocus"`)
- Module_Scanner/Views/View_Scanner_ManageItemsDialog.xaml: Part textbox (`Header="Part"`, `Text="{x:Bind PayloadPartId, Mode=TwoWay}"`)
- Module_Scanner/Views/View_Scanner_Workbench.xaml: Part entry textbox (`Header="Part"`, `LostFocus="PartTextBox_LostFocus"`)
- Module_Dunnage/Views/View_Dunnage_EditPartDialog.xaml: `PartIdTextBox`
- Module_Dunnage/Views/View_Dunnage_QuickAddPartDialog.xaml: `PartIdTextBox`
- Module_Settings.Volvo/Views/View_Settings_Volvo_PartAddEditDialog.xaml: `PartNumberTextBox`

### Location Textboxes (Include Even If Not InforVisual-Validated)

- Module_Dunnage/Views/View_Dunnage_DetailsEntryView.xaml: `LocationTextBox`
- Module_Receiving/Views/View_Receiving_LoadEntry.xaml: Location entry textbox (`Header` bound via `LoadEntryLocationHeaderText`, `LostFocus="LocationTextBox_LostFocus"`)
- Module_Receiving/Views/View_Receiving_ManualEntry.xaml: Location editable textbox in grid (`LostFocus="LocationTextBox_LostFocus"`)
- Module_Scanner/Views/View_Scanner_ManageItemsDialog.xaml: From and To location textboxes (`LostFocus="LocationTextBox_LostFocus"` on both)
- Module_Scanner/Views/View_Scanner_Workbench.xaml: From location textbox (`LostFocus="FromLocationTextBox_LostFocus"`)
- Module_Scanner/Views/View_Scanner_Workbench.xaml: To location textbox (`LostFocus="ToLocationTextBox_LostFocus"`)
- Module_Settings.Dunnage/Views/View_Settings_Dunnage_PersonalDefaults.xaml: `DefaultLocationTextBox`
- Module_Settings.Dunnage/Views/View_Settings_Dunnage_UserPreferences.xaml: `DefaultLocationTextBox`
- Module_Settings.Receiving/Views/View_Settings_Receiving_Defaults.xaml: Default location textbox (`Header="Default Location"`)
- Module_Settings.Receiving/Views/View_Settings_Receiving_EntryDefaults.xaml: Default location textbox (`Header="Default Location"`)
- Module_Settings.Receiving/Views/View_Settings_Receiving_Reconciliation.xaml: `IgnoredLocationTextBox`
- Module_Settings.Receiving/Views/View_Settings_Receiving_UserPreferences.xaml: `IgnoredLocationTextBox`
- Module_Volvo/Views/VolvoShipmentEditDialog.xaml: `AddPartLocationBox` (`Header="Location"`)

### Candidate Controls Requiring Triage (Name Contains "Part" But May Not Be Part-Number Entry)

- Module_Dunnage/Views/View_Dunnage_EditPartDialog.xaml: `PartSpecificSpecNameTextBox`, `PartSpecificUnitTextBox`, `PartSpecificChoiceTextBox`
- Module_Dunnage/Views/View_Dunnage_QuickAddPartDialog.xaml: `PartSpecificSpecNameTextBox`, `PartSpecificUnitTextBox`, `PartSpecificChoiceTextBox`

### Non-TextBox Part or Location Entry Controls (Also Include In Migration Plan)

- Module_Dunnage/Views/View_Dunnage_PartSelectionView.xaml: `PartNumberComboBox` (part selection entry point)
- Module_Dunnage/Views/View_Dunnage_EditModeView.xaml: Part picker button in Part ID column (`Click="PartIdButton_Click"`)
- Module_Dunnage/Views/View_Dunnage_EditModeView.xaml: Location picker button in Location column (`Click="LocationButton_Click"`)

### Verification Note

This list was generated from Serena-led pattern discovery in XAML. Before migration, validate each candidate against its actual code path so only true part-number and location entry controls are converted.