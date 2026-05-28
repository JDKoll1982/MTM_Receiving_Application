# 03-FeatureChange-Reporting-OptionsPageSettingsCardsAndDisplayModeDropdown

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this feature slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

<feature_name>
Reporting options redesign with collapsible settings cards and row-display dropdown guidance
</feature_name>

<implementation_order>
03
</implementation_order>

<why_this_runs_third>
The options experience should be redesigned after the preview page host exists, because the
options flow is no longer constrained by the current dialog overlay layout.
</why_this_runs_third>

<confirmed_decisions>
- The options dialog must be rewritten as part of the updated page flow.
- The row-display configuration must use collapsible settings cards in the same visual spirit as the Customer Pull n' Pack filter card.
- The row-display settings card must be collapsed by default.
- Row display mode selection must move from bullet/radio selection to a dropdown.
- A contextual explanation region below the dropdown must describe the currently selected mode.
</confirmed_decisions>

<current_repo_state>
- Current options UX is an overlay inside the preview dialog:
  - `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml`
- Current row-display mode selection uses radio buttons backed by boolean convenience properties:
  - `ViewModel_Reporting_Main.IsRawRowsMode`
  - `ViewModel_Reporting_Main.IsUniquePartNumbersEntireDateRangeMode`
  - `ViewModel_Reporting_Main.IsUniquePartNumbersAndLotNumbersEntireDateRangeMode`
  - `ViewModel_Reporting_Main.IsUniquePartNumbersPerDayMode`
  - `ViewModel_Reporting_Main.IsUniquePartNumbersAndLotNumbersPerDayMode`
- Current enum source:
  - `Module_Reporting/Models/Enum_ReportingPreviewRowDisplayMode.cs`
</current_repo_state>

<required_outcome>
Replace the current options overlay UX with a page-integrated settings experience that uses
collapsible settings cards, a dropdown-based row display mode selector, and explanatory guidance
for the selected display mode.
</required_outcome>

<primary_change_areas>
- Rewrite the current options overlay layout.
- Replace row-display radio buttons with a dropdown-driven selector.
- Add dropdown-linked explanation text.
- Default the row-display settings card to collapsed.
</primary_change_areas>

<files_that_must_change>
- `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml`
- `Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs`
- `Module_Reporting/Models/Enum_ReportingPreviewRowDisplayMode.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_Reporting/ViewModels/ViewModel_Reporting_MainTests.cs`
</files_that_must_change>

<implementation_steps>
1. Remove the current dialog-overlay-oriented options presentation.
2. Introduce a page-integrated settings region built from collapsible settings cards.
3. Make the row-display settings card collapsed by default.
4. Replace the radio-button selection model with a dropdown bound to the row-display mode enum/options.
5. Add an explanation region below the dropdown that changes with the selected row-display mode.
6. Keep module options in the same overall settings area, but aligned with the new page-integrated card model.
</implementation_steps>

<task_checklist>
- [x] Replace the current overlay options presentation.
- [x] Add collapsible settings cards.
- [x] Default the row-display card to collapsed.
- [x] Replace radio buttons with a dropdown.
- [x] Add explanation text that updates with the selected display mode.
- [x] Update focused viewmodel tests for the new row-display selection shape.
</task_checklist>

<validation>
- Add tests proving the selected row-display mode still updates the same underlying enum state.
- Add tests proving the explanation text matches the selected mode.
- Confirm the row-display card is collapsed by default.
- Run focused reporting viewmodel tests.
</validation>

<guardrails>
- Do not change grouped-row aggregation rules in this slice.
- Do not change per-module column inclusion rules in this slice.
- Do not reintroduce a dialog-only options dependency after the preview host becomes page-based.
</guardrails>

<completion_criteria>
- The options experience no longer depends on the old dialog overlay.
- Row display mode is selected from a dropdown.
- The row-display settings card is collapsed by default.
- A contextual explanation region appears below the dropdown.
</completion_criteria>