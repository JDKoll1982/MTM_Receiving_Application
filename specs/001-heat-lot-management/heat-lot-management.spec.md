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

A lot number can also be corrected after the material has been marked. Edit Mode rewrites it on both
the queue row and the history row, which leaves those rows holding a marking chosen for the number
that used to be there. **Update All Lots** exists to put that right: it applies the marking recorded
for the corrected number, or makes the corrected number a lot of its own with a marking of its own,
and clears out any lot whose number no longer appears on a receiving row.

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

### User Story 7 - A wrong lot number is put right on the lots it already marked (Priority: P2)

A handler types a lot number wrong. The lot is given a shape and a color chosen from that wrong number,
so the material carries a marking for a lot that does not exist. The handler notices, corrects the
number on the row in Edit Mode, and then comes to this tool and runs Update All Lots. Every row whose
marking no longer matches the lot number it now carries is put right: rows fall in with a lot that
already exists under the corrected number, or the corrected number becomes a lot of its own with a
marking of its own. Lots left describing nothing are cleared out of the list.

**Why this priority**: A wrong number produces a wrong marking, and until it is put right the tool
shows a marking belonging to no lot while a real lot sits waiting to be marked differently when it is
received again. It is second to the core marking because it repairs a mistake rather than making the
marking in the first place.

**Independent Test**: Mark a lot, correct its number in Edit Mode to a number that does not exist yet,
run Update All Lots, then check the row and the list. The row shows the new lot's marking, the
corrected number is now in the list, and the wrong number is gone.

**Acceptance Scenarios**:

1. **Given** a marked row whose lot number was corrected to a number that already exists as a lot,
   **When** Update All Lots runs and is confirmed, **Then** the row is changed to show that lot's shape
   and color, and no second marking is created for that number.
2. **Given** a marked row whose lot number was corrected to a number that does not exist as a lot,
   **When** Update All Lots runs and is confirmed, **Then** the corrected number becomes a lot with a
   shape and a color that no other lot holds, and the row shows it.
3. **Given** either case, **When** the corrected number is received again afterwards, **Then** it shows
   the same shape and color the row shows, with no marking computed a second way.
4. **Given** a lot whose lot number appears on no receiving row, **When** Update All Lots runs and is
   confirmed, **Then** the lot is removed from the list along with its shape and color, and that shape
   and color can be given to a later lot.
5. **Given** a lot whose lot number still appears on at least one receiving row, **When** Update All
   Lots runs, **Then** the lot stays in the list, even if only one row still carries that number.
6. **Given** a marked row whose lot number was never changed and whose marking is already correct,
   **When** Update All Lots runs, **Then** the row is not touched and is not reported as a change.
7. **Given** a lot whose marking is protected, **When** Update All Lots runs, **Then** the lot is not
   altered and is not removed, and it is reported with the reason instead.
8. **Given** Update All Lots has shown what it would do, **When** the person cancels it, **Then**
   nothing has been written.

---

### User Story 8 - The lot list shows everything it holds, and how much of each lot there is (Priority: P2)

The list of recorded markings is the page a person opens to answer questions about a lot. Today some of
its columns are cut off, with no way to reach the hidden ones, and it says nothing about how much
material a lot covers or when that lot was last seen. A handler looking at a lot wants to know both.

**Why this priority**: The list is how the feature is inspected, so a column nobody can read is a
column that does not exist. It is second to the stories that record and repair markings because it
reads rather than writes.

**Independent Test**: Open the tool on a narrow window, confirm every column is reachable, and confirm
a lot that has receiving lines shows the right number of them and the right most recent date.

**Acceptance Scenarios**:

1. **Given** the lot list is shown, **When** the window is too narrow for every column, **Then** the
   list scrolls sideways so no column is clipped and no column is unreachable.
2. **Given** a lot that several receiving lines carry across the queue and history, **When** the list is
   shown, **Then** that lot's entry reports how many lines carry it.
3. **Given** a lot whose lines have been received over time, **When** the list is shown, **Then** it
   reports the date of the most recent line.
4. **Given** a lot with no receiving lines at all, **When** the list is shown, **Then** it reports a
   count of zero and no date, and it stays in the list rather than disappearing.
5. **Given** the list is refreshed, **When** the counts and dates are read, **Then** no marking is
   changed by having been read.

---

### User Story 9 - A reprinted label carries the marking and comes back complete (Priority: P2)

Reprinting queues a copy of a past receipt so its label can be printed again. The copy is not faithful:
four of the receiving columns never make it back, and two of them arrive with the wrong values, so a
reprinted row can come back with a weight that is really a quantity and a part type that was thrown
away. A reprint must be a copy, not a retyping.

**Why this priority**: A label is what a handler reads, and a copy that silently differs from the
receipt it came from is worse than no copy. It is second to the marking stories only because it is a
fidelity repair rather than new behaviour.

**Independent Test**: Take a history row, queue it for reprint, and compare the queued row against the
history row column by column. Every value matches except the marking columns, which carry the lot's
recorded marking.

**Acceptance Scenarios**:

1. **Given** a history row, **When** it is queued for reprint, **Then** every value it carries is
   present on the queued row, including the weight, the part type, the skid sequence and the skid total.
2. **Given** a history row whose weight differs from its quantity, **When** it is queued, **Then** the
   queued weight is the history weight and not the quantity.
3. **Given** a history row that carries no marking, **When** it is queued for reprint, **Then** the
   queued row is given the marking recorded for its lot number and supplier, so the printed label shows
   it.
4. **Given** a history row that already carries a marking, **When** it is queued, **Then** the queued
   row carries that same marking and no new one is computed.
5. **Given** the same lot appears elsewhere in the tool, **When** a label for it is reprinted, **Then**
   the marking on the label and the marking shown for that lot are the same shape and color.
6. **Given** a history row is queued for reprint, **When** the queue is written, **Then** the history
   row itself is unchanged.
7. **Given** a history row is already queued for reprint, **When** it is queued again, **Then** it is
   refused and no second copy is created.

---

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
- A lot number cleared to nothing must leave no marking on the row, and must not become a lot.
- Correcting a number must not change the lot number, the supplier, a quantity, or any other receiving
  value; only markings and the record of markings are put right.
- Only some rows of a lot corrected: the corrected rows move to the corrected number while the rows
  still carrying the old number keep the old lot, which therefore stays in the list.
- Rows carrying no marking are out of scope for Update All Lots. It puts existing markings right and
  does not introduce new ones, which is what the fill-in action is for.
- A label printed before a lot number was corrected still shows the marking chosen from the wrong
  number, so it and the tool disagree until the label is reprinted. Reprinting is out of scope, and
  removing a lot frees its shape and color, so a later lot can be given a pair a label in use still
  shows.
- A lot's line count and its most recent date are read for display only. Neither is stored on the lot,
  so neither can go stale, and reading them never writes anything.
- A lot whose lines were all received before the marking feature existed has lines but no markings on
  them. It still reports a count and a date, and its marking is the one recorded for its lot number.
- Reprinting a history row that was received before markings existed is the case that most needs the
  marking to be resolved rather than copied, because there is none on the history row to copy.
- A history row with no lot number keeps no marking when it is queued for reprint, and must still be
  reprintable.
- Queuing a row for reprint twice is refused, so the count of queued copies for one receipt can never
  exceed one.

### User Story 10 - Only the parts on the list are marked, and the list shows one material's lots (Priority: P1)

The tool has been handing out shapes and colors to material nobody approved. Every receipt that
carried a lot number was given a marking, whatever part it was for, and the list of recorded markings
is filled with lots belonging to parts that are not on the list at all. The list exists to answer
questions about a material's lots, and another material's lots in it answer nothing. The list must
show only the lots of material on the list, and must narrow to one part when one is chosen.

**Why this priority**: The feature is opt-in per part, and this is that opt-in not working. A marking
handed to a part that is not on the list is material marked that nobody asked to mark, which is the
one thing the list exists to prevent. It belongs with the core marking because it is the same
requirement, not an addition to it.

**Independent Test**: Open the tool where the enabled list holds one part. Confirm the list shows only
lots whose receiving rows include that part, and that a lot belonging solely to another part does not
appear. Choose that part and confirm the list narrows to it. Receive a part that is not on the list
and confirm no marking is created for it.

**Acceptance Scenarios**:

1. **Given** a part that is not on the enabled list, **When** a receipt for it is saved, **Then** no
   marking is created and its lot number does not appear in the list.
2. **Given** a part that is on the enabled list, **When** a receipt for it is saved, **Then** its lot
   receives a marking.
3. **Given** a lot whose receiving rows include a part on the enabled list, **When** the list is shown
   with no part chosen, **Then** the lot appears.
4. **Given** a lot whose receiving rows carry only parts that are not on the enabled list, **When** the
   list is shown with no part chosen, **Then** the lot does not appear.
5. **Given** a part is chosen in the part list, **When** the marking list is read, **Then** it shows
   only lots whose receiving rows include that part.
6. **Given** a lot whose number appears on no receiving row at all, **When** the list is shown, **Then**
   it still appears, because it cannot be attributed to any part.
7. **Given** a part was removed from the list after its lots were marked, **When** that removed part is
   chosen, **Then** its lots are shown, because choosing a part is a deliberate act.
8. **Given** a lot already has a recorded marking, **When** a receipt for it arrives for a part that is
   not on the enabled list, **Then** the marking already recorded is returned unchanged.

**Edge Cases**:

- A lot can be carried by several parts at once, some on the list and some not. It is shown, because
  one of the parts carrying it is on the list.
- A receipt that carries no part number cannot be attributed, so it creates no marking and its lot is
  not attributed to anything.
- A lot carried by a part that was removed from the list keeps its marking, and nothing new is created
  for that material.

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
- **FR-032**: A user MUST be able to run an Update All Lots action from the tool, on demand, which puts
  markings right after lot numbers have been corrected on the receiving rows.
- **FR-033**: Update All Lots MUST report every lot it would create, every lot it would remove, and
  every row whose marking would change, with how many rows and lots are affected, and MUST write
  nothing until the user confirms.
- **FR-034**: A report that is dismissed MUST leave all stored data exactly as it was.
- **FR-035**: Update All Lots MUST consider receiving rows in both the receiving queue and receiving
  history.
- **FR-036**: A marked row MUST be treated as needing repair when the marking it shows is not the
  marking recorded for the lot number and supplier that row now carries.
- **FR-037**: Where a marking is already recorded for the row's current lot number and supplier, that
  marking MUST be applied to the row.
- **FR-038**: Where the row's current lot number carries no recorded marking, that lot number MUST
  become a new lot, MUST be given a shape and a color chosen the same way a receipt chooses one, and
  the row MUST be changed to show them.
- **FR-039**: The shape and color given to a new lot MUST be a combination that no other lot in the
  list holds, so that two live lots can never look the same.
- **FR-040**: A lot whose lot number appears on no receiving row MUST be removed from the list with its
  shape and color, and that shape and color MUST be free for a later lot to be given.
- **FR-041**: A lot whose lot number still appears on any receiving row MUST NOT be removed, even when
  only a single row still carries that number.
- **FR-042**: A lot whose marking is protected MUST NOT be altered or removed by Update All Lots, and
  MUST be reported with the reason it was left alone.
- **FR-043**: Where a row's current lot number is missing, blank, or the value the receiving flow
  substitutes for blank, its marking MUST be removed, and the blank value MUST NOT become a lot.
- **FR-044**: Whether a row carries a lot number at all MUST be decided by the shared lot number
  normalization used everywhere else, so this action and the automatic path can never disagree about
  which rows are lots.
- **FR-045**: Update All Lots MUST NOT change a lot number, a supplier, a quantity, or any other
  receiving value.
- **FR-046**: Update All Lots MUST record who ran it and when, on every marking it writes or removes.
- **FR-047**: Running Update All Lots twice in a row MUST report changes the first time and nothing the
  second time.
- **FR-048**: Update All Lots MUST NOT depend on which part numbers are on the enabled list.
- **FR-049**: Update All Lots MUST NOT run as part of saving an edit. It is only ever run by a person,
  from the tool.
- **FR-050**: Update All Lots MUST report the number of lots it left alone because they were already
  correct, so that an empty result is distinguishable from a failed one.
- **FR-051**: The lot list MUST present every column it holds, and when the available width is too
  small for all of them, the list MUST scroll so that every column can be reached rather than being
  clipped.
- **FR-052**: Each lot in the list MUST report how many receiving lines carry it, counting both the
  receiving queue and receiving history.
- **FR-053**: Each lot in the list MUST report the date of its most recent receiving line, and MUST
  show no date rather than a misleading one when it has no lines.
- **FR-054**: The counts and dates MUST be read in the same pass that reads the list, and reading them
  MUST NOT change any marking.
- **FR-055**: A lot with no receiving lines MUST still appear in the list, with a count of zero.
- **FR-056**: Queueing a history row for reprint MUST carry every value the history row holds onto the
  queued row, including the weight, the part type, the skid sequence and the skid total.
- **FR-057**: A column that the history row carries a value for MUST NOT be filled from a different
  column, and MUST NOT be left empty.
- **FR-058**: Queueing a history row for reprint MUST ensure the queued row carries the marking recorded
  for its lot number and supplier, resolving it when the history row carries none.
- **FR-059**: Where the lot already has a recorded marking, that marking MUST be the one used. Reprint
  MUST NOT compute a different one.
- **FR-060**: The marking on a queued reprint row MUST be the same shape and color that the same lot
  shows everywhere else in the tool.
- **FR-061**: Queueing a row for reprint MUST NOT change the history row it copies from.
- **FR-062**: A history row that is already queued for reprint MUST NOT be queued a second time.
- **FR-063**: A marking MUST NOT be created for a part number that is not on the enabled list, whatever
  the receipt carries.
- **FR-064**: A marking already recorded for a lot MUST be returned unchanged even when the part
  arriving against it is not on the enabled list, so a marking a handler has already seen is never
  withdrawn.
- **FR-065**: Whether a part is on the enabled list MUST be decided when a marking would be created,
  and MUST NOT be consulted when a marking is read.
- **FR-066**: A receipt whose part number is absent MUST receive no marking, because a marking that
  cannot be attributed to a part cannot be shown to belong to one.
- **FR-067**: The lot list MUST show only lots that at least one enabled part carries.
- **FR-068**: Choosing a part number in the tool MUST narrow the lot list to the lots that part carries.
- **FR-069**: Removing a part from the list MUST NOT on its own withhold the markings already recorded
  for its lots from a user who chooses that part.
- **FR-070**: A lot whose number appears on no receiving row MUST remain in the list, because it cannot
  be attributed to a part. This is what keeps a corrected-away lot visible.
- **FR-071**: Each lot in the list MUST report the part numbers that carry it.
- **FR-072**: The part numbers a lot is attributed to MUST be read from the receiving rows that carry
  its number rather than from a value stored on the marking, so attribution is right for lots recorded
  before this rule existed.
- **FR-073**: When a heat number search matches nothing, the list MUST say that it covers only the
  parts on the enabled list, so an empty result is not mistaken for a lot that has no marking.
- **FR-074**: When the enabled part list cannot be read, the system MUST NOT create a marking, so an
  unreadable list can never mark material that was not approved.

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
- **SC-013**: After Update All Lots is applied, 100% of marked rows show the marking recorded for the
  lot number and supplier that row carries.
- **SC-014**: A lot number that is corrected and then received again shows the same marking as the
  corrected row in 100% of cases, with no second marking created.
- **SC-015**: After Update All Lots is applied, no lot remains in the list whose lot number appears on
  no receiving row, apart from protected lots, which are reported.
- **SC-016**: Running Update All Lots twice in a row changes nothing the second time.
- **SC-017**: Every row whose marking Update All Lots changes is reported with its marking before and
  after, so that no change to what a handler sees is silent.
- **SC-018**: A dismissed report leaves the stored data byte-for-byte unchanged.
- **SC-019**: A shape and color freed by removing a lot can be assigned to a later lot.
- **SC-020**: Every column of the lot list can be reached at any window width, with none clipped.
- **SC-021**: The count and date reported for a lot match the receiving lines that carry it in 100% of
  cases.
- **SC-022**: A queued reprint row matches the history row it came from value for value in 100% of
  cases, apart from the two marking columns.
- **SC-023**: A reprinted label carries the same shape and color as its lot shows elsewhere in the tool
  in 100% of cases.
- **SC-024**: Reading the lot list changes no stored value.
- **SC-025**: Zero markings are created for parts that are not on the enabled list.
- **SC-026**: Zero lots that no enabled part carries appear in the lot list.
- **SC-027**: Choosing a part shows every lot that part carries and no lot it does not, in 100% of
  cases.
- **SC-028**: Zero already-recorded markings are altered or removed by this rule, so no label already
  on the floor changes what it says.

## Assumptions

- The lot number is the heat number that receipts already capture. No additional lot identifier is
  introduced.
- Shape is the primary identifier and color reinforces it, so the two never depend on color alone.
- Markings are shown on screen only for this release. Printing them on labels is a later change, and
  the recorded values are kept so that change is possible.
- Recorded markings are kept permanently. Nothing is deleted or aged out.
  **Superseded on 2026-10-14 for exactly one case:** a lot whose lot number appears on no receiving
  row is removed by Update All Lots together with its shape and color, so the pair can be given to a
  later lot. A protected lot is never removed.
- Any user who already has access to the Ship/Rec Tools module may correct and lock a marking.
- Receipts without a supplier, which is the normal case for non-PO receipts, share one common group.
- Volvo and Dunnage receiving are out of scope for this release.
- The shape and color lists start complete as specified and can be extended later without affecting
  existing markings.
- There are 10 shapes and 12 colors, giving 120 combinations. Two simultaneous lots can therefore
  collide by chance, which is handled by giving the later lot a free combination instead.
- The enabled part list starts empty, so the feature is invisible until a part is added.
- A corrected lot number that already exists as a lot takes that lot's marking; one that does not exist
  becomes a new lot with a marking of its own. Update All Lots therefore follows the corrected number
  rather than the material, and a label printed before the correction shows the older marking until it
  is reprinted.
- Update All Lots is run by a person after correcting the row, not automatically when an edit is saved.
- Clearing a lot number to nothing removes the marking, following the rule that a line with no lot
  number gets no marking.
- Update All Lots covers every part, not only enabled ones, because the record of markings is not
  scoped to a part.
- The lot list's count is the number of receiving lines that carry the lot, not a skid count. The
  receiving tables carry a skid sequence and a skid total per line, and neither is a count of skids for
  a lot, so a line count is the honest measure of how much of that lot came in. A skid figure is
  reported instead if the count asked for was about skids.
- The lot list's date is the most recent receipt date among the lot's lines, falling back to the time
  the line was written when a line has no receipt date. The receipt date is the business fact; the
  write time is the fallback because it is always present.
- The reprint marking is resolved in the application and written onto the queued row, because the
  matching key for a lot number and supplier is produced in C#, not in SQL.
- Reprint covers the receiving labels only. The marking columns exist on the receiving tables and
  nowhere else, and Volvo and Dunnage receiving are out of scope for this release, so the Volvo and
  Dunnage reprint paths are left as they are.
- The lot list is scoped to the enabled part list, but Update All Lots is not. It reads the receiving
  rows directly and still covers every part, so a lot number corrected on material that is not on the
  list is still put right. FR-048 continues to hold.
- A lot's part numbers are read from the receiving rows that carry its number, not from the marking.
  That is what lets the list be scoped, and it is why attribution is right for the lots recorded
  before the enabled list was consulted.
- Markings already recorded against material that is not on the enabled list are left as they are, and
  nothing new is created for that material. Withdrawing them would change what a label already on the
  floor says. A user can still reach a removed part's lots by choosing that part.
- A part is matched to the enabled list case-insensitively after trimming, because a part number is
  stored as it was typed on the receiving side and as it was added to the list.

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
- The Update All Lots action name: `Update All Lots`
- The Update All Lots request, as given: "the lot number can be accidently written in worng, causing an
  improper shape/color assignment, if i later edit the row in question i want to then be able to come
  to the heat lot management and update the assigned clr/sharp to etiher fit an existing lot# if the lot
  I changed already exists, or create a new lot #, also it should check for lot #s that dont exist and
  remove the shape / color from the list"
- The part scope request, as given: "showing / generating shapes / colors for materials that are not on
  the approved list, this should not occure and the list should only show the selected material's lot
  numbers"
