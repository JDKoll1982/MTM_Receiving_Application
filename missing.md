You are an AI agent responsible for maintaining project documentation. Please read the current versions of `_bmad-output/planning-artifacts/prd.md`, `_bmad-output/planning-artifacts/architecture.md`, and `_bmad-output/planning-artifacts/ui-design.md`.

Update these three files to incorporate the following critical feedback, ensuring the **Routing Module** strictly adheres to the UX/UI patterns established in the existing `Receiving` and `Dunnage` modules:

### 1. PRD Updates (`prd.md`)
*   **Workflow Refinement (Step 1):** Change the "Package Details" step to a **PO & Line Selection** workflow. Instead of manual entry, the user enters a PO, and the system must present a **DataGrid** of available lines (mimicking `View_Receiving_POEntry`) to ensure accurate data capture.
*   **Global Controls:** Specify that **Next**, **Back**, **Validation Checkbox**, **Mode Selection**, and **Reset CSV** buttons must reside on the **Main Form's Lower Row** (Shell), not within individual wizard pages.
*   **Review Step:** Add a requirement for the Review step to support toggling between **Single Item View** and **Table View**.
*   **Safety:** Add a requirement that clicking "Mode Selection" must prompt the user that current inputs will be cleared.

### 2. Architecture Updates (`architecture.md`)
*   **Shell View Responsibility:** Update `View_Routing_Workflow` (Shell) to host the global navigation buttons (Next, Back, Reset) and the Notification Box.
*   **Visibility Logic:** Define logic in `ViewModel_Routing_Workflow` to control the visibility/enabled state of bottom bar buttons based on the current step (matching `Dunnage` workflow logic).
*   **Data Retrieval:** Update `Service_Routing_Data` to support fetching PO Lines for the new Step 1 DataGrid.
*   **Help Service:** Explicitly integrate `Service_Help` to populate the Notification Box and handle the Help Button logic.

### 3. UI Design Updates (`ui-design.md`)
*   **Shell Layout:** Redesign the `Application Shell` wireframe. Move the "Wizard" navigation buttons and "Mode Selection" trigger to a persistent **Bottom Bar**.
*   **Step 1 Wireframe:** Replace the simple form with a **PO Entry + DataGrid** layout for line selection.
*   **Review Wireframe:** Add controls to toggle between Single View and Table View.
*   **Components:** Add the **Help Button** (multi-view implementation) and **Notification Box** to the standard layout.

Please regenerate the artifacts to reflect these requirements.