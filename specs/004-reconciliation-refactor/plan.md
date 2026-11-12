# Implementation Plan: Receiving Validator

**Feature**: [spec.md](spec.md)
**Branch**: `004-reconciliation-refactor`
**Created**: 2026-11-12
**Size**: oversized (55 projected files, 65 projected tasks)

**Scale note**: This change touches about 55 files across five areas: a new working module, a new settings module, three new stored procedures, three Visual queries, and a removal sweep through Receiving, the shell, and the tests. The reader should watch two things while reviewing: the boundary between `Module_Reconciliation` and `Module_Receiving` is enforced by namespace only because the repository compiles every module into one assembly, and the quantity proof is the only place a line becomes writable.

## Summary

Extract location reconciliation from the Receiving workflow into `Module_Reconciliation`, reached from one navigation entry in the Reporting group and labelled Receiving Validator. The module works one calendar day at a time: it reads that day's receiving rows through its own data access, derives where Infor Visual moved each part, and proposes a destination per line.

The existing 821-line `Service_ReceivingLocationReconciliation` is split into focused collaborators rather than moved. A line is writable only when the two-part quantity proof passes: every destination bucket nets to exactly zero, and the source residual equals the on-hand balance at the receiving location measured as of the end of the strict window.

MySQL writes go through exactly one reconciliation-owned stored procedure. Infor Visual stays read-only.

## Technical Context

- **Language and stack**: C# 13 on .NET 10 (`net10.0-windows10.0.22621.0`), WinUI 3, CommunityToolkit.Mvvm 8.4.2. No new packages.
- **Assembly model**: one project, `MTM_Receiving_Application.csproj`. Modules are folders compiled into a single assembly, so module boundaries are namespace and folder rules, not project references. This is the fact that shapes the whole extraction and it is recorded as decision D-01.
- **Application database**: MySQL via `MySql.Data`. Every write is a stored procedure (R-DATA-03).
- **Infor Visual**: SQL Server, `ApplicationIntent=ReadOnly`, selects only (R-DATA-04).
- **Testing**: xUnit with FluentAssertions and Bogus under `MTM_Receiving_Application.Tests`. Visual is faked; the suite never reaches the live database (TS-03).
- **Performance targets**: 500 rows in 15 seconds per day, a scrollable grid at 250 rows (NF-01).

## Project Structure

```text
Module_Reconciliation/                        new module, depends on Module_Core and Module_Shared only
  Contracts/
    IService_ReconciliationDayReader.cs
    IService_ReconciliationPreview.cs
    IService_ReconciliationGroupResolver.cs
    IService_ReconciliationNonPurchaseOrderResolver.cs
    IService_ReconciliationMovementSelector.cs
    IService_ReconciliationBucketBuilder.cs
    IService_ReconciliationAllocator.cs
    IService_ReconciliationQuantityProof.cs
    IService_ReconciliationWriteService.cs
    IService_ReconciliationSettings.cs
  Data/
    Dao_ReconciliationReceivingRows.cs        own DAO over receiving_label_data / receiving_history
    Dao_ReconciliationInforVisual.cs          own read-only Visual DAO
  Models/
    Enum_ReconciliationOutcome.cs
    Model_ReconciliationRow.cs
    Model_ReconciliationGroupKey.cs
    Model_ReconciliationGroup.cs
    Model_ReconciliationMovement.cs
    Model_ReconciliationDestinationBucket.cs
    Model_ReconciliationSourceResidual.cs
    Model_ReconciliationLocationBalance.cs
    Model_ReconciliationFinding.cs
    Model_ReconciliationDay.cs
    Model_ReconciliationRun.cs
    Model_ReconciliationDaySummary.cs
  Services/
    Service_ReconciliationDayReader.cs
    Service_ReconciliationGroupResolver.cs
    Service_ReconciliationNonPurchaseOrderResolver.cs
    Service_ReconciliationMovementSelector.cs
    Service_ReconciliationBucketBuilder.cs
    Service_ReconciliationAllocator.cs
    Service_ReconciliationQuantityProof.cs
    Service_ReconciliationPreviewOrchestrator.cs
    Service_ReconciliationWriteService.cs
    Service_ReconciliationSettings.cs
  ViewModels/
    ViewModel_ReconciliationMain.cs
    ViewModel_ReconciliationNeedsAttention.cs
  Views/
    View_ReconciliationMain.xaml
    View_ReconciliationMain.xaml.cs
    View_ReconciliationNeedsAttentionWindow.xaml
    View_ReconciliationNeedsAttentionWindow.xaml.cs
  Converters/
    Converter_ReconciliationOutcomeBrush.cs
  Settings/
    ReconciliationSettingsKeys.cs
    ReconciliationSettingsDefaults.cs
  Navigation/
    ReconciliationHeaderTitleProvider.cs
  Documentation/                              per R-DOC-01 and module-doc-maintenance.instructions.md

Module_Settings.Reconciliation/               new settings project, never hosts UI inside the working module
  ViewModels/ViewModel_Settings_Reconciliation_Main.cs
  Views/View_Settings_Reconciliation_Main.xaml
  Views/View_Settings_Reconciliation_Main.xaml.cs

Database/Database_Deployment/Sql_Files/StoredProcedures/Reconciliation/
  sp_Reconciliation_ReceivingRows_GetDay.sql
  sp_Reconciliation_DayActivity_GetRange.sql
  sp_Reconciliation_ReceivingLocation_Update.sql      the single write procedure

Database/InforVisualScripts/Queries/Reconciliation/
  18a_GetTransferCandidatePairs.sql                   moved, direction taken from TYPE
  18b_GetTransferPairDetails.sql                      moved
  18c_GetLocationMovementAfterWindow.sql              new, as-of balance support

Infrastructure/DependencyInjection/
  ModuleServicesExtensions.cs                  add the module's registrations
Module_Core/Themes/ModuleAccentBrushes.xaml    add the reconciliation accent triple
MainWindow.xaml                                add the one navigation entry in the Reporting group
MainWindow.xaml.cs                             add the accent map row

MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/
  Services/ ...                                one file per collaborator
  ViewModels/ViewModel_ReconciliationMainTests.cs

Module_Receiving/ ...                          removal sweep, see Removal Surface below
```

**Structure Decision**: a sibling module folder plus a sibling settings folder, matching the layout every other module uses, with stored procedures beside their neighbours and Visual queries under a `Reconciliation` folder inside the existing query root. Query 16 and `GetRecommendedLocationsAsync` stay where they are.

## Removal Surface

The new module lands first and the old surface comes out last, one step at a time, with the solution building between steps (`R-SCOPE-03`, `MG-04`). Every path below was confirmed present on 2026-11-12.

| Path | Action | Why |
| --- | --- | --- |
| `Module_Receiving/Services/Service_ReceivingLocationReconciliation.cs` | **split, not moved** | `GetRecommendedLocationsAsync` stays with its query 16 call. The reconcile methods are replaced by the new module's collaborators (FR-006, FR-071) |
| `Module_Receiving/Contracts/IService_ReceivingLocationReconciliation.cs` | keep one method | the three reconcile methods leave; the recommendation method and its signature do not change (FR-006) |
| `Module_Receiving/Models/Model_ReceivingLocationReconciliationItem.cs` | delete | replaced by `Model_ReconciliationFinding` |
| `Module_Receiving/Models/Model_ReceivingLocationReconciliationSummary.cs` | delete | replaced by `Model_ReconciliationDaySummary` |
| `Module_Receiving/ViewModels/ViewModel_Receiving_LocationReconciliationReview.cs` | delete | replaced by the module's own ViewModel (FR-066) |
| `Module_Receiving/Views/View_Receiving_Dialog_LocationReconciliationReview.xaml` and `.xaml.cs` | delete | replaced by the module's page and modal window |
| `Module_Receiving/Views/View_Receiving_Workflow.xaml` and `.xaml.cs` | remove `ReconciliationReviewHost` | the host and its visibility plumbing go (FR-065) |
| `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs` | remove the step | six references, including the visibility flag and both switch arms |
| `Module_Receiving/Views/View_Receiving_ModeSelection.xaml` | remove the card | the card, its command binding and its accessible name (FR-064) |
| `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs` | remove `ReconcileSavedLocationsAsync` | the command has no destination once the step is gone |
| `Module_Receiving/Settings/ReceivingSettingsKeys.cs` and `ReceivingSettingsDefaults.cs` | remove two keys | `ValidateAllHistoryForLocationReconciliation` and `IgnoredReconciliationLocationsJson` (FR-060) |
| `Module_Settings.Receiving/Views/View_Settings_Receiving_Reconciliation.xaml` and `.xaml.cs` | delete | the ignored-locations editor has no setting left to edit |
| `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_UserPreferences.cs` and `_PartFormatting.cs` | remove the ignored-locations editor | both read the removed key |
| `Module_Core/Models/Enums/Enum_ReceivingWorkflowStep.cs` | remove `ReconciliationReview = 12` | nothing persists the value (R-8) |
| `Module_Core/Services/Help/Service_Help.cs` | remove three entries | key `Receiving.ReconciliationReview` at lines 160, 261 and 788 |
| `Module_Core/Contracts/Services/IService_InforVisual.cs` | remove one member | query 17 plumbing (R-7) |
| `Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs` | remove one method | query 17 plumbing |
| `Module_Core/Services/Database/Service_InforVisualConnect.cs` | remove the pass-through | query 17 plumbing |
| `Module_Core/Models/InforVisual/Model_InforVisualLocationTransaction.cs` | delete | only query 17 uses it |
| `Database/InforVisualScripts/Queries/17_GetReceivingLocationTransactionHistory.sql` | delete | zero feature callers (R-7) |
| `Database/InforVisualScripts/Queries/18_GetReceivingLocationTransferMovements.sql` | delete | superseded by the completed 18a and 18b split (R-6) |
| `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` | remove four registrations, add the module's | the two reconciliation services, the review ViewModel, the review dialog, the settings view (FR-061) |
| `MTM_Receiving_Application.Tests/Unit/Module_Receiving/Services/Service_ReceivingLocationReconciliationTests.cs` | split | recommendation tests stay, allocation tests are rewritten in the module (`TS-02`, FR-066) |
| `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_LocationReconciliationReviewTests.cs` | delete | the ViewModel it tests is gone |
| `docs/features/receiving/location-reconciliation-process.md` | move | into the module's documentation, preserving the algorithm rules (`MG-02`) |

**Deliberately untouched.** Query 16, `GetRecommendedLocationsAsync` and its result, Receiving's Edit Mode ownership rule, `Module_Receiving/Proposed_UI_Changes.xaml.md` and its renders (not reconcile specific, R-9), and the legacy `docs/` folders in the other modules.

## Design Blocks

### Calls

The preview path, which is the whole day in one call:

```text
ViewModel_ReconciliationMain
  -> IService_ReconciliationDayReader.GetDayAsync(date)
       -> Dao_ReconciliationReceivingRows.GetDayRowsAsync(date)          sp_Reconciliation_ReceivingRows_GetDay
       -> Dao_ReconciliationReceivingRows.GetActivityDatesAsync(range)   sp_Reconciliation_DayActivity_GetRange
  -> IService_ReconciliationPreview.PreviewDayAsync(day)
       -> IService_ReconciliationGroupResolver.ResolveAsync(rows)
            -> IService_ReconciliationNonPurchaseOrderResolver.ResolveAsync(rows)
       -> IService_ReconciliationMovementSelector.SelectAsync(group)
            -> Dao_ReconciliationInforVisual.GetTransferPairsAsync(part, date)    18a then 18b
       -> IService_ReconciliationBucketBuilder.BuildAsync(movements)
       -> IService_ReconciliationAllocator.AllocateAsync(rows, buckets)
       -> IService_ReconciliationQuantityProof.VerifyAsync(group, allocations)
            -> Dao_ReconciliationInforVisual.GetLocationBalancesAsync(part, locations)   18c plus current on-hand
```

The write path, entered once per accepted line:

```text
ViewModel_ReconciliationMain
  -> IService_ReconciliationWriteService.AcceptAsync(finding, destination, operator)
       -> IService_ReconciliationQuantityProof.VerifyAsync(group, allocations)
       -> Dao_ReconciliationReceivingRows.UpdateLocationAsync(row, destination, operator)
            sp_Reconciliation_ReceivingLocation_Update
```

`Service_ReconciliationPreviewOrchestrator` is the only type that composes the others. No collaborator calls another service; each takes the data it needs as a parameter.

### Code

- **Quantities**: every quantity in the module is `decimal`. Nothing round-trips through `double` or `float` (R-SCOPE-05). Messages format with `CultureInfo.InvariantCulture`.
- **Row quantity**: the proof uses `Model_ReconciliationRow.Quantity`, which maps the `quantity INT` column. It does not use `WeightQuantity`. Today's service reads `weight_quantity` and falls back to `quantity`, which is why switching the field is a behaviour change and not a rename (decision D-05).
- **Allocation comparator**, applied to the rows left after the saved-location pass:

```csharp
// Row order is the documented invariant: largest first, then load number, then load id.
rows.OrderByDescending(row => row.Quantity)
    .ThenBy(row => row.LoadNumber)
    .ThenBy(row => row.LoadId);
```

- **Bucket choice** for one row: the bucket whose remaining quantity is exactly the row quantity wins; otherwise the smallest bucket that still covers the row; ties break on latest movement time, then location id.

```csharp
// Exact fit first, then smallest sufficient bucket. The subtraction is the fit test.
bucket.RemainingQuantity - row.Quantity;
```

- **As-of residual**: the balance the residual is compared against is reconstructed, never assumed.

```csharp
// on-hand now, less everything that left or arrived at the location after the strict window,
// is the balance the residual must equal. For a same-day run the delta is empty and this is
// exactly the live on-hand figure the spec names.
var balanceAsOf = liveOnHand - netMovementAfterWindow;
```

- **Outcome assignment** is one exhaustive switch expression over `Enum_ReconciliationOutcome`, with no `else` and no string literals (R-CODE-01, R-CODE-04). The old free-text `Resolution` string does not survive.

### States

A finding moves through these states. Only `Writable` may be written.

| State | Reached when | Terminal |
| --- | --- | --- |
| `Proposed` | allocation placed the row in a bucket | no |
| `Proven` | both parts of the quantity proof passed | no |
| `Writable` | proven, and the derived destination differs from the saved location | no |
| `Written` | the write procedure reported success | yes |
| `Unresolved` | proof failed, or nothing covered the row, or Visual failed | no |
| `SkippedForRun` | the operator pressed Escape | yes, for the session |

A `Writable` finding that is accepted becomes `Written`. A finding accepted after the operator typed a destination becomes `Written` and carries `Manually Set`. A failed write returns the finding to `Unresolved` with the failure reason; the cycle does not advance.

The day moves `NotLoaded -> Loaded -> Previewing -> Previewed -> Reviewing -> Complete`, with `Failed` reachable from `Previewing` and `Reviewing`. A sweep repeats the same sequence per day and stops on the first `Failed` day.

### Screens

**Receiving Validator page**: a day bar (date picker, previous, next, Today, sweep start and end, Stop), a day summary strip, a three-column grid (part, purchase order number, quantity) grouped by part and received date with row colour carrying outcome, and a detail pane holding every remaining field from FR-042. Needs Attention is a button in the day bar, disabled when its count is zero.

**Needs Attention window**: a modal window, not a dialog, listing reason, evidence, and raw difference per line, with a location field per line that writes through the same accepted-line path.

**Failure surfaces**: an inline retry banner for an unreachable Infor Visual, a page-level reason for an unreachable application database, and a page-level empty state for no rows, all matched, and all skipped.

## Constitution Check

Gate evaluated before Phase 0 and re-checked after Phase 1 design.

| Principle | Assessment | Note |
| --- | --- | --- |
| I. MVVM and View Purity | PASS | Grid and pane bind with `x:Bind`, every binding names an explicit mode, code-behind holds view-only work. |
| II. Data Access Integrity | PASS | Instance DAOs, `Model_Dao_Result<T>`, stored-procedure-only MySQL writes, Visual opened with `ApplicationIntent=ReadOnly`. |
| III. CQRS + Mediator First | JUSTIFIED VIOLATION | `R-MVVM-09` states this module does not need CQRS. ViewModels depend on service interfaces. See Complexity Tracking. |
| IV. Dependency Injection and Modular Boundaries | PASS | Registrations go in `Infrastructure/DependencyInjection`. Module logic stays in its folder. The shell edits are a presentation table and one nav item, authorised by FR-062. |
| V. Validation, Errors, and Structured Logging | PASS | Typed results throughout, one bounded retry per Visual call, run id on every log line. |
| VI. Security and Session Discipline | PASS | No credentials in code, Visual read-only enforced by the DAO constructor guard. FR-054's role gate is recorded as an open business decision with a working default. |
| VII. Library-First Reuse | PASS | No new packages. Existing CommunityToolkit, Serilog, FluentAssertions and Bogus cover the need. |

Re-checked after Phase 1 design: unchanged, one violation, one justification.

### Complexity Tracking

| Violation | Why needed | Simpler alternative rejected |
| --- | --- | --- |
| Principle III: no MediatR handlers at the ViewModel boundary | `R-MVVM-09` records that this module does not need CQRS, and a handler per operation would wrap services that already exist and already return typed results | Command and handler per operation, which adds a layer that carries no behaviour and makes the per-line write path harder to test in isolation |
