# Research: Module_Volvo CQRS Modernization

**Feature**: Module_Volvo CQRS Modernization  
**Branch**: `001-volvo-modernization`  
**Date**: January 16, 2026  
**Phase**: 0 - Research & Decision Documentation

## Overview

This document captures research findings and architectural decisions made during the planning phase of Module_Volvo CQRS modernization. All "NEEDS CLARIFICATION" items from the technical context have been resolved through code analysis and constitutional review.

---

## Research Areas

### 1. MediatR CQRS Pattern Implementation

**Question**: How to structure command/query handlers for Volvo module operations while maintaining functional parity?

**Research Findings**:

MediatR 12.4.1 is already installed in Module_Core with pipeline behaviors configured:

- `LoggingBehavior`: Logs execution time for all requests
- `ValidationBehavior`: Automatically validates commands using FluentValidation
- `AuditBehavior`: Captures user context from IService_UserSessionManager

**Decision**:

- Use MediatR `IRequest<TResponse>` pattern for all queries and commands
- Commands return `Model_Dao_Result` or `Model_Dao_Result<T>` to maintain DAO contract consistency
- Queries return `Model_Dao_Result<TData>` where TData is the query result (e.g., `List<Model_VolvoShipment>`, `Model_VolvoEmailData`)
- Request/Response DTOs in `Module_Volvo/Requests/Commands/` and `Module_Volvo/Requests/Queries/`
- Handlers in `Module_Volvo/Handlers/Commands/` and `Module_Volvo/Handlers/Queries/`

**Rationale**:

- Maintains constitutional compliance (Principle III: CQRS + Mediator First)
- Leverages existing Module_Core infrastructure (no duplication)
- Consistent error handling via Model_Dao_Result pattern across all layers
- Pipeline behaviors provide automatic logging, validation, and auditing

**Alternatives Considered**:

- Custom command/query infrastructure: Rejected (violates Principle VII: Library-First Reuse)
- Returning void/bool from commands: Rejected (loses error context needed for IService_ErrorHandler)

---

### 2. FluentValidation Integration for Command Validation

**Question**: How to implement validation for Volvo commands while preserving existing validation rules?

**Research Findings**:

Current validation logic is embedded in services (Service_Volvo.ValidateShipmentAsync) and ViewModels (CanAddPart, CanEditPart methods). FluentValidation 11.10.0 is installed with ValidationBehavior configured to run automatically before command handlers.

**Existing Validation Rules** (extracted from code analysis):

- Shipment validation: At least one part required, shipment date not in future
- Part validation: Part number required, quantity per skid > 0, no duplicate parts in shipment
- Discrepancy validation: If HasDiscrepancy = true, ExpectedSkidCount and DiscrepancyNote required
- CSV import validation: Valid part number format, numeric quantity per skid

**Decision**:

- Create AbstractValidator<TCommand> for each of the 8 commands
- Validators in `Module_Volvo/Validators/` folder
- Use Ardalis.GuardClauses for common null/empty checks (cleaner syntax per Principle VII)
- ValidationBehavior will automatically execute validators before handlers
- Failed validation returns `Model_Dao_Result.Failure()` with concatenated error messages

**Example Pattern**:

```csharp
public class CompleteShipmentCommandValidator : AbstractValidator<CompleteShipmentCommand>
{
    public CompleteShipmentCommandValidator()
    {
        RuleFor(x => x.ShipmentDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTimeOffset.Now)
            .WithMessage("Shipment date cannot be in the future");
            
        RuleFor(x => x.Parts)
            .NotEmpty()
            .WithMessage("Shipment must have at least one part");
            
        RuleForEach(x => x.Parts).ChildRules(part =>
        {
            part.RuleFor(p => p.PartNumber).NotEmpty();
            part.RuleFor(p => p.ReceivedSkidCount).GreaterThan(0);
            part.When(p => p.HasDiscrepancy, () =>
            {
                part.RuleFor(p => p.ExpectedSkidCount).NotNull();
                part.RuleFor(p => p.DiscrepancyNote).NotEmpty();
            });
        });
    }
}
```

**Rationale**:

- Centralizes validation logic (removes duplication from services/ViewModels)
- Automatic execution via pipeline behavior (constitutional compliance)
- Declarative syntax improves maintainability and testability
- Preserves all existing validation rules with no functional regression

**Alternatives Considered**:

- Manual validation in handlers: Rejected (loses pipeline behavior benefits, code duplication)
- Data annotations on models: Rejected (less flexible, no complex rule support)

---

### 3. ViewModel Refactoring Strategy

**Question**: How to migrate ViewModels from service injection to IMediator without breaking existing UI?

**Research Findings**:

Current ViewModels inject services directly:

- `ViewModel_Volvo_ShipmentEntry`: Injects `IService_Volvo`, `IService_Window`, `IService_UserSessionManager`
- `ViewModel_Volvo_History`: Injects `IService_Volvo`
- `ViewModel_Volvo_Settings`: Injects `IService_VolvoMasterData`

All ViewModels are already `partial` classes with `[ObservableProperty]` and `[RelayCommand]` attributes (constitutional compliance Principle I).

**Decision**:

- Replace service injections with `IMediator` injection
- Keep `IService_ErrorHandler`, `IService_LoggingUtility`, `IService_Window`, `IService_UserSessionManager` (UI/infrastructure services)
- Convert business logic methods to `await _mediator.Send(new QueryOrCommand())`
- Maintain existing `[RelayCommand]` methods (no signature changes to avoid XAML binding breaks)
- Keep legacy services temporarily with `[Obsolete]` attribute for gradual rollback capability

**Migration Pattern**:

```csharp
// BEFORE (Legacy - Principle III Violation)
public partial class ViewModel_Volvo_ShipmentEntry : ViewModel_Shared_Base
{
    private readonly IService_Volvo _volvoService;
    
    [RelayCommand]
    private async Task CompleteShipmentAsync()
    {
        var result = await _volvoService.CompleteShipmentAsync(shipment);
        // ... handle result
    }
}

// AFTER (CQRS - Principle III Compliant)
public partial class ViewModel_Volvo_ShipmentEntry : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    
    [RelayCommand]
    private async Task CompleteShipmentAsync()
    {
        var command = new CompleteShipmentCommand
        {
            ShipmentDate = ShipmentDate,
            Parts = Parts.ToList(),
            Notes = Notes
        };
        var result = await _mediator.Send(command);
        // ... handle result (same logic)
    }
}
```

**Rationale**:

- Maintains UI contract (RelayCommand signatures unchanged, no XAML updates needed for command binding)
- Eliminates Principle III violation (no direct service calls)
- Gradual migration path (keep legacy services during transition)
- Easier testing (mock IMediator instead of multiple services)

**Alternatives Considered**:

- Big-bang service removal: Rejected (high risk, no rollback path)
- Keeping both services and IMediator: Rejected (violates zero deviations policy post-migration)

---

### 4. Functional Parity Verification Strategy

**Question**: How to guarantee byte-for-byte parity for CSV labels and email formatting?

**Research Findings**:

Current implementation:

- CSV label generation: `Service_Volvo.GenerateLabelCsvAsync()` creates CSV with specific column order (Part Number, Qty Per Skid, Skids Received, Total Pieces, etc.)
- Email formatting: `Service_Volvo.FormatEmailAsHtml()` and `FormatEmailTextAsync()` generate HTML + plain text emails
- Component explosion: `Service_Volvo.CalculateComponentExplosionAsync()` computes piece counts from skids × qty/skid × components

**Decision**:

- **Golden File Tests**: Capture current CSV/email outputs before refactoring, store in `Module_Volvo.Tests/GoldenFiles/`
- **Property-Based Tests**: Use FsCheck.Xunit or similar to generate 1000+ random test cases for component explosion calculations
- **Integration Tests**: Full pipeline tests (ViewModel → Handler → Service → DAO → DB → Verify output)
- **Diff Verification**: Byte-for-byte comparison of refactored outputs against golden files

**Golden File Test Pattern**:

```csharp
[Fact]
public async Task GenerateLabelCsv_ProducesIdenticalOutput()
{
    // Arrange
    var query = new GenerateLabelCsvQuery { ShipmentId = 1 };
    var expectedCsv = await File.ReadAllTextAsync("GoldenFiles/expected_label.csv");
    
    // Act
    var result = await _mediator.Send(query);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Data.Should().Be(expectedCsv); // Byte-for-byte match
}
```

**Property-Based Test Pattern**:

```csharp
[Property(Arbitrary = new[] { typeof(VolvoArbitraries) })]
public Property ComponentExplosion_AlwaysProducesConsistentResults(
    int skidCount, 
    double qtyPerSkid, 
    int componentQty)
{
    // Arrange
    var input = new VolvoShipmentLine 
    { 
        ReceivedSkidCount = skidCount,
        PartNumber = "TEST-PART"
    };
    var part = new VolvoPart { QuantityPerSkid = qtyPerSkid };
    var component = new VolvoPartComponent { QuantityPerParent = componentQty };
    
    // Act
    var legacyResult = _legacyService.CalculateComponentExplosion(input, part, component);
    var cqrsResult = _mediator.Send(new CalculateComponentExplosionQuery { ... }).Result.Data;
    
    // Assert
    return (legacyResult == cqrsResult).ToProperty();
}
```

**Rationale**:

- Golden files provide definitive reference for exact output matching (SC-005, SC-006)
- Property-based tests catch edge cases in calculations (SC-007)
- Integration tests verify full pipeline correctness (SC-009)
- Automated verification prevents manual comparison errors

**Alternatives Considered**:

- Manual testing only: Rejected (not scalable, error-prone, no regression protection)
- Unit tests only: Rejected (misses integration issues, no end-to-end validation)

---

### 5. XAML Binding Migration Strategy

**Question**: How to safely migrate 20 occurrences of `{Binding}` to `x:Bind` in DataGrid columns?

**Research Findings**:

Current violations (identified via grep search):

- `View_Volvo_ShipmentEntry.xaml`: 6 `{Binding}` occurrences in DataGridTextColumn and DataGridTemplateColumn
- `View_Volvo_History.xaml`: 6 `{Binding}` occurrences in DataGrid columns
- `View_Volvo_Settings.xaml`: 2 `{Binding}` occurrences in DataGrid columns
- `VolvoShipmentEditDialog.xaml`: 6 `{Binding}` occurrences in DataGrid columns

All are in DataGrid column definitions binding to properties of list item models (e.g., `{Binding PartNumber}`, `{Binding ShipmentDate}`).

**Decision**:

- Migrate View-by-View (not all at once) to reduce risk
- Use `x:Bind` with `x:DataType` attribute on DataTemplate for compile-time type safety
- Add `Mode=OneWay` explicitly for read-only bindings
- Add `Mode=TwoWay` for editable columns (ExpectedSkidCount, HasDiscrepancy, DiscrepancyNote)
- Test each View after migration before proceeding to next

**Migration Pattern**:

```xml
<!-- BEFORE (Legacy - Principle I Violation) -->
<controls:DataGridTextColumn 
    Header="Part Number" 
    Binding="{Binding PartNumber}" />

<!-- AFTER (CQRS - Principle I Compliant) -->
<controls:DataGridTextColumn 
    Header="Part Number">
    <controls:DataGridTextColumn.Binding>
        <Binding Path="PartNumber" Mode="OneWay" />
        <!-- Note: x:Bind not supported in DataGridTextColumn.Binding, but we can use DataGridTemplateColumn -->
    </controls:DataGridTextColumn.Binding>
</controls:DataGridTextColumn>

<!-- BETTER: Use DataGridTemplateColumn for x:Bind support -->
<controls:DataGridTemplateColumn Header="Part Number">
    <controls:DataGridTemplateColumn.CellTemplate>
        <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
            <TextBlock Text="{x:Bind PartNumber, Mode=OneWay}" />
        </DataTemplate>
    </controls:DataGridTemplateColumn.CellTemplate>
</controls:DataGridTemplateColumn>
```

**Rationale**:

- Compile-time binding catches errors early (constitutional Principle VI)
- Performance improvement (x:Bind is faster than runtime Binding)
- Type safety prevents runtime binding errors
- Gradual migration reduces risk of breaking multiple Views simultaneously

**Alternatives Considered**:

- Keep `{Binding}` for DataGrid columns: Rejected (violates Principle I, loses type safety)
- Big-bang XAML migration: Rejected (high risk, hard to isolate binding errors)

**Known Limitation**:

- CommunityToolkit.WinUI DataGrid `DataGridTextColumn.Binding` property does NOT support `x:Bind` directly
- **Workaround**: Convert `DataGridTextColumn` to `DataGridTemplateColumn` with `x:DataType` and `x:Bind` in CellTemplate

---

### 6. Test Infrastructure Setup

**Question**: What test infrastructure is needed to achieve 80% coverage and functional parity verification?

**Research Findings**:

Current test setup:

- xUnit framework already configured
- FluentAssertions for assertions
- Moq for mocking (likely - verify in existing test projects)
- Integration test database connection configured via appsettings.Test.json

**Additional Requirements**:

- Property-based testing framework (FsCheck.Xunit)
- Golden file storage and comparison utilities
- Test data generators (Bogus recommended by Principle VII)

**Decision**:

- **NuGet Packages to Add**:
  - `Bogus` 35.6.1 (test data generation)
  - `FsCheck.Xunit` 2.16.6 (property-based testing)
  - `Verify.Xunit` 26.0.0 (snapshot testing for golden files)
  
- **Test Project Structure**:

```text
Module_Volvo.Tests/
├── Handlers/
│   ├── Commands/
│   │   ├── CompleteShipmentCommandHandlerTests.cs
│   │   └── [7 more command handler tests]
│   └── Queries/
│       ├── GenerateLabelCsvQueryHandlerTests.cs
│       └── [10 more query handler tests]
├── Validators/
│   ├── CompleteShipmentCommandValidatorTests.cs
│   └── [7 more validator tests]
├── Integration/
│   ├── ShipmentWorkflowIntegrationTests.cs  # Full ViewModel → Handler → DAO flow
│   └── DatabaseIntegrationTests.cs
├── PropertyBased/
│   ├── ComponentExplosionPropertyTests.cs
│   └── PieceCountCalculationPropertyTests.cs
├── GoldenFiles/
│   ├── expected_label_basic.csv
│   ├── expected_label_with_discrepancy.csv
│   ├── expected_email_html.html
│   └── expected_email_text.txt
└── Fixtures/
    ├── VolvoTestDataGenerator.cs  # Bogus faker for Volvo models
    └── VolvoArbitraries.cs         # FsCheck arbitrary generators
```

- **Coverage Target**: 80% minimum (SC-004)
  - Handlers: 90%+ (business logic critical)
  - Validators: 100% (all rules must be tested)
  - ViewModels: 70%+ (mostly IMediator.Send calls)

**Rationale**:

- Comprehensive coverage (unit + integration + property-based + golden file)
- Automated verification (no manual testing for parity checks)
- Constitutional compliance (Principle VIII: 80% minimum coverage)
- Regression protection (golden files catch unintended changes)

**Alternatives Considered**:

- Manual testing for CSV/email verification: Rejected (not repeatable, error-prone)
- Lower coverage target: Rejected (violates constitutional minimum 80%)

---

## Technology Decisions Summary

| Decision Area | Technology Choice | Rationale |
|--------------|-------------------|-----------|
| CQRS Framework | MediatR 12.4.1 | Already installed in Module_Core, constitutional Principle VII (library-first) |
| Validation | FluentValidation 11.10.0 | Already installed, automatic pipeline execution, declarative syntax |
| Logging | Serilog 4.1.0 | Already installed, structured logging via LoggingBehavior |
| DTO Mapping | Mapster (recommended) | Faster than AutoMapper, less configuration, Principle VII recommended |
| Guard Clauses | Ardalis.GuardClauses | Cleaner validation syntax, Principle VII recommended |
| Test Data Generation | Bogus 35.6.1 | Principle VII recommended, widely used |
| Property-Based Testing | FsCheck.Xunit 2.16.6 | Standard for .NET property-based testing |
| Golden File Testing | Verify.Xunit 26.0.0 | Snapshot testing for byte-for-byte comparison |
| Mocking | Moq (likely already installed) | Standard for .NET unit testing |

---

## Risk Mitigation Strategies

### High-Risk Areas

1. **Component Explosion Calculation Changes**
   - **Risk**: Refactored calculation produces different results
   - **Mitigation**: Property-based tests with 1000+ random inputs, golden file tests for known scenarios
   - **Rollback**: Keep legacy Service_Volvo.CalculateComponentExplosionAsync() until property tests pass

2. **CSV Label Format Changes**
   - **Risk**: Downstream Volvo systems reject modified CSV format
   - **Mitigation**: Byte-for-byte golden file comparison, integration test with actual label printer validation
   - **Rollback**: Revert handler to call legacy service method if golden file tests fail

3. **Email Format Changes**
   - **Risk**: Recipients confused by different email format
   - **Mitigation**: Golden file HTML/text comparison, visual inspection during UAT
   - **Rollback**: Revert to legacy email formatting service if format differs

4. **Missing FluentValidation Rules**
   - **Risk**: Edge cases not caught by new validators
   - **Mitigation**: Comprehensive validator tests mapping all legacy validation paths
   - **Rollback**: ValidationBehavior can be disabled temporarily if validation issues detected

---

## Open Questions / Future Considerations

**Q1: Should we add optimistic concurrency for shipment editing?**

- **Status**: Out of scope for MVP (Assumption: Single-user workflow)
- **Future**: Consider adding RowVersion/Timestamp column to volvo_label_data if concurrent editing becomes requirement

**Q2: Should we implement real-time notifications for shipment status changes?**

- **Status**: Out of scope (specification: "No SignalR or real-time updates")
- **Future**: Could leverage SignalR for multi-user scenarios

**Q3: Should we add caching for master data queries?**

- **Status**: Not required for MVP (Performance Goals: <500ms database operations)
- **Future**: Consider IMemoryCache for GetAllVolvoPartsQuery if performance degrades

**Q4: Should we create a separate DTO layer for request/response models?**

- **Status**: Yes - Requests/ folder structure includes Commands/ and Queries/ subdirectories
- **Decision**: Use separate request DTOs to avoid polluting domain models with MediatR interfaces

---

## Constitutional Compliance Matrix

| Principle | Before Modernization | After Modernization | Status |
|-----------|---------------------|---------------------|--------|
| I. MVVM Purity | ❌ 20 `{Binding}` occurrences | ✅ All `x:Bind` (DataGridTemplateColumn pattern) | RESOLVED |
| II. Data Access Integrity | ✅ Instance DAOs, Model_Dao_Result | ✅ Maintained | COMPLIANT |
| III. CQRS/MediatR | ❌ ViewModels call services directly | ✅ ViewModels use IMediator only | RESOLVED |
| IV. DI Registration | ✅ Services registered | ✅ Handlers auto-registered via assembly scan | COMPLIANT |
| V. Validation | ❌ No FluentValidation | ✅ 8 validators with pipeline behavior | RESOLVED |
| VI. WinUI 3 Practices | ✅ Partial ViewModels, async/await | ✅ Maintained | COMPLIANT |
| VII. Library-First | ✅ Uses approved libraries | ✅ + Mapster, Ardalis, Bogus, FsCheck, Verify | ENHANCED |
| VIII. Testing | ⚠️ Unknown coverage | ✅ 80%+ with unit/integration/property-based/golden | COMPLIANT |

---

## Phase 0 Completion Checklist

- ✅ All NEEDS CLARIFICATION items resolved
- ✅ Technology stack decisions documented with rationale
- ✅ Migration strategies defined for ViewModels, Views, and Services
- ✅ Test infrastructure requirements identified
- ✅ Functional parity verification approach established
- ✅ Risk mitigation strategies documented
- ✅ Constitutional compliance matrix completed
- ✅ Open questions captured for future consideration

**Phase 0 Status**: ✅ COMPLETE - Ready for Phase 1 (Data Model & Contracts Design)

---

**Next Phase**: Phase 1 - Generate data-model.md, contracts/, and quickstart.md
