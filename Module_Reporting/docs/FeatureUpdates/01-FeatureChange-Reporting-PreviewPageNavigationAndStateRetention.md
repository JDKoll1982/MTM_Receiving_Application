# 01-FeatureChange-Reporting-PreviewPageNavigationAndStateRetention

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this feature slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

<feature_name>
Reporting preview page navigation and retained selection state
</feature_name>

<implementation_order>
01
</implementation_order>

<why_this_runs_first>
The reporting redesign depends on replacing the current `ContentDialog` preview host with a real
page while preserving the existing report-selection state. That navigation shell must exist before
the preview layout, options redesign, or grouped-row behavior can be updated safely.
</why_this_runs_first>

<confirmed_decisions>
- Clicking `Generate report` must navigate to a real page instead of opening a dialog.
- When the user returns from the preview page, the reporting entry controls must keep their current values.
- The updated preview page must not render its own page header because the main shell already owns headers.
- State to preserve includes quick range, explicit date values, and selected modules.
</confirmed_decisions>

<current_repo_state>
- Current entry page:
  - `Module_Reporting/Views/View_Reporting_Main.xaml`
  - `Module_Reporting/Views/View_Reporting_Main.xaml.cs`
- Current reporting state owner:
  - `Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs`
- Current preview host is a dialog:
  - `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml`
  - `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml.cs`
- Current preview launch seam:
  - `ViewModel_Reporting_Main.PreviewRequested`
  - `View_Reporting_Main.OnPreviewRequested(...)`
</current_repo_state>

<required_outcome>
Replace the report preview dialog flow with page navigation while keeping the existing reporting
selection state alive when the user moves between the report setup page and the preview page.
</required_outcome>

<primary_change_areas>
- Remove the `ContentDialog`-only preview host assumption.
- Introduce a real preview page surface.
- Keep preview and entry state in the same viewmodel or another stable state owner so back navigation does not reset the inputs.
- Ensure the preview page does not emit a duplicate page header.
</primary_change_areas>

<files_that_must_change>
- `Module_Reporting/Views/View_Reporting_Main.xaml.cs`
- `Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs`
- `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml`
- `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml.cs`
- `MainWindow.xaml.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_Reporting/ViewModels/ViewModel_Reporting_MainTests.cs`
</files_that_must_change>

<files_to_add>
- Add a dedicated preview page under `Module_Reporting/Views/`.
- Add a page-level code-behind file only if required for thin navigation wiring.
- Add or update focused tests that prove preview navigation no longer depends on a dialog-only event flow.
</files_to_add>

<recommended_target_shape>
- Keep `ViewModel_Reporting_Main` as the durable state owner unless a stronger existing reporting state container already exists nearby.
- Route `Generate report` into page navigation after preview state is prepared.
- Route back navigation to the existing reporting entry page without clearing selection state.
- Let the main shell header own the page title region; do not add a page-local header banner in the preview page.
</recommended_target_shape>

<implementation_steps>
1. Introduce a real reporting preview page.
2. Replace the `PreviewRequested` dialog-launch flow in `View_Reporting_Main.xaml.cs` with page navigation.
3. Keep reporting selection state in the current reporting viewmodel so returning to the entry page preserves:
   - quick range
   - explicit start/end dates
   - selected module checkboxes
4. Ensure the preview page is driven by the already prepared preview state rather than rebuilding everything in code-behind.
5. Remove the assumption that the preview must be hosted by a `ContentDialog`.
6. Ensure the preview page does not add its own header section.
</implementation_steps>

<task_checklist>
- [x] Add a reporting preview page.
- [x] Replace dialog launch with page navigation.
- [x] Preserve reporting entry selections when navigating back.
- [x] Keep preview state bound to a durable state owner.
- [x] Remove duplicate header UI from the preview page.
- [x] Update focused reporting viewmodel/navigation tests.
</task_checklist>

<validation>
- Add focused tests proving `Generate report` no longer depends on the dialog-only preview event contract.
- Add tests proving the reporting entry selections persist after navigating to preview and back.
- Confirm the preview page renders without a redundant page header.
- Run the focused reporting viewmodel tests.
</validation>

<guardrails>
- Do not redesign the preview layout details in this slice beyond what is needed for page hosting.
- Do not rewrite the options UX in this slice.
- Do not reset selection state when moving between setup and preview.
</guardrails>

<completion_criteria>
- `Generate report` opens a real page instead of a `ContentDialog`.
- Returning to the reporting entry page preserves quick range, date range, and selected module state.
- The preview page does not render its own duplicate header.
</completion_criteria>