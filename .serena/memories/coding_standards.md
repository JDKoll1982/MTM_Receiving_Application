# Coding Standards & Conventions

**MVVM Pattern:**
- **ViewModels:**
  - Inherit from `BaseViewModel` (where applicable).
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
  - `Dao_User` is an instance class registered in DI.
  - Other DAOs (e.g., `Dao_ReceivingLine`) are static classes.
  - *Note: Check existing pattern before creating new DAOs.*
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
