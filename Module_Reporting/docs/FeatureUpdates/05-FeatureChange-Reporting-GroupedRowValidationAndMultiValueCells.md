# 05-FeatureChange-Reporting-GroupedRowValidationAndMultiValueCells

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this feature slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

<feature_name>
Reporting grouped-row validation and explicit mixed-value cell output across all module tabs
</feature_name>

<implementation_order>
05
</implementation_order>

<why_this_runs_last>
Grouped-row value validation should be implemented after the preview host, options shell, and
column-option policies are stable, because this slice changes the core aggregation semantics used
by displayed and copied preview rows.
</why_this_runs_last>

<confirmed_decisions>
- When rows are combined into a single grouped row, the app must validate grouped source values before deciding how to display the cell.
- If grouped date values differ, a range should be shown when that makes sense.
- If grouped non-date values differ and there is no sensible range, explicit filler text should explain the condition, such as `Multiple Receivers`.
- The app must not silently assume all grouped values are identical or empty.
- This rule applies to Receiving, Dunnage, and Volvo.
</confirmed_decisions>

<current_repo_state>
- Grouped-row shaping currently lives in:
  - `Module_Reporting/Models/Model_ReportingPreviewModuleCard.cs`
- The preview card currently groups rows by display mode and then emits one value per requested column.
- The source row value access seam currently lives in:
  - `Module_Core/Models/Reporting/Model_ReportRow.cs`
- Email/copy formatting currently consumes the already-shaped preview rows:
  - `Module_Reporting/Services/Service_Reporting.cs`
  - `MTM_Receiving_Application.Tests/Unit/Module_Reporting/Services/Service_ReportingFormattingTests.cs`
</current_repo_state>

<required_outcome>
When grouped-row display modes collapse multiple source rows into one preview row, each emitted
cell value must be validated across the grouped source rows so the UI and copied report can show:
- one shared value when all values match
- a range when differing date values can be summarized as a range
- explicit mixed-value text when differing non-date values should not collapse silently
</required_outcome>

<primary_change_areas>
- Grouped-row source aggregation logic.
- Column-specific grouped-cell formatting rules.
- Shared output behavior used by both preview UI and copied/exported text.
</primary_change_areas>

<files_that_must_change>
- `Module_Reporting/Models/Model_ReportingPreviewModuleCard.cs`
- `Module_Core/Models/Reporting/Model_ReportRow.cs`
- `Module_Reporting/Services/Service_Reporting.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_Reporting/Services/Service_ReportingFormattingTests.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_Reporting/ViewModels/ViewModel_Reporting_MainTests.cs`
</files_that_must_change>

<recommended_target_shape>
- Centralize grouped-cell resolution in one helper path rather than scattering special cases across templates.
- Distinguish at least these grouped output categories:
  - identical scalar values
  - date/date-time values eligible for ranges
  - mixed categorical/text values that require explicit placeholder text
- Keep the emitted preview row values deterministic so preview UI and copied output stay aligned.
</recommended_target_shape>

<implementation_steps>
1. Identify the grouped-row cell-value seam in `Model_ReportingPreviewModuleCard`.
2. For grouped display modes, gather all source values for each included column before choosing the emitted value.
3. If every normalized value matches, emit that value.
4. If the column is date-like and multiple distinct values exist, emit a range using the minimum and maximum values.
5. If the column is non-date and multiple distinct values exist, emit explicit filler text that explains the mixed-value case rather than leaving the cell blank.
6. Apply the same grouped validation rules across Receiving, Dunnage, and Volvo.
7. Ensure copied/email-formatted output uses the same resolved grouped cell text as the preview UI.
</implementation_steps>

<task_checklist>
- [ ] Validate grouped values before emitting combined-row cells.
- [ ] Emit identical values only when all grouped values match.
- [ ] Emit ranges for meaningful date/date-time differences.
- [ ] Emit explicit mixed-value placeholder text for differing non-date values.
- [ ] Apply the rules consistently to Receiving, Dunnage, and Volvo.
- [ ] Keep preview and copied output aligned.
</task_checklist>

<validation>
- Add tests covering grouped rows with:
  - identical values
  - differing date values that should render as ranges
  - differing receiver/user/text values that should render as explicit mixed-value text
- Add tests proving the same grouped output appears in copied/email-formatted output.
- Run focused reporting formatting tests.
</validation>

<guardrails>
- Do not leave mixed-value cells blank when the grouped source data differs.
- Do not use one arbitrary source row as the representative value without validation.
- Do not scope the validation rules to only Receiving; they apply to all three module tabs.
</guardrails>

<completion_criteria>
- Grouped rows validate source values before emitting a single preview cell value.
- Date-like columns can emit ranges when grouped values differ.
- Non-date mixed values emit explicit explanatory text instead of silent blanks.
- The grouped-value rules apply consistently across Receiving, Dunnage, and Volvo.
</completion_criteria>