# Data Access Contracts

**Module**: `Module_Reconciliation`
**Namespaces**: `...Module_Reconciliation.Data`, `...Module_Reconciliation.Settings`

The module owns its data access. It reads the receiving tables through its own procedures and writes through exactly one procedure, which is the whole data surface between `Module_Reconciliation` and `Module_Receiving` (`FR-005`, `R-MOD-08`).

No type here is shared with `Module_Receiving`, including procedure names (FR-002).

## Dao_ReconciliationReceivingRows

Instance class, connection material through constructor injection (`R-DATA-01`). Returns `Model_Dao_Result<T>` and does not throw for an expected failure (`R-DATA-02`).

| Method | Returns | Calls |
| --- | --- | --- |
| `GetDayRowsAsync(DateOnly date)` | `Model_Dao_Result<List<Model_ReconciliationRow>>` | `sp_Reconciliation_ReceivingRows_GetDay` |
| `GetActivityDatesAsync(DateOnly from, DateOnly to)` | `Model_Dao_Result<List<DateOnly>>` | `sp_Reconciliation_DayActivity_GetRange` |
| `UpdateLocationAsync(Enum_ReconciliationRowSource source, int recordId, string destinationLocation, string operatorUserId)` | `Model_Dao_Result<int>` | `sp_Reconciliation_ReceivingLocation_Update` |

**Mapping rules**

- `quantity` maps to `Model_ReconciliationRow.Quantity` as an `int`, and `weight_quantity` maps to `WeightQuantity` as a `decimal?`. The two are never collapsed into one property, which is what the Receiving module does today and why the integer quantity is currently invisible.
- `transaction_date` maps to `TransactionDate`; `received_date` maps to `ReceivedAt`.
- `row_source` is projected by the procedure, not inferred by the caller.
- A `null` `weight_quantity` is `null`, not zero, so display can tell "not entered" from "entered zero".

## Stored Procedures

All three live in `Database/Database_Deployment/Sql_Files/StoredProcedures/Reconciliation/` and follow `sp_<Module>_<Area>_<Action>` (`R-SQL-04`), with a header comment block, `SET NOCOUNT ON` on the modifying procedure, explicit `BEGIN` and `COMMIT`, parameterised statements and no dynamic SQL (`R-SQL-03`). Each carries a toggleable test block in the `-- TEST BLOCK START:` / `-- TEST BLOCK END:` form (`R-SQL-05`). Deployment follows the script safety workflow, validated on `mtm_receiving_application_test` first (`R-SQL-06`).

### sp_Reconciliation_ReceivingRows_GetDay

Returns every receiving row for one calendar day, from both row sources.

| Parameter | Type | Direction | Meaning |
| --- | --- | --- | --- |
| `p_transaction_date` | `DATE` | in | the day to load |

**Returns** one row set:

| Column | Type | Notes |
| --- | --- | --- |
| `row_source` | `VARCHAR(16)` | `CurrentLabel` or `History` |
| `record_id` | `INT` | the row's own id in its table |
| `load_id` | `CHAR(36)` | nullable |
| `load_number` | `INT` | nullable, the allocation tie-break |
| `part_id` | `VARCHAR(50)` | |
| `part_description` | `VARCHAR(500)` | nullable |
| `po_number` | `VARCHAR(20)` | nullable, empty for non-purchase-order rows |
| `po_line_number` | `VARCHAR(10)` | nullable |
| `transaction_date` | `DATE` | |
| `received_date` | `DATETIME` | nullable |
| `quantity` | `INT` | the proof's quantity |
| `weight_quantity` | `DECIMAL(18,2)` | nullable, display only |
| `unit_of_measure` | `VARCHAR(20)` | nullable |
| `initial_location` | `VARCHAR(50)` | nullable, the saved location |
| `vendor_name` | `VARCHAR(255)` | coalesced from `vendor_name` and `po_vendor` |
| `is_non_po_item` | `TINYINT(1)` | |
| `is_reprint` | `TINYINT(1)` | returned so the caller can count exclusions |

**Behaviour**

- A `UNION ALL` of `receiving_label_data` and `receiving_history`, filtered on `transaction_date = p_transaction_date` in both branches.
- Re-print rows are returned, not filtered out. The service excludes them and counts them, so the excluded count comes from the same row set the day reports (FR-016).
- Ordered by `part_id`, then `transaction_date`, then `load_number`, then `load_id`, so the caller's input is stable.
- Explicit column lists only, and the filter sits on the indexed `transaction_date` column with no function applied to it (`R-SQL-02`).

### sp_Reconciliation_DayActivity_GetRange

Answers which days have receiving activity, cheaply.

| Parameter | Type | Direction | Meaning |
| --- | --- | --- | --- |
| `p_from` | `DATE` | in | range start, inclusive |
| `p_to` | `DATE` | in | range end, inclusive |

**Returns** one row set with the single column `activity_date DATE`, one row per distinct day holding at least one row across the two tables, ordered ascending.

**Behaviour**

- Groups on `transaction_date` and unions the two tables, then deduplicates.
- Must not join, must not count movements, and must not touch Infor Visual. This is the query that makes day stepping cheap (FR-009).
- A bounded range is enforced by the caller. A sweep asks for its own span, a stepper asks for a window around the current day.

### sp_Reconciliation_ReceivingLocation_Update

The single write procedure. Both modules touch these tables; this procedure is the only write surface between them (`R-MOD-08`, decision D-11).

| Parameter | Type | Direction | Meaning |
| --- | --- | --- | --- |
| `p_row_source` | `VARCHAR(16)` | in | `CurrentLabel` or `History` |
| `p_record_id` | `INT` | in | the row's id in the named table |
| `p_destination_location` | `VARCHAR(50)` | in | the location to write |
| `p_operator_user_id` | `VARCHAR(100)` | in | who accepted the line |
| `p_affected_rows` | `INT` | out | 1 on a successful update, 0 when the row was absent |

**Behaviour**

- `SET NOCOUNT ON`, explicit `BEGIN` and `COMMIT`, one `UPDATE` per branch.
- The table is chosen with `IF p_row_source = 'CurrentLabel' ... ELSEIF p_row_source = 'History' ... ELSE SIGNAL`, never with dynamic SQL (`R-SQL-03`).
- Updates `initial_location` only. It does not touch quantity, part, purchase order, or any audit column, because this slice adds no audit trail (FR-055).
- Rejects an empty `p_destination_location` and an empty `p_operator_user_id`.
- No ownership predicate. The procedure corrects a row regardless of the employee or Windows user who created it, which is `DS-06` and FR-052. Receiving's edit-mode ownership rule lives in Receiving code and is untouched.
- The caller treats a return of 0 affected rows as a failed write, not a success.

## Settings Keys

Both keys live under the module's own prefix (`FR-059`) and are written and read through the settings core facade, never directly. The settings page is `Module_Settings.Reconciliation` (`R-SET-03`).

| Key | Type | Default | Meaning |
| --- | --- | --- | --- |
| `Reconciliation.BusinessRules.MovementDiscoveryWindowDays` | `int` | `2` | days either side of the receipt date used to discover movements |
| `Reconciliation.BusinessRules.StrictValidationWindowDays` | `int` | `1` | days after the receipt date used for strict validation |

**Rules**

- Neither default is migrated from Receiving. `Receiving.BusinessRules.ValidateAllHistoryForLocationReconciliation` disappears with the day scope, and `Receiving.UserPreferences.IgnoredReconciliationLocationsJson` is not migrated (FR-060, `LG-14`).
- The module reads no `Receiving.` key at any point.
- A sweep uses the same two values for every day it touches and never widens them (FR-014).

## Registration

Every service, ViewModel and View is registered in `Infrastructure/DependencyInjection/` and never in the application host (`FR-061`, `R-DI-01`, `R-DI-02`).

- Collaborators and services: singleton, because they hold no per-day state.
- ViewModels and Views: transient.
- `Dao_ReconciliationReceivingRows` and `Dao_ReconciliationInforVisual`: singleton, constructed with their connection material.

The shell edits are one navigation item in `MainWindow.xaml`, one row in `ModuleAccentMap` in `MainWindow.xaml.cs`, and the accent triple in `Module_Core/Themes/ModuleAccentBrushes.xaml`. No service is registered there.
