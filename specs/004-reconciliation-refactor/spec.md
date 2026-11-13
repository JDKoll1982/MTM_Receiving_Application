# Feature Specification: Receiving Validator

**Feature Branch**: `004-reconciliation-refactor`

**Created**: 2026-11-12

**Status**: Draft

**Source**: SProd Offic26111208330.pdf

**Input**: Extract "Reconcile Saved Locations from InforVisual" out of the Receiving module into its own module, and rebuild it as a one-day-at-a-time validator with a per-line review cycle. The decisions captured in `Review.md`, `Review-Rules.md`, and `Review-Clarifications.md` are authoritative and are encoded here as requirements, cited by decision id.

The receiving application saves a location against every receipt it records. Material then moves in Infor Visual, and the saved location drifts from where the material actually went. This feature compares the two and offers a correction, one line at a time, but only where the Infor Visual ledger proves the destination and fully accounts for the quantity.

## User Scenarios & Testing

An operator opens Receiving Validator, lands on today, and sees the day's receipts as a list of lines, each already carrying a proposed outcome. Lines that already match Infor Visual are reported and set aside. Lines whose saved location differs enter a review cycle, one line at a time, where the operator accepts the proposed destination, changes it, or skips the line. Every accepted line is written the moment it is accepted, and only when the movement evidence accounts for that line's quantity. Lines the module cannot answer are collected in a Needs Attention window instead of being guessed at.

The day is the unit of work. The operator steps day by day, never over an open-ended range, and any backlog is cleared as a sweep that repeats the same single-day pass.

### User Story 1 - See what one day holds (Priority: P1)

An operator opens Receiving Validator from the Reporting group and lands on today. The page lists the day's receipt lines, grouped by part and received date, with each line's outcome already worked out: already correct, changed, or not answerable. Stepping to the previous or next day moves to the nearest day that actually has receiving activity, so the operator never lands on an empty screen.

**Why this priority**: every other story reads from this list. It is also worth shipping alone, because it tells the operator what the day looks like before anything is corrected.

**Independent Test**: choose a day with known receipts and no Infor Visual transfers, open the module on that day, and confirm the grid lists exactly those rows, each reported as already correct, with no proposed location offered and nothing written.

**Acceptance Scenarios**:

1. **Given** the operator opens the module, **When** the page loads, **Then** the selected day is today and the list shows that day's receipt lines grouped by part and received date. A line that already matches is not listed and is reached through the outcome filter instead (FR-043, `D-13`).
2. **Given** the operator steps to the next day, **When** the following day has no receiving rows, **Then** the page advances to the next day that has rows and states which day it landed on.
3. **Given** the operator picks a date with the date picker, **When** the day loads, **Then** only rows received on that calendar day are listed.
4. **Given** the selected day has no receiving rows at all, **When** the page loads, **Then** it shows an explicit empty state with a way to step to another day, not an empty grid.
5. **Given** a preview has been produced, **When** the operator has taken no further action, **Then** nothing has been written.

### User Story 2 - Correct one line at a time (Priority: P2)

An operator selects a line whose saved location differs from where Infor Visual put the material. The detail pane shows the saved location, the proposed location, and the evidence behind it. The destination is already filled with the proposal, so the common case is a single keystroke: the operator presses Enter, that line is written, and the selection moves to the next unresolved line. If the proposal is wrong, the operator types the right location first. If the line should not be touched, Escape skips it for this run.

**Why this priority**: this is the correction itself. Without it the page only reports.

**Independent Test**: on a day with three mismatched lines, accept all three with only the keyboard, then reopen the page and confirm each line now reports its saved location as matching, and that a fourth skipped line is unchanged.

**Acceptance Scenarios**:

1. **Given** a line whose saved location differs from the derived location, **When** it is selected, **Then** the detail pane shows the saved location, the proposed location, the evidence, and a destination field pre-set to the proposal.
2. **Given** the proposal is accepted, **When** the operator confirms, **Then** that one line is written immediately and the grid selection advances to the next unresolved line.
3. **Given** the operator changes the destination before accepting, **When** the line is written, **Then** the typed destination is written and the line records that a person set it.
4. **Given** the operator skips a line, **When** the cycle advances, **Then** nothing is written for that line and the line is counted as skipped in the day summary.
5. **Given** the operator closes and reopens the page within the session, **When** the day reloads, **Then** the cycle resumes at the first unresolved line and the accepted and skipped counts are preserved for the session.
6. **Given** the operator presses Enter with the proposal in place, **When** the line is written, **Then** no bulk action and no confirmation dialog stands between the keystroke and the write.

### User Story 3 - Only write when the material is fully accounted for (Priority: P3)

Before a line may be written, the module proves two things. First, every destination that received material from the receipt must be consumed exactly by the lines allocated to it, with nothing left over. Second, whatever remains in the original receiving location is real stock, and it must equal the live on-hand quantity Infor Visual reports there. Material that never moved is not a failure; it is stock that is still where it was received.

**Why this priority**: this is what makes a written correction trustworthy. A correction written on an unproven destination is worse than no correction, because it moves a real row to a wrong place and hides the real problem.

**Independent Test**: replay the reference receipt below through the module and confirm the four destination lines are written, the residual line is reported as unchanged, and the residual figure agrees with the live on-hand at the receiving location.

**Acceptance Scenarios**:

1. **Given** part `MMC0001000` receives 50,000 into `RECV`, and transfers move 5,000 to `SHIP`, 2,500 to `V-C0-01`, 500 to `R-04`, and 1,000 to `R-05`, **When** the day is reconciled and the live on-hand at `RECV` is 41,000, **Then** each of the four destination buckets is consumed exactly to zero, the four matching destination lines are offered `SHIP`, `V-C0-01`, `R-04`, and `R-05`, and the lines totalling 41,000 keep `RECV` and are reported unchanged.
2. **Given** the same receipt, **When** the live on-hand at `RECV` is not 41,000, **Then** the residual lines are not written and the day summary reports the residual disagreement.
3. **Given** a destination bucket keeps any quantity no line can consume, **When** validation runs, **Then** the affected lines are not writable and carry the named outcome that says whether the movement showed too much or too little.
4. **Given** a destination location ran negative during the period, **When** validation runs, **Then** the affected lines are not writable and are never corrected automatically.
5. **Given** a line's quantity matches its bucket exactly, **When** the comparison runs, **Then** it passes with no tolerance and no rounding allowance.
6. **Given** a receipt was fully transferred out of the receiving location, **When** validation runs, **Then** the residual is zero and no line is expected to remain at the receiving location.

### User Story 4 - Triage what the module could not answer (Priority: P4)

Some lines cannot be pre-filled. The movement evidence may show more material than the lines can take, or less. A line's group may have failed against Infor Visual. Those lines are kept out of the review cycle and collected in a Needs Attention window, which is only available when there is something in it. From there the operator can read the reason and the evidence, and, if the operator knows the answer, set the location by hand.

**Why this priority**: unresolved lines must stay visible without polluting the fast cycle. Burying them is how a bad day gets silently accepted.

**Independent Test**: run a day whose evidence shows a surplus, open the Needs Attention window, confirm the affected lines appear there with the difference stated, set one location by hand, and confirm the day summary still counts the untouched ones as unresolved.

**Acceptance Scenarios**:

1. **Given** the day has lines that could not be resolved, **When** the page renders, **Then** the Needs Attention action is enabled and shows the count of such lines.
2. **Given** the day has no unresolved lines, **When** the page renders, **Then** the Needs Attention action is disabled and cannot open an empty window.
3. **Given** the window is open, **When** a line is shown, **Then** it carries its reason, the raw quantity difference where one applies, and the evidence that was found.
4. **Given** the operator sets a location inside the window, **When** the line is written, **Then** it goes through the same gate and the same write path as an accepted line, and records that a person set the value.
5. **Given** a group found no qualifying movement at all, **When** the day finishes, **Then** its lines are not listed in the window, are logged, and are counted in the day summary.

### User Story 5 - Non-purchase-order receipts are assessed, not skipped (Priority: P5)

Receipts with no purchase order cannot be matched on a purchase order, but they are still material that arrived. A separate service assesses them, grouping by part, received date, and vendor, and holding them to the same quantity proof and the same named outcomes. They appear in the same day workspace and the same review cycle as purchase-order lines.

**Why this priority**: skipping them silently leaves a growing share of the dock unvalidated, and the operator has no way to tell the difference.

**Independent Test**: run a day containing only non-purchase-order receipts from one vendor, confirm each line is assessed, and confirm a correct destination is proposed where the evidence supports it and a named unresolved outcome is reported where it does not.

**Acceptance Scenarios**:

1. **Given** a receipt has no purchase order, **When** the day is reconciled, **Then** it is assessed rather than skipped, and it is grouped by part, received date, and vendor.
2. **Given** a non-purchase-order group's evidence accounts for the rows, **When** the cycle runs, **Then** the same destination proposal and the same accept-or-change review apply.
3. **Given** a non-purchase-order group's evidence does not account for the rows, **When** validation runs, **Then** the lines are not writable and carry one of the named unresolved outcomes.
4. **Given** a non-purchase-order receipt has no vendor recorded, **When** it is grouped, **Then** it is reported as unresolved rather than dropped from the day's counts.

### User Story 6 - Clear a backlog a day at a time (Priority: P6)

After a holiday or an outage, the operator sets a start and end date and lets the module work through the days. Each day runs as its own pass with its own progress, the operator can stop between days, and the sweep stops on the first day that fails and leaves the operator on that day so the problem is visible. Days already worked carry a visible mark.

**Why this priority**: it makes the one-day model usable for catching up without reintroducing a multi-day algorithm.

**Independent Test**: sweep a five-day backlog that includes one day with a deliberate Infor Visual failure and confirm the first four days complete in order with marks, the sweep stops on the fifth, and the operator is left on the failing day with the failure stated.

**Acceptance Scenarios**:

1. **Given** a start and end date are set, **When** the sweep runs, **Then** each day is reconciled by the same single-day path, in order, with per-day progress.
2. **Given** the sweep is running, **When** the operator asks to stop, **Then** the current day completes or aborts cleanly and no further day starts.
3. **Given** a day fails, **When** the sweep reaches it, **Then** the sweep stops, the failing day is selected, and the failure is stated on the page.
4. **Given** a day completed, **When** the operator returns to it later in the session, **Then** the day carries a visible completed mark.
5. **Given** the sweep covers ten days, **When** each day validates, **Then** each day uses its own receipt-anchored movement windows and the windows are never widened by the sweep.

### User Story 7 - The feature has its own home and Receiving keeps its own behaviour (Priority: P7)

The validator leaves the Receiving workflow. It appears as its own entry in the Reporting group, with its own colour accent and its own title, and Receiving no longer offers a reconciliation card or a reconciliation step. Receiving keeps the location suggestion it shows while an operator types a receipt, and Receiving's edit mode keeps its own rules for who may change a row.

**Why this priority**: this is the extraction the request asks for. It is last because it removes the old surface, and the new surface must exist first.

**Independent Test**: open the application, confirm the validator is reachable only from its own entry, confirm the Receiving mode selection no longer offers reconciliation, and confirm entry-time location suggestion still returns a suggestion for a known part and purchase order.

**Acceptance Scenarios**:

1. **Given** the application is open, **When** the operator looks at navigation, **Then** the validator appears once, in the Reporting group, labelled as Receiving Validator.
2. **Given** the Receiving mode selection page, **When** it renders, **Then** the reconciliation card and its command are gone and nothing else on the page has moved.
3. **Given** the Receiving workflow, **When** an operator works a normal receipt, **Then** no reconciliation step is offered and entry-time location suggestion still works.
4. **Given** the validator is open, **When** its header renders, **Then** its title and accent come from its own module identity and differ from Receiving's.
5. **Given** the Receiving module is built without the validator, **When** the solution builds, **Then** it succeeds, and the reverse is also true.

### Edge Cases

- **A day with no receiving rows.** The page shows an empty state with a way to another day. The stepper skips that day entirely when moving.
- **A day with rows but no Infor Visual movement.** No line is offered a destination, each is reported as not found, and the day summary carries a not-found count so this day is distinguishable from a day where nothing was found for one line only.
- **All rows already correct.** The page says so explicitly; the review cycle does not open, and the Needs Attention action stays disabled.
- **Every line skipped.** The day summary reports every line as skipped and states that nothing was written.
- **A bucket keeps quantity no line can take.** The affected lines are not writable. The named outcome says which way the numbers diverged, and the raw difference is stated.
- **A destination ran negative.** The affected lines are not writable and are never corrected automatically, because a negative destination means material was consumed ahead of its transfer.
- **The residual disagrees with live on-hand.** The residual lines are not written and the disagreement is reported. It is not silently corrected.
- **A chained move passes through staging.** If the staging location fully forwards, it drops out of the destinations. If it keeps quantity, it stays an eligible destination for that remaining quantity.
- **The same purchase order and part on two dates.** They are two independent groups and never compete for the same movement quantity.
- **The same purchase order and part on one date.** They are one group, as they are today.
- **A reprint row.** It is excluded from the candidates and counted in the log, so one physical skid cannot consume movement quantity twice.
- **A group holding more than one unit of measure.** Units are not blocked and not gated, so the group is reconciled and a warning is logged. The unit is shown in the detail pane so a reviewer can see why a line looks wrong.
- **A weight-based part whose rounded quantity does not match a decimal movement.** The line lands in an unresolved outcome. This is accepted for now and measured on the first live day.
- **A row a previous run already corrected.** It reports as already correct. Distinguishing it from a row that was always correct is deferred with the audit tables.
- **A line whose saved location matches the derived location.** It is reported as unchanged and never enters the review cycle, even when movement evidence exists.
- **Infor Visual unreachable.** The page shows an inline retry banner, the day fails visibly, and the operator is not thrown back to mode selection. A sweep stops on that day.
- **The application database unreachable.** The day fails with a stated reason and nothing is written.
- **A per-group failure.** Only that group's lines are marked as errored; the rest of the day still resolves.
- **A stored workflow step number.** Removing the old workflow step is only safe once nothing persists its numeric value. If something does, the member stays reserved with a comment.
- **A rejected write.** If the single write procedure reports failure, the line stays unresolved, the failure is stated, and the cycle does not advance as if it succeeded.

## Requirements

### What This Replaces

Today the feature is a workflow step inside Receiving. These artifacts are the starting point, and their disposition is a requirement, not a suggestion:

- A single location reconciliation service in Receiving holds the whole algorithm in one file, measured at about 700 lines on 2026-11-12. It is split into focused collaborators, not moved as one file.
- Its contract holds both the entry-time recommendation call and the reconcile calls. The recommendation call is used by the Receiving load entry screen and stays in Receiving. The reconcile calls leave.
- Two reconciliation models, a reconciliation review view model, and a reconciliation review dialog are deleted and replaced by the new module's own models and views.
- Receiving's workflow step `ReconciliationReview = 12`, the mode selection card, and the card's command are removed.
- Four Infor Visual query files back the feature: `16` for location evidence, `17` for transaction history, and the `18` family for transfer movements and their pair details. Query `16` also backs the entry-time recommendation and stays in Receiving. The `18` family moves. Query `17` is deleted only if it has no callers.
- Two Receiving settings drive the old behaviour: validate-all-history, and an ignored-locations list. Neither survives in the new module.

### Module Boundary (Hard Requirements)

The separation is the point of the change, and it is enforced the same way as the rest of the repository's architecture rules. Decisions `MG-05`, `SC-01`, and rules R-MOD-07 and R-MOD-08 govern it:

- The validator is its own module, named with the full word, never an abbreviation.
- The new module may depend on the shared foundation only: core infrastructure and shared presentation base. It may not depend on Receiving.
- Receiving may not depend on the new module either.
- What the two modules share is physical storage, never code. Each owns its own data access and its own stored procedures, including procedure names.
- Exactly one reconciliation-owned write procedure touches receiving rows, and that procedure is the single point of contact between the modules.
- The entry-time location suggestion stays in Receiving with its behaviour unchanged.

### Functional Requirements

**Module boundary and ownership**

- **FR-001**: The feature MUST live in its own module whose folder, namespace, and types use the full word for reconciliation and no abbreviation (`SC-01`, R-NAM-03).
- **FR-002**: The new module MUST NOT reference the Receiving module in any form: no project reference, no namespace import, no injected Receiving service, and no shared model, enum, constant, data access class, stored procedure name, or settings key (R-MOD-07, `MG-05`).
- **FR-003**: The Receiving module MUST NOT reference the new module, directly or through settings or a shared contract (R-MOD-04).
- **FR-004**: Each module MUST own its own models, data access, services, stored procedures, views, view models, converters, settings keys, navigation entry, header title provider, and accent triple (R-MOD-07, `MG-06`).
- **FR-005**: The two modules MAY read and write the same physical receiving tables, but only through their own stored procedures, and the validator MUST reach those rows through exactly one reconciliation-owned write procedure (R-MOD-08, `MG-05`).
- **FR-006**: The entry-time location recommendation MUST remain in Receiving with its result unchanged, and the reconcile methods MUST leave Receiving with no second reconciliation implementation left behind (`SC-02`).

**Day scope and stepping**

- **FR-007**: The unit of work MUST be exactly one calendar day, and no run may reconcile a span in a single pass (`UI-02`).
- **FR-008**: The working day MUST be chosen with a date picker, a previous-day control, a next-day control, and Today, and a new session MUST open on today (`UI-02`).
- **FR-009**: The previous and next controls MUST move only to days that have receiving rows for this module's selection, determined from a cheap activity index rather than by probing a full reconciliation, and Today MUST remain reachable in one action from any position (`UI-02`).
- **FR-010**: "No activity" MUST mean no receiving rows for the day, never no Infor Visual transfers (`UI-02`).
- **FR-011**: A sweep MUST repeat the identical single-day pass one day at a time, show per-day progress, stop between days on request, stop on the first day that errors, and leave the operator on that day (`UI-02`, `AR-08`).
- **FR-012**: A day that completed in the current session MUST carry a visible completed mark (`UI-02`).
- **FR-013**: Row selection MUST use the date column pair chosen by the deferred date-column investigation, and that choice MUST be recorded as a decision entry before implementation tasks begin (`DS-01`, R-1, R-2, R-3).
- **FR-014**: Movement discovery MUST stay anchored on the receipt date plus and minus two days, strict validation MUST stay at the receipt date and the receipt date plus one day, both windows MUST be configurable, and a sweep MUST NOT widen either (`LG-10`).

**Grouping and movement evidence**

- **FR-015**: Candidate rows MUST be grouped by purchase order, part, and received date, so two receipts of one purchase order and part on the same day stay one group and the same pair on two days becomes two independent groups (`LG-01`).
- **FR-016**: Reprint rows MUST be excluded from the candidate set, and the excluded count MUST be written to the log (`LG-11`).
- **FR-017**: Chained movements MUST resolve to the net destination: an intermediate holding location that fully forwards drops out, and one that retains quantity stays an eligible destination for that remainder (R-SCOPE-02).
- **FR-018**: Allocation MUST be deterministic: rows ordered by quantity descending, then load number, then load id; each row takes an exact-fit destination where one exists and otherwise the smallest destination that still fully covers it; ties break by latest movement time, then location id (R-SCOPE-02).
- **FR-019**: Current-label and history rows for the same part and receipt date MUST continue to reconcile together in one pass, never separated (`R-SCOPE-02`).
- **FR-020**: Non-purchase-order rows MUST NOT be skipped. A separate service MUST assess them, grouped by part, received date, and vendor, under the same quantity proof and the same named outcomes (`LG-13`).
- **FR-021**: Every row loaded for the day MUST appear in exactly one resolution, and the resolution counts MUST sum to the rows loaded.

**The quantity proof**

- **FR-022**: Every destination bucket MUST be consumed exactly to zero by the rows allocated to it, and a non-zero remainder MUST leave the affected rows not writable (`LG-04` Part 1).
- **FR-023**: Whatever remains in the original receiving location MUST be treated as legitimate stock rather than a failure, and it MUST equal the live on-hand quantity for that part at that location (`LG-04` Part 2, `LG-03`).
- **FR-024**: Where the residual does not equal live on-hand, the residual lines MUST NOT be written, and the disagreement MUST be reported on the page and in the day summary (`LG-04`).
- **FR-025**: A destination that ran negative during the period MUST leave the affected rows not writable, and the module MUST NOT correct them automatically (`LG-04`, R-DATA-09).
- **FR-026**: Quantity comparison MUST be exact, with no tolerance, no epsilon, and no percentage allowance (`LG-07`).
- **FR-027**: The quantity used by the proof MUST be the row's integer quantity, and the decimal weight quantity MUST NOT be used for the proof (`LG-05`).
- **FR-028**: Unit of measure MUST NOT block allocation: no unit gate and no mixed-unit guard. A group holding more than one unit MUST log a warning, and the unit MUST appear in the detail pane (`LG-06`).
- **FR-029**: A first-pass current-inventory comparison MUST short-circuit any line whose saved location already matches the derived location to unchanged, before allocation runs (`LG-03`).
- **FR-030**: Movement quantity MUST be derived from the inventory ledger: quantity is always positive, direction comes from the transaction type, and a location transfer is the matched in and out pair linked by the transfer identifier. Sign MUST NOT be inferred from the quantity value (R-DATA-09).
- **FR-031**: The module MUST NOT require a receipt to be fully consumed by movement, because material that stayed in the receiving location is real stock (R-DATA-09, `LG-04`).

**Named outcomes**

- **FR-032**: Each line MUST carry exactly one outcome, drawn from: unchanged, updated, extra surplus, not enough, not found, skipped, error, and manually set. The old single "ambiguous" outcome MUST NOT be used, and legacy references to it mean the extra-surplus and not-enough family (`LG-16`, C-08).
- **FR-033**: The extra-surplus and not-enough outcomes MUST name which way the numbers diverged, and MUST carry the raw difference in the reason text (`LG-16`).
- **FR-034**: A group with no qualifying movement MUST NOT be surfaced in the review, MUST be logged with part, day, and group, and MUST be counted in the day summary (`LG-15`).
- **FR-035**: The module MUST NOT consult or apply an ignored-locations list, and the ignored-by-settings outcome MUST NOT exist (`LG-14`).
- **FR-036**: A line whose saved location already matches the derived location MUST be reported as unchanged and MUST NOT enter the review cycle.
- **FR-037**: A row already corrected by an earlier run MUST report as unchanged. The distinct already-reconciled outcome is deferred with the audit tables and MUST NOT be presented as available (`LG-12`, `DS-07`).

**Preview, review, and apply**

- **FR-038**: Previewing a day MUST never write (`UI-03`).
- **FR-039**: Only lines whose saved location differs from the derived location MUST enter the review cycle, and each MUST be presented with the destination pre-set to the proposal (`UI-03`).
- **FR-040**: A reviewer MUST be able to change the destination before accepting or skip the line. A skip MUST apply to this run only and MUST resurface in the day summary (`UI-03`).
- **FR-041**: Accepting a line MUST write that line immediately, MUST NOT offer an end-of-run bulk apply, and MUST refuse to write a line that failed the quantity proof (`UI-03`, `DS-06`).
- **FR-042**: The review surface MUST be a page made of a grid and a detail pane, and the grid MUST show exactly three columns: part, purchase order number, and quantity (`UI-04`, `UI-05`). All remaining detail MUST live in the pane: description, purchase order line, load or skid, unit, saved location, proposed location, outcome, the Infor Visual user, movement time, movement count, evidence locations, and the reason.
- **FR-043**: The page MUST support filtering by outcome and by part or purchase order text, and MUST group by part and received date (`UI-06`). The outcome filter MUST offer every one of the eight named outcomes, including the unchanged outcome, so the lines that already match — which the default list does not show — remain reachable. Decided with the operator on 2026-11-13: matching lines are for checking rather than working, so they are filtered for, never listed by default.
- **FR-044**: The cycle MUST show progress as line N of M together with accepted and skipped counts, and MUST be resumable within the session (`UI-08`).
- **FR-045**: Enter MUST accept the pre-set proposal and Escape MUST skip the current line (`UI-08`).
- **FR-046**: Lines that cannot be pre-filled MUST NOT enter the cycle. They MUST be reachable through a Needs Attention action that is disabled whenever the count is zero (`UI-07`).
- **FR-047**: The Needs Attention surface MUST be a modal window, not a dialog, MUST show each line's reason, evidence, and raw difference where one applies, and MUST allow a location to be set by hand, written through the same accepted-line path and recorded as manually set (`UI-07`).
- **FR-048**: Empty and failure states MUST be page-level: no rows for the day, all lines already match, all lines skipped, Infor Visual unreachable, application database unreachable, and residual disagreement. An Infor Visual failure MUST surface as an inline retry banner and MUST NOT throw the operator back to mode selection (`UI-09`).
- **FR-049**: The day summary MUST show scanned, written, unchanged, unresolved, skipped, error, and not-found counts; the day reconciled; run duration; the Infor Visual user attribution; and a copyable run id. It MUST NOT offer any export action (`UI-10`, `AR-10`).
- **FR-050**: Line state MUST be conveyed by row colour, not by an additional status column (`UI-05`).
- **FR-051**: Every interactive control MUST be keyboard reachable with an accessible name, and every binding MUST be a compiled binding with an explicit mode (R-MVVM-03, R-MVVM-05).

**Writing corrections**

- **FR-052**: The module MUST be able to correct a saved location on a row created by any employee or Windows user, through its own write procedure, while Receiving's edit-mode ownership rule stays untouched (`DS-06`, `MG-05`).
- **FR-053**: "May correct any row" MUST NOT weaken the review: every write still requires acceptance of that line and a passing quantity proof (`DS-06`, C-06).
- **FR-054**: A write MUST require an authenticated user whose permission to correct other users' rows has been approved. [NEEDS CLARIFICATION: whether any authenticated user may correct other users' rows or a supervisor role is required. No role gate is assumed in this draft; the review flow and the gate apply either way.]

**Data, persistence, and integrity**

- **FR-055**: This change MUST NOT add tables or columns. Runs and changes are recorded in logs only, and the reconciliation state tables stay deferred to a separately approved change (`AR-07`, `DS-07`).
- **FR-056**: New stored procedures ARE required. Every write to the application database MUST go through a stored procedure owned by this module, named in the module's own namespace in the style of its neighbours, with no ad-hoc write statement in application code (R-DATA-03, `MG-05`).
- **FR-057**: Infor Visual MUST remain strictly read only: every Visual query is a select, and the connection is opened with read-only intent (R-DATA-04).
- **FR-058**: Quantity comparisons MUST NOT round-trip through single or double precision, and numeric values in messages and logs MUST format with the invariant culture (R-SCOPE-05).

**Scaffolding, settings, and identity**

- **FR-059**: The module's settings MUST use its own key namespace, MUST be added through the settings core facade, and MUST expose the movement discovery window and the strict validation window. Its settings page MUST live in a dedicated settings project for this module, never inside the working module (R-SET-01, R-SET-03, `AR-05`, C-10).
- **FR-060**: The old Receiving keys MUST NOT carry over: the validate-all-history key disappears with the day scope, and the ignored-locations key is not migrated (`AR-04`, `LG-14`, C-10).
- **FR-061**: The module's services, view models, and views MUST be registered through the infrastructure dependency injection extensions, never in the application host (R-DI-01, R-DI-02).
- **FR-062**: The module MUST appear once in navigation, in the Reporting group, labelled as Receiving Validator with "from Infor Visual" as supporting text, and MUST carry its own accent triple in the shared accent brushes with its own module accent mapping, distinct from Receiving's (`SC-05`, `SC-09`, `AR-11`, R-HDR-04). Titles MUST come through a header title provider, never written directly by a view or view model (R-HDR-01).

**Removing the old surface**

- **FR-063**: The Receiving workflow step for reconciliation review MUST be removed, and that removal MUST follow a check that nothing persists its numeric value (`SC-04`, `TS-06`, R-8).
- **FR-064**: The Receiving mode selection card, its command, and its accessible name MUST be removed without disturbing the rest of the page (`SC-06`).
- **FR-065**: The reconciliation host inside the Receiving workflow view MUST be removed (`MG-01`).
- **FR-066**: The old reconciliation models, the old review view model, the old review dialog, and the old review view model tests MUST be deleted, and the existing service tests MUST be split rather than copied: the recommendation test stays in Receiving, the allocation tests are rewritten in the new module (`TS-02`, `MG-01`).
- **FR-067**: Query `16` MUST stay in Receiving. The `18` family MUST move under a reconciliation query folder without renumbering. Query `17` MUST be deleted only after its caller count is confirmed at zero (`AR-09`, `DS-02`, R-7).

**Operations**

- **FR-068**: A failure in one group MUST be isolated to that group's lines, MUST get one bounded retry per Infor Visual call, MUST be reported in a run-level summary, and MUST make the affected day fail visibly (R-ERR-01, `NF-02`).
- **FR-069**: Every log line in a run MUST carry the run id, and a failure MUST name the part, the day, and the group rather than the run alone (R-ERR-03, `AR-10`).
- **FR-070**: A day of up to 500 rows MUST complete within 15 seconds on the shop floor terminal, and the grid MUST stay usable above 200 rows through virtualization (`NF-01`).
- **FR-071**: The reconciliation service MUST be split into focused collaborators covering allocation, movement selection, destination buckets, group resolution, and preview orchestration. It MUST NOT move as one large file (R-CODE-01, R-CODE-02).

### Out of Scope in This Slice

- Label presence and print integrity findings, in every form, including unprinted labels, duplicate labels, and reprint abuse. Decision `UI-13` places those with the reprint module or a separate audit module.
- Export of any kind from the day summary, including CSV and spreadsheet, per decision `UI-10`.
- Reconciliation run, change, and finding tables, per-row change audit, undo, and finding disposition. These arrive with a separately approved change (`DS-07`, `AR-07`).
- The already-reconciled outcome, which depends on those tables (`LG-12`).
- An ignored-locations list and every outcome that depended on it (`LG-14`).
- Pluggable data sources for other modules. One receiving data source is in scope; a provider framework is not (`SC-08`, R-MOD-06).
- Any write to Infor Visual, and any live Infor Visual dependency in the automated test suite (R-DATA-04, `TS-03`).
- Shared extraction of the day stepper, review card, or attention list into shared presentation. They are built in this module, and extracted only when a second real consumer exists (`MG-06`, R-MOD-06).

## Key Entities

- **Receiving row**: one saved receipt line as the receiving application records it. Holds part, purchase order and line, received date, quantity, decimal weight quantity, unit, saved location, vendor, and whether it is a reprint. The row is the unit the operator corrects.
- **Reconciliation group**: the set of receiving rows that compete for one pool of movement evidence, identified by purchase order, part, and received date, or by part, received date, and vendor for receipts without a purchase order.
- **Movement**: one Infor Visual inventory transaction, carrying part, location, direction, quantity, transaction type, transaction time, the user who posted it, and the transfer link that pairs an out leg with its in leg.
- **Destination bucket**: a location that received net quantity from the group's receipt after chained movements are resolved, with the quantity it received and the rows allocated to it. Consumed to zero or the rows are not writable.
- **Source residual**: the quantity still held at the original receiving location after movements, which must agree with live on-hand at that location.
- **Live location balance**: the current on-hand quantity for a part at a warehouse and location, which is the authority the residual is checked against.
- **Finding**: one row's outcome, its reason, the raw difference where one applies, and the evidence behind it.
- **Day**: one calendar day of receiving rows, its activity state, whether it completed, and its counts.
- **Run**: one pass over one day, identified by a run id, with its start and finish, duration, counts, and the Infor Visual user attribution.
- **Window setting**: the movement discovery window and the strict validation window, owned by this module.

## Success Criteria

### Measurable Outcomes

- **SC-001**: For part `MMC0001000` with 50,000 received into `RECV` and 5,000, 2,500, 500, and 1,000 moved to `SHIP`, `V-C0-01`, `R-04`, and `R-05`, the module writes exactly four destination changes, reports the lines totalling 41,000 as unchanged at `RECV`, and reports zero non-writable lines when live on-hand at `RECV` is 41,000.
- **SC-002**: Across a thirty-day sample, zero locations are written for a line whose destination bucket or residual check failed. Every correction has a passing proof.
- **SC-003**: For any day, the outcomes reported sum exactly to the rows loaded, and no row is missing from the counts.
- **SC-004**: A day of 500 rows completes within 15 seconds on the shop floor terminal, and the grid remains scrollable at 250 rows without freezing the page.
- **SC-005**: Stepping forward and backward across a month never lands on a day with no receiving rows, and Today returns to today from any position in one action.
- **SC-006**: A thirty-day sweep stops on the first failing day, and the operator sees which day failed and why.
- **SC-007**: Re-running the same day immediately after writing corrections produces zero further writes.
- **SC-008**: The solution builds and its tests pass with either module present and the other absent, demonstrating that neither depends on the other.
- **SC-009**: Entry-time location suggestion returns the same recommendation as before the change for the same part, purchase order, line, and date: a regression check.
- **SC-010**: All sixty-six decisions in the register map to at least one requirement in this specification, and none is contradicted by one.
- **SC-011**: An operator clears a day of twenty mismatched lines in under five minutes using only Enter for correct proposals, with zero bulk actions.
- **SC-012**: The day summary offers no export action, and no label-print finding is produced anywhere in the module.

## Assumptions

- **Research lands in plan, never as a blocker.** The ten investigations deferred by the register are closed during the plan and research phase, and each becomes a decision entry beside the design it justifies before implementation tasks begin.
- **Inconclusive research takes the stated default.** Where an investigation cannot decide, the register's default applies rather than stalling.
- **The reference quantity set is the calibration case.** Part `MMC0001000` with 50,000 in, 9,000 moved, and a 41,000 residual is the worked example every quantity rule is checked against.
- **Today means today.** A new session opens on the calendar day the operator is working in, and the work is not carried across days by default.
- **The integer quantity is accepted with its known cost.** Because the proof uses the rounded integer quantity, ignores unit of measure, and demands exact equality, weight-based parts will produce more non-writable lines than a decimal comparison would. One live day is measured before the choice is revisited.
- **Non-purchase-order grouping by part, received date, and vendor is the confirmed working shape.** The register proposes it and asks for confirmation rather than restating only.
- **Any authenticated user may correct another user's rows until a role is agreed.** No supervisor gate is assumed in this draft. This is the one open business decision, carried by the clarification marker on FR-054.
- **No settings values are migrated.** With the ignored-locations list removed and the all-history flag replaced by the day scope, there is nothing left to copy. The two new window settings start at the values the code uses today: discovery at two days either side, strict at one day.
- **A dedicated settings project is still justified.** Two settings and their validation live outside the working module because settings user interface is never hosted inside a module, and a later slice will add more.
- **Existing resolution names survive where they still mean something.** Skipped, not found, unchanged, and updated behave as they do today, except that not found is no longer surfaced.
- **Legacy references to the ambiguous outcome mean the extra-surplus and not-enough family.** Where an older document or test names the ambiguous outcome, both named outcomes are intended.
- **The reprint exclusion is visible only in logs.** Reprint rows are not shown to the operator, so the log is the only place their count can be recovered.
- **One receiving data source is in scope.** Dunnage and Volvo sources are not, and the seam is named without being built.
- **The automated suite does not reach Infor Visual.** Visual queries stay manually validated, and the service is tested against fakes.

### Research Carried Into Plan

Each item below is expected to change a detail, not the shape of the extraction. None blocks this specification.

| Id | Investigation | What it settles |
| --- | --- | --- |
| R-1 | Compare the candidate received, transaction, and created date columns in the application database for completeness and for what the operator means by "the day I received this" | FR-013 |
| R-2 | Check the Infor Visual receiver tables against the schema reference, then probe read only: which columns carry quantity, date, purchase order, line, and part, and whether a clean join to a receiving row exists | FR-013, and whether a true received quantity is available at all |
| R-3 | Confirm what the Visual transaction date means for receipts versus transfers | FR-013, FR-014 |
| R-4 | Confirm live location balances cover every destination the module will propose, and reflect the same day's movements | FR-023, FR-024 |
| R-5 | Measure the non-writable rate for one day with the integer quantity, exact equality, and no unit gate | Whether FR-027 changes to the decimal quantity with a fallback |
| R-6 | Confirm the transfer pair query split still has the right shape after the quantity proof and the receiver decision | FR-067 |
| R-7 | Confirm the transaction history query has zero callers before deleting it | FR-067 |
| R-8 | Search for any numeric persistence of the old workflow step before removing the member | FR-063 |
| R-9 | Determine whether the Receiving proposed user interface change documents are reconciliation specific | Removal boundary |
| R-10 | Confirm which stored project memories name the Receiving reconciliation path explicitly | Removal boundary |

An inconclusive outcome takes the register's default. For R-2 that default is no receiver query: the receiving rows remain the proxy, and the residual check compares against live on-hand only.

## Verbatim Constraints

These values are pinned by the request or the decision record and MUST be used exactly as written. Do not paraphrase, re-case, pluralize, or rename them.

| What | Exact value | Where it appears |
| --- | --- | --- |
| Module name | `Module_Reconciliation` | Project folder, namespace, and every type prefix |
| Feature label | `Receiving Validator` | Navigation entry and page title |
| Supporting text | `from Infor Visual` | Beside the feature label |
| Outcome names | `Unchanged`, `Updated`, `Extra Surplus`, `Not Enough`, `NotFound`, `Skipped`, `Error`, `Manually Set` | Every line's outcome, the filters, and the summary |
| Reference part | `MMC0001000` | The calibration case and its acceptance scenario |
| Reference locations | `RECV`, `SHIP`, `V-C0-01`, `R-04`, `R-05` | The calibration case |
| Query folder | `Queries/Reconciliation/` | Where the moved movement queries live |
| Settings key prefix | `Reconciliation.` | The module's own settings namespace |
