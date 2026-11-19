# Feature Specification: Persistent Shell Action Bar for Page Navigation

**Feature Branch**: `005-shell-action-bar`

**Created**: 2026-11-19

**Status**: Draft

**Input**: User description: "Move page navigation out of the Receiving workflow pages into a persistent
bottom action bar on the main window, and delete the Receiving mode-selection page."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Every page keeps its navigation in one known place (Priority: P1)

A warehouse operator works through Receiving: entering a PO number, choosing a part, entering load
details, weight and quantity, heat and lot, and package type, then reviewing and saving. On every
one of those screens the operator needs the same things in the same spot: go back, go forward, clear
the label data collected so far, and get help. Today each screen draws its own row of buttons, so
Back and Next move around and the operator has to hunt for them. After the change, one strip of
controls sits under the page content on the side of the window away from the navigation pane, and
it holds the same controls in the same order on every screen.

**Why this priority**: It is the whole point of the feature. Without it there is no shared location,
and every later story (mode buttons, page-specific save verbs) has nothing to sit in.

**Independent Test**: Walk the full guided Receiving sequence with one PO and confirm that Back,
Next, Clear Label Data, and Help appear in the same strip, in the same order, on every screen, and
that no screen still draws a navigation row of its own.

**Acceptance Scenarios**:

1. **Given** the operator is on the Select Part screen, **When** the screen is shown, **Then** Back,
   Next, Clear Label Data, and Help appear in the bottom strip beneath the page content, and the
   page itself contains no Back, Next, or Clear Label Data button.
2. **Given** the operator is on a screen that cannot go back, **When** the screen is shown, **Then**
   the Back control is absent from the strip rather than shown disabled.
3. **Given** the operator is on a page that offers no navigation at all, **When** the page is shown,
   **Then** the strip collapses completely and the page content fills the full remaining height.
4. **Given** the window is narrowed to its smallest supported width, **When** the strip is shown,
   **Then** every control for that page stays reachable by wrapping or horizontal scrolling.
5. **Given** the operator uses the Back or Next keyboard shortcut, **When** that action is not
   available on the current page, **Then** nothing happens, exactly as if the control were not there.

---

### User Story 2 - Receiving opens straight into a mode, with no picker in the way (Priority: P1)

Today opening Receiving lands on a screen that asks the operator to choose Guided, Manual, Edit, or
Overlay before any work can start. That screen goes away. Opening Receiving starts the mode the
operator last chose as their default, and an operator who has never chosen one starts in Guided,
which becomes their stored default from that point on.

**Why this priority**: Removing the picker is what makes the shared strip possible, and it removes a
whole screen from every Receiving session.

**Independent Test**: Delete any stored default for a test user, open Receiving, and confirm Guided
starts immediately with no picker shown and the default recorded as Guided. Then sign in as a user
with a stored Manual default and confirm Manual starts immediately.

**Acceptance Scenarios**:

1. **Given** a user with no stored default, **When** they open Receiving, **Then** Guided starts
   immediately and Guided is stored as that user's default.
2. **Given** a user whose stored default is Manual, **When** they open Receiving, **Then** Manual
   starts immediately.
3. **Given** Receiving is open, **When** the user looks for a way to choose a mode, **Then** the
   four mode controls are available in the strip and no mode-selection page exists anywhere in the
   application's navigation, settings, or help.
4. **Given** the application was last closed on the old mode-selection screen, **When** the user
   reopens Receiving, **Then** their default mode starts and no error or blank page appears.

---

### User Story 3 - Deciding which mode is my default, from the strip (Priority: P2)

The four mode controls live in the strip: Guided, Manual, Edit, and Overlay. Pressing one enters
that mode immediately. Beside Guided, Manual, and Edit sits a small unlabelled checkbox with a
tooltip explaining it makes that mode the default. Ticking one remembers that mode for next time and
clears the other two, so only one default can ever be stored.

**Why this priority**: It replaces the only thing the deleted picker still did usefully, and it
depends on User Story 1's strip existing.

**Independent Test**: Tick Manual's checkbox, close and reopen the application, and confirm Manual
starts. Then tick Edit's checkbox and confirm Manual's is cleared and only Edit is stored. Then
untick Edit's and confirm the next open falls back to Guided.

**Acceptance Scenarios**:

1. **Given** Guided, Manual, and Edit all have unticked checkboxes, **When** the user ticks Manual's
   checkbox, **Then** Manual becomes the single stored default and no mode change occurs from the
   tick itself.
2. **Given** Manual is the stored default, **When** the user ticks Edit's checkbox, **Then**
   Manual's checkbox clears, Edit's is ticked, and the stored default becomes Edit.
3. **Given** Edit is the stored default, **When** the user unticks Edit's checkbox and leaves no
   other ticked, **Then** no default is stored and the next open starts Guided.
4. **Given** the strip's mode controls are visible, **When** the user presses Overlay, **Then** the
   label overlay window opens and no default is changed.
5. **Given** a non-Receiving module is active, **When** its page is shown, **Then** none of the four
   mode controls or their checkboxes appear.

---

### User Story 4 - The primary action for a page lands in the expected slot (Priority: P2)

Some Receiving pages do not advance or go back at all: they save. Manual Entry and Edit Mode save and
finish; Review and Save writes to the database; the Result page starts a new entry. Each of those
verbs now occupies the slot where Next or Back would normally sit, so the operator still finds the
primary action in the same place they expect.

**Why this priority**: It stops the shared strip from overwriting page-specific verbs, and it is what
lets the pages give up their own save buttons without losing the action.

**Independent Test**: Open Manual Entry and confirm Save and Finish appears where Next normally sits
and the page no longer draws a Save button. Repeat for Edit Mode, Review and Save (Save to Database),
and the Result page (Start New Entry in the Back slot, Overlay present).

**Acceptance Scenarios**:

1. **Given** Manual Entry is open, **When** the page is shown, **Then** the strip shows the four mode
   controls, Clear Label Data, Help, and Save and Finish in the Next slot, and the page shows no Save
   button.
2. **Given** Edit Mode is open, **When** the page is shown, **Then** the strip shows Save and Finish
   in the Next slot, the paging controls stay on the page, and no Save button remains on the page.
3. **Given** Review and Save is open, **When** the page is shown, **Then** the strip shows Save to
   Database in the Next slot and no Back, and the page shows no Add another Part/PO button.
4. **Given** the Result page is shown after a successful save, **When** the page is shown, **Then**
   Start New Entry occupies the Back slot, Overlay is present, and no Mode Selection action exists.
5. **Given** the save attempted from the strip fails, **When** the failure is reported, **Then** the
   operator stays on the same page with their data intact.

---

### User Story 5 - Other modules are left alone (Priority: P3)

Operators also use Reconciliation, Dunnage, Reporting, Settings, and Volvo features. Those pages get
no mode controls, and they inherit the shared strip only if their page chooses to offer actions. When
they offer nothing, the strip disappears and the page uses the full height.

**Why this priority**: It protects every module outside Receiving from an accidental behaviour change
while the shared strip is introduced.

**Independent Test**: Visit one page from each other module and confirm no mode controls appear, and
that pages offering no actions show no strip and full-height content.

**Acceptance Scenarios**:

1. **Given** any non-Receiving page is active, **When** the page is shown, **Then** no Guided,
   Manual, Edit, or Overlay control appears in the strip.
2. **Given** a non-Receiving page offers no actions, **When** the page is shown, **Then** the strip is
   absent and the page content occupies the full remaining height.
3. **Given** the application labels shortcuts footer is visible in the navigation pane, **When** the
   window is resized to any width, **Then** that footer stays in the navigation pane and is never
   merged into the strip.

### Edge Cases

- A user unticking the only ticked default leaves no stored default; the next open must resolve to
  Guided and store Guided again rather than leaving the mode undecided.
- Pressing the mode control for the mode already active must not restart the workflow or discard
  label data already collected.
- Pressing Clear Label Data when there is nothing to clear must leave the control unavailable rather
  than fail.
- Next must stay unavailable on the first guided step until that PO's part list has loaded, and on
  the part step until a part has been chosen, exactly as today.
- Clear Label Data on Manual Entry and Edit Mode must follow the same rules as the guided walk,
  including when the page holds unsaved edits.
- A page that offers Help but has no help topic for the current context must not open an empty help
  surface.
- The label overlay window must not open twice when Overlay is pressed while it is already open.
- Manual Entry and Edit Mode must not lose their editing toolbars (add row, add multiple rows, remove
  row, auto fill, file/paging/filter controls) while losing their save button.
- Any control that moved into the strip must have exactly one place it can be activated from, so a
  keyboard shortcut cannot reach an action the strip has hidden or disabled.
- Restarting the application while it was last left on a page that no longer exists must land on a
  valid page.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The application MUST present every page's navigation actions in one shared strip, pinned
  directly beneath the page content on the side of the window opposite the navigation pane.
- **FR-002**: The strip MUST NOT replace, merge with, or relocate the label-shortcut footer that lives
  in the left navigation pane; the two surfaces MUST never share space.
- **FR-003**: The strip MUST size to its content, and MUST collapse entirely so the page content takes
  the full remaining height when the active page supplies no actions.
- **FR-004**: At compact window widths the strip MUST wrap or scroll horizontally; it MUST NOT hide
  any action the current page offers.
- **FR-005**: The strip's left group MUST offer the four entry-mode controls Guided, Manual, Edit, and
  Overlay in that order with a divider between each, and MUST be absent on every non-Receiving page.
- **FR-006**: Guided, Manual, and Edit MUST each carry an unlabelled default checkbox with an
  explanatory tooltip; the checkbox MUST only persist the per-user default and MUST NOT itself change
  the active page or mode.
- **FR-007**: At most one mode default MUST be stored. Ticking one checkbox MUST clear the other two,
  and unticking the last ticked one MUST be allowed and MUST leave no stored default.
- **FR-008**: The strip's right group MUST offer Back, then Next, then a divider, then Clear Label
  Data, then Help, in that order.
- **FR-009**: Back MUST appear only on pages that support going back, and Next MUST appear only on
  pages that support advancing and MUST be unavailable until that page's own conditions are met.
- **FR-010**: Clear Label Data MUST clear the current user's label data under the existing rules and
  MUST be offered on every guided Receiving page, on Manual Entry, and on Edit Mode.
- **FR-011**: Help MUST open help for the current page.
- **FR-012**: Each page MUST show exactly the actions agreed for it in the per-page matrix below, and
  nothing else.

| Page | Actions offered |
| --- | --- |
| Enter PO Number | Guided, Manual, Edit, Overlay, Next, Clear Label Data, Help |
| Select Part | Guided, Manual, Edit, Overlay, Back, Next, Clear Label Data, Help |
| Enter Load Information | Guided, Manual, Edit, Overlay, Back, Next, Clear Label Data, Help |
| Enter Weight & Quantity | Guided, Manual, Edit, Overlay, Back, Next, Clear Label Data, Help |
| Enter Heat & Lot | Guided, Manual, Edit, Overlay, Back, Next, Clear Label Data, Help |
| Enter Package Type | Guided, Manual, Edit, Overlay, Back, Next, Clear Label Data, Help |
| Manual Entry | Guided, Manual, Edit, Overlay, Clear Label Data, Help, Save & Finish in the Next slot |
| Edit Mode | Guided, Manual, Edit, Overlay, Clear Label Data, Help, Save & Finish in the Next slot |
| Review & Save | Guided, Manual, Edit, Overlay, Back, Clear Label Data, Help, Save to Database in the Next slot |
| Result (complete or failed) | Guided, Manual, Edit, Overlay, Clear Label Data, Help, Start New Entry in the Back slot |
| Any non-Receiving page | Only what that page's own contract offers; nothing when it offers nothing |

- **FR-013**: Opening Receiving MUST go directly to the user's stored default mode. No mode-selection
  page or mode-selection step MUST remain in navigation, settings, or help.
- **FR-014**: When a user has no stored default, Receiving MUST open in Guided and MUST store Guided
  as that user's default at that moment.
- **FR-015**: Pressing a mode control MUST enter that mode immediately, reproducing what the deleted
  mode-selection page did.
- **FR-016**: Every action the strip owns MUST be removed from the page that previously drew it,
  leaving no duplicate control and no second binding of the same command.
- **FR-017**: The shell MUST NOT reference any module's view-model types. The decision of which
  actions are visible and enabled MUST come from a shell-owned contract that a page's view model
  implements or supplies, registered through the existing dependency-injection composition root.
- **FR-018**: Review and Save MUST offer Save to Database in the Next slot and MUST NOT offer an Add
  another Part/PO action anywhere.
- **FR-019**: The Result page MUST offer Start New Entry in the Back slot and MUST include Overlay.
- **FR-020**: Manual Entry and Edit Mode MUST offer Save & Finish in the Next slot and MUST NOT show a
  Save & Finish control of their own.
- **FR-021**: The shell header back control MUST stay hidden on every page; Back MUST be offered only
  by the strip. The existing header back capability MUST remain wired so re-enabling it later is a
  visibility change rather than new plumbing.
- **FR-022**: Existing Back and Next keyboard shortcuts MUST keep working and MUST respect exactly the
  same visible and enabled rules as the strip's controls.
- **FR-023**: Editing toolbars and non-navigation controls on Manual Entry and Edit Mode (add row, add
  multiple rows, remove row, auto fill, and file, paging, and filter controls) MUST stay on their
  pages and MUST NOT be duplicated in the strip.
- **FR-024**: The strip MUST invoke the same actions the pages already used; it MUST NOT re-implement
  a page's behaviour.
- **FR-025**: All user-editable text for these actions, including button labels and tooltips, MUST
  remain configurable through the existing Receiving settings keys and settings service.
- **FR-026**: The guided walk's receipt header card and step content MUST be unchanged apart from the
  removal of its in-page navigation row.
- **FR-027**: No business logic may be added to view code-behind; views MUST bind to view models with
  compiled bindings only.
- **FR-028**: Pages outside Receiving MUST keep their current behaviour apart from receiving no mode
  controls and gaining a strip only when they offer actions.

### Key Entities

- **Page action set**: the collection of navigation actions a single page offers, each with a visible
  state and an enabled state. It is supplied by the page, not known by the shell.
- **Action slot**: one of the named positions in the strip: the mode group on the left, and Back,
  Next, Clear Label Data, and Help on the right. A page-specific verb may occupy the Back or Next
  slot instead of the default action.
- **User default receiving mode**: at most one value per user, one of guided, manual, or edit, or
  absent when the user has not chosen. Absent always resolves to guided.
- **Receiving page**: one screen of the guided walk, or one of the standalone modes (Manual Entry,
  Edit Mode, Review and Save, Result). Each has its own action set.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100 percent of Receiving pages show their navigation actions in the same strip, in the
  same order, in the same on-screen region.
- **SC-002**: Zero Receiving pages draw their own Back, Next, Clear Label Data, Help, or mode-selection
  control after the change.
- **SC-003**: An operator with no stored default reaches the first guided data-entry step in one
  action from opening Receiving; today that takes two.
- **SC-004**: A chosen default survives an application restart in 100 percent of attempts, and at no
  point is more than one default stored for a user.
- **SC-005**: At the smallest supported window width, every action offered by the current page remains
  reachable; zero actions become unreachable because of the strip.
- **SC-006**: The application builds with zero errors and the existing automated test suite passes.
- **SC-007**: A spot check of one page in each non-Receiving module finds zero mode controls.
- **SC-008**: Zero user-facing labels or tooltips introduced by this change are hard-coded where the
  Receiving settings already make them configurable.

## Assumptions

- Pressing a mode control changes only the active mode for the current session; only the checkbox
  changes the stored default. The sole exception is the first-open Guided fallback, which stores
  Guided because the application has to resolve a mode anyway.
- The label overlay window's contents are unchanged; only the trigger moves into the strip.
- Default mode is stored per user on this workstation using the existing user-preference storage;
  no new storage mechanism is introduced.
- Guided remains the fallback because it is the behaviour the deleted picker defaulted to.
- The header step bar stays as it is; only the header back control is hidden.
- Dunnage has its own separate mode-selection screens; they are out of scope and unchanged.
- The existing Receiving settings keys already cover the labels and tooltips the strip needs.
- A page that offers no actions is a valid state, not an error.
- Keyboard shortcuts are the ones that exist today for Back and Next; no new shortcuts are added.

## Verbatim Constraints

Exact UI copy the result must match, in backticks, as written:

- Action labels: `Guided`, `Manual`, `Edit`, `Overlay`, `Back`, `Next`, `Clear Label Data`, `Help`,
  `Save & Finish`, `Save to Database`, `Start New Entry`.
- Default-checkbox tooltip text: `make Guided my default`, `make Manual my default`,
  `make Edit my default`.
- Labels that must no longer appear anywhere: `Mode Selection`, `Add another Part/PO`.
- Page names as used for per-page availability: `Enter PO Number`, `Select Part`,
  `Enter Load Information`, `Enter Weight & Quantity`, `Enter Heat & Lot`, `Enter Package Type`,
  `Manual Entry`, `Edit Mode`, `Review & Save`, `Result`.
- Stored default mode values: `guided`, `manual`, `edit`.

Shell and page elements the work must respect:

- Shell elements that stay: `ContentFrame`, `HeaderBarBorder`, `HeaderBackButton`, `HeaderStepBar`,
  `UserMenuButton`, `PrimaryLabelButtonsPanel`.
- Existing resolve-and-track pattern to follow: `IViewModel_HeaderTitleProvider`,
  `IViewModel_HeaderStepBarProvider`, `MainWindow.UpdateHeader`, `MainWindow.TrackHeaderStepBar`.
- Header back capability that stays wired but hidden: `IService_HeaderBackNavigation`.
- In-page navigation row to delete: `ReceivingWorkflowNavigationGrid` in
  `Module_Receiving/Views/View_Receiving_Workflow.xaml`.
- Mode-selection page to delete: `Module_Receiving/Views/View_Receiving_ModeSelection.xaml` and
  `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs`, plus the `ModeSelection`
  workflow step, its step-bar entry, and `ReturnToModeSelectionCommand`.
- Existing actions the strip must invoke rather than re-implement: `PreviousStepCommand`,
  `NextStepCommand`, `ResetLabelDataAsync`, `CanClearLabelData`, `ShowHelpAsync`.
- Existing services the work must reuse: `IService_UserPreferences.UpdateDefaultReceivingModeAsync`,
  `IService_ReceivingWorkflow.IsPoPartListLoaded`.
- Overlay window: `View_Receiving_LabelOverlayWindow`.
- Settings surface that must hold the text: `ReceivingSettingsKeys`, `IService_ReceivingSettings`.
- Dependency-injection registration location: `Infrastructure/DependencyInjection`, never
  `App.xaml.cs`.
- Keyboard shortcuts that must keep working: `Ctrl+Left`, `Ctrl+Right`.
