# Coding Standards & Conventions

Last Updated: 2026-03-21

**MVVM Pattern:**

- **ViewModels:**
  - Inherit from `ViewModel_Shared_Base`.
  - Must be `partial` classes (required for CommunityToolkit.Mvvm attributes).
  - Use `[ObservableProperty]` for properties.
  - Use `[RelayCommand]` for commands.
  - Inject services via constructor.
  - No business logic in code-behind (`.xaml.cs`).
- **Views:**
  - Use `x:Bind` for data binding (compile-time).
  - Code-behind only for UI-specific logic.

**Data Access (DAO):**

- **Naming:** `Dao_<EntityName>`.
- **Pattern:**
  - ALL DAOs are instance-based classes registered in DI (never static).
  - Accept `string connectionString` in constructor.
  - Return `Model_Dao_Result<T>` from all methods; never throw exceptions.
- **Methods:** Async/Await for all DB operations (`<Action><Entity>Async`).
- **Error Handling:** Use `Model_Dao_Result<T>` for return values to encapsulate success/failure and error messages.

**Services:**

- **Naming:** `Service_<Name>`, `IService_<Name>`.
- **Pattern:** Interface-based design, registered in DI container.

**Models:**

- **Naming:** `Model_<EntityName>`.
- **Structure:** Properties match database columns.

**General:**

- **Async:** Use `Async` suffix for async methods.
- **Nullable:** Enable nullable reference types (`<Nullable>enable</Nullable>`).
- **File Scoped Namespaces:** Preferred.
