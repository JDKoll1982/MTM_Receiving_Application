# 06-FeatureChange-CustomerPullPack-CustomerBlurFuzzySearch

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this feature slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

<feature_name>
Customer Pull n' Pack customer textbox shared fuzzy-search resolution on blur
</feature_name>

<implementation_order>
06
</implementation_order>

<why_this_runs_sixth>
The core Customer Pull n' Pack report, source-resolution split, multiselect behavior, waitlist
batching, and fulfillment projection are already in place. This slice is a focused UX refinement
that should build on the stabilized report view, not compete with the larger workflow changes.
</why_this_runs_sixth>

<confirmed_decisions>
- The Customer Pull n' Pack customer textbox must reuse the shared fuzzy-search tool pattern already used elsewhere in the repo.
- The fuzzy-search flow should activate only when the customer textbox loses focus.
- The blur-triggered search should run only if the textbox text changed from the value captured when focus was gained.
- If the textbox is blank when focus is lost, do not trigger fuzzy search.
- The resolved textbox display must continue using the existing `ID - Name` format.
</confirmed_decisions>

<current_repo_state>
- Current customer input surface:
  - `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackReport.xaml`
  - `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackReport.xaml.cs`
- Current Customer Pull n' Pack report viewmodel:
  - `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs`
- Current customer display resolution is local and mock-catalog-based:
  - `ViewModel_Tool_CustomerPullPackReport.ResolveCustomerDisplay(...)`
- Current shared fuzzy picker infrastructure:
  - `Module_Core/Dialogs/Dialog_FuzzySearchPicker.xaml.cs`
  - `Module_Core/Models/InforVisual/Model_FuzzySearchResult.cs`
- Current shared fuzzy-search usage examples:
  - `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_MaterialAvailabilityBoard.cs`
  - `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_OutsideServiceHistory.cs`
  - `Module_ShipRec_Tools/Views/View_Tool_MaterialAvailabilityBoard.xaml.cs`
  - `Module_ShipRec_Tools/Views/View_Tool_OutsideServiceHistory.xaml.cs`
- Current shared Infor Visual fuzzy-search contract does not expose customer search yet:
  - `Module_Core/Contracts/Services/IService_InforVisual.cs`
  - `Module_Core/Services/Database/Service_InforVisualConnect.cs`
  - `Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs`
- Current feature-owned mock customer source remains the Customer Pull n' Pack mock catalog:
  - `Module_ShipRec_Tools/Contracts/Services/IService_CustomerPullPackMockDataCatalog.cs`
  - `Module_ShipRec_Tools/Services/CustomerPullPack/Service_CustomerPullPackMockDataCatalog.cs`
</current_repo_state>

<required_outcome>
When the user edits the Customer Pull n' Pack customer textbox and then tabs or clicks away, the
feature should resolve the customer through the shared fuzzy-search flow only if the value changed
since focus was gained and the blurred value is not blank. A single match should normalize the
textbox automatically; multiple matches should use the shared picker; blank or unchanged blur
should do nothing.
</required_outcome>

<primary_change_areas>
- Add a blur-trigger seam for the Customer Pull n' Pack customer AutoSuggestBox.
- Track the textbox value at focus-gain time so blur can compare old versus new values.
- Add a Customer Pull n' Pack customer-resolution method that follows the shared fuzzy-search UX pattern.
- Add shared customer fuzzy-search candidate retrieval for live mode.
- Preserve feature-owned mock customer discovery for mock mode instead of reintroducing shared mock-catalog coupling.
</primary_change_areas>

<files_that_must_change>
- `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackReport.xaml`
- `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackReport.xaml.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs`
- `Module_Core/Contracts/Services/IService_InforVisual.cs`
- `Module_Core/Services/Database/Service_InforVisualConnect.cs`
- `Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReportTests.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_Core/Services/Database/Service_InforVisualConnectTests.cs`
</files_that_must_change>

<files_to_add>
- Add a shared Infor Visual SQL query for fuzzy customer search, for example:
  - `Database/InforVisualScripts/Queries/21_FuzzySearchCustomersByIdOrName.sql`
- Add focused unit coverage for the new customer fuzzy-search path if existing service tests do not already cover it cleanly.
</files_to_add>

<recommended_target_shape>
- Keep the existing `AutoSuggestBox` rather than replacing the control type.
- Capture the customer textbox text on focus-gain in the report view layer.
- On blur, compare the normalized current text against the focus-gain snapshot.
- If unchanged, stop.
- If blank, stop.
- If changed and nonblank, call a report-viewmodel customer-resolution method.
- For live mode, retrieve customer candidates through the shared Infor Visual fuzzy-search service.
- For mock mode, derive distinct customer candidates from the feature-owned Customer Pull n' Pack mock demand rows.
- Reuse `Model_FuzzySearchResult` and `Dialog_FuzzySearchPicker` for multi-candidate confirmation.
- If there is exactly one candidate, accept it without showing the picker.
- After confirmation, normalize the textbox to `ID - Name`.
</recommended_target_shape>

<implementation_steps>
1. Add focus-gained and lost-focus handling for the Customer Pull n' Pack customer AutoSuggestBox.
2. Capture the textbox text when focus is gained.
3. On blur, normalize the current textbox text and compare it to the focus-gain snapshot.
4. If the value did not change, do nothing.
5. If the value is blank on blur, do nothing.
6. Add a report-viewmodel method that resolves customer text into a confirmed customer display value.
7. Extend the shared Infor Visual fuzzy-search contract to support customer candidate lookup by customer ID and name.
8. Add the live SQL/DAO/service path for fuzzy customer candidates.
9. Keep mock-mode customer candidates sourced from the feature-owned Customer Pull n' Pack mock catalog so the current resolver split stays intact.
10. Reuse the shared fuzzy picker behavior:
    - one candidate => accept directly
    - multiple candidates => open `Dialog_FuzzySearchPicker`
    - cancelled picker => keep the current textbox text unchanged and show a lightweight informational status
11. Write the confirmed selection back to `CustomerSearchText` using the existing `ID - Name` display shape.
12. Do not auto-refresh the report on blur in this slice; keep report loading on the existing explicit refresh flow.
</implementation_steps>

<task_checklist>
- [ ] Add focus-gain snapshot tracking for the customer textbox.
- [ ] Add blur handling for the customer textbox.
- [ ] Skip fuzzy search when blur text is unchanged.
- [ ] Skip fuzzy search when blur text is blank.
- [ ] Add shared live customer fuzzy-search candidate retrieval.
- [ ] Keep mock customer candidate retrieval feature-owned.
- [ ] Reuse `Model_FuzzySearchResult` and `Dialog_FuzzySearchPicker`.
- [ ] Auto-accept a single customer candidate.
- [ ] Normalize confirmed customer text to `ID - Name`.
- [ ] Preserve the current explicit refresh/report-load workflow.
- [ ] Add focused tests for blur-trigger and candidate-resolution behavior.
</task_checklist>

<validation>
- Add report-viewmodel tests covering:
  - changed blur text triggers customer resolution
  - unchanged blur text does not trigger customer resolution
  - blank blur text does not trigger customer resolution
  - one candidate auto-selects and normalizes the textbox
  - multiple candidates route through the shared picker and apply the chosen result
  - picker cancellation leaves the current textbox text unchanged
- Add focused shared-service tests covering the new live customer fuzzy-search path.
- Confirm the customer textbox still compiles as an `AutoSuggestBox` and the blur event wiring builds cleanly.
- Run the focused Customer Pull n' Pack report tests plus the shared Infor Visual service tests touched by the new fuzzy customer search.
</validation>

<guardrails>
- Do not replace the shared fuzzy picker with a Customer Pull n' Pack-specific dialog.
- Do not trigger fuzzy search on every keystroke in this slice.
- Do not trigger fuzzy search on blur when the textbox is blank.
- Do not trigger fuzzy search on blur when the text did not change during that focus session.
- Do not auto-refresh the report after fuzzy resolution in this slice.
- Do not reintroduce mock/live branching directly inside the report page or report viewmodel outside the established Customer Pull n' Pack source seams.
</guardrails>

<completion_criteria>
- The Customer Pull n' Pack customer textbox uses the shared fuzzy-search pattern on blur.
- Fuzzy search runs only when the textbox value changed since focus-gain and is nonblank on blur.
- Single-candidate results normalize automatically; multi-candidate results use the shared picker.
- The customer display remains in `ID - Name` format.
- The existing explicit report refresh flow remains unchanged.
</completion_criteria>