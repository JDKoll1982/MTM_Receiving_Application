# Pending Shipment Save & Resume Workflow

**User Story**: US1 - Volvo Shipment Entry with CQRS  
**Scenario**: User starts a shipment, saves it as pending (no PO yet), then resumes it later to complete

## End-to-End Flow

```mermaid
sequenceDiagram
    actor User
    participant View as View_Volvo_ShipmentEntry.xaml
    participant VM as ViewModel_Volvo_ShipmentEntry
    participant Med as IMediator
    participant InitH as GetInitialShipmentDataQueryHandler
    participant PendH as GetPendingShipmentQueryHandler
    participant SaveH as SavePendingShipmentCommandHandler
    participant CompleteH as CompleteShipmentCommandHandler
    participant DAO_Ship as Dao_VolvoShipment
    participant DAO_Line as Dao_VolvoShipmentLine
    participant DB as MySQL Database

    Note over User,DB: Part 1: Create & Save Pending Shipment

    User->>View: Start new shipment
    View->>VM: InitializeNewShipmentCommand
    VM->>Med: Send(GetInitialShipmentDataQuery)
    Med->>InitH: Handle(query)
    InitH->>DAO_Ship: GetNextShipmentNumberAsync()
    DAO_Ship->>DB: CALL sp_volvo_shipment_get_next_number()
    DB-->>DAO_Ship: ShipmentNumber: 1001
    DAO_Ship-->>InitH: 1001
    InitH-->>VM: InitialShipmentData
    VM-->>View: Display form with #1001

    User->>View: Add parts (e.g., 3 parts)
    View->>VM: Multiple AddPartCommand calls
    VM->>VM: Parts collection grows to 3 items

    Note over User: User doesn't have PO number yet<br/>Needs to wait for purchasing dept

    User->>View: Click "Save as Pending"
    View->>VM: SaveAsPendingCommand
    VM->>Med: Send(SavePendingShipmentCommand)
    
    Note over Med,SaveH: FluentValidation checks:<br/>- ShipmentDate <= Now<br/>- Parts.Count > 0

    Med->>SaveH: Handle(command) [after validation]
    SaveH->>DAO_Ship: InsertAsync(shipment)
    DAO_Ship->>DB: CALL sp_volvo_shipment_insert()<br/>(Status='Pending')
    DB-->>DAO_Ship: ShipmentId: 123
    DAO_Ship-->>SaveH: (123, 1001)
    
    loop For each part in command.Parts
        SaveH->>DAO_Line: InsertAsync(line)
        DAO_Line->>DB: CALL sp_volvo_shipment_line_insert()
        DB-->>DAO_Line: LineId
        DAO_Line-->>SaveH: Success
    end
    
    SaveH-->>Med: Model_Dao_Result<int>(123)
    Med-->>VM: Success + ShipmentId=123
    VM->>VM: CurrentShipmentId = 123
    VM-->>View: StatusMessage = "Saved as pending"
    View-->>User: "Shipment saved, can resume later"

    Note over User,DB: ⏸️ User Logs Out / Closes App<br/>Database: Shipment #1001 status='Pending'

    Note over User,DB: Part 2: Resume Pending Shipment (Hours/Days Later)

    User->>View: Open Volvo Shipment Entry
    View->>VM: Constructor / Activated
    VM->>Med: Send(GetPendingShipmentQuery)
    Med->>PendH: Handle(query)
    PendH->>DAO_Ship: GetPendingAsync()
    DAO_Ship->>DB: CALL sp_volvo_shipment_get_pending()
    DB-->>DAO_Ship: Model_VolvoShipment (Id=123)
    DAO_Ship-->>PendH: Pending shipment
    
    PendH->>DAO_Line: GetByShipmentIdAsync(123)
    DAO_Line->>DB: CALL sp_volvo_shipment_line_get_by_shipment(123)
    DB-->>DAO_Line: List<Model_VolvoShipmentLine> (3 parts)
    DAO_Line-->>PendH: Lines
    PendH->>PendH: Populate shipment.Lines
    PendH-->>Med: Model_VolvoShipment with Lines
    Med-->>VM: Pending shipment data

    VM->>VM: Restore state:<br/>CurrentShipmentId = 123<br/>ShipmentNumber = 1001<br/>Parts.Clear()<br/>Parts.AddRange(lines)
    VM-->>View: Update all bindings
    View-->>User: Show "Resume Shipment #1001"<br/>with 3 parts in grid

    Note over User: User now has PO number<br/>from purchasing dept

    User->>View: Add PO Number, Receiver Number
    View->>VM: PONumber, ReceiverNumber (x:Bind)
    
    User->>View: Optional: Add more parts
    View->>VM: AddPartCommand
    VM->>VM: Parts.Add(new part)

    User->>View: Click "Complete Shipment"
    View->>VM: CompleteShipmentCommand
    VM->>Med: Send(CompleteShipmentCommand)
    Med->>CompleteH: Handle(command)
    
    CompleteH->>DAO_Ship: UpdateAsync(shipment) [if ShipmentId exists]
    DAO_Ship->>DB: CALL sp_volvo_shipment_update()<br/>(Status='Completed', add PO/Receiver)
    DB-->>DAO_Ship: Success
    DAO_Ship-->>CompleteH: Updated
    
    alt New parts added after resume
        loop For each NEW part
            CompleteH->>DAO_Line: InsertAsync(line)
            DAO_Line->>DB: CALL sp_volvo_shipment_line_insert()
            DB-->>DAO_Line: LineId
            DAO_Line-->>CompleteH: Success
        end
    end
    
    CompleteH->>DAO_Ship: CompleteAsync(123, PO, Receiver)
    DAO_Ship->>DB: CALL sp_volvo_shipment_complete()
    DB-->>DAO_Ship: Success
    DAO_Ship-->>CompleteH: Completed
    
    CompleteH-->>Med: Model_Dao_Result<int>(123)
    Med-->>VM: Success
    VM-->>View: StatusMessage = "Completed"
    View-->>User: "Shipment #1001 completed"

    Note over User,DB: ✅ Workflow Complete<br/>Database: Shipment #1001 status='Completed'
```

## Key CQRS Components

### Queries Used

1. **GetInitialShipmentDataQuery** → Gets next shipment number (new flow)
2. **GetPendingShipmentQuery** → Retrieves saved pending shipment (resume flow)

### Commands Used

1. **SavePendingShipmentCommand** → Saves incomplete shipment with status='Pending'
2. **CompleteShipmentCommand** → Updates pending shipment to 'Completed', adds PO/Receiver

### Validation Rules

**SavePendingShipmentCommand**:

- `ShipmentDate <= DateTime.Now`
- `Parts.Count > 0` (must have at least 1 part)
- NO requirement for PO/Receiver (that's the point of pending)

**CompleteShipmentCommand**:

- Same as regular completion
- If `ShipmentId` is provided, updates existing shipment instead of inserting new

### State Management

**ViewModel Tracks**:

- `CurrentShipmentId` (null for new, set after save/resume)
- `IsResumedShipment` (flag to determine Insert vs Update)
- `Parts` collection (in-memory until saved)

**Database States**:

1. **New Shipment**: No DB record, parts in VM memory only
2. **Pending**: `status='Pending'`, shipment + lines in DB, no PO/Receiver
3. **Completed**: `status='Completed'`, has PO/Receiver, immutable

### Database Operations

**Stored Procedures Called**:

**Save Flow**:

1. `sp_volvo_shipment_insert()` - Insert with status='Pending'
2. `sp_volvo_shipment_line_insert()` - Insert each line

**Resume Flow**:
3. `sp_volvo_shipment_get_pending()` - Get latest pending for user
4. `sp_volvo_shipment_line_get_by_shipment()` - Get associated lines

**Complete Flow**:
5. `sp_volvo_shipment_update()` - Update if resuming (optional)
6. `sp_volvo_shipment_line_insert()` - Insert any new lines added during resume
7. `sp_volvo_shipment_complete()` - Mark as completed, add PO/Receiver

### Business Rules

✅ **Only ONE pending shipment per user allowed** (enforced by `GetPendingAsync()`)  
✅ **Pending shipments can be edited** (add/remove parts before completion)  
✅ **Completed shipments are immutable** (cannot go back to pending)  
✅ **Resume happens automatically** on app launch (if pending exists)  
✅ **User can choose to discard pending** and start new instead

### Success Criteria

✅ Shipment saved mid-workflow without PO/Receiver  
✅ State restored perfectly after app restart  
✅ All parts preserved in database during pending state  
✅ Transition from Pending → Completed is atomic  
✅ No data loss during save/resume cycle
