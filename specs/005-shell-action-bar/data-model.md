# Data Model: Persistent Shell Action Bar

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md) | **Research**: [research.md](research.md)
**Created**: 2026-11-19

This feature introduces no database tables, no DAOs, and no stored procedures for the action bar itself. The
one persistent value the bar touches already exists: the per-user default receiving mode stored through
`IService_UserPreferences` and surfaced on the session user as `DefaultReceivingMode`.

US8, appended afterwards, does add data access. It introduces no table and no procedure either: the welded
part list is the `settings_weldedcoils` table that Extra Tools already maintains, reached through
`sp_Settings_WeldedCoils_GetAll`, `_Insert` and `_Delete`. It adds one entity, one DAO, and one service.

Types split across three modules exactly as the plan's Structure Decision states: the contract and the
state snapshot live in `Module_Core` beside the two header provider contracts, the shell projection lives
in `Module_Shared`, and the only implementation lives in `Module_Receiving`. No Receiving type crosses into
`Module_Core` or `Module_Shared`; the contract speaks only in booleans, strings, and `ICommand`. The welded
part entity lives in `Module_Settings.Receiving` because the page that owns it lives there.

## Entity Overview

```text
IViewModel_ShellActionProvider               Module_Core contract, implemented by Receiving workflow
        -> Model_ShellActionBarState         immutable snapshot, replaced wholesale on every change
                -> Model_ShellModeAction     guided, manual, edit, overlay (named positions, not a list)
                -> Model_ShellActionSlot     back, next, clear label data, help
ViewModel_Shared_ShellActionBar              Module_Shared projection the bar binds with x:Bind
ViewModel_Receiving_Workflow                 Module_Receiving implementation; owns the per-page matrix
        -> Enum_ReceivingWorkflowStep        reshaped: ModeSelection member deleted
        -> DefaultReceivingMode (session user)  reshaped: single stored value, guided | manual | edit | null

Model_Settings_ReceivingWeldedPart           Module_Settings.Receiving entity for US8
        -> Dao_Settings_ReceivingWeldedParts sp_Settings_WeldedCoils_GetAll, _Insert, _Delete
        -> Service_Settings_ReceivingWeldedParts  normalise, 11-character limit, part-master check
```

## IViewModel_ShellActionProvider

The shell-owned contract a page view model implements to tell the bar what to show. It sits in
`Module_Core/Contracts/ViewModels/` beside `IViewModel_HeaderTitleProvider` and
`IViewModel_HeaderStepBarProvider`, and it is resolved with the same three-step lookup those two already
use: the frame content, then its `DataContext`, then its `ViewModel` property.

| Member | Type | Notes |
| --- | --- | --- |
| `ShellActions` | `Model_ShellActionBarState` | read-only snapshot the shell re-reads on every `PropertyChanged` |
| `SetDefaultMode` | `void` method, `string? storedModeValue` | persists the default; takes the stored string, not an enum |

**Rules**

- The interface extends `INotifyPropertyChanged`, matching the two header providers.
- `ShellActions` changes are announced by raising `PropertyChanged` for that one member; the projection
  re-reads the whole snapshot rather than any loose per-control property.
- `SetDefaultMode` accepts exactly `"guided"`, `"manual"`, `"edit"`, or `null`. It carries no Receiving
  type, which is what keeps the core contract module-agnostic (FR-017, R5).
- A page that supplies no provider is a valid state: the shell treats it as an empty snapshot (FR-003,
  Assumptions).

## Model_ShellActionBarState

The immutable snapshot the contract returns, in `Module_Core/Models/UI/`. It is replaced whole on each
step change, which is exactly how the underlying state changes (R5).

| Field | Type | Notes |
| --- | --- | --- |
| `IsVisible` | `bool` | false collapses the bar row so the page takes full height |
| `ShowModeGroup` | `bool` | the left group, shown only on Receiving pages |
| `Guided` | `Model_ShellModeAction` | first in the left group, divider after |
| `Manual` | `Model_ShellModeAction` | second, divider after |
| `Edit` | `Model_ShellModeAction` | third, divider after |
| `Overlay` | `Model_ShellModeAction` | fourth, no divider, no default checkbox |
| `Back` | `Model_ShellActionSlot` | the Back slot; a page-specific verb may occupy it |
| `Next` | `Model_ShellActionSlot` | the Next slot; a page-specific verb may occupy it |
| `ClearLabelData` | `Model_ShellActionSlot` | the Clear Label Data position |
| `Help` | `Model_ShellActionSlot` | the Help position |
| `IsGuidedDefault` | `bool` | which checkbox is ticked; at most one of the three is true |
| `IsManualDefault` | `bool` | |
| `IsEditDefault` | `bool` | |

The four mode positions are named, not a list. The only thing that differs between them is the checkbox
and the divider, and a list would force a template selector or conditional template branches to express
that (R5).

**Rules**

- `IsVisible` is true only when the page supplies a provider and the provider reports at least one visible
  action; a page offering nothing collapses the bar entirely (FR-003, US1-3).
- `ShowModeGroup` is false on every non-Receiving page, so no Guided, Manual, Edit, or Overlay control
  ever appears outside Receiving (FR-005, US3-5, US5-1).
- Back and Next each appear only when the page supports them; Back is absent rather than disabled when
  unsupported, and Next is disabled until the page's own conditions are met (FR-009, US1-2).

## Model_ShellActionSlot

One named position in the right group. The same shape describes Back, Next, Clear Label Data, and Help,
so the shell renders every position uniformly and a page-specific verb swaps in by changing only label,
tooltip, and command.

| Field | Type | Notes |
| --- | --- | --- |
| `IsVisible` | `bool` | false removes the control from the strip |
| `IsEnabled` | `bool` | false renders it disabled but still present |
| `Label` | `string` | from `ReceivingSettingsKeys`; the accessibility name when set |
| `Tooltip` | `string` | from `ReceivingSettingsKeys` |
| `AccessibilityName` | `string` | automation and screen-reader text |
| `Command` | `ICommand` | the existing command the page already used (FR-024) |
| `ShiftCommand` | `ICommand?` | the wider variant run when the control is activated with Shift held; null when the position has no wider form |
| `ShowDividerAfter` | `bool` | puts the divider in the right place without a second collection |

**Rules**

- The `Command` is always one of the page's existing commands: `PreviousStepCommand`, `NextStepCommand`,
  `ResetLabelDataAsync`, `ShowHelpAsync`, or the page's own save/start verb. The bar never re-implements
  a page's behaviour (FR-024).
- Back is removed by setting `IsVisible` false, never by disabling (US1-2, FR-009).
- Clear Label Data is disabled when `CanClearLabelData` is false; there is no failure path when nothing
  is clearable (Edge Cases).
- Clear Label Data carries `ResetLabelDataCommand` as its `Command` and `ResetLabelDataAllCommand` as its
  `ShiftCommand`, so the two strengths of the button it replaced survive on the strip itself and not only
  on the keyboard shortcut (FR-041).
- Every user-visible `Label` and `Tooltip` comes through `IService_ReceivingSettings` from
  `ReceivingSettingsKeys`, so nothing the user reads is hard-coded in the shell (FR-025, SC-008).

## Model_ShellModeAction

One entry-mode control in the left group. It carries the same visible and enabled flags as a right-group
slot, plus the checkbox the three defaultable modes own. Overlay carries a command and no checkbox.

| Field | Type | Notes |
| --- | --- | --- |
| `Mode` | `string` | `guided`, `manual`, `edit`, or `overlay` |
| `Label` | `string` | `Guided`, `Manual`, `Edit`, or `Overlay` from settings |
| `Tooltip` | `string` | from settings |
| `AccessibilityName` | `string` | automation and screen-reader text |
| `Command` | `ICommand` | enters that mode immediately (FR-015) |
| `ShowDefaultCheckbox` | `bool` | true only for guided, manual, and edit |
| `DefaultTooltip` | `string` | `make Guided my default` and the like |
| `ShowDividerAfter` | `bool` | true for the first three |

**Rules**

- Pressing `Command` changes only the active mode for the current session; it never changes the stored
  default (US3-1, Assumptions).
- The checkbox is a separate control: ticking it persists the default and never navigates (FR-006). It
  ticks through the projection's settable boolean, which calls `SetDefaultMode`; the snapshot only
  supplies the tooltip and whether the checkbox exists.
- At most one of `IsGuidedDefault`, `IsManualDefault`, and `IsEditDefault` is true. Ticking one clears the
  other two; unticking the last ticked one leaves none (FR-007, US3-2, US3-3).
- Pressing the mode control for the mode already active must not restart the workflow or discard collected
  label data (Edge Cases).

## ViewModel_Shared_ShellActionBar

The shell projection the bar binds to with `x:Bind`, in `Module_Shared`. The resolved provider's type is
only known at run time, so this fixed projection gives the bar a compile-time bindable surface while every
value still comes from the page (R4).

| Member | Type | Notes |
| --- | --- | --- |
| `IsVisible` | `bool` | mirror of the snapshot |
| `ShowModeGroup` | `bool` | mirror of the snapshot |
| `Guided` / `Manual` / `Edit` / `Overlay` | `Model_ShellModeAction` | mirrors of the snapshot |
| `Back` / `Next` / `ClearLabelData` / `Help` | `Model_ShellActionSlot` | mirrors of the snapshot |
| `IsGuidedDefault` / `IsManualDefault` / `IsEditDefault` | `bool`, settable | two-way bound to the three checkboxes; the setter calls the provider's `SetDefaultMode` |
| `Attach(IViewModel_ShellActionProvider)` | method | subscribes and projects the first snapshot |
| `Detach()` | method | unsubscribes and collapses the row |

**Rules**

- `Attach` subscribes to the provider's `PropertyChanged`, then projects the snapshot immediately so the
  first render is correct.
- `Detach` collapses the row and clears every slot. It must collapse immediately, not wait for a later
  provider, matching how `ClearHeaderSubscription` hides the step bar on a null provider (plan, Code
  Notes).
- A checkbox tick sets the matching boolean, calls `SetDefaultMode("guided" | "manual" | "edit")` or
  `SetDefaultMode(null)` when it was unticked, then re-reads `ShellActions` so the other two boxes clear
  from the one place that decides.
- The projection holds no domain logic; it is a straight copy of the snapshot (R4, R12).

## ViewModel_Receiving_Workflow

The only implementation of `IViewModel_ShellActionProvider`. It already owns the workflow step, the
guided-walk commands, and the reset/help actions; the feature adds the contract implementation and the
per-page matrix, and deletes the mode-selection view model it replaced.

| Field | Type | Notes |
| --- | --- | --- |
| `ShellActions` | `Model_ShellActionBarState` | the contract's snapshot, re-projected on every step change |
| `_currentStep` | `Enum_ReceivingWorkflowStep` | starts at `POEntry`; `ModeSelection` member is gone |
| attached child VMs | Manual Entry, Edit Mode, Review | handed over by the workflow page at construction (R3) |

**Per-page matrix** (FR-012). The right group's slots derive from `Service_ReceivingWorkflow.CurrentStep`
and the existing `ShowWorkflowNextButton`, `IsNextButtonEnabled`, `ShowWorkflowBackButton`, and
`CanClearLabelData` members; the page-specific verbs come from the attached child view models.

| Page | Back | Next | Clear Label Data | Help |
| --- | --- | --- | --- | --- |
| Enter PO Number | absent | `NextStepCommand`, disabled until `IsPoPartListLoaded` | shown | shown |
| Select Part | `PreviousStepCommand` | disabled until a part is chosen | shown | shown |
| Enter Load Information | `PreviousStepCommand` | `NextStepCommand` | shown | shown |
| Enter Weight & Quantity | `PreviousStepCommand` | `NextStepCommand` | shown | shown |
| Enter Heat & Lot | `PreviousStepCommand` | `NextStepCommand` | shown | shown |
| Enter Package Type | `PreviousStepCommand` | `NextStepCommand` | shown | shown |
| Manual Entry | absent | Save & Finish | shown | shown |
| Edit Mode | absent | Save & Finish | shown | shown |
| Review & Save | `PreviousStepCommand` | Save to Database | shown | shown |
| Result | Start New Entry | absent | shown | shown |
| Saving | absent | absent | absent | absent |

**Rules**

- `CanGoToNextStep()` reads `ShowWorkflowNextButton && IsNextButtonEnabled`, the single source for both
  the bar's Next slot and the `Ctrl+Right` keyboard gate (plan, Code Notes).
- `RequestedEditDataSource` is set to `CurrentLabels` whenever Edit mode is entered, so the Edit mode
  control and the Edit default checkbox share one service call path (plan, Code Notes).
- The keyboard handler keeps gating on `NextStepCommand.CanExecute`, `PreviousStepCommand.CanExecute`,
  `CanClearLabelData`, and the Help shortcut repointed at the bar's Help command, so a shortcut can never
  reach an action the bar has hidden or disabled (FR-022, Edge Cases, R11).
- The mode group is always shown on Receiving pages, so no mode-selection shortcut remains (R9).

## Enum_ReceivingWorkflowStep

Reshaped: the `ModeSelection` member and its backing field are deleted, and the field that held it now
starts at `POEntry` because `StartWorkflowAsync` always resolves a mode before the first page shows.

| Member | Still present | Notes |
| --- | --- | --- |
| `ModeSelection` | removed | its only destinations are deleted |
| `POEntry` through `Complete` | unchanged | the step bar and guided walk keep their order |

**Rule**: no code path may assign or compare against `ModeSelection` after the change; the removed member
takes the `ReturnToModeSelectionCommand` and the mode-selection step-bar entry with it.

## User Default Receiving Mode

The single persistent value the feature reads and writes. It already exists on the session user
(`DefaultReceivingMode`) and in the preference store; the feature reshapes how it is decided and removes
the competing `DefaultModeOnStartup` setting from the routing decision.

| Value | Meaning |
| --- | --- |
| `guided` | the operator's default is the guided walk |
| `manual` | the operator's default is Manual Entry |
| `edit` | the operator's default is Edit Mode |
| `null` / empty / unrecognised | no stored default; resolves to `guided` |

**State transitions**

```text
NoDefault  --open Receiving-->  Guided (stored back as guided)
DefaultManual --open Receiving--> Manual
DefaultEdit --open Receiving--> Edit
AnyMode --tick another checkbox--> NewDefault (previous default cleared)
OnlyDefault --untick last checkbox--> NoDefault
```

**Rules**

- `StartWorkflowAsync` resolves the mode from the stored preference, falls back to Guided when the value
  is missing, empty, or unrecognised, and writes Guided back through
  `IService_UserPreferences.UpdateDefaultReceivingModeAsync` when it had to fall back (R7, FR-014).
- `SetDefaultMode` writes through the same service for the signed-in user only, then updates the session
  user's `DefaultReceivingMode` so the checkboxes and the next launch agree (R6).
- The `Receiving.BusinessRules.DefaultModeOnStartup` setting is no longer consulted for routing; its key
  stays readable and harmless, and `Mode Selection` is removed from its option lists (R7).

## Bar States

The snapshot's `IsVisible` and slot set collapse to these page-level states, matching the plan's States
section.

```text
NoActions     provider absent, or IsVisible false  -> bar row Collapsed, page full height
GuidedStep    a guided walk step is active         -> mode group, Clear Label Data, Help; Back and Next follow the step
ModeOnlyPage  Manual Entry or Edit Mode active     -> no Back; Save & Finish in the Next slot; mode group shown
ReviewPage    Review active                        -> Back shown; Save to Database in the Next slot
ResultPage    Complete or failed save              -> Start New Entry in the Back slot; mode group and Overlay shown; no Next
Saving        save in flight                       -> no actions offered, bar collapses
```

## Reused Types
`Module_Core` gains only a presentation contract (`IViewModel_ShellActionProvider`) and a plain state
model (`Model_ShellActionBarState` and its slot and mode records). `Module_Shared` gains the projection.
From existing types the feature reuses `IService_UserPreferences.UpdateDefaultReceivingModeAsync`,
`IService_ReceivingWorkflow.IsPoPartListLoaded`, the `ReceivingSettingsKeys` surface through
`IService_ReceivingSettings`, the existing `IService_ErrorHandler` and `IService_Help`, and the
`PreviousStepCommand` / `NextStepCommand` / `ResetLabelDataAsync` / `CanClearLabelData` / `ShowHelpAsync`
actions already on `ViewModel_Receiving_Workflow`. No new enum is added for the bar's state; `Enum_ShellActionKind`
is a plain colour kind on a slot.

Seven settings keys are added under `Receiving.UiText.Shell`, because no existing key carries the bar's
wording: the four mode labels default to `Guided`, `Manual`, `Edit`, and `Overlay` (the existing
`Receiving.UiText.ModeSelection.GuidedTitle` holds `Guided Wizard`, which the spec forbids on the bar
button), and three tooltips default to `make Guided my default`, `make Manual my default`, and
`make Edit my default`. The bar reuses the existing `WorkflowBack`, `WorkflowNext`,
`WorkflowResetLabelData`, `WorkflowHelp`, `ManualEntrySaveAndFinish`, `EditModeSaveAndFinish`,
`ReviewSaveToDatabase`, and `CompletionStartNewEntry` keys for the other positions.

## Model_Settings_ReceivingWeldedPart

The one row of the welded part list. It is deliberately two fields: the list is a set of part numbers, so a
row has nothing else to say.

| Field | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | the stored row id, from `settings_weldedcoils.id` |
| `PartId` | `string` | upper-case part number, at most 11 characters |

**Invariants**

- `PartId` is stored trimmed and upper-cased, so two rows cannot differ only by case or padding.
- `PartId` is at most 11 characters, the column width, and is refused before the procedure runs when it is
  longer.
- `PartId` exists in the Infor Visual part master, checked through `IService_InforVisual.PartExistsAsync`
  which is read only.
- A row is never edited in place. It is added or removed.

**Storage**

```text
settings_weldedcoils            shared with Extra Tools, Welded Coils; unique key on partid
sp_Settings_WeldedCoils_GetAll  every row, ordered by part
sp_Settings_WeldedCoils_Insert  @p_partid in, @p_new_id out
sp_Settings_WeldedCoils_Delete  @p_id in
```

No table, column or procedure is created by this feature. The page writes the same list Extra Tools writes,
which is what makes SC-013 true without a refresh path of its own.

## Service_Settings_ReceivingWeldedParts

The page's one dependency, and the only place the rules sit.

| Member | Returns | Rule |
| --- | --- | --- |
| `GetAllAsync` | `Model_Dao_Result<List<Model_Settings_ReceivingWeldedPart>>` | passes the DAO result through |
| `AddAsync(string partId)` | `Model_Dao_Result<int>` | normalises, enforces the length, checks the part master, then inserts |
| `DeleteAsync(int id)` | `Model_Dao_Result` | passes the DAO result through |

A failure from `AddAsync` carries a message the page shows as is, so a refused part never becomes a stored
row.
