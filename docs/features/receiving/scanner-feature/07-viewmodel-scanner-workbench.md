# Scanner ViewModel

Last Updated: 2026-07-28

This view-model is the operational center for preparing items, validating data, and sending them to inventory.

## ViewModel Purpose

- manage the active session and batch items collection
- expose item editing, ordering, and validation commands
- trigger send-next and send-all operations
- present progress and partial-send state to the user

## Current Implementation State

- CurrentSession
- SessionItems collection
- SelectedSessionItem
- OwnerUserId and OwnerDisplayName
- AppWindowTitleSnapshot and AppWindowClassSnapshot
- LastValidationStatus and LastValidationNotes
- HasActiveSession
- Status and notification state from the shared base view-model
- ManageItemsDialogCommand

## Required State

- ActiveSession
- ActiveProfile
- StagedItems collection
- SelectedItem
- IsRunning
- IsStopRequested
- CurrentRunMessage
- CurrentRunSeverity
- SentCount, FailedCount, PendingCount
- CanSendNext, CanSendBatch, CanStop
- LastRunSummary
- ItemValidationSummaryMessage
- HasInvalidItems

## Required Commands

- CreateNewSession
- LoadSession
- SaveForLater (formerly SaveDraft)
- OpenManageItems
- AddItem
- ValidateNewItem
- RemoveItem
- MoveItemUp and MoveItemDown
- CheckAll
- SendNext
- SendAll (formerly SendBatchCycle)
- StopAfterThis (formerly StopAfterCurrentCycle)
- RetryFailedItem
- ClearHistory

## Implemented Workbench Behaviors

- StartDraftSessionAsync creates a new draft session and initializes the workbench state.
- AddDraftItemAsync validates new items and stages them into the active session without blocking the workflow when validation returns warnings or issues.
- BuildRunSnapshotAsync creates the run snapshot and updates the current session status from the workflow service response.
- CheckAllAsync revalidates all staged items and refreshes status/notes summaries.
- SendNextAsync and SendAllAsync transition eligible items into sent state and update counters and session status.
- StopAfterThisAsync records a stop request for the current batch.
- ClearHistoryAsync clears the active workbench UI state and ExportAsync writes the current batch to a text export.
- ManageItemsDialogAsync opens the batch-edit dialog, allows add/duplicate/reorder/delete actions, and persists the reordered session back through the workflow service.

## Validation Responsibilities

- perform pre-send validation of required payload fields
- validate each newly added item against Infor Visual as soon as the item is added or edited in Manage Items
- use existing fuzzy-check behavior for part id, from location, and to location resolution
- update the Manage Items Status and Notes columns with the validation result instead of blocking the add outright
- prevent execution when items remain invalid even if they were allowed into the batch for later review
- prevent execution when profile is missing or invalid
- mark items with issues clearly and keep them out of send pipeline

## Runtime Responsibilities

- call orchestration service only through explicit commands
- react to progress callbacks and update counters
- keep sent items read-only in UI
- preserve failed and pending items for review and rerun

## Error And Notification Behavior

- use standardized status surfaces from shared base view-model
- translate technical failures into user-friendly messages
- avoid direct dialog logic in this view-model

## Use Or Modify Guidance

Use existing:

- ViewModel_Shared_Base as parent base class
- IService_ErrorHandler, IService_LoggingUtility, IService_Notification from base dependencies
- IService_Dispatcher for UI-safe progress updates

Modify or extend:

- scanner module navigation host to include scanner workbench route
- scanner module command wiring to expose scanner entry points
- workbench command surface to open the manage-items dialog for inline item entry and reorder workflows
- manage-items dialog implementation in Module_Scanner.Views and its workflow persistence path

Do not modify:

- shared base class with scanner-only properties; keep scanner state local to this view-model
