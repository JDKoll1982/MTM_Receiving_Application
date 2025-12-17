# Feature Specification: Multi-Step Receiving Label Entry Workflow

**Feature Branch**: `001-receiving-workflow`  
**Created**: December 16, 2025  
**Status**: Draft  
**Input**: User description: "Implement multi-step receiving label entry workflow with PO number entry, part selection, skid information, quantities, heat numbers, packages, and saving to CSV/database. UI elements should be in MainWindow.xaml NavigationView except for actual dialog boxes."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Complete Basic Receiving Entry (Priority: P1)

A warehouse receiving clerk enters a single PO with one part, specifying quantities, heat numbers, and package counts, then saves the receiving data to CSV and database.

**Why this priority**: This represents the core functionality - the ability to record receiving data for one complete PO. Without this, the application delivers no value.

**Independent Test**: Can be fully tested by entering a PO number, selecting a part, entering skid counts, quantities, heat numbers, and packages, then verifying the data saves to both CSV files and database. Delivers immediate value by replacing manual receiving data entry.

**Acceptance Scenarios**:

1. **Given** the application is open to the receiving entry page, **When** the user enters a valid 6-digit PO number and clicks "Load PO", **Then** the system queries Infor Visual database and displays all parts associated with that PO
2. **Given** the application is open to the receiving entry page, **When** the user clicks "Non-PO Item" button, **Then** the system displays a manual part entry form for customer-supplied items without PO lookup
3. **Given** the user is entering a non-PO item, **When** the user enters part ID manually and clicks "Continue", **Then** the system advances to skid information entry without validating against Infor Visual
4. **Given** parts are displayed from the PO, **When** the user selects a part and clicks "Select Part", **Then** the system advances to skid information entry showing the selected part details
3. **Given** skid information entry is displayed, **When** the user enters the number of lines (skids) between 1-99 and clicks "Continue", **Then** the system creates that many line entries and advances to quantity entry
4. **Given** line entries are created, **When** the user enters a quantity for each line and all quantities are greater than 0, **Then** the "Continue to Heat Numbers" button becomes enabled
5. **Given** quantities are entered, **When** the sum of quantities exceeds the PO ordered quantity, **Then** the system displays a warning allowing the user to proceed or revise quantities
6. **Given** quantities are entered, **When** the part was already received today in Infor Visual, **Then** the system displays a comparison warning showing Visual's received quantity vs. user's entered total
7. **Given** quantities are entered for all lines, **When** the user enters heat/lot numbers for each line and clicks "Continue", **Then** the system advances to package entry
8. **Given** package entry is displayed, **When** the user sees each line, **Then** the system automatically sets package type to "Coils" for part IDs starting with MMC, "Sheets" for part IDs starting with MMF, and shows a dropdown for other part IDs
9. **Given** a line has a default package type, **When** the user clicks the package type dropdown, **Then** the system displays options: "Coils", "Sheets", "Boxes", "Custom", allowing the user to change the default
10. **Given** the user selects "Custom" from the package type dropdown, **When** the user enters a custom package type name, **Then** the system accepts the custom name and uses it for that line
11. **Given** the user changes a package type for a part ID, **When** the data is saved, **Then** the system persists the package type preference for that part ID to the MySQL database for future use
12. **Given** package type is set and packages per skid is entered for each line, **When** the user enters the package count, **Then** the system calculates and displays weight per package (Quantity ÷ Packages) with the appropriate package type label
13. **Given** all line data is complete, **When** the user clicks "Save to CSV & Database", **Then** the system saves data to local CSV, network CSV, and MySQL database, showing progress indicators for each step
14. **Given** save is complete, **When** the system displays save confirmation, **Then** the user sees file locations and success counts, with options to start new entry, view history, or print labels

---

### User Story 2 - Enter Non-PO Items (Customer Supplied) (Priority: P1)

A warehouse clerk receives customer-supplied materials that do not have PO numbers and needs to enter receiving data by looking up part information directly from Infor Visual without a PO.

**Why this priority**: Customer-supplied items are a regular occurrence and require the same receiving workflow as PO items. Without this capability, the application cannot handle a common business scenario.

**Independent Test**: Can be fully tested by clicking "Non-PO Item", entering a valid part ID, verifying system retrieves part details from Infor Visual, completing the workflow, and verifying data saves correctly with appropriate non-PO indicators.

**Acceptance Scenarios**:

1. **Given** the receiving entry page is displayed, **When** the user clicks "Non-PO Item" button, **Then** the system displays a manual entry form with a Part ID field
2. **Given** the manual entry form is displayed, **When** the user enters a valid Part ID and clicks "Look Up Part", **Then** the system queries Infor Visual database directly for part information (Part Type, Description) and displays it
3. **Given** the part lookup succeeds, **When** the user clicks "Continue", **Then** the system proceeds to skid information with the retrieved part details
4. **Given** a non-PO item is being entered, **When** the user reaches quantity validation, **Then** the system skips PO ordered quantity validation and same-day receiving checks (but part ID was already validated)
5. **Given** a non-PO item workflow is complete, **When** data is saved, **Then** the system marks the record with a null or "N/A" PO number indicator

---

### User Story 3 - Enter Multiple Parts from Same PO (Priority: P2)

A warehouse clerk receives a shipment with multiple different parts on the same PO and needs to enter receiving data for each part before saving.

**Why this priority**: Many real-world shipments contain multiple parts. This increases efficiency by allowing batch entry before saving, reducing the number of save operations and improving data consistency.

**Independent Test**: Can be tested by entering data for one part, clicking "Add Another Part/PO" at the review step, entering a second part from the same PO, then verifying both parts save together in one operation.

**Acceptance Scenarios**:

1. **Given** the user has completed entering data for one part (through Step 6), **When** the user is shown the review screen and clicks "Add Another Part/PO", **Then** the system stores the current part's lines and returns to Step 1 for PO entry
2. **Given** the user has data for multiple parts stored in the session, **When** the user completes the final part and clicks "Save to CSV & Database", **Then** all parts' data saves together in one operation
3. **Given** multiple parts are in the current session, **When** the save completes, **Then** the system shows the total count of all lines saved across all parts

---

### User Story 4 - Smart Heat Number Selection (Priority: P2)

A warehouse clerk receives multiple skids of the same part with the same heat/lot number and wants to quickly apply that heat number to multiple lines without retyping.

**Why this priority**: Significantly improves data entry speed and reduces errors when multiple skids share the same heat number, which is a common scenario in receiving operations.

**Independent Test**: Can be tested by entering a heat number for Line 1, verifying it appears in the quick-select list, then checking that heat number's checkbox and verifying it applies to other lines. Delivers value by reducing repetitive typing.

**Acceptance Scenarios**:

1. **Given** the user is on the heat number entry step, **When** the user enters a heat number for Line 1, **Then** that heat number appears in the "Quick Select Heat Numbers" list with an unchecked checkbox
2. **Given** a heat number exists in the quick-select list, **When** the user checks the checkbox for that heat number, **Then** that heat number is applied to all lines that don't already have a heat number entered
3. **Given** the user enters a heat number for Line 2 that matches Line 1's heat number, **When** the entry is complete, **Then** the existing heat number in the quick-select list shows as checked
4. **Given** multiple different heat numbers are entered across lines, **When** the quick-select list is displayed, **Then** all unique heat numbers appear in the list with their corresponding line numbers

---

### User Story 5 - CSV Reset on Startup (Priority: P3)

A warehouse supervisor starting a new receiving session wants to optionally reset the CSV files to prevent label printing confusion from previous sessions.

**Why this priority**: Provides data management flexibility and prevents confusion with stale data, but the application can function without this feature.

**Independent Test**: Can be tested by launching the application with existing CSV files, choosing "Yes, Reset" on the dialog, and verifying the CSV files are deleted. Choosing "No, Continue" should load existing CSV data.

**Acceptance Scenarios**:

1. **Given** the application is starting up, **When** the application initializes, **Then** a ContentDialog appears asking "Reset CSV File?" with "Yes, Reset" and "No, Continue" buttons
2. **Given** the reset dialog is displayed, **When** the user clicks "Yes, Reset", **Then** both local (%APPDATA%) and network (\\mtmanu-fs01) CSV files are deleted
3. **Given** the reset dialog is displayed, **When** the user clicks "No, Continue", **Then** existing CSV files remain and their data is available for viewing/printing
4. **Given** the user chooses to reset, **When** the dialog closes, **Then** the application proceeds to the receiving entry page with a clean state

---

### User Story 6 - View Entry Status and Progress (Priority: P3)

A warehouse clerk wants to see their current progress through the multi-step workflow, understand what data has been entered, and see status messages for guidance.

**Why this priority**: Improves user experience and reduces errors by providing clear feedback, but core functionality works without it.

**Independent Test**: Can be tested by proceeding through each step and verifying that status messages update appropriately, step numbers are displayed, and entered data is visible in review screens.

**Acceptance Scenarios**:

1. **Given** the user is at any step in the workflow, **When** the step is displayed, **Then** the current step number and title are clearly shown (e.g., "[Step 3] Skid Information for PART-001")
2. **Given** the user performs an action (e.g., loads PO, selects part), **When** the action completes, **Then** a status message displays the result (e.g., "PO 123456 loaded. 5 parts found.")
3. **Given** the user is at Step 7 (Review), **When** the review screen displays, **Then** the system shows a summary including part name, number of lines entered, and total quantity
4. **Given** the user is at the save step (Step 8-9), **When** saving is in progress, **Then** a progress bar shows percentage complete and current operation (local CSV, network CSV, database)

---

### Edge Cases

- **What happens when the PO number doesn't exist in Infor Visual?** System displays error message "PO not found. Please verify the PO number and try again." User can retry PO entry or click "Non-PO Item" to proceed with manual entry.
- **What happens when a PO has no parts?** System displays error message "PO [number] contains no parts." User remains at Step 1.
- **What happens if user enters a non-PO part ID that doesn't exist in Infor Visual?** System displays error "Part ID [part ID] not found in Visual. Please verify and try again." User can retry or return to PO entry.
- **What happens if user enters a non-PO item with a part ID that matches an existing PO part?** System retrieves part details from Visual successfully and proceeds; saves with null PO number to indicate customer-supplied.
- **What happens when network CSV path is unavailable?** System attempts to save to local CSV and database, displays warning "Network CSV location unavailable. Data saved locally and to database. Network sync required later."
- **What happens when database save fails?** System displays error with specific message, keeps data in JSON file and CSV files as backup, and offers "Retry" option. User can close application without losing entered data.
- **What happens if user enters 0 for number of lines?** "Continue" button remains disabled and status message displays "Number of lines must be greater than 0."
- **What happens if user enters invalid characters in NumberBox controls?** WinUI 3 NumberBox automatically rejects non-numeric input.
- **What happens if the sum of all quantities for a part ID exceeds the PO ordered quantity?** System displays warning dialog: "Total quantity ([sum]) exceeds PO ordered quantity ([ordered qty]) for part [part ID]. Do you want to continue?" with Yes/No options. User can proceed if intentional over-receiving.
- **What happens if the same part on the same PO was already received today in Infor Visual?** System queries Infor Visual for same-day receipts, displays warning: "Part [part ID] was already received today. Visual shows [qty] received. Your entry totals [sum]. Please verify." User can choose to proceed or review their entry.
- **What happens if user navigates away mid-entry?** System keeps progress saved in a JSON file so if the applicaiton is closed it will retain its current progress.
- **What happens if a part ID doesn't match MMC or MMF pattern?** System displays dropdown with no default selection, requiring user to choose a package type before continuing.
- **What happens if user selects "Custom" but doesn't enter a custom name?** System keeps the "Custom" option selected but requires a custom name entry before enabling the "Continue" button.
- **What happens when a part ID has a saved package type preference that differs from the default?** System uses the saved preference instead of the default (e.g., if user previously set MMC-12345 to "Sheets", it shows "Sheets" instead of "Coils").
- **What happens if the session JSON file is corrupted or unreadable?** System displays warning "Unable to restore previous session. Starting fresh.", deletes the corrupted file, and starts with a clean session.
- **What happens if user starts application and has unsaved work from previous session?** System automatically loads the previous session state and displays an info bar: "Previous session restored. You can continue your work or start fresh."

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a multi-step guided workflow with 9 distinct steps: startup reset prompt, PO entry, part selection, skid information, quantities, heat numbers, packages, review, and save
- **FR-002**: System MUST provide a "Non-PO Item" option on the PO entry screen allowing users to enter customer-supplied items without PO lookup
- **FR-003**: System MUST display manual entry form for non-PO items with Part ID field and "Look Up Part" button that queries Infor Visual database directly for part information (Part Type, Description)
- **FR-004**: System MUST validate that the entered Part ID exists in Infor Visual database for non-PO items and display error if part not found
- **FR-005**: System MUST query Infor Visual database (SQL Server) when user enters a PO number to retrieve all associated parts with their IDs, types, quantities, and descriptions
- **FR-006**: System MUST validate that PO numbers are numeric and up to 6 digits in length when PO entry is used
- **FR-007**: System MUST skip PO quantity validation (FR-013) and same-day receiving checks (FR-014) for non-PO items while still validating part ID existence
- **FR-008**: System MUST save non-PO item records with null or "N/A" PO number indicator in all destinations (CSV, database)
- **FR-009**: System MUST allow users to select one part at a time from the loaded PO parts list
- **FR-010**: System MUST allow users to specify the number of lines (skids) between 1 and 99 for each part
- **FR-011**: System MUST create one receiving line entry for each skid, each capable of holding quantity, heat/lot number, and package count
- **FR-012**: System MUST validate that all line quantities are greater than 0 before allowing progression to heat number entry
- **FR-013**: System MUST validate the sum of all quantities for a part ID against the PO ordered quantity from Infor Visual and display warning if exceeded, allowing user to override (applies only to PO items)
- **FR-014**: System MUST query Infor Visual for same-day receipts (matching PO number, part ID, and current date) and warn user if discrepancies exist between Visual's received quantity and the user's entered total (applies only to PO items)
- **FR-015**: System MUST allow users to enter alphanumeric heat/lot numbers for each line
- **FR-016**: System MUST provide a quick-select mechanism showing all unique heat numbers entered in the current part, allowing users to apply a heat number to multiple lines via checkbox selection
- **FR-017**: System MUST allow users to enter packages per skid for each line (numeric, greater than 0)
- **FR-018**: System MUST automatically determine package type based on part ID prefix: "Coils" for part IDs starting with "MMC", "Sheets" for part IDs starting with "MMF"
- **FR-019**: System MUST display a package type dropdown for all lines, allowing users to override default package types with options: "Coils", "Sheets", "Boxes", "Custom"
- **FR-020**: System MUST allow users to enter a custom package type name when "Custom" is selected from the package type dropdown
- **FR-021**: System MUST persist package type preferences per part ID to MySQL database when user changes from the default
- **FR-022**: System MUST retrieve and apply saved package type preferences for part IDs that have been previously customized
- **FR-023**: System MUST calculate and display weight per package as (Quantity ÷ Packages) with the package type name (e.g., "50 lbs per Coil", "25 lbs per Sheet")
- **FR-024**: System MUST provide a review screen showing total lines entered, total quantity, and part identification before saving
- **FR-025**: System MUST allow users to add multiple parts to the current session before saving (via "Add Another Part/PO" option)
- **FR-026**: System MUST save receiving line data to three destinations: local CSV file (%APPDATA%\ReceivingData.csv), network CSV file (\\mtmanu-fs01\...\JKOLL\ReceivingData.csv), and MySQL database (Note: CSV paths will be made configurable in future settings feature per CONFIGURABLE_SETTINGS.md)
- **FR-027**: System MUST display progress indicators during save operations showing percentage complete and current operation (local CSV, network CSV, database)
- **FR-028**: System MUST display save confirmation with file locations and record counts upon successful save
- **FR-024**: System MUST provide a startup ContentDialog asking whether to reset CSV files with warning that reset makes previous work unprintable
- **FR-025**: System MUST delete both local and network CSV files when user chooses "Yes, Reset" on the startup dialog
- **FR-026**: System MUST preserve existing CSV files when user chooses "No, Continue" on the startup dialog
- **FR-027**: System MUST display the receiving workflow UI within the MainWindow.xaml NavigationView content frame (not separate windows)
- **FR-028**: System MUST display only actual dialog boxes (like CSV reset prompt) as ContentDialog elements, not main workflow steps
- **FR-029**: System MUST persist session state to a JSON file in %APPDATA% allowing users to navigate away, close the application, and return without losing entered data
- **FR-030**: System MUST automatically load persisted session state from JSON file when application starts if in-progress session exists
- **FR-031**: System MUST clear persisted session state (delete JSON file) after successful save operation
- **FR-032**: System MUST display status messages throughout the workflow to guide users and confirm actions
- **FR-033**: System MUST enable/disable buttons contextually based on data validity (e.g., "Continue" button disabled until required data entered)

### Key Entities

- **ReceivingLine**: Represents one skid of received material with attributes: PartID, PartType, PONumber (nullable for non-PO items), IsNonPOItem (boolean flag), Quantity, HeatNumber (lot number), PackagesOnSkid, PackageTypeName (e.g., "Coils", "Sheets", "Boxes"), WeightPerPackage (calculated), LineNumber
- **InforVisualPO**: Represents a purchase order from Infor Visual with attributes: PONumber, Parts (collection of InforVisualPart)
- **InforVisualPart**: Represents a part on a PO with attributes: PartID, PartType, QuantityOrdered, Description
- **ReceivingSession**: Represents the current data entry session with collection of ReceivingLines, allowing accumulation of multiple parts before saving
- **HeatCheckboxItem**: UI helper entity representing a selectable heat number with attributes: HeatNumber, IsChecked, LineNumber (where first entered)
- **PackageTypePreference**: Represents a saved package type preference with attributes: PartID, PackageTypeName (user's preferred package type for this part), LastModified (timestamp)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can complete a single-part receiving entry (all 9 steps) in under 3 minutes with typical data
- **SC-002**: System successfully queries Infor Visual database and returns PO data within 2 seconds for 95% of requests
- **SC-003**: System saves receiving data to all three destinations (local CSV, network CSV, MySQL) within 5 seconds for entries with up to 50 lines
- **SC-004**: Data entry error rate (invalid quantities, missing heat numbers) reduces by 60% compared to manual paper-based entry due to validation and guided workflow
- **SC-005**: Users successfully complete receiving entry on first attempt without errors for 90% of standard receiving operations (PO exists, network available, valid data)
- **SC-006**: Quick-select heat number feature reduces total keystrokes by 40% when entering multiple skids with the same heat number
- **SC-007**: System handles PO queries that return up to 100 parts without performance degradation
- **SC-008**: Progress indicators update smoothly during save operations, never appearing frozen to users
- **SC-009**: Package type defaults are correctly applied based on part ID prefix for 100% of MMC (Coils) and MMF (Sheets) parts
- **SC-010**: Saved package type preferences are correctly retrieved and applied for 100% of previously customized part IDs
- **SC-011**: Session state persists and restores correctly for 100% of application restarts, with no data loss of entered information
- **SC-012**: System correctly identifies and warns users when entered quantities exceed PO ordered quantities for 100% of over-receiving scenarios (PO items only)
- **SC-013**: System correctly detects same-day receiving transactions in Infor Visual and displays accurate discrepancy warnings for 100% of applicable cases (PO items only)
- **SC-014**: Users can successfully enter and save non-PO items without PO validation for 100% of customer-supplied material scenarios

## Assumptions *(optional)*

- **Infor Visual Connection**: Assumes connection string to Infor Visual SQL Server database is configured in application settings or environment variables, using stored credentials
- **Network Path Access**: Assumes user workstations have network access to \\mtmanu-fs01 file share with read/write permissions to the specified directory
- **MySQL Database Schema**: Assumes receiving_lines table exists in MySQL database with columns matching ReceivingLine entity attributes including PackageTypeName (or will be created as part of this feature); also assumes package_type_preferences table exists for storing user preferences per part ID
- **CSV Format**: Assumes CSV files use standard comma-separated format with headers matching ReceivingLine properties including PackageTypeName column
- **CSV File Paths**: Assumes CSV file paths are hard-coded in this feature (%APPDATA%\ReceivingData.csv and \\mtmanu-fs01\...\JKOLL\ReceivingData.csv); these will be made configurable through a future settings page as documented in CONFIGURABLE_SETTINGS.md
- **Authentication**: Assumes users are already authenticated through Windows authentication; no additional application-level login required for this feature
- **Single User Session**: Assumes one user per workstation; no concurrent editing of the same session by multiple users
- **Data Persistence**: Assumes session data persists to JSON file in %APPDATA%\MTM_Receiving_Application folder; session automatically restores on application restart if unsaved work exists
- **JSON File Location**: Assumes %APPDATA%\MTM_Receiving_Application\session.json is used for persistence with read/write permissions available
- **Infor Visual Schema**: Assumes Infor Visual database has accessible views or tables containing PO and Part information with standard column names (PONumber, PartID, PartType, QuantityOrdered, Description), plus receiving transaction tables with date/time stamps for same-day receipt queries
- **Performance Expectations**: Assumes standard desktop hardware (4GB RAM, modern processor) and local network speeds (100 Mbps+)
- **Validation Rules**: Assumes standard validation rules (numeric quantities, non-empty heat numbers, positive values) without complex business rule validation requiring external service calls

## Dependencies *(optional)*

- **Phase 1 Infrastructure**: Complete - Application framework, MVVM structure, dependency injection, and base services exist
- **Database Tables**: 
  - MySQL: `receiving_lines` table with PackageTypeName column (may need creation/modification)
  - MySQL: `package_type_preferences` table with PartID, PackageTypeName, LastModified columns (may need creation)
  - SQL Server (Infor Visual): PO and Part tables/views (read-only access assumed)
- **Stored Procedures**: 
  - SQL Server: Query for fetching PO data with parts (e.g., `sp_GetPOWithParts @PONumber`)
  - SQL Server: Query for fetching part details by Part ID for non-PO items (e.g., `sp_GetPartByID @PartID`)
  - SQL Server: Query for fetching same-day receiving records (e.g., `sp_GetReceivingByPOPartDate @PONumber, @PartID, @Date`)
  - MySQL: Insert/update procedures for receiving_lines (or ADO.NET direct inserts)
- **External Systems**: 
  - Infor Visual SQL Server database (read access)
  - Network file share \\mtmanu-fs01 (read/write access)
- **Other Features**: 
  - Authentication system (for workstation/user identification in saved data)
  - Configuration system (for database connection strings and file paths)
  - Logging service (for error tracking and audit trail)
- **NuGet Packages**:
  - CommunityToolkit.Mvvm (for RelayCommand and ObservableProperty)
  - Microsoft.Data.SqlClient (for SQL Server connectivity)
  - MySql.Data or MySqlConnector (for MySQL connectivity)
  - CsvHelper or similar (for CSV file operations)
  - System.Text.Json or Newtonsoft.Json (for session state JSON serialization)

## Out of Scope *(optional)*

- **Label Printing**: The actual printing of labels is not part of this feature; only data entry and saving. Printing will be handled by separate feature using the saved CSV/database data
- **Data Editing**: No ability to edit previously saved receiving lines; this feature only handles new data entry
- **History Viewing**: The "View History" button mentioned in mockups is out of scope; that's a separate feature
- **Settings Page**: Configuration of database connections, CSV file paths, and other settings through UI is out of scope for this feature; CSV paths will be hard-coded initially and made configurable in a future settings implementation (see CONFIGURABLE_SETTINGS.md)
- **User Management**: No user accounts, permissions, or role-based access control
- **Advanced Reporting**: No analytics, dashboards, or reporting on receiving data
- **Mobile/Tablet Support**: Desktop-only; no responsive design for touch devices
- **Full Offline Mode**: While session state persists across application restarts, the feature still requires live database and network access for PO queries and final save operations (no full offline mode with later synchronization)
- **Barcode Scanning**: No barcode scanner integration for PO numbers, part numbers, or heat numbers
- **Batch Import**: No ability to import receiving data from external files or systems
- **Data Validation Against Inventory**: No validation that received quantities match expected quantities or update inventory systems
- **Multi-language Support**: English-only UI
- **Keyboard Shortcuts**: No custom keyboard shortcuts for navigation or actions (only standard Tab/Enter behavior)
