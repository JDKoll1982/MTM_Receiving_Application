# 02-FeatureChange-Reporting-PreviewPageLayoutAndReceivingOnlySummary

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this feature slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

<feature_name>
Reporting preview page layout refinement and Receiving-only summary tables
</feature_name>

<implementation_order>
02
</implementation_order>

<why_this_runs_second>
Once the preview is hosted as a real page, the page-specific layout can be refined without mixing
navigation/state concerns with module-specific rendering decisions.
</why_this_runs_second>

<confirmed_decisions>
- Only Receiving should render a summary table on the updated preview page.
- Dunnage and Volvo should not render summary tables.
- The preview page should still expose the module tabs/content areas for all included modules.
</confirmed_decisions>

<current_repo_state>
- Preview module card template currently always renders a `Summary` section above the detail table:
  - `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml`
- Summary tables are currently built for selected modules through:
  - `Module_Reporting/Contracts/IService_Reporting.cs`
  - `Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs`
  - `Module_Reporting/Services/Service_Reporting.cs`
- The preview card view model currently always carries `SummaryTable` plus `DetailSection`:
  - `Module_Reporting/Models/Model_ReportingPreviewModuleCard.cs`
</current_repo_state>

<required_outcome>
Change the reporting preview rendering so Receiving keeps its summary table while Dunnage and
Volvo render only their detail sections.
</required_outcome>

<primary_change_areas>
- Preview page card template logic.
- Preview module-card visibility rules for the summary section.
- Summary table generation/binding assumptions that currently treat all modules the same.
</primary_change_areas>

<files_that_must_change>
- `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml`
- `Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs`
- `Module_Reporting/Models/Model_ReportingPreviewModuleCard.cs`
- `Module_Reporting/Services/Service_Reporting.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_Reporting/Services/Service_ReportingFormattingTests.cs`
</files_that_must_change>

<implementation_steps>
1. Add an explicit summary-visibility rule to the preview module-card path.
2. Restrict summary rendering to Receiving cards.
3. Remove Dunnage and Volvo summary-table rendering from the preview page.
4. Keep Dunnage and Volvo detail sections intact and visible.
5. Update any formatting/export assumptions that still expect all included modules to emit summary tables.
</implementation_steps>

<task_checklist>
- [x] Restrict summary rendering to Receiving.
- [x] Remove Dunnage summary-table rendering.
- [x] Remove Volvo summary-table rendering.
- [x] Preserve detail-table rendering for all included modules.
- [x] Update tests and formatting expectations to match Receiving-only summary output.
</task_checklist>

<validation>
- Add tests proving only Receiving shows a summary section.
- Verify copied/email-formatted output still matches the new preview structure.
- Run focused reporting formatting tests.
</validation>

<guardrails>
- Do not change module inclusion logic in this slice.
- Do not redesign the options experience in this slice.
- Do not remove Receiving summary behavior.
</guardrails>

<completion_criteria>
- Receiving still renders a summary table.
- Dunnage and Volvo no longer render summary tables.
- All included modules still render their detail sections correctly.
</completion_criteria>