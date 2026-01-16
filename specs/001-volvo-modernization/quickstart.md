# Quickstart Guide: Module_Volvo CQRS Modernization

**Feature**: Module_Volvo CQRS Modernization  
**Branch**: `001-volvo-modernization`  
**Date**: January 16, 2026

## Overview

This quickstart guide helps developers begin implementing the Module_Volvo CQRS modernization. Follow these steps to set up your environment, understand the architecture, and start coding.

---

## Prerequisites

### Required Software

- ✅ Visual Studio 2022 17.8+ or VS Code with C# Dev Kit
- ✅ .NET 8.0 SDK
- ✅ MySQL 8.0+ (mtm_receiving_application database)
- ✅ Git (for branch management)

### Required Knowledge

- C# 12 and .NET 8
- MediatR CQRS pattern
- FluentValidation
- WinUI 3 / XAML
- CommunityToolkit.Mvvm

### Recommended Reading

- [specs/001-volvo-modernization/spec.md](spec.md) - Feature specification
- [specs/001-volvo-modernization/research.md](research.md) - Research decisions
- [specs/001-volvo-modernization/data-model.md](data-model.md) - Data model
- [.github/copilot-instructions.md](../../.github/copilot-instructions.md) - Coding standards

---

## Step 1: Environment Setup (10 minutes)

### 1.1 Clone and Switch Branch

```powershell
# Navigate to repo
cd C:\Users\johnk\source\repos\MTM_Receiving_Application

# Ensure on correct branch
git checkout 001-volvo-modernization
git pull origin 001-volvo-modernization

# Verify branch
git branch --show-current  # Should output: 001-volvo-modernization
```

### 1.2 Restore NuGet Packages

```powershell
# Restore packages
dotnet restore

# Verify MediatR and FluentValidation installed
dotnet list package | Select-String "MediatR|FluentValidation"
```

**Expected Output**:

```
   > MediatR                               12.4.1
   > MediatR.Extensions.Microsoft.DependencyInjection    12.4.1
   > FluentValidation                      11.10.0
   > FluentValidation.DependencyInjectionExtensions      11.10.0
```

### 1.3 Database Connection Test

```powershell
# Test MySQL connection
mysql -h localhost -P 3306 -u root -p mtm_receiving_application -e "SELECT COUNT(*) FROM volvo_label_data;"
```

If successful, proceed. If connection fails, check `appsettings.json` connection string.

---

## Step 2: Understand Current Architecture (15 minutes)

### 2.1 Explore Existing Module_Volvo

```powershell
# List ViewModels
ls Module_Volvo\ViewModels\*.cs

# Output:
# ViewModel_Volvo_History.cs
# ViewModel_Volvo_Settings.cs
# ViewModel_Volvo_ShipmentEntry.cs

# List Services (to be deprecated)
ls Module_Volvo\Services\*.cs

# Output:
# Service_Volvo.cs
# Service_VolvoAuthorization.cs
# Service_VolvoMasterData.cs

# List DAOs (already compliant)
ls Module_Volvo\Data\*.cs

# Output:
# Dao_VolvoPart.cs
# Dao_VolvoPartComponent.cs
# Dao_VolvoSettings.cs
# Dao_VolvoShipment.cs
# Dao_VolvoShipmentLine.cs
```

### 2.2 Identify Constitutional Violations

Run grep search to verify violations documented in spec:

```powershell
# Violation 1: ViewModels call services directly (Principle III)
Select-String -Path "Module_Volvo\ViewModels\*.cs" -Pattern "IService_Volvo" | Measure-Object

# Violation 2: {Binding} instead of x:Bind (Principle I)
Select-String -Path "Module_Volvo\Views\*.xaml" -Pattern "\{Binding" | Measure-Object

# Violation 3: No FluentValidation validators (Principle V)
Test-Path "Module_Volvo\Validators" # Should be False
```

**Expected Counts**:

- Violation 1: ~3 service injections
- Violation 2: ~20 binding occurrences
- Violation 3: Validators folder doesn't exist

---

## Step 3: Create Folder Structure (5 minutes)

```powershell
# Create CQRS handler folders
New-Item -ItemType Directory -Path "Module_Volvo\Handlers\Commands" -Force
New-Item -ItemType Directory -Path "Module_Volvo\Handlers\Queries" -Force

# Create request DTO folders
New-Item -ItemType Directory -Path "Module_Volvo\Requests\Commands" -Force
New-Item -ItemType Directory -Path "Module_Volvo\Requests\Queries" -Force

# Create validator folder
New-Item -ItemType Directory -Path "Module_Volvo\Validators" -Force

# Create test project folders
New-Item -ItemType Directory -Path "Module_Volvo.Tests\Handlers\Commands" -Force
New-Item -ItemType Directory -Path "Module_Volvo.Tests\Handlers\Queries" -Force
New-Item -ItemType Directory -Path "Module_Volvo.Tests\Validators" -Force
New-Item -ItemType Directory -Path "Module_Volvo.Tests\Integration" -Force
New-Item -ItemType Directory -Path "Module_Volvo.Tests\PropertyBased" -Force
New-Item -ItemType Directory -Path "Module_Volvo.Tests\GoldenFiles" -Force

# Verify structure
tree Module_Volvo /F
```

---

## Step 4: Implement First Query Handler (30 minutes)

### 4.1 Create Query DTO

**File**: `Module_Volvo/Requests/Queries/GetInitialShipmentDataQuery.cs`

```csharp
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

public record GetInitialShipmentDataQuery : IRequest<Model_Dao_Result<InitialShipmentData>>
{
}

public record InitialShipmentData
{
    public DateTimeOffset CurrentDate { get; init; }
    public int NextShipmentNumber { get; init; }
}
```

### 4.2 Create Query Handler

**File**: `Module_Volvo/Handlers/Queries/GetInitialShipmentDataQueryHandler.cs`

```csharp
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Queries;

public class GetInitialShipmentDataQueryHandler 
    : IRequestHandler<GetInitialShipmentDataQuery, Model_Dao_Result<InitialShipmentData>>
{
    private readonly IService_Volvo _volvoService;
    private readonly IService_LoggingUtility _logger;

    public GetInitialShipmentDataQueryHandler(
        IService_Volvo volvoService,
        IService_LoggingUtility logger)
    {
        _volvoService = volvoService;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<InitialShipmentData>> Handle(
        GetInitialShipmentDataQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInfo("Getting initial shipment data");

        try
        {
            var currentDate = DateTimeOffset.Now;
            
            // Get next shipment number from service (temporary - will move to handler later)
            var shipmentNumberResult = await _volvoService.GetNextShipmentNumberAsync(currentDate);
            
            if (!shipmentNumberResult.IsSuccess)
            {
                return Model_Dao_Result<InitialShipmentData>.Failure(
                    shipmentNumberResult.ErrorMessage);
            }

            var data = new InitialShipmentData
            {
                CurrentDate = currentDate,
                NextShipmentNumber = shipmentNumberResult.Data
            };

            return Model_Dao_Result<InitialShipmentData>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting initial shipment data: {ex.Message}");
            return Model_Dao_Result<InitialShipmentData>.Failure(
                $"Unexpected error: {ex.Message}");
        }
    }
}
```

### 4.3 Create Unit Test

**File**: `Module_Volvo.Tests/Handlers/Queries/GetInitialShipmentDataQueryHandlerTests.cs`

```csharp
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using Xunit;

namespace Module_Volvo.Tests.Handlers.Queries;

public class GetInitialShipmentDataQueryHandlerTests
{
    private readonly Mock<IService_Volvo> _mockVolvoService;
    private readonly Mock<IService_LoggingUtility> _mockLogger;
    private readonly GetInitialShipmentDataQueryHandler _handler;

    public GetInitialShipmentDataQueryHandlerTests()
    {
        _mockVolvoService = new Mock<IService_Volvo>();
        _mockLogger = new Mock<IService_LoggingUtility>();
        _handler = new GetInitialShipmentDataQueryHandler(
            _mockVolvoService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ReturnsInitialData_WhenServiceSucceeds()
    {
        // Arrange
        var expectedShipmentNumber = 42;
        _mockVolvoService
            .Setup(s => s.GetNextShipmentNumberAsync(It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(Model_Dao_Result<int>.Success(expectedShipmentNumber));

        var query = new GetInitialShipmentDataQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.NextShipmentNumber.Should().Be(expectedShipmentNumber);
        result.Data.CurrentDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenServiceFails()
    {
        // Arrange
        var errorMessage = "Database connection failed";
        _mockVolvoService
            .Setup(s => s.GetNextShipmentNumberAsync(It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(Model_Dao_Result<int>.Failure(errorMessage));

        var query = new GetInitialShipmentDataQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
    }
}
```

### 4.4 Run Test

```powershell
# Build solution
dotnet build

# Run specific test
dotnet test --filter "FullyQualifiedName~GetInitialShipmentDataQueryHandlerTests"

# Expected: 2 passed tests
```

---

## Step 5: Refactor ViewModel to Use IMediator (20 minutes)

### 5.1 Update ViewModel Constructor

**File**: `Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs`

**BEFORE**:

```csharp
private readonly IService_Volvo _volvoService;

public ViewModel_Volvo_ShipmentEntry(
    IService_Volvo volvoService,
    IService_ErrorHandler errorHandler,
    IService_LoggingUtility logger,
    IService_Window windowService,
    IService_UserSessionManager sessionManager) : base(errorHandler, logger)
{
    _volvoService = volvoService;
    // ...
}
```

**AFTER**:

```csharp
private readonly IMediator _mediator;
// Keep for gradual migration:
[Obsolete("Use IMediator instead - will be removed after full CQRS migration")]
private readonly IService_Volvo _volvoService;

public ViewModel_Volvo_ShipmentEntry(
    IMediator mediator,
    IService_Volvo volvoService, // Keep temporarily
    IService_ErrorHandler errorHandler,
    IService_LoggingUtility logger,
    IService_Window windowService,
    IService_UserSessionManager sessionManager) : base(errorHandler, logger)
{
    _mediator = mediator;
    _volvoService = volvoService; // Keep for now
    // ...
}
```

### 5.2 Refactor InitializeAsync Method

**BEFORE**:

```csharp
public async Task InitializeAsync()
{
    try
    {
        IsBusy = true;
        var result = await _volvoService.GetInitialShipmentDataAsync();
        if (result.IsSuccess)
        {
            ShipmentDate = result.Data.CurrentDate;
            ShipmentNumber = result.Data.NextShipmentNumber;
        }
    }
    finally { IsBusy = false; }
}
```

**AFTER**:

```csharp
public async Task InitializeAsync()
{
    try
    {
        IsBusy = true;
        
        // CQRS: Query handler replaces service call
        var query = new GetInitialShipmentDataQuery();
        var result = await _mediator.Send(query);
        
        if (result.IsSuccess)
        {
            ShipmentDate = result.Data.CurrentDate;
            ShipmentNumber = result.Data.NextShipmentNumber;
        }
        else
        {
            _errorHandler.ShowUserError(
                result.ErrorMessage,
                "Initialization Error",
                nameof(InitializeAsync));
        }
    }
    catch (Exception ex)
    {
        _errorHandler.HandleException(
            ex,
            Enum_ErrorSeverity.Medium,
            nameof(InitializeAsync),
            nameof(ViewModel_Volvo_ShipmentEntry));
    }
    finally { IsBusy = false; }
}
```

### 5.3 Verify ViewModel Compiles

```powershell
# Build Module_Volvo
dotnet build Module_Volvo --no-restore

# Expected: Build succeeded, 0 errors
```

---

## Step 6: Verify MediatR Pipeline (10 minutes)

### 6.1 Check App.xaml.cs Configuration

**File**: `App.xaml.cs`

Verify MediatR is registered:

```csharp
// Should exist in ConfigureServices():
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Should exist for pipeline behaviors:
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
```

### 6.2 Test End-to-End

```powershell
# Run application
dotnet run --project MTM_Receiving_Application.csproj

# Navigate to Volvo Shipment Entry screen
# Verify screen initializes with current date and next shipment number
# Check logs for MediatR LoggingBehavior output
```

**Expected Log Output**:

```
[INFO] Executing GetInitialShipmentDataQuery
[INFO] Getting initial shipment data
[INFO] GetInitialShipmentDataQuery executed in 45ms
```

---

## Step 7: Golden File Capture (15 minutes)

Before refactoring CSV/email logic, capture golden files for regression testing.

### 7.1 Generate Sample Shipment

```powershell
# Create test shipment via UI:
# 1. Add Part: TEST-001, Skids: 5
# 2. Add Part: TEST-002, Skids: 10
# 3. Complete Shipment
# 4. Save generated label CSV to Module_Volvo.Tests/GoldenFiles/expected_label_basic.csv
```

### 7.2 Capture Email HTML

```powershell
# Use "Preview Email" button
# Copy HTML content to Module_Volvo.Tests/GoldenFiles/expected_email_html.html
# Copy plain text to Module_Volvo.Tests/GoldenFiles/expected_email_text.txt
```

### 7.3 Create Golden File Test

**File**: `Module_Volvo.Tests/GoldenFiles/LabelCsvGoldenFileTests.cs`

```csharp
[Fact]
public async Task GenerateLabelCsv_MatchesGoldenFile_BasicShipment()
{
    // Arrange
    var expectedCsv = await File.ReadAllTextAsync(
        "GoldenFiles/expected_label_basic.csv");
    var query = new GenerateLabelCsvQuery { ShipmentId = TestShipmentId };

    // Act
    var result = await _mediator.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Data.Should().Be(expectedCsv); // Byte-for-byte match
}
```

---

## Next Steps

### Immediate (Phase 2 - Handler Implementation)

1. **Implement Remaining Query Handlers** (11 total)
   - Priority: GenerateLabelCsvQuery, FormatEmailDataQuery (functional parity critical)
   - Follow GetInitialShipmentDataQueryHandler pattern
   - Write unit tests for each handler

2. **Implement Command Handlers** (8 total)
   - Start with CompleteShipmentCommand (P1 priority)
   - Create FluentValidation validators first (TDD approach)
   - Write integration tests (handler → service → DAO → DB)

3. **Refactor Remaining ViewModels**
   - ViewModel_Volvo_History (9 methods)
   - ViewModel_Volvo_Settings (12 methods)
   - Replace service calls with `_mediator.Send()`

### Medium-Term (Phase 3 - View Migration)

1. **Migrate XAML Bindings**
   - Convert DataGridTextColumn to DataGridTemplateColumn
   - Replace `{Binding}` with `x:Bind` + `x:DataType`
   - Test each View after migration

2. **Remove Service Dependencies**
   - Move business logic from Service_Volvo to handlers
   - Mark services as [Obsolete]
   - Remove service registrations from App.xaml.cs

### Long-Term (Phase 4 - Testing & Validation)

1. **Comprehensive Testing**
   - Property-based tests for component explosion
   - Golden file tests for all CSV/email outputs
   - Integration tests for full pipelines
   - Achieve 80%+ code coverage

2. **Performance Validation**
   - Benchmark MediatR overhead (<10ms target)
   - Verify database operation times (<500ms constitutional target)
   - Compare shipment completion time to legacy

---

## Troubleshooting

### Issue: MediatR handler not found

**Symptom**: `InvalidOperationException: No handler registered for query`

**Solution**: Verify handler is public class and Assembly.GetExecutingAssembly() includes it.

```powershell
# Check handler registration
dotnet build --verbosity detailed | Select-String "MediatR"
```

### Issue: FluentValidation not executing

**Symptom**: Invalid commands pass through without validation errors

**Solution**: Verify ValidationBehavior registered in App.xaml.cs:

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

### Issue: Golden file tests fail

**Symptom**: CSV/email content doesn't match golden files

**Solution**: Check for whitespace differences (line endings, trailing spaces):

```powershell
# Compare files with detailed diff
Compare-Object (Get-Content expected.csv) (Get-Content actual.csv)
```

---

## Resources

- **Specification**: [specs/001-volvo-modernization/spec.md](spec.md)
- **Research**: [specs/001-volvo-modernization/research.md](research.md)
- **Data Model**: [specs/001-volvo-modernization/data-model.md](data-model.md)
- **Contracts**: [specs/001-volvo-modernization/contracts/README.md](contracts/README.md)
- **Constitution**: `.specify/memory/constitution.md`
- **MVVM Guide**: `.github/instructions/mvvm-pattern.instructions.md`
- **DAO Guide**: `.github/instructions/dao-pattern.instructions.md`

---

## Getting Help

- **Questions**: Ask in #copilot-questions Slack channel
- **Code Review**: Request review from architecture team
- **Constitutional Issues**: Consult `.specify/memory/constitution.md`

---

**Quickstart Status**: ✅ COMPLETE - Ready for implementation

**Estimated Time to First Handler**: 1 hour  
**Estimated Time to Full Migration**: 40-60 hours (P1: 20h, P2: 20-30h, P3: 10h)
