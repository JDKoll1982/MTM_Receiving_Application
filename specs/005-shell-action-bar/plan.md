# Implementation Plan: Persistent Shell Action Bar

**Scale note**: this change spans roughly thirty files across the shell (`MainWindow`), the new core
contract, state snapshot, and shell projection, every Receiving workflow view and view model, the
Receiving settings keys and manifest, the workflow service's startup mode resolution, the Receiving
settings hub, and the Receiving test folders. Read the call path section first: how the bar learns about
a page is the whole design, and every page edit follows from it. The settings work appended as US7 and
US8 sits outside it: US7 repairs the hub's navigation, US8 adds one settings page, and neither shares a
design decision with the bar beyond the module it lives in.

## Summary

Receiving navigation moves out of the pages and into one shell-owned action bar that sits under the page
content on the right, and the Receiving mode-selection screen is deleted so Receiving opens directly in
the operator's stored default mode. The shell stays module-agnostic by learning what to show from a new
core contract, `IViewModel_ShellActionProvider`, resolved from the active page with the same three-step
lookup the header already uses for `IViewModel_HeaderTitleProvider` and
`IViewModel_HeaderStepBarProvider`. `Module_Receiving` implements that contract on
`ViewModel_Receiving_Workflow`, so every visibility and enablement decision stays in Receiving; the bar
only renders what the contract reports and invokes the commands it is handed.

The two appended stories touch nothing in that path. US7 makes each Receiving settings card resolve the
step it names rather than its position in the list. US8 adds a Welded Part Numbers page to the same hub,
reading and writing the `settings_weldedcoils` list that Extra Tools already maintains, with the part
number normalised and confirmed against the read-only Infor Visual part master before it is stored.

## Project Structure

```text
MainWindow.xaml                     ContentFrame wrapped in a two-row grid; action bar in row 1
MainWindow.xaml.cs                  resolve, attach, and detach the provider beside the header sync
Module_Core/Contracts/ViewModels/
  IViewModel_ShellActionProvider.cs     new shell contract (read-only state plus SetDefaultMode)
Module_Core/Models/UI/
  Model_ShellActionBarState.cs          new immutable snapshot the contract returns
Module_Shared/ViewModels/
  ViewModel_Shared_ShellActionBar.cs    new shell projection the bar binds to with x:Bind
Infrastructure/DependencyInjection/
  CoreServiceExtensions.cs              registration for the projection view model
Module_Receiving/ViewModels/
  ViewModel_Receiving_Workflow.cs       implements the contract; owns the per-page matrix
  ViewModel_Receiving_ModeSelection.cs  deleted
Module_Receiving/Views/
  View_Receiving_Workflow.xaml(.cs)     nav row and mode host removed; child VMs handed to the contract
  View_Receiving_ManualEntry.xaml       Save & Finish panel removed
  View_Receiving_EditMode.xaml          Save & Finish button removed from the pager row
  View_Receiving_Review.xaml(.cs)       Save to Database and Add another Part/PO removed
  View_Receiving_ModeSelection.xaml(.cs) deleted
Module_Receiving/Services/
  Service_ReceivingWorkflow.cs          default-mode resolution, Guided fallback, write-back
Module_Receiving/Settings/
  ReceivingSettingsKeys.cs              shell label and tooltip keys; mode-selection keys removed
  ReceivingSettingsDefaults.cs          matching defaults
Module_Settings.Core/Defaults/
  settings.manifest.json                matching manifest entries; Mode Selection options dropped
Module_Settings.Receiving/ViewModels/
  ViewModel_Settings_Receiving_CategoryHub.cs   hub step list and card commands
  ViewModel_Settings_Receiving_WeldedParts.cs   new welded part list page model
Module_Settings.Receiving/Views/
  View_Settings_Receiving_CategoryHub.xaml(.cs) each card carries the step title it opens
  View_Settings_Receiving_WeldedParts.xaml(.cs) new welded part list page
Module_Settings.Receiving/Models/
  Model_Settings_ReceivingWeldedPart.cs         stored row id and part number
Module_Settings.Receiving/Data/
  Dao_Settings_ReceivingWeldedParts.cs          sp_Settings_WeldedCoils_GetAll, _Insert, _Delete
Module_Settings.Receiving/Contracts/
  IService_Settings_ReceivingWeldedParts.cs     the page's one dependency
Module_Settings.Receiving/Services/
  Service_Settings_ReceivingWeldedParts.cs      normalise, 11-character limit, part-master check
Module_Core/Models/Enums/
  Enum_ShellActionKind.cs               Plain, Back, Save; what the action colour is chosen from
Module_Core/Helpers/UI/
  Helper_ShellBrush.cs                  static brush lookups bound from XAML
Module_Core/Themes/
  ModuleAccentBrushes.xaml              ShellSuccessBrush and ShellDangerBrush, light and dark
MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/
  ViewModel_Receiving_ShellActionTests.cs   new per-page matrix tests
  ViewModel_Receiving_ModeSelectionTests.cs deleted
MTM_Receiving_Application.Tests/Unit/Module_Settings/Receiving/Services/
  Service_Settings_ReceivingWeldedPartsTests.cs  normalise, length, part-master rules
```

**Structure Decision**: the contract lives in `Module_Core` beside the two header provider contracts, the
projection lives in `Module_Shared` because the bar is cross-module presentation, and the implementation
stays in `Module_Receiving`. Nothing Receiving-specific moves into `Module_Core` or `Module_Shared`. The
welded part list stays inside `Module_Settings.Receiving` because it is a settings page. Its data access is
the only new DAO, and it carries no SQL text: it calls the `sp_Settings_WeldedCoils_*` procedures that
already exist, so the list it edits is the same one Extra Tools shows.

## Call Paths

```calls shell resolves the action bar from the active page
  MainWindow.SyncPageHeader @ MainWindow.xaml.cs:2554
    MainWindow.ClearHeaderSubscription @ MainWindow.xaml.cs:2263
+   MainWindow.ResolveShellActionProvider @ MainWindow.xaml.cs:2580
    MainWindow.TrackHeaderStepBar @ MainWindow.xaml.cs:2637
```

The new resolver sits beside the two existing ones and uses the same lookup order.

```calls receiving composes the snapshot the shell reads
+ ViewModel_Receiving_Workflow.ShellActions @ Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs:603
    ViewModel_Receiving_Workflow.UpdateNextButtonEnabled @ Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs:570
    ViewModel_Receiving_Workflow.RefreshClearLabelDataAvailabilityAsync @ Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs:554
    ViewModel_Receiving_Workflow.CanGoToNextStep @ Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs:603
    Service_ReceivingWorkflow.StartWorkflowAsync @ Module_Receiving/Services/Service_ReceivingWorkflow.cs:139
```

Note: the first row is new; the rest are the members it reads and the startup resolver it depends on.

Every other crossing the feature needs is a plain command call on an existing member, so it stays out of a
call path block: mode entry calls `GoToStep`, default persistence calls
`IService_UserPreferences.UpdateDefaultReceivingModeAsync`, and Overlay calls `RequestOpenLabelOverlay`.

```calls a settings card opens the page it names
+ View_Settings_Receiving_CategoryHub.OnStepClicked @ Module_Settings.Receiving/Views/View_Settings_Receiving_CategoryHub.xaml.cs
    ViewModel_Settings_Receiving_CategoryHub.Steps @ Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_CategoryHub.cs
      Module_Settings.Core.Models.Model_SettingsNavigationStep.Title
```

The card carries the step title in its `Tag`; the handler resolves the step by that title and never by the
card's position, which is the whole of US7.

```calls a welded part number reaches the list Extra Tools shows
  ViewModel_Settings_Receiving_WeldedParts.AddPartAsync @ Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_WeldedParts.cs
    Service_Settings_ReceivingWeldedParts.AddAsync @ Module_Settings.Receiving/Services/Service_Settings_ReceivingWeldedParts.cs
      IService_InforVisual.PartExistsAsync @ read-only Infor Visual part master
      Dao_Settings_ReceivingWeldedParts.InsertAsync @ Module_Settings.Receiving/Data/Dao_Settings_ReceivingWeldedParts.cs
        sp_Settings_WeldedCoils_Insert
```

That chain is US8's whole design: validate in the service, write through the existing procedure, then reload
so the page and Extra Tools, Welded Coils read the same list.

## Code Notes

```csharp Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs:599-603
        /// <summary>
        /// Mirrors the workflow Next button so the Next shortcut cannot advance a step that the
        /// button keeps disabled, such as PO entry without a selected part.
        /// </summary>
        private bool CanGoToNextStep() => ShowWorkflowNextButton && IsNextButtonEnabled;
```

pin 603: the bar's Next slot and Ctrl+Right read this one gate; keep them reading it.

```csharp Module_Receiving/Services/Service_ReceivingWorkflow.cs:66-66
        private Enum_ReceivingWorkflowStep _currentStep = Enum_ReceivingWorkflowStep.ModeSelection;
```

pin 66: deleted with the enum member; the field starts at POEntry because startup always resolves a mode.

```csharp Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs:554-556
        public async Task RefreshClearLabelDataAvailabilityAsync()
        {
            try
```

pin 554: the only place CanClearLabelData is refreshed, so it must run on Manual Entry and Edit Mode too.

Two more pins worth carrying: `ClearHeaderSubscription` hides the step bar because `TrackHeaderStepBar`
returns early on a null provider, so the bar's `Detach` must collapse its row the same way; and
`SetDefaultMode(string? storedModeValue)` takes the stored string rather than an enum, so the core
contract carries no Receiving type.

## States

```states action bar
NoActions: nothing is offered here, so the row collapses. (start) shows content-only
GuidedStep: a guided walk step is active. shows action-bar
ModeOnlyPage: Manual Entry or Edit Mode is active. shows action-bar
ReviewPage: Review is active. shows action-bar
Saving: the save is in flight. shows content-only
ResultPage: Complete or failed, success or failure alike. shows action-bar
NoActions -> GuidedStep: Receiving opens in guided
NoActions -> ModeOnlyPage: Receiving opens in manual or edit
GuidedStep -> ModeOnlyPage: a mode control is pressed
GuidedStep -> ReviewPage: the walk reaches review
ModeOnlyPage -> ReviewPage: save and finish advances
ReviewPage -> Saving: save to database
Saving -> ResultPage: the save ends
ResultPage -> GuidedStep: start new entry in the back slot
grid:
  NoActions GuidedStep ModeOnlyPage ReviewPage
  Saving ResultPage . .
```

```states default mode
NoDefault: nothing is stored, so Guided is assumed and stored. (start)
GuidedDefault: guided is the stored default.
ManualDefault: manual is the stored default.
EditDefault: edit is the stored default.
NoDefault -> GuidedDefault: opening Receiving with nothing stored
GuidedDefault -> ManualDefault: tick Manual
GuidedDefault -> EditDefault: tick Edit
ManualDefault -> EditDefault: tick Edit
EditDefault -> ManualDefault: tick Manual
ManualDefault -> NoDefault: untick Manual
EditDefault -> NoDefault: untick Edit
grid:
  NoDefault GuidedDefault ManualDefault EditDefault
```

## Screens

```screen action-bar Page content with the shell action strip
title: Select Part
row:
  chip: Guided (1)
  chip: Manual
  chip: Edit
  chip: Overlay
  button: Back
  button: Next
  button: Clear Label Data (2)
  button: Help
```

1: Each of the first three chips carries an unlabelled default checkbox, and a divider follows it.
2: Next is the accent button and Clear Label Data is red; Help and Overlay are icon buttons.

```screen content-only Page content with no action strip
title: Dashboard
row:
  text: the page content fills the full remaining height
```

```screen receiving-settings-hub Cards that name the page they open
title: Receiving Settings
grid:
  card: Entry Defaults
  card: Validation Rules
  card: Part Formatting
  card: Workflow Defaults
  card: Keyboard Shortcuts
  card: Custom Vendor Variables
  card: Welded Part Numbers
```

Each card's tag is the exact step title it opens, so the hub needs no index bookkeeping.

```screen welded-part-numbers Add and remove the welded part list
title: Welded Part Numbers
field: Part Number (max 11 characters, upper-cased)
button: Add Part
field: Search part numbers
list: welded part numbers, each row with Remove
```

The add is refused with a message when the part is blank, too long, already listed, or absent from the part
master; nothing is stored in any of those cases.

## Constitution Check

| Principle | Assessment |
| --- | --- |
| I. MVVM and View Purity | PASS. Views keep only wiring; the bar binds `x:Bind` to one shell projection, and all decisions live in the Receiving view model. |
| II. Data Access Integrity | PASS. The bar adds no DAO. US8 adds one, `Dao_Settings_ReceivingWeldedParts`, whose writes all go through the existing `sp_Settings_WeldedCoils_*` procedures, so no SQL text appears in C#. Infor Visual is read only, through `IService_InforVisual.PartExistsAsync`. |
| III. CQRS and Mediator First | VIOLATION, justified. This is presentation wiring over existing commands and services; introducing MediatR handlers for view state would add a layer the Receiving workflow never had. Recorded below. |
| IV. Dependency Injection and Modular Boundaries | PASS. New registrations go in `Infrastructure/DependencyInjection`, none in `App.xaml.cs`. `Module_Core` gains only a presentation contract, a plain state model, an action-kind enum and two static brush helpers. |
| V. Validation, Errors, and Structured Logging | PASS. Existing validation gates and `IService_ErrorHandler` calls are reused unchanged; the contract adds no failure path of its own. |
| VI. Security and Session Discipline | PASS. Default mode is read and written for the signed-in user only; no credential or connection change. |
| VII. Library-First Reuse | PASS. No new package. The bar reuses CommunityToolkit commands, the collection controls already in use, and the existing help service. |

## Complexity Tracking

| Violation | Why needed | Simpler alternative rejected |
| --- | --- | --- |
| No new MediatR command or handler for the bar's actions | The bar re-invokes commands that already exist on Receiving view models, and FR-024 forbids re-implementing them. Adding handlers would duplicate those commands behind a second path. | Routing Next, Back, and Save through MediatR would rewrite working workflow orchestration far outside this feature's scope. |

## Constraints

- Flow stays View -> ViewModel -> Service -> DAO. The shell projection view model holds no domain logic; it
  forwards to whatever the attached provider exposes.
- `x:Bind` only. No runtime `{Binding}`, including for the mode checkboxes.
- No business logic in `MainWindow.xaml.cs`. Resolving, attaching, detaching, and flipping a row height are
  view wiring, matching what `SyncPageHeader` already does.
- `MainWindow` must not name a `Module_Receiving` type for these actions. The four existing
  `ContentFrame.Content is View_Receiving_Workflow` checks are pre-existing and must not grow.
- MySQL writes stay stored-procedure only; the default-mode write already is, and so is every welded part
  write.
- Infor Visual stays read-only. The welded part page reads the part master through
  `IService_InforVisual.PartExistsAsync`; the bar reads nothing new from it.
- Every user-visible string the bar shows comes from `ReceivingSettingsKeys` through
  `IService_ReceivingSettings`, with the defaults holding the verbatim copy.

## See Also

- [spec.md](spec.md)
- [research.md](research.md)
- [data-model.md](data-model.md)
- [contracts/shell-action-provider.md](contracts/shell-action-provider.md)
- [contracts/per-page-action-matrix.md](contracts/per-page-action-matrix.md)
- [contracts/welded-parts-service.md](contracts/welded-parts-service.md)
