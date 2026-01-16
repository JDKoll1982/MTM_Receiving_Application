# Part Search & Add Workflow (Real-time)

**User Story**: US1 - Volvo Shipment Entry with CQRS  
**Scenario**: User types in part search box, gets autocomplete suggestions, selects a part to add

## End-to-End Flow

```mermaid
sequenceDiagram
    actor User
    participant View as View_Volvo_ShipmentEntry.xaml
    participant ASB as AutoSuggestBox
    participant VM as ViewModel_Volvo_ShipmentEntry
    participant Med as IMediator
    participant SearchH as SearchVolvoPartsQueryHandler
    participant AddH as AddPartToShipmentCommandHandler
    participant ValB as ValidationBehavior<br/>(FluentValidation)
    participant Validator as AddPartToShipmentCommandValidator
    participant DAO_Part as Dao_VolvoPart
    participant DB as MySQL Database

    Note over User,DB: Phase 1: Real-time Part Search (Autocomplete)

    User->>ASB: Type "VOL" in search box
    ASB->>VM: PartSearchText = "VOL"<br/>(x:Bind TwoWay, UpdateSourceTrigger=PropertyChanged)
    
    Note over VM: Debounce / Throttle<br/>(optional performance optimization)
    
    VM->>Med: Send(SearchVolvoPartsQuery { SearchText = "VOL" })
    Med->>SearchH: Handle(query)
    SearchH->>DAO_Part: GetAllActivePartsAsync()
    DAO_Part->>DB: CALL sp_volvo_part_get_all_active()
    DB-->>DAO_Part: List<Model_VolvoPart> (all active parts)
    DAO_Part-->>SearchH: Model_Dao_Result<List<Model_VolvoPart>>
    
    SearchH->>SearchH: Filter in-memory:<br/>parts.Where(p => p.PartNumber.Contains("VOL")<br/>|| p.Description?.Contains("VOL"))
    SearchH-->>Med: Filtered List (e.g., 3 matches)
    Med-->>VM: PartSuggestions collection
    
    VM->>VM: PartSuggestions.Clear()<br/>PartSuggestions.AddRange(filtered)
    VM-->>ASB: ItemsSource updated
    ASB-->>User: Show dropdown with 3 matches:<br/>- VOLVO-123<br/>- VOLVO-456<br/>- VOLVO-789

    User->>ASB: Continue typing "VOL12"
    ASB->>VM: PartSearchText = "VOL12"
    VM->>Med: Send(SearchVolvoPartsQuery { "VOL12" })
    Med->>SearchH: Handle(query)
    SearchH->>SearchH: Filter (same parts, narrower)
    SearchH-->>VM: 1 match: VOLVO-123
    VM-->>ASB: Update dropdown
    ASB-->>User: Show 1 match

    Note over User,DB: Phase 2: Select Part from Suggestions

    User->>ASB: Click "VOLVO-123" from dropdown
    ASB->>VM: SelectedPart = Model_VolvoPart<br/>(PartNumber="VOLVO-123")
    VM->>VM: Auto-populate form fields:<br/>PartNumber = "VOLVO-123"<br/>QuantityPerSkid = part.QuantityPerSkid<br/>Focus on ReceivedSkidCount input

    Note over User,DB: Phase 3: Enter Quantity & Add Part

    User->>View: Enter ReceivedSkidCount = 5
    View->>VM: ReceivedSkidCount = 5 (x:Bind TwoWay)
    
    alt User reports discrepancy
        User->>View: Check "Has Discrepancy" checkbox
        View->>VM: HasDiscrepancy = true
        VM-->>View: Enable ExpectedSkidCount + DiscrepancyNote fields
        
        User->>View: Enter ExpectedSkidCount = 6
        View->>VM: ExpectedSkidCount = 6
        User->>View: Enter DiscrepancyNote = "Missing 1 skid"
        View->>VM: DiscrepancyNote = "Missing 1 skid"
    else No discrepancy
        Note over User,VM: ExpectedSkidCount = ReceivedSkidCount<br/>HasDiscrepancy = false
    end

    User->>View: Click "Add Part" button
    View->>VM: AddPartCommand.Execute()
    VM->>VM: Create command:<br/>AddPartToShipmentCommand {<br/>  PartNumber = "VOLVO-123",<br/>  ReceivedSkidCount = 5,<br/>  ExpectedSkidCount = 6,<br/>  HasDiscrepancy = true,<br/>  DiscrepancyNote = "Missing 1 skid"<br/>}
    
    VM->>Med: Send(AddPartToShipmentCommand)
    
    Note over Med,ValB: MediatR Pipeline Behavior:<br/>ValidationBehavior intercepts

    Med->>ValB: Pipeline.Handle(command)
    ValB->>Validator: ValidateAsync(command)
    
    Validator->>Validator: Check rules:<br/>✓ PartNumber.NotEmpty()<br/>✓ ReceivedSkidCount > 0<br/>✓ HasDiscrepancy = true<br/>  → ExpectedSkidCount > 0 ✓<br/>  → DiscrepancyNote.NotEmpty() ✓
    
    alt Validation Passes
        Validator-->>ValB: ValidationResult { IsValid = true }
        ValB->>AddH: Continue to handler
        AddH->>AddH: Create ShipmentLineDto from command
        AddH-->>Med: Model_Dao_Result<ShipmentLineDto>
        Med-->>VM: Success + ShipmentLineDto
        
        VM->>VM: Parts.Add(new ShipmentLineDto {<br/>  PartNumber = "VOLVO-123",<br/>  ReceivedSkidCount = 5,<br/>  ExpectedSkidCount = 6,<br/>  HasDiscrepancy = true,<br/>  DiscrepancyNote = "Missing 1 skid",<br/>  QuantityPerSkid = 100,<br/>  TotalPieces = 500<br/>})
        
        VM->>VM: Clear form:<br/>PartSearchText = ""<br/>ReceivedSkidCount = 0<br/>HasDiscrepancy = false<br/>DiscrepancyNote = ""
        
        VM-->>View: Update DataGrid (Parts collection)<br/>StatusMessage = "Part added"
        View-->>User: Part appears in grid<br/>Form cleared for next part
    else Validation Fails
        Validator-->>ValB: ValidationResult {<br/>  IsValid = false,<br/>  Errors = ["Part number required"]<br/>}
        ValB-->>Med: Throw ValidationException
        Med-->>VM: Catch exception
        VM->>VM: ErrorHandler.ShowUserError(<br/>  "Validation failed: Part number required"<br/>)
        VM-->>View: Show error InfoBar
        View-->>User: Red error message
    end

    Note over User,DB: ✅ Part Added to In-Memory Collection<br/>Ready to add more parts or complete shipment

    Note over User,DB: Repeat Phase 1-3 for each additional part
```

## Key CQRS Components

### Queries Used

**SearchVolvoPartsQuery**

- **Request**: `{ SearchText: string }`
- **Response**: `Model_Dao_Result<List<Model_VolvoPart>>`
- **Handler**: `SearchVolvoPartsQueryHandler`
- **DAO**: `Dao_VolvoPart.GetAllActivePartsAsync()`
- **Filter**: Client-side LINQ after fetching all active parts

### Commands Used

**AddPartToShipmentCommand**

- **Request**: `{ PartNumber, ReceivedSkidCount, ExpectedSkidCount?, HasDiscrepancy, DiscrepancyNote? }`
- **Response**: `Model_Dao_Result<ShipmentLineDto>`
- **Handler**: `AddPartToShipmentCommandHandler`
- **Validator**: `AddPartToShipmentCommandValidator`
- **Effect**: Creates DTO for in-memory collection (no DB write until Complete/Save)

### Validation Rules (AddPartToShipmentCommandValidator)

```csharp
RuleFor(x => x.PartNumber)
    .NotEmpty().WithMessage("Part number is required");

RuleFor(x => x.ReceivedSkidCount)
    .GreaterThan(0).WithMessage("Received skid count must be greater than 0");

When(x => x.HasDiscrepancy, () => 
{
    RuleFor(x => x.ExpectedSkidCount)
        .GreaterThan(0).WithMessage("Expected skid count required for discrepancies");
    
    RuleFor(x => x.DiscrepancyNote)
        .NotEmpty().WithMessage("Discrepancy note required when reporting discrepancy");
});
```

### Performance Optimization Strategies

**Option 1: Debouncing** (Current Recommendation)

- Wait 300ms after user stops typing before sending query
- Prevents excessive queries while typing fast

**Option 2: Caching**

- Cache `GetAllActivePartsAsync()` result for 5 minutes
- Filter cached list client-side for instant suggestions
- Trade-off: Stale data vs performance

**Option 3: Server-side Search**

- Modify DAO to accept search parameter
- Database executes `WHERE part_number LIKE '%{search}%'`
- Better for large datasets (1000+ parts)

**Current Implementation**: Option 1 (Debouncing recommended in ViewModel)

### UI/UX Patterns

**AutoSuggestBox Configuration**:

```xml
<AutoSuggestBox
    Text="{x:Bind ViewModel.PartSearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
    ItemsSource="{x:Bind ViewModel.PartSuggestions, Mode=OneWay}"
    TextMemberPath="PartNumber"
    PlaceholderText="Search part number..."
    QuerySubmitted="AutoSuggestBox_QuerySubmitted" />
```

**DataGrid Binding**:

```xml
<DataGrid ItemsSource="{x:Bind ViewModel.Parts, Mode=OneWay}">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Part" Binding="{Binding PartNumber}" />
        <DataGridTextColumn Header="Received" Binding="{Binding ReceivedSkidCount}" />
        <DataGridTextColumn Header="Expected" Binding="{Binding ExpectedSkidCount}" />
        <DataGridTemplateColumn Header="Discrepancy">
            <DataTemplate>
                <TextBlock Text="{Binding DiscrepancyNote}" Foreground="Red" 
                           Visibility="{Binding HasDiscrepancy}" />
            </DataTemplate>
        </DataGridTemplateColumn>
    </DataGrid.Columns>
</DataGrid>
```

### State Management

**In-Memory (ViewModel)**:

- `Parts` collection grows as user adds parts
- No database writes until "Complete" or "Save Pending"
- Can remove parts before saving (`RemovePartFromShipmentCommand`)

**Benefits**:

- Fast user experience (no DB round-trips per add)
- Easy to undo/edit before committing
- Atomic save on completion

### Success Criteria

✅ Search results appear < 300ms after typing stops  
✅ Autocomplete shows relevant parts as user types  
✅ Validation prevents invalid part additions  
✅ Discrepancy fields required only when checkbox checked  
✅ DataGrid updates immediately after add  
✅ Parts stored in memory until explicit save/complete  
✅ No direct ViewModel→DAO calls (all through IMediator)
