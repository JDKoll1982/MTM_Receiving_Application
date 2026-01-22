# Module_Volvo - Quick Reference Guide

**Version:** 1.0.0 | **Generated:** 2026-01-17 | **Architecture:** CQRS with MediatR

---

## 📋 Module Overview

**Purpose:** Volvo-specific dunnage shipment management, including part catalog, shipment entry, label generation, and email requisition processing.

**Key Features:**

- Volvo part master catalog management (add, edit, deactivate, import/export CSV)
- Shipment entry with discrepancy tracking
- Barcode label generation (CSV for Bartender)
- PO requisition email formatting
- Shipment history with filtering and export
- Pending shipment save/resume workflow

---

## 🎯 Commands (Write Operations)

### 1. AddPartToShipmentCommand

**Purpose:** Add a part to the current shipment (in-memory operation, not persisted until save/complete).

**Handler:** `AddPartToShipmentCommandHandler`
**Validator:** `AddPartToShipmentCommandValidator`

**Parameters:**

- `PartNumber` (string) - Part number from Volvo master data
- `ReceivedSkidCount` (int) - Number of skids received
- `ExpectedSkidCount` (int?) - Expected number of skids (when HasDiscrepancy = true)
- `HasDiscrepancy` (bool) - Indicates discrepancy between received and expected counts
- `DiscrepancyNote` (string?) - Note explaining discrepancy (required when HasDiscrepancy = true)
- `PendingShipmentId` (int?) - Optional pending shipment ID (for editing pending shipments)

**Returns:** `Model_Dao_Result`

**Usage Example:**

```csharp
var command = new AddPartToShipmentCommand
{
    PartNumber = "V-EMB-500",
    ReceivedSkidCount = 10,
    HasDiscrepancy = false
};
var result = await _mediator.Send(command);
```

---

### 2. AddVolvoPartCommand

**Purpose:** Add a new Volvo part to the master catalog.

**Handler:** `AddVolvoPartCommandHandler`
**Validator:** `AddVolvoPartCommandValidator`

**Parameters:**

- `PartNumber` (string) - Part number to add
- `QuantityPerSkid` (int) - Quantity per skid

**Returns:** `Model_Dao_Result`

**Validation Rules:**

- PartNumber: Required, max 50 characters
- QuantityPerSkid: Greater than 0

---

### 3. CompleteShipmentCommand

**Purpose:** Finalize shipment (generates labels, sends email, marks as completed).

**Handler:** `CompleteShipmentCommandHandler`
**Validator:** `CompleteShipmentCommandValidator`

**Parameters:**

- `ShipmentDate` (DateTimeOffset) - Shipment date
- `ShipmentNumber` (int) - Shipment number (auto-generated if 0)
- `Notes` (string) - Optional notes for this shipment
- `Parts` (List<ShipmentLineDto>) - List of parts in this shipment
- `PONumber` (string) - PO number for requisition
- `ReceiverNumber` (string) - Receiver number for tracking

**Returns:** `Model_Dao_Result<int>` (Shipment ID)

**Validation Rules:**

- ShipmentDate: Required
- Parts: At least one part required
- PONumber: Required, max 50 characters
- ReceiverNumber: Required, max 50 characters

---

### 4. DeactivateVolvoPartCommand

**Purpose:** Deactivate a Volvo part (hides from dropdown but preserves historical data).

**Handler:** `DeactivateVolvoPartCommandHandler`
**Validator:** `DeactivateVolvoPartCommandValidator`

**Parameters:**

- `PartNumber` (string) - Part number to deactivate

**Returns:** `Model_Dao_Result`

**Validation Rules:**

- PartNumber: Required

---

### 5. ImportPartsCsvCommand

**Purpose:** Bulk import Volvo parts from CSV file.

**Handler:** `ImportPartsCsvCommandHandler`
**Validator:** `ImportPartsCsvCommandValidator`

**Parameters:**

- `CsvFilePath` (string) - Full file path to CSV file containing part data (PartNumber, QuantityPerSkid)

**Returns:** `Model_Dao_Result<ImportPartsCsvResult>`

**Validation Rules:**

- CsvFilePath: Required, file must exist

---

### 6. RemovePartFromShipmentCommand

**Purpose:** Remove a part from the current shipment (in-memory operation).

**Handler:** `RemovePartFromShipmentCommandHandler`

**Parameters:**

- `PartNumber` (string) - Part number to remove from current shipment

**Returns:** `Model_Dao_Result`

**Note:** No validator exists (simple removal operation)

---

### 7. SavePendingShipmentCommand

**Purpose:** Save shipment as pending (allows user to resume later).

**Handler:** `SavePendingShipmentCommandHandler`
**Validator:** `SavePendingShipmentCommandValidator`

**Parameters:**

- `ShipmentId` (int?) - Optional existing shipment ID (null for new pending shipment)
- `ShipmentDate` (DateTimeOffset) - Shipment date
- `ShipmentNumber` (int) - Shipment number (auto-generated if not provided)
- `Notes` (string) - Optional notes for this shipment
- `Parts` (List<ShipmentLineDto>) - List of parts in this shipment

**Returns:** `Model_Dao_Result<int>` (Shipment ID)

**Validation Rules:**

- ShipmentDate: Required
- Parts: At least one part required

---

### 8. UpdateShipmentCommand

**Purpose:** Update an existing shipment (for pending or completed shipments).

**Handler:** `UpdateShipmentCommandHandler`
**Validator:** `UpdateShipmentCommandValidator`

**Parameters:**

- `ShipmentId` (int) - Shipment ID to update
- `ShipmentDate` (DateTimeOffset) - Updated shipment date
- `Notes` (string) - Updated notes
- `PONumber` (string) - Updated PO number (if completed)
- `ReceiverNumber` (string) - Updated receiver number (if completed)
- `Parts` (List<ShipmentLineDto>) - Updated shipment line items

**Returns:** `Model_Dao_Result`

**Validation Rules:**

- ShipmentId: Greater than 0
- ShipmentDate: Required
- Parts: At least one part required

---

### 9. UpdateVolvoPartCommand

**Purpose:** Update an existing Volvo part in the master catalog.

**Handler:** `UpdateVolvoPartCommandHandler`
**Validator:** `UpdateVolvoPartCommandValidator`

**Parameters:**

- `PartNumber` (string) - Part number to update
- `QuantityPerSkid` (int) - Updated quantity per skid

**Returns:** `Model_Dao_Result`

**Validation Rules:**

- PartNumber: Required
- QuantityPerSkid: Greater than 0

---

## 🔍 Queries (Read Operations)

### 1. ExportPartsCsvQuery

**Purpose:** Export Volvo parts catalog to CSV file.

**Handler:** `ExportPartsCsvQueryHandler`

**Parameters:**

- `IncludeInactive` (bool) - Include inactive parts in the export (default: false)

**Returns:** `Model_Dao_Result<string>` (CSV file path)

---

### 2. ExportShipmentsQuery

**Purpose:** Export shipment history to CSV file with filtering.

**Handler:** `ExportShipmentsQueryHandler`

**Parameters:**

- `StartDate` (DateTimeOffset?) - Optional start date filter
- `EndDate` (DateTimeOffset?) - Optional end date filter
- `StatusFilter` (string) - Optional status filter ("All", "Pending PO", "Completed", or raw status string)

**Returns:** `Model_Dao_Result<string>` (CSV file path)

---

### 3. FormatEmailDataQuery

**Purpose:** Generate email data for PO requisition (subject, body, discrepancies).

**Handler:** `FormatEmailDataQueryHandler`

**Parameters:**

- `ShipmentId` (int) - Shipment ID to format email data for

**Returns:** `Model_Dao_Result<Model_VolvoEmailData>`

---

### 4. GenerateLabelCsvQuery

**Purpose:** Generate barcode label CSV for Bartender label printer.

**Handler:** `GenerateLabelCsvQueryHandler`

**Parameters:**

- `ShipmentId` (int) - Shipment ID to generate labels for

**Returns:** `Model_Dao_Result<string>` (CSV file path)

---

### 5. GetAllVolvoPartsQuery

**Purpose:** Retrieve all Volvo parts for settings grid or dropdown population.

**Handler:** `GetAllVolvoPartsQueryHandler`

**Parameters:**

- `IncludeInactive` (bool) - Include inactive parts in the result set (default: false)

**Returns:** `Model_Dao_Result<List<Model_VolvoPart>>`

---

### 6. GetInitialShipmentDataQuery

**Purpose:** Get current date and next available shipment number for new shipment entry.

**Handler:** `GetInitialShipmentDataQueryHandler`

**Parameters:** (None)

**Returns:** `Model_Dao_Result<InitialShipmentData>`

---

### 7. GetPartComponentsQuery

**Purpose:** Retrieve components for a parent Volvo part (for multi-part assemblies).

**Handler:** `GetPartComponentsQueryHandler`

**Parameters:**

- `PartNumber` (string) - Parent part number to retrieve components for

**Returns:** `Model_Dao_Result<List<Model_VolvoPartComponent>>`

---

### 8. GetPendingShipmentQuery

**Purpose:** Find pending shipment for the current user (for resume workflow).

**Handler:** `GetPendingShipmentQueryHandler`

**Parameters:**

- `UserName` (string) - Username of current user to find their pending shipment

**Returns:** `Model_Dao_Result<Model_VolvoShipment>`

---

### 9. GetRecentShipmentsQuery

**Purpose:** Retrieve recent shipments for dashboard or quick access.

**Handler:** `GetRecentShipmentsQueryHandler`

**Parameters:**

- `Days` (int) - Number of days to look back (default: 30)

**Returns:** `Model_Dao_Result<List<Model_VolvoShipment>>`

---

### 10. GetShipmentDetailQuery

**Purpose:** Retrieve full shipment details including lines for viewing/editing.

**Handler:** `GetShipmentDetailQueryHandler`

**Parameters:**

- `ShipmentId` (int) - Shipment ID to retrieve

**Returns:** `Model_Dao_Result<ShipmentDetail>`

---

### 11. GetShipmentHistoryQuery

**Purpose:** Retrieve shipment history with date/status filtering.

**Handler:** `GetShipmentHistoryQueryHandler`

**Parameters:**

- `StartDate` (DateTimeOffset?) - Optional start date filter
- `EndDate` (DateTimeOffset?) - Optional end date filter
- `StatusFilter` (string) - Optional status filter ("All", "Pending PO", "Completed", or raw status string)

**Returns:** `Model_Dao_Result<List<Model_VolvoShipment>>`

---

### 12. SearchVolvoPartsQuery

**Purpose:** Search Volvo parts by partial part number match (for autocomplete/dropdown).

**Handler:** `SearchVolvoPartsQueryHandler`

**Parameters:**

- `SearchText` (string) - Search text to match against part numbers (partial match supported)
- `MaxResults` (int) - Maximum number of results to return (default: 10)

**Returns:** `Model_Dao_Result<List<Model_VolvoPart>>`

---

## 🎨 ViewModels

### 1. ViewModel_Volvo_History

**Purpose:** View and manage Volvo shipment history with filtering and export.

**Base Class:** `ViewModel_Shared_Base`

**Observable Properties:**

- `History` (ObservableCollection<Model_VolvoShipment>) - Shipment history collection
- `SelectedShipment` (Model_VolvoShipment?) - Currently selected shipment
- `StartDate` (DateTimeOffset?) - Start date filter (default: 30 days ago)
- `EndDate` (DateTimeOffset?) - End date filter (default: today)
- `StatusFilter` (string) - Status filter ("All", "Pending PO", "Completed")
- `StatusOptions` (ObservableCollection<string>) - Available status filter options

**Commands:**

- `GoBackCommand` - Navigate back to previous view
- `LoadRecentShipmentsCommand` - Load shipments from last 30 days
- `FilterCommand` - Filter history by date/status
- `ViewDetailCommand` - View shipment details
- `EditCommand` - Edit existing shipment
- `ExportCommand` - Export filtered history to CSV

**Dependencies:**

- `IMediator` - For sending queries/commands
- `IService_ErrorHandler` - For error handling
- `IService_LoggingUtility` - For logging

---

### 2. ViewModel_Volvo_Settings

**Purpose:** Manage Volvo master data settings (parts catalog).

**Base Class:** `ViewModel_Shared_Base`

**Observable Properties:**

- `Parts` (ObservableCollection<Model_VolvoPart>) - Parts catalog collection
- `SelectedPart` (Model_VolvoPart?) - Currently selected part
- `ShowInactive` (bool) - Include inactive parts in grid (default: false)
- `TotalPartsCount` (int) - Total parts count
- `ActivePartsCount` (int) - Active parts count

**Commands:**

- `RefreshCommand` - Reload parts catalog
- `AddPartCommand` - Add new part to catalog
- `EditPartCommand` - Edit existing part
- `DeactivatePartCommand` - Deactivate selected part
- `ViewComponentsCommand` - View components for selected part
- `ImportCsvCommand` - Import parts from CSV file
- `ExportCsvCommand` - Export parts to CSV file

**Dependencies:**

- `IMediator` - For sending queries/commands
- `IService_ErrorHandler` - For error handling
- `IService_LoggingUtility` - For logging

---

### 3. ViewModel_Volvo_ShipmentEntry

**Purpose:** Enter new Volvo shipments with discrepancy tracking, label generation, and email preview.

**Base Class:** `ViewModel_Shared_Base`

**Observable Properties:**

- `ShipmentDate` (DateTimeOffset?) - Shipment date (default: today)
- `ShipmentNumber` (int) - Shipment number (auto-assigned)
- `Notes` (string) - Optional shipment notes
- `Parts` (ObservableCollection<Model_VolvoShipmentLine>) - Shipment line items
- `AvailableParts` (ObservableCollection<Model_VolvoPart>) - Available parts for selection
- `SuggestedParts` (ObservableCollection<Model_VolvoPart>) - Auto-suggest parts based on search
- `SelectedPart` (Model_VolvoShipmentLine?) - Currently selected shipment line
- `SelectedPartToAdd` (Model_VolvoPart?) - Part selected for addition
- `PartSearchText` (string) - Part search/autocomplete text
- `ReceivedSkidsToAdd` (string) - Number of received skids to add
- `CanSave` (bool) - Indicates if shipment can be saved
- `IsSuccessMessageVisible` (bool) - Success message visibility flag
- `SuccessMessage` (string) - Success message text
- `HasPendingShipment` (bool) - Indicates if user has a pending shipment

**Commands:**

- `InitializeCommand` - Load initial data and check for pending shipment
- `LoadAllPartsCommand` - Load all active parts
- `LoadPendingShipmentCommand` - Resume pending shipment if exists
- `AddPartCommand` - Add selected part to shipment
- `RemovePartCommand` - Remove part from shipment
- `GenerateLabelsCommand` - Generate barcode label CSV
- `PreviewEmailCommand` - Preview PO requisition email
- `SaveAsPendingCommand` - Save shipment as pending
- `CompleteShipmentCommand` - Complete shipment with PO/Receiver numbers
- `ToggleDiscrepancyCommand` - Mark/unmark discrepancy for a line
- `ViewHistoryCommand` - Navigate to shipment history
- `StartNewEntryCommand` - Clear form and start new entry

**Dependencies:**

- `IMediator` - For sending queries/commands
- `IService_ErrorHandler` - For error handling
- `IService_LoggingUtility` - For logging
- `IService_Window` - For dialog XamlRoot access
- `IService_UserSessionManager` - For current user info

---

## 📦 Data Access Objects (DAOs)

### 1. Dao_VolvoPart

**Purpose:** Data access for Volvo parts catalog operations.

**Database:** MySQL (stored procedures only)

**Methods:**

- `GetPartAsync(string partNumber)` - Get single part by part number
- `GetAllPartsAsync(bool includeInactive)` - Get all parts (optionally include inactive)
- `SearchPartsAsync(string searchText, int maxResults)` - Search parts by partial match
- `InsertPartAsync(Model_VolvoPart part)` - Insert new part
- `UpdatePartAsync(Model_VolvoPart part)` - Update existing part
- `DeactivatePartAsync(string partNumber)` - Deactivate part (soft delete)

**Stored Procedures:**

- `sp_Volvo_Part_Get`
- `sp_Volvo_Part_GetAll`
- `sp_Volvo_Part_Search`
- `sp_Volvo_Part_Insert`
- `sp_Volvo_Part_Update`
- `sp_Volvo_Part_Deactivate`

---

### 2. Dao_VolvoPartComponent

**Purpose:** Data access for Volvo part component relationships (multi-part assemblies).

**Database:** MySQL (stored procedures only)

**Methods:**

- `GetComponentsByPartNumberAsync(string partNumber)` - Get components for parent part
- `InsertComponentAsync(Model_VolvoPartComponent component)` - Insert component relationship
- `DeleteComponentAsync(int componentId)` - Delete component relationship

**Stored Procedures:**

- `sp_Volvo_PartComponent_GetByPartNumber`
- `sp_Volvo_PartComponent_Insert`
- `sp_Volvo_PartComponent_Delete`

---

### 3. Dao_VolvoSettings

**Purpose:** Data access for Volvo module settings (email recipients, etc.).

**Database:** MySQL (stored procedures only)

**Methods:**

- `GetSettingAsync(string settingKey)` - Get single setting by key
- `GetAllSettingsAsync(string? category)` - Get all settings (optionally filtered by category)
- `UpsertSettingAsync(string settingKey, string settingValue, string modifiedBy)` - Insert or update setting
- `ResetSettingAsync(string settingKey, string modifiedBy)` - Reset setting to default value

**Stored Procedures:**

- `sp_Volvo_Settings_Get`
- `sp_Volvo_Settings_GetAll`
- `sp_Volvo_Settings_Upsert`
- `sp_Volvo_Settings_Reset`

**Settings Keys:**

- `email_to_recipients` - JSON array of TO recipients for PO requisition emails
- `email_cc_recipients` - JSON array of CC recipients for PO requisition emails

---

### 4. Dao_VolvoShipment

**Purpose:** Data access for Volvo shipment header operations.

**Database:** MySQL (stored procedures only)

**Methods:**

- `GetShipmentAsync(int shipmentId)` - Get single shipment by ID
- `GetShipmentByNumberAsync(int shipmentNumber)` - Get shipment by shipment number
- `GetRecentShipmentsAsync(int days)` - Get recent shipments (last N days)
- `GetShipmentHistoryAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, string statusFilter)` - Get filtered history
- `GetPendingShipmentAsync(string userName)` - Get pending shipment for user
- `GetNextShipmentNumberAsync()` - Get next available shipment number
- `InsertShipmentAsync(Model_VolvoShipment shipment)` - Insert new shipment
- `UpdateShipmentAsync(Model_VolvoShipment shipment)` - Update existing shipment
- `CompleteShipmentAsync(int shipmentId, string poNumber, string receiverNumber)` - Mark shipment as completed
- `DeletePendingShipmentAsync(int shipmentId)` - Delete pending shipment

**Stored Procedures:**

- `sp_Volvo_Shipment_Get`
- `sp_Volvo_Shipment_GetByNumber`
- `sp_Volvo_Shipment_GetRecent`
- `sp_Volvo_Shipment_GetHistory`
- `sp_Volvo_Shipment_GetPending`
- `sp_Volvo_Shipment_GetNextNumber`
- `sp_Volvo_Shipment_Insert`
- `sp_Volvo_Shipment_Update`
- `sp_Volvo_Shipment_Complete`
- `sp_Volvo_Shipment_DeletePending`

---

### 5. Dao_VolvoShipmentLine

**Purpose:** Data access for Volvo shipment line item operations.

**Database:** MySQL (stored procedures only)

**Methods:**

- `GetLinesByShipmentIdAsync(int shipmentId)` - Get all lines for a shipment
- `InsertLineAsync(Model_VolvoShipmentLine line)` - Insert new shipment line
- `UpdateLineAsync(Model_VolvoShipmentLine line)` - Update existing shipment line
- `DeleteLineAsync(int lineId)` - Delete shipment line
- `DeleteLinesByShipmentIdAsync(int shipmentId)` - Delete all lines for a shipment

**Stored Procedures:**

- `sp_Volvo_ShipmentLine_GetByShipmentId`
- `sp_Volvo_ShipmentLine_Insert`
- `sp_Volvo_ShipmentLine_Update`
- `sp_Volvo_ShipmentLine_Delete`
- `sp_Volvo_ShipmentLine_DeleteByShipmentId`

---

## 📊 Models

### 1. Model_VolvoPart

**Purpose:** Volvo dunnage part in the master catalog.

**Properties:**

- `PartNumber` (string) - Part number (primary key, e.g., V-EMB-1, V-EMB-500)
- `QuantityPerSkid` (int) - Number of pieces per skid (e.g., V-EMB-2 = 20 pcs/skid)
- `IsActive` (bool) - Flag indicating if part is active (default: true)
- `CreatedDate` (DateTime) - Timestamp when part was created
- `ModifiedDate` (DateTime) - Timestamp when part was last modified

---

### 2. Model_VolvoPartComponent

**Purpose:** Component relationship for multi-part assemblies.

**Properties:**

- `ComponentId` (int) - Component ID (primary key)
- `ParentPartNumber` (string) - Parent part number
- `ChildPartNumber` (string) - Child part number
- `Quantity` (int) - Quantity of child part in parent assembly
- `CreatedDate` (DateTime) - Timestamp when relationship was created

---

### 3. Model_VolvoSetting

**Purpose:** Configuration setting for Volvo module.

**Properties:**

- `SettingKey` (string) - Setting key (primary key)
- `SettingValue` (string) - Setting value (JSON or plain text)
- `Category` (string) - Setting category for grouping
- `DefaultValue` (string) - Default value for reset
- `Description` (string) - Human-readable description
- `ModifiedBy` (string) - Last modified by username
- `ModifiedDate` (DateTime) - Timestamp when setting was last modified

---

### 4. Model_VolvoShipment

**Purpose:** Volvo dunnage shipment header.

**Properties:**

- `ShipmentId` (int) - Shipment ID (primary key)
- `ShipmentDate` (DateTimeOffset) - Shipment date
- `ShipmentNumber` (int) - Shipment number (display identifier)
- `Status` (VolvoShipmentStatus) - Shipment status (Pending, PendingPO, Completed)
- `Notes` (string) - Optional shipment notes
- `PONumber` (string) - PO number (when completed)
- `ReceiverNumber` (string) - Receiver number (when completed)
- `CreatedBy` (string) - Created by username
- `CreatedDate` (DateTime) - Timestamp when shipment was created
- `ModifiedBy` (string) - Last modified by username
- `ModifiedDate` (DateTime) - Timestamp when shipment was last modified
- `Lines` (List<Model_VolvoShipmentLine>) - Shipment line items (navigation property)

---

### 5. Model_VolvoShipmentLine

**Purpose:** Volvo shipment line item (part on a shipment).

**Properties:**

- `LineId` (int) - Line ID (primary key)
- `ShipmentId` (int) - Parent shipment ID (foreign key)
- `PartNumber` (string) - Part number
- `ReceivedSkidCount` (int) - Number of skids received
- `ExpectedSkidCount` (int?) - Expected number of skids (when discrepancy exists)
- `HasDiscrepancy` (bool) - Indicates discrepancy between received and expected counts
- `DiscrepancyNote` (string?) - Note explaining discrepancy
- `QuantityPerSkid` (int) - Cached quantity per skid (from master data)
- `TotalPieces` (int) - Calculated total pieces (ReceivedSkidCount * QuantityPerSkid)
- `CreatedDate` (DateTime) - Timestamp when line was created

---

### 6. Model_VolvoEmailData

**Purpose:** Formatted email data for PO requisition.

**Properties:**

- `Subject` (string) - Email subject line
- `Greeting` (string) - Email greeting
- `Message` (string) - Email message body
- `RequestedLines` (Dictionary<string, int>) - Requested lines (PartNumber → Quantity)
- `Discrepancies` (List<DiscrepancyInfo>) - List of discrepancies
- `AdditionalNotes` (string) - Additional notes
- `Signature` (string) - Email signature

---

### 7. Model_EmailRecipient

**Purpose:** Email recipient for settings (TO/CC).

**Properties:**

- `Name` (string) - Recipient display name
- `Email` (string) - Recipient email address

**Methods:**

- `ToOutlookFormat()` - Returns formatted string: "Name <email@domain.com>"

---

## 🔐 Services

### 1. Service_Volvo

**Purpose:** Volvo module-specific business logic (legacy service, being migrated to CQRS).

**Interface:** `IService_Volvo`

**Key Methods:**

- `GetAllPartsAsync()` - Get all active Volvo parts
- `SaveShipmentAsync()` - Save shipment (legacy method)
- `GenerateLabelCsvAsync(int shipmentId)` - Generate barcode label CSV
- `SendEmailAsync(int shipmentId)` - Send PO requisition email

**Note:** This service is gradually being replaced by CQRS handlers. New code should use `IMediator` instead.

---

### 2. Service_VolvoMasterData

**Purpose:** Volvo master data management (parts catalog).

**Interface:** `IService_VolvoMasterData`

**Key Methods:**

- `GetPartAsync(string partNumber)` - Get single part
- `AddPartAsync(Model_VolvoPart part)` - Add new part
- `UpdatePartAsync(Model_VolvoPart part)` - Update existing part
- `DeactivatePartAsync(string partNumber)` - Deactivate part
- `ImportPartsFromCsvAsync(string filePath)` - Import parts from CSV
- `ExportPartsToCsvAsync(string filePath, bool includeInactive)` - Export parts to CSV

---

### 3. Service_VolvoAuthorization

**Purpose:** Authorization checks for Volvo module operations.

**Interface:** `IService_VolvoAuthorization`

**Key Methods:**

- `CanUserEditParts()` - Check if user can edit parts catalog
- `CanUserManageShipments()` - Check if user can create/edit shipments
- `CanUserViewHistory()` - Check if user can view shipment history

**Note:** This service provides service-level authorization. No explicit handler-level attributes found.

---

## 🧪 Testing Strategy

### Unit Tests

**Location:** `MTM_Receiving_Application.Tests/Module_Volvo/`

**Coverage:**

- All command handlers (9 tests)
- All query handlers (12 tests)
- All validators (8 tests)

**Test Pattern:** AAA (Arrange-Act-Assert)
**Assertion Library:** FluentAssertions

### Integration Tests

**Location:** `MTM_Receiving_Application.Tests/Module_Volvo/Integration/`

**Coverage:**

- End-to-end shipment workflows
- CSV import/export operations
- Label generation workflow

---

## 📝 Notes for GitHub Copilot

### CQRS Modernization Status

✅ **Fully Modernized:** Module_Volvo uses CQRS with MediatR  
✅ **Validators:** FluentValidation validators exist for all commands  
✅ **Handlers:** All handlers follow `IRequestHandler<TRequest, TResponse>` pattern  
⚠️ **Legacy Services:** `Service_Volvo` still exists but is being phased out

### Common Patterns

- **DAO Pattern:** All DAOs are instance-based, accept connection string in constructor
- **MVVM Pattern:** All ViewModels are partial classes inheriting from `ViewModel_Shared_Base`
- **Command Pattern:** All commands are `record` types implementing `IRequest<T>`
- **Query Pattern:** All queries are `record` types implementing `IRequest<T>`

### Best Practices

1. **New Features:** Use `IMediator.Send()` instead of calling services directly
2. **Validation:** Add FluentValidation validators for all new commands
3. **Testing:** Add unit tests for all new handlers and validators
4. **DAOs:** Use stored procedures only (no raw SQL in C#)
5. **Error Handling:** All handlers return `Model_Dao_Result` or `Model_Dao_Result<T>`

---

**For more details, see:**

- `.github/copilot-instructions.md` - Project-wide standards
- `.specify/memory/constitution.md` - Architectural non-negotiables
- `Module_Volvo/SETTABLE_OBJECTS.md` - Configuration inventory
- `Module_Volvo/PRIVILEGES.md` - Authorization requirements
