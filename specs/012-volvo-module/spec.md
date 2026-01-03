# Feature Specification: Volvo Dunnage Requisition Module

**Feature Branch**: `012-volvo-module`  
**Created**: 2026-01-03  
**Status**: Active  
**Input**: User description: "Volvo dunnage receiving workflow with PO requisition generation, component explosion, discrepancy tracking, and label generation for LabelView 2022"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Receive Volvo Dunnage Shipment (Priority: P1) 🎯 MVP

User physically receives Volvo dunnage shipment, enters part numbers and skid counts, tracks discrepancies, generates labels, and creates PO requisition email for purchasing department.

**Why this priority**: Core workflow - without this, users cannot process any Volvo shipments. All other features depend on this.

**Independent Test**: User can enter a Volvo shipment (date, parts, skid counts), optionally flag discrepancies, generate labels via CSV export, copy formatted email, and save as "Pending PO" status. Label CSV file exists and can be imported into LabelView 2022.

**Acceptance Scenarios**:

1. **Given** user opens Volvo module and no pending shipment exists, **When** user enters shipment data (V-EMB-21: 10 skids, V-EMB-750: 7 skids), **Then** system calculates component explosion from master data and shows requested piece counts
2. **Given** user has entered parts with discrepancies checked, **When** user enters packlist quantity (12) vs received quantity (10), **Then** system calculates difference (-2) and includes discrepancy notice in email
3. **Given** user clicks "Generate Labels", **When** system processes component explosion, **Then** CSV file is created with all aggregated parts ready for LabelView import
4. **Given** user clicks "Preview/Edit Email", **When** modal opens, **Then** user can edit greeting and custom notes while tables remain read-only
5. **Given** user clicks "Copy to Clipboard", **When** email text is copied, **Then** formatted email with discrepancy notice (if any) and requested lines table is ready to paste into Outlook
6. **Given** user clicks "Save as Pending PO", **When** shipment is saved, **Then** status = 'pending_po' and CSV file remains available for label printing

---

### User Story 2 - Complete PO After Purchasing Response (Priority: P2) 🎯 MVP

After purchasing department creates PO and user receives into Infor Visual, user enters PO number and receiver number to complete the shipment record.

**Why this priority**: Required to close the loop on shipments. Without this, records stay in pending state forever and reporting is incomplete.

**Independent Test**: User can open a pending shipment, click "Complete with PO", enter PO number (e.g., PO-062450) and receiver number (e.g., 134393), save, and shipment moves to completed status with CSV file cleared.

**Acceptance Scenarios**:

1. **Given** pending PO shipment exists from User Story 1, **When** user opens Volvo module, **Then** modal displays pending shipment with option to complete or view/edit
2. **Given** user clicks "Complete with PO", **When** user enters PO and Receiver numbers, **Then** system warns about CSV file clearing before saving
3. **Given** user confirms completion, **When** system saves, **Then** status changes to 'completed', CSV file content is cleared (file exists but empty), and shipment is archived to history
4. **Given** shipment is completed, **When** user views in history, **Then** PO and Receiver numbers are displayed

---

### User Story 3 - Manage Volvo Parts Master Data (Priority: P2) 🎯 MVP

Administrators manage the Volvo parts master data (part numbers, quantities per skid, included components) to ensure accurate component explosion calculations.

**Why this priority**: Required for accurate PO requisitions. Without correct master data, purchasing will order wrong quantities.

**Independent Test**: Admin can add new Volvo part (V-EMB-NEW: 50 pcs/skid, includes V-EMB-2 and V-EMB-92), edit existing part to change quantity or components, deactivate obsolete parts, and import/export CSV files. Historical shipment data is not affected by changes.

**Acceptance Scenarios**:

1. **Given** admin opens Settings → Volvo → Master Data, **When** DataGrid loads, **Then** all active parts are displayed with quantities and component indicators
2. **Given** admin clicks "Add New Part", **When** form is filled (Part: V-EMB-NEW, Qty: 50, Components: Skid=V-EMB-1, Lid=V-EMB-71), **Then** part is saved and available in shipment entry dropdowns
3. **Given** admin edits existing part V-EMB-500 from 88 to 90 pcs/skid, **When** saved, **Then** warning displays that existing shipments will NOT be recalculated (historical integrity)
4. **Given** admin clicks "Import from CSV", **When** valid DataSheet.csv is selected, **Then** preview shows new/updated/unchanged parts before import
5. **Given** admin deactivates V-EMB-OLD, **When** confirmed, **Then** part is grayed out in grid and removed from shipment entry dropdowns

---

### User Story 4 - View and Edit Shipment History (Priority: P3)

Users view historical Volvo shipments with filtering, can edit existing records (regenerates labels), and export data for reporting.

**Why this priority**: Needed for corrections and auditing, but not blocking core workflow.

**Independent Test**: User can filter history by date range and status (Pending PO / Completed), click on shipment to view details (parts, discrepancies, requested lines), edit a shipment to correct quantities (CSV regenerates), and export to CSV.

**Acceptance Scenarios**:

1. **Given** user clicks "View History", **When** history screen loads with date filter (last 30 days), **Then** DataGrid shows shipments with date, part count, PO/Receiver (if completed), and status
2. **Given** user clicks "View" on a shipment, **When** detail flyout opens, **Then** all parts entered, discrepancies, and calculated requested lines are displayed
3. **Given** user clicks "Edit" on pending shipment, **When** user changes V-EMB-21 from 10 to 12 skids, **Then** system recalculates components, warns about CSV regeneration, and updates record
4. **Given** user clicks "Export", **When** SaveFileDialog appears, **Then** CSV file is generated with all history data

---

### User Story 5 - View Volvo Shipments in End-of-Day Reports (Priority: P3)

Users generate End-of-Day reports that include Volvo module data alongside Receiving, Dunnage, and Routing modules.

**Why this priority**: Needed for daily operational reporting, but not blocking shipment processing.

**Independent Test**: User selects date range, checks Volvo checkbox (if data exists for range), generates report showing Pending PO and Completed sections with part counts, PO numbers, and receivers.

**Acceptance Scenarios**:

1. **Given** user opens Reports module and selects date range (01/02-01/03), **When** "Check Availability" is clicked, **Then** checkboxes show record counts for each module including Volvo
2. **Given** Volvo has no data for selected range, **When** availability is checked, **Then** Volvo checkbox is disabled (grayed out)
3. **Given** user checks Volvo and Receiving checkboxes, **When** "Generate Report" is clicked, **Then** tabbed interface shows both modules with Volvo displaying Pending PO and Completed DataGrids
4. **Given** user clicks "Complete" on pending shipment in report, **When** PO/Receiver dialog is filled and saved, **Then** shipment moves to Completed section and report refreshes

---

### User Story 6 - Packlist Summary View (Priority: P4) 📦 FUTURE

**OPTIONAL FEATURE - Future Implementation**

Users view aggregated packlist summaries similar to Google Sheets "PO Numbers" tracking table, grouped by date and PO number with drill-down details.

**Why this priority**: Nice-to-have for historical analysis, NOT required for MVP.

**Independent Test**: User can view summary DataGrid grouped by date/PO, click PO to see part breakdown, and export detail to CSV.

**Acceptance Scenarios**:

1. **Given** user clicks "Packlist Summary" from Volvo menu, **When** summary screen loads, **Then** aggregated view shows Date, PO Number, Unique Dunnage Types, Total Skids
2. **Given** user clicks PO number link, **When** flyout opens, **Then** full part breakdown with skid counts and piece counts is displayed
3. **Given** user clicks "Export Detail", **When** SaveFileDialog appears, **Then** CSV with full part details is generated

---

### Edge Cases

- What happens when user tries to create second pending shipment before completing first? → Modal blocks action and prompts to complete or view/edit existing pending shipment
- How does system handle master data changes after shipment is created? → Historical data is stored at time of creation, changes to master do not affect old records (historical integrity)
- What if user closes Volvo module without saving? → Data is lost unless "Save as Pending PO" was clicked
- What if CSV file is deleted by user between label generation and archival? → File exists but may be empty, no crash (LabelView compatibility requires file to exist)
- What if purchasing never responds with PO? → Shipment stays in "Pending PO" status indefinitely, visible in history and reports
- What if user imports CSV with duplicate part numbers? → Validation error shows which rows are duplicates, import is blocked until corrected
- What if discrepancy is found but user forgets to check discrepancy checkbox? → Email will NOT include discrepancy notice, only the received quantities are used
- What if component part (e.g., V-EMB-2) is used by multiple main parts (V-EMB-500 and V-EMB-600)? → System aggregates components across all parts in single shipment

## Requirements *(mandatory)*

### Functional Requirements

**Shipment Entry & Processing**
- **FR-001**: System MUST auto-generate date (configurable: auto-fill today or user-editable) and shipment number (auto-increments, resets daily)
- **FR-002**: System MUST allow only ONE active "Pending PO" shipment at a time
- **FR-003**: System MUST provide dropdown populated from volvo_parts_master table for part number selection
- **FR-004**: System MUST allow user to enter received skid count for each part
- **FR-005**: System MUST provide optional discrepancy tracking with 4 fields: Packlist (user input), Received (user input), Difference (calculated), Note (user input)
- **FR-006**: System MUST calculate component explosion from volvo_parts_master and aggregate duplicate components across all parts in shipment
- **FR-007**: System MUST generate CSV file for LabelView 2022 with calculated piece counts (not skid counts)
- **FR-008**: System MUST hide PO field on labels for parts starting with "V-EMB-" (LabelView logic)
- **FR-009**: System MUST allow user to edit greeting line and discrepancy message in email preview modal
- **FR-010**: System MUST keep email tables (Discrepancies, Requested Lines) read-only in preview
- **FR-011**: System MUST provide "Copy to Clipboard" button for formatted email text
- **FR-012**: System MUST save shipment with status='pending_po' and preserve CSV file until archived

**PO Completion**
- **FR-013**: System MUST display pending shipment modal on module entry if one exists
- **FR-014**: System MUST provide "Complete with PO" dialog requiring PO Number and Receiver Number fields
- **FR-015**: System MUST warn user about CSV file clearing before completing shipment
- **FR-016**: System MUST clear CSV file content (not delete file) when shipment is completed
- **FR-017**: System MUST change status to 'completed' and archive to history when PO/Receiver are entered

**Master Data Management**
- **FR-018**: System MUST allow admin to add new Volvo parts with Part Number, Quantity per Skid, and optional components
- **FR-019**: System MUST validate unique part numbers (prevent duplicates)
- **FR-020**: System MUST allow admin to edit existing parts (quantity and components only, part number read-only)
- **FR-021**: System MUST warn admin that changes will NOT affect existing shipment records (historical integrity)
- **FR-022**: System MUST store calculated piece counts at time of shipment creation (not re-calculate from master)
- **FR-023**: System MUST allow admin to deactivate/reactivate parts (deactivated parts hidden from dropdowns)
- **FR-024**: System MUST provide CSV import with preview showing new/updated/unchanged parts
- **FR-025**: System MUST provide CSV export of master data
- **FR-026**: System MUST validate CSV import (required fields, data types, duplicates)

**History & Reporting**
- **FR-027**: System MUST allow users to filter history by date range and status (Pending PO / Completed / All)
- **FR-028**: System MUST provide view detail flyout showing parts entered, discrepancies, and calculated requested lines
- **FR-029**: System MUST allow users to edit historical shipments (regenerates CSV with warning)
- **FR-030**: System MUST export history to CSV with Date, Shipment#, PartNumber, Quantity, PONumber, Receiver, Status columns
- **FR-031**: System MUST integrate with shared End-of-Day reporting module via checkbox selection
- **FR-032**: System MUST disable Volvo checkbox in reports if no data exists for selected date range
- **FR-033**: System MUST display Pending PO and Completed sections in reports with option to complete pending shipments directly from report

### Key Entities

- **Volvo Shipment**: Represents a single dunnage shipment from Volvo with date, shipment number, PO number (optional), receiver number (optional), employee, notes, status (pending_po/completed), timestamps
- **Volvo Shipment Line**: Individual part entry within a shipment with part number, received skid count, optional discrepancy data (packlist qty, difference, note), calculated piece count (stored at creation)
- **Volvo Part Master**: Catalog of available Volvo parts with part number (PK), description, quantity per skid, is_active flag, timestamps
- **Volvo Part Component**: Components included with each main part (e.g., V-EMB-500 includes 1× V-EMB-2, 1× V-EMB-92) with component part number, quantity per skid
- **Volvo Label Export**: Generated labels for LabelView with material ID, quantity (pieces), employee, date, time, receiver (blank initially), notes

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can complete full Volvo shipment workflow (entry → label generation → email copy → save as pending) in under 5 minutes for typical 10-part shipment
- **SC-002**: Component explosion calculations are 100% accurate based on master data (verified by comparing manual calculations to system output)
- **SC-003**: CSV file exports are compatible with LabelView 2022 (labels print correctly with no errors)
- **SC-004**: Email format matches current Google Sheets output format (purchasing department can process requests without confusion)
- **SC-005**: Historical data integrity is maintained (editing master data does NOT change piece counts in old shipments)
- **SC-006**: Discrepancy tracking reduces manual follow-up emails by 80% (discrepancies captured and communicated in initial email)
- **SC-007**: Only one pending PO can exist at a time (prevents data loss and confusion)
- **SC-008**: Pending shipments are never lost (accessible via modal on re-entry to module)

## Assumptions *(optional)*

- Volvo shipments arrive irregularly (not daily), average 2-3 shipments per week
- Purchasing department responds with PO number within 1-3 business days
- User has LabelView 2022 installed and configured to read from CSV export folder
- DataSheet.csv from Google Sheets is authoritative source for master data initial import
- Multiple shipments can arrive same day (hence sequence number auto-increment)
- Email signature is configured in user settings (not hardcoded in Volvo module)
- Infor Visual receiving happens AFTER PO is created by purchasing (not before)
- Labels are printed immediately after CSV generation (before archiving to history)

## Dependencies *(optional)

- **Phase 1 Infrastructure**: Complete (MVVM architecture, DI, error handling, logging)
- **Database Tables**: volvo_shipments, volvo_shipment_lines, volvo_parts_master, volvo_part_components (see data-model.md)
- **Stored Procedures**: sp_volvo_shipment_insert, sp_volvo_shipment_update, sp_volvo_shipment_complete, sp_volvo_part_master_CRUD, sp_volvo_history_get (see data-model.md)
- **External Systems**: 
  - LabelView 2022 (CSV import for label printing)
  - Infor Visual (for final PO receiving after purchasing creates PO)
- **Other Features**: 
  - Shared End-of-Day Reporting Module (for integrated reporting)
  - Settings infrastructure (for Volvo settings tab integration)
  - BaseViewModel, BaseWindow (for MVVM pattern)

## Out of Scope *(optional)*

- Automatic email sending (user copies text and sends via Outlook manually)
- Integration with purchasing department's PO system (manual PO number entry)
- Automatic LabelView triggering (user manually imports CSV into LabelView)
- Real-time inventory tracking (this module tracks requisitions, not stock levels)
- Barcode scanning for part entry (manual dropdown selection only)
- Mobile app version (Windows desktop only)
- Multi-language support (English only)
- Direct integration with Volvo's shipping system (manual packlist comparison)

---

## Constitution Compliance Check *(mandatory)*

### Principle Alignment

- [x] **MVVM Architecture**: Views (XAML), ViewModels (business logic), Models (data), Services (operations) clearly separated
- [x] **Database Layer**: All operations via stored procedures, return Model_Dao_Result, no raw SQL
- [x] **Dependency Injection**: Services registered in App.xaml.cs (IVolvoService, IVolvoMasterDataService, Dao_VolvoShipment, Dao_VolvoParts)
- [x] **Error Handling**: All exceptions handled via IService_ErrorHandler, user-facing errors shown in InfoBars/ContentDialogs
- [x] **Security & Authentication**: Employee number from authentication context (no additional auth required)
- [x] **WinUI 3 Practices**: x:Bind (compile-time), ObservableCollection, async/await, CommunityToolkit.Mvvm attributes ([ObservableProperty], [RelayCommand])
- [x] **Specification-Driven**: This spec follows Speckit workflow with user stories, requirements, success criteria, and templates
