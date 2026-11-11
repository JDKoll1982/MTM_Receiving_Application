# Infor VISUAL Test Purchase Orders (read-only survey)

Last Updated: 2026-11-11

Real purchase orders in `MTMFG` selected to exercise the Visual Macro Tester grid, in particular the
requirement that **a grid cell can be addressed by its line number** and that a PO with several lines can
be inspected without typing the three reference values first.

Nothing was written. Every statement was a `SELECT`, the session ran as login `JKOLL` against `MTMFG`,
and `@@TRANCOUNT` was `0` afterwards. The connection used the same read-only intent as the application
(`ApplicationIntent=ReadOnly`).

## Why these were chosen

The dropdown and grid tests need three different shapes of PO, because one PO cannot cover them:

| Need | Shape required |
| ---- | -------------- |
| Prove line addressing uses the `Ln#` value, not the row index | a PO whose line numbers have a **gap** |
| Prove multi-line reading and column alignment | a PO with roughly 8 to 15 lines, several still open |
| Prove grid paging and scrolling | a PO with enough rows to overflow one screen |
| Prove the degenerate case | a PO with exactly one line |

## Candidates

| Use | PO | Lines | Vendor part populated | Lines still short | Status | Receivers |
| --- | -- | ----- | --------------------- | ----------------- | ------ | --------- |
| **Verified live** against the running window | `PO-072947` | 2 | 2 | 1 | R (open) | 1 |
| Line-number gap, with parts | `PO-072845` | 21 | 21 | 2 | R (open) | 5 |
| Multi-line, mixed progress | `PO-072764` | 11 | 11 | 9 | R (open) | 10 |
| Multi-line, mostly untouched | `PO-072965` | 10 | 10 | 9 | R (open) | 4 |
| Many rows on one screen | `PO-069865` | 15 | 14 | 1 | R (open) | 66 |
| Gap with NULL part numbers | `PO-072437` | 4 | 0 | 4 | R (open) | see note |
| Single-line baseline | `PO-073112` | 1 | 1 | not checked | R (open) | not checked |

Status `R` means released and still receivable. `C` means closed.

## `PO-072947`: the two-line case, confirmed against the live window

Loaded in `VMRCVENT` on 2026-11-11 as receiver `159462`. This is the PO that proved an editable grid cell
can be read without keystrokes, so it is the best starting point for any grid test.

| Ln | Part ID | Vendor part | Qty ordered | Total qty recv | Qty received |
| -- | ------- | ----------- | ----------- | -------------- | ------------ |
| 1 | `NULL` | `ZP-JBH85-2N-5` | 2 | 2 | 0 |
| 2 | `NULL` | `ZP-MC-CC-M10A-22` | 1 | 0 | 1 |

`PART_ID` is `NULL` on both lines, so the on-screen **Part ID** column is blank while **Vendor Part ID** is
populated. Do not assume `PART_ID` is present.

## `PO-072845`: the line-number gap case

This is the important one. Line numbers run 1 to 13, then **jump to 16**, then continue to 23. Lines 14
and 15 do not exist. A test that addresses the grid by row index will therefore read the wrong line from
row 14 onwards; a test that addresses it by `Ln#` value will not.

| Ln | Vendor part | Qty ordered | Qty received | Remaining |
| -- | ----------- | ----------- | ------------ | --------- |
| 1 | `06442974` | 2 | 2 | 0 |
| 2 | `09297516` | 2 | 2 | 0 |
| 3 | `88121892` | 3 | 3 | 0 |
| 4 | `81995565` | 3 | 3 | 0 |
| 5 | `42183699` | 1 | 1 | 0 |
| 6 | `68135656` | 3 | 3 | 0 |
| 7 | `66939935` | 3 | 3 | 0 |
| 8 | `05051644` | 3 | 3 | 0 |
| 9 | `61085429` | 1 | 1 | 0 |
| 10 | `89053326` | 3 | 0 | 3 |
| 11 | `29243433` | 3 | 3 | 0 |
| 12 | `11495058` | 3 | 3 | 0 |
| 13 | `02511731` | 3 | 3 | 0 |
| -- | *lines 14 and 15 do not exist* | | | |
| 16 | `17118951` | 3 | 3 | 0 |
| 17 | `76418045` | 6 | 6 | 0 |
| 18 | `18593137` | 3 | 3 | 0 |
| 19 | `76636109` | 2 | 2 | 0 |
| 20 | `79814323` | 3 | 3 | 0 |
| 21 | `98448616` | 3 | 3 | 0 |
| 22 | `01831841` | 3 | 0 | 3 |
| 23 | `10201499` | 2 | 2 | 0 |

`PART_ID` is `NULL` on every one of these lines while `VENDOR_PART_ID` is populated. That makes this PO a
useful negative case as well: anything that assumes `PART_ID` is always present will fail here.

## `PO-072764`: multi-line with partial receipts

Eleven lines, nine of them still short. Good for checking that ordered, received and remaining quantities
are read from the correct column.

| Ln | Part | Vendor part | Qty ordered | Qty received | Remaining |
| -- | ---- | ----------- | ----------- | ------------ | --------- |
| 1 | `A22-77724-210` | `A22-77724-210` | 1 | 1 | 0 |
| 2 | `A22-77724-170` | `A22-77724-170` | 31 | 31 | 0 |
| 3 | `A22-77724-150` | `A22-77724-150` | 156 | 136 | 20 |
| 4 | `A22-77724-140` | `A22-77724-140` | 156 | 62 | 94 |
| 5 | `A22-77724-065` | `A22-77724-065` | 5 | 0 | 5 |
| 6 | `A22-77724-090` | `A22-77724-090` | 15 | 0 | 15 |
| 7 | `A22-77724-085` | `A22-77724-085` | 15 | 3 | 12 |
| 8 | `A22-77724-050` | `A22-77724-050` | 2 | 1 | 1 |
| 9 | `22-77723-040` | `22-77723-040` | 50 | 48 | 2 |
| 10 | `22-77723-035` | `22-77723-035` | 12 | 8 | 4 |
| 11 | `22-74371-029` | `22-74371-029` | 8 | 6 | 2 |

Recent receivers for this PO include **159430** and **159418**, both dated 2026-11-11. Either can be used
to open the receipt screen.

## `PO-072965`: multi-line, mostly untouched

| Ln | Part | Vendor part | Qty ordered | Qty received | Remaining |
| -- | ---- | ----------- | ----------- | ------------ | --------- |
| 1 | `A22-74373-041` | `A22-74373-041` | 17 | 0 | 17 |
| 2 | `A22-77724-210` | `A22-77724-210` | 20 | 20 | 0 |
| 3 | `A22-77724-140` | `A22-77724-140` | 103 | 0 | 103 |
| 4 | `A22-77724-125` | `A22-77724-125` | 157 | 19 | 138 |
| 5 | `A22-77724-110` | `A22-77724-110` | 312 | 50 | 262 |
| 6 | `A22-77724-085` | `A22-77724-085` | 9 | 5 | 4 |
| 7 | `A22-77724-080` | `A22-77724-080` | 30 | 0 | 30 |
| 8 | `A22-77724-075` | `A22-77724-075` | 20 | 0 | 20 |
| 9 | `A22-77724-065` | `A22-77724-065` | 5 | 0 | 5 |
| 10 | `A22-77724-050` | `A22-77724-050` | 4 | 0 | 4 |

## `PO-069865`: enough rows to overflow the grid

Fifteen lines and 66 receivers, so it is also the best PO for checking the receiver-number lookup path.
Most lines are already fully received; only line 2 is still short.

| Ln | Part | Vendor part | Qty ordered | Qty received | Remaining |
| -- | ---- | ----------- | ----------- | ------------ | --------- |
| 1 | `MMC0000650` | `MMC0000650` | 215340 | 217551 | -2211 |
| 2 | `MMC0000651` | `MMC0000651` | 174416 | 169770 | 4646 |
| 3 | `MMC0000659` | `(blank)` | 344754 | 344754 | 0 |
| 4 | `MMC0000364` | `MMC0000364` | 14460 | 14460 | 0 |
| 5 | `MMC0000364` | `MMC0000364` | 31914 | 31914 | 0 |
| 6 | `MMC0000364` | `MMC0000364` | 28439 | 28439 | 0 |
| 7 | `MMC0000364` | `MMC0000364` | 18518 | 18518 | 0 |
| 8 | `MMC0000364` | `MMC0000364` | 13604 | 13604 | 0 |
| 9 | `MMC0000364` | `MMC0000364` | 28008 | 28008 | 0 |
| 10 | `MMC0000364` | `MMC0000364` | 29198 | 29198 | 0 |
| 11 | `MMC0000364` | `MMC0000364` | 27362 | 27362 | 0 |
| 12 | `MMC0000651` | `MMC0000651` | 33824 | 33824 | 0 |
| 13 | `MMC0000651` | `MMC0000651` | 21222 | 21222 | 0 |
| 14 | `MMC0000651` | `MMC0000651` | 35414 | 35414 | 0 |
| 15 | `MMC0000364` | `MMC0000364` | 20650 | 20650 | 0 |

Line 3 deliberately has a blank vendor part, so it tests the empty-value path. Line 1 is over-received
(217,551 against 215,340), which produces a negative remaining quantity of -2,211 and is worth keeping as
an edge case.

## Things that look like good test data but are not

| Trap | Why |
| ---- | --- |
| Blanket orders such as `PO-067017B` (365 lines) or `PO-060584B` (302 lines) | Hundreds of lines and **no vendor part numbers at all**, so they exercise scrolling but nothing else |
| Closed POs, status `C` | They cannot be received again, so they test the read path only |
| POs with zero receivers, such as `PO-073057` and `PO-0769636B` | Nothing has been received against them, so no receipt screen state exists to open |
| `PO-072437` | It has the gap (lines 1, 3, 4, 5) but every part and vendor part is `NULL`, so use `PO-072845` for the gap test instead |

## How this was produced

Run with `sqlcmd` as login `JKOLL` against server `VISUAL`, database `MTMFG`. The password is supplied
through the `SQLCMDPASSWORD` environment variable and is deliberately not recorded here. Selecting the
candidates:

```sql
SELECT TOP 25
    l.PURC_ORDER_ID AS PoNumber,
    COUNT(*)        AS Lines,
    SUM(CASE WHEN NULLIF(LTRIM(RTRIM(l.VENDOR_PART_ID)),'') IS NOT NULL THEN 1 ELSE 0 END) AS HasVendPart,
    SUM(CASE WHEN l.ORDER_QTY > l.TOTAL_RECEIVED_QTY THEN 1 ELSE 0 END) AS OpenLines,
    MAX(CAST(po.STATUS AS varchar(4))) AS Status,
    (SELECT COUNT(*) FROM dbo.RECEIVER r WHERE r.PURC_ORDER_ID = l.PURC_ORDER_ID) AS Receivers
FROM dbo.PURC_ORDER_LINE l
JOIN dbo.PURCHASE_ORDER po ON po.ID = l.PURC_ORDER_ID
WHERE CAST(po.STATUS AS varchar(4)) = 'R'
GROUP BY l.PURC_ORDER_ID
HAVING COUNT(*) BETWEEN 3 AND 40
   AND SUM(CASE WHEN l.ORDER_QTY > l.TOTAL_RECEIVED_QTY THEN 1 ELSE 0 END) >= 1
   AND SUM(CASE WHEN NULLIF(LTRIM(RTRIM(l.VENDOR_PART_ID)),'') IS NOT NULL THEN 1 ELSE 0 END) >= 2
ORDER BY COUNT(*) DESC;
```

Finding the gap cases compares the row count with the line-number span:

```sql
WITH c AS (
  SELECT l.PURC_ORDER_ID AS Po, COUNT(*) AS n, MIN(l.LINE_NO) AS mn, MAX(l.LINE_NO) AS mx
  FROM dbo.PURC_ORDER_LINE l
  JOIN dbo.PURCHASE_ORDER po ON po.ID = l.PURC_ORDER_ID
  WHERE CAST(po.STATUS AS varchar(4)) = 'R'
  GROUP BY l.PURC_ORDER_ID
)
SELECT Po, n, mn, mx, (mx - mn + 1) AS Span
FROM c
WHERE n BETWEEN 3 AND 40 AND (mx - mn + 1) <> n
ORDER BY n;
```

A span larger than the row count means at least one line number is missing.

## See Also

- [InforVisual-Macro-Variable-Reference.md](InforVisual-Macro-Variable-Reference.md) - what each macro variable binds to
- [InforVisual-Macro-Dropdown-Annotated.md](InforVisual-Macro-Dropdown-Annotated.md) - the dropdown entries annotated
- [InforVisual-Macro-Automation.md](InforVisual-Macro-Automation.md) - how the app drives the receipt window
