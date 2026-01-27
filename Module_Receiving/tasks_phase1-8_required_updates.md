# Required Feature Updates - Phases 1-8 Task Additions

**Generated:** 2025-01-30  
**Purpose:** Add missing critical features from Old_Module_Receiving to existing phase task plans  
**Scope:** Features applicable to Wizard Mode and Mode Selection Hub  
**Architecture:** CQRS pattern with MediatR, leveraging Module_Core where appropriate

---

## ?? CRITICAL VALIDATION RULE

**ALL TEXTBOX/INPUT VALIDATION MUST OCCUR ON `LostFocus` EVENT - NOT DURING TYPING**

This applies to:
- PO Number validation
- Part Number validation
- Load Count validation
- Weight/Quantity validation
- Heat/Lot validation
- Package count validation
- All user input fields

**Implementation Pattern:**
```xaml
<TextBox 
    Text="{x:Bind ViewModel.PoNumber, Mode=TwoWay}"
    LostFocus="OnPoNumberLostFocus" />
```

```csharp
private async void OnPoNumberLostFocus(object sender, RoutedEventArgs e)
{
    await ViewModel.ValidatePoNumberCommand.ExecuteAsync(null);
}
```

**Rationale:** Validating during typing creates poor UX and excessive query traffic. Validate only when user completes input and moves to next field.

---

## Executive Summary

This document adds **47 new tasks** across Phases 2-8 to implement critical missing features:

| Phase | New Tasks | Priority Breakdown | Estimated Hours |
|-------|-----------|-------------------|-----------------|
| **Phase 2** | 7 tasks | 7 P0 CRITICAL | 12-15 hours |
| **Phase 3** | 8 tasks | 6 P0, 2 P1 | 10-12 hours |
| **Phase 4** | 12 tasks | 2 P0, 6 P1, 4 P2 | 18-22 hours |
| **Phase 6** | 8 tasks | 6 P0, 2 P1 | 12-15 hours |
| **Phase 7** | 10 tasks | 4 P1, 6 P2 | 15-18 hours |
| **Phase 8** | 2 tasks | 2 P1 | 3-4 hours |
| **TOTAL** | **47 tasks** | **19 P0, 16 P1, 12 P2** | **70-86 hours** |

**Critical Path Impact:** Quality Hold (P0) blocks production release. CSV Export (P1) required for business operations.

---

## Phase 2: Database & DAOs - New Tasks

**Add to:** `Module_Receiving/tasks_phase2.md`  
**Insert After:** Task 2.58 (last DAO test task)

---

### ?? P0 CRITICAL: Quality Hold Infrastructure

#### Task 2.59: Create tbl_Receiving_QualityHold Table

**Priority:** P0 - CRITICAL COMPLIANCE REQUIREMENT  
**File:** `Module_Databases/Module_Receiving_Database/Tables/tbl_Receiving_QualityHold.sql`  
**Dependencies:** None  
**Estimated Time:** 2 hours

**Table Schema:**
```sql
CREATE TABLE dbo.tbl_Receiving_QualityHold
(
    QualityHoldID           INT IDENTITY(1,1)   NOT NULL,
    LineID                  NVARCHAR(50)        NOT NULL,  -- FK to tbl_Receiving_Line
    PartNumber              NVARCHAR(100)       NOT NULL,
    RestrictionType         NVARCHAR(50)        NOT NULL,  -- 'MMFSR' or 'MMCSR'
    RequiredBy              NVARCHAR(100)       NOT NULL,  -- User who entered the part
    RequiredAt              DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
    
    -- Acknowledgment 1 (User entering data)
    UserAcknowledged        BIT                 NOT NULL DEFAULT 0,
    UserAcknowledgedAt      DATETIME2           NULL,
    
    -- Acknowledgment 2 (Pre-save confirmation)
    FinalAcknowledged       BIT                 NOT NULL DEFAULT 0,
    FinalAcknowledgedAt     DATETIME2           NULL,
    
    -- Quality Inspector Sign-off (future enhancement)
    QualityApprovedBy       NVARCHAR(100)       NULL,
    QualityApprovedAt       DATETIME2           NULL,
    QualityNotes            NVARCHAR(500)       NULL,
    
    IsActive                BIT                 NOT NULL DEFAULT 1,
    CreatedAt               DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
    ModifiedAt              DATETIME2           NULL,
    
    CONSTRAINT PK_Receiving_QualityHold PRIMARY KEY CLUSTERED (QualityHoldID),
    CONSTRAINT FK_Receiving_QualityHold_Line FOREIGN KEY (LineID)
        REFERENCES dbo.tbl_Receiving_Line(LineID),
    CONSTRAINT CK_Receiving_QualityHold_RestrictionType 
        CHECK (RestrictionType IN ('MMFSR', 'MMCSR'))
);

CREATE NONCLUSTERED INDEX IX_Receiving_QualityHold_LineID 
    ON dbo.tbl_Receiving_QualityHold(LineID);

CREATE NONCLUSTERED INDEX IX_Receiving_QualityHold_PartNumber 
    ON dbo.tbl_Receiving_QualityHold(PartNumber);
```

**Acceptance Criteria:**
- [ ] Table created with all fields
- [ ] Primary key constraint named correctly
- [ ] Foreign key to tbl_Receiving_Line
- [ ] Check constraint for RestrictionType
- [ ] Indexes for performance
- [ ] Compiles without errors

---

#### Task 2.60: Create sp_Receiving_QualityHold_Insert

**Priority:** P0 - CRITICAL  
**File:** `Module_Databases/Module_Receiving_Database/StoredProcedures/QualityHold/sp_Receiving_QualityHold_Insert.sql`  
**Dependencies:** Task 2.59  
**Estimated Time:** 1.5 hours

**Stored Procedure Signature:**
```sql
CREATE PROCEDURE dbo.sp_Receiving_QualityHold_Insert
    @p_LineID               NVARCHAR(50),
    @p_PartNumber           NVARCHAR(100),
    @p_RestrictionType      NVARCHAR(50),
    @p_RequiredBy           NVARCHAR(100),
    @p_QualityHoldID        INT OUTPUT,
    @p_Status               INT OUTPUT,
    @p_ErrorMsg             NVARCHAR(500) OUTPUT
AS
BEGIN
    -- Insert new quality hold record
    -- Set user acknowledged to 1 (first acknowledgment)
    -- Return new QualityHoldID
END
```

**Business Rules:**
- Initial insert sets `UserAcknowledged = 1`, `UserAcknowledgedAt = GETUTCDATE()`
- `FinalAcknowledged` remains 0 until pre-save confirmation
- Returns @p_Status = 0 on success, 1 on error
- Returns @p_QualityHoldID for tracking

**Acceptance Criteria:**
- [ ] Inserts record with initial acknowledgment
- [ ] Returns QualityHoldID output parameter
- [ ] Standard error handling pattern
- [ ] Transaction wrapped
- [ ] Compiles without errors

---

#### Task 2.61: Create sp_Receiving_QualityHold_UpdateFinalAcknowledgment

**Priority:** P0 - CRITICAL  
**File:** `Module_Databases/Module_Receiving_Database/StoredProcedures/QualityHold/sp_Receiving_QualityHold_UpdateFinalAcknowledgment.sql`  
**Dependencies:** Task 2.59  
**Estimated Time:** 1 hour

**Stored Procedure Signature:**
```sql
CREATE PROCEDURE dbo.sp_Receiving_QualityHold_UpdateFinalAcknowledgment
    @p_QualityHoldID        INT,
    @p_Status               INT OUTPUT,
    @p_ErrorMsg             NVARCHAR(500) OUTPUT
AS
BEGIN
    -- Update FinalAcknowledged = 1
    -- Set FinalAcknowledgedAt = GETUTCDATE()
END
```

**Acceptance Criteria:**
- [ ] Updates final acknowledgment flag
- [ ] Sets acknowledgment timestamp
- [ ] Standard error handling
- [ ] Compiles without errors

---

#### Task 2.62: Create sp_Receiving_QualityHold_SelectByLineID

**Priority:** P0 - CRITICAL  
**File:** `Module_Databases/Module_Receiving_Database/StoredProcedures/QualityHold/sp_Receiving_QualityHold_SelectByLineID.sql`  
**Dependencies:** Task 2.59  
**Estimated Time:** 1 hour

**Stored Procedure Signature:**
```sql
CREATE PROCEDURE dbo.sp_Receiving_QualityHold_SelectByLineID
    @p_LineID               NVARCHAR(50)
AS
BEGIN
    -- Return quality hold records for given LineID
    -- Include all acknowledgment status fields
END
```

**Acceptance Criteria:**
- [ ] Returns all quality hold fields
- [ ] Filtered by LineID
- [ ] Ordered by CreatedAt DESC
- [ ] Compiles without errors

---

#### Task 2.63: Create Model_Receiving_Entity_QualityHold

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/Models/Entities/Model_Receiving_Entity_QualityHold.cs`  
**Dependencies:** Task 2.59  
**Estimated Time:** 1 hour

**Entity Model:**
```csharp
namespace MTM_Receiving_Application.Module_Receiving.Models.Entities;

/// <summary>
/// Entity representing a quality hold record for restricted parts (MMFSR/MMCSR)
/// Maps to tbl_Receiving_QualityHold database table
/// </summary>
public class Model_Receiving_Entity_QualityHold
{
    public int QualityHoldID { get; set; }
    public string LineID { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public string RestrictionType { get; set; } = string.Empty;
    public string RequiredBy { get; set; } = string.Empty;
    public DateTime RequiredAt { get; set; }
    
    public bool UserAcknowledged { get; set; }
    public DateTime? UserAcknowledgedAt { get; set; }
    
    public bool FinalAcknowledged { get; set; }
    public DateTime? FinalAcknowledgedAt { get; set; }
    
    public string? QualityApprovedBy { get; set; }
    public DateTime? QualityApprovedAt { get; set; }
    public string? QualityNotes { get; set; }
    
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    
    /// <summary>
    /// Indicates if both user acknowledgments are complete (ready for save)
    /// </summary>
    public bool IsBothAcknowledgmentsComplete => UserAcknowledged && FinalAcknowledged;
    
    /// <summary>
    /// Indicates if quality inspector has approved (future enhancement)
    /// </summary>
    public bool IsQualityApproved => !string.IsNullOrEmpty(QualityApprovedBy) && QualityApprovedAt.HasValue;
}
```

**Acceptance Criteria:**
- [ ] All table fields mapped
- [ ] Computed properties for acknowledgment status
- [ ] XML documentation comments
- [ ] No CommunityToolkit dependencies (plain entity)
- [ ] Compiles without errors

---

#### Task 2.64: Create Dao_Receiving_Repository_QualityHold

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/Data/Dao_Receiving_Repository_QualityHold.cs`  
**Dependencies:** Tasks 2.60-2.62, 2.63  
**Estimated Time:** 3 hours

**DAO Pattern:**
```csharp
public class Dao_Receiving_Repository_QualityHold
{
    private readonly string _connectionString;
    private readonly IService_LoggingUtility _logger;

    public Dao_Receiving_Repository_QualityHold(
        string connectionString,
        IService_LoggingUtility logger)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(logger);
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<int>> InsertQualityHoldAsync(
        Model_Receiving_Entity_QualityHold qualityHold);
    
    public async Task<Model_Dao_Result> UpdateFinalAcknowledgmentAsync(int qualityHoldId);
    
    public async Task<Model_Dao_Result<List<Model_Receiving_Entity_QualityHold>>> 
        SelectByLineIdAsync(string lineId);
}
```

**Methods:**
1. `InsertQualityHoldAsync` - Calls `sp_Receiving_QualityHold_Insert`
2. `UpdateFinalAcknowledgmentAsync` - Calls `sp_Receiving_QualityHold_UpdateFinalAcknowledgment`
3. `SelectByLineIdAsync` - Calls `sp_Receiving_QualityHold_SelectByLineID`

**Acceptance Criteria:**
- [ ] All 3 methods implemented
- [ ] Uses `Helper_Database_StoredProcedure` for all SP calls
- [ ] Returns `Model_Dao_Result` pattern
- [ ] Never throws exceptions (returns failure results)
- [ ] Logging for all operations
- [ ] Parameter null validation
- [ ] Compiles without errors

---

#### Task 2.65: Add Quality Hold Fields to Model_Receiving_Entity_ReceivingLoad

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/Models/Entities/Model_Receiving_Entity_ReceivingLoad.cs` (MODIFY)  
**Dependencies:** None  
**Estimated Time:** 30 minutes

**Add Properties:**
```csharp
/// <summary>
/// Indicates if this load contains a restricted part requiring quality hold
/// </summary>
public bool IsQualityHoldRequired { get; set; }

/// <summary>
/// Type of quality hold restriction (MMFSR or MMCSR)
/// </summary>
public string? QualityHoldRestrictionType { get; set; }

/// <summary>
/// Indicates if user has acknowledged quality hold warning (Acknowledgment 1 of 2)
/// </summary>
public bool UserAcknowledgedQualityHold { get; set; }

/// <summary>
/// Indicates if final acknowledgment completed before save (Acknowledgment 2 of 2)
/// </summary>
public bool FinalAcknowledgedQualityHold { get; set; }
```

**Acceptance Criteria:**
- [ ] 4 new properties added
- [ ] XML documentation comments
- [ ] No breaking changes to existing code
- [ ] Compiles without errors

---

## Phase 3: CQRS Handlers & Validators - New Tasks

**Add to:** `Module_Receiving/tasks_phase3.md`  
**Insert After:** Last validator task

---

### ?? P0 CRITICAL: Quality Hold CQRS Commands/Queries

#### Task 3.XX: CommandRequest_Receiving_Shared_Create_QualityHold

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/Requests/Commands/CommandRequest_Receiving_Shared_Create_QualityHold.cs`  
**Dependencies:** Phase 2 quality hold tasks  
**Estimated Time:** 1 hour

**Command Definition:**
```csharp
public class CommandRequest_Receiving_Shared_Create_QualityHold : IRequest<Model_Dao_Result<int>>
{
    public string LineID { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public string RestrictionType { get; set; } = string.Empty;
    public string RequiredBy { get; set; } = string.Empty;
}
```

**Acceptance Criteria:**
- [ ] Implements `IRequest<Model_Dao_Result<int>>`
- [ ] Returns QualityHoldID on success
- [ ] Follows 5-part naming convention
- [ ] XML documentation comments
- [ ] Compiles without errors

---

#### Task 3.XX: CommandHandler_Receiving_Shared_Create_QualityHold

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/Handlers/Commands/CommandHandler_Receiving_Shared_Create_QualityHold.cs`  
**Dependencies:** Task above, Phase 2 DAO  
**Estimated Time:** 1.5 hours

**Handler Pattern:**
```csharp
public class CommandHandler_Receiving_Shared_Create_QualityHold 
    : IRequestHandler<CommandRequest_Receiving_Shared_Create_QualityHold, Model_Dao_Result<int>>
{
    private readonly Dao_Receiving_Repository_QualityHold _dao;
    private readonly IService_LoggingUtility _logger;

    public async Task<Model_Dao_Result<int>> Handle(
        CommandRequest_Receiving_Shared_Create_QualityHold request,
        CancellationToken cancellationToken)
    {
        _logger.LogInfo($"Creating quality hold for part {request.PartNumber}");
        
        var entity = new Model_Receiving_Entity_QualityHold
        {
            LineID = request.LineID,
            PartNumber = request.PartNumber,
            RestrictionType = request.RestrictionType,
            RequiredBy = request.RequiredBy
        };
        
        return await _dao.InsertQualityHoldAsync(entity);
    }
}
```

**Acceptance Criteria:**
- [ ] Injects DAO and logger
- [ ] Maps request to entity
- [ ] Calls DAO method
- [ ] Logs operation
- [ ] Returns result without modification
- [ ] Compiles without errors

---

#### Task 3.XX: QueryRequest_Receiving_Shared_Get_QualityHoldsByLine

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/Requests/Queries/QueryRequest_Receiving_Shared_Get_QualityHoldsByLine.cs`  
**Dependencies:** Phase 2 quality hold tasks  
**Estimated Time:** 30 minutes

**Query Definition:**
```csharp
public class QueryRequest_Receiving_Shared_Get_QualityHoldsByLine 
    : IRequest<Model_Dao_Result<List<Model_Receiving_Entity_QualityHold>>>
{
    public string LineID { get; set; } = string.Empty;
}
```

**Acceptance Criteria:**
- [ ] Implements `IRequest<Model_Dao_Result<List<...>>>`
- [ ] Single LineID parameter
- [ ] XML documentation
- [ ] Compiles without errors

---

#### Task 3.XX: QueryHandler_Receiving_Shared_Get_QualityHoldsByLine

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/Handlers/Queries/QueryHandler_Receiving_Shared_Get_QualityHoldsByLine.cs`  
**Dependencies:** Task above, Phase 2 DAO  
**Estimated Time:** 1 hour

**Handler Pattern:**
```csharp
public class QueryHandler_Receiving_Shared_Get_QualityHoldsByLine 
    : IRequestHandler<QueryRequest_Receiving_Shared_Get_QualityHoldsByLine, 
                      Model_Dao_Result<List<Model_Receiving_Entity_QualityHold>>>
{
    private readonly Dao_Receiving_Repository_QualityHold _dao;
    private readonly IService_LoggingUtility _logger;

    public async Task<Model_Dao_Result<List<Model_Receiving_Entity_QualityHold>>> Handle(
        QueryRequest_Receiving_Shared_Get_QualityHoldsByLine request,
        CancellationToken cancellationToken)
    {
        _logger.LogInfo($"Retrieving quality holds for line {request.LineID}");
        return await _dao.SelectByLineIdAsync(request.LineID);
    }
}
```

**Acceptance Criteria:**
- [ ] Injects DAO and logger
- [ ] Calls DAO method
- [ ] Logs operation
- [ ] Compiles without errors

---

### ?? P0: Quality Hold Validators

#### Task 3.XX: Validator_Receiving_Shared_ValidateIf_RestrictedPart

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/Validators/Validator_Receiving_Shared_ValidateIf_RestrictedPart.cs`  
**Dependencies:** None  
**Estimated Time:** 1 hour

**Validator Pattern:**
```csharp
public class Validator_Receiving_Shared_ValidateIf_RestrictedPart : AbstractValidator<string>
{
    public Validator_Receiving_Shared_ValidateIf_RestrictedPart()
    {
        RuleFor(partNumber => partNumber)
            .Must(BeRestrictedPart)
            .WithMessage("Part {PropertyValue} requires quality hold acknowledgment");
    }

    private bool BeRestrictedPart(string? partNumber)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
            return false;

        var upper = partNumber.ToUpperInvariant();
        return upper.Contains("MMFSR") || upper.Contains("MMCSR");
    }
}
```

**Usage:** Standalone validator that can be called from ViewModels to detect restricted parts.

**Acceptance Criteria:**
- [ ] FluentValidation AbstractValidator
- [ ] Detects MMFSR and MMCSR parts
- [ ] Case-insensitive matching
- [ ] XML documentation
- [ ] Compiles without errors

---

#### Task 3.XX: Enhance Validator_Receiving_Shared_ValidateOn_SaveTransaction (Quality Hold Check)

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/Validators/Validator_Receiving_Shared_ValidateOn_SaveTransaction.cs` (MODIFY)  
**Dependencies:** Quality hold entity model  
**Estimated Time:** 1.5 hours

**Add Validation Rule:**
```csharp
// Add to existing validator
RuleForEach(cmd => cmd.Lines)
    .Must(line => !line.IsQualityHoldRequired || line.FinalAcknowledgedQualityHold)
    .WithMessage("Quality hold acknowledgment required for line {PropertyValue.LoadNumber}. " +
                 "Part {PropertyValue.PartNumber} requires quality inspection approval.");
```

**Business Rule:**
- If line has `IsQualityHoldRequired = true`, MUST have `FinalAcknowledgedQualityHold = true`
- Blocks save if any quality hold not fully acknowledged
- Provides clear error message with part number and load number

**Acceptance Criteria:**
- [ ] Quality hold validation added
- [ ] Clear error messages
- [ ] No breaking changes to existing validation
- [ ] Compiles without errors
- [ ] All existing tests still pass

---

### ?? P1 HIGH: CSV Export Commands

#### Task 3.XX: CommandRequest_Receiving_Shared_Export_TransactionToCSV

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/Requests/Commands/CommandRequest_Receiving_Shared_Export_TransactionToCSV.cs`  
**Dependencies:** None  
**Estimated Time:** 1 hour

**Command Definition:**
```csharp
public class CommandRequest_Receiving_Shared_Export_TransactionToCSV : IRequest<Model_Receiving_Result_CSVExport>
{
    public string TransactionID { get; set; } = string.Empty;
    public bool ExportToNetwork { get; set; } = true;
    public bool ExportToLocal { get; set; } = true;
}
```

**Note:** Returns custom result model (not `Model_Dao_Result`) because CSV export is file I/O, not database operation.

**Acceptance Criteria:**
- [ ] Implements `IRequest<Model_Receiving_Result_CSVExport>`
- [ ] Flags for local/network export control
- [ ] XML documentation
- [ ] Compiles without errors

---

#### Task 3.XX: Create Model_Receiving_Result_CSVExport

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/Models/Results/Model_Receiving_Result_CSVExport.cs`  
**Dependencies:** None  
**Estimated Time:** 30 minutes

**Result Model:**
```csharp
namespace MTM_Receiving_Application.Module_Receiving.Models.Results;

/// <summary>
/// Result of CSV export operation with local and network path tracking
/// </summary>
public class Model_Receiving_Result_CSVExport
{
    public bool LocalSuccess { get; set; }
    public string? LocalFilePath { get; set; }
    public string? LocalErrorMessage { get; set; }
    
    public bool NetworkSuccess { get; set; }
    public string? NetworkFilePath { get; set; }
    public string? NetworkErrorMessage { get; set; }
    
    public int RecordsExported { get; set; }
    public DateTime ExportedAt { get; set; }
    
    /// <summary>
    /// True if at least one destination succeeded
    /// </summary>
    public bool OverallSuccess => LocalSuccess || NetworkSuccess;
    
    /// <summary>
    /// Summary message for user display
    /// </summary>
    public string GetSummaryMessage()
    {
        var parts = new List<string>();
        if (LocalSuccess)
            parts.Add($"Local: {LocalFilePath}");
        if (NetworkSuccess)
            parts.Add($"Network: {NetworkFilePath}");
        
        if (parts.Count == 0)
            return "CSV export failed";
        
        return $"Exported {RecordsExported} records - " + string.Join(", ", parts);
    }
}
```

**Acceptance Criteria:**
- [ ] Separate success/fail tracking for local and network
- [ ] File paths stored
- [ ] Error messages captured
- [ ] Summary method for UI display
- [ ] XML documentation
- [ ] Compiles without errors

---

## Phase 4: Wizard ViewModels - New Tasks

**Add to:** `Module_Receiving/tasks_phase4.md`  
**Insert at appropriate sections**

---

### ?? P0 CRITICAL: Quality Hold Detection Service (Module_Receiving standalone)

#### Task 4.XX: Create Service_Receiving_QualityHoldDetection

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/Services/Service_Receiving_QualityHoldDetection.cs`  
**Dependencies:** None  
**Estimated Time:** 2 hours

**Why Module_Receiving and not Module_Core?**
- Quality hold logic is DOMAIN-SPECIFIC to receiving module
- Part number pattern matching (MMFSR/MMCSR) is business rule
- ViewModels will inject this service directly (not through CQRS)
- CQRS is for data persistence, not domain services

**Service Pattern:**
```csharp
namespace MTM_Receiving_Application.Module_Receiving.Services;

public interface IService_Receiving_QualityHoldDetection
{
    bool IsRestrictedPart(string? partNumber);
    string GetRestrictionType(string partNumber);
    Task<bool> ShowFirstWarningDialogAsync(string partNumber);
    Task<bool> ShowFinalWarningDialogAsync(string partNumber);
}

public class Service_Receiving_QualityHoldDetection : IService_Receiving_QualityHoldDetection
{
    private readonly IService_Window _windowService;
    private readonly IService_LoggingUtility _logger;

    public Service_Receiving_QualityHoldDetection(
        IService_Window windowService,
        IService_LoggingUtility logger)
    {
        _windowService = windowService;
        _logger = logger;
    }

    public bool IsRestrictedPart(string? partNumber)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
            return false;

        var upper = partNumber.ToUpperInvariant();
        return upper.Contains("MMFSR") || upper.Contains("MMCSR");
    }

    public string GetRestrictionType(string partNumber)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
            return string.Empty;

        var upper = partNumber.ToUpperInvariant();
        if (upper.Contains("MMFSR"))
            return "MMFSR";
        if (upper.Contains("MMCSR"))
            return "MMCSR";
        
        return string.Empty;
    }

    public async Task<bool> ShowFirstWarningDialogAsync(string partNumber)
    {
        var restrictionType = GetRestrictionType(partNumber);
        var materialType = restrictionType == "MMFSR" ? "Sheet Material" : "Coil Material";

        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot == null)
        {
            _logger.LogError("Cannot show quality hold warning: XamlRoot is null");
            return false;
        }

        var dialog = new ContentDialog
        {
            Title = "?? QUALITY HOLD - Acknowledgment 1 of 2",
            Content = new TextBlock
            {
                Text = $"?? QUALITY HOLD REQUIRED ??\n\n" +
                       $"ACKNOWLEDGMENT 1 of 2\n\n" +
                       $"Part Number: {partNumber}\n" +
                       $"Type: {materialType} - Quality Hold Required\n\n" +
                       $"IMMEDIATE ACTION REQUIRED:\n" +
                       $"• Contact Quality NOW\n" +
                       $"• Quality MUST inspect and accept this load\n" +
                       $"• DO NOT sign any paperwork until Quality accepts\n\n" +
                       $"You will be asked to confirm again before saving.\n" +
                       $"This is a critical quality control checkpoint.",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14
            },
            PrimaryButtonText = "I Understand - Will Contact Quality",
            CloseButtonText = "Cancel Entry",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot
        };

        var result = await dialog.ShowAsync();
        bool acknowledged = result == ContentDialogResult.Primary;

        _logger.LogInfo($"Quality hold first warning for {partNumber}: {(acknowledged ? "Acknowledged" : "Cancelled")}");
        return acknowledged;
    }

    public async Task<bool> ShowFinalWarningDialogAsync(string partNumber)
    {
        var restrictionType = GetRestrictionType(partNumber);
        var materialType = restrictionType == "MMFSR" ? "Sheet Material" : "Coil Material";

        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot == null)
        {
            _logger.LogError("Cannot show final quality hold warning: XamlRoot is null");
            return false;
        }

        var dialog = new ContentDialog
        {
            Title = "?? QUALITY HOLD - FINAL ACKNOWLEDGMENT 2 of 2",
            Content = new TextBlock
            {
                Text = $"?? FINAL QUALITY HOLD CONFIRMATION ??\n\n" +
                       $"ACKNOWLEDGMENT 2 of 2 (FINAL)\n\n" +
                       $"Part Number: {partNumber}\n" +
                       $"Type: {materialType}\n\n" +
                       $"CONFIRM YOU HAVE:\n" +
                       $"? Contacted Quality Department\n" +
                       $"? Quality is aware of this receiving\n" +
                       $"? Will NOT sign paperwork until Quality accepts\n\n" +
                       $"By clicking 'Confirm', you acknowledge this is a\n" +
                       $"quality-controlled item requiring inspection.",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Foreground = new SolidColorBrush(Colors.DarkRed)
            },
            PrimaryButtonText = "Confirm - Quality Has Been Notified",
            CloseButtonText = "Cancel Save",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = xamlRoot
        };

        var result = await dialog.ShowAsync();
        bool acknowledged = result == ContentDialogResult.Primary;

        _logger.LogInfo($"Quality hold final warning for {partNumber}: {(acknowledged ? "Acknowledged" : "Cancelled")}");
        return acknowledged;
    }
}
```

**Acceptance Criteria:**
- [ ] Interface and implementation created
- [ ] Part detection logic (MMFSR/MMCSR)
- [ ] First warning dialog (Acknowledgment 1 of 2)
- [ ] Final warning dialog (Acknowledgment 2 of 2)
- [ ] Logging for all dialogs
- [ ] Uses Module_Core `IService_Window` for XamlRoot
- [ ] XML documentation
- [ ] Compiles without errors

---

### ?? P1: Enhance ViewModel_Receiving_Wizard_Display_PartSelection (Quality Hold Detection)

#### Task 4.XX: Add Quality Hold Detection to Part Selection

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Step1/ViewModel_Receiving_Wizard_Display_PartSelection.cs` (MODIFY)  
**Dependencies:** Service_Receiving_QualityHoldDetection  
**Estimated Time:** 2 hours

**Add Service Injection:**
```csharp
private readonly IService_Receiving_QualityHoldDetection _qualityHoldService;

public ViewModel_Receiving_Wizard_Display_PartSelection(
    IMediator mediator,
    IService_ErrorHandler errorHandler,
    IService_LoggingUtility logger,
    IService_Notification notificationService,
    IService_Receiving_QualityHoldDetection qualityHoldService)  // ADD THIS
    : base(errorHandler, logger, notificationService)
{
    _mediator = mediator;
    _qualityHoldService = qualityHoldService;
}
```

**Add Quality Hold Detection Logic:**
```csharp
[RelayCommand]
private async Task SelectPartAsync(string partNumber)
{
    if (IsBusy) return;
    try
    {
        IsBusy = true;
        SelectedPartNumber = partNumber;

        // Check if restricted part
        if (_qualityHoldService.IsRestrictedPart(partNumber))
        {
            _logger.LogWarning($"Restricted part selected: {partNumber}");
            
            // Show first warning dialog
            bool acknowledged = await _qualityHoldService.ShowFirstWarningDialogAsync(partNumber);
            
            if (!acknowledged)
            {
                // User cancelled
                SelectedPartNumber = string.Empty;
                ShowNotification("Part entry cancelled - quality hold not acknowledged", 
                                 Enum_Shared_Severity.Warning);
                return;
            }

            // Mark load as requiring quality hold
            IsQualityHoldRequired = true;
            QualityHoldRestrictionType = _qualityHoldService.GetRestrictionType(partNumber);
            UserAcknowledgedQualityHold = true;
            
            ShowNotification($"Quality hold required for {partNumber} - Contact Quality NOW", 
                             Enum_Shared_Severity.Error);
        }
        else
        {
            IsQualityHoldRequired = false;
            QualityHoldRestrictionType = null;
            UserAcknowledgedQualityHold = false;
        }

        // Continue with normal part selection logic...
    }
    catch (Exception ex)
    {
        _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, 
                                       nameof(SelectPartAsync), 
                                       nameof(ViewModel_Receiving_Wizard_Display_PartSelection));
    }
    finally
    {
        IsBusy = false;
    }
}
```

**Add Properties:**
```csharp
[ObservableProperty]
private bool _isQualityHoldRequired;

[ObservableProperty]
private string? _qualityHoldRestrictionType;

[ObservableProperty]
private bool _userAcknowledgedQualityHold;
```

**Acceptance Criteria:**
- [ ] Service injected
- [ ] Quality hold detection on part selection
- [ ] First warning dialog shown if restricted
- [ ] Properties set for quality hold tracking
- [ ] User can cancel selection
- [ ] Notification shown for restricted parts
- [ ] Compiles without errors

---

### ?? P1: Enhance ViewModel_Receiving_Wizard_Orchestration_SaveOperation (Final Quality Hold Check)

#### Task 4.XX: Add Final Quality Hold Validation to Save Operation

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Step3/ViewModel_Receiving_Wizard_Orchestration_SaveOperation.cs` (MODIFY)  
**Dependencies:** Service_Receiving_QualityHoldDetection  
**Estimated Time:** 2 hours

**Add Service Injection:**
```csharp
private readonly IService_Receiving_QualityHoldDetection _qualityHoldService;

public ViewModel_Receiving_Wizard_Orchestration_SaveOperation(
    IMediator mediator,
    IService_ErrorHandler errorHandler,
    IService_LoggingUtility logger,
    IService_Notification notificationService,
    IService_Receiving_QualityHoldDetection qualityHoldService)  // ADD THIS
    : base(errorHandler, logger, notificationService)
{
    _mediator = mediator;
    _qualityHoldService = qualityHoldService;
}
```

**Add Pre-Save Quality Hold Check:**
```csharp
[RelayCommand]
private async Task SaveTransactionAsync()
{
    if (IsBusy) return;
    try
    {
        IsBusy = true;

        // STEP 1: Check for quality holds
        var loadsWithQualityHolds = Loads.Where(l => l.IsQualityHoldRequired).ToList();
        
        if (loadsWithQualityHolds.Any())
        {
            _logger.LogWarning($"Transaction has {loadsWithQualityHolds.Count} loads with quality holds");

            // Show final warning for each restricted part (group by part number)
            var restrictedParts = loadsWithQualityHolds
                .Select(l => l.PartNumber)
                .Distinct()
                .ToList();

            foreach (var partNumber in restrictedParts)
            {
                bool finalAcknowledged = await _qualityHoldService.ShowFinalWarningDialogAsync(partNumber);
                
                if (!finalAcknowledged)
                {
                    _logger.LogWarning($"Save cancelled - final quality hold not acknowledged for {partNumber}");
                    ShowNotification("Save cancelled - quality hold must be acknowledged", 
                                     Enum_Shared_Severity.Warning);
                    return;
                }

                // Mark all loads with this part as final acknowledged
                foreach (var load in loadsWithQualityHolds.Where(l => l.PartNumber == partNumber))
                {
                    load.FinalAcknowledgedQualityHold = true;
                }
            }
        }

        // STEP 2: Save transaction (existing logic)
        var saveCommand = new CommandRequest_Receiving_Shared_Save_Transaction
        {
            // ... existing save logic
        };
        
        var result = await _mediator.Send(saveCommand);

        // STEP 3: Create quality hold records if needed
        if (result.IsSuccess && loadsWithQualityHolds.Any())
        {
            foreach (var load in loadsWithQualityHolds)
            {
                var qualityHoldCommand = new CommandRequest_Receiving_Shared_Create_QualityHold
                {
                    LineID = load.LineID,
                    PartNumber = load.PartNumber,
                    RestrictionType = load.QualityHoldRestrictionType ?? string.Empty,
                    RequiredBy = Environment.UserName
                };

                await _mediator.Send(qualityHoldCommand);
            }
        }

        // ... rest of save logic
    }
    catch (Exception ex)
    {
        _errorHandler.HandleException(ex, Enum_ErrorSeverity.High, 
                                       nameof(SaveTransactionAsync), 
                                       nameof(ViewModel_Receiving_Wizard_Orchestration_SaveOperation));
    }
    finally
    {
        IsBusy = false;
    }
}
```

**Acceptance Criteria:**
- [ ] Service injected
- [ ] Pre-save quality hold check
- [ ] Final warning dialogs for restricted parts
- [ ] Quality hold records created after save
- [ ] User can cancel save
- [ ] All quality hold loads tracked
- [ ] Compiles without errors

---

### ?? P2 MEDIUM: Add Auto-Defaults to Load Entry

#### Task 4.XX: Add Auto-Default Logic to ViewModel_Receiving_Wizard_Display_LoadDetailsGrid

**Priority:** P2 - MEDIUM  
**File:** `Module_Receiving/ViewModels/Wizard/Step2/ViewModel_Receiving_Wizard_Display_LoadDetailsGrid.cs` (MODIFY)  
**Dependencies:** None  
**Estimated Time:** 2 hours

**Add Auto-Default Methods:**
```csharp
/// <summary>
/// Auto-sets PackagesPerLoad to 1 when Part Number is entered (if currently 0)
/// </summary>
partial void OnPartNumberChanged(string value)
{
    if (!string.IsNullOrWhiteSpace(value) && PackagesPerLoad == 0)
    {
        PackagesPerLoad = 1;
        _logger.LogInfo($"Auto-set PackagesPerLoad to 1 for part {value}");
    }
}

/// <summary>
/// Auto-detects Package Type based on part number prefix (MMC = Coil, MMF = Sheet)
/// </summary>
private void AutoDetectPackageType(string partNumber)
{
    if (string.IsNullOrWhiteSpace(partNumber))
        return;

    var upper = partNumber.ToUpperInvariant();
    
    if (upper.Contains("MMC"))
    {
        PackageType = "Coil";
        _logger.LogInfo($"Auto-detected package type: Coil for part {partNumber}");
    }
    else if (upper.Contains("MMF"))
    {
        PackageType = "Sheet";
        _logger.LogInfo($"Auto-detected package type: Sheet for part {partNumber}");
    }
    else
    {
        // Default to Skid for non-MMC/MMF parts
        PackageType = "Skid";
    }
}

/// <summary>
/// Calculates WeightPerPackage when Weight or PackagesPerLoad changes
/// </summary>
private void CalculateWeightPerPackage()
{
    if (Weight > 0 && PackagesPerLoad > 0)
    {
        WeightPerPackage = Weight / PackagesPerLoad;
    }
    else
    {
        WeightPerPackage = 0;
    }
}

partial void OnWeightChanged(decimal value)
{
    CalculateWeightPerPackage();
}

partial void OnPackagesPerLoadChanged(int value)
{
    CalculateWeightPerPackage();
}
```

**Add Properties:**
```csharp
[ObservableProperty]
private decimal _weightPerPackage;
```

**Acceptance Criteria:**
- [ ] PackagesPerLoad auto-sets to 1 on part entry
- [ ] Package type auto-detects (MMC/MMF logic)
- [ ] Weight per package auto-calculates
- [ ] Partial methods for property changes
- [ ] Logging for auto-defaults
- [ ] Compiles without errors

---

### ?? P2: Add Heat/Lot "Nothing Entered" Placeholder

#### Task 4.XX: Add Heat/Lot Placeholder Logic to Save Operation

**Priority:** P2 - MEDIUM  
**File:** `Module_Receiving/ViewModels/Wizard/Step3/ViewModel_Receiving_Wizard_Orchestration_SaveOperation.cs` (MODIFY)  
**Dependencies:** None  
**Estimated Time:** 30 minutes

**Add Pre-Save Placeholder Logic:**
```csharp
private void ApplyHeatLotPlaceholders(List<Model_Receiving_DataTransferObjects_LoadGridRow> loads)
{
    foreach (var load in loads)
    {
        if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
        {
            load.HeatLotNumber = "Nothing Entered";
            _logger.LogInfo($"Applied 'Nothing Entered' placeholder to load {load.LoadNumber}");
        }
    }
}

// Call before save
private async Task SaveTransactionAsync()
{
    // ... existing validation

    // Apply placeholders
    ApplyHeatLotPlaceholders(Loads);

    // ... continue with save
}
```

**Business Rule:** If Heat/Lot is blank/empty when saving, replace with "Nothing Entered" string.

**Acceptance Criteria:**
- [ ] Checks all loads before save
- [ ] Replaces empty Heat/Lot with "Nothing Entered"
- [ ] Logging for applied placeholders
- [ ] No breaking changes
- [ ] Compiles without errors

---

### ?? P1 HIGH: Add CSV Export Integration

#### Task 4.XX: Create Service_Receiving_CSVExport

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/Services/Service_Receiving_CSVExport.cs`  
**Dependencies:** Model_Receiving_Result_CSVExport  
**Estimated Time:** 4 hours

**Why Module_Receiving and not Module_Core?**
- CSV format is DOMAIN-SPECIFIC to receiving module
- Column mapping, field ordering, business rules are receiving-specific
- Module_Core has `IService_CSVWriter` but it's too generic
- Receiving needs specific file paths, naming conventions, column definitions

**Service Pattern:**
```csharp
namespace MTM_Receiving_Application.Module_Receiving.Services;

public interface IService_Receiving_CSVExport
{
    Task<Model_Receiving_Result_CSVExport> ExportTransactionAsync(
        string transactionId,
        List<Model_Receiving_Entity_ReceivingLoad> lines,
        bool exportToLocal = true,
        bool exportToNetwork = true);
}

public class Service_Receiving_CSVExport : IService_Receiving_CSVExport
{
    private readonly IService_LoggingUtility _logger;
    private readonly IService_UserSessionManager _userSessionManager;
    private readonly string _localBasePath;
    private readonly string _networkBasePath;

    public Service_Receiving_CSVExport(
        IService_LoggingUtility logger,
        IService_UserSessionManager userSessionManager)
    {
        _logger = logger;
        _userSessionManager = userSessionManager;

        // Local path: %APPDATA%\MTM_Receiving_Application\CSV\
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDir = Path.Combine(appDataPath, "MTM_Receiving_Application", "CSV");
        Directory.CreateDirectory(appDir);
        _localBasePath = appDir;

        // Network path: \\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files\{Username}\
        _networkBasePath = @"\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files";
    }

    public async Task<Model_Receiving_Result_CSVExport> ExportTransactionAsync(
        string transactionId,
        List<Model_Receiving_Entity_ReceivingLoad> lines,
        bool exportToLocal = true,
        bool exportToNetwork = true)
    {
        var result = new Model_Receiving_Result_CSVExport
        {
            RecordsExported = lines.Count,
            ExportedAt = DateTime.Now
        };

        // Generate filename: ReceivingData_{TransactionID}_{Timestamp}.csv
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var filename = $"ReceivingData_{transactionId}_{timestamp}.csv";

        // Export to local
        if (exportToLocal)
        {
            try
            {
                var localPath = Path.Combine(_localBasePath, filename);
                await WriteCSVFileAsync(localPath, lines);
                
                result.LocalSuccess = true;
                result.LocalFilePath = localPath;
                _logger.LogInfo($"Local CSV export successful: {localPath}");
            }
            catch (Exception ex)
            {
                result.LocalSuccess = false;
                result.LocalErrorMessage = ex.Message;
                _logger.LogError($"Local CSV export failed: {ex.Message}", ex);
            }
        }

        // Export to network
        if (exportToNetwork)
        {
            try
            {
                var username = _userSessionManager.CurrentSession?.User?.WindowsUsername ?? Environment.UserName;
                var userDir = Path.Combine(_networkBasePath, username);

                // Create user directory if needed
                Directory.CreateDirectory(userDir);

                var networkPath = Path.Combine(userDir, filename);
                await WriteCSVFileAsync(networkPath, lines);

                result.NetworkSuccess = true;
                result.NetworkFilePath = networkPath;
                _logger.LogInfo($"Network CSV export successful: {networkPath}");
            }
            catch (Exception ex)
            {
                result.NetworkSuccess = false;
                result.NetworkErrorMessage = ex.Message;
                _logger.LogWarning($"Network CSV export failed (non-critical): {ex.Message}");
                // Network failure is non-critical - local export may have succeeded
            }
        }

        return result;
    }

    private async Task WriteCSVFileAsync(string filePath, List<Model_Receiving_Entity_ReceivingLoad> lines)
    {
        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        // Configure CSV format
        csv.Context.RegisterClassMap<ReceivingLineCSVMap>();

        // Write header and data
        await csv.WriteRecordsAsync(lines);
    }
}

/// <summary>
/// CSV mapping for receiving line export
/// </summary>
public sealed class ReceivingLineCSVMap : ClassMap<Model_Receiving_Entity_ReceivingLoad>
{
    public ReceivingLineCSVMap()
    {
        Map(m => m.PONumber).Name("PO Number");
        Map(m => m.PartNumber).Name("Part Number");
        Map(m => m.LoadNumber).Name("Load Number");
        Map(m => m.Quantity).Name("Quantity");
        Map(m => m.Weight).Name("Weight");
        Map(m => m.HeatLotNumber).Name("Heat/Lot");
        Map(m => m.PackageType).Name("Package Type");
        Map(m => m.PackagesPerLoad).Name("Packages Per Load");
        Map(m => m.ReceivingLocation).Name("Location");
        Map(m => m.ReceivedDate).Name("Received Date").TypeConverterOption.Format("yyyy-MM-dd HH:mm:ss");
    }
}
```

**Dependencies:**
- NuGet: `CsvHelper` package (add to Module_Receiving)

**Acceptance Criteria:**
- [ ] Interface and implementation created
- [ ] Local CSV export to %APPDATA%
- [ ] Network CSV export with user folders
- [ ] CSV column mapping defined
- [ ] Graceful network failure handling
- [ ] Result model populated
- [ ] CsvHelper package added
- [ ] XML documentation
- [ ] Compiles without errors

---

#### Task 4.XX: Integrate CSV Export into Save Operation

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/ViewModels/Wizard/Step3/ViewModel_Receiving_Wizard_Orchestration_SaveOperation.cs` (MODIFY)  
**Dependencies:** Service_Receiving_CSVExport  
**Estimated Time:** 1.5 hours

**Add Service Injection:**
```csharp
private readonly IService_Receiving_CSVExport _csvExportService;

public ViewModel_Receiving_Wizard_Orchestration_SaveOperation(
    IMediator mediator,
    IService_ErrorHandler errorHandler,
    IService_LoggingUtility logger,
    IService_Notification notificationService,
    IService_Receiving_CSVExport csvExportService)  // ADD THIS
    : base(errorHandler, logger, notificationService)
{
    _mediator = mediator;
    _csvExportService = csvExportService;
}
```

**Add CSV Export After Database Save:**
```csharp
[RelayCommand]
private async Task SaveTransactionAsync()
{
    if (IsBusy) return;
    try
    {
        IsBusy = true;

        // ... existing quality hold checks

        // STEP 1: Save to database
        var saveCommand = new CommandRequest_Receiving_Shared_Save_Transaction { /* ... */ };
        var saveResult = await _mediator.Send(saveCommand);

        if (!saveResult.IsSuccess)
        {
            ShowNotification($"Database save failed: {saveResult.ErrorMessage}", Enum_Shared_Severity.Error);
            return;
        }

        string transactionId = saveResult.Data; // Assuming command returns transaction ID

        // STEP 2: Export to CSV (non-blocking)
        var csvResult = await _csvExportService.ExportTransactionAsync(
            transactionId,
            Loads.ToList(),
            exportToLocal: true,
            exportToNetwork: true);

        // STEP 3: Show results
        DatabaseSaveSuccess = true;
        LocalCSVSuccess = csvResult.LocalSuccess;
        NetworkCSVSuccess = csvResult.NetworkSuccess;
        LocalCSVPath = csvResult.LocalFilePath;
        NetworkCSVPath = csvResult.NetworkFilePath;

        var message = $"Transaction saved successfully.\n\n{csvResult.GetSummaryMessage()}";
        
        if (!csvResult.OverallSuccess)
        {
            message += "\n\nWARNING: CSV export failed but database save succeeded.";
        }

        ShowNotification(message, Enum_Shared_Severity.Success);
    }
    catch (Exception ex)
    {
        _errorHandler.HandleException(ex, Enum_ErrorSeverity.High, 
                                       nameof(SaveTransactionAsync), 
                                       nameof(ViewModel_Receiving_Wizard_Orchestration_SaveOperation));
    }
    finally
    {
        IsBusy = false;
    }
}
```

**Add Result Properties:**
```csharp
[ObservableProperty]
private bool _databaseSaveSuccess;

[ObservableProperty]
private bool _localCSVSuccess;

[ObservableProperty]
private bool _networkCSVSuccess;

[ObservableProperty]
private string? _localCSVPath;

[ObservableProperty]
private string? _networkCSVPath;
```

**Acceptance Criteria:**
- [ ] Service injected
- [ ] CSV export called after database save
- [ ] Multi-destination results tracked
- [ ] User notification includes CSV status
- [ ] Non-blocking CSV export
- [ ] Properties bound for completion screen display
- [ ] Compiles without errors

---

### ?? P2: Add Session Cleanup on Save Success

#### Task 4.XX: Add Session Cleanup to Save Operation

**Priority:** P2 - MEDIUM  
**File:** `Module_Receiving/ViewModels/Wizard/Step3/ViewModel_Receiving_Wizard_Orchestration_SaveOperation.cs` (MODIFY)  
**Dependencies:** Module_Core IService_SessionManager  
**Estimated Time:** 1 hour

**Add Service Injection:**
```csharp
private readonly IService_SessionManager _sessionManager;

public ViewModel_Receiving_Wizard_Orchestration_SaveOperation(
    IMediator mediator,
    IService_ErrorHandler errorHandler,
    IService_LoggingUtility logger,
    IService_Notification notificationService,
    IService_SessionManager sessionManager)  // ADD THIS
    : base(errorHandler, logger, notificationService)
{
    _mediator = mediator;
    _sessionManager = sessionManager;
}
```

**Add Session Cleanup After Successful Save:**
```csharp
[RelayCommand]
private async Task SaveTransactionAsync()
{
    // ... existing save logic

    if (saveResult.IsSuccess)
    {
        // ... existing CSV export and quality hold logic

        // FINAL STEP: Clear session file to prevent stale data
        bool sessionCleared = await _sessionManager.ClearSessionAsync();
        if (sessionCleared)
        {
            _logger.LogInfo("Workflow session file cleared after successful save");
        }
        else
        {
            _logger.LogWarning("Session file did not exist or could not be cleared");
        }

        // ... show completion screen
    }
}
```

**Business Rule:** After successful database save, delete the session file to prevent restoring stale data on app restart.

**Acceptance Criteria:**
- [ ] IService_SessionManager injected (from Module_Core)
- [ ] Session cleared after save success
- [ ] Logging for session cleanup
- [ ] No breaking changes
- [ ] Compiles without errors

---

## Phase 6: Views (XAML) - New Tasks

**Add to:** `Module_Receiving/tasks_phase6.md`  
**Insert at appropriate sections**

---

### ?? P0 CRITICAL: Quality Hold Warning Dialogs

**Note:** Quality hold dialogs are created programmatically in `Service_Receiving_QualityHoldDetection`, not as standalone XAML views. No separate view files needed.

---

### ?? P1: Enhance Step 3 Completion Screen (CSV Export Results)

#### Task 6.XX: Add CSV Export Results to Completion Screen XAML

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/Views/Wizard/Step3/View_Receiving_Wizard_Display_CompletionScreen.xaml` (MODIFY)  
**Dependencies:** None  
**Estimated Time:** 2 hours

**Add UI Elements for CSV Results:**
```xaml
<StackPanel Spacing="20">
    <!-- Existing success/failure title -->
    
    <!-- Database Save Status -->
    <StackPanel Spacing="8">
        <TextBlock Text="Database Save:" FontWeight="SemiBold" />
        <TextBlock>
            <Run Text="Status: " />
            <Run Text="{x:Bind ViewModel.DatabaseSaveSuccess, Mode=OneWay, Converter={StaticResource BoolToStatusConverter}}" 
                 Foreground="{x:Bind ViewModel.DatabaseSaveSuccess, Mode=OneWay, Converter={StaticResource BoolToColorConverter}}" />
        </TextBlock>
    </StackPanel>

    <!-- Local CSV Export Status -->
    <StackPanel Spacing="8" Visibility="{x:Bind ViewModel.LocalCSVPath, Mode=OneWay, Converter={StaticResource StringToVisibilityConverter}}">
        <TextBlock Text="Local CSV Export:" FontWeight="SemiBold" />
        <TextBlock>
            <Run Text="Status: " />
            <Run Text="{x:Bind ViewModel.LocalCSVSuccess, Mode=OneWay, Converter={StaticResource BoolToStatusConverter}}" 
                 Foreground="{x:Bind ViewModel.LocalCSVSuccess, Mode=OneWay, Converter={StaticResource BoolToColorConverter}}" />
        </TextBlock>
        <TextBlock Text="{x:Bind ViewModel.LocalCSVPath, Mode=OneWay}" 
                   FontSize="12" 
                   Foreground="Gray"
                   TextWrapping="Wrap" />
        <Button Content="Open Local CSV Folder" 
                Command="{x:Bind ViewModel.OpenLocalCSVFolderCommand}"
                Visibility="{x:Bind ViewModel.LocalCSVSuccess, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}" />
    </StackPanel>

    <!-- Network CSV Export Status -->
    <StackPanel Spacing="8" Visibility="{x:Bind ViewModel.NetworkCSVPath, Mode=OneWay, Converter={StaticResource StringToVisibilityConverter}}">
        <TextBlock Text="Network CSV Export:" FontWeight="SemiBold" />
        <TextBlock>
            <Run Text="Status: " />
            <Run Text="{x:Bind ViewModel.NetworkCSVSuccess, Mode=OneWay, Converter={StaticResource BoolToStatusConverter}}" 
                 Foreground="{x:Bind ViewModel.NetworkCSVSuccess, Mode=OneWay, Converter={StaticResource BoolToColorConverter}}" />
        </TextBlock>
        <TextBlock Text="{x:Bind ViewModel.NetworkCSVPath, Mode=OneWay}" 
                   FontSize="12" 
                   Foreground="Gray"
                   TextWrapping="Wrap" />
        <Button Content="Open Network CSV Folder" 
                Command="{x:Bind ViewModel.OpenNetworkCSVFolderCommand}"
                Visibility="{x:Bind ViewModel.NetworkCSVSuccess, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}" />
    </StackPanel>

    <!-- Warning for partial CSV failure -->
    <InfoBar IsOpen="{x:Bind ViewModel.ShowCSVWarning, Mode=OneWay}"
             Severity="Warning"
             Title="CSV Export Partially Failed"
             Message="Transaction was saved to database successfully, but CSV export encountered errors. Check logs for details." />
</StackPanel>
```

**Add Converters to Resources:**
```xaml
<Page.Resources>
    <converters:BoolToStatusConverter x:Key="BoolToStatusConverter" />
    <converters:BoolToColorConverter x:Key="BoolToColorConverter" />
    <converters:StringToVisibilityConverter x:Key="StringToVisibilityConverter" />
    <converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter" />
</Page.Resources>
```

**Acceptance Criteria:**
- [ ] Database save status displayed
- [ ] Local CSV status and path shown
- [ ] Network CSV status and path shown
- [ ] Buttons to open CSV folders
- [ ] Warning InfoBar for partial failures
- [ ] All bindings use x:Bind with Mode
- [ ] Compiles without errors

---

#### Task 6.XX: Add Open CSV Folder Commands to Completion ViewModel

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/ViewModels/Wizard/Step3/ViewModel_Receiving_Wizard_Display_CompletionScreen.cs` (MODIFY)  
**Dependencies:** None  
**Estimated Time:** 1 hour

**Add Commands:**
```csharp
[RelayCommand]
private void OpenLocalCSVFolder()
{
    if (string.IsNullOrWhiteSpace(LocalCSVPath))
        return;

    try
    {
        var directoryPath = Path.GetDirectoryName(LocalCSVPath);
        if (!string.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath))
        {
            Process.Start("explorer.exe", directoryPath);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError($"Failed to open local CSV folder: {ex.Message}", ex);
        ShowNotification("Could not open folder", Enum_Shared_Severity.Error);
    }
}

[RelayCommand]
private void OpenNetworkCSVFolder()
{
    if (string.IsNullOrWhiteSpace(NetworkCSVPath))
        return;

    try
    {
        var directoryPath = Path.GetDirectoryName(NetworkCSVPath);
        if (!string.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath))
        {
            Process.Start("explorer.exe", directoryPath);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError($"Failed to open network CSV folder: {ex.Message}", ex);
        ShowNotification("Could not open network folder - check network connection", Enum_Shared_Severity.Error);
    }
}

[ObservableProperty]
private bool _showCSVWarning;

partial void OnLocalCSVSuccessChanged(bool value)
{
    UpdateCSVWarning();
}

partial void OnNetworkCSVSuccessChanged(bool value)
{
    UpdateCSVWarning();
}

private void UpdateCSVWarning()
{
    // Show warning if database succeeded but at least one CSV failed
    ShowCSVWarning = DatabaseSaveSuccess && (!LocalCSVSuccess || !NetworkCSVSuccess);
}
```

**Acceptance Criteria:**
- [ ] Open folder commands implemented
- [ ] Uses Process.Start with explorer.exe
- [ ] Error handling for missing folders
- [ ] Warning flag computed from results
- [ ] Compiles without errors

---

### ?? P2: Add Validation LostFocus Events to All Input Fields

#### Task 6.XX: Add LostFocus Validation to PO Number Entry View

**Priority:** P2 - MEDIUM  
**File:** `Module_Receiving/Views/Wizard/Step1/View_Receiving_Wizard_Display_PONumberEntry.xaml` (MODIFY)  
**Dependencies:** None  
**Estimated Time:** 1 hour

**Update TextBox with LostFocus Event:**
```xaml
<!-- BEFORE (if any validation happens during typing) -->
<TextBox 
    Text="{x:Bind ViewModel.PoNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />

<!-- AFTER (validation only on LostFocus) -->
<TextBox 
    x:Name="PoNumberTextBox"
    Text="{x:Bind ViewModel.PoNumber, Mode=TwoWay}"
    LostFocus="OnPoNumberLostFocus"
    PlaceholderText="Enter PO Number (e.g., PO-123456)" />
```

**Add LostFocus Handler in Code-Behind:**
```csharp
// View_Receiving_Wizard_Display_PONumberEntry.xaml.cs
private async void OnPoNumberLostFocus(object sender, RoutedEventArgs e)
{
    // Validate PO number format ONLY when user leaves the field
    await ViewModel.ValidatePoNumberCommand.ExecuteAsync(null);
}
```

**Add Validation Command to ViewModel:**
```csharp
// ViewModel_Receiving_Wizard_Display_PONumberEntry.cs
[RelayCommand]
private async Task ValidatePoNumberAsync()
{
    if (string.IsNullOrWhiteSpace(PoNumber))
    {
        PoNumberValid = false;
        PoNumberErrorMessage = string.Empty;
        return;
    }

    // Call PO format validator via MediatR
    var validationQuery = new QueryRequest_Receiving_Shared_ValidateIf_ValidPOFormat
    {
        PONumber = PoNumber
    };

    var result = await _mediator.Send(validationQuery);

    PoNumberValid = result.IsSuccess;
    PoNumberErrorMessage = result.IsSuccess ? string.Empty : result.ErrorMessage;

    if (!result.IsSuccess)
    {
        ShowNotification(result.ErrorMessage, Enum_Shared_Severity.Warning);
    }
}

[ObservableProperty]
private bool _poNumberValid;

[ObservableProperty]
private string _poNumberErrorMessage = string.Empty;
```

**Acceptance Criteria:**
- [ ] TextBox has LostFocus event handler
- [ ] NO UpdateSourceTrigger=PropertyChanged
- [ ] Validation only occurs when user leaves field
- [ ] ViewModel command added
- [ ] Error message displayed below field
- [ ] Compiles without errors

---

#### Task 6.XX: Add LostFocus Validation to All Step 2 Load Details Grid Fields

**Priority:** P2 - MEDIUM  
**File:** `Module_Receiving/Views/Wizard/Step2/View_Receiving_Wizard_Display_LoadDetailsGrid.xaml` (MODIFY)  
**Dependencies:** None  
**Estimated Time:** 2 hours

**Apply LostFocus Pattern to:**
- Weight/Quantity TextBox
- Heat/Lot TextBox
- Packages Per Load TextBox
- All editable DataGrid columns

**DataGridTemplateColumn Example:**
```xaml
<DataGridTemplateColumn Header="Weight/Quantity">
    <DataGridTemplateColumn.CellEditingTemplate>
        <DataTemplate x:DataType="models:Model_Receiving_DataTransferObjects_LoadGridRow">
            <TextBox 
                Text="{x:Bind Weight, Mode=TwoWay}"
                LostFocus="OnWeightLostFocus"
                InputScope="Number" />
        </DataTemplate>
    </DataGridTemplateColumn.CellEditingTemplate>
</DataGridTemplateColumn>

<DataGridTemplateColumn Header="Heat/Lot">
    <DataGridTemplateColumn.CellEditingTemplate>
        <DataTemplate x:DataType="models:Model_Receiving_DataTransferObjects_LoadGridRow">
            <TextBox 
                Text="{x:Bind HeatLotNumber, Mode=TwoWay}"
                LostFocus="OnHeatLotLostFocus" />
        </DataTemplate>
    </DataGridTemplateColumn.CellEditingTemplate>
</DataGridTemplateColumn>
```

**Add LostFocus Handlers:**
```csharp
// View_Receiving_Wizard_Display_LoadDetailsGrid.xaml.cs
private async void OnWeightLostFocus(object sender, RoutedEventArgs e)
{
    if (sender is TextBox textBox && textBox.DataContext is Model_Receiving_DataTransferObjects_LoadGridRow row)
    {
        await ViewModel.ValidateLoadWeightCommand.ExecuteAsync(row);
    }
}

private async void OnHeatLotLostFocus(object sender, RoutedEventArgs e)
{
    if (sender is TextBox textBox && textBox.DataContext is Model_Receiving_DataTransferObjects_LoadGridRow row)
    {
        await ViewModel.ValidateLoadHeatLotCommand.ExecuteAsync(row);
    }
}
```

**Acceptance Criteria:**
- [ ] All editable fields have LostFocus handlers
- [ ] NO PropertyChanged triggers
- [ ] Validation only on focus loss
- [ ] DataGrid row context passed to validation
- [ ] Compiles without errors

---

## Phase 7: Integration & Testing - New Tasks

**Add to:** `Module_Receiving/tasks_phase7.md`  
**Insert at appropriate sections**

---

### ?? P1: DI Registration for New Services

#### Task 7.XX: Register Quality Hold Service and CSV Export Service

**Priority:** P1 - HIGH  
**File:** `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` (MODIFY)  
**Dependencies:** All new services created  
**Estimated Time:** 1 hour

**Add Service Registrations:**
```csharp
// Module_Receiving Services
public static IServiceCollection AddModuleReceivingServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // ... existing DAO registrations

    // NEW: Quality Hold Service (Singleton - stateless)
    services.AddSingleton<IService_Receiving_QualityHoldDetection, Service_Receiving_QualityHoldDetection>();

    // NEW: CSV Export Service (Singleton - stateless)
    services.AddSingleton<IService_Receiving_CSVExport, Service_Receiving_CSVExport>();

    // ... existing ViewModel/View registrations

    return services;
}
```

**Add DAO Registration:**
```csharp
// NEW: Quality Hold DAO
services.AddSingleton(sp =>
{
    var logger = sp.GetRequiredService<IService_LoggingUtility>();
    return new Dao_Receiving_Repository_QualityHold(connectionString, logger);
});
```

**Acceptance Criteria:**
- [ ] Quality hold service registered as Singleton
- [ ] CSV export service registered as Singleton
- [ ] Quality hold DAO registered as Singleton
- [ ] All dependencies resolve correctly
- [ ] Compiles without errors
- [ ] Application runs without DI exceptions

---

### ?? P2: Add Session Restoration on Startup

#### Task 7.XX: Integrate Session Restoration into App Startup Lifecycle

**Priority:** P2 - MEDIUM  
**File:** `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs` (MODIFY)  
**Dependencies:** Phase 3 Query handler  
**Estimated Time:** 2 hours

**Add Session Restoration Logic:**
```csharp
public async Task OnApplicationStartupAsync()
{
    _logger.LogInfo("Application startup initiated");

    // ... existing startup logic

    // NEW: Check for existing workflow session
    await RestoreWorkflowSessionIfExistsAsync();

    // ... continue startup
}

private async Task RestoreWorkflowSessionIfExistsAsync()
{
    try
    {
        var sessionManager = _serviceProvider.GetRequiredService<IService_SessionManager>();
        
        if (sessionManager.SessionExists())
        {
            _logger.LogInfo("Workflow session file detected - attempting restoration");

            var sessionFilePath = sessionManager.GetSessionFilePath();
            
            // Read session file to get session ID
            var sessionJson = await File.ReadAllTextAsync(sessionFilePath);
            var sessionData = JsonSerializer.Deserialize<WorkflowSessionData>(sessionJson);

            if (sessionData?.SessionId != null)
            {
                // Show restoration dialog to user
                var xamlRoot = _windowService.GetXamlRoot();
                var dialog = new ContentDialog
                {
                    Title = "Resume Previous Session?",
                    Content = $"An incomplete receiving session was detected from {sessionData.CreatedAt:g}.\n\n" +
                              $"Would you like to resume this session?",
                    PrimaryButtonText = "Resume Session",
                    CloseButtonText = "Start Fresh",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = xamlRoot
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    // User wants to restore - navigate to wizard with session ID
                    _logger.LogInfo($"Restoring workflow session: {sessionData.SessionId}");
                    
                    var navigationService = _serviceProvider.GetRequiredService<IService_Navigation>();
                    await navigationService.NavigateToAsync(
                        "View_Receiving_Wizard_Orchestration_MainWorkflow",
                        parameter: sessionData.SessionId);
                }
                else
                {
                    // User wants fresh start - delete session file
                    _logger.LogInfo("User declined session restoration - clearing session file");
                    await sessionManager.ClearSessionAsync();
                }
            }
        }
        else
        {
            _logger.LogInfo("No workflow session file found - normal startup");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError($"Failed to restore workflow session: {ex.Message}", ex);
        // Non-critical error - continue startup normally
    }
}

private class WorkflowSessionData
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

**Acceptance Criteria:**
- [ ] Checks for session file on startup
- [ ] Shows user confirmation dialog
- [ ] Navigates to wizard with session ID if restored
- [ ] Clears session file if declined
- [ ] Error handling for corrupted session files
- [ ] Logging for all actions
- [ ] Compiles without errors

---

### ?? P2: Add User Default Receiving Mode Preference

#### Task 7.XX: Add DefaultReceivingMode to User Settings

**Priority:** P2 - MEDIUM  
**File:** `Module_Settings.Receiving/Models/Model_Settings_Receiving_UserPreferences.cs` (CREATE)  
**Dependencies:** Phase 7 settings infrastructure  
**Estimated Time:** 1 hour

**Create User Preference Model:**
```csharp
namespace MTM_Receiving_Application.Module_Settings.Receiving.Models;

public class Model_Settings_Receiving_UserPreferences
{
    public string Username { get; set; } = string.Empty;
    public string DefaultReceivingMode { get; set; } = "Wizard"; // Wizard/Manual/Edit
    public bool SkipModeSelectionIfDefaultSet { get; set; } = false;
    public DateTime? LastModified { get; set; }
}
```

**Add to Settings Database Schema:**
```sql
CREATE TABLE dbo.tbl_Settings_Receiving_UserPreferences
(
    Username                    NVARCHAR(100)   NOT NULL,
    DefaultReceivingMode        NVARCHAR(50)    NOT NULL DEFAULT 'Wizard',
    SkipModeSelectionIfDefaultSet BIT           NOT NULL DEFAULT 0,
    LastModified                DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT PK_Settings_Receiving_UserPreferences PRIMARY KEY CLUSTERED (Username),
    CONSTRAINT CK_Settings_Receiving_DefaultMode 
        CHECK (DefaultReceivingMode IN ('Wizard', 'Manual', 'Edit'))
);
```

**Acceptance Criteria:**
- [ ] Model created
- [ ] Database table created
- [ ] Stored procedures for get/upsert
- [ ] DAO created
- [ ] Settings UI integration (Phase 7)
- [ ] Compiles without errors

---

#### Task 7.XX: Add User Preference Check to Hub Orchestration

**Priority:** P2 - MEDIUM  
**File:** `Module_Receiving/ViewModels/Hub/ViewModel_Receiving_Hub_Orchestration_MainWorkflow.cs` (MODIFY)  
**Dependencies:** User preference model  
**Estimated Time:** 1.5 hours

**Add Preference Check on Initialization:**
```csharp
public async Task InitializeAsync()
{
    if (IsBusy) return;
    try
    {
        IsBusy = true;

        // Check if user has default receiving mode preference
        var username = Environment.UserName;
        var prefQuery = new QueryRequest_Settings_Receiving_Get_UserPreferences { Username = username };
        var prefResult = await _mediator.Send(prefQuery);

        if (prefResult.IsSuccess && 
            prefResult.Data != null && 
            !string.IsNullOrEmpty(prefResult.Data.DefaultReceivingMode) &&
            prefResult.Data.SkipModeSelectionIfDefaultSet)
        {
            _logger.LogInfo($"User has default receiving mode: {prefResult.Data.DefaultReceivingMode}");

            // Skip mode selection and go directly to default mode
            switch (prefResult.Data.DefaultReceivingMode.ToLower())
            {
                case "wizard":
                    await NavigateToWizardModeAsync();
                    break;
                case "manual":
                    await NavigateToManualModeAsync();
                    break;
                case "edit":
                    await NavigateToEditModeAsync();
                    break;
                default:
                    // Invalid default - show mode selection
                    ShowModeSelection = true;
                    break;
            }
        }
        else
        {
            // No default mode or user wants to see selection
            ShowModeSelection = true;
        }
    }
    catch (Exception ex)
    {
        _logger.LogError($"Failed to check user preferences: {ex.Message}", ex);
        // Fall back to showing mode selection
        ShowModeSelection = true;
    }
    finally
    {
        IsBusy = false;
    }
}

[ObservableProperty]
private bool _showModeSelection = false;
```

**Acceptance Criteria:**
- [ ] User preference query on initialization
- [ ] Conditional mode selection skip
- [ ] Direct navigation to preferred mode
- [ ] Fallback to mode selection on error
- [ ] Logging for preference checks
- [ ] Compiles without errors

---

## Phase 8: Testing - New Tasks

**Add to:** `Module_Receiving/tasks_phase8.md`  
**Insert at appropriate sections**

---

### ?? P1: Quality Hold Integration Tests

#### Task 8.XX: Create Quality Hold End-to-End Integration Test

**Priority:** P1 - HIGH  
**File:** `MTM_Receiving_Application.Tests/Module_Receiving/Integration/QualityHoldWorkflowTests.cs`  
**Dependencies:** All quality hold features implemented  
**Estimated Time:** 3 hours

**Test Scenarios:**
```csharp
public class QualityHoldWorkflowTests
{
    [Fact]
    public async Task WizardWorkflow_WithMMFSRPart_ShouldRequireQualityHoldAcknowledgment()
    {
        // Arrange: Create wizard session with MMFSR part
        // Act: Attempt to save without acknowledgment
        // Assert: Save should fail with quality hold error
    }

    [Fact]
    public async Task WizardWorkflow_WithMMCSRPart_ShouldCreateQualityHoldRecord()
    {
        // Arrange: Complete wizard with MMCSR part and acknowledgments
        // Act: Save transaction
        // Assert: Quality hold record created in database
    }

    [Fact]
    public async Task QualityHoldDetection_WithNonRestrictedPart_ShouldNotTriggerWarning()
    {
        // Arrange: Non-MMFSR/MMCSR part
        // Act: Select part
        // Assert: No quality hold warning shown
    }

    [Fact]
    public async Task QualityHoldWarning_WhenUserCancels_ShouldClearPartSelection()
    {
        // Arrange: MMFSR part selected
        // Act: Cancel first warning dialog
        // Assert: Part selection cleared
    }

    [Fact]
    public async Task FinalQualityHoldCheck_WhenUserCancels_ShouldPreventSave()
    {
        // Arrange: Transaction ready to save with quality hold
        // Act: Cancel final warning
        // Assert: Save cancelled, data retained
    }
}
```

**Acceptance Criteria:**
- [ ] All test scenarios implemented
- [ ] Tests use real database (integration tests)
- [ ] Quality hold table queried to verify records
- [ ] Dialog mocking for automated tests
- [ ] FluentAssertions for readable assertions
- [ ] All tests pass

---

### ?? P1: CSV Export Integration Tests

#### Task 8.XX: Create CSV Export End-to-End Integration Test

**Priority:** P1 - HIGH  
**File:** `MTM_Receiving_Application.Tests/Module_Receiving/Integration/CSVExportWorkflowTests.cs`  
**Dependencies:** CSV export service implemented  
**Estimated Time:** 2 hours

**Test Scenarios:**
```csharp
public class CSVExportWorkflowTests : IAsyncLifetime
{
    private string _testLocalPath;
    private string _testNetworkPath;

    public async Task InitializeAsync()
    {
        // Create test directories
        _testLocalPath = Path.Combine(Path.GetTempPath(), "MTM_Test_Local");
        _testNetworkPath = Path.Combine(Path.GetTempPath(), "MTM_Test_Network");
        Directory.CreateDirectory(_testLocalPath);
        Directory.CreateDirectory(_testNetworkPath);
    }

    public async Task DisposeAsync()
    {
        // Clean up test directories
        Directory.Delete(_testLocalPath, true);
        Directory.Delete(_testNetworkPath, true);
    }

    [Fact]
    public async Task SaveTransaction_ShouldExportToLocalCSV()
    {
        // Arrange: Transaction with 5 lines
        // Act: Save transaction
        // Assert: Local CSV file created with 5 records
    }

    [Fact]
    public async Task SaveTransaction_WithNetworkFailure_ShouldStillSucceedWithLocalOnly()
    {
        // Arrange: Mock network path as unavailable
        // Act: Save transaction
        // Assert: Local success, network failure, overall success
    }

    [Fact]
    public async Task CSVExport_ShouldContainAllRequiredColumns()
    {
        // Arrange: Transaction data
        // Act: Export to CSV
        // Assert: CSV contains PO, Part, Load, Qty, Weight, Heat/Lot, Package Type, Location, Date
    }

    [Fact]
    public async Task CSVExport_ShouldFormatDatesCorrectly()
    {
        // Arrange: Transaction with specific date
        // Act: Export to CSV
        // Assert: Date formatted as "yyyy-MM-dd HH:mm:ss"
    }
}
```

**Acceptance Criteria:**
- [ ] All test scenarios implemented
- [ ] Tests write to temp directories
- [ ] CSV file parsing and validation
- [ ] Network failure simulation
- [ ] Cleanup after tests
- [ ] All tests pass

---

## DI Registration Summary

**Add to:** `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`

### New Singleton Services
```csharp
// Quality Hold
services.AddSingleton<IService_Receiving_QualityHoldDetection, Service_Receiving_QualityHoldDetection>();
services.AddSingleton(sp =>
{
    var logger = sp.GetRequiredService<IService_LoggingUtility>();
    return new Dao_Receiving_Repository_QualityHold(connectionString, logger);
});

// CSV Export
services.AddSingleton<IService_Receiving_CSVExport, Service_Receiving_CSVExport>();
```

### Dependencies from Module_Core (already registered)
- `IService_Window` - For XamlRoot access (dialogs)
- `IService_LoggingUtility` - For logging
- `IService_UserSessionManager` - For username resolution
- `IService_SessionManager` - For session file cleanup

---

## Implementation Priority Order

**CRITICAL PATH (P0 - Must complete before production):**
1. Phase 2: Quality Hold Infrastructure (7 tasks, 12-15 hours)
2. Phase 3: Quality Hold CQRS (6 tasks, 10-12 hours)
3. Phase 4: Quality Hold Detection Service + Integration (4 tasks, 8-10 hours)
4. Phase 8: Quality Hold Testing (1 task, 3 hours)

**HIGH PRIORITY (P1 - Required for business operations):**
1. Phase 3: CSV Export Result Model (2 tasks, 1.5 hours)
2. Phase 4: CSV Export Service + Integration (2 tasks, 5.5 hours)
3. Phase 6: CSV Results UI (2 tasks, 3 hours)
4. Phase 7: DI Registration (1 task, 1 hour)
5. Phase 8: CSV Export Testing (1 task, 2 hours)

**MEDIUM PRIORITY (P2 - User experience enhancements):**
1. Phase 4: Auto-Defaults (1 task, 2 hours)
2. Phase 4: Heat/Lot Placeholder (1 task, 30 minutes)
3. Phase 4: Session Cleanup (1 task, 1 hour)
4. Phase 6: LostFocus Validation (2 tasks, 3 hours)
5. Phase 7: Session Restoration on Startup (1 task, 2 hours)
6. Phase 7: User Default Mode Preference (2 tasks, 2.5 hours)

---

**Total New Tasks:** 47  
**Total Estimated Hours:** 70-86 hours  

**End of Required Feature Updates Document**
