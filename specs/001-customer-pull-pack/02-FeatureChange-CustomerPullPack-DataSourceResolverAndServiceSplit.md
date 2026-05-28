# 02-FeatureChange-CustomerPullPack-DataSourceResolverAndServiceSplit

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this feature slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

<feature_name>
Customer Pull n' Pack mock/live resolver and service-boundary split
</feature_name>

<implementation_order>
02
</implementation_order>

<why_this_runs_second>
The dedicated mock assets from `01-FeatureChange-CustomerPullPack-MockDataAssetsAndParity.md`
must exist before the feature can stop reading Customer Pull n' Pack rows from the shared Infor
Visual mock catalog. This slice rewires the workflow so source selection happens once at activation
instead of inside DAOs and viewmodels.
</why_this_runs_second>

<confirmed_decisions>
- Data-source selection must happen at module or view activation through a resolver-style service.
- All Customer Pull n' Pack screens must share the same resolved source mode during a session.
- The mock/live decision must not remain duplicated inside DAOs or viewmodels after this slice.
- Scope includes all Customer Pull n' Pack screens hosted under ShipRec Tools.
</confirmed_decisions>

<current_repo_state>
- Current toggle seam:
  - `Module_Core/Contracts/Services/IService_AppSettings.cs`
  - `Module_Core/Services/Service_AppSettings.cs`
- Current mixed DAOs:
  - `Module_ShipRec_Tools/Data/CustomerPullPack/Dao_CustomerPullPackDemand.cs`
  - `Module_ShipRec_Tools/Data/CustomerPullPack/Dao_CustomerPullPackWaitlist.cs`
- Current mixed viewmodel logic:
  - `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs`
- Current DI registration seams:
  - `Infrastructure/DependencyInjection/CoreServiceExtensions.cs`
  - `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
- Current host for workflow activation:
  - `Module_ShipRec_Tools/Views/View_ShipRecTools_Main.xaml.cs`
</current_repo_state>

<required_outcome>
Refactor Customer Pull n' Pack so the feature consumes separate mock and live services behind a
shared contract, with the active source resolved once when the workflow is activated.
</required_outcome>

<primary_change_areas>
- Introduce a Customer Pull n' Pack data-source resolver.
- Introduce separate contracts for demand and waitlist data access at the feature-service boundary.
- Remove mock/live branching from `Dao_CustomerPullPackDemand`.
- Remove mock/live branching from `Dao_CustomerPullPackWaitlist`.
- Remove direct report-viewmodel branching on `GetUseInforVisualMockData()`.
- Ensure all Customer Pull n' Pack screens consume the same resolved source mode.
</primary_change_areas>

<files_that_must_change>
- `Module_ShipRec_Tools/Data/CustomerPullPack/Dao_CustomerPullPackDemand.cs`
- `Module_ShipRec_Tools/Data/CustomerPullPack/Dao_CustomerPullPackWaitlist.cs`
- `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackReportHandler.cs`
- `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackWaitlistQueueHandler.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackQueue.cs`
- `Module_ShipRec_Tools/Views/View_ShipRecTools_Main.xaml.cs`
- `Infrastructure/DependencyInjection/CoreServiceExtensions.cs`
- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
</files_that_must_change>

<files_to_add>
- Add Customer Pull n' Pack feature-service contracts under the module contract area.
- Add resolver contract and implementation for workflow source mode.
- Add live demand/waitlist service implementations that wrap the live DAOs.
- Add mock demand/waitlist service implementations that use the module-owned Customer Pull n' Pack mock catalog.
- Add or update targeted unit tests around report handler, queue handler, and report viewmodel source resolution.
</files_to_add>

<recommended_contract_shape>
- One resolver contract that resolves and exposes the active Customer Pull n' Pack source mode.
- One demand-source contract used by report/query flows.
- One waitlist-source contract used by queue and linked-waitlist flows.
- Live implementations should wrap the existing DAOs after those DAOs become single-purpose live storage/query objects.
- Mock implementations should use the module-owned Customer Pull n' Pack mock catalog created in slice 01.
</recommended_contract_shape>

<implementation_steps>
1. Add a Customer Pull n' Pack source-mode resolver contract and implementation.
2. Resolve source mode once when Customer Pull n' Pack workflow activation begins in the ShipRec Tools host.
3. Add shared feature contracts for Customer Pull n' Pack demand retrieval and waitlist retrieval/persistence.
4. Convert `Dao_CustomerPullPackDemand` into a live-only Infor Visual DAO.
5. Convert `Dao_CustomerPullPackWaitlist` into an MTM waitlist DAO without mock branching.
6. Add mock service implementations that fulfill the same contracts using the module-owned Customer Pull n' Pack mock catalog.
7. Update query handlers to depend on the new contracts instead of the mixed DAOs.
8. Remove direct mock/live discovery from `ViewModel_Tool_CustomerPullPackReport`.
9. Ensure the report, queue, pull list, floor copy, and defaults surfaces all consume the same resolved mode.
10. Update DI so the feature contracts resolve to the correct implementation set for the active source mode.
</implementation_steps>

<task_checklist>
- [x] Add a workflow-scoped Customer Pull n' Pack source resolver.
- [x] Add separate feature contracts for demand and waitlist behavior.
- [x] Make `Dao_CustomerPullPackDemand` live-only.
- [x] Make `Dao_CustomerPullPackWaitlist` live-only for MTM persistence.
- [x] Add mock demand and waitlist service implementations.
- [x] Update query handlers to depend on the feature contracts.
- [x] Remove viewmodel-level source branching from the report viewmodel.
- [x] Update the ShipRec Tools host to establish the shared source mode for all Customer Pull n' Pack screens.
- [x] Update DI registrations to use the resolver-driven split.
- [x] Update focused unit tests to reflect the new contract boundaries.
</task_checklist>

<validation>
- Update `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Query_CustomerPullPackReportHandlerTests.cs`.
- Update `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Query_CustomerPullPackWaitlistQueueHandlerTests.cs`.
- Update `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReportTests.cs` where mock/live branching currently depends on `IService_AppSettings` plus the shared mock catalog.
- Confirm no remaining Customer Pull n' Pack production code path branches directly on `GetUseInforVisualMockData()`.
- Run a focused build/test pass for the touched ShipRec Tools slice.
</validation>

<guardrails>
- Do not implement multi-select UI changes in this slice.
- Do not implement fulfillment allocation changes in this slice.
- Do not leave the resolver as a passive helper while the DAOs still branch internally; the branch removal is part of this slice.
- Preserve existing behavior for other Infor Visual consumers outside Customer Pull n' Pack.
</guardrails>

<completion_criteria>
- Customer Pull n' Pack source mode is resolved once at activation.
- The report and queue paths depend on shared feature contracts, not directly on mixed DAOs.
- Customer Pull n' Pack production code no longer duplicates mock/live branching inside both viewmodel and DAO layers.
- All Customer Pull n' Pack screens share the same resolved source mode during a session.
</completion_criteria>