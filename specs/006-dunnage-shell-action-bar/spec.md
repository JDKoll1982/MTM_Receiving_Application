# Feature Specification: Shell Action Bar for Non-Inventoried Labels

**Feature Branch**: `006-dunnage-shell-action-bar`

**Created**: 2026-11-21

**Status**: Draft

**Input**: User description: "read through 005-shell-action-bar as i want to now do the same thing to
Module_Dunnage", with clarifications: the mode group carries Guided, Edit, and Image Search; Manual
mode is removed from Non-Inventoried Labels entirely; the mode-selection page is deleted; the workflow
page's navigation row and every remaining in-page Save button move to the strip; the default mode is set
by Ctrl+click with a Default badge; strip copy comes from settings keys; plus help, clear-all-labels, a
settings-hub card repair, and an app-wide search-bar audit.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Every Non-Inventoried Labels page keeps its navigation in one known place (Priority: P1)

An operator works the Non-Inventoried Labels guided walk: choosing a dunnage type, choosing the part,
entering the loads and quantities, entering details, then reviewing and saving. Today the workflow page
draws one row of buttons beneath the content, and review, edit mode and manual entry each draw their own
save button, so the actions an operator needs are spread across two places and shift when a step changes.
After the change, one strip of controls sits under the page content on the side of the window away from
the navigation pane, and it holds the same controls in the same order on every Non-Inventoried Labels
screen.

**Why this priority**: It is the whole point of the feature. Without it there is no shared location, and
the mode controls, the page-specific save verbs, and the clear-labels control have nothing to sit in.

**Independent Test**: Walk the full guided Non-Inventoried Labels sequence for one part and confirm that
Back, Next, Clear All Labels and Help appear in the same strip, in the same order, on every screen, and
that the workflow page no longer draws a navigation row of its own.

**Acceptance Scenarios**:

1. **Given** the operator is on the Select Part screen, **When** the screen is shown, **Then** the strip
   shows the mode controls, Back, Next, Clear All Labels and Help, and the page itself draws no Back,
   Next, Mode Selection or Clear All Labels control.
2. **Given** the operator is on the first guided screen, **When** the screen is shown, **Then** the Back
   control is absent from the strip rather than shown disabled.
3. **Given** the operator is on a screen that offers no navigation, **When** the screen is shown,
   **Then** the strip collapses completely and the page content fills the full remaining height.
4. **Given** the window is narrowed to its smallest supported width, **When** the strip is shown,
   **Then** every control for that page stays reachable by wrapping or horizontal scrolling.
5. **Given** the operator holds Shift and activates Clear All Labels, **When** the activation completes,
   **Then** every non-inventoried label row is archived to history and cleared, exactly as the button it
   replaced did.

---

### User Story 2 - Non-Inventoried Labels opens straight into a mode (Priority: P1)

Today opening Non-Inventoried Labels lands on a screen that asks the operator to choose a mode before any
work can start. That screen goes away. Opening Non-Inventoried Labels starts the mode the operator last
chose as their default, and an operator who has never chosen one starts in Guided, which becomes their
stored default from that point on.

**Why this priority**: Removing the picker is what makes the shared strip possible, and it removes a whole
screen from every Non-Inventoried Labels session.

**Independent Test**: Delete any stored default for a test user, open Non-Inventoried Labels, and confirm
Guided starts immediately with no picker shown and Guided recorded as the default. Then sign in as a user
with a stored Edit default and confirm Edit starts immediately.

**Acceptance Scenarios**:

1. **Given** a user with no stored default, **When** they open Non-Inventoried Labels, **Then** Guided
   starts immediately and Guided is stored as that user's default.
2. **Given** a user whose stored default is Edit, **When** they open Non-Inventoried Labels, **Then**
   Edit starts immediately.
3. **Given** Non-Inventoried Labels is open, **When** the operator looks for a way to choose a mode,
   **Then** the mode controls are available in the strip and no mode-selection page exists anywhere in
   the application's navigation, settings, or help.
4. **Given** the application was last closed on the old mode-selection screen, **When** the operator
   reopens Non-Inventoried Labels, **Then** their default mode starts and no error or blank page appears.

---

### User Story 3 - Manual mode is gone from Non-Inventoried Labels (Priority: P1)

Manual mode is never used. It has already lost its button on the mode-selection screen, but the mode, its
screen, its step, and its settings copy still exist, so a stored default of manual, a keyboard route, or
a stale search result can still reach it. After the change nothing in the application enters, offers,
names, or reaches manual mode for Non-Inventoried Labels.

**Why this priority**: Leaving a dead mode in place means the strip would have to carry a mode nobody
uses, and any leftover route would open a screen with no owner.

**Independent Test**: Search the application for any way to reach manual mode in Non-Inventoried Labels,
open Non-Inventoried Labels as a user whose stored default is manual, and confirm the mode, its screen
and its settings entry are all absent and Guided starts instead.

**Acceptance Scenarios**:

1. **Given** a user whose stored default is manual, **When** they open Non-Inventoried Labels, **Then**
   Guided starts and the stored value is corrected to Guided.
2. **Given** the strip's mode controls are shown, **When** the operator looks at them, **Then** only
   Guided, Edit and Image Search are offered.
3. **Given** any settings page for Non-Inventoried Labels, **When** it is shown, **Then** no option,
   description, or help topic offers manual mode.
4. **Given** the guided walk, **When** the operator advances through it, **Then** no step leads to the
   former manual entry screen and no screen for it can be opened.

---

### User Story 4 - Deciding which mode is my default, from the strip (Priority: P2)

The three mode controls live in the strip: Guided, Edit, and Image Search. Pressing one enters that mode
immediately. An operator holds Ctrl and presses one to make it their default, and the word Default
appears beneath the mode that holds it. Only one default can ever be stored.

**Why this priority**: It replaces the only useful thing the deleted picker did, and it depends on User
Story 1's strip existing.

**Independent Test**: Ctrl+click Edit, close and reopen the application, and confirm Edit starts. Then
Ctrl+click Image Search and confirm the Default badge moves and only Image Search is stored.

**Acceptance Scenarios**:

1. **Given** no mode is marked as default, **When** the operator Ctrl+clicks Edit, **Then** Edit becomes
   the single stored default and the page does not change.
2. **Given** Edit is the stored default, **When** the operator Ctrl+clicks Image Search, **Then** the
   Default badge moves under Image Search and Edit is no longer stored.
3. **Given** Guided is the stored default, **When** the operator presses Image Search without Ctrl,
   **Then** Image Search opens and no default changes.
4. **Given** a non-Non-Inventoried-Labels module is active, **When** its page is shown, **Then** none of
   the three Non-Inventoried Labels mode controls or their badges appear.
5. **Given** the operator holds Ctrl and presses the mode already in force, **When** the press completes,
   **Then** that mode is stored as the default and the workflow does not restart.

---

### User Story 5 - The primary action for a page lands in the expected slot (Priority: P2)

Some Non-Inventoried Labels pages do not advance or go back: they save or hand off. Enter Details offers
Review, Review offers Save, Edit Mode offers Save, and the Image Search screen hands a chosen part back
into the guided part step. Each of those verbs occupies the slot where Next or Back would normally sit, so
the operator finds the primary action in the same place they expect.

**Why this priority**: It stops the shared strip from overwriting page-specific verbs, and it is what lets
the pages give up their own save buttons without losing the action.

**Independent Test**: Open Enter Details and confirm Review appears where Next normally sits and the page
draws no Review button. Repeat for Review and Save, and for Edit Mode.

**Acceptance Scenarios**:

1. **Given** Enter Details is open, **When** the page is shown, **Then** the strip shows the mode
   controls, Back, Review in the Next slot, Clear All Labels and Help, and the page shows no Review
   button.
2. **Given** Review is open, **When** the page is shown, **Then** the strip shows Save in the Next slot
   and the page shows no Save button.
3. **Given** Edit Mode is open, **When** the page is shown, **Then** the strip shows Save in the Next
   slot, the editing toolbar stays on the page, and no Save button remains on the page.
4. **Given** Image Search is open, **When** the operator chooses a part, **Then** the guided part step
   opens with that part selected and the operator can advance from the strip.
5. **Given** a save attempted from the strip fails, **When** the failure is reported, **Then** the
   operator stays on the same page with their data intact.

---

### User Story 6 - The strip's words stay editable (Priority: P3)

Every label, tooltip and hint the strip shows for Non-Inventoried Labels is stored as configuration, so a
manager can reword a control without a code change, and the shipped wording matches what the operator saw
before this change.

**Why this priority**: It keeps parity with how the Receiving strip is configured, so both modules are
maintained the same way.

**Independent Test**: Change one strip label in settings, reopen Non-Inventoried Labels, and confirm the
strip shows the new text.

**Acceptance Scenarios**:

1. **Given** default settings, **When** the strip is shown, **Then** every label and tooltip matches the
   wording the replaced controls used.
2. **Given** a strip label is changed in settings, **When** the strip is next shown, **Then** the new
   text appears without an application rebuild.

---

### User Story 7 - Settings cards open the page they name (Priority: P3)

Every card on the Non-Inventoried Labels settings hub, and every card on the Receiving settings hub,
opens the page it names. A card that opens a different page, or nothing at all, makes the whole hub
untrustworthy.

**Why this priority**: The hub is the only way into each module's settings, so a wrong or dead card costs
the operator the page they came for. The Receiving hub is included because the same fault was reported
there.

**Independent Test**: Open each settings hub and click each card in turn, then confirm each opens the page
named on it.

**Acceptance Scenarios**:

1. **Given** the Non-Inventoried Labels settings hub, **When** any card is clicked, **Then** the page
   named on that card opens.
2. **Given** the Receiving settings hub, **When** any card is clicked, **Then** the page named on that
   card opens.
3. **Given** a settings page that does not exist, **When** either hub is shown, **Then** no card offers
   it.

---

### User Story 8 - Search finds every page I may open, and only those (Priority: P3)

The application's search box is the fastest way to a page, but its list of destinations has drifted: some
pages are missing, some are offered that no longer exist or are switched off, and some entries lead into
the middle of a guided workflow. After the change the search box covers the whole application, offers
only pages that exist, never drops the operator into the middle of a guided sequence, and applies the
same permission check the navigation pane applies.

**Why this priority**: A search box that finds the wrong pages, or opens a page the operator may not use,
is worse than no search box at all, and the fault affects every module.

**Independent Test**: Compare the search results against everything reachable from the navigation pane and
both settings hubs, open a few results, and confirm that a page guarded by an access rule is refused from
search exactly as it is from the navigation pane.

**Acceptance Scenarios**:

1. **Given** the application's navigation pane and settings hubs, **When** an operator searches for any
   page reachable from them, **Then** that page appears in the results and opens.
2. **Given** a page that no longer exists or is switched off, **When** an operator searches for it,
   **Then** it does not appear.
3. **Given** a guided workflow, **When** an operator searches for one of its inner steps, **Then** no
   result offers that step, and the module's entry page is offered instead.
4. **Given** a page guarded by an access rule, **When** an operator who is not permitted searches for it,
   **Then** it does not open and the same refusal message appears that the navigation pane shows.
5. **Given** a page guarded by an access rule, **When** a permitted operator searches for it, **Then** it
   opens normally.

### Edge Cases

- A user whose stored default is manual must land on Guided, and the stored value must be corrected
  rather than left wrong in the database.
- Pressing the mode control for the mode already active must not restart the workflow or discard work
  already entered.
- Pressing Clear All Labels when there is nothing to clear must leave the control unavailable rather than
  fail.
- Next must stay unavailable on Select Part until a part is chosen and on Enter Loads until the entry is
  valid, exactly as today.
- A page that offers Help but has no help topic for the current context must not open an empty help
  surface.
- Edit Mode must keep its editing toolbar (load, select all, remove, columns, filters, paging) while
  losing its save button.
- Any control that moved into the strip must have exactly one place it can be activated from, so a
  keyboard shortcut cannot reach an action the strip has hidden or disabled.
- Restarting the application while it was last left on the deleted mode-selection screen must land on a
  valid page.
- The Image Search screen must not be reachable as a mode selection substitute when the operator has
  unsaved guided work; leaving guided work must ask first, as it does today.
- A search entry for a module that is switched off must be absent, and switching the module back on must
  restore it without a code change.
- Searching while a guided workflow holds unsaved work must not silently discard it; the same leave
  confirmation that navigation uses must apply.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The application MUST present every Non-Inventoried Labels page's navigation actions in the
  one shared strip that already serves Receiving, pinned directly beneath the page content on the side of
  the window opposite the navigation pane.
- **FR-002**: The strip MUST size to its content and MUST collapse entirely so the page content takes the
  full remaining height when the active page supplies no actions.
- **FR-003**: At compact window widths the strip MUST wrap or scroll horizontally; it MUST NOT hide any
  action the current page offers.
- **FR-004**: The strip's mode group MUST offer exactly Guided, Edit, and Image Search for
  Non-Inventoried Labels pages, in that order, and MUST be absent on every page that belongs to neither
  Receiving nor Non-Inventoried Labels.
- **FR-005**: A module's mode group MUST show that module's own modes with that module's own labels; the
  strip MUST NOT show Receiving's modes on a Non-Inventoried Labels page or the reverse.
- **FR-006**: Pressing a mode control MUST enter that mode immediately, reproducing what the deleted
  mode-selection page did, without restarting a workflow the operator is already in.
- **FR-007**: Ctrl+pressing a mode control MUST store that mode as the signed-in user's default without
  changing the page, and MUST mark it with the word Default beneath that mode's control.
- **FR-008**: At most one Non-Inventoried Labels mode default MUST be stored. Ctrl+pressing a different
  mode MUST replace it.
- **FR-009**: When no Non-Inventoried Labels default is stored, the application MUST treat Guided as the
  default and MUST store Guided at the moment Non-Inventoried Labels is opened.
- **FR-010**: Opening Non-Inventoried Labels MUST go directly to the user's stored default mode. No
  mode-selection page or mode-selection step MUST remain in navigation, settings, or help.
- **FR-011**: Manual mode MUST NOT exist for Non-Inventoried Labels: no control MUST offer it, no step or
  screen MUST implement it, and no help or settings text MUST name it.
- **FR-012**: A stored default of manual MUST resolve to Guided and MUST be corrected in storage on the
  next open.
- **FR-013**: The strip's right group MUST offer Back, then Next, then a divider, then Clear All Labels,
  then Help, in that order, with the divider drawn only when both Back and Next are shown.
- **FR-014**: Back MUST appear only on pages that support going back, and Next MUST appear only on pages
  that support advancing and MUST be unavailable until that page's own conditions are met.
- **FR-015**: Clear All Labels MUST archive the active non-inventoried label queue to history under the
  existing rules, and MUST take the wider clear when activated with Shift held.
- **FR-016**: Help MUST open help for the current page.
- **FR-017**: Each page MUST show exactly the actions agreed for it in the per-page matrix below, and
  nothing else.

| Page | Actions offered |
| --- | --- |
| Select Type | Guided, Edit, Image Search, Next, Clear All Labels, Help |
| Select Part | Guided, Edit, Image Search, Back, Next, Clear All Labels, Help |
| Enter Loads | Guided, Edit, Image Search, Back, Next, Clear All Labels, Help |
| Enter Details | Guided, Edit, Image Search, Back, Review in the Next slot, Clear All Labels, Help |
| Review & Save | Guided, Edit, Image Search, Back, Save in the Next slot, Clear All Labels, Help |
| Edit Mode | Guided, Edit, Image Search, Save in the Next slot, Clear All Labels, Help |
| Image Search | Guided, Edit, Image Search, Back, Clear All Labels, Help |
| Any page outside both modules | Only what that page's own contract offers; nothing when it offers nothing |

- **FR-018**: Every action the strip owns MUST be removed from the page that previously drew it, leaving
  no duplicate control and no second binding of the same command. This includes the workflow page's whole
  navigation row and the save buttons on Review, Edit Mode, and any other Non-Inventoried Labels screen.
- **FR-019**: Editing toolbars and non-navigation controls on Edit Mode MUST stay on their page and MUST
  NOT be duplicated in the strip.
- **FR-020**: The strip MUST invoke the same actions the pages already used; it MUST NOT re-implement a
  page's behaviour.
- **FR-021**: Existing Back, Next and save keyboard shortcuts MUST keep working and MUST respect exactly
  the same visible and enabled rules as the strip's controls.
- **FR-022**: All user-editable text for these actions, including button labels and tooltips, MUST come
  from Non-Inventoried Labels settings keys whose defaults hold the exact wording the replaced controls
  used.
- **FR-023**: Back and its icon MUST be drawn in the application's danger colour, whichever save verb
  fills the Next slot MUST be drawn in the application's success colour, and both colours MUST follow
  whichever theme is active rather than being fixed values.
- **FR-024**: Clear All Labels MUST be drawn as an icon alone, and Help as an icon alone. Both MUST keep
  their tooltip and their screen-reader name.
- **FR-025**: The mode in force MUST be marked with success-green text and a success-green border, and
  the modes not in force MUST carry neither.
- **FR-026**: The shared shell MUST learn what to show only from the active page's own description of
  its actions, so no module's internals are known to the shared shell.
- **FR-027**: Leave confirmation MUST keep working when the operator changes mode, opens another page, or
  enters settings while a workflow holds unsaved work.
- **FR-028**: Every card on the Non-Inventoried Labels settings hub MUST open the page named on it,
  resolved by that name rather than by its position in the list, and the hub MUST NOT offer a card for a
  page that does not exist.
- **FR-029**: Every card on the Receiving settings hub MUST open the page named on it, resolved by that
  name rather than by its position in the list, and the hub MUST NOT offer a card for a page that does
  not exist.
- **FR-030**: The application's search box MUST offer a destination for every page reachable from the
  navigation pane and from either settings hub, in every module.
- **FR-031**: The search box MUST NOT offer a destination for a page that no longer exists, is switched
  off, or is a step inside a guided workflow. Searching for an inner step MUST offer the module's entry
  page instead.
- **FR-032**: The search box MUST apply the same permission check the navigation pane applies, and MUST
  show the same refusal when the operator is not permitted, rather than opening the page.
- **FR-033**: Selecting a search result MUST run the same leave confirmation that navigation uses, so a
  workflow holding unsaved work cannot be abandoned by searching.
- **FR-034**: Page behaviour MUST stay behind each page's view model; view code-behind MUST hold wiring
  only.
- **FR-035**: Pages outside Receiving and Non-Inventoried Labels MUST keep their current behaviour apart
  from gaining a strip only when they offer actions.

### Key Entities

- **Page action set**: the collection of navigation actions a single page offers, each with a visible
  state and an enabled state. It is supplied by the page, not known by the shell.
- **Mode group**: the entry modes of the module that owns the active page, each with a label, a tooltip,
  a command, and whether it holds the user's default.
- **Action slot**: one of the named positions in the strip: the mode group on the left, and Back, Next,
  Clear All Labels, and Help on the right. A page-specific verb may occupy the Back or Next slot instead
  of the default action.
- **User default entry mode**: at most one value per user per module, one of guided, edit, or image
  search for Non-Inventoried Labels, or absent when the user has not chosen. Absent always resolves to
  guided.
- **Search destination**: one page the search box can open, carrying the module it belongs to and whether
  it is an entry page rather than an inner step.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100 percent of Non-Inventoried Labels pages show their navigation actions in the same
  strip, in the same order, in the same on-screen region.
- **SC-002**: Zero Non-Inventoried Labels pages draw their own Back, Next, Mode Selection, Clear All
  Labels, or save control after the change.
- **SC-003**: An operator with no stored default reaches the first guided entry step in one action from
  opening Non-Inventoried Labels; today that takes two.
- **SC-004**: A chosen default survives an application restart in 100 percent of attempts, and at no
  point is more than one default stored for a user.
- **SC-005**: Zero ways remain to enter manual mode for Non-Inventoried Labels, including controls,
  shortcuts, settings entries and search results.
- **SC-006**: At the smallest supported window width, every action offered by the current page remains
  reachable; zero actions become unreachable because of the strip.
- **SC-007**: A spot check of every settings hub card opens the page named on it, with zero dead or
  wrong cards across both modules.
- **SC-008**: A comparison of search results against the navigation pane and both settings hubs finds
  zero pages missing from search and zero search results pointing at a page that no longer exists.
- **SC-009**: Zero search results lead into the middle of a guided workflow.
- **SC-010**: A page guarded by an access rule is refused from search in 100 percent of attempts by a
  user who is not permitted, with the same message navigation shows.
- **SC-011**: The application builds with zero errors and the existing automated test suite passes.

## Assumptions

- The shared strip introduced for Receiving is the surface this feature extends; no second strip is
  built, and its positioning, collapse behaviour, and keyboard shortcuts carry over unchanged.
- Non-Inventoried Labels keeps three modes after this change. The existing Image Search mode becomes the
  third mode control in place of Receiving's Overlay control.
- Manual mode has already lost its mode-selection button, so its removal is a clean deletion rather than
  a migration with user-visible warnings beyond the stored-default correction.
- A user whose stored value is manual is corrected to Guided silently, because Guided is what an absent
  default already resolves to.
- The Receiving settings hub repair is limited to cards that fail to open the page they name; no Receiving
  settings page gains or loses functionality.
- "Mid-guided workflow" means any step reached only by advancing within a module's guided sequence. A
  module's entry page is always what the search box offers.
- Access rules for search are the ones already enforced on navigation, including the plant-wide allow
  list that guards the scanner page. This feature adds no new permission model.
- The strip's copy defaults are the wording already on screen today, so no operator-facing text changes
  as a side effect.

## Verbatim Constraints

- The three Non-Inventoried Labels mode labels are exactly `Guided`, `Edit`, and `Image Search`.
- The three stored default values are exactly `guided`, `edit`, and `image-search`.
- The clear control's label is exactly `Clear All Labels`.
- The default marker is exactly the word `Default`, shown beneath the mode control that holds it.
- A saved entry's review verb is exactly `Review`, and a save verb is exactly `Save`.
- The leave-confirmation and access-refusal messages reuse the existing application strings rather than
  introducing new wording.
