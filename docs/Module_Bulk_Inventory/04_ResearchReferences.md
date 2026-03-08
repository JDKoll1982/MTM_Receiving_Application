# Module_Bulk_Inventory — Research References

> Quick-access lookup table of every internal resource needed during implementation.
> Read this before opening any implementation file to orient yourself.

---

## 1 — Infor Visual Automation

### 1.1  Confirmed Window Titles (from legacy `VisualLogger.cs`)

| Window | Exact Title String | Notes |
|--------|--------------------|-------|
| Main Visual hub | `"Infor VISUAL - {server}/{username}"` | e.g. `"Infor VISUAL - MTMFG/JKOLL"` |
| Inventory Transaction Entry | `"Inventory Transaction Entry - Infor VISUAL - MTMFG"` | Used for New Transaction mode |
| Inventory Transfers | `"Inventory Transfers"` | Used for Transfer mode |
| Parts lookup popup | window class `"Gupta:AccFrame"`, title `"Parts"` | Appears when a Part ID is not recognized; dismiss with `{UP}{ENTER}` |
| Duplicate transaction warning | window class `"Gupta:AccFrame"`, title `"Inventory Transaction Entry"` | Appears mid-fill on To-Location tab; wait-loop until dismissed by user |

### 1.2  Confirmed AutomationId Map

| AutomationId | Window | Field | Notes |
|---|---|---|---|
| `4102` | Inventory Transfers | Part ID | Fill first; triggers Part lookup |
| `4111` | Inventory Transfers | Quantity | Must focus and wait ~1s before value sticks |
| `4123` | Inventory Transfers | From Warehouse | Hardcoded `"002"` |
| `4124` | Inventory Transfers | From Location | User-supplied |
| `4142` | Inventory Transfers | To Warehouse | Hardcoded `"002"` |
| `4143` | Inventory Transfers | To Location | Fill last; triggers duplicate-check popup |
| `4115` | Inventory Transaction Entry | Work Order | New Transaction mode only; format `WO-######` (e.g. `WO-123456`) |
| `4116` | Inventory Transaction Entry | Lot No | Hardcoded `"1"` |
| `4143` | Inventory Transaction Entry | Quantity | ⚠️ Same ID as Transfer To-Location — different window! |
| `4148` | Inventory Transaction Entry | To Warehouse | Hardcoded `"002"` |
| `4152` | Inventory Transaction Entry | To Location | New Transaction mode only |
| `4127` | Inventory Transaction Entry | Part Number (read-back) | Used for Google Sheets (now removed — but useful for confirmation) |

> ⚠️ `4143` appears in BOTH windows but means different things:
> - In *Inventory Transfers* → **To Location**
> - In *Inventory Transaction Entry* → **Quantity**
> Always check the window handle before filling by AutomationId.

### 1.3  Executable Paths (confirmed from legacy)

```
\\visual\visual908$\VMFG\VM.exe          ← Full Visual login (used only if VMINVENT is not directly accessible)
\\visual\visual908$\VMFG\VMINVENT.exe    ← Inventory module direct launch (preferred)
```

Launch args format: `-d MTMFG -u {visualUserName} -p {visualPassword}`

> ⚠️ These args are visible to any user running Task Manager or Get-Process.
> See `02_Suggestions.md §5.1` for mitigation options.

### 1.4  Fill Sequence — Transfer Mode

**Two popups can appear during this sequence.**  Both must be handled before proceeding.

| Popup | Class | Title | Trigger | Correct action |
|-------|-------|-------|---------|----------------|
| Parts lookup | `Gupta:AccFrame` | `"Parts"` | After Part ID fill when Visual can’t auto-resolve the part | Bring to foreground (verified), send `{UP}{ENTER}`, wait for window to close |
| Duplicate transaction warning | `Gupta:AccFrame` | `"Inventory Transaction Entry"` | After To Location fill + TAB when Visual detects a duplicate | Do NOT dismiss programmatically — wait for user to manually close it; row status → `WaitingForConfirmation` |

```
[Pre-Step] Consolidate batch: group rows sharing (PartID, FromLocation, ToLocation)
           and sum their Quantities. Iterate over the consolidated rows only.
1. FindWindow("Inventory Transfers")
   → If not found: check VMINVENT.exe is running; navigate via ALT+E, S
2. FillField("4102", partId)              → Part ID
3. await Task.Delay(1000)                 → Wait for Visual to process Part ID
   [Popup check A — lag-tolerant] WaitForPopupAsync("Gupta:AccFrame", "Parts", settleTimeout: 2s, pollMs: 100)
      → Polls every 100 ms for up to 2 s; returns hwnd when popup appears or IntPtr.Zero on timeout.
         A one-shot WindowExists check is not used here because Visual can show this popup
         several hundred milliseconds after the field fill returns on slower networks.
      → If hwnd != IntPtr.Zero: SetForegroundVerified(hwnd) → {UP}{ENTER} → WaitForWindowToCloseAsync(3s)
         Log "Parts popup dismissed for part {partId}"
      → If IntPtr.Zero: no popup — continue to Quantity fill
4. FillField("4111", quantity)            → Quantity
5. FillField("4123", warehouseCode)       → From Warehouse (default "002" from settings)
6. FillField("4124", fromLocation)        → From Location
7. FillField("4142", warehouseCode)       → To Warehouse (default "002" from settings)
8. FillField("4143", toLocation) + TAB    → To Location (TAB triggers duplicate-check in Visual)
   [Popup check B — lag-tolerant] WaitForPopupAsync("Gupta:AccFrame", "Inventory Transaction Entry", settleTimeout: 2s, pollMs: 100)
      → Polls every 100 ms for up to 2 s. A fixed ~300 ms delay is not used here because Visual
         processes the duplicate check server-side; on loaded servers the popup can appear
         well past 300 ms.
      → If IntPtr.Zero (did not appear within settle timeout):
           Row status → Success
      → If hwnd != IntPtr.Zero (popup appeared):
           Row status → WaitingForConfirmation
           UI shows: "Visual requires confirmation — please dismiss the dialog in Visual to continue"
           await WaitForWindowToCloseAsync("Gupta:AccFrame", "Inventory Transaction Entry", 5min, ct)
              → On timeout/cancel: row status → Failed, error = "User did not confirm duplicate check"
              → On closed: row status → Success
9. Log result via IService_LoggingUtility
```

### 1.5  Fill Sequence — New Transaction Mode

**No popup is expected during the New Transaction fill sequence** (the duplicate-check popup
only appears in Transfer mode from the To Location TAB event).  However, a Parts lookup popup
could theoretically appear; check for it after Work Order entry as a precaution.

```
[Pre-Step] Consolidate batch (same rule applies — group on PartID+Location, sum Quantity).
1. FindWindow("Inventory Transaction Entry - Infor VISUAL - MTMFG")
   → If Transfer window is open first: close it via WM_CLOSE (only if app opened it)
2. FillField("4115", workOrder) + TAB     → Work Order (format: WO-######, e.g. WO-123456)
   [Popup check A — precautionary, lag-tolerant]
      WaitForPopupAsync("Gupta:AccFrame", "Parts", settleTimeout: 2s, pollMs: 100)
      → Popup B is not expected in New Transaction mode, but Popup A can still appear
         if the Work Order field triggers a parts lookup. Poll for 2 s rather than
         checking once — same lag reasoning as Transfer mode.
3. FillField("4116", lotNo) + TAB         → Lot No (from settings, default "1")
4. FillField("4143", quantity)            → Quantity (no tab) ⚠️ same ID as Transfer To-Location
5. FillField("4148", warehouseCode)       → To Warehouse (default "002" from settings)
6. FillField("4152", toLocation) + TAB   → To Location
7. Row status → Success
8. Log result via IService_LoggingUtility
```

### 1.6  Popup Reference Summary

| Popup name | Class | Title | Appears when | How to detect | How to dismiss |
|---|---|---|---|---|---|
| Parts lookup | `Gupta:AccFrame` | `"Parts"` | Part ID not auto-resolved by Visual | `WindowExists(class, title)` | `SetForegroundVerified` → `{UP}{ENTER}` → `WaitForWindowToCloseAsync` |
| Duplicate transaction warning | `Gupta:AccFrame` | `"Inventory Transaction Entry"` | TAB after To Location fill in Transfer mode | `WindowExists(class, title)` | User must dismiss manually; row → `WaitingForConfirmation` |

---

## 2 — Internal Code Reference Files

### 2.1  Legacy Implementation

| File | Purpose |
|------|---------|
| [MTM Receiving Manager/Classes/VisualLogger.cs](../../MTM%20Receiving%20Manager/Classes/VisualLogger.cs) | **Primary reference** — all Win32 + UI Automation methods that drive Visual |
| [MTM Receiving Manager/Windows/MainForm.cs](../../MTM%20Receiving%20Manager/Windows/MainForm.cs) | Data source for the legacy transaction grid (Google Sheets rows) |
| [MTM Receiving Manager/Windows/MainForm.Designer.cs](../../MTM%20Receiving%20Manager/Windows/MainForm.Designer.cs) | WinForms control layout and event wiring |
| [MTM Receiving Manager/Classes/ApplicationVariables.cs](../../MTM%20Receiving%20Manager/Classes/ApplicationVariables.cs) | Global static credential/config store (anti-pattern — do not copy) |

### 2.2  Existing Module Patterns to Follow

| File / Folder | What to Learn From It |
|---|---|
| [Module_ShipRec_Tools/](../../Module_ShipRec_Tools/) | Best example of a tool-hub module with navigation service + multiple sub-views |
| [Module_ShipRec_Tools/Services/Service_ShipRecTools_Navigation.cs](../../Module_ShipRec_Tools/Services/Service_ShipRecTools_Navigation.cs) | Registry/routing pattern for a multi-tool module |
| [Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs](../../Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs) | How to write a SQL Server READ-ONLY DAO |
| [Module_Core/Services/Database/Service_InforVisualConnect.cs](../../Module_Core/Services/Database/Service_InforVisualConnect.cs) | Read-only Infor Visual service wrapper |
| [Module_Core/Helpers/Database/Helper_Database_StoredProcedure.cs](../../Module_Core/Helpers/Database/Helper_Database_StoredProcedure.cs) | MySQL stored procedure execution helper with retry logic |
| [Module_Core/Contracts/Services/IService_InforVisual.cs](../../Module_Core/Contracts/Services/IService_InforVisual.cs) | Interface shape for Visual services |
| [Module_Receiving/Data/](../../Module_Receiving/Data/) | Example DAO + SP pair for MySQL CRUD |
| [Infrastructure/DependencyInjection/ModuleServicesExtensions.cs](../../Infrastructure/DependencyInjection/ModuleServicesExtensions.cs) | Where to register new module DAOs, services, ViewModels, Views |
| [MainWindow.xaml.cs](../../MainWindow.xaml.cs) | NavView routing switch; where to add `"BulkInventoryPage"` case |
| [MainWindow.xaml](../../MainWindow.xaml) | Nav restructure required: `End of Day Reports` and `Ship/Rec Tools` move to `FooterMenuItems`; Bulk Inventory added to `FooterMenuItems` as `Visibility="Collapsed"`. See nav order in `03_PreRequisites.md §P2.3`. |

### 2.3  ViewModel Base Class and MVVM Utilities

| File | Notes |
|------|-------|
| `Module_Core/ViewModels/ViewModel_Shared_Base.cs` | Base class — all new ViewModels must inherit this |
| CommunityToolkit.Mvvm `[ObservableProperty]` | Use on `private` backing fields; generates public property + notification |
| CommunityToolkit.Mvvm `[RelayCommand]` | Use on `private async Task` methods; generates `ICommand` |

### 2.4  Infor Visual Database Reference

| File | Contents |
|------|---------|
| [Module_Core/Data/InforVisual/INFOR_VISUAL_QUERIES.md](../../Module_Core/Data/InforVisual/INFOR_VISUAL_QUERIES.md) | All verified SQL queries with column names tested against live DB |
| [Database/InforVisualScripts/Queries/03_GetPartByNumber.sql](../../Database/InforVisualScripts/Queries/03_GetPartByNumber.sql) | Part ID validation query (reuse for pre-validation step) |
| [Database/InforVisualScripts/Queries/06_FuzzySearchPartsByID.sql](../../Database/InforVisualScripts/Queries/06_FuzzySearchPartsByID.sql) | Fuzzy part search (useful for auto-complete in the entry grid) |
| [docs/InforVisual/DatabaseReferenceFiles/](../../docs/InforVisual/DatabaseReferenceFiles/) | CSV schema exports: Tables, PKs, FKs, Indexes, ColumnDetails |

### 2.5  Connection String Reference

| Key | Value Location | Notes |
|-----|----------------|-------|
| `MySql` | `appsettings.json` → `ConnectionStrings:MySql` | Used by all module DAOs |
| `InforVisual` | `appsettings.json` → `ConnectionStrings:InforVisual` | READ ONLY — `ApplicationIntent=ReadOnly` required |
| Visual exe credentials | `IService_UserSessionManager.CurrentSession?.User.VisualUsername` and `.VisualPassword` (fields on `Model_User`, populated from the `users` MySQL table) | Fetch at point-of-use only; never store as static/instance fields |
| Visual exe path | `Dao_SettingsCoreSystem.GetByKeyAsync("System", "Core.SharedPaths.InforVisualExePath")` — setting to be added per `03_PreRequisites.md §P1.5` | Default `\\visual\visual908$\VMFG`; configurable in Settings → Shared Paths |
| Default warehouse code | `Dao_SettingsCoreSystem.GetByKeyAsync("BulkInventory", "BulkInventory.Defaults.WarehouseCode")` | Default `"002"` |
| Default lot no | `Dao_SettingsCoreSystem.GetByKeyAsync("BulkInventory", "BulkInventory.Defaults.LotNo")` | Default `"1"` |

> ⚠️ `Helper_Database_Variables.GetInforVisualConnectionString()` currently embeds `SHOP2`/`SHOP`
> as the SQL Server credentials (for the DB read connection — separate from the Visual app
> login).  This is a pre-existing pattern; do NOT change it for the Bulk Inventory module.
> The credential guard (`IService_VisualCredentialValidator`) only applies to the
> **Visual application** login — not to the SQL Server read connection.

---

### 2.6  Settings Pattern Reference

The project uses `settings.manifest.json` as the canonical default-values definition, backed by a
MySQL `settings` table read via `Dao_SettingsCoreSystem`.  The UI for shared path settings lives in
`View_Settings_SharedPaths.xaml`.

| File | Role |
|------|------|
| [Module_Settings.Core/Defaults/settings.manifest.json](../../Module_Settings.Core/Defaults/settings.manifest.json) | Declares all system settings: key, display name, category, default value, data type |
| [Module_Settings.Core/Data/Dao_SettingsCoreSystem.cs](../../Module_Settings.Core/Data/Dao_SettingsCoreSystem.cs) | DAO: `GetByKeyAsync(category, key)` and `GetByCategoryAsync(category)` |
| [Module_Settings.Core/Models/Model_CoreSetting.cs](../../Module_Settings.Core/Models/Model_CoreSetting.cs) | Shape of a settings row: `Category`, `SettingKey`, `SettingValue`, etc. |
| [Module_Settings.Core/Views/View_Settings_SharedPaths.xaml](../../Module_Settings.Core/Views/View_Settings_SharedPaths.xaml) | UI stub for shared paths — extend here for Visual exe path, warehouse, lot no |
| [Module_Settings.Core/ViewModels/ViewModel_Settings_SharedPaths.cs](../../Module_Settings.Core/ViewModels/ViewModel_Settings_SharedPaths.cs) | ViewModel for the above view; add observable properties + load/save commands |

**Keys to add (per `03_PreRequisites.md §P1.5`):**
- `"Core.SharedPaths.InforVisualExePath"` in category `"System"`
- `"BulkInventory.Defaults.WarehouseCode"` in category `"BulkInventory"`
- `"BulkInventory.Defaults.LotNo"` in category `"BulkInventory"`

---

## 3 — Documentation References

| Document | Contents |
|---------|---------|
| [.github/copilot-instructions.md](../../.github/copilot-instructions.md) |🔴 MUST READ — All MVVM rules, naming conventions, forbidden patterns |
| [.github/instructions/infor-visual-database-reference.instructions.md](../../.github/instructions/infor-visual-database-reference.instructions.md) | Schema CSV file guide for writing Infor Visual SQL queries |
| [.github/instructions/infor-visual-query-authoring.instructions.md](../../.github/instructions/infor-visual-query-authoring.instructions.md) | Step-by-step guide for new Visual SQL queries |
| [.github/instructions/sql-sp-generation.instructions.md](../../.github/instructions/sql-sp-generation.instructions.md) | MySQL stored procedure naming, structure, and parameter conventions |
| [docs/Module_Bulk_Inventory/01_Assumptions.md](./01_Assumptions.md) | All assumptions about the module — must be approved before starting |
| [docs/Module_Bulk_Inventory/02_Suggestions.md](./02_Suggestions.md) | Improvement ideas to incorporate |
| [docs/Module_Bulk_Inventory/03_PreRequisites.md](./03_PreRequisites.md) | Repo changes needed before implementation |
| [docs/Module_Bulk_Inventory/05_LegacyPitfalls.md](./05_LegacyPitfalls.md) | Specific bugs/anti-patterns from the legacy app to avoid |

---

## 4 — Named Things to Look Up During Implementation

These are specific identifiers you will need to reference; captured here to save search time.

| Symbol | Location | Purpose |
|--------|----------|---------|
| `Model_User.VisualUsername` / `.VisualPassword` | `Module_Core/Models/Systems/Model_User.cs` | Visual app login credentials — fetch from `IService_UserSessionManager.CurrentSession?.User` at point of use |
| `Dao_SettingsCoreSystem.GetByKeyAsync` | `Module_Settings.Core/Data/Dao_SettingsCoreSystem.cs` | Read a system setting by category + key |
| `View_Settings_SharedPaths.xaml` | `Module_Settings.Core/Views/` | Add Visual exe path, warehouse, and lot no UI controls here |
| `ViewModel_Settings_SharedPaths` | `Module_Settings.Core/ViewModels/` | ViewModel for shared path settings; add observable properties |
| `IService_UIAutomation` | `Module_Core/Contracts/Services/` (to be created) | Generic window automation — not Visual-specific |
| `Service_VisualInventoryAutomation` | `Module_Core/Services/VisualAutomation/` (to be created) | Visual-specific fill sequences, wraps `IService_UIAutomation` |
| `ViewModel_Shared_Base` | `Module_Core/ViewModels/` | Inherit all new ViewModels from this |
| `IService_ErrorHandler` | `Module_Core/Contracts/Services/` | Show user errors; log silently |
| `IService_LoggingUtility` | `Module_Core/Contracts/Services/` | Structured logging via Serilog |
| `IService_UserSessionManager` | `Module_Core/Contracts/Services/` | Get current user's Visual credentials |
| `Helper_Database_StoredProcedure.ExecuteAsync` | `Module_Core/Helpers/Database/` | MySQL stored procedure — no return set |
| `Helper_Database_StoredProcedure.ExecuteWithResultsAsync` | `Module_Core/Helpers/Database/` | MySQL stored procedure — returns rows |
| `Helper_SqlQueryLoader` | `Module_Core/Helpers/Database/` | Load `.sql` files from `Database/InforVisualScripts/Queries/` |
| `Model_Dao_Result<T>` | `Module_Core/Models/Core/` | Standard DAO return type |
| `Model_Dao_Result_Factory` | `Module_Core/Models/Core/` | Factory for `Success<T>()` / `Failure<T>()` |
| `IService_ToolDefinition` / `Model_ToolDefinition` | `Module_ShipRec_Tools/` | Tool registry pattern (if module uses sub-tools) |
| `NavigateWithDI` | `MainWindow.xaml.cs` | Use this (not `ContentFrame.Navigate`) for DI-resolved pages |
| `WindowHelper_WindowSizeAndStartupLocation` | `Module_Core/Helpers/UI/` | Set window size at startup |

---

*Last Updated: 2026-03-08 — Updated to reflect approved corrections*
