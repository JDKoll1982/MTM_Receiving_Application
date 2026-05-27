# 04-FeatureChange-CustomerPullPack-WaitlistSubPartEntriesAndQueueUi

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this feature slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

<feature_name>
Customer Pull n' Pack waitlist batch entries by unique sub-part and queue side-panel UI
</feature_name>

<implementation_order>
04
</implementation_order>

<why_this_runs_fourth>
The report must already support multi-select and the feature must already have stable mock/live
source resolution before waitlist creation can batch over selected report rows and the queue can
rehydrate richer per-location detail.
</why_this_runs_fourth>

<confirmed_decisions>
- When the user creates waitlist work from the report, create one waitlist entry for each unique sub-part identifier represented by the current selected report rows.
- In the current codebase, the best-supported sub-part identifier is the grouped `ParentPartId` / `SubPartId` concept already used by print context and grouped report models.
- The queue UI row should show:
  - sub-part number
  - total locations
  - time of request
- Clicking a queue row should show all locations in a side panel, sorted by location, with:
  - location
  - quantity in that location
  - sub-part id for each location
- The side-panel locations table should be UI-ready and selectable, but no business logic should be attached to that selection yet.
- Only one waitlist queue row may be selected at a time.
- Per-location queue detail should be rehydrated from the current live/mock Customer Pull n' Pack data, not persisted as a snapshot in MTM waitlist storage.
</confirmed_decisions>

<current_repo_state>
- Current waitlist entry model is line-centric:
  - `Module_ShipRec_Tools/Models/Model_CustomerPullPack_WaitlistEntry.cs`
- Current requester dialog creates exactly one waitlist entry from one selected report line:
  - `Module_ShipRec_Tools/ViewModels/ViewModel_Dialog_CustomerPullPackWaitlistEditor.cs`
- Current queue UI is single-row select and shows a compact summary:
  - `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackQueue.xaml`
  - `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackQueue.cs`
- Existing sub-part grouping concept already exists in print-context code:
  - `Module_ShipRec_Tools/Models/Model_CustomerPullPack_SubPartPrintGroup.cs`
  - `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackPrintContextHandler.cs`
</current_repo_state>

<required_outcome>
Change waitlist creation and queue presentation so the workflow is sub-part-centric rather than
single-report-line-centric, while keeping queue selection single-row and making the side-panel
location table UI-ready for future logic.
</required_outcome>

<primary_change_areas>
- Batch waitlist creation from the selected report-line set.
- Group selected report rows by unique sub-part identifier.
- Queue projection updates so the list shows sub-part number, total locations, and request time.
- Queue side-panel expansion so it can show sorted per-location detail rehydrated from the current source contract.
- Keep queue row selection single-select.
</primary_change_areas>

<files_that_must_change>
- `Module_ShipRec_Tools/Models/Model_CustomerPullPack_WaitlistEntry.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Dialog_CustomerPullPackWaitlistEditor.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackQueue.cs`
- `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackQueue.xaml`
- `Module_ShipRec_Tools/Data/CustomerPullPack/Dao_CustomerPullPackWaitlist.cs`
- `Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackBatchUpsertHandler.cs`
- `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackWaitlistQueueHandler.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReportTests.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackQueueTests.cs`
</files_that_must_change>

<implementation_steps>
1. Define the exact sub-part grouping rule for waitlist creation using the existing `ParentPartId` / print-context `SubPartId` concept.
2. Update the report-side waitlist creation flow so it creates one waitlist entry per unique selected sub-part group instead of one entry for one selected line.
3. Update the requester dialog/viewmodel so it can build a batch shaped around unique sub-part groups.
4. Update the waitlist entry model and queue projection so each queue row clearly exposes:
   - sub-part id
   - total selected locations
   - request timestamp
5. Keep the queue list itself single-select.
6. Expand the queue side panel with a per-location table sourced by rehydrating current Customer Pull n' Pack data for the selected queue row.
7. Sort the side-panel location rows by location.
8. Include sub-part id and quantity on each side-panel row.
9. Make the side-panel location table selectable, but do not hook that selection into status updates, reassignment, or other logic in this slice.
</implementation_steps>

<task_checklist>
- [ ] Change waitlist creation to batch by unique sub-part id from the selected report rows.
- [ ] Stop treating waitlist creation as a one-line-only operation.
- [ ] Update queue row projection to show sub-part number, total locations, and request time.
- [ ] Add rehydrated per-location side-panel detail for the selected queue row.
- [ ] Sort side-panel locations by location.
- [ ] Show location, qty, and sub-part id in the side-panel table.
- [ ] Keep queue row selection single-select.
- [ ] Make the side-panel table selectable but UI-only for now.
- [ ] Update focused report and queue tests.
</task_checklist>

<validation>
- Update report tests that currently assume a single waitlist entry built from a single selected line.
- Update queue tests to assert the new queue-row projection fields.
- Add tests proving queue side-panel rows are rehydrated from current Customer Pull n' Pack data rather than persisted location snapshots.
- Add tests proving the queue ListView remains single-select.
- Add tests proving the requester flow creates one batch entry per unique selected sub-part id.
</validation>

<guardrails>
- Do not attach business logic to the new side-panel location table selection yet.
- Do not change fulfillment allocation in this slice.
- Do not persist per-location queue detail snapshots in MTM waitlist storage.
- Keep MySQL waitlist persistence focused on stable workflow state and identifiers.
</guardrails>

<completion_criteria>
- Waitlist creation from the report creates one entry per unique selected sub-part id.
- Queue rows show sub-part number, total locations, and request time.
- Queue row selection stays single-select.
- The queue side panel shows a sorted, selectable table of current locations with qty and sub-part id.
- The side-panel table is UI-ready only and not yet wired to downstream business logic.
</completion_criteria>