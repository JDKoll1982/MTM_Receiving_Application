# Label Generation Workflow (CSV Export)

**User Story**: US1 - Volvo Shipment Entry with CQRS  
**Scenario**: After shipment completion, system auto-generates CSV labels for printing

## End-to-End Flow

```mermaid
sequenceDiagram
    actor User
    participant VM as ViewModel_Volvo_ShipmentEntry
    participant Med as IMediator
    participant LabelH as GenerateLabelCsvQueryHandler
    participant DAO_Ship as Dao_VolvoShipment
    participant DAO_Line as Dao_VolvoShipmentLine
    participant DAO_Part as Dao_VolvoPart
    participant DB as MySQL Database
    participant FS as File System
    participant Printer as Label Printer

    Note over User,Printer: Triggered automatically after CompleteShipmentCommand succeeds

    VM->>VM: CompleteShipmentCommand succeeded<br/>ShipmentId = 123
    VM->>Med: Send(GenerateLabelCsvQuery { ShipmentId = 123 })
    Med->>LabelH: Handle(query)

    Note over LabelH,DB: Step 1: Retrieve Shipment Header

    LabelH->>DAO_Ship: GetByIdAsync(123)
    DAO_Ship->>DB: CALL sp_volvo_shipment_get_by_id(123)
    DB-->>DAO_Ship: Model_VolvoShipment {<br/>  Id=123, ShipmentNumber=1001,<br/>  ShipmentDate=2026-01-16,<br/>  PONumber="PO-2026-001",<br/>  ReceiverNumber="RCV-123"<br/>}
    DAO_Ship-->>LabelH: Shipment header

    Note over LabelH,DB: Step 2: Retrieve Shipment Lines

    LabelH->>DAO_Line: GetByShipmentIdAsync(123)
    DAO_Line->>DB: CALL sp_volvo_shipment_line_get_by_shipment(123)
    DB-->>DAO_Line: List<Model_VolvoShipmentLine> [<br/>  { PartNumber="VOLVO-123", ReceivedSkidCount=5 },<br/>  { PartNumber="VOLVO-456", ReceivedSkidCount=3 },<br/>  { PartNumber="VOLVO-789", ReceivedSkidCount=2 }<br/>]
    DAO_Line-->>LabelH: Lines

    Note over LabelH,DB: Step 3: Enrich with Part Master Data

    loop For each shipment line
        LabelH->>DAO_Part: GetByPartNumberAsync("VOLVO-123")
        DAO_Part->>DB: CALL sp_volvo_part_get_by_number("VOLVO-123")
        DB-->>DAO_Part: Model_VolvoPart {<br/>  PartNumber="VOLVO-123",<br/>  QuantityPerSkid=100,<br/>  Components=[...]<br/>}
        DAO_Part-->>LabelH: Part details
        LabelH->>LabelH: Calculate:<br/>TotalPieces = ReceivedSkidCount * QuantityPerSkid<br/>TotalPieces = 5 * 100 = 500
    end

    Note over LabelH: Step 4: Component Explosion (Nested Parts)

    loop For each part with components
        LabelH->>LabelH: Explode components:<br/>If VOLVO-123 has 2 sub-parts,<br/>calculate quantities for each
    end

    Note over LabelH: Step 5: Generate CSV Format (Legacy Compatibility)

    LabelH->>LabelH: Build CSV string:<br/>Header: ShipmentDate, PO, Receiver<br/>Line format (per CSV spec):<br/>"2026-01-16","PO-2026-001","RCV-123","VOLVO-123",5,100,500<br/>"2026-01-16","PO-2026-001","RCV-123","VOLVO-456",3,200,600<br/>"2026-01-16","PO-2026-001","RCV-123","VOLVO-789",2,150,300

    Note over LabelH: ⚠️ CRITICAL: Byte-for-byte match with legacy format<br/>Verified by golden file test

    LabelH-->>Med: Model_Dao_Result<string> {<br/>  Success = true,<br/>  Data = "csvContent..."<br/>}
    Med-->>VM: CSV content string

    Note over VM,FS: Step 6: Save CSV to File System

    VM->>VM: Determine file path:<br/>C:\VolvoLabels\Shipment_1001_20260116.csv
    VM->>FS: File.WriteAllText(path, csvContent)
    FS-->>VM: File saved

    Note over VM,Printer: Step 7: Optional Auto-Print

    alt Auto-print enabled in settings
        VM->>Printer: Print CSV file to label printer
        Printer-->>VM: Print job queued
    else Manual print
        VM->>VM: Open file location in Explorer
    end

    VM->>VM: StatusMessage = "Labels generated: Shipment_1001_20260116.csv"
    VM-->>User: Show success notification

    Note over User,Printer: ✅ CSV Labels Generated<br/>File: C:\VolvoLabels\Shipment_1001_20260116.csv

    Note over User,Printer: Golden File Validation (Test T061)

    participant GoldenTest as LabelCsvGoldenFileTests
    GoldenTest->>GoldenTest: Load expected_label_basic.csv<br/>(captured from legacy system)
    GoldenTest->>Med: Send(GenerateLabelCsvQuery { ShipmentId = testId })
    Med-->>GoldenTest: Generated CSV content
    GoldenTest->>GoldenTest: Compare byte-for-byte:<br/>Assert.Equal(expected, actual)
    GoldenTest-->>GoldenTest: ✅ Test Passed: Functional Parity Verified
```

## CSV Format Specification

### Header Row (Optional)

```csv
ShipmentDate,PONumber,ReceiverNumber,PartNumber,SkidCount,QuantityPerSkid,TotalPieces,DiscrepancyNote
```

### Data Rows (Per Part)

```csv
"2026-01-16","PO-2026-001","RCV-123","VOLVO-123",5,100,500,""
"2026-01-16","PO-2026-001","RCV-123","VOLVO-456",3,200,600,"Missing 1 skid"
```

### Field Descriptions

| Field | Type | Source | Calculation |
|-------|------|--------|-------------|
| ShipmentDate | Date | Shipment header | ISO 8601 format |
| PONumber | String | Shipment header | From completing user input |
| ReceiverNumber | String | Shipment header | From completing user input |
| PartNumber | String | Shipment line | Direct from part entry |
| SkidCount | Integer | Shipment line | ReceivedSkidCount |
| QuantityPerSkid | Integer | Part master data | From `volvo_parts` table |
| TotalPieces | Integer | Calculated | SkidCount × QuantityPerSkid |
| DiscrepancyNote | String | Shipment line | Only if HasDiscrepancy=true |

### Component Explosion Logic

**Example**: Part `VOLVO-ASSY-001` is an assembly with 3 components

**Part Master Data**:

```
VOLVO-ASSY-001 (Assembly)
├── VOLVO-COMP-A (Qty: 2 per assembly)
├── VOLVO-COMP-B (Qty: 4 per assembly)
└── VOLVO-COMP-C (Qty: 1 per assembly)
```

**If received 5 skids of VOLVO-ASSY-001 @ 10 assemblies/skid**:

- Total assemblies: 5 × 10 = 50
- VOLVO-COMP-A pieces: 50 × 2 = 100
- VOLVO-COMP-B pieces: 50 × 4 = 200
- VOLVO-COMP-C pieces: 50 × 1 = 50

**CSV Output** (4 rows for 1 received part):

```csv
"2026-01-16","PO-123","RCV-456","VOLVO-ASSY-001",5,10,50,""
"2026-01-16","PO-123","RCV-456","VOLVO-COMP-A",0,0,100,"Component of VOLVO-ASSY-001"
"2026-01-16","PO-123","RCV-456","VOLVO-COMP-B",0,0,200,"Component of VOLVO-ASSY-001"
"2026-01-16","PO-123","RCV-456","VOLVO-COMP-C",0,0,50,"Component of VOLVO-ASSY-001"
```

## Key CQRS Components

### Query Used

**GenerateLabelCsvQuery**

- **Request**: `{ ShipmentId: int }`
- **Response**: `Model_Dao_Result<string>` (CSV content)
- **Handler**: `GenerateLabelCsvQueryHandler`
- **No Command**: Read-only operation, no database writes

### Handler Responsibilities

1. **Retrieve Data**:
   - Shipment header (PO, Receiver, Date)
   - Shipment lines (Parts, Quantities)
   - Part master data (QuantityPerSkid, Components)

2. **Calculate Totals**:
   - `TotalPieces = ReceivedSkidCount × QuantityPerSkid`
   - Recursive component explosion for assemblies

3. **Format CSV**:
   - Match legacy format EXACTLY (byte-for-byte)
   - Handle special characters (commas, quotes in notes)
   - Windows line endings (`\r\n`)

4. **Return String**:
   - No file I/O in handler (handler returns string)
   - ViewModel responsible for saving to disk

### Functional Parity Requirements

**Legacy System Calculation**:

```csharp
// Old code (pre-CQRS)
foreach (var part in parts)
{
    var totalPieces = part.ReceivedSkidCount * part.QuantityPerSkid;
    csvLines.Add($"{date},{po},{receiver},{part.PartNumber},{part.ReceivedSkidCount},{part.QuantityPerSkid},{totalPieces}");
}
```

**CQRS Handler Must Match**:

```csharp
// New code (CQRS)
public async Task<Model_Dao_Result<string>> Handle(GenerateLabelCsvQuery request, CancellationToken cancellationToken)
{
    // Same calculation logic
    // Same CSV format
    // Verified by golden file test
}
```

### Golden File Testing (T061)

**Purpose**: Ensure 100% functional parity with legacy label generation

**Test Setup**:

1. Capture CSV from legacy system for known shipment
2. Save as `Module_Volvo.Tests/GoldenFiles/expected_label_basic.csv`
3. Create same shipment data in test database
4. Call `GenerateLabelCsvQuery` with test shipment ID
5. Compare byte-for-byte with golden file

**Verification Code**:

```csharp
[Fact]
public async Task GenerateLabelCsvQuery_ProducesByteForByteMatch_WithExpectedLabelBasicCsv()
{
    // Arrange
    var expectedCsv = File.ReadAllText("GoldenFiles/expected_label_basic.csv");
    var query = new GenerateLabelCsvQuery { ShipmentId = TestShipmentId };
    
    // Act
    var result = await _mediator.Send(query);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Data.Should().Be(expectedCsv); // Exact string match
}
```

### File System Conventions

**File Path Pattern**:

```
C:\VolvoLabels\Shipment_{ShipmentNumber}_{YYYYMMDD}.csv
```

**Examples**:

- `C:\VolvoLabels\Shipment_1001_20260116.csv`
- `C:\VolvoLabels\Shipment_1002_20260116.csv`

**Directory Creation**:

- Auto-create `C:\VolvoLabels\` if doesn't exist
- Configurable via `appsettings.json`:

  ```json
  {
    "Volvo": {
      "LabelOutputPath": "C:\\VolvoLabels"
    }
  }
  ```

### Error Handling

**Potential Failures**:

1. Shipment not found → Return failure result
2. No lines for shipment → Return failure (shouldn't happen if validation works)
3. Part master data missing → Return failure (data integrity issue)
4. File write permission denied → Show error to user, log event

**Handler Never Throws**:

```csharp
catch (Exception ex)
{
    return Model_Dao_Result_Factory.Failure<string>(
        $"Error generating labels: {ex.Message}", ex);
}
```

### Success Criteria

✅ CSV format matches legacy byte-for-byte (golden file test passes)  
✅ Component explosion calculations match legacy system  
✅ Discrepancy notes appear correctly in CSV  
✅ File saved to configured directory  
✅ Handler is pure query (no side effects)  
✅ ViewModel handles file I/O (separation of concerns)  
✅ Multi-skid calculations accurate  
✅ Special characters escaped properly in CSV
