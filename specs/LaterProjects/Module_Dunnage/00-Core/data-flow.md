# Module_Dunnage - Data Flow

**Category**: Core Specification  
**Last Updated**: 2026-01-25  
**Related Documents**: [Purpose and Overview](./purpose-and-overview.md), [Workflow Modes](../../02-Workflow-Modes/)

---

## Overview

This document details the complete data flow through Module_Dunnage, from user input through processing, validation, persistence, and CSV export. The module supports three primary workflow modes (Guided, Manual, Edit) plus an Admin mode for configuration.

---

## Guided Mode Data Flow

### Complete Transaction Flow

```mermaid
flowchart TD
    Start[User Starts Workflow] --> ModeCheck{Default Mode Set?}
    ModeCheck -->|Yes| SkipToMode[Go to Default Mode]
    ModeCheck -->|No| ModeSelect[Mode Selection Screen]
    SkipToMode --> GuidedStart
    ModeSelect -->|Select Guided| GuidedStart[Guided Mode Start]
    
    GuidedStart --> Step1[Step 1: Type Selection]
    Step1 --> ValidateType{Type Selected?}
    ValidateType -->|No| Step1
    ValidateType -->|Yes| LoadTypeSpecs[Load Type Specifications]
    
    LoadTypeSpecs --> Step2[Step 2: Part Selection]
    Step2 --> ValidatePart{Part Selected?}
    ValidatePart -->|No| Step2
    ValidatePart -->|Yes| Step3[Step 3: Quantity Entry]
    
    Step3 --> ValidateQty{Quantity > 0?}
    ValidateQty -->|No| Step3
    ValidateQty -->|Yes| Step4[Step 4: Details Entry]
    
    Step4 --> RenderSpecs[Render Dynamic Spec Fields]
    RenderSpecs --> FillSpecs[User Fills Spec Values]
    FillSpecs --> ValidateSpecs{Required Fields Filled?}
    ValidateSpecs -->|No| Step4
    ValidateSpecs -->|Yes| AddToSession[Add Load to Session]
    
    AddToSession --> Step5[Step 5: Review]
    Step5 --> ReviewChoice{User Action?}
    ReviewChoice -->|Add More| Step1
    ReviewChoice -->|Edit Load| EditLoad[Edit Existing Load]
    EditLoad --> Step5
    ReviewChoice -->|Delete Load| DeleteLoad[Remove Load]
    DeleteLoad --> Step5
    ReviewChoice -->|Save & Export| ValidateAll{All Loads Valid?}
    
    ValidateAll -->|No| ShowErrors[Show Validation Errors]
    ShowErrors --> Step5
    ValidateAll -->|Yes| SaveToDB[(Save to Database)]
    
    SaveToDB --> DBSuccess{Save Success?}
    DBSuccess -->|No| DBError[Show Error, Retry]
    DBError --> Step5
    DBSuccess -->|Yes| ExportCSV[Generate CSV]
    
    ExportCSV --> ExportLocal[Export to Local Path]
    ExportLocal --> LocalSuccess{Local Success?}
    LocalSuccess -->|No| CriticalError[CRITICAL ERROR]
    LocalSuccess -->|Yes| ExportNetwork[Export to Network Path]
    
    ExportNetwork --> NetworkSuccess{Network Success?}
    NetworkSuccess -->|No| WarnUser[Warn: Network Failed]
    NetworkSuccess -->|Yes| AllSuccess[All Exports Success]
    
    WarnUser --> Complete[Transaction Complete]
    AllSuccess --> Complete
    Complete --> ClearSession[Clear Session]
    ClearSession --> End[Return to Mode Selection]
```

---

## Manual Entry Mode Data Flow

### Grid-Based Entry Flow

```mermaid
flowchart TD
    Start[Manual Entry Mode Start] --> InitGrid[Initialize Empty Grid]
    InitGrid --> GridReady[Grid Ready for Input]
    
    GridReady --> UserInput[User Enters Row Data]
    UserInput --> ValidateRow{Row Valid?}
    ValidateRow -->|No| ShowRowError[Highlight Invalid Cells]
    ShowRowError --> UserInput
    ValidateRow -->|Yes| NextRow{Add Another?}
    
    NextRow -->|Yes| UserInput
    NextRow -->|No| ReviewGrid[Review All Rows]
    
    ReviewGrid --> BulkActions{Bulk Operations?}
    BulkActions -->|Add Multiple| AddMultipleDialog[Add Multiple Rows Dialog]
    AddMultipleDialog --> UserInput
    BulkActions -->|Delete Selected| DeleteRows[Remove Selected Rows]
    DeleteRows --> ReviewGrid
    BulkActions -->|Save| ValidateAll{All Rows Valid?}
    
    ValidateAll -->|No| ShowErrors[Highlight All Errors]
    ShowErrors --> ReviewGrid
    ValidateAll -->|Yes| SaveBatch[(Batch Save to DB)]
    
    SaveBatch --> BatchSuccess{Save Success?}
    BatchSuccess -->|No| SaveError[Show Errors, Retry]
    SaveError --> ReviewGrid
    BatchSuccess -->|Yes| ExportCSV[Generate CSV]
    
    ExportCSV --> ExportPaths[Export Local & Network]
    ExportPaths --> Complete[Transaction Complete]
    Complete --> End[Return to Mode Selection]
```

---

## Edit Mode Data Flow

### Historical Transaction Modification Flow

```mermaid
flowchart TD
    Start[Edit Mode Start] --> SearchUI[Display Search Interface]
    SearchUI --> EnterFilters[User Enters Search Filters]
    EnterFilters --> ExecuteSearch[(Query Database)]
    
    ExecuteSearch --> Results{Results Found?}
    Results -->|No| NoResults[Show No Results]
    NoResults --> SearchUI
    Results -->|Yes| DisplayResults[Display Transaction List]
    
    DisplayResults --> SelectTxn[User Selects Transaction]
    SelectTxn --> LoadTxn[(Load Transaction Data)]
    LoadTxn --> DisplayLoads[Display Loads Grid]
    
    DisplayLoads --> EditChoice{User Action?}
    EditChoice -->|Edit Load| ModifyLoad[Modify Load Data]
    ModifyLoad --> ValidateEdit{Valid?}
    ValidateEdit -->|No| ShowError[Show Validation Error]
    ShowError --> ModifyLoad
    ValidateEdit -->|Yes| MarkDirty[Mark Load as Modified]
    MarkDirty --> DisplayLoads
    
    EditChoice -->|Delete Load| ConfirmDelete{Confirm?}
    ConfirmDelete -->|No| DisplayLoads
    ConfirmDelete -->|Yes| RemoveLoad[Remove Load]
    RemoveLoad --> DisplayLoads
    
    EditChoice -->|Add Load| AddNewLoad[Add New Load to Transaction]
    AddNewLoad --> DisplayLoads
    
    EditChoice -->|Save Changes| ValidateAll{All Changes Valid?}
    ValidateAll -->|No| ShowAllErrors[Show Validation Errors]
    ShowAllErrors --> DisplayLoads
    ValidateAll -->|Yes| SaveChanges[(Update Database)]
    
    SaveChanges --> SaveSuccess{Success?}
    SaveSuccess -->|No| SaveError[Show Error, Retry]
    SaveError --> DisplayLoads
    SaveSuccess -->|Yes| LogAudit[(Log Audit Trail)]
    
    LogAudit --> ReExportChoice{Re-export CSV?}
    ReExportChoice -->|Yes| ExportCSV[Generate Updated CSV]
    ReExportChoice -->|No| Complete
    ExportCSV --> Complete[Changes Saved]
    Complete --> End[Return to Search or Exit]
```

---

## Admin Mode Data Flow

### Configuration Management Flow

```mermaid
flowchart TD
    Start[Admin Mode Start] --> AdminMenu[Admin Menu Display]
    AdminMenu --> SelectArea{Select Management Area}
    
    SelectArea -->|Types| TypeMgmt[Dunnage Type Management]
    SelectArea -->|Parts| PartMgmt[Part Management]
    SelectArea -->|Inventory| InvMgmt[Inventory Management]
    
    TypeMgmt --> TypeAction{Type Action?}
    TypeAction -->|Add| AddType[Add Type Dialog]
    TypeAction -->|Edit| EditType[Edit Type Dialog]
    TypeAction -->|Delete| DeleteType[Confirm & Delete]
    TypeAction -->|Manage Specs| SpecMgmt[Spec Field Configuration]
    
    AddType --> TypeData[Enter Type Data]
    TypeData --> SelectIcon[Select Icon]
    SelectIcon --> DefineSpecs[Define Spec Fields]
    DefineSpecs --> SaveType[(Save Type to DB)]
    SaveType --> TypeSuccess{Success?}
    TypeSuccess -->|No| TypeError[Show Error]
    TypeError --> TypeData
    TypeSuccess -->|Yes| RefreshTypes[Refresh Type List]
    RefreshTypes --> TypeMgmt
    
    SpecMgmt --> SpecAction{Spec Action?}
    SpecAction -->|Add Field| AddSpec[Add Spec Field]
    SpecAction -->|Edit Field| EditSpec[Edit Spec Field]
    SpecAction -->|Delete Field| DeleteSpec[Delete Spec Field]
    SpecAction -->|Reorder| ReorderSpecs[Change Display Order]
    
    AddSpec --> FieldDef[Define Field Properties]
    FieldDef --> FieldType{Field Type?}
    FieldType -->|Text| TextConfig[Text Field Config]
    FieldType -->|Number| NumberConfig[Number Field Config]
    FieldType -->|Dropdown| DropdownConfig[Dropdown Options Config]
    FieldType -->|Date| DateConfig[Date Field Config]
    
    TextConfig --> SetRequired[Set Required Flag]
    NumberConfig --> SetRequired
    DropdownConfig --> DefineOptions[Define Dropdown Options]
    DefineOptions --> SetRequired
    DateConfig --> SetRequired
    
    SetRequired --> SaveSpec[(Save Spec Field)]
    SaveSpec --> SpecSuccess{Success?}
    SpecSuccess -->|No| SpecError[Show Error]
    SpecError --> FieldDef
    SpecSuccess -->|Yes| RefreshSpecs[Refresh Spec List]
    RefreshSpecs --> SpecMgmt
    
    PartMgmt --> PartAction{Part Action?}
    PartAction -->|Add| AddPart[Add Part Dialog]
    PartAction -->|Edit| EditPart[Edit Part Dialog]
    PartAction -->|Manage Assoc| ManageAssoc[Manage Type Associations]
    
    ManageAssoc --> SelectTypes[Select Associated Types]
    SelectTypes --> SaveAssoc[(Save Associations)]
    SaveAssoc --> RefreshParts[Refresh Part List]
    RefreshParts --> PartMgmt
    
    InvMgmt --> InvAction{Inventory Action?}
    InvAction -->|Add to Inventory| AddToInv[Add to Inventoried List]
    InvAction -->|Remove| RemoveFromInv[Remove from List]
    InvAction -->|Reorder| ReorderInv[Change Priority Order]
    
    AddToInv --> SaveInv[(Save Inventory Config)]
    RemoveFromInv --> SaveInv
    ReorderInv --> SaveInv
    SaveInv --> RefreshInv[Refresh Inventory List]
    RefreshInv --> InvMgmt
```

---

## Database Persistence Layer

### Data Storage Flow

```mermaid
flowchart TD
    ViewModel[ViewModel Layer] --> Service[Service Layer]
    Service --> Validation[Business Logic Validation]
    Validation --> ValidCheck{Valid?}
    ValidCheck -->|No| ReturnError[Return Validation Error]
    ReturnError --> ViewModel
    
    ValidCheck -->|Yes| DAO[DAO Layer]
    DAO --> PrepareQuery[Prepare SQL Statement]
    PrepareQuery --> OpenConn[(Open MySQL Connection)]
    OpenConn --> BeginTxn[(Begin Transaction)]
    
    BeginTxn --> InsertLoad[(INSERT dunnage_loads)]
    InsertLoad --> LoadID[Get Load ID]
    LoadID --> InsertLines[(INSERT dunnage_lines)]
    InsertLines --> InsertSpecs[(INSERT spec_values)]
    
    InsertSpecs --> TxnCheck{Transaction OK?}
    TxnCheck -->|No| Rollback[(ROLLBACK)]
    Rollback --> LogError[Log Error]
    LogError --> CloseConn[(Close Connection)]
    CloseConn --> ReturnError
    
    TxnCheck -->|Yes| Commit[(COMMIT)]
    Commit --> CloseConn
    CloseConn --> ReturnSuccess[Return Success Result]
    ReturnSuccess --> ViewModel
```

---

## CSV Export Flow

### Dual-Path Export with Graceful Degradation

```mermaid
flowchart TD
    Start[Export Request] --> BuildCSV[Build CSV Content]
    BuildCSV --> FormatRFC[Format RFC 4180 Compliant]
    FormatRFC --> LocalPath[Get Local CSV Path]
    
    LocalPath --> WriteLocal[Write to Local File]
    WriteLocal --> LocalCheck{Write Success?}
    LocalCheck -->|No| CriticalFail[CRITICAL FAILURE]
    CriticalFail --> ErrorLog[Log Critical Error]
    ErrorLog --> ReturnFail[Return Failure Result]
    
    LocalCheck -->|Yes| LogLocal[Log Local Success]
    LogLocal --> NetworkPath[Get Network CSV Path]
    NetworkPath --> PathCheck{Network Path Configured?}
    
    PathCheck -->|No| SkipNetwork[Skip Network Export]
    SkipNetwork --> LocalOnlySuccess[Return Local-Only Success]
    
    PathCheck -->|Yes| CheckAccess{Network Accessible?}
    CheckAccess -->|No| NetworkFail[Network Unavailable]
    NetworkFail --> WarnUser[Warn User: Network Failed]
    WarnUser --> LogWarning[Log Warning]
    LogWarning --> LocalOnlySuccess
    
    CheckAccess -->|Yes| WriteNetwork[Write to Network File]
    WriteNetwork --> NetworkCheck{Write Success?}
    NetworkCheck -->|No| NetworkFail
    NetworkCheck -->|Yes| LogNetwork[Log Network Success]
    LogNetwork --> CompleteSuccess[Return Complete Success]
    
    LocalOnlySuccess --> End[Export Complete]
    CompleteSuccess --> End
    ReturnFail --> End
```

---

## State Transitions

### Workflow State Machine

```mermaid
stateDiagram-v2
    [*] --> ModeSelection
    
    ModeSelection --> GuidedMode: Select Guided
    ModeSelection --> ManualMode: Select Manual
    ModeSelection --> EditMode: Select Edit
    ModeSelection --> AdminMode: Select Admin
    
    state GuidedMode {
        [*] --> TypeSelection
        TypeSelection --> PartSelection
        PartSelection --> QuantityEntry
        QuantityEntry --> DetailsEntry
        DetailsEntry --> Review
        Review --> TypeSelection: Add More
        Review --> DetailsEntry: Edit Load
        Review --> [*]: Save Complete
    }
    
    state ManualMode {
        [*] --> GridEntry
        GridEntry --> GridEntry: Add/Edit Rows
        GridEntry --> [*]: Save Complete
    }
    
    state EditMode {
        [*] --> Search
        Search --> TransactionView
        TransactionView --> LoadEdit
        LoadEdit --> TransactionView: Save Changes
        TransactionView --> Search: New Search
        TransactionView --> [*]: Exit
    }
    
    state AdminMode {
        [*] --> AdminMenu
        AdminMenu --> TypeManagement
        AdminMenu --> PartManagement
        AdminMenu --> InventoryManagement
        TypeManagement --> SpecManagement
        SpecManagement --> TypeManagement
        TypeManagement --> AdminMenu
        PartManagement --> AdminMenu
        InventoryManagement --> AdminMenu
        AdminMenu --> [*]: Exit
    }
    
    GuidedMode --> ModeSelection: Cancel
    ManualMode --> ModeSelection: Cancel
    EditMode --> ModeSelection: Exit
    AdminMode --> ModeSelection: Exit
```

---

## Validation Points

### Data Validation Flow

```mermaid
flowchart TD
    Input[User Input] --> ClientValidation[Client-Side Validation]
    ClientValidation --> ClientCheck{Valid?}
    ClientCheck -->|No| ShowClientError[Show Immediate Feedback]
    ShowClientError --> Input
    
    ClientCheck -->|Yes| ServiceValidation[Service Layer Validation]
    ServiceValidation --> BusinessRules[Apply Business Rules]
    BusinessRules --> RuleCheck{Rules Pass?}
    RuleCheck -->|No| ShowServiceError[Show Validation Error]
    ShowServiceError --> Input
    
    RuleCheck -->|Yes| DAOValidation[DAO Layer Validation]
    DAOValidation --> DBConstraints[Check DB Constraints]
    DBConstraints --> DBCheck{Constraints Met?}
    DBCheck -->|No| ShowDBError[Show Database Error]
    ShowDBError --> Input
    
    DBCheck -->|Yes| Persist[(Persist to Database)]
    Persist --> Success[Validation Complete]
```

**Validation Points by Layer:**

**Client-Side (ViewModel):**
- Required field presence
- Data type correctness
- Format validation (e.g., numeric range)
- Field length limits

**Service Layer:**
- Business rule enforcement
- Cross-field validation
- State consistency checks
- Permission validation

**DAO Layer:**
- Foreign key integrity
- Unique constraint checks
- Database-specific constraints
- Transaction integrity

---

## Error Handling Flow

### Comprehensive Error Management

```mermaid
flowchart TD
    Error[Error Occurs] --> ErrorType{Error Type?}
    
    ErrorType -->|Validation| ValidationError[Validation Error Handler]
    ErrorType -->|Database| DatabaseError[Database Error Handler]
    ErrorType -->|Network| NetworkError[Network Error Handler]
    ErrorType -->|System| SystemError[System Error Handler]
    
    ValidationError --> LogValidation[Log Validation Details]
    LogValidation --> ShowUser1[Show User-Friendly Message]
    ShowUser1 --> AllowRetry1[Allow User to Correct]
    
    DatabaseError --> LogDB[Log Database Error]
    LogDB --> CheckRecoverable{Recoverable?}
    CheckRecoverable -->|Yes| RetryDB[Offer Retry]
    CheckRecoverable -->|No| ShowCritical[Show Critical Error]
    ShowCritical --> RollbackDB[(Rollback Changes)]
    
    NetworkError --> LogNetwork[Log Network Error]
    LogNetwork --> Degrade[Graceful Degradation]
    Degrade --> WarnUser[Warn User, Continue]
    
    SystemError --> LogSystem[Log System Error]
    LogSystem --> CrashReport[Generate Crash Report]
    CrashReport --> NotifyDev[Notify Development Team]
    NotifyDev --> ShowSystemError[Show System Error to User]
    
    AllowRetry1 --> Recovery[Return to Entry Point]
    RetryDB --> Recovery
    WarnUser --> Recovery
    ShowSystemError --> ForcedExit[Forced Exit to Safe State]
```

---

## Session Management

### User Session Lifecycle

```mermaid
flowchart TD
    AppStart[Application Start] --> LoadUser[(Load User Profile)]
    LoadUser --> CheckDefault{Default Mode Set?}
    CheckDefault -->|Yes| InitSession[Initialize Session with Default]
    CheckDefault -->|No| EmptySession[Initialize Empty Session]
    
    InitSession --> WorkflowActive[Workflow Active]
    EmptySession --> WorkflowActive
    
    WorkflowActive --> StoreData[Store Session Data]
    StoreData --> SessionData[(Session State)]
    SessionData --> UserAction{User Action?}
    
    UserAction -->|Add Load| UpdateSession[Update Session]
    UserAction -->|Edit Load| UpdateSession
    UserAction -->|Delete Load| UpdateSession
    UpdateSession --> SessionData
    
    UserAction -->|Save & Exit| PersistData[(Persist to DB)]
    PersistData --> ClearSession[Clear Session State]
    
    UserAction -->|Cancel| ConfirmCancel{Confirm Discard?}
    ConfirmCancel -->|No| WorkflowActive
    ConfirmCancel -->|Yes| ClearSession
    
    ClearSession --> SessionEnd[Session Ended]
    SessionEnd --> AppExit{Exit App?}
    AppExit -->|No| EmptySession
    AppExit -->|Yes| Cleanup[Cleanup Resources]
    Cleanup --> End[Application Exit]
```

---

## Integration Points

### External System Integration

**MySQL Database:**
- Connection pooling for performance
- Transaction management for data integrity
- Error handling with retry logic
- Connection string from configuration

**CSV Export System:**
- RFC 4180 compliant formatting
- UTF-8 encoding with BOM
- CRLF line endings
- Dual-path export (local + network)

**Module_Core Services:**
- Error Handler: Centralized error management
- Logger: Structured logging
- Session Manager: User session state
- Settings Service: Configuration retrieval

**Module_Settings.Dunnage:**
- Type configuration retrieval
- Spec field definitions
- Part-type associations
- User preferences

---

## Performance Considerations

### Data Flow Optimizations

**Caching Strategy:**
- Type definitions cached on app startup
- Spec fields loaded on-demand per type
- Part associations cached per type
- Inventory list cached globally

**Lazy Loading:**
- Historical transactions loaded on search only
- Large result sets paginated
- Grid rows virtualized for 100+ loads

**Batch Operations:**
- Multiple loads saved in single transaction
- Bulk CSV write vs multiple file operations
- Batch validation for grid entries

**Async Operations:**
- All database operations async
- CSV export non-blocking
- UI remains responsive during saves

---

## Related Documentation

- [Purpose and Overview](./purpose-and-overview.md) - Module overview
- [Guided Mode Specification](../../02-Workflow-Modes/001-guided-mode-specification.md) - Detailed workflow
- [Dynamic Specification Fields](../../01-Business-Rules/dynamic-specification-fields.md) - Spec field system
- [CSV Export Paths](../../01-Business-Rules/csv-export-paths.md) - Export configuration

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Status:** Complete
