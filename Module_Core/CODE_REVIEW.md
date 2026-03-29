# Module_Core — Connection Edge Case Code Review

**Date:** 2026-03-29  
**Scope:** MySQL and SQL Server connection edge cases across all shared infrastructure  
**Reviewed Files:**

- `Module_Core/Helpers/Database/Helper_Database_Variables.cs`
- `Module_Core/Helpers/Database/Helper_Database_StoredProcedure.cs`
- `Module_Core/Models/InforVisual/Model_InforVisualConnection.cs`
- `Infrastructure/Configuration/DatabaseSettings.cs`

---

## Summary

All connection-layer edge cases are centralized in `Module_Core` shared infrastructure.
No per-module DAO changes are required. Secondary modules (Receiving, Dunnage, Volvo,
Reporting, ShipRec_Tools, Settings) have no connection-specific issues — they consume
the fixed helpers.

### Good Patterns Confirmed ✅

- All DAOs use `await using` — no connection leak risk
- All transactions have explicit `RollbackAsync()` in catch blocks
- No raw SQL in C# code (MySQL) — stored procedures only
- MySQL pool cleared (`ClearAllPools()`) on shutdown in `App.xaml.cs`
- `Dao_InforVisualConnection`, `Dao_InforVisualPO` both validate `ApplicationIntent=ReadOnly`

---

## Findings

### 🔴 CRITICAL-1 — Security: Hardcoded MySQL credentials in Helper_Database_Variables

**File:** `Module_Core/Helpers/Database/Helper_Database_Variables.cs` (lines 13, 19)

`root`/`root` are embedded as C# string literals in `ProductionConnectionString` and
`TestConnectionString`. `GetInforVisualConnectionString()` similarly hardcodes `SHOP2`/`SHOP`.

**Why this matters:**  
Hardcoded credentials appear in source control, decompiled binaries, and stack traces.
If the password ever changes, a binary redeploy is required instead of a config update.
Any developer with repo access sees the production password.

**Fix:**  
Initialize via `IConfiguration` at startup; remove all literals from source.
Add `Initialize(IConfiguration configuration)` static method reading
`configuration.GetConnectionString("MySql")` and `configuration.GetConnectionString("InforVisual")`.
Call `Helper_Database_Variables.Initialize(configuration)` in `CoreServiceExtensions.AddCoreServices()`.

- [x] Applied

---

### 🔴 CRITICAL-2 — Reliability: No CommandTimeout on any stored procedure execution

**File:** `Module_Core/Helpers/Database/Helper_Database_StoredProcedure.cs`  
**Methods:** `ExecuteAsync` (1), `ExecuteNonQueryAsync` (2), `ExecuteSingleAsync<T>` (3),
`ExecuteListAsync<T>` (4), `ExecuteDataTableAsync` (5), `ExecuteInTransactionAsync` (6)

None of the 6 Execute methods set `MySqlCommand.CommandTimeout`. Default MySQL command
timeout is 30 s; however, without an explicit override there is no mechanism to tune
this value without recompiling. `appsettings.json` already defines
`"InforVisual": { "QueryTimeoutSeconds": 120 }` but the value is never read into
`DatabaseSettings` or applied to commands.

**Why this matters:**  
Long-running batch receives or reporting queries silently hang the UI thread (or async
task) for up to 30 s before failing. There is no way to tune per-environment without
a code change.

**Fix:**  
Add optional `int commandTimeoutSeconds = 30` parameter to all 6 methods. Set
`command.CommandTimeout = commandTimeoutSeconds;` immediately after command construction.
Add `CommandTimeoutSeconds` property to `DatabaseSettings.cs`; callers that need
non-default timeouts can pass the configured value.

- [x] Applied

---

### 🟡 HIGH-1 — Security: Hardcoded SQL Server credentials in Model_InforVisualConnection

**File:** `Module_Core/Models/InforVisual/Model_InforVisualConnection.cs` (lines 12–13)

```csharp
public string UserId   { get; set; } = "SHOP2";
public string Password { get; set; } = "SHOP";
```

Anyone calling `new Model_InforVisualConnection()` without setting credentials gets a
connection string containing `SHOP2`/`SHOP`. These defaults are not sourced from config.

**Why this matters:**  
Model classes appear in unit tests, logs, and serialized state. Hardcoded defaults spread
credentials into test fixtures and logged outputs. Changing the password requires a code
change.

**Fix:**  
Replace initializers with `string.Empty`; callers must supply credentials from
`appsettings.json` (already present as `ConnectionStrings.InforVisual`).

- [x] Applied

---

### 🟢 MEDIUM — CancellationToken support missing (out of scope)

No DAO or helper `Execute` method accepts a `CancellationToken`. Long SQL queries on
the SQL Server side cannot be cancelled when the user navigates away. Deferred to a
future dedicated review.

---

## Variables / False Positives

| Item                                                             | Status                                                       |
| ---------------------------------------------------------------- | ------------------------------------------------------------ |
| `Dao_InforVisualConnection` missing `ValidateReadOnlyConnection` | ✅ False positive — confirmed present at line 29             |
| MySQL pool leak on shutdown                                      | ✅ Not a finding — `ClearAllPools()` called in `App.xaml.cs` |
