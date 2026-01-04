# Brainstorming Session - Routing Module Overhaul
**Date:** 2026-01-04
**Participants:** John Koll (PM), Architect Agent, UI/UX Agent

## Session Goals
1.  Define the "Greenfield" architecture for the Routing Module.
2.  Solve the "Package First" workflow challenge.
3.  Establish consistency with Receiving/Dunnage modules.

## Key Decisions

### 1. Workflow Reversal ("Package First")
*   **Problem:** Users often have the package (PO) before knowing the recipient.
*   **Solution:** Invert the wizard. Step 1 is now "PO & Line Selection" (DataGrid based), Step 2 is "Recipient Selection".
*   **Benefit:** Matches physical reality. Allows auto-filling description from PO data.

### 2. Global Validation Toggle
*   **Problem:** Some packages have no PO or "weird" data that blocks strict validation.
*   **Solution:** A global "Enable Validation" checkbox in the bottom bar.
*   **Scope:** Persists across the session. Applies to Wizard, Manual, and Edit modes.

### 3. "Quick Add" Recipients
*   **Problem:** 80% of packages go to the same 5 departments (Assembly, Paint, etc.).
*   **Solution:** "Quick Add" buttons in Step 2, populated by a dynamic SQL query (`sp_routing_recipient_get_top`) based on usage frequency.

### 4. Shell Architecture
*   **Problem:** Navigation buttons inside wizard pages create code duplication and inconsistent UI.
*   **Solution:** Adopt the `Dunnage` pattern. The Shell View (`View_Routing_Workflow`) hosts the Bottom Bar (Next, Back, Reset, Help). The ViewModel (`ViewModel_Routing_Workflow`) controls button visibility based on the current step.

### 5. Data Entry Refinement
*   **Problem:** Manual entry of package descriptions is slow and error-prone.
*   **Solution:** Step 1 will use a **DataGrid** to display PO Lines (fetched from Infor Visual). User selects a line to auto-populate the description.

## Action Items
1.  [x] Update PRD with new workflow.
2.  [x] Update Architecture with Shell/ViewModel responsibilities.
3.  [x] Update UI Design with Bottom Bar layout.
4.  [ ] Implement `Service_Routing_Workflow` and `Service_Routing_Session`.
5.  [ ] Create `View_Routing_Workflow` shell.

## Open Questions
*   **Q:** How do we handle "Other" PO reasons?
*   **A:** Inline dropdown in Step 1. If "Other" is selected, prompt for custom text and save to `routing_po_reasons`.

---
*Session closed at 14:30.*






