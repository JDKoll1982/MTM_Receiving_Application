# Tasks: Persistent Shell Action Bar

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md) | **Data model**: [data-model.md](data-model.md)

**Scale note**: roughly thirty files across four areas: the shell (`MainWindow`), a new core contract with
its immutable snapshot and a shell projection in `Module_Shared`, the whole Receiving workflow view and
view-model cluster, and the settings and help surfaces that still name the deleted picker. Watch the Phase 3
boundary: the per-page matrix lives in one Receiving view model, so it is decided once there and every later
story proves it rather than re-deciding it.

Every task names the file it creates or edits. A path appears in exactly one phase; if a later phase's test
fails, reopen the task that owns the file instead of adding a second copy of the decision.

## Phase 1: Setup

New shared types and the settings text surface. Nothing here changes behaviour, so every task can land
before any story starts.

**Wave 1 — independent (different files):**

- [x] **T001** [P] Create the `Model_ShellActionSlot` record (visible, enabled, label, tooltip, accessibility
  name, command, divider-after) · `Module_Core/Models/UI/Model_ShellActionSlot.cs`
- [x] **T002** [P] Create the `Model_ShellModeAction` record (mode, label, tooltip, accessibility name,
  command, show-default-checkbox, default tooltip, divider-after) · `Module_Core/Models/UI/Model_ShellModeAction.cs`
- [x] **T003** [P] Add the seven `Receiving.UiText.Shell` constants: four mode labels and three default
  tooltips (FR-025) · `Module_Receiving/Settings/ReceivingSettingsKeys.cs`
- [x] **T004** [P] Add the seven string defaults with the verbatim copy `Guided`, `Manual`, `Edit`,
  `Overlay`, `make Guided my default`, `make Manual my default`, `make Edit my default` (FR-025) ·
  `Module_Receiving/Settings/ReceivingSettingsDefaults.cs`
- [x] **T005** [P] Add the seven matching manifest rows, and remove every Receiving row whose display name or
  default reads `Mode Selection` so the string no longer appears in settings (FR-013) ·
  `Module_Settings.Core/Defaults/settings.manifest.json`

**⟶ Wait for Wave 1 to finish, then:**

**Wave 2 — the snapshot:**

- [x] **T006** Create the `Model_ShellActionBarState` snapshot that holds both new records plus visible flags
  and the three default flags · `Module_Core/Models/UI/Model_ShellActionBarState.cs`

**⟶ Wait for Wave 2 to finish, then:**

**Wave 3 — the contract:**

- [x] **T007** Add `IViewModel_ShellActionProvider` with `ShellActions` and `SetDefaultMode(string?)`
  (FR-017) · `Module_Core/Contracts/ViewModels/IViewModel_ShellActionProvider.cs`

## Phase 2: Foundational

The shell mechanism: one projection the bar binds to, and the resolve, attach, and detach wiring beside the
existing header sync. No Receiving work begins until this phase is done.

Files: `Module_Shared/ViewModels/ViewModel_Shared_ShellActionBar.cs`, `MainWindow.xaml`,
`MainWindow.xaml.cs`, `Infrastructure/DependencyInjection/CoreServiceExtensions.cs`.

**Wave 1 — the projection:**

- [x] **T008** Create `ViewModel_Shared_ShellActionBar`: mirror every slot and mode action, hold the three
  settable default flags whose setters call the provider's `SetDefaultMode`, and make `Attach` subscribe and
  project immediately while `Detach` collapses and clears · `Module_Shared/ViewModels/ViewModel_Shared_ShellActionBar.cs`

**⟶ Wait for Wave 1 to finish, then:**

**Wave 2 — independent (different files):**

- [x] **T009** [P] Register the projection in the composition root (never in `App.xaml.cs`, FR-017) ·
  `Infrastructure/DependencyInjection/CoreServiceExtensions.cs`
- [x] **T010** [P] Wrap `ContentFrame` in a two-row grid, put the strip in the auto-height row beneath the
  content on the right, give it a horizontal scroller so compact widths never hide an action (FR-001,
  FR-003, FR-004), and bind every control with `x:Bind` · `MainWindow.xaml`
- [x] **T011** [P] Add `ResolveShellActionProvider` beside the two header resolvers, attach and detach it in
  `SyncPageHeader` and `ClearHeaderSubscription`, and force `HeaderBackButton` collapsed while leaving
  `IService_HeaderBackNavigation` wired (FR-017, FR-021, FR-027) · `MainWindow.xaml.cs`

## Phase 3: User Story 1 (Priority P1) — every page keeps its navigation in one known place

The strip's Receiving-side matrix, and the guided walk page that stops drawing its own navigation row.

Files: `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs`,
`Module_Receiving/Views/View_Receiving_Workflow.xaml`, `Module_Receiving/Views/View_Receiving_Workflow.xaml.cs`.

### Tests

`MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_ShellActionTests.cs`.
Drive `Service_ReceivingWorkflow` step changes with a mocked service, the same shape
`ViewModel_Receiving_WorkflowTests` already uses, and read the provider's snapshot. Write these first and run
them red.

- [x] **T012** [US1] `ShellActions_ShouldOfferBackNextClearLabelDataAndHelpInOrder_WhenStepIsPartSelection`
  (FR-001, FR-008, FR-012)
- [x] **T013** [US1] `ShellActions_ShouldOmitBack_WhenStepIsEnterPoNumber` and assert it is absent, not
  disabled (FR-009)
- [x] **T014** [US1] `ShellActions_ShouldDisableNext_WhenThePoPartListHasNotLoaded` and
  `..._WhenNoPartIsChosenOnSelectPart` (FR-009)
- [x] **T015** [US1] `ShellActions_ShouldDisableClearLabelData_WhenTheUserHasNoActiveLabelData` (FR-010)
- [x] **T016** [US1] `ShellActions_ShouldOfferClearLabelData_WhenStepIsManualEntryOrEditMode` (FR-010)
- [x] **T017** [US1] `ShellActions_ShouldCarryTheExistingShowHelpCommand_WhenHelpIsInvoked` (FR-011, FR-024)
- [x] **T018** [US1] `ShellActions_ShouldOfferGuidedManualEditOverlayInThatOrder_WhenThePageIsReceiving`
  (FR-005, FR-015)
- [x] **T019** [US1] `NextSlot_ShouldMatchNextStepCommandCanExecute_WhenThePoPartListHasNotLoaded` so the
  `Ctrl+Right` gate and the button cannot disagree (FR-022)
- [x] **T020** [US1] `ShellActions_ShouldOfferNothing_WhenStepIsSaving` (FR-012)

### Implementation

**Wave 1 — the matrix:**

- [x] **T021** [US1] Implement `IViewModel_ShellActionProvider` on the workflow view model: build the whole
  per-page matrix (FR-012), expose the four mode actions that run the entry paths the deleted picker owned
  (FR-005, FR-015), persist the default through `IService_UserPreferences` (FR-006, FR-007), accept the
  hosted Manual Entry, Edit Mode and Review view models so their verbs can occupy the Back and Next slots
  (FR-018, FR-019, FR-020), raise `PropertyChanged` for `ShellActions` on every step change, refresh Clear
  Label Data availability on Manual Entry and Edit Mode (FR-010), and delete `ReturnToModeSelectionCommand`,
  `IsModeSelectionVisible`, the mode-selection title text and the mode-selection switch cases from this file
  · `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs`

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T022** [P] [US1] Delete `ReceivingWorkflowNavigationGrid`, `ModeSelectionHost`, and the completion
  panel's Start New Entry and Mode Selection buttons, leaving the receipt header card and step content
  otherwise untouched (FR-016, FR-019, FR-026) · `Module_Receiving/Views/View_Receiving_Workflow.xaml`
- [x] **T023** [P] [US1] Drop the mode-selection view parameter and its host assignment, hand the Manual
  Entry, Edit Mode and Review view models to the provider, delete the mode-selection shortcut handler, repoint
  the help shortcut at the same command the strip uses, and keep `Ctrl+Left` and `Ctrl+Right` gated on the
  members the snapshot reads (FR-022, FR-027) · `Module_Receiving/Views/View_Receiving_Workflow.xaml.cs`

**Checkpoint**: walking the guided sequence shows Back, Next, Clear Label Data and Help in one strip, in the
same order, on every step, and the page draws no navigation row of its own.

## Phase 4: User Story 2 (Priority P1) — Receiving opens straight into a mode

Remove the picker and let startup resolve the stored default. This phase runs immediately after Phase 3
because until the startup change lands, a user with no stored default lands on a step nothing hosts any more.

Files: `Module_Receiving/Services/Service_ReceivingWorkflow.cs`,
`Module_Core/Models/Enums/Enum_ReceivingWorkflowStep.cs`,
`Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs`,
`Module_Receiving/Views/View_Receiving_ModeSelection.xaml`,
`Module_Receiving/Views/View_Receiving_ModeSelection.xaml.cs`,
`Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`, `Module_Core/Services/Help/Service_Help.cs`,
`Module_Core/Helpers/UI/Helper_WorkflowHelpContentGenerator.cs`,
`Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_BusinessRules.cs`,
`Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_WorkflowDefaults.cs`,
`Module_Settings.Receiving/Models/Model_Settings_ReceivingShortcuts.cs`,
`Module_Receiving/Services/Service_ReceivingShortcuts.cs`,
`Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_KeyboardShortcuts.cs`,
`Module_Settings.Receiving/Views/View_Settings_Receiving_KeyboardShortcuts.xaml`.

### Tests

`MTM_Receiving_Application.Tests/Unit/Module_Receiving/Services/Service_ReceivingWorkflowStartModeTests.cs`.
Write these first and run them red.

- [x] **T024** [US2] `StartWorkflowAsync_ShouldOpenManualEntry_WhenTheStoredDefaultIsManual` (FR-013)
- [x] **T025** [US2] `StartWorkflowAsync_ShouldOpenEditModeAndUseCurrentLabels_WhenTheStoredDefaultIsEdit`
  (FR-013)
- [x] **T026** [US2] `StartWorkflowAsync_ShouldOpenPoEntryAndStoreGuided_WhenNoDefaultIsStored`, asserting the
  write reaches `IService_UserPreferences` (FR-014)
- [x] **T027** [US2] `StartWorkflowAsync_ShouldOpenPoEntryAndStoreGuided_WhenTheStoredValueIsUnrecognised`,
  covering the restart on a value that no longer names a page (FR-014)

### Implementation

**Wave 1 — independent removals and the startup rewrite (different files):**

- [x] **T028** [P] [US2] Delete the picker page, its view model and its tests ·
  `Module_Receiving/Views/View_Receiving_ModeSelection.xaml`, `Module_Receiving/Views/View_Receiving_ModeSelection.xaml.cs`,
  `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs`,
  `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelectionTests.cs`
- [x] **T029** [P] [US2] Drop the two mode-selection registrations ·
  `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
- [x] **T030** [P] [US2] Remove the `Receiving.ModeSelection` help topic, its step mappings, its related keys,
  and the two mode-selection tooltips so help never offers the deleted step (FR-013) ·
  `Module_Core/Services/Help/Service_Help.cs`
- [x] **T031** [P] [US2] Remove the `ModeSelection` case and `CreateModeSelectionHelp` ·
  `Module_Core/Helpers/UI/Helper_WorkflowHelpContentGenerator.cs`
- [x] **T032** [P] [US2] Drop the `Mode Selection` option and normalise a stored `ModeSelection` value ·
  `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_BusinessRules.cs`
- [x] **T033** [P] [US2] Drop the `Mode Selection` option and normalise a stored `ModeSelection` value ·
  `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_WorkflowDefaults.cs`
- [x] **T034** [P] [US2] Remove the mode-selection shortcut from the model, the service, the settings view
  model, the settings view and the service test (R9) ·
  `Module_Settings.Receiving/Models/Model_Settings_ReceivingShortcuts.cs`,
  `Module_Receiving/Services/Service_ReceivingShortcuts.cs`,
  `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_KeyboardShortcuts.cs`,
  `Module_Settings.Receiving/Views/View_Settings_Receiving_KeyboardShortcuts.xaml`,
  `MTM_Receiving_Application.Tests/Unit/Module_Receiving/Services/Service_ReceivingShortcutsTests.cs`
- [x] **T035** [P] [US2] Resolve the opening mode from the session user's stored preference, fall back to
  Guided and write Guided back through `IService_UserPreferences` when the value is missing or unrecognised,
  stop consulting `DefaultModeOnStartup` for routing, and remove every remaining mode-selection destination
  from advance, back and reset (FR-013, FR-014) · `Module_Receiving/Services/Service_ReceivingWorkflow.cs`

**⟶ Wait for Wave 1 to finish, then:**

**Wave 2 — the enum:**

- [x] **T036** [US2] Delete the `ModeSelection` enum member now that no file names it ·
  `Module_Core/Models/Enums/Enum_ReceivingWorkflowStep.cs`

**Checkpoint**: opening Receiving with no stored default starts Guided and stores it; a stored Manual or Edit
default starts directly; no mode-selection page, shortcut, help topic, or settings option remains.

## Phase 5: User Story 3 (Priority P2) — deciding which mode is my default, from the strip

Files: `Module_Receiving/Views/View_Receiving_LabelOverlayWindow.cs`.

### Tests

`MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_ShellDefaultModeTests.cs`.
The provider from Phase 3 owns these decisions; these tests prove them through `SetDefaultMode` and the
snapshot, verifying the write against a mocked `IService_UserPreferences`.

- [x] **T037** [US3] `SetDefaultMode_ShouldStoreManualOnly_WhenManualIsTicked`, and assert the active step did
  not change (FR-006)
- [x] **T038** [US3] `ShellActions_ShouldTickEditAlone_WhenManualWasTheStoredDefaultAndEditIsTicked` (FR-007)
- [x] **T039** [US3] `SetDefaultMode_ShouldStoreNothing_WhenTheLastTickedDefaultIsCleared` (FR-007)
- [x] **T040** [US3] `SetDefaultMode_ShouldWriteForTheSignedInUserOnly_WhenAPreferenceIsPersisted` (FR-007)
- [x] **T041** [US3] `ShellActions_ShouldLeaveTheStoredDefaultAlone_WhenTheOverlayCommandRuns` (US3-4)

### Implementation

**Wave 1 — the one surface this story still owns:**

- [x] **T042** [US3] Guard `ShowModalAsync` so pressing Overlay while the window is already open does not open
  a second one (Edge Cases, US3-4) · `Module_Receiving/Views/View_Receiving_LabelOverlayWindow.cs`

**Checkpoint**: ticking a default survives a restart, only one default can be stored, unticking the last one
falls back to Guided on the next open, and Overlay opens one window and changes no default.

## Phase 6: User Story 4 (Priority P2) — the primary action for a page lands in the expected slot

The strip already offers the verbs; this phase removes the buttons the pages still draw, so no control exists
twice.

Files: `Module_Receiving/Views/View_Receiving_ManualEntry.xaml`,
`Module_Receiving/Views/View_Receiving_EditMode.xaml`, `Module_Receiving/Views/View_Receiving_Review.xaml`,
`Module_Receiving/Views/View_Receiving_Review.xaml.cs`, `Module_Receiving/ViewModels/ViewModel_Receiving_Review.cs`.

### Tests

`MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_ShellPrimaryActionTests.cs`.

- [x] **T043** [US4] `ShellActions_ShouldPutSaveAndFinishInTheNextSlot_WhenStepIsManualEntry` (FR-020)
- [x] **T044** [US4] `ShellActions_ShouldPutSaveAndFinishInTheNextSlotAndOmitBack_WhenStepIsEditMode`
  (FR-020)
- [x] **T045** [US4] `ShellActions_ShouldPutSaveToDatabaseInTheNextSlotAndOfferNoAddAnother_WhenStepIsReview`
  (FR-018)
- [x] **T046** [US4] `ShellActions_ShouldPutStartNewEntryInTheBackSlotAndKeepOverlay_WhenStepIsComplete`
  (FR-019)
- [x] **T047** [US4] `ShellActions_ShouldOfferNoRowToolbarActions_WhenStepIsManualEntryOrEditMode`, proving
  the editing toolbars stay on the page (FR-012, FR-023)

### Implementation

**Wave 1 — independent pages (different files):**

- [x] **T048** [P] [US4] Delete the page's own Save and Finish panel, leaving the grid intact (FR-020) ·
  `Module_Receiving/Views/View_Receiving_ManualEntry.xaml`
- [x] **T049** [P] [US4] Delete the Save button from the pager row and keep its editing, file, paging and
  filter controls (FR-020, FR-023) · `Module_Receiving/Views/View_Receiving_EditMode.xaml`
- [x] **T050** [P] [US4] Delete the Save to Database button and the Add another Part/PO button (FR-018) ·
  `Module_Receiving/Views/View_Receiving_Review.xaml`

**⟶ Wait for Wave 1 to finish, then:**

**Wave 2 — independent (different files):**

- [x] **T051** [P] [US4] Repoint `FocusForAccess` at a control that still exists now that `SaveToDatabaseButton`
  is gone (FR-018) · `Module_Receiving/Views/View_Receiving_Review.xaml.cs`
- [x] **T052** [P] [US4] Delete `AddAnotherPartCommand`, its text and its accessibility name so no Add another
  Part/PO action survives anywhere (FR-018) · `Module_Receiving/ViewModels/ViewModel_Receiving_Review.cs`

**Checkpoint**: Manual Entry, Edit Mode, Review and Save, and Result each show their verb in the Next or Back
slot, and none of them draws that verb itself.

## Phase 7: User Story 5 (Priority P3) — other modules are left alone

Files: none. The shell resolves no provider for a page that supplies none, so the strip and the mode group are
already absent; this phase proves it.

### Tests

`MTM_Receiving_Application.Tests/Unit/Module_Shared/ViewModels/ViewModel_Shared_ShellActionBarTests.cs`.

- [x] **T053** [US5] `Detach_ShouldCollapseTheBarAndClearEverySlot_WhenTheActivePageSuppliesNoProvider`
  (FR-003, FR-028)
- [x] **T054** [US5] `Attach_ShouldHideTheModeGroup_WhenTheSnapshotReportsNoReceivingModes` (FR-005, FR-028)
- [x] **T055** [US5] `Attach_ShouldReportCollapsed_WhenTheProviderOffersNoActions`, so the label-shortcut
  footer keeps its own pane and never shares space with the strip (FR-002)

### Implementation

No production file changes. If a test here fails, reopen T008 rather than fixing it here.

**Checkpoint**: one page from each non-Receiving module shows no mode control, and a page offering nothing
shows no strip and full-height content.

## Phase 8: Polish

- [x] **T056** Record the shell action bar and the deleted mode-selection page in the change log ·
  `CHANGELOG.md`
- [x] **T057** Validate against Success Criteria: `dotnet build MTM_Receiving_Application.slnx` and
  `dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj` must both pass
  (SC-006)
- [x] **T058** Spot check one page from each of Reconciliation, Dunnage, Reporting, Settings and Volvo, and
  confirm zero mode controls and no strip on pages that offer no actions (SC-001, SC-002, SC-007)

## Dependencies and Execution Order

- Phase 1 blocks everything: the records and the contract are what the projection and the provider compile
  against.
- Phase 2 blocks every story: the strip must exist before any page can report into it. Its waves are
  T008, then T009, T010 and T011 in parallel.
- Phase 3 (US1) owns the receiving workflow view model and its page: T021 first, then T022 and T023 in
  parallel.
- Phase 4 (US2) must run immediately after Phase 3. Its wave 1 (T028 to T035) removes every remaining
  mode-selection reference, and T036 removes the enum member only once wave 1 is done.
- Phase 5 (US3), Phase 6 (US4) and Phase 7 (US5) touch disjoint files and may run in any order after
  Phase 3. Within Phase 6, T048, T049 and T050 run in parallel, then T051 and T052 in parallel.
- Phase 8 closes the spec and runs the suites once, because this project has no post-implement validation
  hook.

### Parallel Opportunities

- Phase 1 wave 1: T001 to T005, five files.
- Phase 2 wave 2: T009, T010 and T011, three files.
- Phase 4 wave 1: T028 to T035, seven independent edits across the module, the help surface and settings.
- Phase 6 wave 1: T048 to T050, three views.

## Phase 9: Convergence

- [x] **T059** [Edge Cases] Make each of the four mode controls a no-op when the mode it enters is already the active mode, so pressing Guided on a guided step or Manual/Edit while that mode is open neither resets the workflow nor clears label data already collected per spec.md Edge Cases (contradicts)
- [x] **T060** [Edge Cases] Gate the Clear Label Data and Help keyboard shortcuts on the same snapshot state the strip renders, so neither shortcut can run an action the strip has hidden while the Saving step collapses it per spec.md Edge Cases (partial)

## Phase 10: User Story 6 (Priority P2) — the strip says what each action does

Files: `MainWindow.xaml` and `MainWindow.xaml.cs` (owned by Phase 2), and
`Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs` (owned by Phase 3). Both phases are closed,
so ownership of those three files passes to this phase. New files this phase owns:
`Module_Core/Models/Enums/Enum_ShellActionKind.cs`, `Module_Core/Helpers/UI/Helper_ShellBrush.cs`.

### Wave 1 — the vocabulary the strip draws from

- [x] **T061** [US6] Give each action position a kind so the strip can tell a way out from a primary
  action, and mark which mode is in force (FR-029, FR-031) ·
  `Module_Core/Models/Enums/Enum_ShellActionKind.cs`, `Module_Core/Models/UI/Model_ShellActionSlot.cs`,
  `Module_Core/Models/UI/Model_ShellModeAction.cs`
- [x] **T062** [US6] Declare success and danger brushes for both light and dark, so neither colour is set per
  control (FR-029) · `Module_Core/Themes/ModuleAccentBrushes.xaml`
- [x] **T063** [US6] Resolve the brushes from XAML rather than from a view model, because a brush can only be
  created on the UI thread and a view-model property throws off it (FR-029, FR-031) ·
  `Module_Core/Helpers/UI/Helper_ShellBrush.cs`

### Wave 2 — depends on Wave 1

- [x] **T064** [US6] Draw Back and the save verbs in their colours, and drop the visible text from Clear Label
  Data and Help while keeping their tooltips and automation names (FR-029, FR-030) · `MainWindow.xaml`
- [x] **T065** [US6] Show the Back-Next divider only when both are present, and remove every trailing divider
  so no divider is left with nothing to its right (FR-032) · `MainWindow.xaml`,
  `Module_Shared/ViewModels/ViewModel_Shared_ShellActionBar.cs`
- [x] **T066** [US6] Mark the mode in force on every step, keeping the arrival mode marked on Review and Save
  and on Result (FR-031) · `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs`
- [x] **T067** [US6] Store the default from Ctrl+click on a mode button and show the Default badge under that
  button, keeping all four buttons on one row (FR-033) · `MainWindow.xaml.cs`, `MainWindow.xaml`

**Checkpoint**: on one guided step, one mode page and the Result page, every action is identifiable by
colour or icon alone in both themes, no divider stands with nothing to its right, and the mode in force is
marked.

## Phase 11: User Story 7 (Priority P3) — settings opens the page it names

Files: `Module_Settings.Receiving/Views/View_Settings_Receiving_CategoryHub.xaml` and its code-behind, and
`Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_CategoryHub.cs`. No earlier phase owns
them.

### Wave 1 — the hub repair

- [x] **T068** [US7] Resolve a hub card by the step title it names instead of by its position, so a change to
  the list cannot send a card to the wrong page (FR-034) ·
  `Module_Settings.Receiving/Views/View_Settings_Receiving_CategoryHub.xaml`, `...xaml.cs`
- [x] **T069** [US7] Remove the card that had no settings page behind it (FR-035) ·
  `Module_Settings.Receiving/Views/View_Settings_Receiving_CategoryHub.xaml`

**Checkpoint**: every card on the hub opens the page named on it, and no card offers a page that does not
exist.

## Phase 11B: User Story 8 (Priority P3) — the welded part list sits with the other Receiving settings

Files: the welded part model, data access, contract, service, view model and view under
`Module_Settings.Receiving/`. No earlier phase owns them. `Infrastructure/DependencyInjection/
ModuleServicesExtensions.cs` was owned by Phase 1; that phase is closed, so the registration task here
edits it rather than opening a second owner.

### Wave 1 — the list itself, independent of Phase 11

- [x] **T070** [US8] Add the welded part model and the data access that calls the existing stored procedures
  (FR-037) · `Module_Settings.Receiving/Models/Model_Settings_ReceivingWeldedPart.cs`,
  `Module_Settings.Receiving/Data/Dao_Settings_ReceivingWeldedParts.cs`
- [x] **T071** [US8] Add the service that normalises the part number, enforces the 11-character limit, and
  confirms the part in the read-only part master (FR-038, FR-040) ·
  `Module_Settings.Receiving/Contracts/IService_Settings_ReceivingWeldedParts.cs`,
  `Module_Settings.Receiving/Services/Service_Settings_ReceivingWeldedParts.cs`
- [x] **T072** [US8] Add the page and its view model, add and remove only, with search and an empty state
  (FR-039, FR-040) · `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_WeldedParts.cs`,
  `Module_Settings.Receiving/Views/View_Settings_Receiving_WeldedParts.xaml`, `...xaml.cs`

### Wave 2 — joins

- [x] **T073** [US8] Register the data access, the service, the view model and the view in the settings module ·
  `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
- [x] **T079** [US8] Add the welded part numbers card to the hub so the new page is reachable from Receiving
  settings (FR-036) · `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_CategoryHub.cs`,
  `Module_Settings.Receiving/Views/View_Settings_Receiving_CategoryHub.xaml`
- [x] **T074** [US8] Cover the normalise, length, duplicate and part-master rules with unit tests (FR-038,
  FR-040) · `MTM_Receiving_Application.Tests/Unit/Module_Settings/Receiving/Services/Service_Settings_ReceivingWeldedPartsTests.cs`

**Checkpoint**: a part added on Welded Part Numbers shows up in Extra Tools, Welded Coils, and a removed row
is gone from both.

## Phase 12: Polish for the appended work

- [x] **T075** [US1] Shorten the scanner's closed-window message so it fits beside the Send button ·
  `Module_Scanner/ViewModels/ViewModel_Scanner_Workbench.cs`,
  `MTM_Receiving_Application.Tests/Unit/Module_Scanner/ViewModels/ViewModel_Scanner_WorkbenchTests.cs`
- [x] **T076** [US7] [US8] Record the welded part numbers page and the settings hub repair in the change log ·
  `CHANGELOG.md`
- [x] **T077** [US6] Add the Spec Kit and Companion field guide as a source document with a regenerated print
  edition · `SpecKit_Companion_Verified_Field_Guide.md`, `SpecKit_Companion_Verified_Field_Guide.pdf`
- [x] **T078** [US6] Re-run the build and the full suite once for everything appended here (SC-006)

## Appended Work Notes

- Phases 10 to 12 were appended after Phase 9 to record work that was already in the branch. Every task is
  checked because the code exists and the suite passed, not because the work was dispatched from this list.
- Phases 10, 11 and 11B touch files that Phases 1, 2 and 3 already owned. Those phases are closed, so the
  new phase is the single owner; nothing runs in parallel with them.
- Phase 11 is US7 (the hub card repair). Phase 11B is US8 (the welded part list), kept separate because the
  two ship independently: a card can be fixed without the new page, and the page can exist with the cards
  still resolving by position.
- Phase 12 covers two items that are not Receiving features: a scanner message and a toolkit document. They
  are recorded here so the branch and the spec agree, not because they belong to the strip.