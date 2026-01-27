# Coding Standards & Conventions

**MVVM Pattern:**

- **ViewModels:**
  - Inherit from `ViewModel_Shared_Base` (REQUIRED).
  - Use `[ObservableProperty]` for properties.
  - Use `[RelayCommand]` for commands.
  - Inject services via constructor (errorHandler, logger, notificationService).
  - No business logic in code-behind (`.xaml.cs`).
  - **ObservableProperty Acronym Casing:** Use lowercase for multi-letter acronyms except first letter
    - ✅ `_poNumber` → `PoNumber`, `_isPoValid` → `IsPoValid`, `_isNonPo` → `IsNonPo`
    - ❌ `_isPOValid` → `IsPOValid`, `_isNonPO` → `IsNonPO` (inconsistent casing)
  - **Partial Methods:** Must match generated property name (`OnPoNumberChanged` for `PoNumber` property)
  
- **Views:**
  - Use `x:Bind` for data binding (compile-time).
  - Code-behind only for UI-specific logic.

**ViewModel Constructor Pattern (REQUIRED):**
```csharp
public ViewModel_XXX(
    IService_ErrorHandler errorHandler,
    IService_LoggingUtility logger,
    IService_Notification notificationService) 
    : base(errorHandler, logger, notificationService)
{
}
```

**Data Access (DAO):**

- **Naming:** `Dao_Receiving_Repository_<EntityName>`.
- **Pattern:**
  - All DAOs are instance-based classes registered in DI.
  - Accept `connectionString` in constructor.
  - Use stored procedures via `Helper_Database_StoredProcedure`.
- **Methods:** Async/Await for all DB operations.
- **Error Handling:** Return `Model_Dao_Result<T>` - NEVER throw exceptions.

**Services:**

- **Naming:** `Service_<Module>_<CategoryType>_<Name>`, `IService_<Module>_<CategoryType>_<Name>`.
- **Pattern:** Interface-based design, registered in DI container.

**Models:**

- **Entities (Database Tables):** `Model_Receiving_TableEntitys_<Name>`
- **DTOs (Data Transfer):** `Model_Receiving_DataTransferObjects_<Name>`
- **CQRS Queries:** `QueryRequest_Receiving_Shared_<Action>_<Name>`
- **CQRS Commands:** `CommandRequest_Receiving_Shared_<Action>_<Name>` or custom names

**Error Handling:**

- **IService_ErrorHandler API:**
  - `Task HandleErrorAsync(string errorMessage, Enum_ErrorSeverity severity, Exception? exception, bool showDialog)`
  - `Task ShowErrorDialogAsync(string title, string message, Enum_ErrorSeverity severity)`
  - `Task ShowUserErrorAsync(string message, string title, string method)`
  - `void HandleException(Exception ex, Enum_ErrorSeverity severity, string method, string className)` (sync compatibility)
- **Severity Enum:** `Enum_ErrorSeverity` (Info, Low, Warning, Medium, Error, Critical, Fatal)
  - ❌ NOT `Enum_Shared_Severity_ErrorSeverity`
  - ❌ NOT `High` - use `Critical` instead

**Workflow Session State:**

- **SessionId:** Database uses `Guid`, ViewModel uses `string` (convert as needed)
- **LoadDetailsJson:** Store as serialized JSON string using `System.Text.Json`
  - Save: `JsonSerializer.Serialize(Loads.ToList())`
  - Load: `JsonSerializer.Deserialize<List<Model_Receiving_DataTransferObjects_LoadGridRow>>(...)`

**General:**

- **Async:** Use `Async` suffix for async methods.
- **Nullable:** Enable nullable reference types (`<Nullable>enable</Nullable>`).
- **File Scoped Namespaces:** Preferred.
- **Required Using Directives:** Always include System, System.Linq, System.Threading.Tasks in ViewModels.
