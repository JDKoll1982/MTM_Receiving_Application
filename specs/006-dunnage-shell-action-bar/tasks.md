# Tasks: Shell Action Bar for Non-Inventoried Labels

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md) | **Data model**: [data-model.md](data-model.md) |
**Contracts**: [per-page matrix](contracts/per-page-action-matrix.md),
[provider](contracts/shell-action-provider.md), [search catalog](contracts/search-destination-catalog.md)

**Scale note**: roughly forty files across five modules, in two centres of gravity. The first is the
shared strip: one more mode slot in core, one more projection property, one more chip in `MainWindow`,
and a Dunnage view model that becomes the page's action provider. The second is deletion: the picker,
manual entry, their step, their shortcut, their help topics and their registrations all go. Read the
phase order before touching anything. Phase 3 (US1) owns the whole workflow page and is written so it
never names a step or a view that Phase 5 deletes, which is why those phases can stay out of each other's
files. The settings-hub repair and the search audit sit outside both and share no decision with them.

Every task names the file it creates or edits. A path appears in exactly one phase **as an edit**. Two
paths appear twice for the one other reason: a `Validate` task names the file it reads.
`MainWindow.xaml` is edited in Phase 2 and read by Phase 3's two markup checks, and `MainWindow.Search.cs`
is created by a move in Phase 2 and owned by Phase 10, so the search audit and the mode routing never
edit `MainWindow.xaml.cs` at the same time.

**How the Tests blocks read.** Each story's `### Tests` block holds one task per requirement the story
covers. Several tasks in a block share a test file, so they are an ordered list rather than parallel
waves: consecutive tasks may write the same file, and no two tasks are ever run at once. The
`### Implementation` blocks are real waves, with `[P]` on everything that touches a different file and
join lines where work must wait.

Phases 1 to 5 are the P1 chain: the copy surface, the strip's fourth slot, the provider that fills it, the
startup decision, and the deletion that follows. Phases 6 to 10 are the P2 and P3 stories, and only
Phases 3 and 7 touch code the strip renders.

## Phase 1: Setup

The copy surface. Nothing here changes behaviour, so all of it lands before any story starts. This phase
also retires the mode-selection shortcut key, because that key's constant, its default and its manifest
row are the three places the shortcut is declared, and they are this phase's three files.

**Wave 1 — independent (different files):**

- [x] **T001** [P] Add the `UiText` group to `DunnageSettingsKeys`: `Workflow.Back`, `Workflow.Next`, `Workflow.Help`, `Workflow.ClearAllLabels`, `Workflow.Review`, `Workflow.Save`, `Shell.Guided`, `Shell.Edit`, `Shell.ImageSearch`, `Shell.Default`, and one `Shell.*DefaultTooltip` per mode; delete `Shortcuts.ModeSelection` (FR-022, FR-011) · `Module_Dunnage/Settings/DunnageSettingsKeys.cs`
- [x] **T002** [P] Add one editable `String` row per new key, each carrying the verbatim default, and delete the `Dunnage.Shortcuts.ModeSelection` row so no settings surface names the deleted screen (FR-022, FR-011) · `Module_Settings.Core/Defaults/settings.manifest.json`

**⟶ Wait for Wave 1 to finish, then:**

**Wave 2 — the defaults, which must name the keys Wave 1 added:**

- [x] **T003** Add the matching `StringDefaults` entries: `Guided`, `Edit`, `Image Search`, `Default`, `Back`, `Next`, `Help`, `Clear All Labels`, `Review`, `Save`, and the three `make <mode> my default` tooltips. `Workflow.Save` is exactly `Save`, not the `Save All` and `Save Changes` it replaces (FR-022, Verbatim Constraints) · `Module_Dunnage/Settings/DunnageSettingsDefaults.cs`

## Phase 2: Foundational

The strip learns a fourth mode slot and learns to hide a chip its module does not own. Both are needed
before any Dunnage page can report into the bar, so no story starts until this phase is done.

Files: `Module_Core/Models/UI/Model_ShellActionBarState.cs`, `Module_Core/Models/UI/Model_ShellModeAction.cs`,
`Module_Shared/ViewModels/ViewModel_Shared_ShellActionBar.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`.

**Wave 1 — independent (different files):**

- [x] **T004** [P] Add the `ImageSearch` mode slot and the `IsImageSearchDefault` flag to the snapshot, defaulting to `Hidden` and `false` so Receiving's snapshot stays equivalent (FR-004, FR-005) · `Module_Core/Models/UI/Model_ShellActionBarState.cs`
- [x] **T005** [P] Add `IsVisible` to `Model_ShellModeAction`, defaulting true, and set it false on `Hidden`, because the chip blocks currently render an empty button for a mode their module does not own (FR-005, FR-017) · `Module_Core/Models/UI/Model_ShellModeAction.cs`

**⟶ Wait for Wave 1 to finish, then:**

**Wave 2 — the projection:**

- [x] **T006** Project `ImageSearch`, `IsImageSearchDefault` and each chip's `IsVisible`; clear them all in `Detach`; and move the mode lookup here as `EnterMode(string)` and `RememberModeAsDefault(string)`, which find the chip whose `Mode` matches and run its command or tick its default flag, so the window stops re-listing a module's modes (FR-005, FR-006, FR-007, FR-026) · `Module_Shared/ViewModels/ViewModel_Shared_ShellActionBar.cs`

**⟶ Wait for Wave 2 to finish, then:**

**Wave 3 — independent (different files):**

- [x] **T007** [P] Add the Image Search chip block between Edit and Overlay, with the two extra grid columns, the `Tag="image-search"` click, the default badge, and a visibility binding on all five chips so an unowned mode collapses instead of drawing an empty button (FR-004, FR-005, FR-017, FR-025) · `MainWindow.xaml`
- [x] **T008** [P] Replace the two per-mode switches with a call to the projection's `EnterMode` and `RememberModeAsDefault`, deleting the mode list the window maintained beside it, and move the navigation and search open path into a new `MainWindow.Search.cs` partial **unchanged**, so Phase 10 has a file of its own. Move, do not rewrite: the build and the existing suite must pass untouched after this task (FR-006, FR-026) · `MainWindow.xaml.cs`

## Phase 3: User Story 1 (Priority P1) — every Non-Inventoried Labels page keeps its navigation in one known place

The whole per-page matrix, the page that stops drawing its own navigation row, and the view-model
provider the shell discovers.

Files: `Module_Dunnage/ViewModels/ViewModel_Dunnage_WorkFlowViewModel.ShellActions.cs` (new),
`Module_Dunnage/Views/View_Dunnage_WorkflowView.xaml`,
`Module_Dunnage/Views/View_Dunnage_WorkflowView.xaml.cs`.

This phase writes both view files so they never name `Enum_DunnageWorkflowStep.ModeSelection`,
`Enum_DunnageWorkflowStep.ManualEntry`, `IsModeSelectionVisible`, `IsManualEntryVisible`, or the manual
entry view. Phase 5 deletes those, and expects to find nobody left naming them here.

### Tests

`MTM_Receiving_Application.Tests/Unit/Module_Dunnage/ViewModels/ViewModel_Dunnage_ShellActionTests.cs` is
where most of these land; the tasks that name another file say so. Drive the mocked workflow service's
`CurrentStep`, hand in mocked child view models, and read the provider's snapshot. Write these first and
run them red.

- [x] **T009** [US1] `ShellActions_ShouldBeVisibleWithItsActions_OnEveryOneOfTheSevenPages`, walking the steps and asserting each snapshot offers what the matrix gives it (FR-001)
- [x] **T010** [US1] `Attach_ShouldReportCollapsed_WhenTheProviderOffersNoActions`, so a page offering nothing takes the full remaining height (FR-002) · `MTM_Receiving_Application.Tests/Unit/Module_Shared/ViewModels/ViewModel_Shared_ShellActionBarTests.cs`
- [x] **T011** [US1] Validate FR-003: the strip still sits in the auto-height row inside its horizontal scroller, so at the smallest supported width every action of the current page stays reachable by wrapping or scrolling. No code change; read the host and scroller and narrow the window (FR-003) · `MainWindow.xaml`
- [x] **T012** [US1] `ShellActions_ShouldOfferExactlyGuidedEditAndImageSearchInThatOrder_WhenThePageIsNonInventoriedLabels`, and offer nothing else in the mode group (FR-004)
- [x] **T013** [US1] `ShellActions_ShouldLeaveManualAndOverlayHidden_WhenThePageIsNonInventoriedLabels`, plus `ShellActions_ShouldLeaveImageSearchHidden_WhenThePageIsReceiving` in the Receiving file, so neither module can show the other's modes (FR-005) · `.../Module_Dunnage/ViewModels/ViewModel_Dunnage_ShellActionTests.cs`, `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_ShellActionTests.cs`
- [x] **T014** [US1] `ShellActions_ShouldOfferBackNextClearLabelDataAndHelpInThatOrder_WithTheDividerBetweenBackAndNextOnly_WhenBothAreShown` (FR-013)
- [x] **T015** [US1] `ShellActions_ShouldOmitBack_WhenThePageHasNoWayBack` and `ShellActions_ShouldDisableNext_UntilThePageConditionHolds`, covering Select Part before a part is chosen, Enter Loads before the entry is valid, and Select Type before a type is chosen (FR-014)
- [x] **T016** [US1] `ClearLabelDataSlot_ShouldCarryParameterlessCommands_ForAPlainAndAShiftActivation`, asserting `CanExecute(null)` is true for both so the button is not left dead, and that the shift command takes the wider clear (FR-015)
- [x] **T017** [US1] `HelpSlot_ShouldOpenHelpForTheCurrentStep_WhenInvoked`, verifying the call reaches `IService_Help` with the step in force (FR-016)
- [x] **T018** [US1] `ShellActions_ShouldMatchThePerPageMatrixActionForAction_OnAllSevenPages`, comparing every page's offered set against the matrix and asserting nothing extra appears (FR-017)
- [x] **T019** [US1] `NextSlotAndReviewSlot_ShouldCarryTheCommandsTheHostedViewModelsAlreadyOwn`, asserting the slot commands are the same instances the child view models expose (FR-020)
- [x] **T020** [US1] `NavigationShortcuts_ShouldUseTheSameCommandsAsTheStrip_SoAHiddenOrDisabledActionCannotBeReached`, asserting the accelerators act through the provider's commands rather than through private paths (FR-021)
- [x] **T021** [US1] Validate FR-023: the Back slot's `Kind` is `Back` and the save verb's is `Save`, and the two helpers that colour them resolve theme brushes rather than fixed colours. Read the bindings and confirm both follow a theme switch (FR-023) · `MainWindow.xaml`
- [x] **T022** [US1] Validate FR-024: the Clear All Labels and Help slots are icon-only and each keeps a non-empty `AccessibilityName` and tooltip, which the snapshot already supplies (FR-024) · `MainWindow.xaml`
- [x] **T023** [US1] `ShellActions_ShouldMarkOnlyTheModeInForce_WithSuccessTextAndBorder`, asserting `IsActive` is true for exactly one chip on every page (FR-025)
- [x] **T024** [US1] `ShellActions_ShouldRaisePropertyChangedAndReplaceTheWholeSnapshot_WhenTheStepChanges`, plus the audit that `MainWindow` names no `Module_Dunnage` type for these actions beyond the pre-existing leave-confirmation checks (FR-026)
- [x] **T025** [US1] Validate FR-034: no member left in the workflow page's code-behind reaches a child view model's gate, a dialog, or a workflow call; what remains is attaching the hosts, the accelerators, and the leave confirmation it already owned. Read every member and confirm (FR-034) · `Module_Dunnage/Views/View_Dunnage_WorkflowView.xaml.cs`
- [x] **T026** [US1] `Attach_ShouldShowOnlyTheActionsThePageOffers_WhenThePageBelongsToNeitherModule`, with a provider reporting Help alone: no mode group, no Back, no Next, no Clear (FR-035) · `MTM_Receiving_Application.Tests/Unit/Module_Shared/ViewModels/ViewModel_Shared_ShellActionBarTests.cs`
- [x] **T027** [US1] Validate FR-027: a mode change, a page change and an entry into settings each still raise the same leave confirmation while the workflow holds unsaved work, and a refusal leaves the step and the data untouched. Read both paths and confirm the new mode commands go through the same confirmation the page already used (FR-027) · `Module_Dunnage/ViewModels/ViewModel_Dunnage_WorkFlowViewModel.ShellActions.cs`, `Module_Dunnage/Views/View_Dunnage_WorkflowView.xaml.cs`

### Implementation

**Wave 1 — the provider:**

- [x] **T028** [US1] Create the second partial of the workflow view model and implement `IViewModel_ShellActionProvider` there: build the seven-page matrix from the mocked-step source of truth, put Review in the Next slot on Enter Details with kind `Plain` and `Save` in it on Review and Edit Mode with kind `Save`, put Back on Image Search pointing at the guided step the operator came from and at Select Type when the search was the opening mode, wrap `ClearLabelDataCommand(bool)` in the parameterless plain and shift commands the strip binds, add the Help, Next, Back and Review commands the page used to handle in code-behind, take the child view models through `AttachShellActionHosts`, refresh through `IService_Dispatcher` when a hosted view model's enablement changes, and persist the default through `IService_UserPreferences.UpdateDefaultDunnageModeAsync`. Give the mode builder the divider as an explicit parameter: the Receiving helper sets `ShowDividerAfter = canBeDefault`, which copied verbatim draws a trailing divider after Image Search (FR-006, FR-013, FR-014, FR-015, FR-016, FR-017, FR-020, FR-021, FR-027) · `Module_Dunnage/ViewModels/ViewModel_Dunnage_WorkFlowViewModel.ShellActions.cs`
- [x] **T029** [US1] Write the matrix so it never names `ModeSelection`, `ManualEntry`, `IsModeSelectionVisible`, `IsManualEntryVisible`, or `ReturnToModeSelectionCommand`; those belong to the deletion in Phase 5 (FR-034) · `Module_Dunnage/ViewModels/ViewModel_Dunnage_WorkFlowViewModel.ShellActions.cs`

**⟶ Wait for Wave 1 to finish, then:**

**Wave 2 — independent (different files):**

- [x] **T030** [P] [US1] Delete the whole navigation row (its `Grid`, its row definition and the row indices it shifts), delete the mode-selection host and the manual-entry host, name the image-search host so it can be reached, and leave every remaining step host's content untouched (FR-018, FR-019) · `Module_Dunnage/Views/View_Dunnage_WorkflowView.xaml`
- [x] **T031** [P] [US1] Attach the child view models in the constructor, delete the navigation handlers and the mode-selection action and accelerator, repoint Next, Back, Help and Clear at the provider's commands so the shortcuts obey the same gates as the strip, and rewrite `HasUnsavedData` without the mode-selection and manual-entry branches (FR-021, FR-027, FR-034) · `Module_Dunnage/Views/View_Dunnage_WorkflowView.xaml.cs`

**Checkpoint**: walking the guided sequence shows the mode chips, Back, Next, Clear All Labels and Help in the
one strip, in the same order, on every Non-Inventoried Labels page, and the page draws no navigation row
of its own.

## Phase 4: User Story 2 (Priority P1) — Non-Inventoried Labels opens straight into a mode

The startup decision, in the one file that makes it.

Files: `Module_Dunnage/Services/Service_DunnageWorkflow.cs`.

### Tests

`MTM_Receiving_Application.Tests/Unit/Module_Dunnage/Services/Service_DunnageWorkflowTests.cs`, with the
session user's stored mode mocked and `IService_UserPreferences` verified. Write these first and run them
red.

- [x] **T032** [US2] `StartWorkflowAsync_ShouldOpenTheGuidedEntryAndStoreGuided_WhenNoDefaultIsStored`, asserting the write reaches `IService_UserPreferences` (FR-009)
- [x] **T033** [US2] `StartWorkflowAsync_ShouldOpenTheStoredMode_WhenTheDefaultIsGuidedEditOrImageSearch`, so opening the module lands on the stored mode and on no other step (FR-010)
- [x] **T034** [US2] `StartWorkflowAsync_ShouldOpenTheGuidedEntryAndCorrectTheStoredValue_WhenTheDefaultIsManualOrUnrecognised`, covering the value retired by this feature and a value that names no page (FR-012)

### Implementation

**Wave 1 — the startup rewrite:**

- [x] **T035** [US2] Read the stored mode, send `guided`, `edit`, `image-search`, a retired `manual` and anything unrecognised to a page that still exists, write `guided` back through `IService_UserPreferences.UpdateDefaultDunnageModeAsync` for the absent and retired cases, and rewrite `GoToStep`'s Select-Type guard so entering Select Type still clears the session when the operator arrives from Review, Image Search or Edit Mode (FR-009, FR-010, FR-012) · `Module_Dunnage/Services/Service_DunnageWorkflow.cs`

**Checkpoint**: a user with no default opens in Guided and has Guided stored; a stored Edit or Image Search
default opens straight into it; a stored manual is corrected to Guided silently; no picker is reached.

## Phase 5: User Story 3 (Priority P1) — manual mode is gone from Non-Inventoried Labels

Deleting it, not hiding it.

Files: `Module_Dunnage/Enums/Enum_DunnageWorkflowStep.cs`,
`Module_Dunnage/ViewModels/ViewModel_Dunnage_WorkFlowViewModel.cs`,
`Module_Dunnage/Services/Service_DunnageShortcuts.cs`,
`Module_Settings.Dunnage/Models/Model_Settings_DunnageShortcuts.cs`,
`Module_Settings.Dunnage/ViewModels/ViewModel_Settings_Dunnage_KeyboardShortcuts.cs`,
`Module_Settings.Dunnage/Views/View_Settings_Dunnage_KeyboardShortcuts.xaml`,
`Module_Core/Services/Help/Service_Help.cs`, `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`.

This phase also registers the Label Files settings page in the same edit to
`ModuleServicesExtensions.cs`, because that page is offered in search and cannot open, and one file with
one owner is cheaper than two.

### Tests

- [x] **T036** [US3] Validate FR-011: no mode chip, badge, step, screen, shortcut, settings row, help topic or search alias offers or names manual or mode selection for Non-Inventoried Labels. This is an app-wide read after the deletions below, plus the suite passing with the deleted tests gone; the search alias is proved by T063 (FR-011) · `Module_Dunnage/`, `Module_Settings.Dunnage/`, `Module_Core/Services/Help/Service_Help.cs`

### Implementation

**Wave 1 — independent removals (different files):**

- [x] **T037** [P] [US3] Delete the mode-selection view model, its view, its code-behind and its test file (FR-011) · `Module_Dunnage/ViewModels/ViewModel_Dunnage_ModeSelectionViewModel.cs`, `Module_Dunnage/Views/View_Dunnage_ModeSelectionView.xaml`, `Module_Dunnage/Views/View_Dunnage_ModeSelectionView.xaml.cs`, `MTM_Receiving_Application.Tests/Unit/Module_Dunnage/ViewModels/ViewModel_Dunnage_ModeSelectionViewModelTests.cs`
- [x] **T038** [P] [US3] Delete the manual-entry view model and its view, code-behind included (FR-011) · `Module_Dunnage/ViewModels/ViewModel_Dunnage_ManualEntryViewModel.cs`, `Module_Dunnage/Views/View_Dunnage_ManualEntryView.xaml`, `Module_Dunnage/Views/View_Dunnage_ManualEntryView.xaml.cs`
- [x] **T039** [P] [US3] Drop the `Dunnage.ModeSelection` and `Dunnage.ManualEntry` topics, their step mappings, their related keys and their tips so help cannot offer the deleted screens (FR-011) · `Module_Core/Services/Help/Service_Help.cs`
- [x] **T040** [P] [US3] Drop the mode-selection shortcut from the model, the service, the settings view model and the settings view, and trim its assertions from the service and manifest tests (FR-011, FR-021) · `Module_Settings.Dunnage/Models/Model_Settings_DunnageShortcuts.cs`, `Module_Dunnage/Services/Service_DunnageShortcuts.cs`, `Module_Settings.Dunnage/ViewModels/ViewModel_Settings_Dunnage_KeyboardShortcuts.cs`, `Module_Settings.Dunnage/Views/View_Settings_Dunnage_KeyboardShortcuts.xaml`, `MTM_Receiving_Application.Tests/Unit/Module_Dunnage/Services/Service_DunnageShortcutsTests.cs`, `MTM_Receiving_Application.Tests/Unit/Module_Settings.Core/Services/SettingsManifestDunnageShortcutTests.cs`
- [x] **T041** [P] [US3] Remove the four dead Dunnage registrations and register the Label Files view model and view, so the page the hub and search offer can actually be resolved (FR-011, FR-028, FR-030) · `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
- [x] **T042** [P] [US3] Drop `IsModeSelectionVisible`, `IsManualEntryVisible`, their reset assignments, both switch cases, `ReturnToModeSelectionCommand` and the mode-selection title, and retitle the opening step to the guided entry (FR-010, FR-011) · `Module_Dunnage/ViewModels/ViewModel_Dunnage_WorkFlowViewModel.cs`
- [x] **T043** [P] [US3] Re-point the workflow view model tests off `ModeSelection` as their neutral starting step onto a step that still exists, in every place they use it (FR-011) · `MTM_Receiving_Application.Tests/Unit/Module_Dunnage/ViewModels/ViewModel_Dunnage_WorkFlowViewModelTests.cs`

**⟶ Wait for Wave 1 to finish, then:**

**Wave 2 — the enum, last, once nothing names the members:**

- [x] **T044** [US3] Delete `ModeSelection` and `ManualEntry` and renumber the survivors from zero, since nothing persists a step number across a session (FR-011) · `Module_Dunnage/Enums/Enum_DunnageWorkflowStep.cs`

**⟶ Wait for Wave 2 to finish, then:**

**Wave 3 — the two remaining references only the enum change reveals:**

- [x] **T045** [US3] Repoint the review view model's post-save and cancel destinations at the guided entry and drop its mode-selection return command (FR-011) · `Module_Dunnage/ViewModels/ViewModel_Dunnage_ReviewViewModel.cs`
- [x] **T046** [US3] Repoint the edit mode view model's return destination at the guided entry (FR-011) · `Module_Dunnage/ViewModels/ViewModel_Dunnage_EditModeViewModel.cs`

**Checkpoint**: manual mode and the mode picker exist nowhere in code, settings, help, search or
navigation, the solution builds, and a stored manual default lands on Guided and is corrected.

## Phase 6: User Story 4 (Priority P2) — deciding which mode is my default, from the strip

Files: none. The provider from Phase 3 owns these decisions and the projection from Phase 2 owns the Ctrl
dispatch; this phase proves them.

### Tests

`MTM_Receiving_Application.Tests/Unit/Module_Dunnage/ViewModels/ViewModel_Dunnage_ShellDefaultModeTests.cs`,
with `IService_UserPreferences` mocked and the session user's stored mode read back through the snapshot.

- [x] **T047** [US4] `EnterMode_ShouldNotRestartTheWorkflow_WhenTheModeIsAlreadyInForce`, asserting no reset and no step change on a second press of the active mode (FR-006)
- [x] **T048** [US4] `RememberModeAsDefault_ShouldStoreTheModeAndLeaveThePageAlone_WhenAModeChipIsPressedWithCtrl`, asserting one write and that the active page did not move (FR-007)
- [x] **T049** [US4] `ShellActions_ShouldTickExactlyOneDefault_WhenADifferentModeBecomesTheDefault`, including Image Search replacing Edit and Guided replacing Image Search (FR-008)

## Phase 7: User Story 5 (Priority P2) — the primary action for a page lands in the expected slot

The strip already occupies the slots; this phase removes the buttons the pages still draw.

Files: `Module_Dunnage/Views/View_Dunnage_ReviewView.xaml`,
`Module_Dunnage/Views/View_Dunnage_EditModeView.xaml`,
`Module_Dunnage/Views/View_Dunnage_ReviewView.xaml.cs`.

### Tests

- [x] **T050** [US5] Validate FR-018: no Non-Inventoried Labels page draws a control the strip owns. Read the three XAML files and confirm zero Back, Next, Clear All Labels, Review or save buttons remain, and that no command is bound twice (FR-018) · `View_Dunnage_WorkflowView.xaml`, `View_Dunnage_ReviewView.xaml`, `View_Dunnage_EditModeView.xaml`
- [x] **T051** [US5] `ShellActions_ShouldNameNoEditingToolbarCommand_OnEditMode`, asserting the snapshot's four slots carry only the strip's verbs while load selection, select all, remove, columns, filters and paging stay on the page (FR-019) · `MTM_Receiving_Application.Tests/Unit/Module_Dunnage/ViewModels/ViewModel_Dunnage_ShellPrimaryActionTests.cs`

### Implementation

**Wave 1 — independent pages (different files):**

- [x] **T052** [P] [US5] Delete the Save All button and leave the per-entry Previous, Next and table-toggle controls in place (FR-018, FR-019) · `Module_Dunnage/Views/View_Dunnage_ReviewView.xaml`
- [x] **T053** [P] [US5] Delete the Save Changes button and leave the load, select-all, remove, column, filter and paging controls in place (FR-018, FR-019) · `Module_Dunnage/Views/View_Dunnage_EditModeView.xaml`

**⟶ Wait for Wave 1 to finish, then:**

**Wave 2 — the code-behind the deletion touches:**

- [x] **T054** [US5] Repoint `FocusForAccess` at a control that still exists now that the save button is gone (FR-018) · `Module_Dunnage/Views/View_Dunnage_ReviewView.xaml.cs`

**Checkpoint**: Review and Save and Edit Mode show their save verb in the strip's Next slot, each page draws
none of its own, and Edit Mode keeps its whole editing toolbar.

## Phase 8: User Story 6 (Priority P3) — the strip's words stay editable

Files: none. Phase 1 declared the copy and Phase 3's provider reads it; this phase proves a manager can
reword the strip without a rebuild.

### Tests

`MTM_Receiving_Application.Tests/Unit/Module_Dunnage/ViewModels/ViewModel_Dunnage_ShellCopyTests.cs`, with
`IService_DunnageSettings` mocked to return overridden copy.

- [x] **T055** [US6] `ShellActions_ShouldTakeEveryLabelAndTooltipFromItsSettingsKey_WithTheVerbatimDefaultWhenUnedited`, and `ShellActions_ShouldShowTheNewWording_WithoutARebuild_WhenAKeyIsChanged` (FR-022)

## Phase 9: User Story 7 (Priority P3) — settings cards open the page they name

Files: `Module_Settings.Dunnage/ViewModels/ViewModel_Settings_Dunnage_CategoryHub.cs`,
`Module_Settings.Dunnage/Views/View_Settings_Dunnage_CategoryHub.xaml`,
`Module_Settings.Dunnage/Views/View_Settings_Dunnage_CategoryHub.xaml.cs`.

### Tests

- [x] **T056** [US7] `Steps_ShouldOfferSixTitlesIncludingLabelFiles_AndEachTitleShouldResolveThePageItNames`, asserting the Label Files step points at `View_Settings_Dunnage_LabelPaths` and that no two titles collide, so resolution by name is unambiguous (FR-028) · `MTM_Receiving_Application.Tests/Unit/Module_Settings.Dunnage/ViewModels/ViewModel_Settings_Dunnage_CategoryHubTests.cs`
- [x] **T057** [US7] Validate FR-029: every Receiving hub card's `Tag` equals the title of the step it opens, and no card names a page that does not exist. Feature 005 already repaired this hub, so this is a read, not a change (FR-029) · `Module_Settings.Receiving/Views/View_Settings_Receiving_CategoryHub.xaml`, `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_CategoryHub.cs`

### Implementation

**Wave 1 — independent (different files):**

- [x] **T058** [P] [US7] Add the Label Files step under Keyboard Shortcuts so the page that exists, and that search already offers, has a card of its own (FR-028) · `Module_Settings.Dunnage/ViewModels/ViewModel_Settings_Dunnage_CategoryHub.cs`
- [x] **T059** [P] [US7] Add `OnStepClicked` beside the six index handlers, resolving the step by the title the card carries and navigating to it, mirroring the Receiving hub's handler (FR-028) · `Module_Settings.Dunnage/Views/View_Settings_Dunnage_CategoryHub.xaml.cs`

**⟶ Wait for Wave 1 to finish, then:**

**Wave 2 — the cards themselves:**

- [x] **T060** [US7] Give every card a `Tag` holding the exact step title it opens, point them all at `OnStepClicked`, and add the Label Files card in the grid's spare position (FR-028) · `Module_Settings.Dunnage/Views/View_Settings_Dunnage_CategoryHub.xaml`

**⟶ Wait for Wave 2 to finish, then:**

**Wave 3 — remove the positional mapping:**

- [x] **T061** [US7] Delete `OnStep0Clicked` to `OnStep5Clicked` and the index-based navigation they call, so a reordered card cannot move another card's destination (FR-028) · `Module_Settings.Dunnage/Views/View_Settings_Dunnage_CategoryHub.xaml.cs`

**Checkpoint**: every card on both hubs opens the page named on it, and adding or moving a card cannot
change another card's destination.

## Phase 10: User Story 8 (Priority P3) — search finds every page I may open, and only those

Files: `MainWindow.Search.cs`.

### Tests

`MTM_Receiving_Application.Tests/Unit/Module_Settings.Core/Services/SearchDestinationCatalogTests.cs`.
The catalog and the routes are private statics on `MainWindow`, so read them by reflection; the settings
page registrations are checked against a container built from the same registration the app uses, with
in-memory configuration.

- [x] **T062** [US8] `Catalog_ShouldNameEveryRouteInNavRoutes_ExceptTheDisabledModule`, so no page reachable from the navigation pane is missing from search (FR-030)
- [x] **T063** [US8] `Catalog_ShouldOfferNoRetiredPageOrInnerStep_AndEverySettingsDestinationShouldResolve`, covering the retired mode-selection alias, every inner guided step, and the Label Files page that was offered but never registered (FR-031)
- [x] **T064** [US8] `OfferedResults_ShouldHideTheScannerEntry_WhenThePolicyRefusesTheUser_AndRestoreItWhenTheListChanges`, so the allow-list filters the results and needs no code change when it grows (FR-032)
- [x] **T065** [US8] Validate FR-033: selecting a result while a guided workflow holds unsaved work raises the same leave confirmation navigation raises. Both kinds already route through the confirmation; read both paths and confirm once (FR-033) · `MainWindow.Search.cs`

### Implementation

**Wave 1 — one opener, one catalog:**

- [x] **T066** [US8] Split the open path so the navigation pane and the search box call one opener that carries the scanner allow-list check and shows the same refusal, filter the scanner entry out of the offered results when the policy refuses, add the Receiving Validator launcher, and retire the `non-inventoried labels mode selection` alias with the screen it names (FR-030, FR-031, FR-032, FR-033) · `MainWindow.Search.cs`

**Checkpoint**: comparing search against the navigation pane and both hubs finds no missing page, no
retired page, no inner step, and a page behind the scanner allow-list is refused from search exactly as
it is from navigation.

## Phase 11: Polish

- [x] **T067** Record the Non-Inventoried Labels strip, the deleted mode picker and manual mode, and the settings and search repairs in the change log · `CHANGELOG.md`
- [x] **T068** Validate against Success Criteria: `dotnet build MTM_Receiving_Application.slnx` and `dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj` must both pass, with zero warnings introduced by this feature (SC-011). No post-implement validation hook owns this, so the suites run here.
- [x] **T069** Spot check a page from each of Reconciliation, Reporting, Settings and Volvo and confirm zero mode controls and no strip on pages that offer no actions (SC-001, SC-002, SC-006, SC-007)
- [x] **T070** Recap the three measurable outcomes that a test cannot reach: one action from opening to the first guided step, a chosen default surviving a restart, and zero ways left into manual mode (SC-003, SC-004, SC-005) · `specs/006-dunnage-shell-action-bar/spec.md`

## Dependencies and Execution Order

Phases run in the order written. The chain is Setup → Foundational → the three P1 stories → the P2
stories → the P3 stories → Polish.

- **Phase 1 blocks everything.** The keys, their defaults and their manifest rows are what Phase 3's
  provider reads. Its waves are T001 and T002 together, then T003.
- **Phase 2 blocks every story.** Its waves are T004 and T005 together, then T006, then T007 and T008
  together. T008 leaves the search region in `MainWindow.Search.cs`, which Phase 10 then owns.
- **Phase 3 (US1)** owns the whole workflow page. T028 and T029 are one file and must land together;
  then T030 and T031 run in parallel on the two remaining files. Nothing after this phase edits those
  three files. Phase 5 works in the other partial and the enum, neither of which they name, which is why
  the two phases stay out of each other's files. Phase 4 and Phase 5 follow it so the page stays
  continuous while the picker goes.
- **Phase 4 (US2)** is one file and one task, T035. Nothing waits on it except the checkpoint that the
  picker is unreachable.
- **Phase 5 (US3)** must come after Phase 4, because the service stops naming the retired steps first.
  Its wave 1 is seven parallel removals (T037 to T043), then T044 drops the enum members, then T045 and
  T046 in parallel fix the two references the enum change surfaces.
- **Phase 6 (US4)** has no implementation: it proves what Phases 2 and 3 built, so it may run any time
  after Phase 3. T047 depends on T028; T048 and T049 also depend on T006.
- **Phase 7 (US5)** may run any time after Phase 3. T052 and T053 run in parallel, then T054.
- **Phase 8 (US6)** may run any time after Phase 3, since it only reads the provider's copy.
- **Phase 9 (US7)** depends on Phase 5 only for the Label Files registration (T041). T058 and T059 run
  in parallel, then T060, then T061.
- **Phase 10 (US8)** depends on Phase 2's move (T008) and on Phase 5's registration (T041). T066 is one
  file and one pass.
- **Phase 11** closes the spec and runs the suites once, because this project has no post-implement
  validation hook.

### Parallel Opportunities

- Phase 1 wave 1: T001 and T002, two files.
- Phase 2 wave 1: T004 and T005; wave 3: T007 and T008.
- Phase 3 wave 2: T030 and T031.
- Phase 5 wave 1: T037 to T043, seven files across the module, help, settings and dependency injection.
- Phase 5 wave 3: T045 and T046.
- Phase 7 wave 1: T052 and T053.
- Phase 9 wave 1: T058 and T059.

### What the implement step must respect

- **T028 before anything gates a slot.** The child view models reach the provider through
  `AttachShellActionHosts`, so no Back, Next, Review or Save slot can reflect a page condition until
  that is in place. It is a prerequisite, never a parallel task.
- **T044 goes last in Phase 5.** Dropping the enum members before T037 to T043 removes every name is a
  build break with no useful error.
- **The divider is explicit in the Dunnage mode builder.** `CreateModeAction` in the Receiving view model
  sets `ShowDividerAfter = canBeDefault`; copied verbatim it draws a trailing divider after Image Search.
- **Clear All Labels binds with no parameter.** The strip calls `Command.CanExecute(null)`, so a
  `bool`-parameter command reports false and leaves the button dead. The slot carries parameterless
  wrappers around `ClearLabelDataCommand(bool)`.
- **`Save` is the save verb.** The two replaced controls read `Save All` and `Save Changes`; the
  Verbatim Constraints win and one key, `Dunnage.UiText.Workflow.Save`, serves both slots.
- **`ViewModel_Dunnage_WorkFlowViewModelTests` will not compile at T044** until T043 has re-pointed it
  off `ModeSelection`.

## Phase 12: Convergence

A code-level assessment of the current implementation against `spec.md`, `plan.md`, `tasks.md` and the
constitution found one gap. FR-030 requires the search box to offer a destination for every page
reachable from the navigation pane and from either settings hub, but the two Receiving cards that open
`View_Settings_Receiving_VendorVariables` and `View_Settings_Receiving_WeldedParts` have no entry in the
search catalog, so SC-008's comparison against both hubs still finds pages missing from search.

- [x] **T071** Add a search destination for `View_Settings_Receiving_VendorVariables` titled "Custom Vendor Variables" and one for `View_Settings_Receiving_WeldedParts` titled "Welded Part Numbers", with aliases matching the card titles, and extend `SearchDestinationCatalogTests` to compare every card title on the Non-Inventoried Labels and Receiving hubs against the catalog so a hub page can no longer go missing from search (FR-030, SC-008) (missing) · `MainWindow.Search.cs`, `MTM_Receiving_Application.Tests/Unit/Module_Settings.Core/Services/SearchDestinationCatalogTests.cs`
