# Feature Specification: Heat / Lot Management

**Feature Branch**: `001-heat-lot-management`
**Created**: 2026-10-14
**Status**: Draft
**Input**: The approved module feature document `Module_ShipRec_Tools/docs/HeatLotManagement_FeatureUpdate.md`

## Overview

Material handlers must be able to tell one lot from another at a glance, without reading and
comparing long heat numbers. This feature gives every unique lot a simple visual marking made of one
shape and one color. The marking is chosen automatically, is the same everywhere it appears, and
never changes once a lot has been seen.

## User Scenarios & Testing

### User Story 1 - A lot gets its marking automatically (Priority: P1)

When material is received, the lot already carries a heat number. Today that number is the only way
to tell one lot from another, so a handler has to read and compare digits. With this feature the lot
also gets a shape and a color, chosen by the system the moment the lot is first seen, and every
later mention of that lot shows the same shape and color.

**Why this priority**: This is the core of the request. Without an automatic, stable marking, there
is nothing for any other part of the feature to display.

**Independent Test**: Receive one line for an enabled part with a heat number, then look the lot up
again. The marking shown the second time is identical to the first, and no user had to choose it.

**Acceptance Scenarios**:

1. **Given** an enabled part number and a heat number that has never been seen, **When** the line is
   received, **Then** the lot is recorded with exactly one shape and one color.
2. **Given** a heat number that was received previously, **When** it is received again, **Then** it
   shows the same shape and color as the first time, with no second marking created.
3. **Given** the same heat number arriving from two different suppliers, **When** both are received,
   **Then** each supplier's lot has its own distinct marking.
4. **Given** a heat number written with different spacing, casing, or separators, **When** it is
   received, **Then** it is treated as the same lot and does not create a second marking.
5. **Given** a receipt line with no heat number, **When** it is received, **Then** the line is saved
   without a marking and the receipt is not blocked.

### User Story 2 - Choosing which parts get markings (Priority: P1)

Not every part needs this treatment. A person maintains a list of the part numbers that should get
markings. They find each part by searching rather than typing its number from memory. Parts that are
not on the list are received exactly as they are today, with no marking.

**Why this priority**: The feature is deliberately opt-in per part. Until parts can be added to the
list, no marking will appear anywhere, so this is needed at the same time as the core marking.

**Independent Test**: Open the tool, add one part number through the search window, and confirm the
list shows it. Receive that part and confirm it now gets a marking, while a part not on the list does
not.

**Acceptance Scenarios**:

1. **Given** an empty part list, **When** a receipt is saved for any part, **Then** no marking is
   produced.
2. **Given** the tool is open, **When** a user searches for a part by a partial number, **Then** a
   window of matching parts is offered and the confirmed part is added to the list.
3. **Given** a part on the list, **When** a receipt for that part is saved, **Then** the lot receives
   a marking.
4. **Given** a part is removed from the list, **When** earlier receipts for that part are reviewed,
   **Then** their recorded markings are unchanged.

### User Story 3 - Seeing the marking where the work happens (Priority: P2)

The marking is only useful if it appears where handlers already look. Each receipt line shows the
shape and color for its lot, and the receipt shows the markings for its lots at the top, so the whole
trip can be scanned before opening any lines.

**Why this priority**: The marking has no value until it is visible during receiving. It is
deliberately after the marking itself, because a visible marking with nothing behind it is not
useful.

**Independent Test**: Open a receipt with several lots and confirm each line shows a shape and color,
and that the top of the receipt shows the same markings.

**Acceptance Scenarios**:

1. **Given** a saved receipt line with a marking, **When** the receipt is opened, **Then** the shape
   and color are shown on that line.
2. **Given** a receipt with several marked lots, **When** the receipt is opened, **Then** each lot's
   marking is shown at the top of the receipt.
3. **Given** a line with no marking, **When** the receipt is opened, **Then** the line displays
   normally with no marking and nothing appears broken.
4. **Given** two lots on one receipt with similar heat numbers, **When** the receipt is opened,
   **Then** their different shapes and colors make them distinguishable.

### User Story 4 - The marking stays put through history and reprint (Priority: P2)

Once a receipt moves out of the active queue into receiving history, and when it is reprinted later,
the lot's marking must still be the same one the handler saw on the floor.

**Why this priority**: A marking that changes after the fact is worse than no marking, because it
sends a handler to the wrong lot. This protects the value of everything built before it.

**Independent Test**: Move a marked receipt to history, reprint it, and confirm the same shape and
color appear as on the original.

**Acceptance Scenarios**:

1. **Given** a marked receipt line in the active queue, **When** it moves into receiving history,
   **Then** its shape and color are still available.
2. **Given** a marked receipt in receiving history, **When** it is reprinted, **Then** the reprint
   shows the same shape and color as the original.
3. **Given** the set of available shapes and colors is expanded later, **When** an old receipt is
   reviewed, **Then** its marking is unchanged.

### User Story 5 - Looking up, correcting, and protecting a marking (Priority: P3)

Two lots can be close together and share a marking by chance. The tool lets a user search for a lot by
heat number, replace its marking by hand, and protect a corrected marking so the system will not
change it again.

**Why this priority**: It is the safety valve for the rare awkward case. It is valuable but not
needed for the feature to work day to day.

**Independent Test**: Search a heat number, change its marking, lock it, and confirm it stays as
changed.

**Acceptance Scenarios**:

1. **Given** the tool is open, **When** a user searches by heat number, **Then** the matching lot and
   its current marking are shown.
2. **Given** a lot's marking is shown, **When** a user chooses a different shape or color, **Then**
   the change is saved and the user who made it is recorded.
3. **Given** a marking has been locked, **When** the lot is seen again, **Then** the system leaves the
   marking exactly as it is.
4. **Given** the tool is open, **When** a user opens the palette view, **Then** every available shape
   and color is listed.

### User Story 6 - Filling in lots that were never marked (Priority: P2)

A part only receives markings once it is on the enabled list. Every receipt taken before that has no
marking, so a handler can see some lots of a part marked and others blank. The tool lets a user pick
an enabled part, see how many of its rows and lots are unmarked, and mark them once they confirm.

**Why this priority**: The gap is visible and confusing to operators, but it does not stop the
feature working for new receipts. It closes the hole left behind when a part is added late.

**Independent Test**: Take an enabled part with unmarked history rows, run the action, and confirm
every row that carries a heat number is marked and no already-marked row changed.

**Acceptance Scenarios**:

1. **Given** an enabled part with unmarked rows, **When** the user asks for a preview, **Then** the
   number of rows and the number of distinct lots are shown and nothing is written.
2. **Given** a preview is shown, **When** the user confirms, **Then** every unmarked row of each listed
   lot is written with the shape and color that lot resolves to, in both the queue and history.
3. **Given** a part whose rows all already carry markings, **When** the user runs the action, **Then**
   the preview reports nothing to do and no row is written.
4. **Given** the action has already run for a part, **When** it is run again, **Then** nothing further
   is written because no unmarked rows remain.
5. **Given** several rows that normalize to one lot, **When** that lot is filled in, **Then** every one
   of those rows carries the same shape and color.

### Edge Cases

- A lot whose heat number is missing, blank, or only whitespace must receive no marking and must not
  block the receipt.
- The same heat number from two suppliers must produce two markings, and a heat number that returns
  later from the same supplier must reuse its original marking.
- Heat numbers entered with different spacing, casing, or separators must resolve to one lot, not two.
- When a lot is first seen while several lots are active, the marking may already be in use by another
  active lot. The later lot must be given a different marking rather than sharing one.
- If every shape and color combination is in use, the system must still assign a marking rather than
  failing, and the duplicate must be visible in the tool.
- A part removed from the enabled list keeps its existing markings; only new assignments stop.
- A user-initiated change to the shape or color set must never alter a marking already recorded.
- A non-PO receipt has no supplier. It must still be receivable and must resolve into one shared group
  so its markings repeat consistently.
- A receipt created before this feature has no recorded marking. Reviewing or reprinting it must work
  exactly as it does today.
- Extremely long or unusual heat values must not stop a receipt from being saved.
- A part number that is not on the enabled list must not be filled in at all; adding the part is a
  separate, deliberate act.
- Filling in markings must never change a row that already carries one, including a row whose lot is
  locked, which keeps the marking it already has.
- Rows whose heat number is blank, or is the sentinel the receiving flow substitutes for blank, must
  stay unmarked when their part is filled in.
- A part whose history spans both the queue and receiving history must end up consistently marked on
  both sides, never on one alone.

## Requirements

### Functional Requirements

- **FR-001**: Every distinct combination of heat number and supplier MUST receive exactly one marking,
  made of one shape and one color.
- **FR-002**: The marking MUST be chosen by the system. A user MUST NOT have to choose a marking for a
  routine receipt.
- **FR-003**: The same heat number and supplier MUST always resolve to the same marking, on every
  screen, in every part of the application, and at any later date.
- **FR-004**: A marking MUST be fixed the first time a lot is seen and MUST NOT change afterwards,
  even if the available shapes or colors are later changed.
- **FR-005**: The same heat number arriving from two different suppliers MUST produce two separate
  markings.
- **FR-006**: Supplier identity MUST be established by matching the supplier to the company's official
  supplier record, not by comparing typed text.
- **FR-007**: Receipts with no supplier MUST share one common group so their markings repeat
  consistently.
- **FR-008**: A receipt line with no heat number MUST receive no marking and MUST NOT block or delay
  the receipt.
- **FR-009**: Only part numbers on the enabled list MUST receive markings.
- **FR-010**: A part number that is not on the enabled list MUST remain fully receivable with no
  marking.
- **FR-011**: A user MUST be able to add a part number to the enabled list by searching for it, and
  MUST NOT be required to type a part number from memory.
- **FR-012**: A user MUST be able to remove a part number from the enabled list without altering any
  marking already recorded for that part.
- **FR-013**: A single system-wide switch MUST be able to turn the feature off without deleting or
  changing any recorded marking.
- **FR-014**: Each receipt line MUST show its lot's shape and color, and each receipt MUST show the
  markings of its lots at the receipt level.
- **FR-015**: When a lot would receive a marking already held by another active lot, the system MUST
  assign a different marking to the later lot automatically.
- **FR-016**: A recorded marking MUST remain available after its receipt moves into receiving history,
  and MUST be shown again when that receipt is reprinted.
- **FR-017**: A user MUST be able to find a marking by searching for its heat number.
- **FR-018**: A user MUST be able to replace a marking by hand, and MUST record a reason for doing so.
- **FR-019**: A marking that has been locked MUST NOT be changed by the system.
- **FR-020**: Every manual change to a marking MUST record who made the change and when.
- **FR-021**: A user MUST be able to see the complete set of available shapes and colors from the
  tool.
- **FR-022**: Access to the marking tool MUST be limited to users who already have access to the
  Ship/Rec Tools module.
- **FR-023**: A user MUST be able to fill in the unmarked lots of one enabled part number on demand.
- **FR-024**: The system MUST report how many rows and how many distinct lots would be filled before
  writing anything, and MUST write nothing until the user confirms.
- **FR-025**: Filling in MUST cover unmarked rows in both the receiving queue and receiving history.
- **FR-026**: Filling in MUST write only to rows that carry no marking. A row that already has one
  MUST NOT be altered.
- **FR-027**: Every row of one lot MUST be given the same shape and color, resolved once for that lot.
- **FR-028**: A lot that already has a recorded marking, including a protected one, MUST be filled in
  with that same marking rather than a new one.
- **FR-029**: Rows whose heat number is missing, blank, or the value the receiving flow substitutes
  for blank MUST be left unmarked.
- **FR-030**: Filling in MUST be offered only for part numbers already on the enabled list.
- **FR-031**: Running the action again for the same part MUST be safe and MUST write nothing further.

### Key Entities

- **Lot Marking**: one lot's permanent visual identity. It holds the heat number, the supplier it
  belongs to, the assigned shape, the assigned color, whether it was chosen automatically or set by a
  person, whether it is protected from automatic change, and who last changed it.
- **Enabled Part**: a part number that should receive markings, together with who added it and when.
  A part number not present here gets no marking.
- **Palette Value**: one available shape or one available color, with its display name and the order
  it should be listed in. Shapes and colors are offered as two separate lists.

## Success Criteria

### Measurable Outcomes

- **SC-001**: A handler can identify a lot's shape and color at a glance, without reading or
  comparing the heat number.
- **SC-002**: 100% of receipts for an enabled part number that carries a heat number end with a
  recorded marking.
- **SC-003**: A repeated visit to the same heat number and supplier shows an identical marking in
  100% of cases.
- **SC-004**: After the set of available shapes or colors is expanded, zero previously recorded
  receipts change their marking.
- **SC-005**: Two suppliers using the same heat number are visually distinguishable in 100% of cases.
- **SC-006**: Adding a part number to the enabled list takes under 30 seconds from opening the tool.
- **SC-007**: A reprinted receipt shows the same marking as the original in 100% of cases.
- **SC-008**: No receipt is blocked, delayed, or failed by a missing heat number or by the feature
  being switched off.
- **SC-009**: A handler can find a lot's marking from its heat number in under 10 seconds.
- **SC-010**: No receipts are lost or altered during the change; every receipt that could be created
  before the change can still be created after it.
- **SC-011**: After filling in an enabled part, 100% of its rows that carry a heat number hold a
  marking, and zero rows that already held one changed.
- **SC-012**: Running the fill-in twice changes nothing the second time.

## Assumptions

- The lot number is the heat number that receipts already capture. No additional lot identifier is
  introduced.
- Shape is the primary identifier and color reinforces it, so the two never depend on color alone.
- Markings are shown on screen only for this release. Printing them on labels is a later change, and
  the recorded values are kept so that change is possible.
- Recorded markings are kept permanently. Nothing is deleted or aged out.
- Any user who already has access to the Ship/Rec Tools module may correct and lock a marking.
- Receipts without a supplier, which is the normal case for non-PO receipts, share one common group.
- Volvo and Dunnage receiving are out of scope for this release.
- The shape and color lists start complete as specified and can be extended later without affecting
  existing markings.
- There are 10 shapes and 12 colors, giving 120 combinations. Two simultaneous lots can therefore
  collide by chance, which is handled by giving the later lot a free combination instead.
- The enabled part list starts empty, so the feature is invisible until a part is added.

## Verbatim Constraints

The following values were pinned by the request and MUST be used exactly as written.

- Tool display name: `Heat / Lot Management`
- Tool key: `HeatLotManagement`
- Recorded column names on receiving label data and receiving history: `heat_shape_code`,
  `heat_color_code`
- Shared supplier group for receipts with no supplier: `NONPO`
- Shapes (10). Each shape is stored as the emoji itself, so the stored value and the drawn value
  are the same thing: `⚡`, `💗`, `🌙`, `⭐`, `💎`, `⬛`, `🔺`, `⭕`, `❌`, `➡️`
- Colors (12): `BLUE`, `RED`, `GREEN`, `WHITE`, `YELLOW`, `BLACK`, `PURPLE`, `ORANGE`, `PINK`,
  `GRAY`, `BROWN`, `TEAL`
- Shapes are limited to emoji that Segoe UI Emoji draws, so a marking can never render as a
  missing-glyph box. Cylinder, hexagon and pentagon have no dependable emoji and are not offered.
