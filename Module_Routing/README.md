# Module_Routing - Internal Routing Labels

**Version:** 2.0
**Last Updated:** 2026-01-06
**Status:** ✅ Production Ready (with noted improvements pending)

---

## Purpose

The Routing Module enables receiving personnel to generate internal routing labels for packages and materials. Labels direct items to specific recipients (employees/departments) and are exported to CSV files for printing or external system integration.

**Key Features:**

- Wizard-based label creation (3-step process)
- PO validation via Infor Visual ERP integration
- Manual entry for non-PO packages ("OTHER" workflow)
- Quick Add buttons for frequently-used recipients
- Label editing and history tracking
- CSV export with automatic fallback (network → local)
- Usage analytics (personalized Quick Add based on employee patterns)

---

## Architecture

### 📐 MVVM Pattern

```
┌─────────────┐     ┌──────────────┐     ┌───────────┐
│   Views     │────▶│  ViewModels  │────▶│ Services  │
│   (XAML)    │     │   (Logic)    │     │ (Business)│
└─────────────┘     └──────────────┘     └─────┬─────┘
                                               │
                                               ▼
                                          ┌─────────┐
                                          │  DAOs   │
                                          │(Database)│
                                          └─────────┘
```

- **Views**: XAML only, no code-behind logic
- **ViewModels**: Business logic, data binding, command handlers
- **Services**: Orchestration, validation, external integration
- **DAOs**: Direct database access (MySQL + SQL Server read-only)

### 🗄️ Database Integration

**MySQL (`mtm_receiving_application`)** - Full CRUD:

- `routing_label_data` - Label master data
- `routing_recipients` - Recipient directory
- `routing_history` - Edit audit trail
- `routing_recipient_tracker` - Quick Add analytics
- `routing_po_alternatives` - Non-PO package types

**SQL Server (Infor Visual - `MTMFG`)** - READ ONLY:

- `po` - Purchase order validation
- `po_detail` - PO line items
- Warehouse: `'002'`

---

## Workflows

### 🎯 Wizard Mode (Primary Workflow)

```
Step 1: PO & Line Selection
├─ Enter PO Number → Validate in Infor Visual
├─ Select Line Item from DataGrid
└─ OR: Switch to OTHER (manual entry)

Step 2: Recipient Selection
├─ Quick Add Buttons (top 5 most-used)
├─ Searchable Recipient List
└─ Real-time filtering

Step 3: Review & Create
├─ Preview label data
├─ Confirm + Save to database
└─ Auto-export to CSV
```

### ✏️ Edit Mode

- View all labels (paginated list)
- Edit existing labels
- Delete labels (soft delete)
- Regenerate CSV if needed
- Full change history logged

### 📄 Manual Entry Mode

- Direct single-label creation
- For power users or bulk operations
- No wizard - all fields on one screen

---

## Services

### Core Services

| Service | Purpose | Dependencies |
|---------|---------|--------------|
| `RoutingService` | Label CRUD, validation, CSV export | Dao_RoutingLabel, Dao_RoutingLabelHistory, IRoutingInforVisualService |
| `RoutingRecipientService` | Recipient management, filtering | Dao_RoutingRecipient |
| `RoutingInforVisualService` | PO validation, line retrieval | Dao_InforVisualPO (SQL Server) |
| `RoutingUsageTrackingService` | Quick Add analytics | Dao_RoutingUsageTracking |

### Service Layer Responsibilities

- **Validation**: Business rules before database operations
- **Error Handling**: Catch exceptions, return `Model_Dao_Result` types
- **Logging**: All operations logged via `IService_LoggingUtility`
- **Integration**: Coordinate between DAOs, external systems (Infor Visual)

---

## Data Models

### Model_RoutingLabel

Primary entity representing a routing label.

**Key Properties:**

- `PONumber` - PO number or "OTHER"
- `LineNumber` - Line item or "0" for OTHER
- `RecipientId` - FK to routing_recipients
- `Quantity` - Item count
- `CsvExported` - Export status flag

### Model_RoutingRecipient

Employee/department receiving packages.

**Key Properties:**

- `Name` - Full name or department name
- `Location` - Physical location (e.g., "Building 2")
- `Department` - Organizational unit
- `IsActive` - Soft delete flag

### Model_RoutingOtherReason

Reasons for non-PO packages.

**Examples:**

- RETURNED (customer returns)
- SAMPLE (vendor samples)
- REWORK (internal rework items)

---

## CSV Export

### File Format

```csv
PO,Line,Part,Quantity,Recipient,Location,Date
12345,001,WIDGET-A,5,John Doe,Building 2,2026-01-06 14:30:00
```

### Export Logic

1. Try network path (configurable in `appsettings.json`)
2. Retry up to 3 times with 500ms delay
3. Fallback to local path if network fails
4. Mark label as `csv_exported = 1` on success
5. **Concurrency Safety**: Semaphore-based file locking (Issue #3 fix)

### Configuration

```json
{
  "RoutingModule": {
    "CsvExportPath": {
      "Network": "\\\\server\\share\\routing_label_data.csv",
      "Local": "C:\\RoutingLabels\\routing_label_data.csv"
    },
    "CsvRetry": {
      "MaxAttempts": 3,
      "DelayMs": 500
    }
  }
}
```

---

## Key ViewModels

### RoutingWizardContainerViewModel

- Orchestrates 3-step wizard
- Holds shared state (selected PO line, recipient)
- Navigation between steps

### RoutingWizardStep1ViewModel

- PO validation via Infor Visual
- OTHER mode toggle
- PO line selection

### RoutingWizardStep2ViewModel

- Quick Add recipient buttons (personalized)
- Real-time search/filter
- Recipient selection

### RoutingWizardStep3ViewModel

- Review selected data
- Final confirmation
- Trigger label creation

### RoutingEditModeViewModel

- Label list (paginated)
- Inline editing
- History viewing
- Label deletion

---

## Integration Points

### Infor Visual (SQL Server)

**Purpose**: PO validation and line item retrieval
**Access**: Read-only (`ApplicationIntent=ReadOnly`)
**Tables**: `po`, `po_detail`
**Warehouse Filter**: `site_ref = '002'`

**Example Query:**

```sql
SELECT po_num, po_line, part_id, qty_ordered, qty_received
FROM po_detail
WHERE po_num = @PoNumber AND site_ref = '002'
```

### Usage Tracking

**Purpose**: Personalize Quick Add buttons based on employee history
**Logic**:

- If employee has 20+ labels: Show their top 5 recipients
- Otherwise: Show system-wide top 5 recipients
- Increments on every label creation

---

## Error Handling

### DAO Layer

- Returns `Model_Dao_Result` or `Model_Dao_Result<T>`
- Never throws exceptions
- Wraps database errors in result objects

### Service Layer

- Catches DAO exceptions
- Logs via `IService_LoggingUtility`
- Returns `Model_Dao_Result` to ViewModels

### ViewModel Layer

- Try/catch around service calls
- Uses `IService_ErrorHandler` for user-facing errors
- Sets `StatusMessage` for UI feedback
- `IsBusy` flag during async operations

---

## Known Limitations & Future Enhancements

See [CODE_REVIEW.md](CODE_REVIEW.md) for complete list.

### High Priority

- **Issue #2**: Transaction handling for label creation + CSV export (data consistency)
- **Issue #7**: Remove hardcoded employee number placeholders (security risk)

### Medium Priority

- **Issue #13**: Implement TODO features (GetOtherReasonsAsync, session service)
- **Issue #18**: Batch insert for label history (performance)

### Planned Features (Not Yet Implemented)

- Label printing integration
- Barcode generation
- Batch label creation
- Label templates
- Advanced reporting

---

## Development Guidelines

### Adding a New Feature

1. **Update Database Schema** (if needed)
   - Create migration script in `Database/Migrations/`
   - Update stored procedures in `Database/StoredProcedures/`

2. **Create/Update Model** in `Models/`
   - Inherit from `ObservableObject`
   - Use `[ObservableProperty]` for bindable properties

3. **Create/Update DAO** in `Data/`
   - Instance-based class (not static)
   - Return `Model_Dao_Result` types
   - Use stored procedures exclusively (MySQL)

4. **Create/Update Service** in `Services/`
   - Create interface in `Contracts/Services/`
   - Implement business logic
   - Register in `App.xaml.cs` DI container

5. **Create ViewModel** in `ViewModels/`
   - Inherit from `BaseViewModel` (if available) or `ObservableObject`
   - Use `[RelayCommand]` for commands
   - Inject services via constructor

6. **Create View** in `Views/`
   - XAML only, no code-behind logic
   - Use `x:Bind` (compile-time binding)
   - Bind to ViewModel properties/commands

7. **Register in DI** (`App.xaml.cs`)
   - Services as Singleton or Transient
   - ViewModels as Transient (new instance per navigation)

---

## Testing

### Unit Tests

- Service layer validation logic
- ViewModel command logic
- Model validation

### Integration Tests

- DAO database operations
- CSV file export
- Infor Visual connectivity

### Manual Test Scenarios

1. Create label with valid PO
2. Create OTHER label
3. Edit existing label
4. Delete label (verify soft delete)
5. CSV export success/failure handling
6. Quick Add personalization (< 20 labels vs 20+ labels)

---

## Dependencies

### NuGet Packages

- `CommunityToolkit.Mvvm` - MVVM helpers
- `Microsoft.Extensions.DependencyInjection` - DI container
- `Microsoft.Extensions.Configuration` - Settings management
- `MySql.Data` - MySQL database access
- `Microsoft.Data.SqlClient` - SQL Server access (Infor Visual)

### Module Dependencies

- `Module_Core` - Shared services (logging, error handling, base classes)
- `Module_Shared` - Shared models and enums

---

## File Structure

```
Module_Routing/
├── Constants/
│   ├── Constant_Routing.cs              # Magic strings ("OTHER", etc.)
│   └── Constant_RoutingConfiguration.cs # Config key constants
├── Data/
│   ├── Dao_RoutingLabel.cs              # Label CRUD operations
│   ├── Dao_RoutingRecipient.cs          # Recipient operations
│   ├── Dao_InforVisualPO.cs             # Infor Visual read-only
│   ├── Dao_RoutingLabelHistory.cs       # Edit history
│   └── Dao_RoutingUsageTracking.cs      # Usage analytics
├── Models/
│   ├── Model_RoutingLabel.cs
│   ├── Model_RoutingRecipient.cs
│   └── Model_RoutingOtherReason.cs
├── Services/
│   ├── RoutingService.cs
│   ├── RoutingRecipientService.cs
│   ├── RoutingInforVisualService.cs
│   └── RoutingUsageTrackingService.cs
├── ViewModels/
│   ├── RoutingWizardContainerViewModel.cs
│   ├── RoutingWizardStep1ViewModel.cs
│   ├── RoutingWizardStep2ViewModel.cs
│   ├── RoutingWizardStep3ViewModel.cs
│   ├── RoutingEditModeViewModel.cs
│   └── RoutingManualEntryViewModel.cs
├── Views/
│   ├── RoutingWizardContainerView.xaml
│   ├── RoutingWizardStep1View.xaml
│   ├── RoutingWizardStep2View.xaml
│   ├── RoutingWizardStep3View.xaml
│   ├── RoutingEditModeView.xaml
│   └── RoutingManualEntryView.xaml
└── CODE_REVIEW.md                       # Automated code review findings
```

---

## Support & Maintenance

**Primary Developer**: GitHub Copilot AI + Human Review
**Documentation**: This README, CODE_REVIEW.md, inline XML comments
**Issue Tracking**: CODE_REVIEW.md (current findings)

**For Questions:**

- Review inline XML documentation in source files
- Check `specs/001-routing-module/` for original specifications
- See `.github/instructions/` for coding standards

---

**Last Updated**: 2026-01-06
**Module Version**: 2.0
**Build Status**: ✅ Passing
