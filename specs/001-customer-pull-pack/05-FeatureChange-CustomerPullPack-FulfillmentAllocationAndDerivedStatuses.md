# 05-FeatureChange-CustomerPullPack-FulfillmentAllocationAndDerivedStatuses

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this feature slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

<feature_name>
Customer Pull n' Pack fulfillment allocation, Qty Satisfied, and derived Complete/Partially Filled statuses
</feature_name>

<implementation_order>
05
</implementation_order>

<why_this_runs_last>
This slice depends on the final data-source split and report selection model being stable. It also
adds derived projection fields and report behavior that would otherwise conflict with earlier DAO,
service, and queue changes.
</why_this_runs_last>

<confirmed_decisions>
- Fulfillment allocation runs across all visible eligible rows, not only the selected rows.
- Allocation runs separately within each `ParentPartId` / sub-part group, not across the whole report.
- `Oldest Added` must use `CUSTOMER_ORDER.CREATE_DATE`.
- `Complete` and `Partially Filled` are derived UI/projection statuses only.
- `Qty Satisfied` lives on the Customer Pull n' Pack demand projection only; it is not persisted in MTM storage.
</confirmed_decisions>

<current_repo_state>
- Current demand projection model:
  - `Module_ShipRec_Tools/Models/Model_CustomerPullPack_DemandLine.cs`
- Current live demand DAO and SQL:
  - `Module_ShipRec_Tools/Data/CustomerPullPack/Dao_CustomerPullPackDemand.cs`
  - `Database/InforVisualScripts/Queries/CustomerPullPack/01_GetCustomerPullPackDemand.sql`
- Current report handler:
  - `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackReportHandler.cs`
- Current report viewmodel and grouped projection:
  - `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs`
  - `Module_ShipRec_Tools/Models/Model_CustomerPullPack_CrystalReportGroup.cs`
- Current tests:
  - `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Query_CustomerPullPackReportHandlerTests.cs`
  - `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReportTests.cs`
</current_repo_state>

<required_outcome>
Add a deterministic, oldest-first allocation pass per sub-part group that computes `Qty Satisfied`
and derived `Complete` / `Partially Filled` projection state without persisting those derived values.
</required_outcome>

<primary_change_areas>
- Extend the live demand query path to expose `CUSTOMER_ORDER.CREATE_DATE` into the demand projection.
- Add projection fields for `OldestAdded`, `QtySatisfied`, and derived fulfillment status.
- Run the allocation pass after demand rows are loaded and before report groups are projected.
- Surface the derived status and satisfied quantity in the report projection/UI.
</primary_change_areas>

<files_that_must_change>
- `Database/InforVisualScripts/Queries/CustomerPullPack/01_GetCustomerPullPackDemand.sql`
- `Module_ShipRec_Tools/Data/CustomerPullPack/Dao_CustomerPullPackDemand.cs`
- `Module_ShipRec_Tools/Models/Model_CustomerPullPack_DemandLine.cs`
- `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackReportHandler.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs`
- `Module_ShipRec_Tools/Models/Model_CustomerPullPack_CrystalReportGroup.cs`
- `Module_ShipRec_Tools/Views/Controls/View_CustomerPullPack_CrystalReportLines.xaml`
- `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Query_CustomerPullPackReportHandlerTests.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReportTests.cs`
</files_that_must_change>

<implementation_steps>
1. Extend the live SQL query so the demand projection can carry `CUSTOMER_ORDER.CREATE_DATE`.
2. Add a stable projection field for the chosen `Oldest Added` value.
3. Add projection fields for:
   - `QtySatisfied`
   - derived fulfillment status
4. Group visible eligible demand rows by `ParentPartId` / sub-part group.
5. Within each group, sort rows by `CUSTOMER_ORDER.CREATE_DATE` ascending.
6. For each group, start from the available FG on-hand total for that group and run the allocation loop in the exact rules order from `ChangesNeeded.md`.
7. Mark fully covered rows as `Complete` with `QtySatisfied == ShipQuantity`.
8. Mark the first partially covered next-oldest row as `Partially Filled` with `0 < QtySatisfied < ShipQuantity`.
9. Leave untouched rows with zero satisfied quantity and no derived fulfilled state.
10. Keep the derived results on the projection only; do not persist them in MySQL waitlist or other MTM storage.
11. Surface the derived results in the report UI so users can see the allocation outcome clearly.
</implementation_steps>

<task_checklist>
- [ ] Add `CUSTOMER_ORDER.CREATE_DATE` to the live demand projection.
- [ ] Add an `Oldest Added` projection field based on `CUSTOMER_ORDER.CREATE_DATE`.
- [ ] Add `QtySatisfied` to the demand projection.
- [ ] Add derived fulfillment status to the demand projection.
- [ ] Run allocation per `ParentPartId` / sub-part group.
- [ ] Sort allocation by oldest added within each group.
- [ ] Mark fully covered rows as `Complete`.
- [ ] Mark only the first partially covered row as `Partially Filled`.
- [ ] Prevent negative inventory during the allocation pass.
- [ ] Surface the derived results in the report projection/UI.
- [ ] Keep all derived values non-persistent.
</task_checklist>

<validation>
- Add report-handler tests covering:
  - oldest-first ordering by `CUSTOMER_ORDER.CREATE_DATE`
  - full allocation producing `Complete`
  - partial allocation producing exactly one `Partially Filled` row per group
  - untouched rows remaining unsatisfied
  - no negative remaining inventory
- Add report-viewmodel/projection tests proving the derived values are visible in the projected report data.
- Confirm the allocation is per `ParentPartId` / sub-part group and not across unrelated groups.
- Run the focused Customer Pull n' Pack report tests.
</validation>

<guardrails>
- Do not persist `QtySatisfied` or derived fulfillment status to MySQL waitlist data.
- Do not reuse a Visual-native completion status unless it exactly matches the confirmed behavior; this slice uses derived projection-only statuses.
- Do not scope allocation to the current selection set; allocation must consider all visible eligible rows.
- Do not use any field other than `CUSTOMER_ORDER.CREATE_DATE` as the authoritative `Oldest Added` rule.
</guardrails>

<completion_criteria>
- The report demand projection carries `Oldest Added`, `QtySatisfied`, and a derived fulfillment status.
- Allocation runs deterministically per sub-part group using `CUSTOMER_ORDER.CREATE_DATE` ordering.
- `Complete` and `Partially Filled` are shown as derived UI/projection states only.
- `QtySatisfied` is visible in the projection and never persisted outside the projection path.
</completion_criteria>