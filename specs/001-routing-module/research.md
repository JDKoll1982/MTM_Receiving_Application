# Research & Technical Decisions: Routing Module

**Feature**: 001-routing-module  
**Date**: 2026-01-04  
**Status**: Complete

This document captures technology decisions, best practices, and architectural patterns for implementing the Internal Routing Module.

## Phase 0: Research Findings

### 1. Wizard Workflow Navigation Pattern

**Decision**: Use UserControl-based wizard steps with ViewModel-driven navigation

**Rationale**:
- WinUI 3 NavigationView is designed for top-level navigation, not sequential wizard flows
- UserControl composition allows embedding wizard steps within a parent Page
- ViewModel manages wizard state (current step, navigation commands, data flow)
- Enables clean separation: each step is a self-contained XAML+ViewModel

**Implementation Pattern**:
```csharp
// RoutingWizardViewModel (parent)
public partial class RoutingWizardViewModel : BaseViewModel
{
    [ObservableProperty]
    private int _currentStep = 1;
    
    [ObservableProperty]
    private UserControl? _currentStepView;
    
    [RelayCommand]
    private void NextStep()
    {
        CurrentStep++;
        LoadStepView();
    }
    
    private void LoadStepView()
    {
        CurrentStepView = CurrentStep switch
        {
            1 => new RoutingWizardStep1View { DataContext = _step1ViewModel },
            2 => new RoutingWizardStep2View { DataContext = _step2ViewModel },
            3 => new RoutingWizardStep3View { DataContext = _step3ViewModel },
            _ => null
        };
    }
}
```

**Alternatives Considered**:
- **NavigationView**: Rejected - designed for persistent left-nav, not linear wizard
- **ContentDialog multi-page**: Rejected - poor UX for complex multi-step forms
- **Separate Pages per step**: Rejected - unnecessary navigation overhead, state management complexity

---

### 2. Quick Add Buttons: Personalized vs. System-Wide

**Decision**: Hybrid approach - personalize after 20 labels, otherwise system-wide top 5

**Rationale**:
- New users need instant value from Quick Add (system-wide most popular recipients)
- Experienced users benefit from personalization (their own usage patterns)
- 20-label threshold provides statistically significant personalization data
- System-wide fallback ensures consistent UX for all users

**SQL Query Pattern**:
```sql
-- Get personalized Quick Add recipients
SELECT r.id, r.name, COUNT(l.id) AS usage_count
FROM routing_recipients r
INNER JOIN routing_labels l ON r.id = l.recipient_id
WHERE l.created_by = @employee_number
  AND r.is_active = 1
GROUP BY r.id, r.name
ORDER BY usage_count DESC
LIMIT 5;

-- If <20 labels, fallback to system-wide
SELECT r.id, r.name, SUM(ut.usage_count) AS total_usage
FROM routing_recipients r
INNER JOIN routing_usage_tracking ut ON r.id = ut.recipient_id
WHERE r.is_active = 1
GROUP BY r.id, r.name
ORDER BY total_usage DESC
LIMIT 5;
```

**Alternatives Considered**:
- **Always personalized**: Rejected - poor UX for new users (empty Quick Add buttons)
- **Always system-wide**: Rejected - doesn't leverage user's specific workflow patterns
- **Configurable threshold**: Rejected - added complexity for minimal benefit

---

### 3. CSV Export: Real-Time vs. Batch

**Decision**: Real-time export with async retry mechanism

**Rationale**:
- LabelView integration requires immediate CSV availability for printing
- Users expect instant feedback ("label created → ready to print")
- Async retry handles file lock scenarios gracefully
- Database save is primary persistence layer (CSV is secondary export)

**Implementation Pattern**:
```csharp
public async Task<Model_Dao_Result> CreateLabelAsync(Model_RoutingLabel label)
{
    // 1. Save to database (critical path)
    var dbResult = await _labelDao.InsertAsync(label);
    if (!dbResult.IsSuccess) return dbResult;
    
    // 2. Export to CSV (best effort, async retry)
    _ = Task.Run(async () =>
    {
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                await File.AppendAllTextAsync(_csvPath, FormatCsvLine(label));
                break;
            }
            catch (IOException) when (attempt < 3)
            {
                await Task.Delay(500 * attempt); // Backoff: 500ms, 1s, 1.5s
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"CSV export failed after {attempt} attempts", ex);
            }
        }
    });
    
    return dbResult;
}
```

**Alternatives Considered**:
- **Batch export (hourly/daily)**: Rejected - breaks LabelView integration requirement
- **Synchronous retry**: Rejected - blocks UI thread, poor UX during file lock
- **Message queue**: Rejected - over-engineering for 10-50 labels/day volume

---

### 4. Recipient Smart Sorting: Client-Side vs. Database

**Decision**: Database-side sorting with ViewModel-side filtering

**Rationale**:
- SQL `ORDER BY usage_count DESC` is faster than C# sorting for 50-200 recipients
- Database calculates usage count per user (personalized sorting)
- ViewModel applies real-time search filter to pre-sorted list
- ObservableCollection updates trigger UI refresh automatically

**Implementation Pattern**:
```csharp
// Service layer (database sort)
public async Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetRecipientsSortedByUsageAsync(int employeeNumber)
{
    var parameters = new Dictionary<string, object>
    {
        { "employee_number", employeeNumber }
    };
    
    return await _recipientDao.GetSortedByUsageAsync(parameters);
}

// ViewModel (client-side filter)
[ObservableProperty]
private string _searchText = string.Empty;

partial void OnSearchTextChanged(string value)
{
    FilteredRecipients.Clear();
    var filtered = string.IsNullOrWhiteSpace(value)
        ? AllRecipients // Already sorted by database
        : AllRecipients.Where(r => r.Name.Contains(value, StringComparison.OrdinalIgnoreCase));
    
    foreach (var recipient in filtered)
        FilteredRecipients.Add(recipient);
}
```

**Alternatives Considered**:
- **Client-side sorting**: Rejected - slower for large lists, redundant DB query
- **Database-side filtering**: Rejected - requires new query per keystroke (network overhead)

---

### 5. Infor Visual Connection Failure Handling

**Decision**: Graceful degradation - allow "OTHER" PO workflow when Visual is unreachable

**Rationale**:
- Infor Visual availability is ~95% (maintenance windows, network issues)
- Business continuity: users can still create labels during outages
- "OTHER" PO workflow provides full functionality (manual description entry)
- Error message guides user to workaround

**Implementation Pattern**:
```csharp
public async Task<Model_Dao_Result<List<Model_InforVisualPOLine>>> GetPOLinesAsync(string poNumber)
{
    try
    {
        using var connection = new SqlConnection(_inforVisualConnectionString);
        await connection.OpenAsync();
        // Query po_detail table
    }
    catch (SqlException ex) when (IsConnectionError(ex))
    {
        return DaoResultFactory.Failure<List<Model_InforVisualPOLine>>(
            "Unable to connect to ERP system. You can still create labels using 'OTHER' PO type.",
            ex,
            Enum_ErrorSeverity.Warning // Not critical - workaround available
        );
    }
}
```

**Alternatives Considered**:
- **Block label creation**: Rejected - poor business continuity
- **Retry loop**: Rejected - delays user workflow, Visual outages are often prolonged
- **Cached PO data**: Rejected - stale data risk, storage complexity

---

### 6. Edit Mode: In-Place vs. Dialog

**Decision**: Dialog-based editing with confirmation

**Rationale**:
- Prevents accidental edits (explicit "Edit" button click required)
- Dialog provides clear edit context (vs. inline grid editing)
- Confirmation step ("Save Changes") prevents mis-clicks
- Audit trail captures edit timestamp and editor ID

**UX Pattern**:
```
[Edit Mode Grid]
  Row: PO-12345 | Part ABC | Recipient: John | Qty: 100 | Created: 2026-01-04

[User clicks "Edit" or double-clicks row]

[Edit Dialog Opens]
  PO Number: PO-12345 (read-only)
  Description: Part ABC
  Recipient: [John ▼]
  Quantity: [100]
  
  [Save Changes] [Cancel]

[User clicks Save]
→ Database UPDATE (not INSERT)
→ routing_label_history INSERT (audit log)
→ Success message: "Label updated. Changes logged for audit."
```

**Alternatives Considered**:
- **Inline editing**: Rejected - high risk of accidental edits (mis-click)
- **Versioning**: Rejected - over-engineering for audit requirement

---

### 7. MySQL 5.7.24 Constraint Workarounds

**Decision**: Use stored procedure logic for constraints, avoid CHECK constraints

**Rationale**:
- MySQL 5.7.24 does NOT support CHECK constraints (introduced in MySQL 8.0.16)
- Stored procedures provide equivalent validation logic
- Application-layer validation adds defense-in-depth
- Performance impact negligible for 10-50 labels/day volume

**Pattern**:
```sql
-- Instead of CHECK constraint
CREATE TABLE routing_labels (
    quantity INT NOT NULL,
    -- CANNOT USE: CHECK (quantity > 0)
);

-- Use stored procedure validation
DELIMITER $$
CREATE PROCEDURE sp_routing_label_insert(
    IN p_quantity INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    IF p_quantity <= 0 THEN
        SET p_status = -1;
        SET p_error_msg = 'Quantity must be greater than zero';
        ROLLBACK;
        LEAVE;
    END IF;
    
    INSERT INTO routing_labels (quantity) VALUES (p_quantity);
    SET p_status = 1;
END $$
```

**Alternatives Considered**:
- **Upgrade to MySQL 8.x**: Rejected - infrastructure constraint (shared server)
- **BEFORE INSERT triggers**: Rejected - less maintainable than stored procedures
- **Application-only validation**: Rejected - no DB-level enforcement (data integrity risk)

---

## Technology Stack Summary

| Category | Technology | Version | Justification |
|----------|------------|---------|---------------|
| **UI Framework** | WinUI 3 | Windows App SDK 1.8+ | Modern Windows desktop, native performance, Material Design support |
| **Language** | C# | 12 / .NET 8.0 | Project standard, async/await, LINQ, record types |
| **MVVM Toolkit** | CommunityToolkit.Mvvm | 8.x | Source generators ([ObservableProperty], [RelayCommand]), industry standard |
| **Database (App)** | MySQL | 8.x (5.7.24 compatible) | Project standard, cost-effective, proven reliability |
| **Database (ERP)** | SQL Server | 2019+ (read-only) | Infor Visual integration, existing infrastructure |
| **CSV Export** | System.IO | .NET 8.0 | Built-in, no external dependencies, simple format |
| **Icons** | Material.Icons.WinUI3 | Latest | Project standard, consistent with Receiving/Dunnage modules |

---

## Best Practices Applied

### 1. Async/Await Throughout
- All database operations are async (`Task<Model_Dao_Result<T>>`)
- CSV writes are fire-and-forget async (non-blocking UI)
- Infor Visual queries are async (network-bound operations)

### 2. Stored Procedures for MySQL
- ALL MySQL CRUD operations via stored procedures (no raw SQL in C#)
- Parameter naming: `p_parameter_name` convention
- OUT parameters for status/error messages

### 3. DAO Pattern Consistency
- All DAOs return `Model_Dao_Result<T>` or `Model_Dao_Result`
- No exceptions thrown from DAO layer
- Connection string via constructor injection

### 4. MVVM Layer Separation
- ViewModels call Services (never DAOs directly)
- Services orchestrate DAOs and business logic
- Views are pure XAML with x:Bind (no code-behind logic)

### 5. Error Handling Strategy
- User-friendly messages via `IService_ErrorHandler`
- Technical details logged via `ILoggingService`
- Severity classification (Low/Medium/High/Critical)

---

## Performance Optimization Decisions

### 1. Infor Visual Query Caching
**Decision**: NO caching (always query live data)

**Rationale**:
- PO data can change frequently (line items added/removed)
- 2-second query time is acceptable UX (95th percentile requirement)
- Cache invalidation complexity not worth 1-second time save

### 2. Recipient List Loading
**Decision**: Load full list once per session, filter client-side

**Rationale**:
- 50-200 recipients fit easily in memory (~10KB)
- One DB query per session vs. query-per-keystroke
- Real-time filtering is instant (<100ms)

### 3. Quick Add Button Calculation
**Decision**: Calculate on page load, cache until navigation

**Rationale**:
- Top 5 calculation is fast (~50ms query)
- Usage counts change infrequently (once per label creation)
- Static buttons prevent confusing mid-workflow changes

---

## Security Considerations

### 1. SQL Injection Prevention
- Parameterized stored procedures for ALL MySQL operations
- No string concatenation in SQL queries
- Infor Visual queries use SqlParameter (never raw strings)

### 2. Infor Visual Read-Only Enforcement
- Connection string includes `ApplicationIntent=ReadOnly`
- Only SELECT queries allowed (no INSERT/UPDATE/DELETE)
- Connection failure handled gracefully (no app crash)

### 3. Audit Trail
- CreatedBy (employee number) captured for all labels
- EditedBy and EditTimestamp logged in routing_label_history
- Soft delete for recipients (is_active flag, never DELETE)

---

## Open Questions & Decisions

### Resolved:
1. ✅ **Q**: Should Quick Add be personalized or system-wide?  
   **A**: Hybrid - personalized after 20 labels, otherwise system-wide

2. ✅ **Q**: How to handle Infor Visual outages?  
   **A**: Graceful degradation - allow "OTHER" PO workflow with user-friendly message

3. ✅ **Q**: In-place editing or dialog for Edit Mode?  
   **A**: Dialog-based to prevent accidental edits, better audit UX

4. ✅ **Q**: Real-time or batch CSV export?  
   **A**: Real-time with async retry (3 attempts, backoff)

5. ✅ **Q**: Client-side or database sorting for recipients?  
   **A**: Database sort, client-side filter (best of both)

### No Clarifications Needed:
- All NEEDS CLARIFICATION items from Technical Context have been resolved
- No unknowns blocking Phase 1 design artifact generation

---

**Status**: ✅ Research Complete - Ready for Phase 1 (data-model, contracts, quickstart)
