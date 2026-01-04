---
title: Product Requirements Document (PRD) - Routing Module Overhaul
status: Draft
author: John Koll (PM)
date: 2026-01-04
---

# Product Requirements Document (PRD)
## Routing Module Overhaul

### 1. Executive Summary
The Routing Module is a critical component of the MTM Receiving Application, responsible for generating internal routing labels for packages. The current implementation is a placeholder or legacy system that needs a complete overhaul to align with the modern WinUI 3 architecture, improve data entry speed, and ensure data integrity. This PRD outlines the requirements for a "Greenfield" rewrite of the module, introducing a "Package First" wizard workflow, smart data entry features, and a robust database foundation.

### 2. Problem Statement
*   **Legacy Constraints:** The current system (or lack thereof) does not leverage the full capabilities of the WinUI 3 framework.
*   **Inefficient Workflow:** The traditional "Recipient First" workflow does not match the physical reality of receiving, where the user often has the package (PO/Description) before knowing the final destination.
*   **Data Integrity:** Lack of strict validation and "Other" reason tracking for missing POs leads to data quality issues.
*   **Inconsistency:** The UI/UX does not match the polished experience of the Receiving and Dunnage modules.

### 3. Goals & Objectives
*   **Speed:** Reduce the time to create a label by 30% through "Quick Add" buttons and smart sorting.
*   **Usability:** Align the workflow with physical processes ("Package First").
*   **Consistency:** Ensure the UI matches the Receiving/Dunnage modules (Bottom Navigation, Visual Style).
*   **Reliability:** Establish a clean, normalized database schema with robust error handling.

### 4. User Personas
*   **Receiving Clerk:** Primary user. Fast-paced environment. Needs to generate labels quickly with minimal clicks. Values keyboard shortcuts and smart defaults.
*   **Administrator:** Needs to manage the list of recipients and "Other" PO reasons.

### 5. Functional Requirements

#### 5.1. Mode Selection & Global Controls
*   **Global Controls (Application Shell):**
    *   **Location:** A persistent **Bottom Bar** within the `MainWindow` shell, visible across all Routing Module views.
    *   **Components:**
        *   **Navigation Buttons:** "Next" and "Back" buttons (visibility controlled by current step).
        *   **Action Buttons:** "Reset CSV" (if applicable) and "Mode Selection" trigger.
        *   **Validation Toggle:** A checkbox labeled "Enable Validation" (checked by default).
        *   **Help:** A context-aware "Help" button.
    *   **Behavior:** This setting persists for the session and applies to ALL modes. If unchecked, all field validations (PO format, required fields) are bypassed.
    *   **Safety:** Clicking "Mode Selection" while a workflow is in progress must prompt the user with a confirmation dialog warning that current inputs will be cleared.
*   **Mode Selection Screen:**
    *   **Mode Options:**
        1.  **Guided Wizard:** Step-by-step "Package First" workflow (Default).
        2.  **Manual Entry:** Grid-based data entry for power users (similar to Receiving/Dunnage).
        3.  **Edit Mode:** View history and edit/reprint past entries.
    *   **Default Mode:** Each mode option shall include a "Set as default mode" checkbox, allowing the user to bypass this screen in future sessions (consistent with Receiving/Dunnage modules).

#### 5.2. Mode A: Guided Wizard ("Package First")
The label creation process shall follow a linear wizard format:
1.  **Step 1: PO & Line Selection**
    *   **Workflow:** User enters a PO Number.
    *   **Display:** System retrieves and displays a **DataGrid** of available lines for that PO (mimicking `View_Receiving_POEntry`).
    *   **Selection:** User selects a line to populate the "Package Description" and other details automatically.
    *   **Manual Override:** If PO is not found (and Validation is disabled), allow manual entry.
    *   Feature: "Other" PO Reason selection (Inline).
2.  **Step 2: Recipient Selection**
    *   Input: Searchable Recipient List.
    *   Feature: "Quick Add" buttons for top 5 most frequent recipients.
    *   Feature: Smart Sorting (Recipients sorted by usage count).
3.  **Step 3: Review & Action**
    *   **View Options:** Toggle between **Single Item View** (Form) and **Table View** (Grid) to review the data before saving.
    *   Display: Summary of label data.
    *   Action: "Add to Queue & Create Another".
    *   Action: "Add to Queue & Finish".
    *   **Saving Overlay:** When saving, a full-screen overlay with a progress ring and status messages (e.g., "Saving to Database...", "Success!") shall be displayed, matching the Receiving module's "Saving Progress" view.

#### 5.3. Mode B: Manual Entry
*   **Interface:** DataGrid view allowing rapid row-based entry.
*   **Features:** Tab-to-next-cell navigation, bulk copy/paste support.
*   **Validation:** Respects the global Validation Toggle.

#### 5.4. Mode C: Edit Mode (History)
*   **Interface:** Searchable list of past labels.
*   **Actions:** Edit existing label, Clone label to new entry.
*   **Validation:** Edits must respect the global Validation Toggle.

#### 5.5. Data Entry Enhancements
*   **Quick Add:** The system shall track usage counts for recipients and display the top 5 as one-click buttons.
*   **Smart Sorting:** The full recipient list shall be sorted by usage frequency (descending).
*   **PO Validation:**
    *   If PO is missing, user must select a reason (Sample, Warranty, RMA, Other).
    *   "Other" selection prompts for a custom text reason, which is saved to the database for future use.

#### 5.3. Database & Architecture
*   **Schema:** New normalized tables:
    *   `routing_labels` (The labels themselves).
        *   Columns: `id` (Label #), `po_number`, `item_description` (Package Description), `qty`, `recipient_id` (Deliver to), `recipient_department` (Department), `employee_id` (Employee), `created_date` (Date).
    *   `routing_recipients` (Recipients with `package_count`).
    *   `routing_po_reasons` (Valid reasons for missing POs).
*   **Architecture:** Strict MVVM pattern using `CommunityToolkit.Mvvm`.
*   **Services:** Dedicated services for Workflow State, Recipient Lookup, and History.

### 6. Non-Functional Requirements
*   **Performance:** Recipient list load time < 200ms.
*   **UI Consistency:** Must use standard `Module_Shared` components and styles.
*   **Database:** All operations must use Stored Procedures (MySQL).
*   **Label Output:** The system must generate data matching the legacy Google Sheet format:
    *   "Deliver to" -> `Recipient Name`
    *   "Department" -> `Recipient Department`
    *   "Package Description" -> `item_description`
    *   "PO Number" -> `po_number`
    *   "Employee" -> `User Name`
    *   "Date" -> `Created Date`
    *   "Label #" -> `ID`

### 7. Migration Plan
*   **Phase 1: Clean Slate Protocol**
    *   **Code Removal:** Delete the entire `Module_Routing` folder.
    *   **Reference Cleanup:** Remove all references to `Module_Routing` components from `App.xaml.cs` (DI registration), `MainWindow.xaml` (Navigation), and any other shared files.
    *   **Verification:** Build and launch the application to ensure it runs stable without the Routing module. This "Green State" must be achieved before any new code is written.
*   **Phase 2: Database Foundation**
    *   **Cleanup:** Remove legacy Routing SQL files.
    *   **Schema:** Deploy new `04_schema_routing.sql`.
*   **Phase 3: Implementation**
    *   **Code:** Implement new `Module_Routing` from scratch following the new architecture.

### 8. Success Metrics
*   Reduction in average time-to-label.
*   Zero "Unknown" PO reasons (all captured via "Other" logic).
*   User adoption of "Quick Add" buttons (>50% of labels created via Quick Add).

### 9. Open Questions / Risks
*   **Risk:** "Package First" workflow might confuse users used to the old way. *Mitigation: Clear UI cues and training.*
*   **Risk:** Database migration might lose legacy data. *Mitigation: Full backup and mapping plan.*

---
**Approved By:** John Koll (PM)
**Date:** 2026-01-04
