---
stepsCompleted: [1]
inputDocuments: ['wizard-workflow-design.md']
session_topic: 'Creative ways to speedup data entry, UI consistency, and robust error handling for the Routing Wizard'
session_goals: 'Generate ideas for faster data entry, ensure UI alignment with Receiving/Dunnage modules, and design robust error handling strategies'
selected_approach: 'Progressive Flow'
techniques_used: ['SCAMPER Method']
ideas_generated: []
context_file: 'wizard-workflow-design.md'
---

# Brainstorming Session Results

**Facilitator:** John Koll
**Date:** 2026-01-04

## Phase 1: Expansive Exploration - SCAMPER Method

We are using SCAMPER to generate ideas for speeding up data entry, improving UI consistency, and handling errors in the Routing Wizard.

### SCAMPER Prompts & Ideas

**S - SUBSTITUTE**
*   **Idea:** Substitute typing with scanning (Barcode/QR) for PO numbers and potentially Recipient badges.
*   **Decision:** Keep "Next" button (don't auto-advance).

**C - COMBINE**
*   **Decision:** Keep steps distinct (don't combine Recipient/Details).
*   **Decision:** Remove print action entirely (CSV/DB only).
*   **Decision:** Keep error handling simple.

**A - ADAPT**
*   **Insight:** Leverage existing modular features from Receiving/Dunnage rather than reinventing. Ensure consistency by reusing shared components.

**M - MODIFY**
*   **Idea:** Optimize layout to reduce eye movement (e.g., Z-pattern or F-pattern scanning, aligning inputs).
*   **Idea:** Streamline the "Other" PO reason flow (make it less intrusive, perhaps inline).
*   **Idea:** Use distinct shapes/colors for primary actions (e.g., "Next" vs "Cancel") to improve affordance.

**P - PUT TO OTHER USES**
*   **Idea:** Use History view for "Edit Mode" (modifying past labels).
*   **Idea (New):** Use Recipient list data to track "Most Frequent" or "Recently Used" per user to auto-sort the list (Smart Sorting).

**E - ELIMINATE**
*   **Decision:** Keep all current steps (Recipient -> Details -> Review).
*   **Decision:** Keep the Review screen (essential for verification).
*   **Decision:** Do NOT eliminate mouse support (PC-first workflow).

**R - REVERSE**
*   **Idea:** Explore a "Package First" workflow (Details -> Recipient). This might help if the user has the box in front of them (PO/Desc) before they know where it's going.
*   **Constraint:** Avoid complex reverse-lookups against Infor Visual (too intrusive/complex). Keep manual entry for POs.

## Phase 3: Idea Development - Solution Matrix

**Strategic Decision:** Complete rewrite of `Module_Routing`. We will replace existing files rather than refactoring, allowing for optimal architecture without legacy constraints.

### Prioritization Matrix

**High Impact / Baseline Effort (Core Features):**
*   **"Package First" Workflow:** Details -> Recipient -> Review. (Now easier to implement as a fresh build).
*   **Quick Add Buttons (Top 5):** Essential for speed.
*   **Smart Sorting:** Essential for speed.
*   **Bottom Navigation:** Standard for consistency.
*   **Inline "Other" PO Reason:** Critical for flow.

**Low Impact / Low Effort (Polish):**
*   **Distinct Colors/Shapes:** Standard UI practice.

**Discarded:**
*   **Refactoring existing ViewModels:** No longer necessary due to rewrite strategy.

## Phase 4: Action Planning - Decision Tree Mapping

We will now map out the implementation plan for the new module.

**1. Foundation (Database Cleanup & Reorganization)**
*   **Safety First:**
    *   **Backup:** Create a full `mysqldump` of the current database before touching anything.
    *   **Mapping:** Create a `database_migration_map.md` to explicitly map every existing file/object to its new location.
*   **Analysis & Validation:**
    *   **Source of Truth:** Analyze `Deploy-Database.ps1` to identify exactly which files are currently being deployed.
    *   **Database Probing:** Use CLI scripts to probe the live database structure to ensure accuracy when creating new schema files.
    *   **Validation:** Ensure new consolidated schema files perfectly match the logic of the old files before deletion.
*   **Consolidation & Structure:**
    *   **Schema:** Create consolidated schema files in `Database/Schemas/`:
        *   `01_core.sql` (Auth, Users, Common)
        *   `02_receiving.sql`
        *   `03_dunnage.sql`
        *   `04_routing.sql`
    *   **Stored Procedures:** Organize into `Database/StoredProcedures/{Module}/` with consistent naming (`sp_{module}_{entity}_{action}`).
*   **Deployment Update:**
    *   Update `Deploy-Database.ps1` to reference the new consolidated file structure.
    *   Remove obsolete SQL files only after validation.

**2. Core Logic (Services & DAOs)**
*   Create `Dao_Routing_POReason`.
*   Update `Dao_Routing_Recipient` (Top 5 logic).
*   Create `Service_Routing_Wizard` (State machine for the new workflow).

**3. UI Construction (Views & ViewModels)**
*   **Step 1: Details View** (PO, Desc, "Other" logic).
*   **Step 2: Recipient View** (Search + Quick Add Buttons).
*   **Step 3: Review View** (Summary + Submit).
*   **Shell:** `View_Routing_Wizard` (Holds the steps + Bottom Nav).

**4. Integration**
*   Hook up `View_Routing_Workflow` to switch between Mode Select / Wizard / History.
*   Register everything in `App.xaml.cs`.

## Session Conclusion

The brainstorming session has concluded with a clear, actionable plan for a complete rewrite of the Routing Module and a comprehensive database reorganization. The focus is on speed, consistency, and safety.

**Next Steps:**
1.  Execute the Database Cleanup & Reorganization plan (Phase 4.1).
2.  Implement the new Routing Module following the "Package First" workflow.

*   **Step 1: Details View** (PO, Desc, "Other" logic).
*   **Step 2: Recipient View** (Search + Quick Add Buttons).
*   **Step 3: Review View** (Summary + Submit).
*   **Shell:** `View_Routing_Wizard` (Holds the steps + Bottom Nav).

**4. Integration**
*   Hook up `View_Routing_Workflow` to switch between Mode Select / Wizard / History.
*   Register everything in `App.xaml.cs`.




