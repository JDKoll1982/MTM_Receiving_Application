# Module_Volvo - Privileges & Authorization

**Version:** 1.0.0 | **Generated:** 2026-01-17

---

## 📋 Overview

This document specifies authorization requirements for Module_Volvo operations. Module_Volvo uses **service-level authorization** via `Service_VolvoAuthorization` rather than explicit handler-level attributes.

**Authorization Model:** Role-Based Access Control (RBAC) via `IService_VolvoAuthorization`

**No Explicit `[Authorize]` Attributes Found:** Module_Volvo implements authorization checks at the service and ViewModel level rather than declaratively on handlers.

---

## 🔐 Authorization Service

### Service_VolvoAuthorization
**Interface:** `IService_VolvoAuthorization`  
**Purpose:** Centralized authorization logic for Volvo module operations  
**Location:** `Module_Volvo/Services/Service_VolvoAuthorization.cs`

**Key Methods:**
```csharp
public interface IService_VolvoAuthorization
{
    /// <summary>
    /// Check if the current user can edit the parts catalog
    /// </summary>
    bool CanUserEditParts();

    /// <summary>
    /// Check if the current user can create/edit shipments
    /// </summary>
    bool CanUserManageShipments();

    /// <summary>
    /// Check if the current user can view shipment history
    /// </summary>
    bool CanUserViewHistory();
}
```

**Implementation Notes:**
- Service reads from Windows Authentication context or `IService_UserSessionManager`
- Role checks are performed against configured role groups
- Authorization failures should trigger UI disabling (buttons, menu items) rather than runtime exceptions

---

## 👥 Privilege Roles

### Role: Volvo.Operator
**Purpose:** Standard receiving operator role for Volvo shipments

**Permissions:**
- ✅ Create new shipments
- ✅ Add/remove parts from shipments
- ✅ Mark discrepancies
- ✅ Save shipments as pending
- ✅ Complete shipments (with PO/Receiver numbers)
- ✅ Generate barcode labels
- ✅ Preview email requisitions
- ✅ View shipment history (own shipments)
- ✅ Search parts catalog (read-only)
- ❌ Edit parts catalog (add/edit/deactivate parts)
- ❌ Import/export parts catalog
- ❌ Edit completed shipments

**Authorization Check:**
```csharp
if (!_volvoAuthService.CanUserManageShipments())
{
    await _errorHandler.ShowUserErrorAsync(
        "You do not have permission to manage Volvo shipments",
        "Access Denied"
    );
    return;
}
```

---

### Role: Volvo.Manager
**Purpose:** Supervisor/manager role with elevated privileges

**Permissions:**
- ✅ All Volvo.Operator permissions
- ✅ Edit parts catalog (add/edit/deactivate parts)
- ✅ Import/export parts catalog CSV
- ✅ Edit completed shipments
- ✅ View all shipments (not just own)
- ✅ Manage email recipient settings
- ✅ View part components

**Authorization Check:**
```csharp
if (!_volvoAuthService.CanUserEditParts())
{
    await _errorHandler.ShowUserErrorAsync(
        "You do not have permission to edit the parts catalog",
        "Access Denied"
    );
    return;
}
```

---

### Role: Volvo.Admin
**Purpose:** System administrator role for Volvo module

**Permissions:**
- ✅ All Volvo.Manager permissions
- ✅ Modify database settings directly
- ✅ Reset settings to defaults
- ✅ Delete shipments (via database)

**Note:** Admin role typically bypasses all authorization checks.

---

## 🎯 Command & Query Authorization

Since Module_Volvo does not use explicit `[Authorize]` attributes on handlers, authorization is enforced **before** calling `IMediator.Send()` in ViewModels or services.

### Commands Requiring Authorization

| Command | Required Role | Check Location | Check Method |
|---------|---------------|----------------|--------------|
| **AddPartToShipmentCommand** | Volvo.Operator | ViewModel_Volvo_ShipmentEntry | `CanUserManageShipments()` |
| **AddVolvoPartCommand** | Volvo.Manager | ViewModel_Volvo_Settings | `CanUserEditParts()` |
| **CompleteShipmentCommand** | Volvo.Operator | ViewModel_Volvo_ShipmentEntry | `CanUserManageShipments()` |
| **DeactivateVolvoPartCommand** | Volvo.Manager | ViewModel_Volvo_Settings | `CanUserEditParts()` |
| **ImportPartsCsvCommand** | Volvo.Manager | ViewModel_Volvo_Settings | `CanUserEditParts()` |
| **RemovePartFromShipmentCommand** | Volvo.Operator | ViewModel_Volvo_ShipmentEntry | `CanUserManageShipments()` |
| **SavePendingShipmentCommand** | Volvo.Operator | ViewModel_Volvo_ShipmentEntry | `CanUserManageShipments()` |
| **UpdateShipmentCommand** | Volvo.Manager | ViewModel_Volvo_History | `CanUserManageShipments()` |
| **UpdateVolvoPartCommand** | Volvo.Manager | ViewModel_Volvo_Settings | `CanUserEditParts()` |

---

### Queries Requiring Authorization

| Query | Required Role | Check Location | Check Method |
|-------|---------------|----------------|--------------|
| **ExportPartsCsvQuery** | Volvo.Manager | ViewModel_Volvo_Settings | `CanUserEditParts()` |
| **ExportShipmentsQuery** | Volvo.Manager | ViewModel_Volvo_History | `CanUserViewHistory()` |
| **FormatEmailDataQuery** | Volvo.Operator | ViewModel_Volvo_ShipmentEntry | `CanUserManageShipments()` |
| **GenerateLabelCsvQuery** | Volvo.Operator | ViewModel_Volvo_ShipmentEntry | `CanUserManageShipments()` |
| **GetAllVolvoPartsQuery** | *(No auth)* | - | Public read |
| **GetInitialShipmentDataQuery** | Volvo.Operator | ViewModel_Volvo_ShipmentEntry | `CanUserManageShipments()` |
| **GetPartComponentsQuery** | Volvo.Manager | ViewModel_Volvo_Settings | `CanUserEditParts()` |
| **GetPendingShipmentQuery** | Volvo.Operator | ViewModel_Volvo_ShipmentEntry | `CanUserManageShipments()` |
| **GetRecentShipmentsQuery** | Volvo.Operator | ViewModel_Volvo_History | `CanUserViewHistory()` |
| **GetShipmentDetailQuery** | Volvo.Operator | ViewModel_Volvo_History | `CanUserViewHistory()` |
| **GetShipmentHistoryQuery** | Volvo.Operator | ViewModel_Volvo_History | `CanUserViewHistory()` |
| **SearchVolvoPartsQuery** | *(No auth)* | - | Public read |

---

## 🎨 UI Authorization Patterns

### Button/Command Disabling
ViewModels should disable commands when user lacks permissions:

```csharp
[RelayCommand(CanExecute = nameof(CanEditPart))]
private async Task EditPartAsync()
{
    // Command implementation
}

private bool CanEditPart()
{
    return _volvoAuthService.CanUserEditParts() && SelectedPart != null;
}
```

**XAML Binding:**
```xml
<Button 
    Content="Edit Part"
    Command="{x:Bind ViewModel.EditPartCommand}"
    IsEnabled="{x:Bind ViewModel.CanEditPartCommand.CanExecute(null), Mode=OneWay}" />
```

---

### Menu Item Visibility
Use visibility bindings to hide menu items for unauthorized users:

```csharp
public bool IsManagerUser => _volvoAuthService.CanUserEditParts();
```

**XAML Binding:**
```xml
<MenuFlyoutItem 
    Text="Import Parts"
    Command="{x:Bind ViewModel.ImportCsvCommand}"
    Visibility="{x:Bind ViewModel.IsManagerUser, Converter={StaticResource BoolToVisibilityConverter}}" />
```

---

## 🛡️ Implementation Guidelines

### Adding Authorization to New Commands/Queries

**Step 1: Define Permission Method in IService_VolvoAuthorization**
```csharp
public interface IService_VolvoAuthorization
{
    /// <summary>
    /// Check if the current user can perform custom operation
    /// </summary>
    bool CanUserPerformCustomOperation();
}
```

**Step 2: Implement Check in Service_VolvoAuthorization**
```csharp
public class Service_VolvoAuthorization : IService_VolvoAuthorization
{
    private readonly IService_UserSessionManager _sessionManager;

    public bool CanUserPerformCustomOperation()
    {
        var currentUser = _sessionManager.GetCurrentUser();
        return currentUser.IsInRole("Volvo.Manager") || currentUser.IsInRole("Volvo.Admin");
    }
}
```

**Step 3: Inject Service into ViewModel**
```csharp
public partial class ViewModel_Volvo_CustomFeature : ViewModel_Shared_Base
{
    private readonly IService_VolvoAuthorization _volvoAuthService;

    public ViewModel_Volvo_CustomFeature(
        IService_VolvoAuthorization volvoAuthService,
        // other dependencies...
    )
    {
        _volvoAuthService = volvoAuthService;
    }
}
```

**Step 4: Enforce in ViewModel Command**
```csharp
[RelayCommand]
private async Task CustomOperationAsync()
{
    if (!_volvoAuthService.CanUserPerformCustomOperation())
    {
        await _errorHandler.ShowUserErrorAsync(
            "You do not have permission to perform this operation",
            "Access Denied"
        );
        return;
    }

    var command = new CustomOperationCommand { /* ... */ };
    var result = await _mediator.Send(command);
    // Handle result...
}
```

---

## 🚨 Security Considerations

### ✅ DO:
- ✅ Always check permissions **before** calling `IMediator.Send()`
- ✅ Disable UI elements (buttons, menu items) for unauthorized users
- ✅ Log authorization failures for security auditing
- ✅ Use consistent role names across all modules
- ✅ Test authorization logic with different user roles

### ❌ DO NOT:
- ❌ Rely solely on UI disabling (always check in ViewModel)
- ❌ Hard-code user names or roles in authorization checks
- ❌ Expose sensitive data in error messages (e.g., "User X cannot access Y")
- ❌ Skip authorization for "internal" or "helper" commands
- ❌ Cache authorization results across sessions

---

## 🧪 Testing Authorization

### Unit Test Example (FluentAssertions)
```csharp
[Fact]
public async Task EditPartCommand_WithoutManagerRole_ShouldShowAccessDenied()
{
    // Arrange
    var mockAuthService = new Mock<IService_VolvoAuthorization>();
    mockAuthService.Setup(x => x.CanUserEditParts()).Returns(false);

    var viewModel = new ViewModel_Volvo_Settings(
        /* mediator */ null,
        mockAuthService.Object,
        /* errorHandler */ mockErrorHandler.Object,
        /* logger */ mockLogger.Object
    );

    // Act
    await viewModel.EditPartCommand.ExecuteAsync(null);

    // Assert
    mockErrorHandler.Verify(x => x.ShowUserErrorAsync(
        It.Is<string>(msg => msg.Contains("permission")),
        "Access Denied",
        It.IsAny<string>()
    ), Times.Once);
}
```

---

## 📊 Authorization Matrix

| Operation | Volvo.Operator | Volvo.Manager | Volvo.Admin |
|-----------|----------------|---------------|-------------|
| **Shipment Management** | | | |
| Create shipment | ✅ | ✅ | ✅ |
| Edit own pending shipment | ✅ | ✅ | ✅ |
| Edit any shipment | ❌ | ✅ | ✅ |
| Delete shipment | ❌ | ❌ | ✅ |
| Complete shipment | ✅ | ✅ | ✅ |
| Generate labels | ✅ | ✅ | ✅ |
| Preview email | ✅ | ✅ | ✅ |
| **Parts Catalog** | | | |
| Search parts | ✅ | ✅ | ✅ |
| View parts | ✅ | ✅ | ✅ |
| Add part | ❌ | ✅ | ✅ |
| Edit part | ❌ | ✅ | ✅ |
| Deactivate part | ❌ | ✅ | ✅ |
| Import CSV | ❌ | ✅ | ✅ |
| Export CSV | ❌ | ✅ | ✅ |
| View components | ❌ | ✅ | ✅ |
| **History & Reporting** | | | |
| View own history | ✅ | ✅ | ✅ |
| View all history | ❌ | ✅ | ✅ |
| Export history | ❌ | ✅ | ✅ |
| **Settings** | | | |
| View settings | ❌ | ✅ | ✅ |
| Edit email recipients | ❌ | ✅ | ✅ |
| Reset settings | ❌ | ❌ | ✅ |

---

## 🔄 Migration to Handler-Level Authorization (Future)

**Current State:** Service-level authorization (`IService_VolvoAuthorization`)  
**Future State:** Handler-level authorization with `[Authorize]` attributes or pipeline behaviors

**Migration Steps:**
1. Create `VolvoAuthorizationBehavior<TRequest, TResponse>` implementing `IPipelineBehavior`
2. Add `[Authorize(Roles = "Volvo.Manager")]` attributes to handler classes
3. Register behavior in `App.xaml.cs`:
   ```csharp
   services.AddTransient(typeof(IPipelineBehavior<,>), typeof(VolvoAuthorizationBehavior<,>));
   ```
4. Remove authorization checks from ViewModels
5. Update PRIVILEGES.md with new attribute-based authorization

**Benefits:**
- ✅ Declarative authorization (easier to audit)
- ✅ Consistent enforcement (no ViewModel bypass)
- ✅ Centralized authorization logic (via pipeline behavior)

---

## 📝 YAML Specification (For Future Privilege Code Generator)

```yaml
module: Module_Volvo

roles:
  - name: Volvo.Operator
    description: Standard receiving operator
    
  - name: Volvo.Manager
    description: Supervisor with catalog management
    
  - name: Volvo.Admin
    description: System administrator

authorize:
  # Commands
  - handler: AddPartToShipmentCommandHandler
    roles: [Volvo.Operator, Volvo.Manager, Volvo.Admin]
    
  - handler: AddVolvoPartCommandHandler
    roles: [Volvo.Manager, Volvo.Admin]
    
  - handler: CompleteShipmentCommandHandler
    roles: [Volvo.Operator, Volvo.Manager, Volvo.Admin]
    
  - handler: DeactivateVolvoPartCommandHandler
    roles: [Volvo.Manager, Volvo.Admin]
    
  - handler: ImportPartsCsvCommandHandler
    roles: [Volvo.Manager, Volvo.Admin]
    
  - handler: UpdateShipmentCommandHandler
    roles: [Volvo.Manager, Volvo.Admin]
    
  - handler: UpdateVolvoPartCommandHandler
    roles: [Volvo.Manager, Volvo.Admin]
    
  # Queries
  - handler: ExportPartsCsvQueryHandler
    roles: [Volvo.Manager, Volvo.Admin]
    
  - handler: ExportShipmentsQueryHandler
    roles: [Volvo.Manager, Volvo.Admin]
    
  - handler: GetPartComponentsQueryHandler
    roles: [Volvo.Manager, Volvo.Admin]
```

---

**For more details, see:**
- `Module_Volvo/QUICK_REF.md` - CQRS components inventory
- `Module_Volvo/SETTABLE_OBJECTS.md` - Configuration inventory
- `.github/copilot-instructions.md` - Project-wide standards
- `.specify/memory/constitution.md` - Authorization architecture principles
