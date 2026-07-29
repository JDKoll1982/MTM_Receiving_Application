# Scanner View Bindings Reference

Last Updated: 2026-07-21

This document defines the required x:Bind map for scanner views and is used as the implementation baseline for Step 5.

## Scope

The binding map covers these required scanner views:

- View_Scanner_Main
- View_Scanner_Workbench
- View_Scanner_History
- View_Scanner_Settings

## View Scanner Main

Source ViewModel: ViewModel_Scanner_Main

Required bindings:

- Workbench host visibility: ViewModel.IsWorkbenchVisible
- History host visibility: ViewModel.IsHistoryVisible
- Settings host visibility: ViewModel.IsSettingsVisible
- Header title source: ViewModel.CurrentHeaderTitle

## View Scanner Workbench

Source ViewModel: ViewModel_Scanner_Workbench

Session control bindings:

- Owner user id input: ViewModel.OwnerUserId (TwoWay)
- Owner display name input: ViewModel.OwnerDisplayName (TwoWay)
- Target window title input: ViewModel.AppWindowTitleSnapshot (TwoWay)
- Session status indicator visibility: ViewModel.HasActiveSession
- Start draft command: ViewModel.StartDraftSessionCommand
- Build run snapshot command: ViewModel.BuildRunSnapshotCommand

Draft item entry bindings:

- Part id: ViewModel.NewPartId (TwoWay)
- From warehouse: ViewModel.NewFromWarehouse (TwoWay)
- From location: ViewModel.NewFromLocation (TwoWay)
- To warehouse: ViewModel.NewToWarehouse (TwoWay)
- To location: ViewModel.NewToLocation (TwoWay)
- Quantity: ViewModel.NewQuantity (TwoWay)
- Add item command: ViewModel.AddDraftItemCommand

Validation feedback bindings:

- Validation state text: ViewModel.LastValidationStatus
- Validation notes text: ViewModel.LastValidationNotes

Draft list bindings:

- Items source: ViewModel.SessionItems
- Selected list item: ViewModel.SelectedSessionItem (TwoWay)
- Item columns: SequenceNumber, PayloadPartId, PayloadFromWarehouse, PayloadFromLocation, PayloadToWarehouse, PayloadToLocation, PayloadQuantity, ValidationState, ValidationNotes, ExecutionState

Workbench detail and control bindings:

- Manage items command: ViewModel.ManageItemsDialogCommand
- Selected item detail fields: SelectedSessionItem.PayloadPartId, PayloadFromWarehouse, PayloadFromLocation, PayloadToWarehouse, PayloadToLocation, ValidationNotes
- Progress counters: CurrentSession.SentItems, CurrentSession.FailedItems, CurrentSession.WaitingItems
- Send control commands: ViewModel.CheckAllCommand, SendNextCommand, SendAllCommand, StopAfterThisCommand, ClearHistoryCommand, ExportCommand

Bottom navigation bindings:

- Workbench tab command: ViewModel.NavigateToWorkbenchCommand
- History tab command: ViewModel.NavigateToHistoryCommand
- Settings tab command: ViewModel.NavigateToSettingsCommand

## View Scanner History

Source ViewModel: ViewModel_Scanner_History

Filter and command bindings:

- Owner user id: ViewModel.OwnerUserId (TwoWay)
- Date from: ViewModel.DateFromUtc (TwoWay)
- Date to: ViewModel.DateToUtc (TwoWay)
- Status options source: ViewModel.StatusOptions
- Status selected value: ViewModel.StatusFilter (TwoWay)
- Max results: ViewModel.MaxResults (TwoWay)
- Refresh command: ViewModel.RefreshHistoryCommand
- Clear-filters command: ViewModel.ClearFiltersCommand

Run list bindings:

- Items source: ViewModel.Runs
- Selected run: ViewModel.SelectedRun (TwoWay)
- Item columns: StartedUtc, FinalStatus, SessionId, SentItems, FailedItems, WaitingItems, FailureSummary

Detail and item-result bindings:

- Selected run summary fields: SelectedRun.OwnerDisplayName, SelectedRun.OwnerUserId, SelectedRun.FailureSummary
- Selected run item source: ViewModel.SelectedRunItems
- Item result columns: SequenceNumber, PartId, FromWarehouse, FromLocation, ToWarehouse, ToLocation, ExecutionState, ValidationState, IssueType, IssueMessage

Bottom navigation bindings:

- Workbench tab command: ViewModel.NavigateToWorkbenchCommand
- History tab command: ViewModel.NavigateToHistoryCommand
- Settings tab command: ViewModel.NavigateToSettingsCommand

## View Scanner Settings

Source ViewModel: ViewModel_Scanner_Settings

Profile editor bindings:

- Owner user id: ViewModel.OwnerUserId (TwoWay)
- Profile name: ViewModel.ProfileName (TwoWay)
- Target executable: ViewModel.TargetExecutableName (TwoWay)
- App window title: ViewModel.AppWindowTitle (TwoWay)
- Target child window title: ViewModel.TargetChildWindowTitle (TwoWay)
- App window class: ViewModel.AppWindowClass (TwoWay)
- From warehouse default: ViewModel.FromWarehouseDefault (TwoWay)
- To warehouse default: ViewModel.ToWarehouseDefault (TwoWay)
- Exact-title flag: ViewModel.RequireExactTitleMatch (TwoWay)
- Activation delay: ViewModel.ActivationDelayMs (TwoWay)
- Delay between fields: ViewModel.DelayBetweenFieldsMs (TwoWay)
- Pause after item: ViewModel.PauseAfterItemMs (TwoWay)
- Popup timeout: ViewModel.PopupTimeoutMs (TwoWay)
- Popup close timeout: ViewModel.PopupCloseTimeoutMs (TwoWay)
- Send hotkey chord: ViewModel.SendShortcutChord (TwoWay)
- Stop hotkey chord: ViewModel.StopShortcutChord (TwoWay)
- Allow advanced timing: ViewModel.AllowAdvancedTiming (TwoWay)

Profile command bindings:

- Load profiles command: ViewModel.LoadProfilesCommand
- Save profile command: ViewModel.SaveProfileCommand
- Set default profile command: ViewModel.SetDefaultProfileCommand
- New profile command: ViewModel.NewProfileCommand
- Duplicate profile command: ViewModel.DuplicateProfileCommand
- Delete profile command: ViewModel.DeleteProfileCommand
- Reset editor command: ViewModel.ResetEditorCommand
- Safe defaults command: ViewModel.ApplySafeDefaultsCommand

Profile list bindings:

- Profiles list source: ViewModel.Profiles
- Selected profile: ViewModel.SelectedProfile (TwoWay)

Bottom navigation bindings:

- Workbench tab command: ViewModel.NavigateToWorkbenchCommand
- History tab command: ViewModel.NavigateToHistoryCommand
- Settings tab command: ViewModel.NavigateToSettingsCommand

## Validation Checklist

- Every binding path in this file resolves to an existing ViewModel property or command.
- Every command listed is generated by RelayCommand and referenced by x:Bind.
- No runtime Binding is used.
- View layout keeps horizontal and vertical stretch behavior.
