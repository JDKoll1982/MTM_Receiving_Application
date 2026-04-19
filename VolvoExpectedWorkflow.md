# Volvo Module — Expected Workflow

---

## Workflow Steps

### Step 1 — User Enters Module_Volvo

#### 1) Load Parts List

- Query `volvo_line_data` and return all rows that have data in any of the 10 columns.
- Display results in the Parts List.
- See → [Mockup M-1a](#m-1a--parts-list-card)

#### 2) User Click "+ Add Part"

- Open "Add Part to Shipment" Dialog
- User Enters Required data (no refactoring of modal needed)
- User hits "Add" or "Cancel"
  - User hits "Add": same process as before, except instead of populating a datagrid it generates a new card of the new line
  - User hits "Cancel": close dialog box, same as before

#### 3) User Clicks the "Report" Discrepancy Button

- Same logic as before except upon saving update the card of the row with the discrepancy

#### 4) User Clicks the "Remove" Discrepancy Button

- Same logic as before except upon saving update the card of the row with the discrepancy

#### 5) User Clicks the "View History" Button

- Same logic

---

## Refactoring Tasks

### R1 — Replace DataGrid with Collapsible Cards

_Applies to: Step 1a_

- Convert the existing DataGrid into a list of collapsible cards, one per row.
- See → [Mockup M-1a](#m-1a--parts-list-card)

#### Database Change

- Add column `po_status` to the `volvo_line_data` table.
- Allowed values: `Pending`, `Received`.

### R2 - Remove the "View" discrepency button and modal as this is covered in the expanded card.

### R3 - Change the Part Number cell to a button with the Part Number as it's text

- Clicking this will bring up a new modal that will allow the user to change the Part Number only, as the qty can be changed in its cell.
- The modal window must have its own xaml file, xaml.cs file, model and view model

### R4 - Remove the "Generate Labels", "Clear Label Data" and "Save as Pending" buttons

- change logic to save/update to volvo_line_data when the user:
  - Adds new row
  - Updates row's Part Number (see R3) or Quantity Columns
  - Changes row's discrepency status

- If there are any rows then the "Complete Shipment" button should be enabled

### R5 - Change the output going to volvo_label_data and volvo_label_history to the user's employee number not the user's windows login name.

---

## Mockups

### M-1a — Parts List Card

#### Collapsed

```
+--------------------------------------------------------------------+
|                                                                    |
|  {part_number} - {part_description}               [Expand Button]  |
|                                                                    |
|  File Discrepency:                                                 |
|  [Report] [Remove]                                    {po_status}  |
|                                                                    |
+--------------------------------------------------------------------+
```

#### Expanded

```
+--------------------------------------------------------------------+
|                                                                    |
|  {part_number} - {part_description}               [Expand Button]  |
|                                                                    |
|  File Discrepency:                                                 |
|  [Report] [Remove]                                    {po_status}  |
|                                                                    |
+--------------------------------------------------------------------+
|                                                                    |
|  Skids: {received_skid_count}    Qty per Skid: {quantity_per_skid} |
|  Calculated Piece Count: {calculated_piece_count}                  |
|                                                                    |
!-{EVERYTHING BELOW SHOULD ONLY BE SHOWN IF has_discrepency = true} -!
|                        Discrepency Details                         |
| +-----------------+----------------+--------------+--------------+ |
| | Expected Skids  | Skids Received | Expected Qty | Received Qty | |
| +-----------------+----------------+--------------+--------------+ |
| |    {expected}   |   {received}   |  {expected}  |  {received}  | |
| +-----------------+----------------+--------------+--------------+ |
|  Discrepancy Notes: {discrepancy_note}                             |
|                                                                    |
+--------------------------------------------------------------------+
```

#### Styling Rules

**Discrepancy Border (`has_discrepancy`)**
| Condition | Border Color |
|--------------------------|----------------------------------------------------------|
| `has_discrepancy = true` | Warning color visible in both light and dark mode |
| `has_discrepancy = false` | Default border (same as all other cards) |

- When `has_discrepancy = true`: show **Expected Skid Count** and **Discrepancy Notes** fields.
- When `has_discrepancy = false`: hide both fields.

**Card Background (`po_status`)**
| `po_status` Value | Background Color |
|-------------------|---------------------------------------------------------------------|
| `Pending` | Muted yellow — readable text in both light and dark mode |
| `Received` | Muted green — readable text in both light and dark mode |

<!-- Add M-2 here -->

---

## Edge Cases

### EC-1 — No Rows in Parts List

- If `volvo_line_data` returns no rows, the Parts List should display an empty state message (e.g., "Click Add Part to begin transaction.").
- The "Complete Shipment" button must remain **disabled** (per R4).

### EC-2 — Part Number Change Collision (R3)

- If the user attempts to change a Part Number to one that already exists in the current shipment, the modal should display a validation error and prevent saving.

### EC-3 — Quantity Set to Zero or Negative

- If the user edits the Quantity column to `0` or a negative number, the cell should reject the value and revert to the previous value with a validation message.

### EC-4 — Discrepancy Reported then Immediately Removed

- If a user reports a discrepancy and then removes it before any save/sync cycle completes, ensure the card state reflects `has_discrepancy = false` and no stale discrepancy data is persisted to `volvo_line_data`.

### EC-5 — `po_status` Null or Unexpected Value

- If `po_status` is `NULL` or contains an unrecognized value, the card should fall back to the default border and background styling without throwing an error, and the value of po_status should be updated to the default value (false / 0 in mysql) quietly

### EC-6 — Employee Number Not Found (R5)

- If the logged-in user's employee number cannot be resolved when writing to `volvo_label_data` or `volvo_label_history`, the operation should fail gracefully with a user-facing error message and not write a blank or null employee number.

### EC-7 — Add Part Dialog Submitted with Duplicate Part Number

- If the user adds a part via "+ Add Part" whose Part Number already exists in the current list, display a warning. Decide whether to block the add or allow duplicates (document the chosen behavior here once decided).

### EC-8 — Network/Database Failure During Auto-Save (R4)

- If the database is unreachable when an auto-save is triggered (add row, update Part Number/Qty, change discrepancy status), display a non-blocking error notification and allow the user to retry. Search for exisisting shared code for this in Module_Core or Module_Shared

### EC-9 — Rapid Expand/Collapse Toggle

- Rapidly toggling the expand/collapse button should not cause duplicate data loads or visual glitches. Debounce or cancel in-flight requests as needed.

### EC-10 — Complete Shipment with Unresolved Discrepancies - NOT REQUIRED

- If the user clicks "Complete Shipment" while one or more cards have `has_discrepancy = true`, display a confirmation warning listing the affected part numbers. Decide whether to block completion or allow it with acknowledgment (document the chosen behavior here once decided).

### EC-11 — Session Timeout / Loss of Focus During Edit - NOT REQUIRED

- If the application loses focus or the session expires while the user is editing a Quantity cell or the Part Number modal is open, unsaved changes should either be auto-discarded with a notification or queued for retry on restore. Define and document the chosen behavior.

### EC-12 — Part Description Missing or Null - YES

- If `part_description` is `NULL` or empty for a row, the card header should display `{part_number} - (No Description)` or similar fallback text to avoid a broken or confusing layout.

### EC-13 — Calculated Piece Count Overflow or Invalid - YES

- If `received_skid_count` or `quantity_per_skid` contain non-numeric or null values, `calculated_piece_count` should display a fallback (e.g., `N/A`) rather than throwing an arithmetic exception.

### EC-14 — Concurrent Edits from Multiple Sessions - NOT REQUIRED

- If the same shipment is open in two sessions simultaneously (e.g., two workstations), and one session saves a change, the other session's auto-save may overwrite it silently. Consider optimistic concurrency (e.g., row timestamps) and surface a conflict warning to the user.

### EC-15 — Remove Discrepancy When None Exists - DIABLE BUTTON WHEN has_discrepency = false

- If the "Remove" discrepancy button is clicked on a card where `has_discrepancy = false`, the action should be a no-op with no error. Optionally disable the "Remove" button when `has_discrepancy = false` to prevent the action entirely.

### EC-16 — Large Parts List Performance - NOT REQUIRED

- If `volvo_line_data` returns a very large number of rows (e.g., 100+), rendering all cards at once may cause UI lag. Consider virtualizing the card list or loading cards in batches to maintain responsiveness.

### EC-17 — Part Number Modal Closed Mid-Validation - YES

- If the user opens the Part Number change modal, triggers validation (e.g., duplicate check), and then closes the modal before the async check completes, the result of the validation should be discarded and no changes applied.

### EC-18 — `po_status` Updated Externally Between Loads - NOT REQUIRED

- If `po_status` is changed in the database by an external process after the parts list is loaded, the card will display a stale status until refreshed. Consider adding a manual refresh option or a periodic lightweight poll to detect out-of-date status values.

### EC-19 — Complete Shipment Button Enabled with All Rows Having Discrepancies - YES

- If every row in the parts list has `has_discrepancy = true`, the "Complete Shipment" button should still follow the R4 rule (enabled when rows exist), but a visual indicator or tooltip should warn the user that all parts have unresolved discrepancies before they proceed.

### EC-20 — Add Part Dialog Opened While Auto-Save Is In Progress - YES

- If the user opens the "+ Add Part" dialog while an auto-save operation is still in progress (e.g., from a previous quantity edit), the dialog should either wait for the save to complete or queue the new insert to avoid race conditions and duplicate/lost records.

### EC-22 — Quantity Per Skid or Received Skid Count Edited to Non-Integer Value - YES - cell rejects value

- If the user enters a decimal or non-integer value into a quantity field that expects whole numbers, the cell should reject the value, revert to the previous value, and display a validation message indicating only whole numbers are accepted.

### EC-23 — Card Expand State Lost on List Refresh - YES

- If the parts list is refreshed (e.g., after an auto-save or manual reload), all cards may collapse to their default state, losing the user's current expand/collapse state. The UI should preserve or restore the expand state of each card by part number after a refresh.

### EC-25 — `volvo_line_data` Row Deleted Externally While Card Is Expanded - NOT REQUIRED

- If a row is deleted from `volvo_line_data` by an external process while the corresponding card is expanded and the user is viewing it, any subsequent action on that card (e.g., updating quantity, reporting discrepancy) should handle the missing row gracefully with a user-facing error rather than a silent failure or crash.

### EC-26 — Employee Number Resolves to Multiple Records - NOT REQUIRED, system does not allow more than 1 uniuqe employee number already

- If the employee number lookup returns more than one result for the logged-in Windows user, the system should not arbitrarily pick one. Surface an error or disambiguation prompt and prevent writing ambiguous data to `volvo_label_data` or `volvo_label_history`.

### EC-27 — Part Number Modal Opened on a Card Mid-Auto-Save - YES - Wait

- If the user opens the Part Number change modal on a card that is currently mid-auto-save (e.g., a quantity change is being persisted), the modal should either wait for the save to settle or lock the card to prevent conflicting concurrent writes to the same row.
