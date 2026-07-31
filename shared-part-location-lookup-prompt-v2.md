# Shared Part And Location Lookup Refactor Prompt (V2)

Use this prompt to implement a reusable, typed Infor Visual lookup framework and migrate part-number and location entry controls to that shared framework.

## Objective

You MUST standardize part and location lookup behavior across the MTM Receiving Application so validation and lookup logic is implemented once and reused everywhere.

## Core Directives

- You MUST preserve MVVM flow: View -> ViewModel -> Service -> DAO -> Database.
- You MUST place shared control and shared lookup workflow components in Module_Shared.
- You MUST keep ViewModels free of direct DAO calls.
- You MUST keep business logic out of XAML code-behind.
- You MUST keep Infor Visual access read-only.
- You MUST migrate both TextBox and non-TextBox entry controls listed in this prompt.
- CRITICAL: You MUST implement typed lookup support for any Infor Visual data type, with Part Number and Location enabled by default.

## Required Shared Architecture

### Shared Lookup Service

You MUST create a generic typed lookup service in Module_Shared.

The service MUST support this sequence for each lookup type:

1. Parameter validation.
2. Optional type-specific formatting.
3. Exact-match lookup in Infor Visual.
4. Fuzzy lookup fallback when exact match is not found.
5. Normalized result return with status metadata.

### Shared User Control

You MUST create one reusable input control in Module_Shared that can run lookups by configured type.

The control MUST:

- Accept raw user input.
- Accept a configured lookup type key.
- Execute the shared lookup service.
- Show validation and resolution status consistently.
- Return resolved value and metadata to the ViewModel.

The control MUST be preconfigured with these default types:

- Part Number
- Location

## Typed Lookup Extensibility Requirements

You MUST implement a typed lookup contract so future types can be added without control refactoring.

Minimum design requirements:

- Shared lookup type identifier (enum or strongly typed equivalent).
- Per-type strategy/provider interface.
- Per-type formatting rule implementation hook.
- Per-type exact-match query hook.
- Per-type fuzzy-search hook.
- Unified result model used by the control and ViewModel.
- DI registration pattern for adding new lookup strategies.

## Instructions For Adding New Lookup Types

You MUST include and follow this extension process:

1. Add a new lookup type identifier.
2. Implement a new type strategy/provider.
3. Add parameter validation rules for the new type.
4. Add optional formatting logic for the new type.
5. Add exact-match lookup logic for the new type.
6. Add fuzzy-fallback lookup logic for the new type.
7. Register the strategy/provider in DI.
8. Map default UI text for the type (header, placeholder, help text).
9. Add tests for valid, invalid, exact-match, and fuzzy-fallback scenarios.

## Required Validation Behavior

### Part Number

You MUST enforce this order:

1. Validate raw input.
2. Apply formatting rule when configured.
3. Try exact Infor Visual match.
4. Use fuzzy match only when exact fails.

### Location

You MUST enforce the same sequence unless explicit module rules override formatting behavior.

## Migration Scope

You MUST migrate the controls listed below to use the shared typed lookup framework.

### Part Entry Controls (TextBox)

- Module_Receiving/Views/View_Receiving_POEntry.xaml: PartIDTextBox
- Module_Receiving/Views/View_Receiving_ManualEntry.xaml: PartID editing textbox (LostFocus="PartIDTextBox_LostFocus")
- Module_Scanner/Views/View_Scanner_ManageItemsDialog.xaml: Part textbox (Header="Part")
- Module_Scanner/Views/View_Scanner_Workbench.xaml: Part textbox (Header="Part", LostFocus="PartTextBox_LostFocus")
- Module_Dunnage/Views/View_Dunnage_EditPartDialog.xaml: PartIdTextBox
- Module_Dunnage/Views/View_Dunnage_QuickAddPartDialog.xaml: PartIdTextBox
- Module_Settings.Volvo/Views/View_Settings_Volvo_PartAddEditDialog.xaml: PartNumberTextBox

### Location Entry Controls (TextBox)

- Module_Dunnage/Views/View_Dunnage_DetailsEntryView.xaml: LocationTextBox
- Module_Receiving/Views/View_Receiving_LoadEntry.xaml: Location textbox (LostFocus="LocationTextBox_LostFocus")
- Module_Receiving/Views/View_Receiving_ManualEntry.xaml: InitialLocation editing textbox (LostFocus="LocationTextBox_LostFocus")
- Module_Scanner/Views/View_Scanner_ManageItemsDialog.xaml: From/To location textboxes (LostFocus="LocationTextBox_LostFocus")
- Module_Scanner/Views/View_Scanner_Workbench.xaml: From location textbox (LostFocus="FromLocationTextBox_LostFocus")
- Module_Scanner/Views/View_Scanner_Workbench.xaml: To location textbox (LostFocus="ToLocationTextBox_LostFocus")
- Module_Settings.Dunnage/Views/View_Settings_Dunnage_PersonalDefaults.xaml: DefaultLocationTextBox
- Module_Settings.Dunnage/Views/View_Settings_Dunnage_UserPreferences.xaml: DefaultLocationTextBox
- Module_Settings.Receiving/Views/View_Settings_Receiving_Defaults.xaml: Default Location textbox
- Module_Settings.Receiving/Views/View_Settings_Receiving_EntryDefaults.xaml: Default Location textbox
- Module_Settings.Receiving/Views/View_Settings_Receiving_Reconciliation.xaml: IgnoredLocationTextBox
- Module_Settings.Receiving/Views/View_Settings_Receiving_UserPreferences.xaml: IgnoredLocationTextBox
- Module_Volvo/Views/VolvoShipmentEditDialog.xaml: AddPartLocationBox

### Non-TextBox Entry Controls (Migrate Or Bridge To Shared Workflow)

- Module_Dunnage/Views/View_Dunnage_PartSelectionView.xaml: PartNumberComboBox
- Module_Dunnage/Views/View_Dunnage_EditModeView.xaml: Part selection button flow (PartIdButton_Click)
- Module_Dunnage/Views/View_Dunnage_EditModeView.xaml: Location selection button flow (LocationButton_Click)

### Triage Candidates (Name Includes Part But May Not Be Lookup Inputs)

- Module_Dunnage/Views/View_Dunnage_EditPartDialog.xaml: PartSpecificSpecNameTextBox
- Module_Dunnage/Views/View_Dunnage_EditPartDialog.xaml: PartSpecificUnitTextBox
- Module_Dunnage/Views/View_Dunnage_EditPartDialog.xaml: PartSpecificChoiceTextBox
- Module_Dunnage/Views/View_Dunnage_QuickAddPartDialog.xaml: PartSpecificSpecNameTextBox
- Module_Dunnage/Views/View_Dunnage_QuickAddPartDialog.xaml: PartSpecificUnitTextBox
- Module_Dunnage/Views/View_Dunnage_QuickAddPartDialog.xaml: PartSpecificChoiceTextBox

You MUST verify each triage candidate before migration.

## MCP Requirements

You MUST use all three MCP systems in execution:

1. Serena MCP:
- Identify symbols and call paths.
- Map all part/location entry points.
- Perform precise refactor targeting.

2. Context7 MCP:
- Validate third-party API usage for formatting and fuzzy matching.
- Confirm modern implementation patterns for any external library used.

3. Microsoft Learn MCP:
- Validate WinUI reusable control and binding guidance.
- Validate dependency-property and MVVM integration patterns.

If any MCP server is unavailable, you MUST document the gap and continue with available tooling.

## Deliverables

You MUST produce:

1. Final migration inventory with status per control.
2. Shared lookup framework design summary.
3. Shared control API summary (inputs, outputs, events, binding contract).
4. New lookup-type extension guide used in implementation.
5. Validation results for part and location default types.
6. Regression test summary.

## Completion Criteria

The task is complete only when all required controls are wired to the shared typed lookup workflow and the framework supports future Infor Visual data types without control-internal refactoring.

The task is complete only when Part Number and Location are configured and verified as default lookup types.