# Data Model: Shell Action Bar for Non-Inventoried Labels

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md) | **Research**: [research.md](research.md)

This feature introduces no stored table and no new persisted entity. Everything below is either a
presentation model the shell already carries and this feature reshapes, or a value already stored on the
user record that this feature narrows. The one write path is the stored default mode.

## Page action set

The collection of navigation actions a single page offers. It is supplied by the page and never known by
the shell (FR-026).

**Carrier**: `Model_ShellActionBarState`, returned by `IViewModel_ShellActionProvider.ShellActions`.

| Field | Type | Rule |
| --- | --- | --- |
| `IsVisible` | `bool` | True only while the page offers at least one action. False collapses the row so the page takes the full remaining height (FR-002). |
| `ShowModeGroup` | `bool` | True only for a page inside Receiving or Non-Inventoried Labels (FR-004). |
| `Guided`, `Manual`, `Edit`, `Overlay`, `ImageSearch` | `Model_ShellModeAction` | One slot per entry mode any module offers. A module fills the modes it owns and leaves the rest at `Hidden` (FR-005). |
| `Back`, `Next`, `ClearLabelData`, `Help` | `Model_ShellActionSlot` | The four right-hand positions, in that order, with the divider drawn only between Back and Next (FR-013). |
| `IsGuidedDefault`, `IsManualDefault`, `IsEditDefault`, `IsImageSearchDefault` | `bool` | Exactly one may be true, and only for a mode the module owns (FR-008). |

**Reshaped here**: `ImageSearch` and `IsImageSearchDefault` are new. `Manual` survives because Receiving
takes it, and is `Hidden` on every Non-Inventoried Labels page (FR-005).

**Validation**: a page that reports `IsVisible` false shows nothing, whatever its slots say. The strip
computes its own visibility from the slots, so a snapshot that claims to be visible while offering no
action still collapses.

## Mode group

The entry modes of the module that owns the active page. One `Model_ShellModeAction` per mode.

| Field | Type | Rule |
| --- | --- | --- |
| `Mode` | `string` | The stored value the mode maps to: `guided`, `edit`, or `image-search` for Non-Inventoried Labels (FR-007). |
| `Label` | `string` | Comes from settings and defaults to exactly `Guided`, `Edit`, or `Image Search` (FR-022, Verbatim Constraints). |
| `Tooltip`, `AccessibilityName` | `string` | Also settings-driven; the accessibility name is never empty (FR-024). |
| `Command` | `ICommand` | Enters the mode. Must not restart a workflow already in that mode (FR-006). |
| `IsActive` | `bool` | True for the mode in force, which draws success-green text and border (FR-025). |
| `CanBeDefault` | `bool` | True for all three Non-Inventoried Labels modes and for Receiving's Guided, Manual, and Edit; false for Receiving's Overlay. |
| `DefaultBadge` | `string` | The word `Default`, shown beneath the control that holds the stored default, empty otherwise (FR-007). |
| `DefaultTooltip` | `string` | Tells the operator how to make this mode the default. |
| `ShowDividerAfter` | `bool` | True between two mode chips, false after the last chip the module fills. |

**Ordering rule**: the group is drawn left to right as `Guided`, `Edit`, `Image Search` for
Non-Inventoried Labels (FR-004). Receiving's order is unchanged at `Guided`, `Manual`, `Edit`, `Overlay`.

## Action slot

One named position in the strip's right-hand group. A page-specific verb may occupy the Back or Next
position instead of the default action (FR-013, FR-017).

| Field | Type | Rule |
| --- | --- | --- |
| `IsVisible` | `bool` | False removes the control rather than disabling it, which is how a page says it cannot go back (FR-014). |
| `IsEnabled` | `bool` | False renders the control disabled but present, which is how Next waits on a page condition (FR-014). |
| `Label`, `Tooltip`, `AccessibilityName` | `string` | Settings-driven, with today's wording as the default (FR-022). |
| `Command` | `ICommand` | The command the page already used. The strip never supplies its own (FR-020). |
| `ShiftCommand` | `ICommand?` | The wider variant run when the control is activated with Shift held. Only Clear All Labels sets it (FR-015). |
| `Kind` | `Enum_ShellActionKind` | `Back` draws the danger colour, `Save` the success colour, `Plain` neither (FR-023). |
| `ShowDividerAfter` | `bool` | Drawn only when both Back and Next are shown (FR-013). |

**Per-page occupancy for Non-Inventoried Labels**:

| Page | Back | Next | Clear All Labels | Help |
| --- | --- | --- | --- | --- |
| Select Type | hidden | `Next` | shown | shown |
| Select Part | `Back` | `Next` | shown | shown |
| Enter Loads | `Back` | `Next` | shown | shown |
| Enter Details | `Back` | `Review`, kind `Plain` | shown | shown |
| Review & Save | `Back` | `Save`, kind `Save` | shown | shown |
| Edit Mode | hidden | `Save`, kind `Save` | shown | shown |
| Image Search | `Back` | hidden | shown | shown |

## User default entry mode

At most one stored value per user per module. It is already stored on the user record as
`Model_User.DefaultDunnageMode` and `Model_User.DefaultReceivingMode`; this feature narrows the Dunnage
one.

| Field | Type | Rule |
| --- | --- | --- |
| `DefaultDunnageMode` | `string?` | One of `guided`, `edit`, `image-search`, or absent. The value `manual` is retired (FR-011, FR-012). |

**Validation**:

- At most one value is stored, so storing one replaces whatever was there (FR-008).
- An absent value resolves to `guided`, and `guided` is written at the moment the module is opened
  (FR-009).
- A stored `manual`, or any value outside the three, resolves to `guided` and is overwritten with
  `guided` on the next open (FR-012).
- The write goes through `IService_UserPreferences.UpdateDefaultDunnageModeAsync` for the signed-in user
  and through no other path.

**Transitions**:

| From | Event | To |
| --- | --- | --- |
| absent | opening the module | `guided`, stored |
| `manual` | opening the module | `guided`, stored |
| `guided` | Ctrl and press Edit | `edit` |
| `edit` | Ctrl and press Image Search | `image-search` |
| `image-search` | Ctrl and press Guided | `guided` |
| any | plain press of a mode | unchanged |

A plain press changes the page and never the stored value. A Ctrl press changes the stored value and
never the page (FR-007).

## Step lifecycle

`Enum_DunnageWorkflowStep` after the change:

| Member | Value | Reachable | Page |
| --- | --- | --- | --- |
| `TypeSelection` | 0 | yes | Select Type, the guided entry |
| `PartSelection` | 1 | yes | Select Part |
| `QuantityEntry` | 2 | yes | Enter Loads |
| `DetailsEntry` | 3 | yes | Enter Details |
| `Review` | 4 | yes | Review & Save |
| `EditMode` | 5 | yes | Edit Mode |
| `ImagePartSearch` | 6 | yes | Image Search |

`ModeSelection` and `ManualEntry` are deleted. The values after them are renumbered, so nothing may
persist a step number across a session. No code does: the step is held in memory only.

**Entry rule**: opening the module always lands on the step the stored default names, and a guided
default lands on `TypeSelection`. No path lands on a mode-choosing step (FR-010).

## Search destination

One page the search box can open.

**Carrier**: the private `SearchDestination` inside `MainWindow`, with three kinds: `FrameRoute`,
`SettingsPage`, and `Command`.

| Field | Type | Rule |
| --- | --- | --- |
| `Key` | `string` | The route tag, the page type's full name, or `command:<text>`. Unique. |
| `Label` | `string` | What the operator sees in the result list. |
| `Kind` | `SearchDestinationKind` | Decides how the result is opened. |
| `RouteTag` | `string?` | Set for `FrameRoute`; must exist in `_navRoutes`. |
| `SettingsPageType` | `Type?` | Set for `SettingsPage`; must be registered in dependency injection. |
| `Detail`, `CommandText`, and aliases | `string` | Search terms only; never displayed behaviour. |

**Validation** (FR-030 to FR-033):

- Every route in `_navRoutes` except the disabled module has an entry. Every settings page registered
  and reachable from a settings hub has an entry. Every entry's target exists and can be opened.
- No entry names a step inside a guided workflow. A module's entry page is what is offered in its place.
- An entry guarded by an access rule is offered only to a user the rule allows, and is refused with the
  same message navigation shows when it is reached another way.
- Opening a result runs the same leave confirmation as navigation.

**Recorded exceptions**: the disabled Outside Service module has no entry, by the comment already in the
catalog. `AppDocumentation` resolves through the documentation launcher rather than a frame route and
keeps that path.
