# Feature Specification: Receiving By Receiver Mode

**Feature Branch**: `007-receiving-by-receiver`

**Created**: 2026-11-23

**Status**: Draft

**Input**: User description: "i want to create a new mode for Module_Receiving called 'By Receiver'.
This will share the same guided workflow as 'Guided' but instead of entering a PO number the user will
get a table of the last # of receivers entered into visual, showing 1) Receiver Number, 2) PO #, 3) Line #,
4) Part Number, 5) Vendor, 6) Location Received to, 7) Qty Received, 8) Qty Ordered, 9) Qty Remaining on
Line. They should also be able to enter a Receiver number into a search box and have the list filter to
that Receiver. Once a line on the table is selected take all the required information that the following
guided steps require and pass them forward. The table of receivers should always be ordered by date
received in, never show receivers that do not contain part numbers, only contain receivers where
inventory is being added, never removed from the system."

Clarifications recorded with the requester: the mode control sits between Guided and Manual; a receiver
with several lines for the same part collapses to one row; the row count is a user-chosen page size with
paging instead of scrolling; the guided walk resumes at Load Entry with location and remaining quantity
filled in; remaining quantity never shows below zero; the search matches any part of the number.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Start a guided entry from a receiver instead of a PO number (Priority: P1)

An operator receives material that has already been entered into Infor Visual by someone else and needs
to walk the same receiving steps to log loads and print labels. Today that operator must know the PO
number to get anywhere, so they leave the application, look the receiver up, come back, and type the PO
number from memory. In the new mode the operator opens Receiving, switches to "By Receiver", sees the
most recent receivers, picks the line that matches the pallet in front of them, and lands on Load Entry
with the location and the remaining quantity already filled in.

**Why this priority**: It removes the only hard prerequisite of the guided workflow, which is knowing a
PO number, and it is the reason the feature exists.

**Independent Test**: Enter Receiving, switch to "By Receiver", select one row, and confirm the guided
walk resumes at Load Entry with that row's part, location and remaining quantity already present, then
complete the walk through Review and save.

**Acceptance Scenarios**:

1. **Given** the operator is in Receiving, **When** they look at the mode controls, **Then** a "By
   Receiver" control sits between "Guided" and "Manual".
2. **Given** the operator activates "By Receiver", **When** the screen settles, **Then** the receiver
   list is shown instead of the "Enter PO" step.
3. **Given** the receiver list is shown, **When** the operator selects a row, **Then** the workflow
   carries that row's receiver number, PO number, line number, part number, vendor, location received
   to, quantity received, quantity ordered and quantity remaining, and the next screen is Load Entry.
4. **Given** the operator arrived at Load Entry from a receiver row, **When** the screen is shown,
   **Then** the location received to and the quantity remaining are already filled in from that row.
5. **Given** the operator is on Load Entry arrived from a receiver row, **When** they use Back,
   **Then** the receiver list returns with the row they picked still highlighted.
6. **Given** the operator arrived from a receiver row, **When** they complete the walk through Review
   and save, **Then** the saved entry is tied to the same receiver number, PO number, line number and
   part number the row carried.

---

### User Story 2 - Find one receiver by typing its number (Priority: P1)

An operator holding a receiver document reads its number and wants that receiver, not a list. They type
the number into a search box above the list and the list narrows to matching receivers as they type.

**Why this priority**: Without it the operator scrolls pages looking for a number they already have,
which is slower than the PO lookup the feature replaces.

**Independent Test**: Type a known receiver number into the search box and confirm only receivers whose
number contains those digits remain listed, then clear the box and confirm the full list returns.

**Acceptance Scenarios**:

1. **Given** the receiver list is shown, **When** the operator types part of a receiver number,
   **Then** only rows whose receiver number contains the typed digits remain.
2. **Given** a search is narrowing the list, **When** the operator clears the search box, **Then** the
   full list returns and the view is back on the first page.
3. **Given** the search matches nothing, **When** the list settles, **Then** an empty-state message is
   shown and the application does not error.
4. **Given** a search matches more rows than fit one page, **When** the results are shown, **Then** the
   pager reflects the filtered count, not the unfiltered count.

---

### User Story 3 - Only inventory receipts appear, newest first (Priority: P2)

The list is a working surface, so every row on it must be something the operator can actually receive
against. Receivers that never added inventory, receivers with no part number, and receivers removed from
the system are noise that leads to dead ends.

**Why this priority**: It protects the operator from selecting a row that cannot carry a part through
the guided steps, and it is easy to regress silently.

**Independent Test**: Compare the rows shown against the same window queried directly in Infor Visual
and confirm no row lacks a part number or a receiving location, none carries a removal marker, and the
newest received date is first.

**Acceptance Scenarios**:

1. **Given** the receiver list is shown, **When** any row is inspected, **Then** its part number and its
   location received to are populated.
2. **Given** the receiver list is shown, **When** any row is inspected, **Then** its receiver is not
   marked for removal in the source system.
3. **Given** the receiver list is shown, **When** the rows are read top to bottom, **Then** received date
   never increases and rows with the same date are ordered by receiver number, newest first.
4. **Given** a receiver has several lines for the same part, **When** the list is shown, **Then** that
   receiver and part appear as one row rather than several.

---

### User Story 4 - Choose how many receivers a page holds (Priority: P2)

The operator decides how much of the list they want on screen. A page-size control offers a small set of
counts, and the list pages rather than scrolling, so the newest receivers are always in a known place.

**Why this priority**: It keeps the list usable on the shop-floor displays this application runs on,
where a long scrolling list is hard to operate.

**Independent Test**: Change the page size, confirm the row count on screen matches the choice, page
forward and back, and confirm the receiver range shown stays consistent.

**Acceptance Scenarios**:

1. **Given** the receiver list is shown, **When** the operator chooses a different page size, **Then**
   the list redisplays from the first page with that many rows per page.
2. **Given** more receivers match than the page size, **When** the operator moves to the next page,
   **Then** the next set of receivers is shown and the page indicator updates.
3. **Given** the filtered result fits on one page, **When** the list is shown, **Then** paging controls
   are present but inactive rather than hidden or erroring.
4. **Given** the operator pages forward and then selects a row, **When** the workflow advances, **Then**
   the row selected is the one from the page they were on.

---

### User Story 5 - Keep "By Receiver" as a personal default (Priority: P3)

An operator who always starts from a receiver should not switch modes every morning. "By Receiver"
becomes one of the modes they can choose as their default in Receiving settings, and Receiving opens in
that mode for them afterwards.

**Why this priority**: It is a convenience on top of a working mode, and the mode is usable without it.

**Independent Test**: Set "By Receiver" as the default in Receiving settings, reopen Receiving, and
confirm the receiver list appears without touching the mode controls. Then set the default back.

**Acceptance Scenarios**:

1. **Given** the Receiving workflow default settings, **When** the mode list is shown, **Then** "By
   Receiver" is one of the choices.
2. **Given** a user's stored default is "By Receiver", **When** they open Receiving, **Then** the
   receiver list is the first screen.
3. **Given** a user's stored default is "By Receiver", **When** they switch to "Guided" and back,
   **Then** no error occurs and the receiver list reloads.

---

### Edge Cases

- A receiver whose PO line no longer exists in the source system must not appear, because the row has no
  part number to carry forward.
- A receiver over-received past its ordered quantity shows a remaining quantity of zero, never a
  negative number.
- A receiver with several lines for the same part collapses to one row; its quantity received is the sum
  of those lines.
- A receiver with several distinct part numbers appears as several rows, each with its own PO line.
- A location received to that is blank on the line, while the warehouse is filled, still counts as an
  inventory receipt and shows the warehouse alone.
- Changing the page size or the search text while on a later page returns the view to the first page.
- A source-system timeout or failure shows an error the operator can read and retry from, and never
  leaves the mode blank.
- Re-entering the mode reloads the list, so a receiver entered while the application was open appears.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The Receiving mode controls MUST offer a "By Receiver" mode positioned between "Guided"
  and "Manual".
- **FR-002**: Selecting "By Receiver" MUST show the receiver list as the first workflow screen in place
  of the "Enter PO" screen.
- **FR-003**: The receiver list MUST show these columns, in this order: Receiver Number, PO #, Line #,
  Part Number, Vendor, Location Received to, Qty Received, Qty Ordered, Qty Remaining on Line.
- **FR-004**: The list MUST be ordered by received date, newest first, with receiver number descending as
  the tie-breaker.
- **FR-005**: The list MUST exclude every receiver line that has no part number.
- **FR-006**: The list MUST include only receivers where inventory is being added, meaning the receiving
  line names a warehouse and location; receipts with neither MUST be excluded.
- **FR-007**: The list MUST exclude receivers marked for removal or purge in the source system.
- **FR-008**: A receiver with several lines for the same part MUST appear as one row, and Qty Received
  for that row MUST be the sum of those lines.
- **FR-009**: A search box above the list MUST filter rows to those whose receiver number contains the
  typed characters, and clearing it MUST restore the full list.
- **FR-010**: The list MUST page rather than scroll, with a page-size control offering a small set of row
  counts and a pager showing the current page and the total.
- **FR-011**: Selecting a row MUST carry receiver number, PO number, line number, part number, vendor,
  location received to, qty received, qty ordered and qty remaining into the receiving workflow state.
- **FR-012**: After a row is selected the workflow MUST resume at Load Entry, with location received to
  and quantity remaining pre-filled from that row.
- **FR-013**: Qty Remaining on Line MUST be ordered quantity minus total received quantity on that PO
  line, and MUST show zero instead of a negative value.
- **FR-014**: Back from Load Entry, when the walk began from a receiver row, MUST return to the receiver
  list with that row still highlighted.
- **FR-015**: "By Receiver" MUST be selectable as a user's stored default receiving mode, and Receiving
  MUST open in it for a user who chose it.
- **FR-016**: The source system MUST be read only; the feature MUST issue no write against Infor Visual.
- **FR-017**: The list MUST reload each time the mode is entered and MUST be refreshable without leaving
  the mode.
- **FR-018**: A source-system failure MUST be reported to the operator as a readable message with a way
  to retry.

### Key Entities *(include if feature involves data)*

- **Receiver row**: one receivable unit of work. Carries receiver number, received date, PO number, PO
  line number, part number, vendor name, warehouse and location received to, quantity received,
  quantity ordered, and quantity remaining. It is a view of source-system data, not stored locally.
- **Receiving workflow session**: the existing guided walk. Gains the receiver row that started it, so
  later steps and the saved record can reference the receiver number.
- **PO line**: the ordered quantity and total received quantity a row's remaining quantity is derived
  from.
- **Receiving mode**: the operator's chosen entry point into the workflow; now includes "By Receiver" and
  can be stored as a personal default.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An operator can start a guided entry from a receiver row in three actions or fewer (mode,
  row, Next).
- **SC-002**: The receiver list shows its default page within two seconds on the shop-floor hardware for
  a 50-row page.
- **SC-003**: Typing into the search box narrows the list within one second for a full receiver history.
- **SC-004**: In a sample of 100 listed rows, zero rows lack a part number, zero rows lack a receiving
  location, and zero rows carry a removal marker.
- **SC-005**: In the same sample, received date never increases down the list.
- **SC-006**: Zero writes are issued against Infor Visual while the feature is exercised end to end.
- **SC-007**: An operator who sets "By Receiver" as their default reaches the receiver list on opening
  Receiving in 100% of attempts.

## Assumptions

- Default page size is 50 rows, with 25, 50, 100 and 200 offered in the page-size control.
- Search matching is a contains match on the receiver number, applied on the already-filtered list, so it
  never surfaces a receiver the inventory or removal rules exclude.
- Merging duplicate part lines within one receiver uses the first PO line found for that part as the
  displayed line number.
- Returning from Load Entry to the list keeps the previously selected row highlighted but requires no
  re-selection unless the operator wants a different row.
- The receiver number carried into the session is informational on later steps; the saved record keeps
  the existing receiving schema and does not require a new database column.
- The list loads on mode entry; no background polling is added.
- Display of vendor uses the vendor name, falling back to the vendor identifier when no name exists.

Out of scope for this feature:

- The Guided mode's existing PO entry path stays exactly as it is.
- The Manual and Edit receiving modes are unchanged.
- No data is written to the source system, and no local receiving schema changes.
- No receiving history browsing, reporting, or export of the receiver list is added.

## Verbatim Constraints

Mode control label: `By Receiver`

Column headers, exactly as written, in this order:

- `Receiver Number`
- `PO #`
- `Line #`
- `Part Number`
- `Vendor`
- `Location Received to`
- `Qty Received`
- `Qty Ordered`
- `Qty Remaining on Line`