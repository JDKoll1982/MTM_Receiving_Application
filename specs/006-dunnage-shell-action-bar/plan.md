# Implementation Plan: Shell Action Bar for Non-Inventoried Labels

**Scale note**: this change spans roughly forty files across five modules. There are two centres of
gravity. The first is the shared strip: the core snapshot and the shell projection gain one more mode
slot, `MainWindow` renders it and learns its keyboard route, and `Module_Dunnage` implements the
provider contract on `ViewModel_Dunnage_WorkFlowViewModel`. Everything about the Dunnage pages follows
from that one implementation, so read `## Call Paths` first. The second is deletion: the mode-selection
screen, manual entry, its step, its shortcut key, and its help topics are all removed, and what is left
behind is a single guided-entry path. The settings-hub repair and the search audit sit outside both and
share no design decision with them.

## Summary

Non-Inventoried Labels navigation moves out of its workflow page and into the one shell strip that
already serves Receiving, and the mode-selection screen, manual mode, and manual entry are deleted so
the module opens directly in the operator's stored default mode. The shared shell already owns the
strip for Receiving, so this feature extends an existing path rather than building one: `Module_Core`
gains a fourth entry-mode slot on the snapshot it already publishes, `Module_Shared` projects it, and
`MainWindow` draws it beside the three that exist. `Module_Dunnage` then implements the same
`IViewModel_ShellActionProvider` contract `ViewModel_Receiving_Workflow` already implements, which keeps
every visibility, enablement, and leave-confirmation decision inside the module that owns the page.

The workflow's Back, Next, Review, and Save verbs move out of `View_Dunnage_WorkflowView.xaml.cs` and
into the view model, because the shell can only reach commands a page's provider exposes. The two
settings hubs resolve their cards by the name each card carries, and the application's search box is
audited against the navigation pane so it offers the same pages under the same access rules.

## Project Structure

```text
MainWindow.xaml                     fourth mode chip in the strip's left group
MainWindow.xaml.cs                  image-search mode route; one route opener shared by nav and search;
                                    catalog corrections and the scanner gate on the search path
Module_Core/Models/UI/
  Model_ShellActionBarState.cs          ImageSearch slot and IsImageSearchDefault
Module_Shared/ViewModels/
  ViewModel_Shared_ShellActionBar.cs    projects the new slot and its default box
Module_Dunnage/Contracts/
  IService_DunnageWorkflow.cs           no ModeSelection or ManualEntry step survives it
Module_Dunnage/Enums/
  Enum_DunnageWorkflowStep.cs           ModeSelection and ManualEntry members deleted
Module_Dunnage/ViewModels/
  ViewModel_Dunnage_WorkFlowViewModel.cs   implements IViewModel_ShellActionProvider; owns the matrix
  ViewModel_Dunnage_ModeSelectionViewModel.cs  deleted
  ViewModel_Dunnage_ManualEntryViewModel.cs    deleted
Module_Dunnage/Views/
  View_Dunnage_WorkflowView.xaml(.cs)   navigation row removed; child view models handed to the view model
  View_Dunnage_ModeSelectionView.xaml(.cs)  deleted
  View_Dunnage_ManualEntryView.xaml(.cs)    deleted
  View_Dunnage_ReviewView.xaml           Save All button removed
  View_Dunnage_EditModeView.xaml         Save Changes button removed
Module_Dunnage/Services/
  Service_DunnageWorkflow.cs            Guided fallback, stored-value correction, step guards
  Service_DunnageShortcuts.cs           ModeSelection shortcut dropped
Module_Dunnage/Settings/
  DunnageSettingsKeys.cs                UiText.Workflow.* and UiText.Shell.* keys
  DunnageSettingsDefaults.cs            matching defaults holding today's wording
Module_Core/Services/Help/
  Service_Help.cs                       mode-selection and manual-entry topics removed
Module_Settings.Core/Defaults/
  settings.manifest.json                matching editable entries; mode-selection shortcut dropped
Module_Settings.Dunnage/ViewModels/
  ViewModel_Settings_Dunnage_CategoryHub.cs     Label Files card added to the step list
  ViewModel_Settings_Dunnage_KeyboardShortcuts.cs  mode-selection shortcut property removed
Module_Settings.Dunnage/Models/
  Model_Settings_DunnageShortcuts.cs            mode-selection shortcut property removed
Module_Settings.Dunnage/Views/
  View_Settings_Dunnage_CategoryHub.xaml(.cs)   cards carry the step title they open; Label Files card
Module_Settings.Receiving/Views/
  View_Settings_Receiving_CategoryHub.xaml      card tags checked against their step titles
Infrastructure/DependencyInjection/
  ModuleServicesExtensions.cs           dead registrations removed; Label Files page registered
MTM_Receiving_Application.Tests/Unit/Module_Dunnage/ViewModels/
  ViewModel_Dunnage_ShellActionTests.cs         new per-page matrix tests
  ViewModel_Dunnage_ShellDefaultModeTests.cs    new default-mode tests
  ViewModel_Dunnage_ModeSelectionViewModelTests.cs  deleted
  ViewModel_Dunnage_WorkFlowViewModelTests.cs   re-pointed off the deleted neutral step
```

**Structure Decision**: nothing new is created outside `Module_Dunnage`. The strip is extended in place
across the three places it already lives, because a second strip is exactly what the spec forbids. The
Dunnage implementation stays in `Module_Dunnage`, so `Module_Core` and `Module_Shared` learn the word
"image search" as a slot name and nothing else about the module. The two new test files sit beside the
Receiving shell tests they mirror.

## Call Paths

```calls the shell learns what a Dunnage page offers
  MainWindow.SyncPageHeader @ MainWindow.xaml.cs:2644
    MainWindow.TrackShellActions @ MainWindow.xaml.cs:2756
      MainWindow.ResolveShellActionProvider @ MainWindow.xaml.cs:2728
      ViewModel_Shared_ShellActionBar.Attach @ Module_Shared/ViewModels/ViewModel_Shared_ShellActionBar.cs:73
+     ViewModel_Dunnage_WorkFlowViewModel.ShellActions @ Module_Dunnage/ViewModels/ViewModel_Dunnage_WorkFlowViewModel.cs
```

The resolver and the attach are the ones 005 added; the last row is what this feature supplies. The
resolver already tries `content is IViewModel_ShellActionProvider`, the `DataContext`, and a
`ViewModel` property, so the Dunnage page needs no shell change to be found.

```calls a mode press enters a mode or stores a default
  MainWindow.ShellModeButton_Click @ MainWindow.xaml.cs:344
~   MainWindow.EnterShellMode @ MainWindow.xaml.cs:360
      ViewModel_Dunnage_WorkFlowViewModel.EnterEditModeCommand @ Module_Dunnage/ViewModels/ViewModel_Dunnage_WorkFlowViewModel.cs
      ViewModel_Dunnage_WorkFlowViewModel.ConfirmModeChangeAsync @ Module_Dunnage/ViewModels/ViewModel_Dunnage_WorkFlowViewModel.cs
      Service_DunnageWorkflow.GoToStep @ Module_Dunnage/Services/Service_DunnageWorkflow.cs:243
~   MainWindow.SetShellDefaultMode @ MainWindow.xaml.cs:376
+     ViewModel_Shared_ShellActionBar.IsImageSearchDefault @ Module_Shared/ViewModels/ViewModel_Shared_ShellActionBar.cs
      IViewModel_ShellActionProvider.SetDefaultMode @ Module_Core/Contracts/ViewModels/IViewModel_ShellActionProvider.cs:28
      IService_UserPreferences.UpdateDefaultDunnageModeAsync @ Module_Settings.Core/Interfaces/IService_UserPreferences.cs:15
```

`EnterShellMode` and `SetShellDefaultMode` are two switches over the stored mode string; each gains an
`image-search` arm and nothing else changes.

```calls a search result opens a page under the navigation rules
  MainWindow.NavigateToSearchDestinationAsync @ MainWindow.xaml.cs:1965
    MainWindow.NavigateToRouteTagAsync @ MainWindow.xaml.cs:1998
+     MainWindow.OpenRouteAsync @ MainWindow.xaml.cs
      MainWindow.RefreshScannerAccessAsync @ MainWindow.xaml.cs:516
      MainWindow.ClearModuleDraftStateBeforeNavigationAsync @ MainWindow.xaml.cs:2084
        View_Dunnage_WorkflowView.ConfirmLeaveModuleAsync @ Module_Dunnage/Views/View_Dunnage_WorkflowView.xaml.cs:595
```

Today the navigation pane checks scanner access at line 787 and the search path does not, so extracting
one opener is what makes FR-032 hold for both. The leave confirmation is already shared, so FR-033 is a
check rather than a change.

## Code Notes

```csharp MainWindow.xaml.cs:360-370
        private void EnterShellMode(string mode)
        {
            System.Windows.Input.ICommand? command = mode switch
            {
                "guided" => ShellActionBar.Guided.Command,
                "manual" => ShellActionBar.Manual.Command,
                "edit" => ShellActionBar.Edit.Command,
                _ => null,
            };

            if (command?.CanExecute(null) == true)
```

pin 362: the mode name arrives as the chip's `Tag`, so the `image-search` arm must be added here and in
`SetShellDefaultMode` together. A chip the switch does not know is a chip that does nothing.

```csharp Module_Dunnage/Services/Service_DunnageWorkflow.cs:243-251
        public void GoToStep(Enum_DunnageWorkflowStep step)
        {
            if (
                step == Enum_DunnageWorkflowStep.TypeSelection
                && CurrentStep == Enum_DunnageWorkflowStep.Review
            )
            {
                _currentEntryLoads.Clear();
            }
```

pin 243: this guard decides when entering Select Type clears the session. Deleting `ModeSelection` from
the enum forces the condition to be rewritten, and it must keep clearing when the operator arrives from
image search or edit mode.

```csharp Module_Dunnage/Services/Service_DunnageWorkflow.cs:97-107
        public Task<bool> StartWorkflowAsync()
        {
            ClearSession();

            var currentUser = _sessionManager.CurrentSession?.User;
            if (currentUser != null && !string.IsNullOrEmpty(currentUser.DefaultDunnageMode))
            {
                switch (currentUser.DefaultDunnageMode.ToLower())
                {
                    case "guided":
                        GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
```

pin 97: the two `GoToStep(Enum_DunnageWorkflowStep.ModeSelection)` calls below this line are what FR-009
and FR-012 remove; the `manual` arm and the `default` arm both become the guided entry.

Two more members carry weight without needing a code block. `CanNavigate` at
`ViewModel_Dunnage_WorkFlowViewModel.cs:166` is the single gate the strip's slots and the page's keyboard
shortcuts both read, so a locked save cannot be advanced from either. And the initial
`_currentStepTitle` at line 124 still names the mode-selection screen; it becomes the guided entry's
title.

One more pin worth carrying: `ViewModel_Dunnage_WorkFlowViewModel` is registered `AddTransient` and
constructs its children in XAML, so the hosted view models reach it through an `AttachShellActionHosts`
call from the page's code-behind, exactly as `ViewModel_Receiving_Workflow.AttachShellActionHosts`
does. The page keeps only that call and the child wiring; no verb stays in code-behind.

## States

```states action bar
GuidedEntry: the Select Type page, with Next available once a type is chosen. shows strip-guided
GuidedWalk: Select Part or Enter Loads. shows strip-guided
DetailsPage: Enter Details, with Review in the Next slot. shows strip-review-verb
ReviewPage: Review and Save, with Save in the Next slot. shows strip-save-verb
EditPage: Edit Mode, with Save in the Next slot and its editing toolbar still on the page. shows strip-edit-verb
ImageSearchPage: Image Search, with Back. shows strip-image-search
Locked: a save or clear is in flight, so every slot is unavailable. shows strip-guided
GuidedEntry -> GuidedWalk: Next, or a type card
GuidedWalk -> DetailsPage: Next reaches details
DetailsPage -> ReviewPage: Review
ReviewPage -> Locked: Save
Locked -> GuidedEntry: the save ends and the session clears
GuidedEntry -> EditPage: the Edit chip
GuidedEntry -> ImageSearchPage: the Image Search chip
ImageSearchPage -> GuidedWalk: a part is chosen, or Back returns to where the operator came from
EditPage -> GuidedEntry: the Guided chip
Locked -> EditPage: the save ends on the edit page
grid:
  GuidedEntry GuidedWalk DetailsPage
  ReviewPage EditPage ImageSearchPage
  Locked . .
```

```states default mode
Stored: whatever the user record holds before the module opens. (start) shows strip-guided
NoDefault: nothing is stored, so Guided is assumed and written. shows strip-guided
GuidedDefault: guided is the stored default. shows strip-guided
EditDefault: edit is the stored default. shows strip-edit-verb
ImageSearchDefault: image-search is the stored default. shows strip-image-search
LegacyManual: manual is stored from an earlier version. shows strip-guided
Stored -> NoDefault: nothing is recorded
Stored -> GuidedDefault: guided is recorded
Stored -> EditDefault: edit is recorded
Stored -> ImageSearchDefault: image-search is recorded
Stored -> LegacyManual: manual is recorded
NoDefault -> GuidedDefault: opening the module writes guided
LegacyManual -> GuidedDefault: opening the module corrects the stored value
GuidedDefault -> EditDefault: Ctrl and press Edit
EditDefault -> ImageSearchDefault: Ctrl and press Image Search
ImageSearchDefault -> GuidedDefault: Ctrl and press Guided
grid:
  Stored NoDefault GuidedDefault
  EditDefault ImageSearchDefault LegacyManual
```

## Screens

```screen strip-guided The strip on a guided page
title: Select Part
row:
  chip: Guided (1)
  chip: Edit
  chip: Image Search
  button: Back
  button: Next
  button: Clear All Labels
  button: Help
```

1: Each chip is a mode button. The mode in force carries success-green text and a success-green border,
and the stored default carries the word Default beneath it. There is no separate checkbox.

Clear All Labels and Help are icon-only, each keeping its tooltip and its screen-reader name. Back is
drawn in the danger colour and the save verb in the success colour, both following the active theme.

```screen strip-review-verb Enter Details puts Review where Next sits
title: Enter Details
row:
  chip: Guided
  chip: Edit
  chip: Image Search
  button: Back
  button: Review (1)
  button: Clear All Labels
  button: Help
```

1: Review occupies the Next slot and the page draws no Review button of its own.

```screen strip-save-verb Review and Save puts the save verb where Next sits
title: Review & Save
row:
  chip: Guided
  chip: Edit
  chip: Image Search
  button: Back
  button: Save (1)
  button: Clear All Labels
  button: Help
```

1: Save occupies the Next slot. The page draws no Save All button; its per-entry Previous, Next, and
table-toggle controls are editing controls and stay.

```screen strip-edit-verb Edit Mode keeps its toolbar and loses its button
title: Edit Mode
row:
  chip: Guided
  chip: Edit
  chip: Image Search
  button: Save (1)
  button: Clear All Labels
  button: Help
```

1: Save occupies the Next slot. The load, select-all, remove, column, filter, and paging controls stay on
the page, and no Back is offered.

```screen strip-image-search Image Search offers a way back
title: Search Parts by Image
row:
  chip: Guided
  chip: Edit
  chip: Image Search
  button: Back
  button: Clear All Labels
  button: Help
```

Choosing a part hands it to Select Part and leaves the strip on the guided page.

```screen dunnage-settings-hub Cards that name the page they open
title: Non-Inventoried Labels Settings
list: Personal Defaults | Image Assets | Image Presentation | Workflow Visuals | Keyboard Shortcuts | Label Files
```

Each card carries the exact step title it opens, so adding or reordering a card cannot move another
card's destination.

```screen receiving-settings-hub The repaired Receiving hub
title: Receiving Settings
list: Entry Defaults | Validation Rules | Part Formatting | Workflow Defaults | Keyboard Shortcuts | Custom Vendor Variables | Welded Part Numbers
```

## Constitution Check

| Principle | Assessment |
| --- | --- |
| I. MVVM and View Purity | PASS. The navigation verbs move out of `View_Dunnage_WorkflowView.xaml.cs` and into the view model, which is the direction this principle asks for. The bar binds `x:Bind` to the shell projection and every remaining code-behind member is wiring. |
| II. Data Access Integrity | PASS. No new DAO and no new SQL. The only new write is the stored default mode, which already goes through `IService_UserPreferences.UpdateDefaultDunnageModeAsync` and its existing procedure. Infor Visual is untouched. |
| III. CQRS and Mediator First | VIOLATION, justified. This is presentation wiring over commands and services the module already has; FR-020 forbids re-implementing those actions, so routing them through MediatR handlers would create a second path to the same behaviour. Recorded below. |
| IV. Dependency Injection and Modular Boundaries | PASS. Dead Dunnage registrations are removed and the Label Files page is registered in `Infrastructure/DependencyInjection`. `Module_Core` gains one property on a presentation model; no module-specific type moves into it. |
| V. Validation, Errors, and Structured Logging | PASS. The existing validation gates (`IsPartSelected`, `IsValid`, `CanProceedToNextStep`) and `IService_ErrorHandler` calls are reused unchanged behind the strip's slots. |
| VI. Security and Session Discipline | PASS, and one gap closes. The search path gains the scanner allow-list check that navigation already performs, so a restricted page cannot be opened by searching for it. Default-mode writes stay scoped to the signed-in user. |
| VII. Library-First Reuse | PASS. No new package. The strip reuses CommunityToolkit commands, the existing help service, and the controls already on the bar. |

## Complexity Tracking

| Violation | Why needed | Simpler alternative rejected |
| --- | --- | --- |
| No new MediatR command or handler for the moved verbs | The strip re-invokes commands that already exist on the Dunnage view models; FR-020 requires exactly that. A handler per verb would duplicate workflow orchestration that `IService_DunnageWorkflow` already owns. | Routing Back, Next, Review, and Save through MediatR would rewrite working workflow orchestration far outside this feature's scope. |

## Constraints

- Flow stays View -> ViewModel -> Service -> DAO. The shell projection holds no domain logic; it forwards
  to whatever provider is attached.
- `x:Bind` only, including the new mode chip and its default marker.
- No business logic in `MainWindow.xaml.cs`. Rendering a fourth chip, mapping a `Tag` to a stored mode
  string, and opening a route are view wiring.
- `MainWindow` must not name a `Module_Dunnage` type for the strip's actions. The existing
  `ContentFrame.Content is View_Dunnage_WorkflowView` checks in the leave-confirmation methods are
  pre-existing and must not grow.
- MySQL writes stay stored-procedure only. Infor Visual stays read-only.
- Every user-visible string the strip shows for Non-Inventoried Labels comes from `DunnageSettingsKeys`
  through `IService_DunnageSettings`, with defaults holding the verbatim copy: `Guided`, `Edit`,
  `Image Search`, `Clear All Labels`, `Default`, `Review`, `Save`.
- Deleting manual mode means deleting it, not hiding it: the enum members, the two view models, the two
  views, the shortcut key, the help topics, and the registration entries all go.

## See Also

- [spec.md](spec.md)
- [research.md](research.md)
- [data-model.md](data-model.md)
- [contracts/per-page-action-matrix.md](contracts/per-page-action-matrix.md)
- [contracts/shell-action-provider.md](contracts/shell-action-provider.md)
- [contracts/search-destination-catalog.md](contracts/search-destination-catalog.md)
