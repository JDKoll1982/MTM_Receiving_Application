# Dunnage Feature Clarification

To ensure the Dunnage Feature is implemented correctly and aligns with your expectations, please review the following questions.

## 1. Infor Visual Integration
**Context**: The Receiving workflow validates POs and Parts against the Infor Visual ERP. The Dunnage business logic mentions "PO Number" but describes "Part Number / Line 1" as "Free text".
**Question**: Should Dunnage entries be validated against Infor Visual?
- [ ] **A) Yes, validate PO and Part Number against Infor Visual.** (Requires ERP lookup logic)
- [ ] **B) Validate PO only, but allow free-text Part Numbers.** (Ensures PO exists, but flexible parts)
- [ ] **C) No, Dunnage is completely free-text.** (No ERP dependency, fastest entry)
- [ ] **D) No, but provide a lookup/search if available.**

**My Suggestion**: **C) No, Dunnage is completely free-text.**
**Reasoning**: Dunnage (pallets, crates) often doesn't have formal part numbers in the ERP in the same way inventory does. Free-text allows for flexibility (e.g., "Broken Pallet", "Blue Skid").

---

## 2. Label Printing & CSV Export
**Context**: The Receiving workflow saves data to a CSV file for LabelView to pick up and print. The Dunnage document is titled "Dunnage Label System".
**Question**: Should saving Dunnage entries generate a CSV file for label printing?
- [ ] **A) Yes, generate a CSV file for LabelView.** (Same mechanism as Receiving)
- [ ] **B) No, just save to the database.** (Tracking only, no physical labels)
- [ ] **C) Yes, but print directly to a printer.** (Bypassing LabelView)

**My Suggestion**: **A) Yes, generate a CSV file for LabelView.**
**Reasoning**: Consistency with the Receiving workflow and the system name "Dunnage Label System".

---

## 3. Workflow Modes
**Context**: You requested the "same exact modes as ReceivingLabelPage.xaml". Receiving has:
1.  **Mode Selection** (Choose between Wizard, Manual, Edit)
2.  **Wizard Flow** (Step-by-step PO -> Part -> Qty)
3.  **Manual Entry** (Grid-based entry)
4.  **Edit Mode** (Review/Edit history)

**Question**: Which modes apply to Dunnage?
- [ ] **A) All Modes (Wizard, Manual, Edit).** (Full parity with Receiving)
- [ ] **B) Manual Entry & Edit Mode only.** (Skip the wizard, as Dunnage is likely bulk entry)
- [ ] **C) Manual Entry only.** (Simple data entry form)

**My Suggestion**: **B) Manual Entry & Edit Mode only.**
**Reasoning**: The Dunnage business logic describes a "Data Entry Form" (Grid) and "History View". The step-by-step Wizard seems overkill for simple dunnage entry.

---

## 4. Database Storage
**Context**: Receiving data is stored in MySQL tables (`receiving_loads`, etc.).
**Question**: Where should Dunnage data be stored?
- [ ] **A) New MySQL table (e.g., `dunnage_loads`).** (Clean separation)
- [ ] **B) Same `receiving_loads` table with a flag.** (Shared schema)
- [ ] **C) CSV only.** (No database)

**My Suggestion**: **A) New MySQL table (e.g., `dunnage_loads`).**
**Reasoning**: Dunnage fields (Line 1, Line 2) might differ slightly from Receiving fields, and separation prevents polluting the main inventory receiving data.

---

## 5. UI Terminology
**Context**: The requirements mention "Part Number / Line 1" and "Quantity / Line 2".
**Question**: How should the columns be labeled in the UI?
- [ ] **A) "Part Number" and "Quantity"** (Specific)
- [ ] **B) "Line 1" and "Line 2"** (Generic, matches label fields)
- [ ] **C) "Part Number (Line 1)" and "Quantity (Line 2)"** (Both)

**My Suggestion**: **C) "Part Number (Line 1)" and "Quantity (Line 2)"**
**Reasoning**: clear to the user what the data represents, while indicating how it maps to the physical label.

---

## 6. History View Grouping
**Context**: The requirements mention "Visual Feedback (History View)" with date-based row grouping (alternating colors).
**Question**: Should this grouping be implemented in the "Edit Mode" grid?
- [ ] **A) Yes, implement visual grouping in the DataGrid.**
- [ ] **B) No, standard grid is fine.**
- [ ] **C) Use a separate "History View" page.**

**My Suggestion**: **A) Yes, implement visual grouping in the DataGrid.**
**Reasoning**: The requirement explicitly asks for "Visual Feedback... alternate row colors by transaction date".

---

## 7. "Row Clearing Logic"
**Context**: "Trigger: User deletes all critical fields... Action: Clear the entire row".
**Question**: Should this happen automatically as the user types/deletes, or upon leaving the row?
- [ ] **A) Automatically when fields become empty.**
- [ ] **B) When the user leaves the row (RowEditEnding).**
- [ ] **C) Explicit "Clear Row" button only.**

**My Suggestion**: **B) When the user leaves the row.**
**Reasoning**: Auto-clearing while typing can be annoying. Clearing upon completion/navigation is smoother.
