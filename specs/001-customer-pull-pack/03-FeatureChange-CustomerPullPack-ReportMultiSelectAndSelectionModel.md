# 03-FeatureChange-CustomerPullPack-ReportMultiSelectAndSelectionModel

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this feature slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

<feature_name>
Customer Pull n' Pack report true multi-select and selected-lines state model
</feature_name>

<implementation_order>
03
</implementation_order>

<why_this_runs_third>
The data-source resolver and service split must be stable before the report UI selection model is
expanded. This slice changes the report control and report viewmodel selection semantics without yet
implementing the downstream waitlist batch-entry behavior.
</why_this_runs_third>

<confirmed_decisions>
- The CO report UI must support true multi-select request lines.
- The report should use `SelectedLines` instead of a single `SelectedLine` model for primary selection state.
- Fulfillment allocation will run against all visible eligible rows, not only the selected set. Do not implement allocation in this slice.
</confirmed_decisions>

<current_repo_state>
- Current report view:
  - `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackReport.xaml`
- Current grouped crystal-style control:
  - `Module_ShipRec_Tools/Views/Controls/View_CustomerPullPack_CrystalReportLines.xaml`
  - `Module_ShipRec_Tools/Views/Controls/View_CustomerPullPack_CrystalReportLines.xaml.cs`
- Current report viewmodel:
  - `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs`
- Current report models:
  - `Module_ShipRec_Tools/Models/Model_CustomerPullPack_DemandLine.cs`
  - `Module_ShipRec_Tools/Models/Model_CustomerPullPack_CrystalReportGroup.cs`
  - `Module_ShipRec_Tools/Models/Model_CustomerPullPack_LocationOption.cs`
- Current tests show single-row behavior:
  - `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReportTests.cs`
</current_repo_state>

<required_outcome>
Change the report selection model from single-row selection to true multi-select request lines, and
replace single-line-centric report state with selected-lines-centric state while preserving grouped
location behavior and keeping the slice UI-ready for later waitlist batch creation.
</required_outcome>

<primary_change_areas>
- The request-line ListView inside the crystal-style report control currently uses a single-select style.
- The request-line selection event args currently carry only one selected source-line key.
- The report viewmodel currently stores and exposes single-line selection context.
- Existing tests assert single selected rows and selection clearing rules that need to be updated for true multi-select.
</primary_change_areas>

<files_that_must_change>
- `Module_ShipRec_Tools/Views/Controls/View_CustomerPullPack_CrystalReportLines.xaml`
- `Module_ShipRec_Tools/Views/Controls/View_CustomerPullPack_CrystalReportLines.xaml.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs`
- `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackReport.xaml`
- `Module_ShipRec_Tools/Models/Model_CustomerPullPack_DemandLine.cs`
- `Module_ShipRec_Tools/Models/Model_CustomerPullPack_CrystalReportGroup.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReportTests.cs`
</files_that_must_change>

<implementation_steps>
1. Change the request-line ListView in `View_CustomerPullPack_CrystalReportLines.xaml` from single-select to true multi-select.
2. Update the request-line selection synchronization code in `View_CustomerPullPack_CrystalReportLines.xaml.cs` so it preserves multiple selected request lines instead of collapsing to the first selected item.
3. Change the request-line selection event args to carry the full selected source-line key set.
4. Update the report view or code-behind handlers so they pass the full selected set into the report viewmodel.
5. Replace the report viewmodel's single-line-centric selection state with selected-lines-centric state.
6. Remove single-row assumptions from selection summary and selection-clearing logic.
7. Preserve location selection as a group-local behavior for the current parent-part report groups, but do not implement the later waitlist batch logic in this slice.
8. Preserve enough selection metadata for the next slice to create waitlist entries per unique sub-part.
</implementation_steps>

<task_checklist>
- [x] Convert the report request-line control to true multi-select.
- [x] Change the request-line selection event payload from one key to a full selected-key set.
- [x] Add selected-lines state to the report viewmodel.
- [x] Remove or downgrade single selected-line assumptions where they only existed to support single-select UI.
- [x] Update selection summary behavior to reflect selected-line count.
- [x] Keep grouped location selection UI functional and UI-ready for the later waitlist slice.
- [x] Update or replace single-select report tests with multi-select expectations.
</task_checklist>

<validation>
- Update the single-select tests in `ViewModel_Tool_CustomerPullPackReportTests.cs`, especially:
  - `ApplyCrystalRequestLineSelection_ShouldUpdateSingleDemandRowAndProjection`
  - selection-clearing tests that currently assume only one selected row
- Add new tests proving:
  - multiple request lines can remain selected at once
  - the viewmodel tracks a selected-lines set instead of only a single selected line
  - request-line projection inside `CrystalReportGroups` reflects multiple selected rows
- Run the focused Customer Pull n' Pack report unit tests.
</validation>

<guardrails>
- Do not implement waitlist batch entry creation in this slice.
- Do not implement fulfillment allocation in this slice.
- Do not reintroduce a hidden single-focus row as the primary selection contract; the selection contract must be `SelectedLines`.
- Keep the existing grouped crystal-style layout intact unless a small targeted UI adjustment is required for multi-select clarity.
</guardrails>

<completion_criteria>
- Users can select multiple report request lines at once.
- The report control and report viewmodel exchange a selected-lines set, not a single selected-line key.
- Existing single-select-only assumptions are removed from the report selection flow.
- The UI is ready for the next slice to build waitlist entries from the selected set.
</completion_criteria>