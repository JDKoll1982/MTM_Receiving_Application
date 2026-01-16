# Complete Shipment Entry Workflow (New Shipment)

**User Story**: US1 - Volvo Shipment Entry with CQRS  
**Scenario**: User creates a new shipment from scratch, adds parts, and completes it

## End-to-End Flow

```mermaid
sequenceDiagram
    actor User
    participant View as View_Volvo_ShipmentEntry.xaml
    participant VM as ViewModel_Volvo_ShipmentEntry
    participant Med as IMediator
    participant InitH as GetInitialShipmentDataQueryHandler
    participant SearchH as SearchVolvoPartsQueryHandler
    participant AddH as AddPartToShipmentCommandHandler
    participant CompleteH as CompleteShipmentCommandHandler
    participant LabelH as GenerateLabelCsvQueryHandler
    participant DAO_Ship as Dao_VolvoShipment
    participant DAO_Line as Dao_VolvoShipmentLine
    participant DAO_Part as Dao_VolvoPart
    participant DB as MySQL Database

    Note over User,DB: Phase 1: Initialize New Shipment

    User->>View: Click "New Shipment"
    View->>VM: InitializeNewShipmentCommand
    VM->>Med: Send(GetInitialShipmentDataQuery)
    Med->>InitH: Handle(query)
    InitH->>DAO_Ship: GetNextShipmentNumberAsync()
    DAO_Ship->>DB: CALL sp_volvo_shipment_get_next_number()
    DB-->>DAO_Ship: ShipmentNumber: 1001
    DAO_Ship-->>InitH: Model_Dao_Result<int>(1001)
    InitH-->>Med: InitialShipmentData { NextNumber=1001, Date=Today }
    Med-->>VM: Result<InitialShipmentData>
    VM->>VM: CurrentShipmentNumber = 1001<br/>ShipmentDate = Today<br/>Parts.Clear()
    VM-->>View: Update bindings (ShipmentNumber, Date)
    View-->>User: Display "Shipment #1001" form

    Note over User,DB: Phase 2: Search & Add Parts (Repeat for each part)

    User->>View: Type "PART" in search box
    View->>VM: PartSearchText = "PART" (x:Bind TwoWay)
    VM->>Med: Send(SearchVolvoPartsQuery { SearchText="PART" })
    Med->>SearchH: Handle(query)
    SearchH->>DAO_Part: GetAllActivePartsAsync()
    DAO_Part->>DB: CALL sp_volvo_part_get_all_active()
    DB-->>DAO_Part: List<Model_VolvoPart>
    DAO_Part-->>SearchH: Model_Dao_Result<List<Model_VolvoPart>>
    SearchH->>SearchH: Filter by SearchText (LINQ)
    SearchH-->>Med: Filtered List<Model_VolvoPart>
    Med-->>VM: PartSuggestions collection
    VM-->>View: Update AutoSuggestBox dropdown
    View-->>User: Show matching parts

    User->>View: Select part, enter qty (5 skids)
    View->>VM: AddPartCommand (PartNumber, Qty=5)
    VM->>Med: Send(AddPartToShipmentCommand)
    
    Note over Med,AddH: FluentValidation checks:<br/>- PartNumber required<br/>- ReceivedSkidCount > 0<br/>- If HasDiscrepancy: ExpectedSkidCount required

    Med->>AddH: Handle(command) [after validation]
    AddH->>AddH: Create ShipmentLineDto
    AddH-->>Med: Model_Dao_Result<ShipmentLineDto>
    Med-->>VM: Success + LineDto
    VM->>VM: Parts.Add(lineDto)
    VM-->>View: Update DataGrid binding
    View-->>User: Part added to grid

    Note over User,DB: Phase 3: Complete Shipment

    User->>View: Enter PO Number, Receiver Number
    View->>VM: PONumber, ReceiverNumber (x:Bind TwoWay)
    
    User->>View: Click "Complete Shipment"
    View->>VM: CompleteShipmentCommand
    VM->>Med: Send(CompleteShipmentCommand)
    
    Note over Med,CompleteH: FluentValidation checks:<br/>- ShipmentDate <= Now<br/>- Parts.Count > 0<br/>- PONumber required<br/>- ReceiverNumber required

    Med->>CompleteH: Handle(command) [after validation]
    
    Note over CompleteH: Authorization Check
    CompleteH->>CompleteH: Check IService_VolvoAuthorization
    
    CompleteH->>DAO_Ship: InsertAsync(shipment)
    DAO_Ship->>DB: CALL sp_volvo_shipment_insert()
    DB-->>DAO_Ship: ShipmentId: 123
    DAO_Ship-->>CompleteH: Model_Dao_Result<(123, 1001)>
    
    loop For each part in command.Parts
        CompleteH->>DAO_Line: InsertAsync(line)
        DAO_Line->>DB: CALL sp_volvo_shipment_line_insert()
        DB-->>DAO_Line: LineId
        DAO_Line-->>CompleteH: Model_Dao_Result<int>
    end
    
    CompleteH->>DAO_Ship: CompleteAsync(123, PO, Receiver)
    DAO_Ship->>DB: CALL sp_volvo_shipment_complete()
    DB-->>DAO_Ship: Success
    DAO_Ship-->>CompleteH: Model_Dao_Result<bool>
    
    CompleteH-->>Med: Model_Dao_Result<int>(123)
    Med-->>VM: Success + ShipmentId=123

    Note over User,DB: Phase 4: Generate Labels (Auto-triggered)

    VM->>Med: Send(GenerateLabelCsvQuery { ShipmentId=123 })
    Med->>LabelH: Handle(query)
    LabelH->>DAO_Ship: GetByIdAsync(123)
    DAO_Ship->>DB: CALL sp_volvo_shipment_get_by_id(123)
    DB-->>DAO_Ship: Model_VolvoShipment
    DAO_Ship-->>LabelH: Shipment data
    
    LabelH->>DAO_Line: GetByShipmentIdAsync(123)
    DAO_Line->>DB: CALL sp_volvo_shipment_line_get_by_shipment(123)
    DB-->>DAO_Line: List<Model_VolvoShipmentLine>
    DAO_Line-->>LabelH: Lines with parts
    
    LabelH->>LabelH: Generate CSV format<br/>(functional parity with legacy)
    LabelH-->>Med: CsvContent (string)
    Med-->>VM: CSV string
    
    VM->>VM: Save CSV to file
    VM-->>View: StatusMessage = "Completed"
    View-->>User: Show success + CSV saved

    Note over User,DB: ✅ Workflow Complete<br/>Database: Shipment #1001 status='Completed'
```

## Key CQRS Components

### Queries Used

1. **GetInitialShipmentDataQuery** → Gets next shipment number for the day
2. **SearchVolvoPartsQuery** → Real-time part search for autocomplete
3. **GenerateLabelCsvQuery** → Produces label CSV file

### Commands Used  

1. **AddPartToShipmentCommand** → Adds part to in-memory collection (validation only)
2. **CompleteShipmentCommand** → Persists shipment, lines, marks complete, triggers label/email

### Validation Rules

**AddPartToShipmentCommand**:

- `PartNumber` required
- `ReceivedSkidCount > 0`
- If `HasDiscrepancy = true`: `ExpectedSkidCount` and `DiscrepancyNote` required

**CompleteShipmentCommand**:

- `ShipmentDate <= DateTime.Now`
- `Parts.Count > 0`
- `PONumber` required and not empty
- `ReceiverNumber` required and not empty
- User must have "Complete Shipments" authorization

### Database Operations

**Stored Procedures Called**:

1. `sp_volvo_shipment_get_next_number()` - Get next sequential number
2. `sp_volvo_part_get_all_active()` - Get active parts for search
3. `sp_volvo_shipment_insert()` - Insert shipment header
4. `sp_volvo_shipment_line_insert()` - Insert each line (looped)
5. `sp_volvo_shipment_complete()` - Update status, add PO/Receiver
6. `sp_volvo_shipment_get_by_id()` - Retrieve for label generation
7. `sp_volvo_shipment_line_get_by_shipment()` - Get lines for labels

### Success Criteria

✅ Shipment created with status='Completed'  
✅ All parts saved to `volvo_shipment_lines` table  
✅ CSV labels generated matching legacy format byte-for-byte  
✅ No direct ViewModel→DAO calls (all through IMediator)  
✅ FluentValidation enforces business rules  
✅ Authorization checked before completion
