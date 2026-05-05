# Material Availability Board Print-Out Update Prompt

Update the Material Availability Board summary print-out in `JDKoll1982/MTM_Receiving_Application`.

Research the existing code during implementation and use the current file names, helper methods, model properties, tests, and conventions already in the repo.

## Primary Areas to Investigate

- `Module_ShipRec_Tools/Services/Service_Tool_MaterialAvailabilityBoard.cs`
- The summary print path in `FormatBoardForPrintAsync(...)`
- Existing helpers for:
  - locations
  - incoming material
  - associated parts
  - section/table rendering
  - summary print CSS
- Existing unit tests for `Service_Tool_MaterialAvailabilityBoard`

Do **not** change the transaction-sheet print layout unless required by tests. The requested changes apply to the normal Material Availability Board summary print-out only.

---

# Required Behavior

## 1. Professional Bordered Print Layout

Wrap the summary print content in a main container with:

- `3px solid #333` border
- `20px` padding
- print-friendly font styling

Use a CSS class such as:

```css
.main-border {
    border: 3px solid #333;
    padding: 20px;
    font-family: sans-serif;
}
```

---

## 2. Remove Old Summary Table

Remove the old per-card summary table that displays things like:

- total on hand
- locations
- next summary

Keep the main report title, search subtitle, part/card sections, and detailed tables.

---

## 3. Current Locations Total

In the Current Locations table, add a total/footer row that sums all location quantities.

Requirements:

- Label: `Total on Hand`
- Bold styling
- Uses actual numeric quantity values, not display strings
- Applies a CSS class such as `total-row`

Example CSS:

```css
.total-row {
    background-color: #eee;
    font-weight: bold;
    border-top: 2px solid #333;
}
```

---

## 4. Manual Inventory Transfers Table

Below each Current Locations table, add a section titled:

```text
Manual Inventory Transfers
```

Add a blank handwriting table with columns:

- Time
- From Location
- To Location
- Qty
- Initials

Add exactly **5 blank rows**.

Rows should be tall enough for handwriting, about `40px`, using a class such as:

```css
.manual-row td {
    height: 40px;
    border: 1px solid #999;
}
```

---

## 5. Conditional Incoming Material Section

Only render the Incoming Material section when incoming material data exists.

If the card has no incoming material, omit the entire section:

- no `Incoming Material` heading
- no empty-state message
- no incoming table

Use the existing model properties/collection checks to determine this.

---

## 6. Associated Parts Run Date Fallback

For the Associated Parts table, update the Run Date display to use this fallback order:

1. Scheduled run/start date
2. Forecasted/next due run date
3. `No scheduled date`

Use the existing associated-run model properties found in the codebase.

---

## 7. Unique Part Coloring

Each unique part number must get a print-friendly background color.

Implementation requirements:

- Use a `Dictionary<string, string>` keyed by part number
- Assign classes `p-0` through `p-14`
- If there are more than 15 unique parts, wrap back to `p-0`
- All rows/sections for the same part should share the same color class

Use these CSS classes:

```css
.p-0 { background-color: #E3F2FD !important; }
.p-1 { background-color: #F3E5F5 !important; }
.p-2 { background-color: #E8F5E9 !important; }
.p-3 { background-color: #FFF3E0 !important; }
.p-4 { background-color: #FFEBEE !important; }
.p-5 { background-color: #E0F2F1 !important; }
.p-6 { background-color: #FFF9C4 !important; }
.p-7 { background-color: #FCE4EC !important; }
.p-8 { background-color: #E8EAF6 !important; }
.p-9 { background-color: #ECEFF1 !important; }
.p-10 { background-color: #F1F8E9 !important; }
.p-11 { background-color: #E0F7FA !important; }
.p-12 { background-color: #FFF8E1 !important; }
.p-13 { background-color: #F8BBD0 !important; }
.p-14 { background-color: #EDE7F6 !important; }
```

---

## 8. Print CSS

Update the summary print CSS to include:

```css
@media print {
    body {
        -webkit-print-color-adjust: exact;
        print-color-adjust: exact;
    }
}

table {
    width: 100%;
    border-collapse: collapse;
    margin-bottom: 25px;
    page-break-inside: avoid;
}

th {
    background-color: #333;
    color: white;
    padding: 10px;
    text-align: left;
}

td {
    border: 1px solid #ccc;
    padding: 8px;
}

.total-row {
    background-color: #eee;
    font-weight: bold;
    border-top: 2px solid #333;
}

.manual-row td {
    height: 40px;
    border: 1px solid #999;
}
```

Preserve any existing page-break behavior for print cards.

---

## 9. Tests

Update or add tests for:

- bordered main container
- removal of the old summary table
- Current Locations total row
- Manual Inventory Transfers table with 5 blank rows
- Incoming Material omitted when no incoming data exists
- run-date fallback behavior
- unique part color classes
- summary print CSS includes print color adjustment and all `p-0` through `p-14` classes

Keep existing tests passing.

---

## Implementation Notes

Let the agent research the exact implementation details while coding.

The agent should:

- Find the current print formatter and helper methods.
- Reuse existing HTML helpers where possible.
- Add small private helper methods only if needed.
- Preserve HTML encoding for data-derived values.
- Preserve existing reporting/document model behavior.
- Avoid changing unrelated print flows.
- Keep the transaction-sheet layout unchanged unless tests prove it must be adjusted.

---

## Acceptance Criteria

The updated summary print-out should:

- Display inside a professional bordered container.
- No longer show the old per-card summary table.
- Show a Current Locations table with a calculated `Total on Hand` row.
- Show a `Manual Inventory Transfers` table with 5 blank handwriting rows.
- Hide the Incoming Material section completely when there is no incoming material.
- Display Associated Parts run dates using the correct fallback chain.
- Apply consistent vivid background colors per unique part number.
- Preserve print color adjustment for browser printing.
- Keep existing tests passing with new/updated coverage.