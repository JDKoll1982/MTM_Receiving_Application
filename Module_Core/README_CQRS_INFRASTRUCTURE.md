# Module_Core - CQRS Infrastructure Added

## ✅ What Was Added

Your existing Module_Core now includes complete CQRS (Command Query Responsibility Segregation) infrastructure:

### 1. **MediatR** - CQRS Pattern Framework
- **Version:** 12.4.1
- **Purpose:** Enables CQRS pattern with handlers for commands and queries
- **Configuration:** Auto-registered in `App.xaml.cs` with assembly scanning

### 2. **FluentValidation** - Declarative Validation
- **Version:** 11.10.0
- **Purpose:** Automatic validation of commands before handlers execute
- **Configuration:** Auto-discovery enabled in `App.xaml.cs`

### 3. **Serilog** - Structured Logging
- **Version:** 4.1.0
- **Purpose:** Replaces basic logging with structured, searchable logs
- **Configuration:** Configured in `App.xaml.cs` before creating the host. Daily rolling logs in `logs/app-.txt` with 30-day retention
- **Note:** Serilog is configured BEFORE the host builder to capture early startup errors

### 4. **Global Pipeline Behaviors** (Module_Core/Behaviors/)
- **LoggingBehavior** - Logs every request with execution time
- **ValidationBehavior** - Validates commands using FluentValidation
- **AuditBehavior** - Logs user context for all operations

### 5. **Shared Models** (Module_Core/Models/)
- **Model_Dao_Result** - Standard result type for DAOs (no exceptions)
- **Model_Dao_Result<T>** - Generic result type with data payload
- **Enum_ErrorSeverity** - Error severity levels (already existed, now integrated)

---

## 📁 New Files Created

```
Module_Core/
├── Behaviors/                          ← NEW FOLDER
│   ├── LoggingBehavior.cs             ← Logs all requests + execution time
│   ├── ValidationBehavior.cs          ← FluentValidation integration
│   └── AuditBehavior.cs               ← Audit trail for compliance
│
└── Models/
    └── Model_Dao_Result.cs            ← Standard DAO return type
```

---

## 🎯 How to Use CQRS in Your Feature Modules

### Step 1: Create a Query (Read Operation)

```csharp
// Module_Receiving/Handlers/Queries/GetReceivingLineQuery.cs

using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Handlers.Queries
{
    /// <summary>
    /// Query to retrieve a receiving line by ID.
    /// </summary>
    public record GetReceivingLineQuery : IRequest<Model_Dao_Result<Model_ReceivingLine>>
    {
        public int LineId { get; init; }
    }
}
```

### Step 2: Create a Handler

```csharp
// Module_Receiving/Handlers/Queries/GetReceivingLineHandler.cs

using MediatR;
using Microsoft.Extensions.Logging;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Handlers.Queries
{
    public class GetReceivingLineHandler : 
        IRequestHandler<GetReceivingLineQuery, Model_Dao_Result<Model_ReceivingLine>>
    {
        private readonly Dao_ReceivingLine _dao;
        private readonly ILogger<GetReceivingLineHandler> _logger;

        public GetReceivingLineHandler(
            Dao_ReceivingLine dao,
            ILogger<GetReceivingLineHandler> logger)
        {
            _dao = dao;
            _logger = logger;
        }

        public async Task<Model_Dao_Result<Model_ReceivingLine>> Handle(
            GetReceivingLineQuery request, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving receiving line {LineId}", request.LineId);

            var result = await _dao.GetByIdAsync(request.LineId);

            return result;
        }
    }
}
```

### Step 3: Create a Command (Write Operation)

```csharp
// Module_Receiving/Handlers/Commands/InsertReceivingLineCommand.cs

using MediatR;
using MTM_Receiving_Application.Module_Core.Models;

namespace MTM_Receiving_Application.Module_Receiving.Handlers.Commands
{
    public record InsertReceivingLineCommand : IRequest<Model_Dao_Result>
    {
        public string PONumber { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public string PartID { get; init; } = string.Empty;
        public string CreatedBy { get; init; } = string.Empty;
    }
}
```

### Step 4: Create a Validator

```csharp
// Module_Receiving/Validators/InsertReceivingLineValidator.cs

using FluentValidation;
using MTM_Receiving_Application.Module_Receiving.Handlers.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Validators
{
    public class InsertReceivingLineValidator : AbstractValidator<InsertReceivingLineCommand>
    {
        public InsertReceivingLineValidator()
        {
            RuleFor(x => x.PONumber)
                .NotEmpty()
                .WithMessage("PO Number is required")
                .MaximumLength(50)
                .WithMessage("PO Number cannot exceed 50 characters");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0");

            RuleFor(x => x.PartID)
                .NotEmpty()
                .WithMessage("Part ID is required");
        }
    }
}
```

### Step 5: Use in ViewModel

```csharp
// Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Receiving.Handlers.Queries;
using MTM_Receiving_Application.Module_Receiving.Handlers.Commands;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_POEntry : ViewModel_Shared_Base
    {
        private readonly IMediator _mediator;

        [ObservableProperty]
        private string _poNumber = string.Empty;

        [ObservableProperty]
        private int _quantity;

        public ViewModel_Receiving_POEntry(
            IMediator mediator,
            IService_ErrorHandler errorHandler,
            ILogger<ViewModel_Receiving_POEntry> logger) : base(errorHandler, logger)
        {
            _mediator = mediator;
        }

        [RelayCommand]
        private async Task SaveLineAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var command = new InsertReceivingLineCommand
                {
                    PONumber = PONumber,
                    Quantity = Quantity,
                    PartID = "PART123",
                    CreatedBy = Environment.UserName
                };

                // ValidationBehavior will automatically validate before handler executes
                // LoggingBehavior will automatically log execution time
                // AuditBehavior will automatically log user context
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    StatusMessage = "Line saved successfully";
                }
                else
                {
                    _errorHandler.ShowUserError(
                        result.ErrorMessage,
                        "Save Error",
                        nameof(SaveLineAsync));
                }
            }
            catch (FluentValidation.ValidationException ex)
            {
                // Validation failed - show user-friendly errors
                var errors = string.Join(", ", ex.Errors.Select(e => e.ErrorMessage));
                _errorHandler.ShowUserError(errors, "Validation Error", nameof(SaveLineAsync));
            }
            catch (Exception ex)
            {
                _errorHandler.HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(SaveLineAsync),
                    nameof(ViewModel_Receiving_POEntry));
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
```

---

## 🔄 Global Pipeline Behaviors (Automatic for ALL Handlers)

Every handler in every module automatically gets these behaviors:

### 1. **LoggingBehavior** (Automatic)
```
Output in logs/app-2026-01-15.txt:

[abc123] Handling InsertReceivingLineCommand
[abc123] Handled InsertReceivingLineCommand in 45ms
```

### 2. **ValidationBehavior** (Automatic)
- Runs FluentValidation validators BEFORE handler executes
- If validation fails, throws `ValidationException` (handler never runs)
- If validation passes, handler executes normally

### 3. **AuditBehavior** (Automatic)
```
Output in logs:

Audit: User JOHNK executing InsertReceivingLineCommand at 2026-01-15T10:30:45Z from MACHINE01
```

---

## 📊 Logging Output Examples

**Before (Basic Logging):**
```
INFO: Saving receiving line
INFO: Line saved
```

**After (Structured Serilog Logging):**
```
2026-01-15 10:30:45.123 [INF] [LoggingBehavior] [{RequestGuid=abc123}] Handling InsertReceivingLineCommand
2026-01-15 10:30:45.145 [INF] [AuditBehavior] Audit: User JOHNK executing InsertReceivingLineCommand at 2026-01-15T10:30:45Z from MACHINE01
2026-01-15 10:30:45.168 [INF] [LoggingBehavior] [{RequestGuid=abc123}] Handled InsertReceivingLineCommand in 45ms
```

You can now search logs by:
- Request GUID (trace single request)
- User (see all actions by user)
- Execution time (find slow queries)
- Error type

---

## ✅ Next Steps

### Option A: Use Module Rebuilder to Modernize Existing Modules
```
@module-rebuilder
Rebuild Module_Receiving using the new CQRS infrastructure
```

This will:
1. Create Handlers from existing Service methods
2. Create FluentValidation validators
3. Refactor ViewModels to use IMediator
4. Achieve 100% module independence

### Option B: Use Module Creator for New Modules
```
@module-creator
Create Module_Inventory from specs/Module_Inventory_Specification.md
```

This will:
1. Generate complete CQRS structure
2. Create Models, Handlers, Validators, ViewModels, Views
3. Follow all established patterns automatically

### Option C: Start Using CQRS Manually
1. Create a `Handlers/` folder in your feature module
2. Create `Queries/` and `Commands/` subfolders
3. Create Query/Command classes + Handlers
4. Create `Validators/` folder for FluentValidation validators
5. Inject `IMediator` into ViewModels instead of services

---

## 🎓 Learning Resources

**MediatR Documentation:**
- https://github.com/jbogard/MediatR/wiki

**FluentValidation Documentation:**
- https://docs.fluentvalidation.net

**Serilog Documentation:**
- https://serilog.net

**Project-Specific:**
- Module Development Guide: `_bmad/module-agents/config/module-development-guide.md`
- CQRS Diagrams: `_bmad/module-agents/diagrams/module-rebuild-diagrams.md`
- Tech Stack Config: `_bmad/module-agents/config/stack-winui3-csharp.yaml`

---

## 🚀 You're Ready!

Your Module_Core now has enterprise-grade CQRS infrastructure. All feature modules can now:
- ✅ Use MediatR for clean CQRS architecture
- ✅ Auto-validate with FluentValidation
- ✅ Get structured logging automatically
- ✅ Have audit trails for all operations
- ✅ Return standardized results (Model_Dao_Result)

**Next: Use `@module-rebuilder` to modernize an existing module or `@module-creator` to build a new one!**
