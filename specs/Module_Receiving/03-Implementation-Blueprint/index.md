# Module_Receiving Implementation Blueprint - Index

**Version:** 1.0  
**Created:** 2026-01-25  
**Purpose:** Complete implementation guide with preset names for all artifacts following MTM naming conventions

## Overview

This blueprint provides **preset names** for every artifact needed to implement Module_Receiving from scratch. All names follow the **5-Part Naming Standard** extended to include methods.

## 5-Part Naming Standard (Extended)

**Format:** `{Type}_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`

**Applies To:**
- ✅ Classes (ViewModels, Views, Services, Models, DAOs, Helpers, Enums)
- ✅ **Methods** (NEW - see csharp-xaml-naming-conventions-extended.md)
- ✅ Files (match class names)
- ✅ Namespaces (organized by Module/Mode)
- ✅ Database Tables (prefix: tbl_)
- ✅ Stored Procedures (prefix: sp_)

## Blueprint Documents

### Core Architecture
| Document | Purpose |
|----------|---------|
| [C# & XAML Naming Conventions](./csharp-xaml-naming-conventions-extended.md) | **5-part naming for C# methods, classes, XAML bindings** |
| [SQL Naming Conventions](./sql-naming-conventions-extended.md) | **SQL Server database naming, migration from MySQL** |
| [File Structure](./file-structure.md) | Complete folder/file tree for Visual Studio solution |
| [Namespaces](./namespaces.md) | All namespace definitions |

### Application Layer
| Document | Purpose |
|----------|---------|
| [ViewModels](./viewmodels.md) | All ViewModel classes, methods, properties, commands |
| [Views](./views.md) | All XAML View files and code-behind |
| [Services](./services.md) | All Service classes, interfaces, methods |
| [Models](./models.md) | All Model classes (Entities, DataTransferObjects, Results) |

### Data Layer
| Document | Purpose |
|----------|---------|
| [DAOs](./daos.md) | All DAO classes, methods |
| [Database Schema](./database-schema.md) | Tables, columns, indexes, constraints |
| [Stored Procedures](./stored-procedures.md) | All MySQL stored procedures with parameters |

### Infrastructure
| Document | Purpose |
|----------|---------|
| [Helpers](./helpers.md) | All Helper/Utility classes, methods |
| [Enums](./enums.md) | All Enum definitions |
| [Dependency Injection](./dependency-injection.md) | All DI registrations in App.xaml.cs |

## Implementation Order

**Phase 1: Foundation (Week 1)**
1. Create folder structure
2. Implement Enums
3. Implement Models
4. Implement Helpers (Database, Validation)
5. Create database schema and stored procedures

**Phase 2: Data Layer (Week 2)**
6. Implement DAOs
7. Test DAOs with integration tests
8. Implement Services
9. Test Services with unit tests

**Phase 3: Application Layer (Week 3-4)**
10. Implement Hub Orchestration (Mode Selection)
11. Implement Guided Mode ViewModels and Views
12. Implement Manual Entry Mode ViewModels and Views
13. Implement Edit Mode ViewModels and Views

**Phase 4: Integration & Testing (Week 5)**
14. Register all components in DI
15. Integration testing
16. End-to-end testing
17. Performance testing

## Quick Reference

### Most Common Patterns

**ViewModel:**
```csharp
namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_Wizard_Display_PONumberEntry : ViewModel_Shared_Base
    {
        private readonly IMediator _mediator;
        
        public ViewModel_Receiving_Wizard_Display_PONumberEntry(
            IMediator mediator,
            IService_Shared_Infrastructure_ErrorHandler errorHandler,
            ILogger<ViewModel_Receiving_Wizard_Display_PONumberEntry> logger) : base(errorHandler, logger)
        {
            _mediator = mediator;
        }
        
        public async Task Load_Receiving_Wizard_Data_PONumberFromSessionAsync()
        {
            var query = new GetWorkflowSessionQuery();
            var result = await _mediator.Send(query);
        }
    }
}
```

**Command (Write Operation):**
```csharp
namespace MTM_Receiving_Application.Module_Receiving.Requests.Commands
{
    public record SaveReceivingTransactionCommand : IRequest<Model_Dao_Result>
    {
        public Guid TransactionId { get; init; }
        public string PONumber { get; init; } = string.Empty;
        public List<ReceivingLineDataTransferObjects> Lines { get; init; } = new();
    }
}
```

**Command Handler:**
```csharp
namespace MTM_Receiving_Application.Module_Receiving.Handlers.Commands
{
    public class SaveReceivingTransactionCommandHandler : 
        IRequestHandler<SaveReceivingTransactionCommand, Model_Dao_Result>
    {
        private readonly Dao_Receiving_Repository_ReceivingTransaction _dao;
        
        public SaveReceivingTransactionCommandHandler(Dao_Receiving_Repository_ReceivingTransaction dao)
        {
            _dao = dao;
        }
        
        public async Task<Model_Dao_Result> Handle(SaveReceivingTransactionCommand request, CancellationToken ct)
        {
            return await _dao.InsertTransactionAsync(request);
        }
    }
}
```

**Query (Read Operation):**
```csharp
namespace MTM_Receiving_Application.Module_Receiving.Requests.Queries
{
    public record GetReceivingLinesByPOQuery : IRequest<Model_Dao_Result<List<Model_Receiving_Entity_ReceivingLine>>>
    {
        public string PONumber { get; init; } = string.Empty;
    }
}
```

**Query Handler:**
```csharp
namespace MTM_Receiving_Application.Module_Receiving.Handlers.Queries
{
    public class GetReceivingLinesByPOQueryHandler :
        IRequestHandler<GetReceivingLinesByPOQuery, Model_Dao_Result<List<Model_Receiving_Entity_ReceivingLine>>>
    {
        private readonly Dao_Receiving_Repository_ReceivingLine _dao;
        
        public GetReceivingLinesByPOQueryHandler(Dao_Receiving_Repository_ReceivingLine dao)
        {
            _dao = dao;
        }
        
        public async Task<Model_Dao_Result<List<Model_Receiving_Entity_ReceivingLine>>> Handle(
            GetReceivingLinesByPOQuery request, 
            CancellationToken ct)
        {
            return await _dao.SelectByPOAsync(request.PONumber);
        }
    }
}
```

**Validator (FluentValidation):**
```csharp
namespace MTM_Receiving_Application.Module_Receiving.Validators
{
    public class SaveReceivingTransactionCommandValidator : AbstractValidator<SaveReceivingTransactionCommand>
    {
        public SaveReceivingTransactionCommandValidator()
        {
            RuleFor(x => x.PONumber)
                .NotEmpty()
                .When(x => !x.IsNonPO)
                .WithMessage("PO Number is required for PO-based transactions");
        }
    }
}
```

**DAO:**
```csharp
namespace MTM_Receiving_Application.Module_Receiving.Data
{
    public class Dao_Receiving_Repository_ReceivingLine
    {
        public async Task<Model_Dao_Result> Insert_Receiving_Database_Record_ReceivingLineAsync(Model_Receiving_Entity_ReceivingLine line) { }
        public async Task<Model_Dao_Result<List<Model_Receiving_Entity_ReceivingLine>>> Select_Receiving_Database_Records_ReceivingLinesByPOAsync(string poNumber) { }
    }
}
```

**Stored Procedure:**
```sql
CREATE PROCEDURE sp_Receiving_Line_Insert (...)
CREATE PROCEDURE sp_Receiving_Line_SelectByPO (...)
```

**Database Table:**
```sql
CREATE TABLE tbl_Receiving_Line (...)
CREATE TABLE tbl_Receiving_Transaction (...)
```

## Navigation Tips

- **New to blueprint?** Start with [Naming Conventions Extended](./naming-conventions-extended.md)
- **Building data layer?** See [Database Schema](./database-schema.md) → [Stored Procedures](./stored-procedures.md) → [DAOs](./daos.md)
- **Building UI?** See [ViewModels](./viewmodels.md) → [Views](./views.md)
- **Need DI setup?** See [Dependency Injection](./dependency-injection.md)

## Compliance Checklist

Before implementation:
- [ ] Read [Naming Conventions Extended](./naming-conventions-extended.md)
- [ ] Review [File Structure](./file-structure.md)
- [ ] Understand 5-part naming for methods
- [ ] Review .editorconfig settings
- [ ] Review .github/copilot-instructions.md

During implementation:
- [ ] All classes follow 5-part naming
- [ ] All methods follow 5-part naming
- [ ] All files match class names exactly
- [ ] All namespaces organized by Module/Mode
- [ ] All database objects use proper prefixes
- [ ] All DI registrations documented

## Related Documentation

- [Core Specifications](../00-Core/purpose-and-overview.md)
- [Business Rules](../01-Business-Rules/)
- [Workflow Modes](../02-Workflow-Modes/)
- [Project Constitution](../../../.github/CONSTITUTION.md)
- [Copilot Instructions](../../../.github/copilot-instructions.md)
