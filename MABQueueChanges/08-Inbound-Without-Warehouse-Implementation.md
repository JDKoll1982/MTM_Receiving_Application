# Implementation — Inbound Purchase Orders With No Warehouse

Last Updated: 2026-10-19

Status: **Applied**. Files: `19_GetMaterialAvailabilityIncomingSupply.sql`,
`Service_Tool_MaterialAvailabilityBoard.cs`.

## Defect 1 — warehouse predicate excluded null-warehouse POs

Query 19 required a warehouse match:

```sql
AND (
      pol.WAREHOUSE_ID = np.WarehouseCode
      OR (
          NULLIF(LTRIM(RTRIM(pol.WAREHOUSE_ID)), '') IS NULL
          AND po.WAREHOUSE_ID = np.WarehouseCode
      )
    )
```

When both `PURC_ORDER_LINE.WAREHOUSE_ID` and `PURCHASE_ORDER.WAREHOUSE_ID` are NULL — the normal
shape for coil purchases, where the receiving location decides the destination — neither branch
matches, so the row was dropped for every warehouse.

Measured on MTMFG 2026-10-19 over active lines with a remaining balance:

| Warehouse source | Open lines | Distinct parts |
| ---------------- | ---------- | -------------- |
| Both line and header NULL | 899 | 333 |
| Line = 002 | 70 | 45 |

That is 93% of all open inbound material.

Reported parts:

| Part | Open lines with a balance | Remaining |
| ---- | ------------------------- | --------- |
| MMC0000364 | 6 | 652,000 |
| MMC0000366 | 5 | 381,534 |
| MMC0000659 | 4 | 346,197 |
| MMC0000850 | 2 | 95,000 |
| Total | 17 | 1,474,731 |

All 17 had NULL warehouse at both levels. `mmc0000364`, `mmc0000366`, `mmc0000659`, and
`mmc0000850` are also at stock locations in warehouse `002` (or, for `MMC0000850`, stocked
elsewhere), so the cards existed but their incoming section was empty.

## Fix 1

The predicate is removed outright. Warehouse `002` is the only warehouse, the board is pinned to it
by `ViewModel_Tool_MaterialAvailabilityBoard.DefaultWarehouseCode`, and `RequestedParts` is already
scoped to the location or part being viewed, so the filter added no scoping value:

```sql
  -- No warehouse filter. Warehouse 002 is the only warehouse in MTMFG and the board is already
  -- warehouse-scoped by configuration, while most coil purchase orders carry no warehouse at all
  -- (899 of 969 open lines had neither a line nor a header warehouse). Filtering on it hid 93% of
  -- inbound material. @WarehouseCode is still reported in the WarehouseCode column.
```

`np.WarehouseCode` remains in the projection (`np.WarehouseCode AS WarehouseCode`), so the result
shape and the DAO mapping are unchanged.

Warehouse values actually present in MTMFG, checked 2026-10-19:

| Table | Values |
| ----- | ------ |
| `CR_PART_LOCATION` | `002` (260,047 rows, all positive stock), NULL (140), `' 002'` (10), `022` (1) — every non-`002` row has zero quantity |
| `PURC_ORDER_LINE` | NULL (195,213), `002` (546), `DC-FG` (2) |
| `PURCHASE_ORDER` | NULL (70,791), `002` (286), `DC-FG` (1), `' '` (1) |

No open line with a remaining balance sits outside `002`, so removing the filter cannot pull in
another warehouse's demand.

Query 18 keeps its warehouse scoping but now normalizes the comparison
(`UPPER(LTRIM(RTRIM(cpl.WAREHOUSE_ID))) = np.WarehouseCode` in both places), because ten stock rows
store the value as `' 002'`.

## Defect 2 — blanket releases ignored their dates

`BuildIncomingPresentation` handled every blanket row as history:

```csharp
if (row.IsBlanketOrder)
{
    qualifyingRollupRows.Add(row);
    if (row.LineLastReceivedDate is DateTime lastReceivedDate) { add "Last received on" }
    continue;   // promise / desired dates never considered
}
```

Measured: 97 open blanket lines, 87 with no `LAST_RECEIVED_DATE`, and all 87 carrying a promise or
desired-receive date. Those 87 rendered with no date at all.

## Fix 2

```csharp
if (row.IsBlanketOrder)
{
    qualifyingRollupRows.Add(row);

    if (row.LineLastReceivedDate is DateTime lastReceivedDate)
    {
        qualifyingLines.Add(
            new IncomingLinePresentation(
                row,
                lastReceivedDate.Date,
                "Last received on",
                true,
                IsConfirmed: false));
    }

    // A blanket release frequently carries a promise or desired date without ever
    // having been received. Ignoring those dates left open blanket lines with a
    // balance and a real date showing no delivery date at all.
    var blanketDelivery = SelectBestIncomingDate(row);
    if (blanketDelivery.Date.HasValue)
    {
        qualifyingLines.Add(blanketDelivery);
    }

    continue;
}
```

Both dates are kept: the promise/desired date drives the delivery captions and the detail line,
while `Last received on` stays in the printed date table as history. `SelectBestIncomingDate` now
returns an undated presentation instead of `null`, so the guard is on `Date.HasValue`.

## Verification

```shell
$(Get-Content Database/InforVisualScripts/Queries/19_GetMaterialAvailabilityIncomingSupply.sql -Raw |
  ForEach-Object { $_ -replace "DECLARE @PartId\s+nvarchar\(30\) = NULL;", "DECLARE @PartId nvarchar(30) = 'MMC0000364';" }) |
  Set-Content $env:TEMP\q19.sql
sqlcmd -S VISUAL -d MTMFG -U JKOLL -P *** -C -i $env:TEMP\q19.sql -W -s "|"
```

- Incoming rows per reported part: `MMC0000364` 6, `MMC0000366` 5, `MMC0000659` 4,
  `MMC0000850` 2 (previously 0).
- `MMC0000850` returns `PO-072153` (promise 11/02/2026) and `PO-072578` (promise 11/04/2026).
- Location volumes after the fix: `RECV` 10 rows (was 0), `T-00` 11, `WC` 53, `NCM` 27 — no
  runaway results.
- Query 18 still returns stock: `MMC0000364` at `V-F0-04`, `V-F0-05`, `WC`.
- Tests: new `GetBoardByPartAsync_ShouldUseBlanketPromiseDateWhenNoReceiptExists`,
  `LoadAndPrepareQuery_ShouldNotFilterIncomingSupplyByWarehouse`, and
  `LoadAndPrepareQuery_ShouldTrimWarehouseWhenReadingStock`; the existing blanket last-received test
  still passes. Material-availability group 40 passed; full suite 841 passed.

## See Also

- [08 — Reader guide](08-Inbound-Without-Warehouse-User-Guide.md)
- [05 — Next delivery date accuracy](05-Next-Delivery-Date-Accuracy-Implementation.md)
