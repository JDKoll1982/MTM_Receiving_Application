# Infor Visual Query Contracts

**Folder**: `Database/InforVisualScripts/Queries/Reconciliation/`
**Access**: read-only, `ApplicationIntent=ReadOnly`. Every file is a `SELECT` (`R-DATA-04`, `R-SQL-07`).

Every table and column named here was verified against `docs/development/InforVisual/DatabaseCSVFiles/` before the contract was written (`R-DATA-05`). The three files are embedded resources through the existing `Database\InforVisualScripts\Queries\**\*.sql` glob, and `Helper_SqlQueryLoader` resolves them by dotted path, so the `Reconciliation` folder name is part of every load call.

## What Moves, What Stays, What Is New

| File | Disposition | Loaded by |
| --- | --- | --- |
| `16_GetReceivingLocationEvidence.sql` | stays in `Queries/` | `Module_Receiving`, for entry-time suggestion |
| `17_GetReceivingLocationTransactionHistory.sql` | deleted, zero callers (R-7) | nothing |
| `18_GetReceivingLocationTransferMovements.sql` | superseded by 18a and 18b, then deleted | nothing |
| `18a_GetTransferCandidatePairs.sql` | moves, direction fixed | `Dao_ReconciliationInforVisual` |
| `18b_GetTransferPairDetails.sql` | moves | `Dao_ReconciliationInforVisual` |
| `18c_GetLocationMovementAfterWindow.sql` | new | `Dao_ReconciliationInforVisual` |

Nothing is renumbered. The file names on disk keep their numbers, and the folder is what changes (`AR-09`).

## 18a_GetTransferCandidatePairs

Finds the transfer pairs worth hydrating for one part around one receipt date.

| Parameter | Type | Meaning |
| --- | --- | --- |
| `@PartId` | `nvarchar(50)` | the part |
| `@ReceivedDate` | `datetime` | the receipt-date anchor |
| `@DiscoveryWindowDays` | `int` | days either side, from settings |

**Returns** `SourceTransactionId INT`, `DestinationTransactionId INT`, one row per pair.

**Behaviour**

- Window is `CAST(TRANSACTION_DATE AS date) BETWEEN @ReceivedDate - @DiscoveryWindowDays AND @ReceivedDate + @DiscoveryWindowDays`. The window is a parameter, never a literal, and never widened by a sweep (FR-014).
- Both legs of a pair point at each other through `TRANSFER_TRANS_ID`.
- **Direction comes from `TYPE`**: the leg with `TYPE='O'` is the source, the leg with `TYPE='I'` is the destination (`R-DATA-09`, decision D-04). The old lower-id-is-source heuristic is removed, because a pair whose out leg carries the higher id would invert every bucket and silently certify a wrong residual.
- Ties within one date are ordered by `TRANSACTION_ID` so the result is stable, but that ordering never decides direction.
- Explicit column list, no `SELECT *`, and the date filter stays a range on the indexed column with no function applied to the column itself; the cast is on the parameter side where practical (`R-SQL-02`).

## 18b_GetTransferPairDetails

Hydrates the pairs 18a returned.

| Parameter | Type | Meaning |
| --- | --- | --- |
| `@PartId` | `nvarchar(50)` | the part, used as a fallback |
| `@SourceTransactionIds` | id list | the source ids from 18a |

**Returns** one row per movement, mapping one to one onto `Model_ReconciliationMovement`:

| Column | Type | Notes |
| --- | --- | --- |
| `PartId` | `nvarchar(30)` | |
| `PONumber` | `nvarchar(15)` | |
| `POLineNumber` | `nvarchar(10)` | |
| `SourceWarehouseId` | `nvarchar(15)` | from the `TYPE='O'` leg |
| `SourceLocationId` | `nvarchar(15)` | from the `TYPE='O'` leg |
| `DestinationWarehouseId` | `nvarchar(15)` | from the `TYPE='I'` leg |
| `DestinationLocationId` | `nvarchar(15)` | from the `TYPE='I'` leg |
| `TransferQuantity` | `decimal` | the inbound leg's `QTY`, always positive |
| `TransactionDate` | `datetime` | the inbound leg's `TRANSACTION_DATE` |
| `TransactionUserId` | `nvarchar(20)` | the inbound leg's `USER_ID` |
| `SourceTransactionId` | `int` | |
| `DestinationTransactionId` | `int` | |

**Behaviour**

- Rows whose source and destination locations are blank or equal are excluded.
- Rows with a quantity that is zero or negative are excluded. Quantity is never negated to express direction (`R-DATA-09`, FR-030).
- Ordered by `TransactionDate`, then `DestinationTransactionId`, then `SourceTransactionId`.
- The id list is the only dynamic-looking element, and it is parameterised rather than concatenated. The candidate set for one part and one day stays small, which is why a table-valued parameter is not needed.

## 18c_GetLocationMovementAfterWindow

Supplies the as-of balance correction the residual check needs (decision D-02).

| Parameter | Type | Meaning |
| --- | --- | --- |
| `@PartId` | `nvarchar(50)` | the part |
| `@AfterDate` | `datetime` | the last day of the strict window; movements after this date are netted |
| `@WarehouseIds` | id list | the warehouses of interest |
| `@LocationIds` | id list | the locations of interest |

**Returns** `WarehouseId`, `LocationId`, `MovementAfterWindow DECIMAL`, where the quantity is `SUM(TYPE='I') - SUM(TYPE='O')` per location over every transaction after `@AfterDate`.

**Behaviour**

- Read-only and bounded by part, warehouse and location, so it stays a small scan on an indexed part (`R-SQL-02`, `DS-04`).
- Combines with `CR_PART_LOCATION.QTY` to give the balance as of the end of the strict window: `QTY - MovementAfterWindow`.
- For a same-day run the result is empty and the balance reduces to live on-hand, which is the figure the acceptance scenarios name.

## Current On-Hand

The on-hand half of the as-of balance is read from `CR_PART_LOCATION`, the view whose `ID` column is the part and whose columns include `WAREHOUSE_ID`, `LOCATION_ID`, `QTY` and `COMMITTED_QTY`. It is a live snapshot, which is exactly why 18c exists (R-4).

The module reads it through its own DAO. It does not call `Module_Core`'s Visual DAO and does not reuse any Receiving query.

## Not Built In This Slice

`RECEIVER` and `RECEIVER_LINE` are **not** queried. The join from a receiver line to a part runs through `PURC_ORDER_LINE`, and it yields a purchase-order-line quantity for one receipt event with no link to an individual skid, so it cannot attribute a quantity to a receiving row. Building it would add a fragile ERP join to satisfy a research question, which is exactly what `C-01` says not to do (decision R-2).

If a true received quantity is ever needed, it comes from `INVENTORY_TRANS` where `TYPE='I'` and `CLASS='R'` carry `PART_ID`, `PURC_ORDER_ID`, `PURC_ORDER_LINE_NO`, `QTY` and `TRANSACTION_DATE`, which is the same ledger the movement evidence already uses.

## Manual Validation

The automated suite never reaches Infor Visual (`TS-03`). These three queries are validated by hand against the live database before the module is wired, using the `MMC0001000` calibration case and the `MMC0000650` five-row example, and the results are recorded in the module's documentation. Service tests run over fakes only.
