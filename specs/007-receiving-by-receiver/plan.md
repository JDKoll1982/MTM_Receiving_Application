# Implementation Plan: Receiving By Receiver Mode

**Feature Branch**: `007-receiving-by-receiver`

**Spec**: [spec.md](./spec.md)

## Summary

Add a fourth receiving mode, "By Receiver", that runs the existing guided walk but opens on a list of
recent inventory receipts read from Infor Visual instead of the "Enter PO" screen. The operator filters
by receiver number, pages through the list, picks a row, and lands on Load Entry with the location and the
remaining quantity already carried from that row. Everything after Load Entry, including Review and save,
is the walk that already exists.

Technically this is one new workflow step hosted by the existing workflow view, one new read-only
Infor Visual query behind the existing `IService_InforVisual` seam, one paged list view model, and a fifth
chip in the shell action bar. Two pieces of workflow state are added to the receiving workflow service: the
receiving mode that opened the walk and the receiver row it was opened from. No new package is needed
(DevWinUI 10.9.1, already referenced, supplies the pager), no MySQL stored procedure changes, and no write
of any kind against Infor Visual.

## Project Structure

```text
Module_Core/
  Models/Enums/Enum_ReceivingWorkflowStep.cs          + ReceiverSelection
  Models/InforVisual/Model_InforVisualReceiverRow.cs  new: one listed receiver/part row
  Models/UI/Model_ShellActionBarState.cs              + ByReceiver, IsByReceiverDefault
  Contracts/Services/IService_InforVisual.cs          + GetRecentReceiversAsync
  Contracts/Services/IService_ReceivingWorkflow.cs    + receiver state members
  Data/InforVisual/Dao_InforVisualReceiver.cs         new: read-only receiver query
  Services/Database/Service_InforVisualConnect.cs     + pass-through, incl. mock branch

Module_Receiving/
  ViewModels/ViewModel_Receiving_ReceiverSelection.cs new: list, search, paging, selection
  ViewModels/ViewModel_Receiving_Workflow.cs          + chip, step hosting, active-mode rule
  ViewModels/ViewModel_Receiving_LoadEntry.cs         + remaining-quantity readout
  Services/Service_ReceivingWorkflow.cs               + opening step, receiver state, Back route
  Views/View_Receiving_ReceiverSelection.xaml(.cs)    new: list screen
  Views/View_Receiving_Workflow.xaml                  + receiver host
  Views/View_Receiving_LoadEntry.xaml                 + Qty Remaining on Line readout
  Settings/ReceivingSettingsKeys.cs / ReceivingSettingsDefaults.cs   + page size, list text

Module_Shared/ViewModels/ViewModel_Shared_ShellActionBar.cs   + ByReceiver mirror
Module_Settings.Receiving/ViewModels/
  ViewModel_Settings_Receiving_WorkflowDefaults.cs    + "By Receiver" option
  ViewModel_Settings_Receiving_BusinessRules.cs       + "By Receiver" option
MainWindow.xaml                                       + chip between Guided and Manual
Database/InforVisualScripts/Queries/35_GetRecentReceivers.sql    new
Infrastructure/DependencyInjection/ModuleServicesExtensions.cs   + VM/View registration
MTM_Receiving_Application.Tests/Unit/**              + list VM, DAO mapping, chip order
```

**Structure Decision**: the feature stays inside `Module_Receiving` for workflow behaviour and inside
`Module_Core` for the read-only Infor Visual access, because that is where every other receiving step and
every other MTMFG query already live. The only edits outside those modules are the shell chip (shared
shell view model plus `MainWindow.xaml`) and the two settings view models that hold the default-mode list.

## Call paths

```calls By Receiver opens the list instead of Enter PO
  StartWorkflowAsync() @ Module_Receiving/Services/Service_ReceivingWorkflow.cs:145
~   storedMode switch @ Module_Receiving/Services/Service_ReceivingWorkflow.cs:174
~   BuildShellActions() @ Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs:1210
+   EnterByReceiverModeAsync() @ Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs:1160
~   OnWorkflowStepChanged() @ Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs:487
+   ViewModel_Receiving_ReceiverSelection **new** @ Module_Receiving/ViewModels/ViewModel_Receiving_ReceiverSelection.cs
~   ResolveActiveMode() @ Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs:1420
```
note: ResolveActiveMode has to read the recorded entry mode, not the step, or Guided lights up for the whole walk.

```calls Rows arrive read-only from Infor Visual
  LoadAsync() @ Module_Receiving/ViewModels/ViewModel_Receiving_ReceiverSelection.cs
+   GetRecentReceiversAsync() @ Module_Core/Contracts/Services/IService_InforVisual.cs:314
+     GetRecentReceiversAsync() @ Module_Core/Services/Database/Service_InforVisualConnect.cs:1688
+       GetRecentReceiversAsync() **new** @ Module_Core/Data/InforVisual/Dao_InforVisualReceiver.cs
```
note: the DAO constructor keeps the ApplicationIntent=ReadOnly guard; the SQL file carries no statement that writes.

```calls A chosen row reaches Load Entry and comes back
  SelectRowAsync() @ Module_Receiving/ViewModels/ViewModel_Receiving_ReceiverSelection.cs
~   GoToStep() @ Module_Receiving/Services/Service_ReceivingWorkflow.cs:589
~   GenerateLoads() @ Module_Receiving/Services/Service_ReceivingWorkflow.cs:474
~   GoToPreviousStep() @ Module_Receiving/Services/Service_ReceivingWorkflow.cs:542
~   OnStepChanged() @ Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntry.cs:166
```
note: GenerateLoads reads CurrentPONumber and CurrentPart, so the selection fills both before the step changes.

## States

```states By Receiver list lifecycle
Idle: Mode not open, nothing read. (start)
Loading: Query sent to Infor Visual, list still empty. (proposed)
Loaded: Rows on screen, search and paging live. shows receiver
Empty: Query succeeded and nothing matched the search. (proposed) shows receiver
Failed: Query failed, message shown with Retry. (proposed) shows receiver
Chosen: Row held in workflow state, Load Entry next. shows loadentry
Idle -> Loading: mode entered or list refreshed (proposed)
Loading -> Loaded: rows returned (proposed)
Loading -> Empty: no rows returned (proposed)
Loading -> Failed: source returned a failure (proposed)
Empty -> Loading: search cleared or retried (proposed)
Failed -> Loading: retry pressed (proposed)
Loaded -> Chosen: row selected (proposed)
Chosen -> Loaded: Back from Load Entry (proposed)
grid:
Idle | Loading | Loaded | Chosen
.    | Empty   | Failed | .
```
note: Changing the page size or the search text returns to the first page without leaving Loaded.

## Screens

```screen receiver Receiver List
title: Receiving - By Receiver
row:
  field: Receiver number search (new) (1)
  field: Rows per page (new) (2)
  button: Refresh (new) (3)
list: Receiver Number | PO # | Line # | Part Number | Vendor | Location Received to | Qty Received | Qty Ordered | Qty Remaining on Line
row:
  button: Previous page (new)
  text: Page indicator (new)
  button: Next page (new)
  text: Empty result message (new) (4)
  text: Failure message (new) (5)
```
1: **Contains match.** Narrows as the operator types; clearing it restores the full list at page one.
2: **Page size.** Offers 25, 50, 100 and 200, opening on 50.
3: **Refresh.** Re-reads the source without leaving the mode.
4: **Empty state.** Says no receiver matched, so the surface never looks broken.
5: **Failure state.** Readable source-system message beside a Retry button.

```screen loadentry Load Entry
title: Receiving - Enter Load Information
row:
  field: Location received to (changed) (1)
  field: Number of loads
  text: Qty Remaining on Line (new) (2)
```
1: **Prefilled.** Carries the location from the chosen row and stays editable.
2: **Read-only.** Shows the chosen row's remaining quantity; hidden when the walk did not start from a receiver.

## Constitution Check

| Principle | Assessment |
|---|---|
| I. MVVM and View Purity | PASS. List state lives in `ViewModel_Receiving_ReceiverSelection`; the view binds with `x:Bind` and holds no logic. |
| II. Data Access Integrity | PASS. The new query is a read-only SQL Server read through an instance DAO returning `Model_Dao_Result<List<...>>`, guarded by the existing `ApplicationIntent=ReadOnly` check. No MySQL changes at all. |
| III. CQRS + Mediator First | JUSTIFIED VIOLATION. The receiving module has no MediatR handlers today; the walk runs on `Service_ReceivingWorkflow`. See Complexity Tracking. |
| IV. Dependency Injection and Modular Boundaries | PASS. New view model and view registered transient in `ModuleServicesExtensions`; the DAO takes its connection string by constructor. |
| V. Validation, Errors, and Structured Logging | PASS. Failures surface as `Model_Dao_Result` and a readable message with Retry; the read is logged. |
| VI. Security and Session Discipline | PASS. Infor Visual stays read-only; the receiver number is session-only and reaches no persisted table. |
| VII. Library-First Reuse | PASS. DevWinUI pager, existing `Service_Pagination`, existing `Helper_SqlQueryLoader`, existing error handler; no new dependency. |

Re-checked after Phase 1 design: unchanged.

### Complexity Tracking

| Violation | Why needed | Simpler alternative rejected |
|---|---|---|
| III. CQRS + Mediator First | The receiving walk is a stateful, multi-step session held on one service; a MediatR query here would be the first handler in the module and would still have to talk to that same service for the step change. One screen does not justify introducing a second architecture beside the walk it must drive. | A MediatR query plus handler for the receiver read: it would wrap a call the service already makes in two lines, and the walk would still be service-driven, so the module would carry two patterns for one flow. |
