# Module_Bulk_Inventory — Repo Pre-requisites

> Work that should be completed **in the existing repo** before the Module_Bulk_Inventory
> implementation begins.  Items are ordered by priority (Critical → Should → Nice-to-Have).

---

## CRITICAL — Must be done before implementation

### P1.1  Create the `IService_VisualCredentialValidator` in Module_Core

**Why:** Any module that drives Infor Visual via automation needs to block shared accounts before
launching a Visual process.  Adding it to Module_Core makes it available to future modules without
coupling.

**Files to create:**
- `Module_Core/Contracts/Services/IService_VisualCredentialValidator.cs`
- `Module_Core/Services/Database/Service_VisualCredentialValidator.cs`

**Contract:**
```csharp
public interface IService_VisualCredentialValidator
{
    /// <summary>Returns true if the credentials are safe to use with Visual automation.</summary>
    bool IsAllowed(string visualUserName);

    /// <summary>Returns a user-facing message when blocked, null when allowed.</summary>
    string? GetBlockedReason(string visualUserName);
}
```

**Blocked usernames (case-insensitive):** `SHOP2` OR `MTMDC` as a user name

**Behaviour when blocked:** The Bulk Inventory **NavigationViewItem must be hidden** (not
disabled; not shown with an error message).  `IsAllowed` is evaluated during startup and on
session creation; the ViewModel controlling the nav item's `Visibility` reads from this service.
The user must update their Visual credentials in Settings before the nav item reappears.

**DI Registration:** Add to `CoreServiceExtensions.cs` as Singleton.

---

### P1.2  Create `IService_VisualUIAutomation` in Module_Core

**Why:** The legacy WinForms app embeds all `System.Windows.Automation` calls inside `VisualLogger`
(which also inherits `Form`).  Extracting a clean, async, testable service into Module_Core prevents
the same mess from occurring in the new module.

**Files to create:**
- `Module_Core/Contracts/Services/IService_UIAutomation.cs`  ← generic, not Visual-specific
- `Module_Core/Services/VisualAutomation/Service_UIAutomation.cs`  ← generic implementation
- `Module_Core/Services/VisualAutomation/Service_VisualInventoryAutomation.cs`  ← Visual-specific
  fill sequences (Transfer + NewTransaction); wraps `IService_UIAutomation`

**Design principle:** `IService_UIAutomation` is entirely window-agnostic — it accepts window
titles, `AutomationElement` references, or `IntPtr` handles.  Visual-specific logic (fill
sequences, AutomationIds, popup dismissal) lives exclusively in
`Service_VisualInventoryAutomation`.  If a future module needs to drive a different application
via UI Automation, it creates its own specific service that also wraps `IService_UIAutomation`
— no changes to the generic service required.

**Interface surface (minimum):**
```csharp
public interface IService_UIAutomation
{
    // ── Window discovery ──────────────────────────────────────────────────────

    /// <summary>Returns the AutomationElement for a top-level window by title, or null if not found within timeout.</summary>
    Task<AutomationElement?> FindWindowAsync(string windowTitle, TimeSpan timeout, CancellationToken ct = default);

    /// <summary>Returns the hwnd for a top-level window matching both class name and window title, or IntPtr.Zero.</summary>
    IntPtr FindWindowByClassAndTitle(string className, string windowTitle);

    /// <summary>Returns true if a window matching both class name and title currently exists.</summary>
    bool WindowExists(string className, string windowTitle);

    /// <summary>
    /// Polls for a popup (matched by class + title) to appear within settleTimeout.
    /// Uses Task.Delay(pollMs) between checks — never spins.
    /// Returns the hwnd if the popup appeared, IntPtr.Zero if it did not appear within the timeout.
    /// Use this for every popup detection point instead of a one-shot WindowExists check,
    /// because Visual can be delayed by hundreds of milliseconds to over a second on loaded networks.
    /// </summary>
    Task<IntPtr> WaitForPopupAsync(string className, string windowTitle, TimeSpan settleTimeout, int pollMs = 100, CancellationToken ct = default);

    // ── Popup lifecycle ───────────────────────────────────────────────────────

    /// <summary>
    /// Polls until the popup (matched by class + title) disappears or the timeout elapses.
    /// Uses Task.Delay(pollMs) between checks — never spins. Throws OperationCanceledException on ct.
    /// </summary>
    Task WaitForWindowToCloseAsync(string className, string windowTitle, TimeSpan timeout, int pollMs = 100, CancellationToken ct = default);

    /// <summary>
    /// If a popup matching className/windowTitle is present, brings it to foreground (verified
    /// via GetForegroundWindow), sends the provided key sequence, then waits for it to close.
    /// Returns true if the popup was found and dismissed, false if it was not present.
    /// </summary>
    Task<bool> DismissPopupIfPresentAsync(string className, string windowTitle, string keySequence, TimeSpan timeout, CancellationToken ct = default);

    // ── Field filling ─────────────────────────────────────────────────────────

    /// <summary>Fills a text field identified by AutomationId; optionally sends Tab after.</summary>
    Task FillFieldAsync(AutomationElement window, string automationId, string value, bool sendTab = false, CancellationToken ct = default);

    /// <summary>Clears existing content then types the new value.</summary>
    Task ClearAndFillFieldAsync(AutomationElement window, string automationId, string value, CancellationToken ct = default);

    // ── Input ─────────────────────────────────────────────────────────────────

    /// <summary>Sends a key sequence to the currently focused window.</summary>
    void SendKeys(string keys);

    /// <summary>
    /// Brings a window handle to the foreground and verifies success via GetForegroundWindow.
    /// Returns false if focus could not be acquired (e.g., another process holds the lock).
    /// </summary>
    bool SetForegroundVerified(IntPtr hwnd);
}
```

**DI Registration:** Add both `IService_UIAutomation` (Singleton) and
`Service_VisualInventoryAutomation` (Singleton) to `CoreServiceExtensions.cs`.

---

### P1.3  Create the MySQL table schema for `bulk_inventory_transactions`

**Why:** The audit log table must exist in the database before any DAO code is written.

**File to create:** `Database/Schemas/bulk_inventory_transactions.sql`

```sql
CREATE TABLE IF NOT EXISTS `bulk_inventory_transactions` (
    `id`                     INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    `created_by_user`        VARCHAR(100)    NOT NULL,
    `created_at`             DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `transaction_type`       ENUM('Transfer','NewTransaction') NOT NULL,
    `part_id`                VARCHAR(50)     NOT NULL,
    `from_warehouse`         VARCHAR(10)     NULL,
    `from_location`          VARCHAR(30)     NULL,
    `to_warehouse`           VARCHAR(10)     NULL,
    `to_location`            VARCHAR(30)     NULL,
    `quantity`               DECIMAL(18,4)   NOT NULL,
    `work_order`             VARCHAR(50)     NULL,
    `lot_no`                 VARCHAR(20)     NULL,
    `status`                 ENUM('Pending','InProgress','WaitingForConfirmation','Success','Failed','Skipped','Consolidated') NOT NULL DEFAULT 'Pending',
    `error_message`          TEXT            NULL,
    `visual_username`        VARCHAR(100)    NOT NULL COMMENT 'Visual user who executed the transaction (never store password)',
    `updated_at`             DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    INDEX `idx_created_by_status` (`created_by_user`, `status`),
    INDEX `idx_part_id`           (`part_id`),
    INDEX `idx_created_at`        (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

---

### P1.4  Review `Service_InforVisualConnect.cs` — remove cross-module model references

**Problem:** `Module_Core/Services/Database/Service_InforVisualConnect.cs` currently imports
`Module_Receiving.Models` (specifically `Model_InforVisualPO` and `Model_InforVisualPart`).
This creates a dependency from `Module_Core` → `Module_Receiving`, which inverts the layering.

**Fix:**
1. Move `Model_InforVisualPO` and `Model_InforVisualPart` to `Module_Core/Models/InforVisual/`
2. Update `Module_Receiving/Data/` Infor Visual DAO to reference the new location
3. Resolve any ambiguous `using` aliases in `Service_InforVisualConnect.cs`

**Files affected:**
- `Module_Core/Services/Database/Service_InforVisualConnect.cs` (remove alias hacks)
- `Module_Core/Models/InforVisual/` (add moved files)
- `Module_Receiving/` (update model references)

---

### P1.5  Add Infor Visual Executable Path, Warehouse Code, and Lot No to Settings

**Why:** No settings currently exist for the Visual executable path, default warehouse code, or
default lot no.  These must be added before the Bulk Inventory module can read them at runtime.

**Pattern to follow (from existing code):**
1. Add entries to `Module_Settings.Core/Defaults/settings.manifest.json` (defines key, display name,
   category, default value, data type)
2. `Dao_SettingsCoreSystem.GetByKeyAsync(category, key)` reads the live value from MySQL (already
   registered — no new DAO needed)
3. Extend `View_Settings_SharedPaths.xaml` and `ViewModel_Settings_SharedPaths.cs` for the new
   UI controls

**Entries to add to `settings.manifest.json`:**
```json
{
  "category": "System",
  "key": "Core.SharedPaths.InforVisualExePath",
  "displayName": "Infor Visual Executable Root Path",
  "defaultValue": "\\\\visual\\visual908$\\VMFG",
  "dataType": "String",
  "scope": "System",
  "permissionLevel": "User",
  "isSensitive": false,
  "validationRules": "Path"
},
{
  "category": "BulkInventory",
  "key": "BulkInventory.Defaults.WarehouseCode",
  "displayName": "Default Warehouse Code",
  "defaultValue": "002",
  "dataType": "String",
  "scope": "System",
  "permissionLevel": "User",
  "isSensitive": false,
  "validationRules": "String"
},
{
  "category": "BulkInventory",
  "key": "BulkInventory.Defaults.LotNo",
  "displayName": "Default Lot Number",
  "defaultValue": "1",
  "dataType": "String",
  "scope": "System",
  "permissionLevel": "User",
  "isSensitive": false,
  "validationRules": "String"
}
```

**Files to update:**
- `Module_Settings.Core/Defaults/settings.manifest.json` — add the three entries above
- `Module_Settings.Core/Views/View_Settings_SharedPaths.xaml` — add path + warehouse + lot no input controls
- `Module_Settings.Core/ViewModels/ViewModel_Settings_SharedPaths.cs` — add observable properties + load/save commands

---

## SHOULD — Strongly recommended before implementation

### P2.1  Create stored procedure stubs in `Database/StoredProcedures/BulkInventory/`

Create the following empty stored procedure files (with header comments) so the DAO layer can be
written alongside the procedures:

| File | Purpose |
|------|---------|
| `sp_BulkInventory_Transaction_Insert.sql` | Insert a new pending transaction row |
| `sp_BulkInventory_Transaction_UpdateStatus.sql` | Update status + error message by ID |
| `sp_BulkInventory_Transaction_GetByUser.sql` | Fetch all pending rows for a user |
| `sp_BulkInventory_Transaction_DeleteById.sql` | Remove a single row |

---

### P2.2  Add `Module_Bulk_Inventory` DI registration stub to `ModuleServicesExtensions.cs`

Add a placeholder private method `AddBulkInventoryModule` (returning immediately) before writing
any module code.  This ensures the DI wiring location is established and avoids the "where does
this go?" question during implementation.

```csharp
// In ModuleServicesExtensions.cs → AddModuleServices()
services.AddBulkInventoryModule(configuration); // Add this call

// New private method
private static IServiceCollection AddBulkInventoryModule(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // TODO: Module_Bulk_Inventory registrations
    return services;
}
```

---

### P2.3  Restructure `MainWindow.xaml` NavigationView — MenuItems and FooterMenuItems

The corrected nav design splits items between `MenuItems` (top-anchored) and `FooterMenuItems`
(bottom-anchored, above the built-in Settings item).

**Target order — MenuItems (top):**
1. Receiving Labels (`ReceivingWorkflowView`)
2. Dunnage Labels (`DunnageLabelPage`)
3. Volvo Dunnage Requisition (`VolvoShipmentEntry`)

**Target order — FooterMenuItems (bottom, above Settings):**
1. Bulk Inventory (`BulkInventoryPage`)  ← NEW, hidden by default via credential check
2. End of Day Reports (`ReportingMainPage`)  ← move from MenuItems
3. Ship/Rec Tools (`ShipRecToolsPage`)  ← move from MenuItems

**Changes required in `MainWindow.xaml`:**
- Remove `End of Day Reports` and `Ship/Rec Tools` from `NavigationView.MenuItems`
- Add `NavigationView.FooterMenuItems` block with all three items above
- Bulk Inventory starts as `Visibility="Collapsed"` (controlled by the credential validator
  at startup/login):

```xml
<NavigationView.FooterMenuItems>
    <NavigationViewItem x:Name="BulkInventoryNavItem"
                        Content="Bulk Inventory"
                        Tag="BulkInventoryPage"
                        Margin="0,0,0,4"
                        Visibility="Collapsed">
        <NavigationViewItem.Icon>
            <FontIcon Glyph="&#xE8B7;" />
        </NavigationViewItem.Icon>
    </NavigationViewItem>
    <NavigationViewItem Content="End of Day Reports" Tag="ReportingMainPage" Margin="0,0,0,4">
        <NavigationViewItem.Icon>
            <FontIcon Glyph="&#xE9F9;" />
        </NavigationViewItem.Icon>
    </NavigationViewItem>
    <NavigationViewItem Content="Ship/Rec Tools" Tag="ShipRecToolsPage" Margin="0,0,0,4">
        <NavigationViewItem.Icon>
            <FontIcon Glyph="&#xE90F;" />
        </NavigationViewItem.Icon>
    </NavigationViewItem>
</NavigationView.FooterMenuItems>
```

---

### P2.4  Refactor `NavView_SelectionChanged` in `MainWindow.xaml.cs`

The switch statement is currently 8 cases and will grow to 9+ with the new module.  Consider
extracting navigation routing to a `Dictionary<string, Type>` or a dedicated
`IService_NavigationRouter` to keep `MainWindow.xaml.cs` from accumulating every module's
routing logic.

---

## NICE-TO-HAVE — Can be deferred

### P3.1  Split `ModuleServicesExtensions.cs` into partial classes

One partial class file per module would make the DI file easier to navigate as the app grows.

### P3.2  Move `ApplicationVariables` pattern to typed settings/session

The legacy app used a static `ApplicationVariables` class for global state.  The current app uses 
`IService_UserSessionManager`, but there are still some places where static access occurs.  A full
audit of static state would reduce hidden coupling.

### P3.3  Document Visual AutomationIds in a central reference file

Create `docs/InforVisual/VisualAutomationIds.md` mapping all confirmed AutomationIds to their
field names (e.g., `4102 = Inventory Transfers: Part ID`).  The legacy app has this knowledge
scattered across dozens of methods.

---

*Last Updated: 2026-03-08 — Updated to reflect approved corrections*
