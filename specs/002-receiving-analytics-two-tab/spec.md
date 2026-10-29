# Feature Specification: Receiving Analytics App Data Tab

**Feature Branch**: `002-receiving-analytics-two-tab`

**Created**: 2026-10-29

**Status**: Draft

**Input**: Receiving Analytics becomes two tabs. The existing tab keeps reading Infor Visual. A new
tab reads the receiving application's own records and answers how much work the dock actually did,
when, how well, and how much of it the app captured.

## User Scenarios & Testing

The Receiving Analytics page answers two different questions, so it becomes two tabs.

**ERP Data** is what the page does today. It reads Infor Visual and answers what the business
received and what is still coming. Its filters and charts do not change.

**App Data** is new. It reads the receiving application's own records and answers what the dock
actually did, when, how, and how well. It needs its own filters because it needs a different date
range.

The value sits in App Data: Infor Visual knows what arrived at the plant, but only the app knows
what the operators completed, and whether the work was done on time. Four panels deliver that in
this slice:

| Panel | Catalog item | Question it answers |
| --- | --- | --- |
| How busy were we | 1, 2, 3 | How many loads, how many labels, and what kind of material |
| Were we on time | 11 | Did the material arrive before the PO due date |
| Who is filling our dock | 20 | Which suppliers account for the work |
| How much does our app actually see | 8, 9 | How much of the real work the app captures, and whether it classifies it the way the ERP does |

### User Story 1 - See the dock's own activity by day (Priority: P1)

An operations lead opens Receiving Analytics and switches to App Data. They pick last week and see
how many loads were worked each day, how many labels were printed and applied, and how much
material came in by category. They can switch the same panel between daily, weekly and monthly
without losing the range. For the first time the answer comes from the app's own records instead of
the ERP's.

**Why this priority**: every other panel aggregates from the same facts, so this is the foundation.
It also stands alone: it answers "how busy were we" with no ERP involvement at all.

**Independent Test**: pick a week with known receiving activity, open App Data for that week, and
compare loads, labels and category totals against a hand count of completed receipts. Then open ERP
Data and confirm its charts and filters behave exactly as before. Passing this proves the app can
report its own work.

**Acceptance Scenarios**:

1. **Given** the page is open, **When** the lead selects the App Data tab, **Then** the page shows
   loads per day, labels per day, and quantity by category for the selected range, and ERP Data is
   still reachable with its own filters untouched.
2. **Given** the activity panel is showing daily figures, **When** the lead switches it to weekly or
   monthly, **Then** the same range is re-bucketed and the range itself does not change.
3. **Given** a range containing receipts still in progress, **When** the panel renders, **Then** the
   in-progress receipts are excluded and the panel states that the figures cover completed work.
4. **Given** a range with no completed receipts, **When** the panel renders, **Then** it shows an
   explicit empty state rather than a chart of zeros that reads as a drop in activity.
5. **Given** the range holds material in more than one unit, **When** the quantity panel renders,
   **Then** it states the unit it is totalling and totals only that unit.

### User Story 2 - See whether receipts met the PO due date (Priority: P2)

The lead looks at the same tab and sees, for each day or week, how much material arrived early, on
time, or late against its purchase order due date. The on-time percentage is compared to a target
and turns green when it meets it and red when it does not, so a bad week is obvious without reading
numbers.

**Why this priority**: this is the panel that turns a volume dashboard into a performance one. It
relies on the same completed receipts as Story 1 and adds one measure, the due date.

**Independent Test**: take a sample of one hundred completed receipts with due dates, classify each
by hand as early, on time or late, and compare the panel's buckets and percentage to that count.
Passing this proves the classification and the target colouring are trustworthy.

**Acceptance Scenarios**:

1. **Given** completed receipts that carry a PO due date, **When** the panel renders, **Then** each
   receipt is stacked as early, on time, or late against that due date, and the three buckets add up
   to the receipts that had a due date.
2. **Given** a receipt completed on its due date, **When** it is classified, **Then** it counts as on
   time.
3. **Given** a configured on-time target of 95 per cent, **When** the period's on-time share is 96
   per cent, **Then** the figure is shown in green, and at 94 per cent it is shown in red, with the
   target in force displayed beside it.
4. **Given** receipts with no PO due date, **When** the panel renders, **Then** they appear as a
   separate remainder and are never counted as on time.
5. **Given** no target has been configured, **When** the panel renders, **Then** the percentage is
   shown plainly with no colour and no implied pass or fail.

### User Story 3 - See which suppliers fill the dock (Priority: P3)

The lead sees the suppliers ranked by how much work they generate, and how much of the total the
leading few account for. The ranking can be read by labels, by quantity, or by loads, so the same
chart answers both "who sends us the most paperwork" and "who sends us the most material".

**Why this priority**: it names the parties the dock deals with and gives the lead a short list to
act on. It is an independent read of the same completed receipts and adds no dependency on the ERP.

**Independent Test**: rank by labels for a month with known supplier activity and compare the order,
the leading suppliers' counts, and the cumulative share against a hand count. Then switch the
measure to quantity and confirm the order changes plausibly and the shares still reach 100 per cent.

**Acceptance Scenarios**:

1. **Given** completed receipts for the range, **When** the supplier panel renders, **Then** it ranks
   suppliers by labels, and the reader can switch the ranking to quantity or to loads.
2. **Given** a ranking, **When** the leading suppliers are shown, **Then** each shows the cumulative
   share it adds up to, so the top few can be read as a proportion of the whole.
3. **Given** suppliers beyond the ranked few, **When** the panel renders, **Then** they are grouped
   into one remainder so the shares total 100 per cent.
4. **Given** a receipt with no supplier recorded, **When** the ranking is built, **Then** that work is
   ranked as Unknown rather than silently dropped from the total.
5. **Given** the ranking is by quantity and the range holds several units, **When** the panel
   renders, **Then** only the selected unit is summed and the unit is named.

### User Story 4 - See how much of the real work the app captured (Priority: P4)

The lead sees two lines on one chart for the same dates: what the app recorded and what Infor Visual
recorded. Beneath it, the app's category split sits beside the ERP's split. Together they show how
much of the plant's receiving the app actually sees, and whether it sorts material the same way the
ERP does.

**Why this priority**: this is the strongest evidence that the tab is worth having, and it is the
only part of the slice that needs both tabs. It is sequenced last because it is only meaningful once
the app-side totals are right, and because it is the one panel that depends on a settled translation
between the two systems' category names.

**Independent Test**: pick one closed month, take the app's receipt count per week from Story 1, take
Infor Visual's count for the same weeks, and confirm the two lines carry the same numbers as the
underlying records, and that both category splits name the same five categories and each sums to 100
per cent. Passing this proves the app's coverage of real work is measurable.

**Acceptance Scenarios**:

1. **Given** the App Data range is set, **When** the coverage panel renders, **Then** it draws two
   lines over the same dates, one for the app's recorded receipts and one for Infor Visual's, counted
   the same way.
2. **Given** the mix panel renders, **When** the reader compares the two sides, **Then** both sides
   name the same five categories and each side's shares total 100 per cent.
3. **Given** Infor Visual records a category the app has no counterpart for, **When** the mix panel
   renders, **Then** that category appears as its own segment on the ERP side rather than being
   merged into another category or dropped.
4. **Given** the ERP side is fetched for a comparison, **When** the request is made, **Then** it uses
   the identical date range and filters as the app side, so the two lines can never describe
   different periods.
5. **Given** a part number that carries the coil or flat marker in the middle rather than at the
   start, **When** the two sides classify it, **Then** both tabs use the same single translation, so
   the disagreement shows up as a real difference between the systems and not as a defect in the page.

### Edge Cases

- **A quantity in more than one unit.** Pounds, pieces and coils cannot be added. Twelve pieces plus
  three thousand pounds plus five coils is not a total, so every quantity total is either filtered to
  one unit or split into one line per unit. A panel that cannot pick a unit must not show a single
  combined figure.
- **A receipt with no unit recorded.** Older rows leave the unit blank. The blank is read as each, and
  the total is labelled accordingly, so a total is never left unnamed.
- **Work still in progress.** The app's reporting surface today unions completed receipts with
  in-progress work. App Data reads only the completed side, otherwise today always reads low and
  a good day looks like a bad one.
- **A receipt with no load number.** Older rows have no load, and the label number stands in as the
  load. On those rows the load count and the label count are the same figure. The number of affected
  receipts in range is disclosed so a reader can tell a real coincidence from an artefact.
- **A receipt with no PO due date.** There is nothing to be early or late against. It is excluded from
  the on-time buckets, shown as a remainder, and never counted as on time.
- **A receipt with no supplier recorded.** It is ranked as Unknown, not dropped, so the ranking still
  adds up to the period's work.
- **A received date with no time.** The date-only transaction day is used, and the receipt still
  counts once. A receipt must never be counted twice because it has both fields.
- **A range that spans years.** Weekly and monthly views must stay readable, and the running share
  must still land on 100 per cent, including when the range holds a single receipt.
- **Two units tied for dominance.** The panel opens on a defined choice rather than an arbitrary one,
  and the unit in force is always visible.
- **The ERP side is unavailable.** The comparison panels report that the ERP side could not be read
  and show the app side alone. The remaining panels keep working.
- **No target configured.** Red and green mean nothing without a target, so the panel shows plain
  numbers instead of inventing a pass mark.

## Requirements

### Functional Requirements

- **FR-001**: The page MUST present exactly two tabs, ERP Data and App Data, with ERP Data first and
  the tab that opens by default.
- **FR-002**: The ERP Data tab MUST keep its current filters, charts and behaviour unchanged.
- **FR-003**: Each tab MUST hold its own filter state and date range. Changing one tab's filters MUST
  NOT alter the other's, and both MUST survive switching between tabs while the page is open.
- **FR-004**: App Data MUST report completed receipts only. Work still in progress MUST be excluded
  from every panel, and each panel MUST state that its figures cover completed work.
- **FR-005**: App Data MUST be read only against both databases.
- **FR-006**: App Data MUST show four panels in this order: How busy were we, Were we on time, Who is
  filling our dock, How much does our app actually see. All four MUST fit the default window without
  scrolling.
- **FR-007**: The activity panel MUST show loads per day, labels per day, and quantity by category,
  each switchable between daily, weekly and monthly over the selected range.
- **FR-008**: A load MUST count once per distinct completed load, and a label MUST count once per
  printed and applied label, so the two figures remain distinguishable.
- **FR-009**: The quantity panel MUST show one stacked bar per period with one segment per material
  category in scope.
- **FR-010**: The on-time panel MUST classify each completed receipt against its PO due date as
  early, on time, or late. A receipt completed on its due date MUST count as on time.
- **FR-011**: Receipts with no PO due date MUST be reported as a separate remainder and MUST NOT be
  counted as on time.
- **FR-012**: The on-time percentage MUST be receipts received on or before the due date divided by
  receipts that carry a due date.
- **FR-013**: Where a target is configured, the on-time percentage MUST be shown green at or above the
  target and red below it, with the target in force displayed beside it. [NEEDS CLARIFICATION: the
  numeric on-time target the business wants to hold the dock to; 95 per cent is assumed until
  confirmed.]
- **FR-014**: An administrator MUST be able to change the on-time target through configuration,
  without a rebuild. Where no target is configured, the percentage MUST be shown plainly with no
  colour.
- **FR-015**: The supplier panel MUST rank suppliers by labels, quantity, or loads, defaulting to
  labels.
- **FR-016**: The leading suppliers MUST carry a cumulative share showing what the top few account
  for.
- **FR-017**: Suppliers beyond the ranked few MUST be grouped into one remainder, so the shares total
  100 per cent.
- **FR-018**: A receipt with no supplier recorded MUST be ranked as Unknown rather than excluded.
- **FR-019**: A quantity ranking MUST sum within a single unit, following FR-021.
- **FR-020**: Every quantity total on App Data MUST name its unit, built from the receipt's stored
  unit of measure, for example Quantity (LB), Quantity (EA), Quantity (COILS).
- **FR-021**: No total MUST add across units. The reader selects a unit and only receipts in that unit
  are summed. Where a single unit cannot be chosen, the panel MUST show a separate line per unit.
- **FR-022**: A receipt with no unit recorded MUST be read as each, and its total labelled accordingly.
- **FR-023**: App Data MUST NOT compare weight against quantity, because the two stored numbers are
  the same operator entry, one unrounded and one rounded. No panel MUST present one as a check on the
  other.
- **FR-024**: All App Data time buckets MUST follow the receipt's received date and time, falling back
  to the date-only transaction day where the time is missing. A receipt MUST be counted once, never
  twice.
- **FR-025**: The coverage panel MUST draw two lines over the same dates, one counting the receipts
  the app recorded and one counting the receipts Infor Visual recorded, counted the same way.
  [NEEDS CLARIFICATION: what the two lines count. One app label against one ERP receipt line is
  assumed; if the ERP records one line per PO line while the app records one per label, the two can
  only be compared after a stated alignment rule.]
- **FR-026**: The mix panel MUST show the app's category split beside Infor Visual's split for the
  same period, naming the same five categories on both sides.
- **FR-027**: The translation between app categories and Infor Visual categories MUST live in exactly
  one place, and both tabs MUST read it, so the two tabs can never disagree.
- **FR-028**: Both sides of a comparison MUST be requested with identical dates and filters.
- **FR-029**: Infor Visual categories with no app counterpart MUST appear as their own segments rather
  than being merged into another category or dropped.
- **FR-030**: No panel on App Data MUST show an individual person's name or employee number.
- **FR-031**: The change MUST use new database objects only. Existing tables MUST NOT be altered.
- **FR-032**: Generated views, functions and triggers MAY be used to produce the totals.
- **FR-033**: Infor Visual MUST remain read only. The page MUST NOT issue an INSERT, UPDATE or DELETE
  against it, and its connection MUST be opened with read-only intent.
- **FR-034**: Every read of the app's own database MUST go through a stored procedure. No ad-hoc SQL
  statement MUST be issued from application code.
- **FR-035**: Where a receipt has no load number and the label number stands in as the load, App Data
  MUST report how many receipts in range were affected, so identical load and label figures can be
  recognised as an artefact.

**Out of scope in this slice.** Catalog items 5, 6, 7, 12, 21, 24, 25, 26, 28, 29, 30, 31, 32, 33,
39, 40, 41, 42, 46, 48, 49 and 50 are excluded. In particular, item 39 marking coverage is deferred
with its target, so no marking-coverage figure or marking target is built here; item 7, 33 and 46
report on named individuals and stay out; and item 25 packaging density is excluded, which removes the
one place a weight comparison could have crept back in.

### Key Entities

- **Completed receipt**: one receiving line that has been filed away, holding the material, the
  quantity with its unit, the supplier, the receipt's date and time, the PO due date, and the load and
  label it belonged to. This is the single source for every App Data panel.
- **Load**: the grouping of labels received together. Identified by its load number, which is absent on
  older rows.
- **Label**: one printed and applied label, one per completed receipt in this slice.
- **Material category**: how a receipt is sorted on each side. The app sorts into Standard, Coil and
  Sheet; Infor Visual sorts into Parts, MMC Coils, MMF Flat, Outside Service and Uninventoried.
- **Category translation**: the one definition that maps an app category to an ERP category and back,
  including the categories that have no counterpart on the other side.
- **Supplier**: the vendor named on the receipt. May be blank, in which case the work is attributed to
  Unknown.
- **Analysis target**: the business goal, currently the on-time percentage, with its value and whether
  it is set. Drives the red and green marking.

## Success Criteria

### Measurable Outcomes

- **SC-001**: All four App Data panels are visible at once in the default window, with no scrolling:
  pass or fail, verified on the shop floor terminal.
- **SC-002**: Zero quantity totals on the page mix units. Every total names its unit.
- **SC-003**: Both category splits name the same five categories, and each side's shares sum to 100
  per cent within one decimal place.
- **SC-004**: A thirty day range returns all four panels within three seconds on the shop floor
  terminal, with the comparison panels reading the ERP side concurrently.
- **SC-005**: The on-time percentage matches a hand count of a hundred sampled receipts exactly, with
  zero misclassified.
- **SC-006**: The same range produced twice, and from either tab, yields identical figures for every
  shared measure.
- **SC-007**: Every receipt carrying no load number is counted and disclosed in a single figure, so a
  reader can tell whether identical load and label totals are real.
- **SC-008**: For identical inputs, the ERP Data tab returns the same charts and figures as before the
  change: pass or fail regression check.
- **SC-009**: Switching tabs ten times, changing filters in between, leaves each tab's filters as the
  user last set them, with zero cross tab leakage.

## Assumptions

- **Completed means filed away.** A receipt counts once it has moved into the receiving history. Rows
  still sitting in the working label data are in-progress work and are excluded.
- **The app is the source for App Data.** Every App Data panel except the two comparison panels reads
  only the app's own records.
- **App Data opens on the last thirty days, ending today.** The ERP tab keeps its current range,
  which reaches into the future because it also forecasts.
- **The dominant unit is the default.** Where the range holds several units, the quantity panel opens
  on the unit carrying the most completed receipts, and on each when that cannot be decided.
- **The on-time target starts at 95 per cent** and is confirmed with the business before release. It
  is configuration, not code.
- **Early counts as kept.** A receipt that arrives before its due date meets the promise, so the
  on-time percentage counts receipts on or before the due date.
- **The comparison counts receipts, one app label to one ERP receipt line.** Coverage compares volume
  of work, not material, because only a count can be compared without resolving units.
- **Suppliers are the vendor name as recorded on the receipt,** grouped case-insensitively, with
  whitespace trimmed.
- **Per-person figures are acceptable on a shared terminal,** but none are in scope here, so nothing
  currently displays an individual name.

## Verbatim Constraints

These values are pinned and MUST be used exactly as written. Do not paraphrase, re-case, pluralize or
rename them.

| What | Exact value | Where it appears |
| --- | --- | --- |
| Tab labels | `ERP Data`, `App Data` | The two tab headers, in that order |
| Panel titles | `How busy were we`, `Were we on time`, `Who is filling our dock`, `How much does our app actually see` | The four App Data panel headings, in that order |
| App category names | `Standard`, `Coil`, `Sheet` | The app side of every category figure |
| ERP category names | `Parts`, `MMC Coils`, `MMF Flat`, `Outside Service`, `Uninventoried` | The ERP side of every category figure |
| Quantity label pattern | `Quantity (LB)`, `Quantity (EA)`, `Quantity (COILS)` | Every quantity total, unit in capitals inside parentheses |
| Missing-unit reading | `EA` | Applied when the stored unit is blank |
| Supplier fallback | `Unknown` | Ranked label for receipts with no supplier |
| ERP connection intent | `ApplicationIntent=ReadOnly` | The Infor Visual connection used by the ERP tab and the comparison panels |
| Receipt date and time column | `received_date` | Preferred time source for App Data buckets |
| Date-only column | `transaction_date` | Fallback when `received_date` has no time |
| Completed work table | `receiving_history` | The only app table App Data counts from |
| Quantity columns | `quantity`, `weight_quantity` | Read together, never compared against each other |
| Unit column | `unit_of_measure` | Source of every unit in a quantity label |
| Due date column | `po_due_date` | Source of the early, on time and late classification |
| Load and label columns | `load_number`, `label_number` | Source of the loads and labels counts |
| Supplier column | `vendor_name` | Source of every supplier ranking |
| Category column | `part_type` | Source of the app side of every category figure |
| App category rule | `MMC` for Coil, `MMF` for Sheet, matched by contains | How the app side decides a category |
| ERP category rule | `MMC` for MMC Coils, `MMF` for MMF Flat, matched by starts with | How the ERP side decides a category |
