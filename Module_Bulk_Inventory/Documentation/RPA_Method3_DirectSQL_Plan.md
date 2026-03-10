# Bulk Inventory — Method 3: Direct SQL Writes to MTMFG Plan

Last Updated: 2026-03-10

## 1. Method Overview

Bypass Infor VISUAL's UI entirely. Instead of driving the desktop app, write the inventory
transaction records directly to the **MTMFG SQL Server database** using the same tables and
the same data that VISUAL itself writes — eliminating all windows, automation, DPI, tab
counts, and image templates.

This is the most reliable long-term approach because it has **zero dependency on VISUAL's
UI** — nothing breaks when VISUAL upgrades, the machine DPI changes, or a window moves.

---

## 2. Critical Prerequisite: Constitutional Change Required

The project constitution (`copilot-instructions.md`) currently states:

> **Write operations to SQL Server/Infor Visual — READ ONLY (no INSERT/UPDATE/DELETE)**

The existing connection string enforces this:
```
"InforVisual": "...ApplicationIntent=ReadOnly;"
```

**This method requires an explicit, scoped relaxation of that rule** for the BulkInventory
module only. The relaxation must be approved by the project owner and documented as a
deliberate architectural decision before any implementation begins.

The risk is real: incorrect writes to MTMFG can corrupt inventory counts, causing
production discrepancies that Infor VISUAL cannot automatically detect or roll back.

**This method must NOT be implemented until the schema research (Human Tasks) is complete
and a VISUAL consultant or Infor documentation confirms the write path.**

---

## 3. Why This Is the Most Reliable Approach

| Factor | UI Automation methods (1, 2, image) | Method 3 (Direct SQL) |
|---|---|---|
| Breaks on VISUAL upgrade | Possible | Never — SQL schema rarely changes |
| Requires VISUAL to be running on the machine | Yes | No |
| Requires the VISUAL window to be open | Yes | No |
| Can run headlessly / as a background service | No | Yes |
| Transaction atomicity | No (UI steps can partially complete) | Yes (SQL transaction) |
| Audit trail native to VISUAL | Built in (VISUAL writes it) | Must be confirmed manually |
| Implementation risk | Low (UI only) | High (schema must be perfectly understood) |
| Setup time | Hours | Days–weeks (schema research) |

---

## 4. What VISUAL Writes During an Inventory Transfer (Research Hypothesis)

> ⚠️ These are **hypotheses** based on the CSV schema files. They must be confirmed by
> SQL Profiler tracing (see Human Tasks). **Do not implement based on this section alone.**

Based on `MTMFG_Schema_Tables.csv` and `MTMFG_Schema_FKs.csv`:

### For Inventory Transfer (location-to-location move)

The transfer is expected to insert into `INVENTORY_TRANS` (confirmed: ~3.5M rows in
`MTMFG_Schema_TableRowCounts.csv`), with columns such as:

| Column (hypothetical) | Value |
|---|---|
| `PART_ID` | `row.PartId` |
| `FROM_LOCATION` | `row.FromLocation` |
| `TO_LOCATION` | `row.ToLocation` |
| `FROM_WAREHOUSE_ID` | Warehouse code |
| `TO_WAREHOUSE_ID` | Warehouse code |
| `TRANS_QTY` | `row.Quantity` |
| `TRANS_DATE` | `GETDATE()` |
| `USER_ID` | VISUAL user (`row.VisualUsername`) |
| `TRANS_TYPE` | Some VISUAL-internal code for "Transfer" |

> **These column names are guesses. Run SQL Profiler to confirm exact names, data types,
> and which related tables are also updated.**

VISUAL is also expected to update `PART` quantity on-hand columns and possibly write to a
`INVENTORY_HISTORY` or similar audit table. **Check constraints are encrypted in MTMFG**
(confirmed in the database reference files) so valid enum values for `TRANS_TYPE` and
similar fields cannot be read from schema — they must be captured from Profiler traces.

---

## 5. Architecture

### 5.1 New write-capable connection string

A separate connection string **without** `ApplicationIntent=ReadOnly` is added for
BulkInventory writes only:

```json
"ConnectionStrings": {
  "InforVisual": "...ApplicationIntent=ReadOnly;",
  "InforVisualBulkWrite": "Server=VISUAL;Database=MTMFG;User Id=SHOP2;Password=SHOP;TrustServerCertificate=True;"
}
```

The `InforVisualBulkWrite` string is used exclusively by `Dao_InforVisualInventoryWrite`.
All other DAOs continue using the read-only string. This limits the blast radius of the
constitutional change.

### 5.2 New DAO: `Dao_InforVisualInventoryWrite`

```csharp
public class Dao_InforVisualInventoryWrite
{
    private readonly string _connectionString;

    public Dao_InforVisualInventoryWrite(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
    }

    /// <summary>
    /// Inserts an inventory transfer record into MTMFG, replicating the exact
    /// write that the VISUAL "Inventory Transfers" window performs.
    /// Returns Model_Dao_Result — never throws.
    /// </summary>
    public async Task<Model_Dao_Result> InsertTransferAsync(
        Model_BulkInventoryTransaction row,
        string warehouseCode,
        CancellationToken ct = default)
    { ... }

    /// <summary>
    /// Inserts an inventory transaction entry into MTMFG, replicating the
    /// "Inventory Transaction Entry" window write.
    /// </summary>
    public async Task<Model_Dao_Result> InsertNewTransactionAsync(
        Model_BulkInventoryTransaction row,
        string warehouseCode,
        string lotNo,
        CancellationToken ct = default)
    { ... }
}
```

Implementation uses **parameterized SQL** (never string concatenation) wrapped in a
`SqlTransaction` so partial writes are rolled back on failure.

### 5.3 Updated `Service_VisualInventoryAutomation`

`ExecuteTransferAsync` changes from driving the VISUAL UI to:
```csharp
var result = await _writeDao.InsertTransferAsync(row, warehouseCode, ct);
if (!result.IsSuccess)
{
    await _bulkService.CompleteRowAsync(row.Id, Enum_BulkInventoryStatus.Failed, result.ErrorMessage);
    return;
}
await _bulkService.CompleteRowAsync(row.Id, Enum_BulkInventoryStatus.Success);
```

The entire `Service_UIAutomation` dependency is removed from this service.

### 5.4 SQL Server stored procedures (recommended alternative to inline SQL)

Rather than inline parameterized SQL in C#, the recommended final implementation is to
create SQL Server stored procedures in MTMFG that encapsulate the write logic — mirroring
the pattern used for MySQL in this app. This requires write-create permission on MTMFG,
which must be confirmed with the DBA.

---

## 6. What Is NOT Changing

| Component | Stays the same |
|---|---|
| `IService_VisualInventoryAutomation` interface | Same method signatures |
| Row status model, MySQL persistence | No change |
| `ViewModel_BulkInventory_Push` | No change |
| All existing read-only Infor Visual DAOs | No change — they keep `ApplicationIntent=ReadOnly` |

---

## 7. Phased Implementation

### Phase 0 — Schema Research (HUMAN TASK — MUST COMPLETE FIRST)

Use SQL Profiler to capture exactly what VISUAL writes. Only begin Phase 1 after this is done.
See Human Tasks file.

### Phase 1 — Constitutional Amendment

- Document the decision in `Changes-and-Decisions/Decisions.md` for the BulkInventory module.
- Add `InforVisualBulkWrite` connection string to `appsettings.json` (no `ReadOnly` flag).
- Add DI registration in `Infrastructure/DependencyInjection/` as Singleton.

### Phase 2 — Implement `Dao_InforVisualInventoryWrite`

- Implement `InsertTransferAsync` and `InsertNewTransactionAsync` using parameterized SQL.
- Wrap each in a `SqlTransaction`.
- Test against a VISUAL **test company** (not MTMFG production) first.

### Phase 3 — Update `Service_VisualInventoryAutomation`

- Remove all UI automation code from this service.
- Replace with `_writeDao` calls.
- Remove `IService_UIAutomation` dependency from constructor.

### Phase 4 — Validation

- Run a batch in the VISUAL test company and compare the INVENTORY_TRANS rows inserted
  by the app against rows inserted by the VISUAL UI for the same part/location/quantity.
  They must match exactly.
- Get sign-off from the inventory manager before enabling on MTMFG production.

---

## 8. Risk Table

| Risk | Likelihood | Mitigation |
|---|---|---|
| Schema hypotheses are wrong (wrong columns, wrong table) | **High** | SQL Profiler trace required before any implementation |
| Missing a table that VISUAL also updates (e.g. on-hand qty) | High | Profiler trace captures all writes in the same transaction |
| VISUAL encrypted check constraints allow only specific TRANS_TYPE values | Medium | Profiler trace reveals actual values in use |
| Duplicate transaction inserted (no VISUAL duplicate check equivalent) | Medium | Implement duplicate-check SELECT before INSERT |
| Partial write on app crash mid-transaction | Low | Wrap all writes in SqlTransaction; roll back on failure |
| MTMFG DB schema changes on VISUAL upgrade | Low | SQL schema is more stable than UI; verify after upgrades |
| Write credentials exposed in config | Medium | Use environment-variable or secrets-manager pattern for `InforVisualBulkWrite` password |

---

## 9. Files to Create / Modify

| Action | File |
|---|---|
| CREATE | `Module_Bulk_Inventory/Data/Dao_InforVisualInventoryWrite.cs` |
| CREATE | `Module_Bulk_Inventory/Contracts/Services/IService_VisualInventoryWrite.cs` (optional wrapper) |
| MODIFY | `Module_Bulk_Inventory/Services/Service_VisualInventoryAutomation.cs` (remove UI automation) |
| MODIFY | `appsettings.json` (add `InforVisualBulkWrite` connection string) |
| MODIFY | `Infrastructure/DependencyInjection/` (register DAO) |
| CREATE | `Module_Bulk_Inventory/Documentation/Changes-and-Decisions/Decisions.md` (constitutional amendment entry) |
| OPTIONAL CREATE | `Database/InforVisualScripts/WriteProcs/sp_InsertInventoryTransfer.sql` |
| OPTIONAL CREATE | `Database/InforVisualScripts/WriteProcs/sp_InsertInventoryTransaction.sql` |
