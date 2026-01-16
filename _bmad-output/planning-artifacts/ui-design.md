---
stepsCompleted: [1]
inputDocuments: ['_bmad-output/planning-artifacts/architecture.md', '_bmad-output/planning-artifacts/prd.md']
workflowType: 'ui-design'
project_name: 'MTM_Receiving_Application'
user_name: 'John Koll'
date: '2026-01-04'
---

# UI Design & Wireframes

This document defines the visual layout and flow for the **Routing Module**, based on the decisions in `architecture.md`.

## 1. Application Shell (MainWindow)

The Routing Module lives within the main application shell, accessible via the navigation pane. A global bottom bar provides access to session-wide settings like Validation.

**Implementation Note:** The `View_Routing_Workflow.xaml` acts as the shell for the module. It contains all child views (Mode Selection, Wizard Steps, Manual Entry) in a single Grid, toggling their visibility based on the current workflow state. This matches the pattern used in `Module_Receiving` and `Module_Dunnage`.

```text
+-----------------------------------------------------------------------+
| MTM Receiving Application                                     [-][O][X]|
+-----------------------+-----------------------------------------------+
| [Navigation]          | [Content Area]                                |
|                       |                                               |
| [ ] Dashboard         |  +-----------------------------------------+  |
| [ ] Receiving Labels  |  | Routing Module - Dashboard              |  |
| [ ] Dunnage Labels    |  |                                         |  |
| [X] Internal Routing  |  | [ Wizard Mode ] [ Manual Entry ] [ Hist]|  |
|                       |  |                                         |  |
|                       |  |  +-----------------------------------+  |  |
|                       |  |  | Wizard Mode                       |  |  |
|                       |  |  | Step-by-step guidance for         |  |  |
|                       |  |  | creating individual labels.       |  |  |
|                       |  |  | [ Start Wizard ]                  |  |  |
|                       |  |  | [ ] Set as default mode           |  |  |
|                       |  |  +-----------------------------------+  |  |
|                       |  |                                         |  |
|                       |  |  +-----------------------------------+  |  |
|                       |  |  | Manual Entry                      |  |  |
|                       |  |  | Rapid-fire entry for              |  |  |
|                       |  |  | experienced users.                |  |  |
|                       |  |  | [ Open Grid ]                     |  |  |
|                       |  |  | [ ] Set as default mode           |  |  |
|                       |  |  +-----------------------------------+  |  |
|                       |  |                                         |  |
|                       |  +-----------------------------------------+  |
|                       |                                               |
| [ Settings ]          | [ Mode Selection ] [ Reset CSV ]              |
|                       | [ Help ] [X] Enable Validation (Global)       |
|                       |                                               |
|                       | [ < Back ]                       [ Next > ]   |
+-----------------------+-----------------------------------------------+
```

## 2. Wizard Workflow

### Step 1: PO & Line Selection

Focus on data entry. `IService_Focus` ensures cursor starts in "PO Number".

```text
+-----------------------------------------------------------------------+
| Routing Wizard - Step 1 of 3                                          |
+-----------------------------------------------------------------------+
|                                                                       |
|  PO Number:            [ 123456       ] [ Validate ]                  |
|                                                                       |
|  +---------------------------------------------------------------+    |
|  | Line | Part Number | Description          | Qty Ordered       |    |
|  +------+-------------+----------------------+-------------------+    |
|  | 1    | 99-888-77   | Widget A             | 100               |    |
|  | 2    | 99-888-78   | Widget B             | 50                |    |
|  +------+-------------+----------------------+-------------------+    |
|                                                                       |
|  Package Description:  [ Widget A     ] (Auto-filled)                 |
|                                                                       |
|  Quantity:             [ 100          ]                               |
|                                                                       |
+-----------------------------------------------------------------------+
```

### Step 2: Recipient Selection

Features "Quick Add" buttons for top recipients and a search filter.

```text
+-----------------------------------------------------------------------+
| Routing Wizard - Step 2 of 3                                          |
+-----------------------------------------------------------------------+
|                                                                       |
|  Quick Add (Top 5):                                                   |
|  [ Assembly ] [ Paint ] [ Warehouse ] [ QC ] [ Shipping ]             |
|                                                                       |
|  Search: [            ]                                               |
|                                                                       |
|  +---------------------------------------------------------------+    |
|  | Name (Deliver To)    | Department                             |    |
|  +----------------------+----------------------------------------+    |
|  | Assembly             | Production                             |    |
|  | Paint Line A         | Production                             |    |
|  | Quality Control      | QA                                     |    |
|  +----------------------+----------------------------------------+    |
|                                                                       |
+-----------------------------------------------------------------------+
```

### Step 3: Review & Print

Final verification before committing to the database.

```text
+-----------------------------------------------------------------------+
| Routing Wizard - Step 3 of 3                                          |
+-----------------------------------------------------------------------+
|                                                                       |
|  [ Single View ] [ Table View ]                                       |
|                                                                       |
|  Summary:                                                             |
|  PO: 123456                                                           |
|  Desc: 99-888-77                                                      |
|  Qty: 100                                                             |
|  Deliver To: Assembly                                                 |
|  Dept: Production                                                     |
|  Employee: John Doe                                                   |
|  Date: 2026-01-04                                                     |
|                                                                       |
|                                                  [ Finish & New ]     |
|                                                                       |
+-----------------------------------------------------------------------+
```

## 3. Manual Entry (Grid View)

Designed for power users. Uses `CommunityToolkit.WinUI.UI.Controls.DataGrid` with a standard Toolbar.

```text
+-----------------------------------------------------------------------+
| Routing - Manual Entry                                                |
+-----------------------------------------------------------------------+
|                                                                       |
|  [ Add Row ] [ Add Multiple ] [ Remove Row ] [ Auto-Fill ]            |
|                                                                       |
|  +-----------------------------------------------------------------+  |
|  | PO Number | Description | Qty | Deliver To | Dept | Actions     |  |
|  +-----------+-------------+-----+------------+------+-------------+  |
|  | 12345     | A-100       | 50  | Paint      | Prod | [Del]       |  |
|  | 12345     | B-200       | 10  | QC         | QA   | [Del]       |  |
|  |           |             |     |            |      | [Save]      |  |
|  +-----------+-------------+-----+------------+------+-------------+  |
|                                                                       |
|                                                    [ Save & Finish ]  |
+-----------------------------------------------------------------------+
```

## 4. Dialogs

### "Other" Reason Prompt

Triggered when a user enters a custom reason in the "PO Issue" field (if applicable) or similar dropdowns.

```text
+-------------------------------------------+
| New Reason Detected                       |
+-------------------------------------------+
|                                           |
|  You entered a new reason:                |
|  "Damaged in Transit"                     |
|                                           |
|  Do you want to save this to the list     |
|  for future use?                          |
|                                           |
|  [ Yes, Save It ]    [ No, Just Once ]    |
|                                           |
+-------------------------------------------+
```

## 5. Implementation Notes

### Global Bottom Bar

* **Location:** `MainWindow.xaml` (inside the `NavigationView` grid).
* **Components:**
  * **Validation Toggle:** Checkbox bound to `ViewModel.IsValidationEnabled`.
  * **Visibility:** Should be visible when the Routing Module is active (or globally if appropriate).
* **Logic:**
  * The `IsValidationEnabled` property must be accessible to all Routing ViewModels (likely via a shared Service or the MainViewModel).
  * When unchecked, `CanExecute` checks on "Next/Print" commands should be relaxed.

### Navigation Changes

* **Carrier Delivery:** Removed from `NavigationView.MenuItems` as per requirements.

```
