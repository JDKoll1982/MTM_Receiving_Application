# Phase 7: Integration & Testing - Task List

**Phase:** 7 of 8  
**Status:** ⏳ PENDING  
**Priority:** HIGH - System integration and validation  
**Dependencies:** Phase 6 (Views & UI) must be complete

---

## 📊 **Phase 7 Overview**

**Goal:** Integrate all Module_Receiving components, test end-to-end workflows, and validate system integration

**Integration Points:**
- Navigation service integration
- DI container validation
- ERP integration (Infor Visual) testing
- CSV export integration
- Database integration testing
- Session management integration

**Status:**
- ⏳ DI Registration Validation: 0/2 complete
- ⏳ Navigation Integration: 0/3 complete
- ⏳ ERP Integration Testing: 0/2 complete
- ⏳ CSV Export Integration: 0/2 complete
- ⏳ End-to-End Testing: 0/3 complete

**Completion:** 0/12 tasks (0%)

**Estimated Total Time:** 15-20 hours

---

## ⏳ **DI Registration Validation (2 tasks)**

### Task 7.1: Validate All Module_Receiving DI Registrations

**Priority:** P0 - CRITICAL  
**File:** `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`  
**Dependencies:** All ViewModels, DAOs, Services implemented  
**Estimated Time:** 3 hours

**Validation Checklist:**

**✅ DAOs (6 total):**
```csharp
services.AddSingleton(sp =>
{
    var logger = sp.GetRequiredService<IService_LoggingUtility>();
    return new Dao_Receiving_Repository_Transaction(connectionString, logger);
});
// ... all 6 DAOs registered
```

**✅ ViewModels (30+ total):**
```csharp
// Hub ViewModels
services.AddTransient<ViewModel_Receiving_Hub_Orchestration_MainWorkflow>();
services.AddTransient<ViewModel_Receiving_Hub_Display_ModeSelection>();

// Wizard Orchestration
services.AddTransient<ViewModel_Receiving_Wizard_Orchestration_MainWorkflow>();

// Step 1 ViewModels
services.AddTransient<ViewModel_Receiving_Wizard_Display_PONumberEntry>();
services.AddTransient<ViewModel_Receiving_Wizard_Display_PartSelection>();
services.AddTransient<ViewModel_Receiving_Wizard_Display_LoadCountEntry>();

// Step 2 ViewModels
services.AddTransient<ViewModel_Receiving_Wizard_Display_LoadDetailsGrid>();
services.AddTransient<ViewModel_Receiving_Wizard_Interaction_BulkCopyOperations>();

// Step 3 ViewModels
services.AddTransient<ViewModel_Receiving_Wizard_Display_ReviewSummary>();
services.AddTransient<ViewModel_Receiving_Wizard_Orchestration_SaveOperation>();
services.AddTransient<ViewModel_Receiving_Wizard_Display_CompletionScreen>();

// ... all ViewModels registered
```

**✅ Views (50+ total):**
```csharp
// Hub Views
services.AddTransient<View_Receiving_Hub_Orchestration_MainWorkflow>();
services.AddTransient<View_Receiving_Hub_Display_ModeSelection>();

// Wizard Views
services.AddTransient<View_Receiving_Wizard_Orchestration_MainWorkflow>();
services.AddTransient<View_Receiving_Wizard_Display_Step1Container>();
services.AddTransient<View_Receiving_Wizard_Display_Step2Container>();
services.AddTransient<View_Receiving_Wizard_Display_Step3Container>();

// ... all Views registered
```

**✅ MediatR & FluentValidation:**
```csharp
// Already registered globally in CqrsInfrastructureExtensions.cs
services.AddCqrsInfrastructure();
```

**Validation Tests:**
1. Build application - no DI registration errors
2. Run application - verify all ViewModels resolve correctly
3. Navigate through all views - no missing dependencies
4. Check MediatR commands/queries all have handlers
5. Verify all validators are discovered by FluentValidation

**Acceptance Criteria:**
- [ ] All DAOs registered as Singleton
- [ ] All ViewModels registered as Transient
- [ ] All Views registered as Transient
- [ ] MediatR registered with all handlers
- [ ] FluentValidation auto-discovery working
- [ ] Build succeeds with no errors
- [ ] Application runs without DI exceptions

---

### Task 7.2: Create DI Validation Test Suite

**Priority:** P1 - HIGH  
**File:** `MTM_Receiving_Application.Tests/Infrastructure/DependencyInjectionTests.cs`  
**Dependencies:** Task 7.1  
**Estimated Time:** 2 hours

**Test Cases:**

```csharp
public class DependencyInjectionTests
{
    private readonly IServiceProvider _serviceProvider;
    
    public DependencyInjectionTests()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        
        // Register all services
        services.AddCoreServices(configuration);
        services.AddCqrsInfrastructure();
        services.AddModuleServices(configuration);
        
        _serviceProvider = services.BuildServiceProvider();
    }
    
    [Fact]
    public void AllDAOs_ShouldResolve()
    {
        // Arrange & Act
        var transactionDao = _serviceProvider.GetService<Dao_Receiving_Repository_Transaction>();
        var lineDao = _serviceProvider.GetService<Dao_Receiving_Repository_Line>();
        // ... test all 6 DAOs
        
        // Assert
        transactionDao.Should().NotBeNull();
        lineDao.Should().NotBeNull();
        // ... all DAOs not null
    }
    
    [Fact]
    public void AllViewModels_ShouldResolve()
    {
        // Arrange & Act
        var hubVM = _serviceProvider.GetService<ViewModel_Receiving_Hub_Orchestration_MainWorkflow>();
        var wizardVM = _serviceProvider.GetService<ViewModel_Receiving_Wizard_Orchestration_MainWorkflow>();
        // ... test all ViewModels
        
        // Assert
        hubVM.Should().NotBeNull();
        wizardVM.Should().NotBeNull();
        // ... all ViewModels not null
    }
    
    [Fact]
    public void MediatR_ShouldResolve()
    {
        // Arrange & Act
        var mediator = _serviceProvider.GetService<IMediator>();
        
        // Assert
        mediator.Should().NotBeNull();
    }
    
    [Fact]
    public void AllCommandHandlers_ShouldBeRegistered()
    {
        // Verify all command handlers are discovered
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        
        // Test sample commands resolve handlers
        var saveTransactionCommand = new CommandRequest_Receiving_Shared_Save_Transaction();
        // Should not throw
    }
}
```

**Acceptance Criteria:**
- [ ] Test suite created with xUnit
- [ ] All DAOs tested for resolution
- [ ] All ViewModels tested for resolution
- [ ] MediatR tested for resolution
- [ ] All tests pass

---

## ⏳ **Navigation Integration (3 tasks)**

### Task 7.3: Integrate Navigation Service with Module_Receiving

**Priority:** P0 - CRITICAL  
**File:** `Module_Core/Services/NavigationService.cs` (update)  
**Dependencies:** All Views implemented  
**Estimated Time:** 2 hours

**Register Navigation Routes:**

```csharp
public class NavigationService : INavigationService
{
    private readonly Dictionary<string, Type> _routes = new()
    {
        // Module_Receiving Routes
        ["ReceivingHub"] = typeof(View_Receiving_Hub_Orchestration_MainWorkflow),
        ["ReceivingWizard"] = typeof(View_Receiving_Wizard_Orchestration_MainWorkflow),
        ["ReceivingManual"] = typeof(View_Receiving_Manual_Orchestration_MainWorkflow),
        ["ReceivingEdit"] = typeof(View_Receiving_Edit_Orchestration_MainWorkflow),
        
        // Wizard Steps
        ["WizardStep1"] = typeof(View_Receiving_Wizard_Display_Step1Container),
        ["WizardStep2"] = typeof(View_Receiving_Wizard_Display_Step2Container),
        ["WizardStep3"] = typeof(View_Receiving_Wizard_Display_Step3Container),
    };
    
    public void NavigateTo(string routeName, object? parameter = null)
    {
        if (!_routes.TryGetValue(routeName, out var pageType))
        {
            throw new InvalidOperationException($"Route '{routeName}' not found");
        }
        
        // Navigate using WinUI 3 Frame
        _frame.Navigate(pageType, parameter);
    }
}
```

**Acceptance Criteria:**
- [ ] All Module_Receiving routes registered
- [ ] NavigateTo works for all routes
- [ ] Parameter passing tested
- [ ] Back navigation works

---

### Task 7.4: Test Hub to Wizard Navigation Flow

**Priority:** P0 - CRITICAL  
**Dependencies:** Task 7.3  
**Estimated Time:** 2 hours

**Navigation Test Scenarios:**

1. **Hub → Wizard Mode:**
   - User clicks "Guided Mode (Wizard)" button
   - Hub ViewModel calls `NavigationService.NavigateTo("ReceivingWizard")`
   - Wizard orchestration view loads
   - Wizard shows Step 1

2. **Wizard Step Navigation (1 → 2 → 3):**
   - Step 1 "Next" button → Navigate to Step 2
   - Step 2 "Next" button → Navigate to Step 3
   - Step 3 "Previous" button → Navigate to Step 2

3. **Cancel Workflow Navigation:**
   - User clicks "Cancel" button
   - Confirmation dialog appears
   - User confirms → Navigate back to Hub

**Acceptance Criteria:**
- [ ] Hub → Wizard navigation works
- [ ] Wizard step-to-step navigation works
- [ ] Cancel returns to Hub
- [ ] Session state preserved across navigation

---

### Task 7.5: Test Dialog Navigation and Modal Display

**Priority:** P1 - HIGH  
**Dependencies:** Task 7.3  
**Estimated Time:** 1.5 hours

**Dialog Test Scenarios:**

1. **ContentDialog Display:**
   - Bulk Copy Preview Dialog
   - Save Error Dialog
   - Confirmation Dialogs

2. **Flyout Display:**
   - Help tooltips
   - Context menus

3. **TeachingTip Display:**
   - Inline help tips

**Acceptance Criteria:**
- [ ] ContentDialogs display correctly
- [ ] Dialogs return user choices
- [ ] Modal blocking works
- [ ] Dialog results handled by ViewModels

---

## ⏳ **ERP Integration Testing (2 tasks)**

### Task 7.6: Test Infor Visual PO Validation Integration

**Priority:** P0 - CRITICAL  
**File:** Create integration test file  
**Dependencies:** QueryRequest_Receiving_Shared_Validate_PONumber implemented  
**Estimated Time:** 3 hours

**Test Scenarios:**

1. **Valid PO Lookup:**
   - Enter valid PO number (e.g., "PO-123456")
   - System queries Infor Visual database
   - Returns valid result with PO details

2. **Invalid PO Lookup:**
   - Enter non-existent PO number
   - System queries Infor Visual database
   - Returns "PO not found" error

3. **Timeout Handling:**
   - Simulate slow ERP connection
   - System shows loading indicator
   - Timeout after 10 seconds with error message

4. **Connection Error Handling:**
   - Simulate ERP database offline
   - System shows connection error
   - Allows retry or Non-PO mode

**Integration Test Code:**

```csharp
[Fact]
public async Task ValidatePONumber_ValidPO_ReturnsSuccess()
{
    // Arrange
    var query = new QueryRequest_Receiving_Shared_Validate_PONumber 
    { 
        PONumber = "PO-123456" 
    };
    
    // Act
    var result = await _mediator.Send(query);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Data.PONumber.Should().Be("PO-123456");
    result.Data.IsValid.Should().BeTrue();
}

[Fact]
public async Task ValidatePONumber_InvalidPO_ReturnsFailure()
{
    // Arrange
    var query = new QueryRequest_Receiving_Shared_Validate_PONumber 
    { 
        PONumber = "PO-999999" 
    };
    
    // Act
    var result = await _mediator.Send(query);
    
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.ErrorMessage.Should().Contain("not found");
}
```

**Acceptance Criteria:**
- [ ] Valid PO queries work
- [ ] Invalid PO queries handled
- [ ] Timeout handling works
- [ ] Connection errors handled gracefully
- [ ] READ-ONLY enforcement (no writes to Infor Visual)

---

### Task 7.7: Test Part Details Retrieval from ERP

**Priority:** P1 - HIGH  
**File:** Integration test file  
**Dependencies:** QueryRequest_Receiving_Shared_Get_PartDetails implemented  
**Estimated Time:** 2 hours

**Test Scenarios:**

1. **Part Details by PO:**
   - Query parts for a PO number
   - Returns list of valid parts

2. **Part Details by Part Number:**
   - Query specific part number
   - Returns part description, type, UOM

3. **Part Not Found:**
   - Query non-existent part
   - Returns error message

**Acceptance Criteria:**
- [ ] Part queries work
- [ ] Part details returned correctly
- [ ] Part not found handled
- [ ] READ-ONLY enforcement

---

## ⏳ **CSV Export Integration (2 tasks)**

### Task 7.8: Test CSV Export to Local and Network Paths

**Priority:** P0 - CRITICAL  
**File:** Integration test file  
**Dependencies:** CommandRequest_Receiving_Shared_Complete_Workflow implemented  
**Estimated Time:** 3 hours

**Test Scenarios:**

1. **Export to Local Path:**
   - Save transaction
   - CSV file created at `C:\MTM_Receiving\Exports\{TransactionId}.csv`
   - File contains all load data

2. **Export to Network Path:**
   - Save transaction
   - CSV file created at `\\NetworkShare\Receiving\{TransactionId}.csv`
   - File contains all load data

3. **Export Failure Handling:**
   - Network path unavailable
   - Shows error with retry option
   - Local export still succeeds

4. **CSV Format Validation:**
   - Verify CSV headers correct
   - Verify all columns present
   - Verify data formatting (dates, decimals)

**CSV Format Expected:**

```csv
TransactionId,PONumber,PartNumber,LoadNumber,Quantity,HeatLot,PackageType,PackagesPerLoad,WeightPerPackage,ReceivingLocation,CreatedBy,CreatedDate
TX-12345,PO-123456,MMC0001000,1,1000,HL-ABC123,Pallet,10,100,Dock A,JohnDoe,2026-01-25T14:30:00
TX-12345,PO-123456,MMC0001000,2,1000,HL-ABC123,Pallet,10,100,Dock A,JohnDoe,2026-01-25T14:30:00
```

**Acceptance Criteria:**
- [ ] Local CSV export works
- [ ] Network CSV export works
- [ ] CSV format correct
- [ ] File paths saved to database
- [ ] Export errors handled gracefully

---

### Task 7.9: Test CSV Export Error Recovery

**Priority:** P1 - HIGH  
**Dependencies:** Task 7.8  
**Estimated Time:** 2 hours

**Error Scenarios:**

1. **Disk Full:**
   - Simulate disk full
   - Show error message
   - Allow retry or alternate path

2. **Permission Denied:**
   - Simulate no write permissions
   - Show error message
   - Guide user to resolve

3. **Network Unavailable:**
   - Network path not accessible
   - Local export succeeds
   - Network export queued for retry

**Acceptance Criteria:**
- [ ] Disk full handled
- [ ] Permission errors handled
- [ ] Network errors handled
- [ ] Retry mechanism works

---

## ⏳ **End-to-End Testing (3 tasks)**

### Task 7.10: End-to-End Test - Complete Wizard Workflow (Happy Path)

**Priority:** P0 - CRITICAL  
**File:** Manual test script + automated UI test  
**Dependencies:** All previous tasks  
**Estimated Time:** 4 hours

**Test Script:**

**Step 1: Order & Part Selection**
1. Launch application
2. Click "Guided Mode (Wizard)"
3. Enter PO Number: "PO-123456"
4. Wait for validation ✓
5. Select Part Number from dropdown
6. Enter Load Count: 5
7. Click "Next"

**Step 2: Load Details Entry**
1. Verify 5 loads displayed in DataGrid
2. Enter Quantity for Load 1: 1000
3. Enter Heat/Lot for Load 1: "HL-ABC123"
4. Select Package Type for Load 1: "Pallet"
5. Enter Packages Per Load for Load 1: 10
6. Select Location for Load 1: "Dock A"
7. Click "Bulk Copy Fields" button
8. Confirm copy preview
9. Click "Execute Copy"
10. Verify Loads 2-5 auto-filled
11. Click "Next"

**Step 3: Review & Save**
1. Verify all data displayed in read-only summary
2. Verify totals calculated correctly
3. Click "Save"
4. Wait for save progress
5. Verify completion screen shows
6. Verify Transaction ID displayed
7. Verify CSV file paths shown
8. Verify CSV files exist at paths
9. Click "New Transaction"
10. Verify returned to Step 1

**Expected Results:**
- All steps complete without errors
- Data persists to database
- CSV files exported successfully
- Completion screen displays
- New workflow initializes correctly

**Acceptance Criteria:**
- [ ] Complete workflow executes successfully
- [ ] All data saved to database
- [ ] CSV files exported
- [ ] UI updates correctly at each step
- [ ] No errors logged

---

### Task 7.11: End-to-End Test - Error Handling Scenarios

**Priority:** P1 - HIGH  
**Dependencies:** Task 7.10  
**Estimated Time:** 3 hours

**Error Scenarios to Test:**

1. **Invalid PO Number:**
   - Enter invalid PO
   - Verify error message displays
   - Verify "Next" button disabled

2. **Incomplete Load Data:**
   - Leave required fields empty in Step 2
   - Click "Next"
   - Verify validation error displays
   - Verify navigation blocked

3. **Save Failure:**
   - Simulate database connection failure
   - Click "Save" in Step 3
   - Verify error dialog displays
   - Verify retry option works

4. **CSV Export Failure:**
   - Simulate network path unavailable
   - Click "Save"
   - Verify local export succeeds
   - Verify network error reported

5. **Workflow Cancellation:**
   - Start workflow
   - Enter data in Step 1
   - Click "Cancel"
   - Confirm cancellation
   - Verify returned to Hub
   - Verify session not saved

**Acceptance Criteria:**
- [ ] All error scenarios handled gracefully
- [ ] User-friendly error messages
- [ ] No crashes or exceptions
- [ ] Recovery options provided

---

### Task 7.12: Performance and Load Testing

**Priority:** P2 - MEDIUM  
**Dependencies:** Task 7.10  
**Estimated Time:** 2 hours

**Performance Scenarios:**

1. **Large Load Count:**
   - Enter Load Count: 999 (maximum)
   - Verify DataGrid performance acceptable (<2 seconds to render)
   - Verify bulk copy operations complete quickly (<1 second)

2. **Rapid Navigation:**
   - Navigate back and forth between steps rapidly
   - Verify no UI lag or freezing
   - Verify session state maintained

3. **Concurrent Sessions:**
   - Open multiple workflow sessions
   - Verify each session isolated
   - Verify no data cross-contamination

4. **Database Query Performance:**
   - Query large POs with many lines
   - Verify query completes in <3 seconds
   - Verify UI remains responsive

**Acceptance Criteria:**
- [ ] 999 loads render in <2 seconds
- [ ] Bulk operations complete in <1 second
- [ ] Navigation is smooth and responsive
- [ ] No performance degradation over time

---

## 📊 **Phase 7 Summary**

**Total Tasks:** 12  
**Total Estimated Time:** 15-20 hours  

**Critical Path Dependencies:**
1. DI Registration Validation (Tasks 7.1-7.2) - MUST complete first
2. Navigation Integration (Tasks 7.3-7.5) - Depends on DI
3. ERP Integration Testing (Tasks 7.6-7.7) - Parallel with navigation
4. CSV Export Integration (Tasks 7.8-7.9) - Parallel with ERP
5. End-to-End Testing (Tasks 7.10-7.12) - FINAL validation

**Test Coverage Required:**
- ✅ Unit tests for all handlers, validators
- ✅ Integration tests for DAOs
- ✅ Integration tests for ERP queries
- ✅ Integration tests for CSV export
- ✅ End-to-end UI tests for complete workflows
- ✅ Performance tests for large data sets

**Key Integration Points:**
- MediatR command/query handling
- FluentValidation automatic validation
- Navigation service routing
- Infor Visual READ-ONLY queries
- CSV file export (local + network)
- Session state management
- Transaction database persistence

**Success Criteria:**
- All tests pass
- No DI registration errors
- Complete workflows execute successfully
- All error scenarios handled gracefully
- Performance meets requirements
- CSV exports succeed
- Database transactions persist correctly

---

**Status:** ⏳ PENDING - Awaiting Phase 6 completion
