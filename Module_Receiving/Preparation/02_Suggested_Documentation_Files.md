# Module_Receiving - Suggested Documentation Files

## Overview

This document outlines non-code documentation files that should exist within Module_Receiving to facilitate easier development, troubleshooting, and onboarding. These files serve as context for AI assistants and human developers.

---

## 1. Core Documentation Files (High Priority)

### `README.md` (Module Root)

**Purpose:** Entry point for module understanding

**Suggested Content:**

```markdown
# Module_Receiving

## Purpose
Handles all receiving workflow operations including PO entry, line item management, 
package type selection, heat lot tracking, and final review before database commit.

## Architecture Overview
- **Pattern:** MVVM with CQRS (MediatR)
- **Data Access:** Instance-based DAOs via Service layer
- **Validation:** FluentValidation
- **Logging:** Serilog

## Key Components
- **ViewModels:** 9 workflow step ViewModels
- **Services:** ReceivingWorkflow orchestration, validation
- **DAOs:** ReceivingLine, ReceivingLoad, PackageTypePreference
- **Models:** ReceivingLine, ReceivingLoad, ReceivingSession, etc.

## Dependencies
- Module_Core (IService_ErrorHandler, IService_LoggingUtility)
- MySQL Database (mtm_receiving_application)
- SQL Server (Infor Visual - READ ONLY)

## Quick Start
See `Preparation/04_Implementation_Order.md` for development workflow.
```

**Location:** `Module_Receiving/README.md`

---

### `ARCHITECTURE.md`

**Purpose:** Deep dive into module architecture, patterns, and design decisions

**Suggested Content:**

- MVVM implementation details (CommunityToolkit.Mvvm usage)
- Service layer responsibilities
- DAO pattern (instance-based, `Model_Dao_Result` returns)
- Navigation flow (workflow state machine)
- Error handling strategy
- Validation approach
- Database interaction patterns

**Location:** `Module_Receiving/ARCHITECTURE.md`

---

### `DATA_MODEL.md`

**Purpose:** Database schema reference specific to receiving module

**Suggested Content:**

- Entity-Relationship Diagram (PlantUML)
- Table schemas (receiving_loads, receiving_lines, package_type_preferences)
- Stored procedure documentation
- Foreign key relationships
- Indexes and performance considerations

**Location:** `Module_Receiving/DATA_MODEL.md`

---

### `WORKFLOWS.md`

**Purpose:** Visual and textual description of user workflows

**Suggested Content:**

- Receiving workflow steps (Mode Selection → PO Entry → ... → Review)
- State transitions diagram
- User interactions per step
- Data flow between ViewModels
- Session management

**Location:** `Module_Receiving/WORKFLOWS.md`

---

## 2. Preparation Files (Planning & Implementation)

### `Preparation/01_Library_Research.md` ✅ Already Created

**Purpose:** Library selection rationale
**Content:** Completed in previous file

---

### `Preparation/02_Suggested_Documentation_Files.md` ✅ This File

**Purpose:** Meta-documentation guide

---

### `Preparation/03_Clarification_Questions.md`

**Purpose:** Questions to ask before implementation begins

**Suggested Content:**

```markdown
# Clarification Questions for Module_Receiving Rebuild

## Architecture Decisions
1. Should we adopt MediatR for CQRS, or keep service layer pattern?
2. Which navigation library (Uno.Extensions, Mvvm.Navigation, or custom)?
3. Keep `Service_ErrorHandler` in Module_Core or move to Module_Receiving?

## Database
4. Can we modify stored procedures, or are they locked?
5. Are there existing integration tests for database operations?
6. What is the migration strategy for existing data?

## Dependencies
7. Which Module_Core services must remain shared vs. moved to Module_Receiving?
8. Can we introduce new NuGet packages (MediatR, Serilog, FluentValidation)?

## Validation
9. Should validation happen in ViewModel, Service, or DAO layer?
10. Are there existing validation rules documented?

## Testing
11. Required test coverage percentage?
12. Prefer unit tests (mocked DAOs) or integration tests (test database)?

## UI/UX
13. Can navigation flow be changed, or must it match current exactly?
14. Are there performance requirements (e.g., load time < X seconds)?

## Deployment
15. What is the deployment process for Module_Receiving changes?
16. Are there backward compatibility requirements?
```

**Location:** `Module_Receiving/Preparation/03_Clarification_Questions.md`

---

### `Preparation/04_Implementation_Order.md`

**Purpose:** Step-by-step development plan

**Suggested Content:**

```markdown
# Implementation Order for Module_Receiving Rebuild

## Phase 1: Foundation (Week 1)
### 1.1 Project Setup
- [ ] Create Defaults folder with default models
- [ ] Install NuGet packages (MediatR, Serilog, FluentValidation, CsvHelper)
- [ ] Update App.xaml.cs DI registration
- [ ] Create Module_Receiving-specific service interfaces

### 1.2 Models & Validation
- [ ] Review and update Models (ReceivingLine, ReceivingLoad, etc.)
- [ ] Create FluentValidation validators for each model
- [ ] Unit test validators

### 1.3 Logging Setup
- [ ] Configure Serilog in App.xaml.cs
- [ ] Create structured logging patterns for Module_Receiving
- [ ] Replace IService_LoggingUtility with ILogger<T> in one ViewModel (POC)

## Phase 2: Data Layer Refactor (Week 2)
### 2.1 DAOs
- [ ] Verify instance-based pattern in existing DAOs
- [ ] Ensure all DAOs return Model_Dao_Result
- [ ] Add XML documentation to all DAO methods
- [ ] Create integration tests for each DAO

### 2.2 CQRS with MediatR (Optional)
- [ ] Create Handlers folder
- [ ] Migrate Service_MySQL_ReceivingLine methods to MediatR handlers
- [ ] Implement pipeline behaviors (logging, validation)
- [ ] Update ViewModels to use IMediator

## Phase 3: ViewModels & Navigation (Week 3)
### 3.1 ViewModels
- [ ] Refactor ViewModel_Receiving_POEntry (pilot ViewModel)
- [ ] Test with new validation, logging, and MediatR
- [ ] Apply pattern to remaining 8 ViewModels

### 3.2 Navigation
- [ ] Implement chosen navigation library (Uno.Extensions or custom)
- [ ] Update Service_ReceivingWorkflow to use new navigation
- [ ] Test navigation flow end-to-end

## Phase 4: Services Cleanup (Week 4)
### 4.1 Service Consolidation
- [ ] Remove redundant services from Module_Core
- [ ] Move Receiving-specific services to Module_Receiving/Services
- [ ] Update DI registrations

### 4.2 CSV Export
- [ ] Replace Service_CSVWriter with CsvHelper
- [ ] Create typed export services in Module_Receiving

## Phase 5: Testing & Documentation (Week 5)
- [ ] Achieve 80% unit test coverage
- [ ] Create integration test suite
- [ ] Update all documentation (README, ARCHITECTURE, etc.)
- [ ] Code review with team
- [ ] Performance benchmarking

## Phase 6: Deployment (Week 6)
- [ ] Merge to development branch
- [ ] QA testing
- [ ] Production deployment
```

**Location:** `Module_Receiving/Preparation/04_Implementation_Order.md`

---

### `Preparation/05_Task_Checklist.md`

**Purpose:** Granular task tracking

**Suggested Content:**

```markdown
# Task Checklist for Module_Receiving Rebuild

## Setup Tasks
- [ ] Install MediatR package
- [ ] Install Serilog packages (Core, File sink, Extensions.Logging)
- [ ] Install FluentValidation package
- [ ] Install CsvHelper package
- [ ] Install Uno.Extensions.Navigation.WinUI (if using)
- [ ] Create Defaults folder structure
- [ ] Create Handlers folder (if using MediatR)
- [ ] Create Validators folder

## Model Tasks
- [ ] Review Model_ReceivingLine
- [ ] Review Model_ReceivingLoad
- [ ] Review Model_ReceivingSession
- [ ] Review Model_PackageTypePreference
- [ ] Review Model_InforVisualPO
- [ ] Review Model_InforVisualPart
- [ ] Create missing models (if any)

## Validator Tasks
- [ ] Create ReceivingLineValidator
- [ ] Create ReceivingLoadValidator
- [ ] Create ReceivingSessionValidator
- [ ] Unit test all validators

## DAO Tasks
- [ ] Verify Dao_ReceivingLine instance pattern
- [ ] Verify Dao_ReceivingLoad instance pattern
- [ ] Verify Dao_PackageTypePreference instance pattern
- [ ] Add XML documentation to all DAOs
- [ ] Integration test Dao_ReceivingLine
- [ ] Integration test Dao_ReceivingLoad

## ViewModel Tasks
- [ ] Refactor ViewModel_Receiving_ModeSelection
- [ ] Refactor ViewModel_Receiving_POEntry
- [ ] Refactor ViewModel_Receiving_ManualEntry
- [ ] Refactor ViewModel_Receiving_WeightQuantity
- [ ] Refactor ViewModel_Receiving_PackageType
- [ ] Refactor ViewModel_Receiving_HeatLot
- [ ] Refactor ViewModel_Receiving_LoadEntry
- [ ] Refactor ViewModel_Receiving_Review
- [ ] Refactor ViewModel_Receiving_Workflow

## Service Tasks
- [ ] Create IService_ReceivingWorkflow interface
- [ ] Implement Service_ReceivingWorkflow
- [ ] Create IService_ReceivingValidation interface (if keeping)
- [ ] Implement Service_ReceivingValidation with FluentValidation
- [ ] Create IService_CSVExport<T> interface
- [ ] Implement Service_CSVExport with CsvHelper

## MediatR Tasks (Optional)
- [ ] Create GetReceivingLinesQuery handler
- [ ] Create InsertReceivingLineCommand handler
- [ ] Create UpdateReceivingLineCommand handler
- [ ] Create DeleteReceivingLineCommand handler
- [ ] Create logging pipeline behavior
- [ ] Create validation pipeline behavior
- [ ] Create transaction pipeline behavior

## Navigation Tasks
- [ ] Choose navigation library
- [ ] Create INavigationService interface
- [ ] Implement NavigationService
- [ ] Update ViewModel_Receiving_Workflow to use INavigationService
- [ ] Test navigation flow

## Testing Tasks
- [ ] Create test project (if not exists)
- [ ] Unit test all ViewModels
- [ ] Unit test all Services
- [ ] Integration test all DAOs
- [ ] End-to-end test receiving workflow

## Documentation Tasks
- [ ] Write README.md
- [ ] Write ARCHITECTURE.md
- [ ] Write DATA_MODEL.md
- [ ] Write WORKFLOWS.md
- [ ] Write DEFAULTS.md
- [ ] Update .github/copilot-instructions.md

## Code Review Tasks
- [ ] Self-review code
- [ ] Peer review by team
- [ ] Address review feedback
- [ ] Final approval
```

**Location:** `Module_Receiving/Preparation/05_Task_Checklist.md`

---

### `Preparation/06_Schematic_File.md`

**Purpose:** Visual diagrams and schematics

**Suggested Content:**

```markdown
# Module_Receiving - Schematic Diagrams

## Folder Structure

\`\`\`
Module_Receiving/
├── README.md
├── ARCHITECTURE.md
├── DATA_MODEL.md
├── WORKFLOWS.md
├── Defaults/
│   ├── Model_DefaultReceivingSettings.cs
│   ├── Model_DefaultPackageTypes.cs
│   └── Model_DefaultValidationRules.cs
├── Data/
│   ├── Dao_ReceivingLine.cs
│   ├── Dao_ReceivingLoad.cs
│   └── Dao_PackageTypePreference.cs
├── Handlers/ (if using MediatR)
│   ├── Queries/
│   │   ├── GetReceivingLinesQuery.cs
│   │   └── GetReceivingLinesHandler.cs
│   └── Commands/
│       ├── InsertReceivingLineCommand.cs
│       └── InsertReceivingLineHandler.cs
├── Models/
│   ├── Model_ReceivingLine.cs
│   ├── Model_ReceivingLoad.cs
│   └── Model_ReceivingSession.cs
├── Services/
│   ├── Service_ReceivingWorkflow.cs
│   ├── Service_ReceivingValidation.cs
│   └── Service_CSVExport.cs
├── Validators/
│   ├── ReceivingLineValidator.cs
│   └── ReceivingLoadValidator.cs
├── ViewModels/
│   ├── ViewModel_Receiving_ModeSelection.cs
│   ├── ViewModel_Receiving_POEntry.cs
│   └── ... (7 more ViewModels)
├── Views/
│   ├── View_Receiving_ModeSelection.xaml
│   ├── View_Receiving_POEntry.xaml
│   └── ... (7 more Views)
└── Preparation/
    ├── 01_Library_Research.md
    ├── 02_Suggested_Documentation_Files.md
    ├── 03_Clarification_Questions.md
    ├── 04_Implementation_Order.md
    ├── 05_Task_Checklist.md
    ├── 06_Schematic_File.md
    └── 07_Research_Archive.md
\`\`\`

## Architecture Diagram (PlantUML)

\`\`\`plantuml
@startuml Module_Receiving_Architecture

package "Module_Receiving" {
    package "Views (XAML)" {
        [View_Receiving_POEntry]
        [View_Receiving_Review]
    }
    
    package "ViewModels (MVVM)" {
        [ViewModel_Receiving_POEntry]
        [ViewModel_Receiving_Review]
    }
    
    package "Services (Business Logic)" {
        [Service_ReceivingWorkflow]
        [Service_ReceivingValidation]
    }
    
    package "Handlers (CQRS)" {
        [GetReceivingLinesHandler]
        [InsertReceivingLineHandler]
    }
    
    package "Data Access (DAOs)" {
        [Dao_ReceivingLine]
        [Dao_ReceivingLoad]
    }
    
    package "Validators" {
        [ReceivingLineValidator]
    }
}

package "Module_Core" {
    [IService_ErrorHandler]
    [ILogger<T>]
}

package "Database" {
    database "MySQL" {
        [receiving_lines]
        [receiving_loads]
    }
}

[View_Receiving_POEntry] --> [ViewModel_Receiving_POEntry]
[ViewModel_Receiving_POEntry] --> [GetReceivingLinesHandler]
[ViewModel_Receiving_POEntry] --> [Service_ReceivingWorkflow]
[GetReceivingLinesHandler] --> [Dao_ReceivingLine]
[Dao_ReceivingLine] --> [receiving_lines]
[InsertReceivingLineHandler] --> [ReceivingLineValidator]
[ReceivingLineValidator] --> [InsertReceivingLineHandler]
[InsertReceivingLineHandler] --> [Dao_ReceivingLine]

[ViewModel_Receiving_POEntry] --> [IService_ErrorHandler]
[ViewModel_Receiving_POEntry] --> [ILogger<T>]

@enduml
\`\`\`

## Workflow State Diagram (PlantUML)

\`\`\`plantuml
@startuml Receiving_Workflow_State_Diagram

[*] --> ModeSelection

ModeSelection --> POEntry : Manual/Scan
ModeSelection --> LoadEntry : Load Entry

POEntry --> WeightQuantity
WeightQuantity --> PackageType
PackageType --> HeatLot
HeatLot --> Review

LoadEntry --> Review

Review --> [*] : Save to DB
Review --> ModeSelection : Cancel

@enduml
\`\`\`

## Data Flow Diagram (PlantUML)

\`\`\`plantuml
@startuml Receiving_Data_Flow

actor User
participant "ViewModel" as VM
participant "IMediator" as MED
participant "Handler" as H
participant "Validator" as V
participant "DAO" as DAO
database "MySQL" as DB

User -> VM : Enter Line Data
VM -> MED : Send(InsertLineCommand)
MED -> H : Handle(command)
H -> V : Validate(line)
V --> H : ValidationResult
alt Validation Success
    H -> DAO : InsertAsync(line)
    DAO -> DB : CALL sp_insert_line
    DB --> DAO : Result
    DAO --> H : Model_Dao_Result
    H --> MED : Success
    MED --> VM : Success
    VM --> User : Show Success
else Validation Failed
    V --> H : Errors
    H --> MED : Failure
    MED --> VM : Errors
    VM --> User : Show Errors
end

@enduml
\`\`\`
```

**Location:** `Module_Receiving/Preparation/06_Schematic_File.md`

---

### `Preparation/07_Research_Archive.md`

**Purpose:** Store research notes, links, and investigation findings

**Suggested Content:**

```markdown
# Research Archive for Module_Receiving

## Library Research Links
- MediatR GitHub: https://github.com/jbogard/MediatR
- MediatR Wiki: https://github.com/jbogard/MediatR/wiki
- Serilog Documentation: https://serilog.net/
- FluentValidation Docs: https://docs.fluentvalidation.net/

## WinUI 3 Resources
- Official Docs: https://learn.microsoft.com/en-us/windows/apps/winui/
- CommunityToolkit.Mvvm: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/

## Architecture Patterns
- Clean Architecture: https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- CQRS Pattern: https://martinfowler.com/bliki/CQRS.html
- Mediator Pattern: https://refactoring.guru/design-patterns/mediator

## DAO Pattern Research
- Repository vs DAO: https://stackoverflow.com/questions/8550124/what-is-the-difference-between-dao-and-repository-patterns
- Instance-based DAOs: [Internal discussion notes]

## Validation Patterns
- FluentValidation Best Practices: https://docs.fluentvalidation.net/en/latest/
- Validation in MVVM: [Team discussion outcomes]

## Performance Considerations
- WinUI 3 Performance: https://learn.microsoft.com/en-us/windows/apps/performance/
- Entity Framework vs Stored Procedures: [Benchmark results]

## Testing Strategies
- xUnit Documentation: https://xunit.net/
- FluentAssertions: https://fluentassertions.com/
- Moq: https://github.com/moq/moq

## Migration Strategies
- Strangler Fig Pattern: https://martinfowler.com/bliki/StranglerFigApplication.html
- Big Bang vs Incremental: [Team decision log]
```

**Location:** `Module_Receiving/Preparation/07_Research_Archive.md`

---

## 3. Runtime Documentation (Developer Experience)

### `DEFAULTS.md`

**Purpose:** Document default values and configuration

**Suggested Content:**

```markdown
# Module_Receiving - Default Values

## Default Package Types
- Standard Box: 12"x12"x12"
- Pallet: 48"x40"
- Custom: User-defined

## Default Validation Rules
- PO Number: 1-50 characters, alphanumeric
- Quantity: Must be > 0
- Weight: Optional, but if provided must be > 0

## Default Workflow Settings
- Auto-advance to next step: Enabled
- Save draft on navigation: Enabled
- Timeout for idle session: 30 minutes

## Default CSV Export Settings
- Delimiter: Comma
- Encoding: UTF-8
- Include Headers: True
```

**Location:** `Module_Receiving/DEFAULTS.md`

---

### `TROUBLESHOOTING.md`

**Purpose:** Common issues and solutions

**Suggested Content:**

```markdown
# Module_Receiving - Troubleshooting Guide

## Common Issues

### 1. ViewModel not binding to View
**Symptom:** UI not updating when ViewModel properties change
**Cause:** Forgot `partial` keyword or `[ObservableProperty]` attribute
**Solution:** 
\`\`\`csharp
public partial class MyViewModel : ViewModel_Shared_Base // ✅ partial
{
    [ObservableProperty] // ✅ attribute
    private string _myProperty;
}
\`\`\`

### 2. MediatR handler not found
**Symptom:** `InvalidOperationException: No service for type IRequestHandler`
**Cause:** Handler not registered in DI
**Solution:** Ensure `services.AddMediatR()` in App.xaml.cs

### 3. Database connection fails
**Symptom:** `MySqlException: Unable to connect`
**Cause:** Connection string misconfigured
**Solution:** Verify `Helper_Database_Variables.GetConnectionString()`

## Debugging Tips
- Enable Serilog debug output to see all logs
- Use FluentAssertions for better error messages in tests
- Check Output window for XAML binding errors (WMC1110, WMC1121)
```

**Location:** `Module_Receiving/TROUBLESHOOTING.md`

---

### `CHANGELOG.md`

**Purpose:** Track changes to module over time

**Suggested Content:**

```markdown
# Module_Receiving - Changelog

## [Unreleased]
### Added
- MediatR for CQRS pattern
- Serilog for structured logging
- FluentValidation for model validation
- CsvHelper for exports

### Changed
- Migrated from Service_MySQL_ReceivingLine to MediatR handlers
- Replaced IService_LoggingUtility with ILogger<T>

### Removed
- Service_CSVWriter (replaced by CsvHelper)

## [1.0.0] - 2026-01-15
### Initial Release
- MVVM receiving workflow
- 9 workflow steps
- MySQL database integration
```

**Location:** `Module_Receiving/CHANGELOG.md`

---

## 4. AI Assistant Context Files

### `.copilot/instructions.md`

**Purpose:** GitHub Copilot-specific instructions for this module

**Suggested Content:**

```markdown
# Copilot Instructions for Module_Receiving

## Patterns to Follow
- Always use `partial` for ViewModels
- Use `[ObservableProperty]` for bindable properties
- Use `[RelayCommand]` for commands
- Return `Model_Dao_Result` from DAOs
- Use `ILogger<T>` for logging, not IService_LoggingUtility

## Validation Pattern
When creating models, always create a FluentValidation validator:
\`\`\`csharp
public class MyModelValidator : AbstractValidator<MyModel>
{
    public MyModelValidator()
    {
        RuleFor(x => x.Property).NotEmpty();
    }
}
\`\`\`

## MediatR Pattern (if using)
For queries:
\`\`\`csharp
public record GetDataQuery(int Id) : IRequest<Model_Dao_Result<Data>>;
public class GetDataHandler : IRequestHandler<GetDataQuery, Model_Dao_Result<Data>> { }
\`\`\`

For commands:
\`\`\`csharp
public record InsertDataCommand(Data data) : IRequest<Model_Dao_Result>;
public class InsertDataHandler : IRequestHandler<InsertDataCommand, Model_Dao_Result> { }
\`\`\`
```

**Location:** `Module_Receiving/.copilot/instructions.md`

---

## 5. Testing Documentation

### `TESTING.md`

**Purpose:** Testing strategy and guidelines

**Suggested Content:**

```markdown
# Module_Receiving - Testing Guide

## Test Structure
\`\`\`
Tests/
├── Unit/
│   ├── ViewModels/
│   ├── Services/
│   └── Validators/
└── Integration/
    └── Data/
\`\`\`

## Unit Testing ViewModels
\`\`\`csharp
[Fact]
public async Task LoadDataCommand_Success_PopulatesItems()
{
    // Arrange
    var mockMediator = new Mock<IMediator>();
    mockMediator.Setup(m => m.Send(It.IsAny<GetLinesQuery>(), default))
        .ReturnsAsync(Model_Dao_Result.Success(new List<Model_ReceivingLine> { new() }));
    
    var vm = new ViewModel_Receiving_Review(mockMediator.Object, ...);
    
    // Act
    await vm.LoadDataCommand.ExecuteAsync(null);
    
    // Assert
    vm.Items.Should().HaveCount(1);
}
\`\`\`

## Integration Testing DAOs
\`\`\`csharp
[Fact]
public async Task InsertLineAsync_ValidData_ReturnsSuccess()
{
    // Arrange
    var dao = new Dao_ReceivingLine(TestConnectionString);
    var line = new Model_ReceivingLine { Quantity = 10, PartID = "P123" };
    
    // Act
    var result = await dao.InsertLineAsync(line);
    
    // Assert
    result.Success.Should().BeTrue();
}
\`\`\`
```

**Location:** `Module_Receiving/TESTING.md`

---

## Summary: Recommended Documentation Files

### Must-Have (High Priority)

1. ✅ `README.md` - Module overview
2. ✅ `ARCHITECTURE.md` - Design patterns and decisions
3. ✅ `Preparation/03_Clarification_Questions.md` - Pre-implementation questions
4. ✅ `Preparation/04_Implementation_Order.md` - Development roadmap
5. ✅ `Preparation/05_Task_Checklist.md` - Granular task tracking

### Should-Have (Medium Priority)

6. ✅ `DATA_MODEL.md` - Database schema reference
2. ✅ `WORKFLOWS.md` - User workflows documentation
3. ✅ `Preparation/06_Schematic_File.md` - Visual diagrams
4. ✅ `TROUBLESHOOTING.md` - Common issues guide

### Nice-to-Have (Low Priority)

10. ✅ `DEFAULTS.md` - Default values reference
2. ✅ `CHANGELOG.md` - Version history
3. ✅ `TESTING.md` - Testing strategy
4. ✅ `Preparation/07_Research_Archive.md` - Research notes
5. ✅ `.copilot/instructions.md` - AI assistant context

---

## Benefits of This Documentation Structure

### For Human Developers

- Faster onboarding for new team members
- Clear reference for patterns and conventions
- Easier troubleshooting with centralized guides

### For AI Assistants

- Better context for code generation
- Consistent pattern application
- Reduced hallucinations (documented patterns vs. guessed patterns)

### For Project Management

- Clear visibility into tasks and progress
- Documented design decisions
- Easier code reviews (context is documented)

---

## Maintenance Strategy

1. **Update documentation alongside code changes**
2. **Review documentation during code reviews**
3. **Archive outdated research in Preparation folder**
4. **Keep README.md and ARCHITECTURE.md current at all times**
