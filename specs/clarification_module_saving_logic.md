# Clarification Questions: Standardization of Module Saving Logic

Before proceeding with the standardization of the saving logic across all modules, I have reviewed the `Module_Receiving` implementation and have the following questions to ensure specific requirements are met:

## 1. "Reset CSV" / Commit Workflow
You mentioned: *"user clicks Reset CSV button - a. data from CSV file of current module is saved to database b. csv file is reset."*
- **Question:** Is this "Reset CSV" button intended to be the **"End of Day" / "End of Shift"** action?
    - Yes and No, its is ment to reset the csv file so there is no so much data in it, keeping the list of printable labels managable in labelview.
- **Question:** Should this button be located in the **Edit Mode** view (alongside "View Labels" / "View History") or on the main Dashboard for the module?
    - Look at the placement of the Module_Receiving's Reset CSV button, use the same design logic
- **Question:** If there are errors saving specific CSV rows to the database (e.g., validation errors), should the process **halt** and keep those lines in the CSV, or skip them and log errors?
    - keep the failed lines, remove the passing lines, show error message

## 2. Scope of Modules
You mentioned *"go through each module"*. Please confirm which of the following modules require this "CSV Buffer -> DB Commit" pattern:
- [x] **Module_Routing** (Routing Labels)
- [x] **Module_Dunnage** (Inventory/Return Labels?)
- [x] **Module_Volvo** (Shipments?)
- [ ] **Module_Reporting** (Likely N/A, but please confirm) - No

## 3. Edit Mode Columns & Data Structure
`Module_Receiving` has a specific set of columns for its labels.
- **Question:** For **Routing** and **Dunnage**, when clicking "View Labels" (CSV) vs "View History" (DB), should the DataGrid columns automatically adapt to show the specific fields for that module (e.g., *Recipient* for Routing, *Dunnage Type* for Dunnage)?
    - Yes
- **Question:** Do you want the **Edit/Update** functionality (clicking a row to edit) to apply *only* to the "View Labels" (CSV) list, or can users also edit "History" (DB) records? (Usually History is Read-Only).
    - Users can Edit both

## 4. Automation vs Manual "Reset"
- **Current Behavior (Routing):** Routing labels currently save to DB immediately.
- **New Behavior:** You want them to go to CSV first.
    - Yes
- **Question:** Is there any scenario (e.g., "Urgent" label) where it should bypass CSV and go straight to DB, or is the "CSV Buffer" rule absolute for all workflows.
    - No CSV Buffer is absolute

## 5. Implementation Detail - CSV Persistence
- **Confirm:** The CSV file acts as a persistent queue. If the app crashes or closes, the labels remain in the CSV and appear in "View Labels" upon restart. Correct?
    - Yes

## 6. Edit Mode UI Standard
I will use `View_Receiving_EditMode` as the gold standard.
- **Check constraint:** Does `Module_Receiving` currently have the "Reset CSV" button functionality implemented as you described, or do I need to implement it there first/too? (I notice `Module_Receiving` has CSV writing, but I want to be sure the "Flush to DB" logic is exactly how you want it there before copying it).
    - Its implemented correctly yes.  Though there is a "Save to Database" button at the end of the wizard workflow that needs to be changed to "Save Labels"

---
**Next Steps:**
Once these clarifications are provided, I will:
1.  Implement the `Service_CsvBuffer` pattern (or similar) for Routing/Dunnage/Volvo.
2.  Refactor `SaveAsync` methods to write to CSV.
3.  Update Validations to pass for CSV writing (if DB constraints don't apply yet).
4.  Update Edit Mode UIs to identical 2-button command bars.

Other Things to Address:

Receiving Manual Entry needs a PO Number Column (If left blank use set to "Nothing Entered")
