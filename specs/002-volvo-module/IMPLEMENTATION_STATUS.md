# Volvo Module Implementation - Status & Next Steps

**Date**: 2026-01-04  
**Branch**: `copilot/implement-volvo-module-specs`  
**Status**: Foundation Complete - Ready for Service Layer Implementation

---

## ✅ Completed Work Summary

### Phase 1: Database Foundation (100% Complete)
**Created 27 Database Files**:
- 1 schema file with 4 tables + 1 view
- 16 stored procedures
- 1 sample data file (13 parts, 10 component relationships)

**Key Database Design Decisions**:
1. **Historical Integrity**: `calculated_piece_count` stored at shipment creation time (never recalculated)
2. **Single Pending PO**: Only one shipment can have `status='pending_po'` at a time (enforced in application logic)
3. **Discrepancy Tracking**: Optional fields in `volvo_shipment_lines` for packlist vs received comparison
4. **Component Explosion**: Recursive relationship via `volvo_part_components` table

### Phase 2: Code Foundation (100% Complete)
**Created 9 C# Files**:
- 4 Models (`Module_Volvo/Models/`)
- 4 DAOs (`Module_Volvo/Data/`)
- 1 DI registration update (`App.xaml.cs`)

**Code Quality Metrics**:
- ✅ All DAOs instance-based (not static)
- ✅ All methods async with `Model_Dao_Result<T>` return types
- ✅ No exceptions thrown (error results returned)
- ✅ DI registration as singletons
- ✅ Constitution-compliant (MVVM, stored procedures only, MySQL 5.7.24 compatible)

---

## 🚀 Next Phase: Service Layer (Phase 3 - User Story 1)

### Task T015-T017: Create Core Volvo Service

**File**: `Module_Core/Contracts/Services/IService_Volvo.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Service interface for Volvo dunnage requisition business logic
/// </summary>
public interface IService_Volvo
{
    /// <summary>
    /// Calculates component explosion and aggregates piece counts for all parts in shipment
    /// </summary>
    /// <param name="lines">List of shipment lines with part numbers and skid counts</param>
    /// <returns>Dictionary of part numbers to total piece counts (includes parent parts + components)</returns>
    Task<Model_Dao_Result<Dictionary<string, int>>> CalculateComponentExplosionAsync(
        List<Model_VolvoShipmentLine> lines);

    /// <summary>
    /// Generates CSV file for LabelView 2022 label printing
    /// </summary>
    /// <param name="shipmentId">Shipment ID to generate labels for</param>
    /// <returns>File path where CSV was written</returns>
    Task<Model_Dao_Result<string>> GenerateLabelCsvAsync(int shipmentId);

    /// <summary>
    /// Formats email text for PO requisition (with discrepancy notice if applicable)
    /// </summary>
    /// <param name="shipment">Shipment header</param>
    /// <param name="lines">Shipment lines with discrepancy data</param>
    /// <param name="requestedLines">Aggregated component explosion results</param>
    /// <returns>Formatted email text ready to copy to Outlook</returns>
    Task<string> FormatEmailTextAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines,
        Dictionary<string, int> requestedLines);

    /// <summary>
    /// Saves shipment and lines with status='pending_po'
    /// </summary>
    Task<Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>> SaveShipmentAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines);

    /// <summary>
    /// Gets pending shipment if one exists
    /// </summary>
    Task<Model_Dao_Result<Model_VolvoShipment?>> GetPendingShipmentAsync();

    /// <summary>
    /// Completes shipment with PO and Receiver numbers
    /// </summary>
    Task<Model_Dao_Result> CompleteShipmentAsync(int shipmentId, string poNumber, string receiverNumber);

    /// <summary>
    /// Gets all active Volvo parts for dropdown population
    /// </summary>
    Task<Model_Dao_Result<List<Model_VolvoPart>>> GetActivePartsAsync();
}
```

**File**: `Module_Volvo/Services/Service_Volvo.cs`

**Key Implementation Details**:

1. **Component Explosion Algorithm**:
   ```csharp
   // For each line in shipment:
   //   1. Add parent part: partNumber × skidCount × qtyPerSkid = pieces
   //   2. Query components for parent part
   //   3. For each component:
   //      - Add component: componentPartNumber × skidCount × componentQty = pieces
   //   4. Aggregate duplicates across all lines
   ```

2. **Email Formatting**:
   ```
   Subject: PO Requisition - Volvo Dunnage - [Date] Shipment #[N]
   
   [Greeting line - user editable]
   
   [IF discrepancies exist]
   Discrepancies:
   Part Number | Packlist | Received | Difference
   V-EMB-21    | 12       | 10       | -2
   [END IF]
   
   Requested Lines:
   Part Number | Quantity (pcs)
   V-EMB-500   | 264
   V-EMB-2     | 3
   ...
   
   [Signature from settings]
   ```

3. **CSV Generation** (LabelView 2022 format):
   ```csv
   Material,Quantity,Employee,Date,Time,Receiver,Notes
   V-EMB-500,264,6229,01/04/2026,14:30:00,,
   V-EMB-2,3,6229,01/04/2026,14:30:00,,
   ```
   - Save to: `%APPDATA%\MTM_Receiving_Application\Volvo\Labels\Shipment_[ID]_[Date].csv`
   - Hide PO field for parts starting with "V-EMB-" (per spec)

**DI Registration** in `App.xaml.cs`:
```csharp
// Add after Dunnage services
services.AddSingleton<IService_Volvo>(sp =>
{
    var shipmentDao = sp.GetRequiredService<Dao_VolvoShipment>();
    var lineDao = sp.GetRequiredService<Dao_VolvoShipmentLine>();
    var partDao = sp.GetRequiredService<Dao_VolvoPart>();
    var componentDao = sp.GetRequiredService<Dao_VolvoPartComponent>();
    var logger = sp.GetRequiredService<IService_LoggingUtility>();
    var errorHandler = sp.GetRequiredService<IService_ErrorHandler>();
    
    return new Service_Volvo(shipmentDao, lineDao, partDao, componentDao, logger, errorHandler);
});
```

---

## 📝 Implementation Checklist for Phase 3

- [ ] **T015**: Create `IService_Volvo` interface
- [ ] **T016**: Implement `Service_Volvo` with component explosion algorithm
- [ ] **T017**: Register service in DI container
- [ ] **T018**: Create `VolvoShipmentEntryViewModel` (inherit from `BaseViewModel`)
  - [ ] Properties: `ObservableCollection<Model_VolvoShipmentLine> Lines`
  - [ ] Properties: `DateTime ShipmentDate`, `int ShipmentNumber`, `string Notes`
  - [ ] Command: `AddPartCommand` (adds empty line to collection)
  - [ ] Command: `RemovePartCommand` (removes selected line)
  - [ ] Command: `GenerateLabelsCommand` (calls service, shows success)
  - [ ] Command: `PreviewEmailCommand` (opens ContentDialog)
  - [ ] Command: `SaveAsPendingCommand` (validates, saves, navigates)
- [ ] **T019-T020**: Create `VolvoShipmentEntryView.xaml` + code-behind
  - [ ] Date/Shipment# header (auto-filled)
  - [ ] DataGrid with columns: Part (ComboBox), Skids (TextBox), Discrepancy (CheckBox)
  - [ ] Expandable discrepancy fields when checkbox checked
  - [ ] CommandBar: Generate Labels, Preview Email, Save as Pending
- [ ] **T021-T026**: Implement ViewModel commands
- [ ] **T027**: Add navigation menu item in `MainWindow.xaml`
- [ ] **T028**: Register ViewModel in DI as Transient

---

## 🧪 Testing Strategy (After Phase 3 Complete)

**Manual Test Scenario** (User Story 1):
1. Open Volvo module → New shipment screen loads
2. Date defaults to today, Shipment# auto-generates as "1"
3. Click "Add Part" → Empty row appears in DataGrid
4. Select "V-EMB-500" from dropdown → Enter 3 skids
5. Click "Add Part" → Select "V-EMB-750" → Enter 2 skids
6. Check discrepancy on V-EMB-750 → Enter Packlist: 5, Received: 2, Note: "Damaged skids"
7. Click "Generate Labels" → CSV file created at `%APPDATA%\...\Volvo\Labels\...`
8. Verify CSV contains:
   - V-EMB-500: 264 pcs (3 × 88)
   - V-EMB-2: 3 pcs (component)
   - V-EMB-92: 3 pcs (component)
   - V-EMB-750: 160 pcs (2 × 80)
   - V-EMB-1, V-EMB-6, V-EMB-71 (components)
9. Click "Preview Email" → ContentDialog shows formatted email with discrepancy table
10. Click "Copy to Clipboard" → Paste into Outlook → Verify formatting
11. Click "Save as Pending PO" → Success message → Record saved with status='pending_po'

**Expected Database State**:
```sql
-- Verify shipment inserted
SELECT * FROM volvo_shipments WHERE status='pending_po';

-- Verify lines inserted
SELECT * FROM volvo_shipment_lines WHERE shipment_id = [ID];

-- Verify CSV file exists
-- Check %APPDATA%\MTM_Receiving_Application\Volvo\Labels\
```

---

## 🗂️ File Structure After Phase 3

```
Module_Volvo/
├── Models/                    [✅ Complete]
│   ├── Model_VolvoShipment.cs
│   ├── Model_VolvoShipmentLine.cs
│   ├── Model_VolvoPart.cs
│   └── Model_VolvoPartComponent.cs
├── Data/                      [✅ Complete]
│   ├── Dao_VolvoShipment.cs
│   ├── Dao_VolvoShipmentLine.cs
│   ├── Dao_VolvoPart.cs
│   └── Dao_VolvoPartComponent.cs
├── Services/                  [⏳ To Create]
│   └── Service_Volvo.cs
├── ViewModels/                [⏳ To Create]
│   └── VolvoShipmentEntryViewModel.cs
└── Views/                     [⏳ To Create]
    ├── VolvoShipmentEntryView.xaml
    ├── VolvoShipmentEntryView.xaml.cs
    └── VolvoEmailPreviewDialog.xaml (+ .cs)

Module_Core/Contracts/Services/ [⏳ To Create]
└── IService_Volvo.cs

Database/                      [✅ Complete]
├── Schemas/
│   └── schema_volvo.sql
├── StoredProcedures/Volvo/
│   └── [16 stored procedures]
└── TestData/
    └── volvo_sample_data.sql
```

---

## 📚 Reference Documentation

**Key Spec Files**:
- `specs/002-volvo-module/spec.md` - User stories and requirements
- `specs/002-volvo-module/plan.md` - Technical architecture
- `specs/002-volvo-module/data-model.md` - Database design
- `specs/002-volvo-module/tasks.md` - Full task breakdown

**Architecture Guidelines**:
- `.github/instructions/mvvm-viewmodels.instructions.md`
- `.github/instructions/mvvm-views.instructions.md`
- `.github/instructions/dao-pattern.instructions.md`
- `.specify/memory/constitution.md` - MTM project constitution

---

## 🎯 Success Criteria for Phase 3

**Definition of Done**:
1. User can enter Volvo shipment with multiple parts
2. Component explosion calculates correctly
3. Labels CSV file generated in LabelView 2022 format
4. Email text formatted with discrepancy notice (if applicable)
5. Shipment saved as `pending_po` status
6. Only one pending shipment allowed (validated in ViewModel)
7. All code follows MVVM pattern (no business logic in code-behind)
8. All operations use DI (no static service access)
9. Error handling via `IService_ErrorHandler`
10. Logging via `IService_LoggingUtility`

**Timeline Estimate**: 4-6 hours for experienced WinUI 3 developer

---

## 🔄 Workflow for Continuing Implementation

1. **Pull latest changes**:
   ```bash
   git checkout copilot/implement-volvo-module-specs
   git pull origin copilot/implement-volvo-module-specs
   ```

2. **Create IService_Volvo interface** (Task T015)

3. **Implement Service_Volvo** (Task T016)
   - Start with `GetActivePartsAsync()` (simple DAO wrapper)
   - Then `CalculateComponentExplosionAsync()` (core algorithm)
   - Then `SaveShipmentAsync()` (transactional save)
   - Then `GenerateLabelCsvAsync()` (CSV writing)
   - Then `FormatEmailTextAsync()` (string formatting)

4. **Create ViewModel** (Task T018)
   - Inherit from `BaseViewModel`
   - Use `[ObservableProperty]` for bindable properties
   - Use `[RelayCommand]` for commands

5. **Create View** (Task T019-T020)
   - Use `x:Bind` for all data binding (not `Binding`)
   - `Mode=TwoWay` for user input controls
   - `Mode=OneWay` for read-only display

6. **Test thoroughly** before moving to Phase 4

---

**Last Updated**: 2026-01-04  
**Next Review**: After Phase 3 complete
