# Feature Specification: Update All Lots

**Feature Branch**: `002-update-all-lots`
**Created**: 2026-10-14
**Status**: Draft
**Input**: User description: "(new new spec) add a Update All Lots (fix previously marked lots in the event the lot number changes after marking)"

**Clarified by the user, 2026-10-14**: "the lot number can be accidently written in worng, causing an
improper shape/color assignment, if i later edit the row in question i want to then be able to come to
the heat lot management and update the assigned clr/sharp to etiher fit an existing lot# if the lot I
changed already exists, or create a new lot #, also it should check for lot #s that dont exist and
remove the shape / color from the list"

## Overview

A lot's marking is chosen once, from its lot number and its supplier, and is never recomputed. That
rule is what makes the marking worth trusting: a shape and a color seen on a coil in March still mean
that coil in August.

The rule assumes the lot number does not change afterwards. It can. Edit Mode lets a person correct
any field on a saved load, including the lot number, and saving rewrites that number on both the queue
row and the history row. It does not rewrite the marking. Correcting a mistyped lot number therefore
leaves each row carrying a shape and a color chosen for a number that is no longer on the row, and
leaves the record of which lot owns which marking still keyed on the old number. The next load of the
corrected number then resolves a different marking from the one already sitting on the floor.

The marking is also wrong in the first place in the case that matters most: the number was mistyped,
so the shape and color were chosen for a number the material never had. Correcting the number leaves
that wrong marking on the row, and leaves a lot in the list that no longer describes anything.

This feature adds one action, **Update All Lots**, to the heat lot management tool. A person corrects
the number on the row in Edit Mode, then runs this action, which does three things:

1. Where the corrected lot number already exists, the rows are given that lot's shape and color, so
   all the material for one number shows one marking.
2. Where the corrected lot number does not exist, it becomes a new lot and is assigned a shape and a
   color of its own, chosen the same way the receiving flow chooses one.
3. Where a lot in the list no longer matches a lot number on any receiving row, it is removed from the
   list together with its shape and color, and that shape and color are free for a later lot.

It shows exactly what it proposes to do before it does any of it, and applies the change only on
confirmation. Nothing about receiving changes: it is only ever run by a person, and it never alters a
lot number, a supplier, or any other receiving value.

## User Scenarios & Testing

### User Story 1 - A wrong lot number is put right, and so is its marking (Priority: P1)

A handler receives a load and types the lot number wrong. The lot is given a shape and a color chosen
from that wrong number, so the marking describes a lot that does not exist. Later the handler corrects
the number in Edit Mode and then comes to heat lot management and runs the update. The row either
falls in with a lot that already exists under the corrected number, or the corrected number becomes a
lot of its own with a marking of its own.

**Why this priority**: This is the reason the feature exists. Everything else is how the change is
driven, bounded, or reported.

**Independent Test**: Mark a lot, correct its number in Edit Mode to a number that does not exist yet,
run the update, then check the row and the list. The row shows the new lot's marking, the corrected
number appears in the list, and the wrong number is gone.

**Acceptance Scenarios**:

1. **Given** a marked row whose lot number was corrected to a number that already exists as a lot,
   **When** the update runs and is confirmed, **Then** the row is changed to show that lot's shape and
   color, and no second marking is created for that number.
2. **Given** a marked row whose lot number was corrected to a number that does not exist as a lot,
   **When** the update runs and is confirmed, **Then** the corrected number becomes a lot with a shape
   and a color that no other lot holds, and the row shows it.
3. **Given** either case, **When** the corrected number is received again afterwards, **Then** it shows
   the same shape and color the row shows, with no marking computed a second way.
4. **Given** a marked row whose lot number was never changed and whose marking is already correct,
   **When** the update runs, **Then** the row is not touched and is not reported as a change.

---

### User Story 2 - The changes are seen before they are made (Priority: P1)

A repair that rewrites markings across history is not something a person should trigger blind. The
action reports what it found, how many rows and lots it covers, and what each change would be, and
writes nothing until the person confirms.

**Why this priority**: The action rewrites markings on receiving history, which is a record of what
was actually received. A change to it has to be visible and deliberate rather than incidental.

**Independent Test**: Make one correction that creates one stale marking, start the repair, and
confirm that the count and the change are shown and that nothing is written until confirmation. Then
cancel and confirm nothing was written.

**Acceptance Scenarios**:

1. **Given** one or more stale markings exist, **When** the action is started, **Then** it reports how
   many rows and how many lots would be affected, without writing anything.
2. **Given** the report is showing, **When** the person cancels it, **Then** nothing has been written
   and running it again reports the same thing.
3. **Given** the report is showing, **When** the person confirms it, **Then** the reported changes are
   applied.

---

### User Story 3 - A lot number that no longer exists is cleared out of the list (Priority: P2)

Correcting a number leaves the old number's lot behind. Nothing points at it any more: no queue row and
no history row carries that number, so it describes nothing, yet it still sits in the list holding a
shape and a color. Over time the list fills with lots that no longer exist.

The update therefore also looks the other way, from the list back to the receiving rows, and removes
every lot whose number does not appear on any row. Removing a lot frees its shape and color, so a later
lot can be given them.

**Why this priority**: It is the other half of the user's request, and it is what keeps the list
readable and stops the pool of shapes and colors filling up with assignments to lots that are gone.

**Independent Test**: Mark a lot, correct its number to a new number, run the update, and confirm the
old number is no longer in the list while the new one is.

**Acceptance Scenarios**:

1. **Given** a lot whose number appears on no receiving row, **When** the update runs and is confirmed,
   **Then** the lot is removed from the list along with its shape and color.
2. **Given** a lot that was removed, **When** its lot number is received again later, **Then** it is
   treated as a lot that has never been seen, and is given a marking then.
3. **Given** a lot whose number still appears on at least one receiving row, **When** the update runs,
   **Then** the lot stays in the list, even if only one row still carries that number.
4. **Given** a lot whose marking is protected, **When** the update runs, **Then** the lot is not
   removed and is reported instead.

---

### User Story 4 - A lot number corrected to nothing (Priority: P2)

The number on a load can be cleared as well as corrected, because not every line has a lot number. A
line with no lot number is not a lot and must not carry a lot's marking.

**Why this priority**: Clearing a number is the one case where the repair removes a marking rather
than reconciling one, so it needs its own rule and its own confirmation.

**Independent Test**: Mark a lot, clear its number in Edit Mode, run the repair, and confirm the row
is left with no marking and that the removal was reported.

**Acceptance Scenarios**:

1. **Given** a marked row whose lot number is now missing, blank, or the value the receiving flow
   substitutes for blank, **When** the repair runs, **Then** the marking is removed from the row.
2. **Given** that removal, **When** it is applied, **Then** it is reported as a removal and not as a
   reconciliation.
3. **Given** that row, **When** the lot number is entered again later, **Then** the row is treated like
   any other unmarked row.

---

### User Story 5 - A protected marking is left alone (Priority: P3)

A marking can be protected so that the automatic path never recomputes or probes it. The update must
not become a way around that protection, either by changing the marking or by removing the lot because
its number no longer appears anywhere.

**Why this priority**: Protection exists for the markings a person has deliberately fixed. It is a
small case, but overriding it silently would defeat the feature that protects them.

**Independent Test**: Protect a lot whose number is then corrected away, run the update, and confirm the
protected lot is still in the list with its marking unchanged, and that it was reported.

**Acceptance Scenarios**:

1. **Given** a row whose lot has a protected recording, **When** the update runs, **Then** the
   protected recording is not modified and the row is not changed to another marking.
2. **Given** a protected lot whose number appears on no receiving row, **When** the update runs, **Then**
   the lot is not removed from the list.
3. **Given** either case, **When** the update finishes, **Then** the lot is reported with the reason it
   was left alone, so the person can deal with it by hand.

---

### Edge Cases

- **A row that was never marked**: out of scope. This action repairs markings that exist. A row with no
  marking is left alone even when it now has a usable lot number, because the separate fill-in action
  already covers unmarked rows and a repair is not the right place to introduce markings.
- **Only some rows of a lot were corrected**: the corrected rows move to the corrected number, and the
  rows still carrying the old number keep the old lot, which still exists because they are on it. The
  old lot is therefore not removed. One physical lot ends up under two numbers with two markings, which
  is the honest result of not having corrected every row; the update is safe to run again once the
  remaining rows are corrected.
- **The corrected number already owns a marking**: that lot's shape and color win, and the rows are
  changed to show them (User Story 1, case 1). The change is reported.
- **Two different lots corrected onto the same number in one run**: both sets of rows are changed to
  that number's single marking, and any old lots left with no rows are removed. The report names both.
- **The printed label on material already on the floor**: a label printed before the correction carries
  the shape and color chosen from the wrong number. After the update the label and the system disagree
  until the label is reprinted. This follows from the request, which asks for a correct assignment
  rather than keeping the wrong one, and it is called out so that reprinting is not forgotten.
- **A shape and color freed by a removal**: removal frees the pair, so a later lot can be given the same
  shape and color that a printed label still shows. The pool of pairs is finite, so freeing them is
  necessary; the cost is that one pair can mean two lots at different times.
- **A lot that is both protected and has no rows**: it is not removed, because protection means a person
  deliberately fixed that marking. It is reported so the person can unprotect it if the lot really is
  gone.
- **A marking that the record has no entry for, and whose number is also unusable**: the marking is
  removed, because a line with no lot number carries no marking (User Story 4).
- **A row whose lot number reads "Refer to Vendor Tag"**: the receiving flow writes that text when a
  person leaves the lot number empty, but the shared blank check recognises only the other substitute,
  "Nothing Entered", so that text is currently a usable lot number and every load carrying it is one
  shared lot. The update treats it as a real lot and deals with it by the ordinary rules, because blank
  must be defined exactly the way the rest of the feature defines it. Whether those rows should be lots
  at all is a separate question, and is called out as an open decision below.
- **A row whose supplier changed but whose lot number did not**: this is also a change of lot, because
  the supplier is half of a lot's identity, and it is repaired the same way.
- **A part removed from the enabled list after being marked**: still repaired. The record of markings
  is not scoped to a part, and a stale marking is stale whatever list the part is on.
- **Someone receives material while the repair is running**: the repair reports what it applied and the
  next run picks up anything the receipt changed. It is safe to run again at any time.
- **The record already holds the same shape and color for the corrected number**: the row already
  agrees, so there is nothing to do and nothing is reported.
- **Running the repair when nothing is stale**: reports nothing to repair and writes nothing.
- **A very large number of affected rows**: the report is produced before anything is written, so the
  size of the change is visible before it is committed.

## Requirements

### Functional Requirements

- **FR-001**: The system MUST provide a named action, Update All Lots, in the lot marking tool, which a
  person runs on demand.
- **FR-002**: The action MUST report what it would change, including how many rows and how many lots
  are affected, and MUST write nothing until the person confirms.
- **FR-003**: A report that is dismissed MUST leave the stored data exactly as it was.
- **FR-004**: The action MUST consider receiving rows in both the receiving queue and receiving history.
- **FR-005**: A marked row MUST be treated as out of agreement when the marking it shows is not the
  marking the record holds for the lot number and supplier that row now carries.
- **FR-006**: A marked row MUST also be treated as out of agreement when the record holds no marking at
  all for the lot number and supplier that row now carries.
- **FR-007**: Where the record holds a marking for the row's current lot number and supplier, that
  recorded marking is authoritative and the row MUST be changed to show its shape and color.
- **FR-008**: Where the row's current lot number is present and the record holds no marking for it, the
  corrected number MUST become a new lot, that lot MUST be assigned a shape and a color, and the row
  MUST be changed to show them. The assignment MUST be made the same way the receiving flow makes it,
  so that a later receipt of that number agrees with the row without computing anything a second way.
- **FR-009**: Where the row's current lot number is missing, blank, or the value that means no lot
  number, the repair MUST remove the marking from the row, because a line with no lot number carries
  no lot, and the blank value MUST NOT become a lot. Whether a row carries a lot number at all MUST be
  decided by the shared lot number normalization used everywhere else, so that this action and the
  automatic marking path can never disagree about which rows are lots.
- **FR-010**: The shape and color assigned to a new lot MUST be a combination that no lot in the list
  currently holds, so that two live lots can never look the same.
- **FR-011**: A lot whose marking is protected MUST NOT be altered by the repair, and MUST be reported
  as skipped along with the reason it was skipped.
- **FR-012**: The repair MUST NOT change a lot number, a supplier, a quantity, or any other receiving
  value. It changes markings and the record of markings only.
- **FR-013**: A lot whose lot number appears on no receiving row MUST be removed from the list together
  with its shape and color, and that shape and color MUST be free for a later lot to be given.
- **FR-014**: The repair MUST record who ran it and when, on every recording it writes or changes.
- **FR-015**: Every change made by the repair MUST be reported individually or by lot, naming the lot
  number, the supplier, the marking before, and the marking after, so that a change to what an operator
  sees on the floor is never silent.
- **FR-016**: A second run of the action MUST find nothing to repair.
- **FR-017**: The repair MUST NOT depend on which part numbers are on the enabled list.
- **FR-018**: The action MUST report the number of lots it left alone because their marking was already
  in agreement, so that an empty result is distinguishable from a failed one.
- **FR-019**: A lot whose marking is protected MUST NOT be removed by the update, and MUST be reported
  so the person can decide about it.
- **FR-020**: A lot whose lot number still appears on any receiving row MUST NOT be removed, even when
  only a single row still carries it.
- **FR-021**: The update MUST NOT run as part of saving an edit. It is only ever run by a person, from
  the heat lot management tool.
- **FR-022**: The report MUST name every lot that would be created, every lot that would be removed,
  and every row whose marking would change, before anything is written.

### Key Entities

- **Lot marking**: One shape and one color held for one lot. A lot is the pair of a normalized lot
  number and a supplier group. The marking is recorded once, is authoritative from then on, and can be
  protected so the automatic path never recomputes it.
- **Marked receiving row**: A queue row or a history row that carries a lot number, a supplier, and a
  shape and color. The row shows the marking; the lot marking records who owns it.
- **Repair**: What the action would do, and what it did. Reports the lots found, how many rows and lots
  each covers, what the change is, and what was skipped with a reason.

## Success Criteria

### Measurable Outcomes

- **SC-001**: After a repair is applied, no marked receiving row disagrees with the record for the lot
  number and supplier that row carries.
- **SC-002**: A lot number that is corrected and then received again is given the same shape and color
  as the corrected row, with no second marking created for it, whether that number already existed or
  was created by the update.
- **SC-003**: Running the action twice in a row reports changes the first time and nothing the second
  time.
- **SC-004**: The number of rows and lots about to change is reported before any write, so that no
  write of unbounded size can happen without it having been shown.
- **SC-005**: Every row the update changes is reported with its marking before and its marking after,
  so that no change to what an operator sees on the floor is silent.
- **SC-006**: No protected marking is ever modified or removed by the action.
- **SC-007**: A dismissed report leaves the stored data byte-for-byte as it was.
- **SC-008**: After the update, no lot remains in the list whose lot number appears on no receiving row,
  except lots whose marking is protected, which are reported.
- **SC-009**: A shape and color freed by a removal can be assigned to a later lot.

## Assumptions

The user was asked about the decisions below on 2026-10-14 and answered the two that change the
behavior most. A1 and A7 were taken by the author first and then corrected by the user. They are
recorded here in their corrected form, with the author's original reading noted, because the
difference between the two is the difference between this feature and a different one.

- **A1 - A corrected lot number that does not exist becomes a new lot with a new marking.** *(Corrected
  by the user. The author first assumed the opposite: that the existing marking would be kept and
  simply re-pointed at the corrected number, so that material already labelled would not change color.)*
  The user's request is that a wrong number produces an "improper shape/color assignment", and that
  correcting it should "fit an existing lot# if the lot I changed already exists, or create a new lot
  #". The marking is therefore recomputed for a number that does not exist yet.
- **A2 - The repair is run by a person, not by the save that changes the number.** It is an on-demand
  action in the lot marking tool.
  *Rejected alternative*: repair automatically whenever an edit is saved. That would rewrite markings
  across history on every save, with no opportunity to see the change first.
- **A3 - Both the queue and history are repaired, for every part.** A queue row and its history row are
  both rewritten when a number is corrected, so both can hold a stale marking, and the record of
  markings is not scoped to a part.
  *Rejected alternative*: repair only parts on the enabled list, which would leave stale markings on
  parts that were marked before being removed from the list.
- **A4 - Where the corrected number already exists, its marking wins.** This is the user's "fit an
  existing lot# if the lot I changed already exists", and it is the ordinary case rather than a
  collision: the rows take that lot's shape and color.
  *Rejected alternative*: overwrite that lot's recorded marking with the moved lot's, which would
  contradict the rule that a recorded marking is never replaced.
- **A5 - A protected lot is skipped and reported, not overridden.** The repair respects the protection.
  *Rejected alternative*: let the repair override it, which would create a second way to change a
  marking that a person deliberately fixed.
- **A6 - A number corrected to blank removes the marking.** This follows the existing rule that a line
  with no lot number gets no marking.
  *Rejected alternative*: leave the old marking in place, which would show a marking for a lot number
  that no longer exists on the row.
- **A7 - A lot with no receiving row behind it is removed from the list, and its shape and color are
  freed.** *(Corrected by the user, whose words are "it should check for lot #s that dont exist and
  remove the shape / color from the list".)* The author first assumed the opposite, following the
  existing rule that markings are kept forever. That rule is superseded for lots that no longer
  describe anything; the supersession is recorded under Verbatim Constraints.
- **A8 - Unmarked rows are out of scope.** The request was to fix *previously marked* lots. Rows with no
  marking are left alone; the separate fill-in action already covers them.
- **A9 - A new lot's marking is chosen by the same assignment the receiving flow uses.** Otherwise this
  update and the next receipt of that number would each compute a marking and disagree.
- **A10 - Removal is never applied to a protected lot.** Protection means a person deliberately fixed
  that marking, so the update reports the lot instead of removing it.

## Verbatim Constraints

- The request, as given: "add a Update All Lots (fix previously marked lots in the event the lot number
  changes after marking)".
- MySQL writes go through stored procedures only. No raw SQL in C#.
- Infor Visual (SQL Server) is read-only and is not touched by this feature.
- MVVM layering holds: View, then ViewModel, then Service, then DAO, then database.
- A recorded marking is authoritative: where a lot number already exists, its shape and color are what
  the rows are given.
- **Superseded by this feature:** the earlier rule "markings are kept forever: no retention or purge job
  may be applied to them" no longer holds for a lot whose lot number appears on no receiving row. The
  user's instruction is to "check for lot #s that dont exist and remove the shape / color from the
  list". Removal is scoped to exactly that case and is never applied to a protected lot.
- A protected marking is never recomputed or probed by the automatic path, and is never removed by this
  update.
- The user's clarifying words, as given: "the lot number can be accidently written in worng, causing an
  improper shape/color assignment, if i later edit the row in question i want to then be able to come
  to the heat lot management and update the assigned clr/sharp to etiher fit an existing lot# if the lot
  I changed already exists, or create a new lot #, also it should check for lot #s that dont exist and
  remove the shape / color from the list".

## Open Decisions Awaiting Confirmation

A1 and A7 were the two decisions that were open, and the user answered both on 2026-10-14: a corrected
number that does not exist becomes a new lot, and a lot whose number no longer exists is removed from
the list. What remains is narrower.

1. **How a removed lot is removed - resolved as a deletion.** The user's words are to "remove the shape
   / color from the list". Keeping the lot with its marking cleared was considered and rejected: the
   lot's own uniqueness rule, one row per lot number and supplier, would then stop that number from
   ever being recorded again, which contradicts User Story 3's second scenario, where a removed number
   is received again and treated as one that has never been seen. The lot is therefore deleted and its
   shape and color are freed, and the report names the number with the shape and color that were
   removed, which is the trace that survives.
2. **A3** decides how much history a single run can rewrite, which on a long history could be a large
   number of rows. The report shows the count before anything is written.
3. **The two blank substitutes still disagree, and this feature deliberately does not change that.**
   The receiving flow writes "Refer to Vendor Tag" when a lot number is left empty. The shared blank
   check treats "Nothing Entered" as meaning no lot, and does not recognise "Refer to Vendor Tag". So a
   load saved with no lot number is currently treated as a real lot, and every such load shares a single
   lot and a single marking. The update keeps that definition rather than changing it, because narrowing
   it would change which rows carry markings, which is a data change well beyond this feature. It is
   raised because the update will meet those rows, and because the two values probably ought to agree.
4. **Reprinting.** Because a corrected number gets a correct marking rather than keeping the wrong one,
   a label printed before the correction shows the old marking. Whether the update should also offer to
   reprint is out of scope here and is not handled.
