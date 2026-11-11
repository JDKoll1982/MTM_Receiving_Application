# Infor VISUAL Macro Variable Reference (VMRCVENT)

Last Updated: 2026-11-11

Proves what every entry in the Purchase Receipt Entry (`VMRCVENT.EXE`) macro **Available Fields** dropdown
actually is: which table and column backs it, which variables only exist in memory, and what a receipt
genuinely requires. All database access was read-only. Nothing was written to `MTMFG`.

Reference data used throughout: PO `PO-072040`, part `MMC0000667`, receiver `159401`, user `JKOLL`.

## How the evidence was gathered

Four sources, each doing a different job.

| # | Source | What it can prove | What it cannot |
| - | ------ | ----------------- | -------------- |
| 1 | Live process memory of `VMRCVENT.EXE` | The real symbol list, and the macro engine's own SQL | Nothing about meaning |
| 2 | Live `MTMFG` database (read-only) | Meaning, backing column, and what is *not* stored | Nothing about the form |
| 3 | VISUAL API Toolkit assemblies + PDFs | Documented required fields and the receipt hierarchy | The VBScript shape of collections |
| 4 | `VMRCVENT.INI` | Line-grid column catalogue | Header fields |

### Source 1 — live process memory (conclusive for names and engine behaviour)

`VMRCVENT.EXE` is a **native x86 Gupta/Team Developer** binary whose outline strings are compressed, so
static scanning cannot recover them. Proof: a byte scan of the 29 MB executable found **0 of 18** strings
that are plainly visible in the running window — `Bill of Lading ID`, `Vendor Packlist Date`, `Order ID`,
`Site ID`, `Buyer`, `Ship via`, `Carrier ID`, `General`, `Released` and the rest are absent in both ASCII and
UTF-16. A zlib probe over 1,633 candidate streams inflated nothing larger than 2 KB.

The names do exist at runtime. Reading the committed readable memory of the live process (~233 MB) and
indexing UTF-16 strings produced 540,618 entries, including every macro variable:

| Symbol | UTF-16 hits in process memory |
| ------ | ----------------------------- |
| `ORDER_ID` | 1276 |
| `PO_USER_1` | 14 |
| `RECVR_LONG_DESC` | 10 |
| `udfStrCombo1` | 8 |
| `udfNumber1` | 7 |
| `MACRO_MESSAGE` | 6 |
| `udfString10` | 4 |
| `udfURL1` | 3 |

Two contiguous blocks are the real prize.

**Block A — the macro engine (0x033A827C).** Its own SQL was recovered verbatim:

```sql
select STORE_MACROS_IN_DB from APPLICATION_GLOBAL into :sSTORE_MACROS_IN_DB
select MACRO from MACRO where USER_ID = :sUserID and PROGRAM_ID = :m_sBaseName
       and NAME = :sMacroName into :lsScript
select 1, NAME from MACRO where program_id = :m_sBaseName and user_id = :sUserID
union
select 2, NAME from MACRO M, USER_PROFILE UP, MACRO_PROFILE MP
 where UP.user_id = :sUserID and UP.profile_id = MP.PROFILE_ID
   and MP.program_id = :m_sBaseName and M.program_id = MP.program_id
   and M.name = MP.macro_name
into :nTemp, :sName
order by 2,1
```

That last statement is the query that fills the **Macros** menu: your own macros first, then
profile-shared ones, ordered by name. Its neighbours `MACRO_SUCCESS`, `MACRO_MESSAGE`, `.vms`, `_*.vms`,
`DeclareVariable` and `GetVariable` are the rest of the engine contract.

**Block B — the screen's field metadata (0x061AE0A4).** A declaration-order table of tab captions, labels
and variable names: `PURC_ORDER_BINARY`, `RECEIVER_BINARY`, `Delivery Schedule`, `SizeAndLocation`, `Part`,
`part_udf_labels`, `cColumnLongDescription`, and `User Defined 1` through `User Defined 10`.

### Source 3 — toolkit assemblies and PDFs

The nine assemblies under `\\visual\visual908$\VMFG\VMFG Ref\` are **.NET Framework 4.x, x86, all at
assembly version 8.0.0.0** (`LsaCore`, `LsaShared`, `VmfgFinancials`, `VmfgInventory`, `VmfgPurchasing`,
`VmfgSales`, `VmfgShared`, `VmfgShopFloor`, `VmfgTrace`). They were read with `System.Reflection.Metadata` in
metadata-only mode — never `Assembly.Load`, since x86 .NET Framework assemblies cannot load into this x64
.NET 10 process.

They are **schema-driven**: `Lsa.Vmfg.Purchasing.ReceiptDataTransaction` exposes only `DataObjectType` and
`ServicedComponentType` and holds rows in a generic data set, so the assemblies cannot enumerate the screen's
variable list. They do expose the receipt hierarchy through `Receipt.NewReceiptLineRow()`,
`Receipt.NewReceiptDelSchedLineRow()` and `Receipt.NewReceiptTraceRow()`.

`VISUALTOOLKITVMFGPURCHASINGREFERENCE_45424.pdf` (toolkit 8.1.100.0) documents the `RECEIPT` header table's
own **Required** annotations, which is the authority used in [Required variables](#required-variables).

## Where macros are stored

From Block A, confirmed in `APPLICATION_GLOBAL`:

```sql
select STORE_MACROS_IN_DB from APPLICATION_GLOBAL
```

On this install `STORE_MACROS_IN_DB = 'N'`, so macros are **files** —
`<PROGRAM_ID>_<NAME>.vms` beside the module executable — and the `MACRO` table stays empty (0 rows,
verified). Had it been `'Y'`, macro source would live in `MACRO.MACRO` (an `image` column) instead, and
`MACRO_PROFILE` would share macros to profiles.

## The dropdown is four different kinds of variable

The most useful finding: only about half of the 87 names are plain database columns.

| Kind | Count | Persisted? | Notes |
| ---- | ----- | ---------- | ----- |
| Header variables backed by a real column | 35 | Yes | `PURCHASE_ORDER` or `RECEIVER`, including `PO_USER_1..10` |
| Line-scoped variables | 2 | Yes | Resolved from the current grid row |
| Engine/runtime variables | 5 | No | `DATABASE`, `USER`, `LINES`, `MACRO_MESSAGE`, `MACRO_SUCCESS` |
| User-defined slots | 45 | **No** | `udf*` — form-level scratch |

Screenshot note: `docs/development_images/` contains **7** `MacroDropdown*.png` files (1–7), not 8. They are
successive scroll positions of one alphabetical list; together they enumerate exactly 87 names, matching the
runtime symbol table.

## Macro variable reference

`RW` is **stored** (backed by a saved column), **derived** (system-supplied, effectively read-only),
**scratch** (no persistence), or **output** (written by a macro to report back).

`PO` = `PURCHASE_ORDER`, `POL` = `PURC_ORDER_LINE`, `RCV` = `RECEIVER`, `RL` = `RECEIVER_LINE`.

| Variable | What it really is | Backing column | Type | RW |
| -------- | ----------------- | -------------- | ---- | -- |
| `ORDER_ID` | The purchase order being received | `RCV.PURC_ORDER_ID` = `PO.ID` | nvarchar(30) | stored |
| `RECEIVER_ID` | The receipt document number | `RCV.ID` | nvarchar(30) | stored |
| `SITE_ID` | Site the receipt is booked in | `RCV.SITE_ID` | nvarchar(30) | stored |
| `VENDOR_ID` | Vendor on the purchase order | `PO.VENDOR_ID` | nvarchar(30) | stored |
| `RECEIVED_DATE` | Receipt transaction date ("Act Recv Date") | `RCV.RECEIVED_DATE` | datetime | stored |
| `STATUS` | PO release status | `PO.STATUS` | nchar(2) | stored |
| `POSTING_CANDIDATE` | Whether the PO may post to the GL | `PO.POSTING_CANDIDATE` | nchar(2) | stored |
| `BUYER` | Buyer code copied onto the PO | `PO.BUYER` | nvarchar(40) | stored |
| `FREE_ON_BOARD` | The box labelled **FOB** on screen | `PO.FREE_ON_BOARD` | nvarchar(50) | stored |
| `SHIP_VIA` | Ship-via text copied onto the PO | `PO.SHIP_VIA` | nvarchar(80) | stored |
| `PROMISE_DATE` | Promised delivery date | `PO.PROMISE_DATE` / `POL.PROMISE_DATE` | datetime | stored |
| `PROMISE_SHIP_DATE` | Promised ship date | `PO.PROMISE_SHIP_DATE` | datetime | stored |
| `DESIRED_RECV_DATE` | The "Receive on" date | `POL.DESIRED_RECV_DATE` (editable per line) | datetime | stored |
| `BOL_ID` | Bill of lading on the **receipt** | `RCV.BOL_ID` | nvarchar | stored |
| `CARRIER_ID` | Carrier on the **receipt** | `RCV.CARRIER_ID` | nvarchar(40) | stored |
| `VEND_PACK_ID` | Vendor packlist on the **receipt** | `RCV.VEND_PACK_ID` | nvarchar(80) | stored |
| `VEND_PACK_DATE` | Vendor packlist date on the **receipt** | `RCV.VEND_PACK_DATE` | datetime | stored |
| `VEND_FREIGHT_ID` | Vendor freight bill on the **receipt** | `RCV.VEND_FREIGHT_ID` | nvarchar(80) | stored |
| `DISPATCH_ID` | Outside-service dispatch this receipt consumes | `SERVICE_RCVR_DISP.DISPATCH_ID` | nvarchar | stored |
| `CONTACT_FIRST_NAME` | Vendor contact block | `PO.CONTACT_FIRST_NAME` | nvarchar(60) | stored |
| `CONTACT_LAST_NAME` | Vendor contact block | `PO.CONTACT_LAST_NAME` | nvarchar(60) | stored |
| `CONTACT_INITIAL` | Vendor contact block | `PO.CONTACT_INITIAL` | nvarchar(4) | stored |
| `CONTACT_HONORIFIC` | Vendor contact block | `PO.CONTACT_HONORIFIC` | nvarchar(30) | stored |
| `CONTACT_PHONE` | Vendor contact block | `PO.CONTACT_PHONE` | nvarchar(40) | stored |
| `CONTACT_FAX` | Vendor contact block | `PO.CONTACT_FAX` | nvarchar(40) | stored |
| `PO_USER_1` | PO-level user-defined text 1 | `PO.USER_1` | nvarchar(160) | stored |
| `PO_USER_2` | PO-level user-defined text 2 | `PO.USER_2` | nvarchar(160) | stored |
| `PO_USER_3` | PO-level user-defined text 3 | `PO.USER_3` | nvarchar(160) | stored |
| `PO_USER_4` | PO-level user-defined text 4 | `PO.USER_4` | nvarchar(160) | stored |
| `PO_USER_5` | PO-level user-defined text 5 | `PO.USER_5` | nvarchar(160) | stored |
| `PO_USER_6` | PO-level user-defined text 6 | `PO.USER_6` | nvarchar(160) | stored |
| `PO_USER_7` | PO-level user-defined text 7 | `PO.USER_7` | nvarchar(160) | stored |
| `PO_USER_8` | PO-level user-defined text 8 | `PO.USER_8` | nvarchar(160) | stored |
| `PO_USER_9` | PO-level user-defined text 9 | `PO.USER_9` | nvarchar(160) | stored |
| `PO_USER_10` | PO-level user-defined text 10 | `PO.USER_10` | nvarchar(160) | stored |
| `RECVR_LONG_DESC` | Long description/specs for the receipt **line** | `RL.DESCRIPTION` | nvarchar | stored |
| `LONG_DESCRIPTION` | Long description of the vendor's part | `VENDOR_PART.LONG_DESCRIPTION` | image | stored |
| `DATABASE` | Connected VISUAL database name (`MTMFG`) | none | n/a | derived |
| `USER` | VISUAL login of the operator (`JKOLL`) | none | n/a | derived |
| `LINES` | Count/handle of the receipt line collection | none | n/a | derived |
| `MACRO_SUCCESS` | Flag a macro sets to report success | none | n/a | output |
| `MACRO_MESSAGE` | Free text a macro returns to the caller | none | n/a | output |

### Naming traps

- **`FREE_ON_BOARD` is the FOB field.** There is no `FOB` variable and no `FOB` column. The screen label
  says FOB; the variable and column say `FREE_ON_BOARD`.
- **`PO_USER_n` maps to `USER_n`, not `PO_USER_n`.** The `PO_` prefix exists only in the macro namespace, to
  keep it distinct from the grid's `colPART_USER_n` (→ `PART.USER_n`) and `colPO_LINE_USER_n`
  (→ `POL.USER_n`).
- **`DISPATCH_ID` is not an ASN.** `SERVICE_RCVR_DISP` has exactly `RECEIVER_ID`, `RECEIVER_LINE_NO`,
  `DISPATCH_ID`, `DISPATCH_LINE_NO`, `QTY` — it is the outside-service consumption link, per receipt line.

### Why `BOL_ID`, `CARRIER_ID` and the `VEND_*` fields bind to `RECEIVER`

`PURCHASE_ORDER` carries same-named columns, so the name alone is ambiguous. Three facts settle it:
`RECEIVER` has all five columns; they are exactly the header boxes on this screen; and `RECEIVER` has no
`PROMISE_*`, `BUYER`, `SHIP_VIA` or `DESIRED_RECV_DATE`, which is why those five must come from
`PURCHASE_ORDER`.

## User-defined slots — 45 variables, not persisted here

`udfBoolean1..5`, `udfDate1..5`, `udfNumber1..10`, `udfStrCombo1..10`, `udfString1..10`, `udfURL1..5`.

| Variable | What it really is | Backing column | Type | RW |
| -------- | ----------------- | -------------- | ---- | -- |
| `udfBoolean1..5` | Form-level user-defined boolean slots | none on this screen | n/a | scratch |
| `udfDate1..5` | Form-level user-defined date slots | none on this screen | n/a | scratch |
| `udfNumber1..10` | Form-level user-defined numeric slots | none on this screen | n/a | scratch |
| `udfStrCombo1..10` | Form-level user-defined combo slots | none on this screen | n/a | scratch |
| `udfString1..10` | Form-level user-defined text slots | none on this screen | n/a | scratch |
| `udfURL1..5` | Form-level user-defined URL slots | none on this screen | n/a | scratch |

Every plausible persistence hypothesis was tested and rejected:

| Hypothesis | Test | Result |
| ---------- | ---- | ------ |
| They are columns on the receipt tables | Column search over `PURCHASE_ORDER`, `PURC_ORDER_LINE`, `RECEIVER`, `RECEIVER_LINE` | No `udf*` column exists anywhere in `MTMFG` |
| They persist into `USER_DEF_FIELDS` | `SELECT PROGRAM_ID, COUNT(*) FROM USER_DEF_FIELDS GROUP BY PROGRAM_ID` | Only `VMMFGWIN_OP` (11,888 rows) and `VMMFGWIN_WO` (9). **Zero `VMRCVENT` rows** |
| They persist into `UDF_LAYOUT` | `SELECT * FROM UDF_LAYOUT` | 7 rows, labels only — no value columns |

So on this screen the `udf*` slots are **in-memory form fields**. Their captions come from configured label
sets, not from receipt tables: `APPLICATION_GLOBAL` carries `PART_UDF_LABELS`, `VEND_UDF_LABELS`,
`CUST_UDF_LABELS`, `CONT_UDF_LABELS`, `PROJ_UDF_LABELS`, `SHOP_UDF_LABELS`, `ACTIVITY_UDF_LABELS` and
`DOC_UDF_LABELS`. On this install `PART_UDF_LABELS` is empty and `VEND_UDF_LABELS` is populated, matching
Block B where `User Defined 1..10` captions sit beside `Part` and `part_udf_labels`.

Practical consequence: **a `udf*` slot is safe scratch space.** Writing one cannot alter a receipt.

## Required variables

The toolkit PDF is explicit about the `RECEIPT` header table, and it contradicts the intuitive assumption
that everything on the form matters:

| Field | PDF annotation | Macro variable |
| ----- | -------------- | -------------- |
| `PURC_ORDER_ID` | "Purchase Order ID being received. **Required**." | `ORDER_ID` |
| `SITE_ID` | "The Site ID for the order. **Required**." | `SITE_ID` |
| `RECEIVER_ID` | "If not provided, receiver will be automatically numbered." | `RECEIVER_ID` |
| `TRANSACTION_DATE` | "Defaults to current date." | `RECEIVED_DATE` |
| `USER_ID` | "Defaults to SYSADM." | `USER` |
| `FREE_ON_BOARD` | "Optional, not validated." | `FREE_ON_BOARD` |
| `SHIP_VIA` | "Optional, not validated." | `SHIP_VIA` |

At line level, `RECEIPT_LINE` requires `PURC_ORDER_LINE_NO`. `DEL_SCHED_LINE` and `TRACE` are only populated
when the order has delivery schedules or traceable parts.

The documented hierarchy:

```text
RECEIPT (header, PK PURC_ORDER_ID)
  +-- RECEIPT_LINE   (PK PURC_ORDER_ID, PURC_ORDER_LINE_NO)
        +-- DEL_SCHED_LINE (PK PURC_ORDER_ID, PURC_ORDER_LINE_NO, DEL_SCHED_LINE_NO)
        +-- TRACE          (PK PURC_ORDER_ID, PURC_ORDER_LINE_NO, TRACE_ID)
```

The macro editor itself mandates nothing — a script may read any field and write any stored field. There is
also no runtime gate on state: with no PO loaded the form holds **61** automation elements and no values;
with `PO-072040` loaded it holds **101** and every value box is populated. `ORDER_ID`, `SITE_ID` and
`RECEIVER_ID` are the identity triple; nothing else has meaning until they exist.

### Safety classes for automation

| Class | Variables | Effect of writing |
| ----- | --------- | ----------------- |
| Read-only in practice | `ORDER_ID`, `RECEIVER_ID`, `SITE_ID`, `VENDOR_ID`, `STATUS`, `POSTING_CANDIDATE`, `RECEIVED_DATE`, `USER`, `DATABASE`, `LINES`, `CONTACT_*` | Changes identity or the vendor snapshot |
| No database consequence | `BOL_ID`, `VEND_PACK_ID`, `VEND_FREIGHT_ID`, `VEND_PACK_DATE`, `CARRIER_ID`, all 45 `udf*` | Free text on the receipt only — safest surface |
| Consequential | `BUYER`, `FREE_ON_BOARD`, `SHIP_VIA`, `PROMISE_DATE`, `PROMISE_SHIP_DATE`, `DESIRED_RECV_DATE`, `PO_USER_1..10`, `LONG_DESCRIPTION`, `RECVR_LONG_DESC`, `DISPATCH_ID` | Updates `PURCHASE_ORDER` or the PO line |

## `LINES` and the line collection

`LINES` is a count, not a safely indexable array. Executing `LINES(1)` outside VISUAL raises **Type
mismatch** (verified by running the generated probe under `cscript`). Inside the module the receipt lines are
the grid rows, and the grid is what automation must drive.

`LINES` returns the number of `RECEIVER_LINE` rows for the receiver — receiver `159401` has exactly 1.

### Grid columns and their real columns

From `VMRCVENT.INI` plus the schema. Editable cells are what a table automation must write.

| Grid column | Caption | Backing column | Editable |
| ----------- | ------- | -------------- | -------- |
| `colLINE_NO` | Ln# | `POL.LINE_NO` / `RL.PURC_ORDER_LINE_NO` | no |
| `colPART_ID` | Part ID | `POL.PART_ID` | no |
| `colVENDOR_PART_ID` | Vendor Part ID | `POL.VENDOR_PART_ID` | no |
| `colUSER_ORDER_QTY` | Quantity Ordered | `POL.USER_ORDER_QTY` | no |
| `colORDER_QTY` | Stock Order Qty | `POL.ORDER_QTY` | no |
| `colTOTAL_USR_RECV_QTY` | Total Qty Recv | `POL.TOTAL_USR_RECD_QTY` | no |
| `colUSER_RECEIVED_QTY` | Quantity Received | `RL.USER_RECEIVED_QTY` | **yes** |
| `colRECEIVED_QTY` | Stock Qty Received | `RL.RECEIVED_QTY` | no |
| `colRECEIVE_TO_INSPECTION` | Recv to Inspect | drives `RL.INSPECT_QTY` | **yes** |
| `colLOCATION_ID` | Location ID | `RL.LOCATION_ID` | **yes** |
| `colWAREHOUSE_ID` | Warehouse ID | `RL.WAREHOUSE_ID` | **yes** |
| `colSHIPTO_ID` | Shipto ID | `POL.SHIPTO_ID` | no |
| `colSHIPFROM_ID` | Ship from ID | `POL.SHIPFROM_ID` | no |
| `colDESIRED_RECV_DATE` | Receive On | `POL.DESIRED_RECV_DATE` | **yes** |
| `colPROMISE_DATE` | Promise Del Date | `POL.PROMISE_DATE` | **yes** |
| `colPROMISE_SHIP_DATE` | Promise Ship Date | `POL.PROMISE_SHIP_DATE` | **yes** |
| `colRECEIVED_DATE` | Last Recv Date | `POL.LAST_RECEIVED_DATE` | no |
| `colPART_USER_1..10` | Part User Defined n | `PART.USER_1..10` | no |
| `colPO_LINE_USER_1..10` | P.O. User Defined n | `POL.USER_1..10` | no |
| `colRR_LONG_DESCRIPTION` | Recvr Specs (hidden) | `RL.DESCRIPTION` | **yes** |

### Verified totals for `PO-072040`

`POL.TOTAL_USR_RECD_QTY` equals the live sum of `RL.USER_RECEIVED_QTY` on every line, so "Total Qty Recv" is
a **stored rollup** VISUAL maintains at post time, not something the grid recomputes from screen state:

| Line | Part | Qty Ordered | `POL.TOTAL_USR_RECD_QTY` | `SUM(RL.USER_RECEIVED_QTY)` | Receipts |
| ---- | ---- | ----------- | ------------------------ | --------------------------- | -------- |
| 1 | `MMC0000667` | 72,000 | 34,680 | 34,680 | 1 |
| 2 | `MMC0000743` | 33,000 | 36,110 | 36,110 | 1 |
| 3 | `MMC0000761` | 80,000 | 83,920 | 83,920 | 4 |
| 4 | `MMC0000761` | 60,000 | 46,760 | 46,760 | 2 |
| 5 | `MMC0000848` | 55,000 | 55,620 | 55,620 | 5 |

Line 1 is 34,680 against 72,000 ordered — a partial receipt, the case automation must handle. `PO-072040`
also demonstrates multi-receiver accumulation: 13 receivers across its 5 lines, including one negative
receipt (`159169`, −18,570) that nets out another.

## What the evidence could not establish

- **Which of `PO`/`POL` a single ambiguous variable targets.** `PROMISE_DATE` and `DESIRED_RECV_DATE` exist on
  both the order and the order line, and the screen shows both a header box and a grid column. The bindings
  above are the reading most consistent with the column sets, but confirming which one a header-level write
  touches needs a live write test.
- **Whether `colRECEIVE_TO_INSPECTION` writes `INSPECT_QTY` or a separate flag.** `RECEIVER_LINE` has no
  boolean inspection column, so the quantity is the only candidate — but every posted receiver on
  `PO-072040` has `INSPECT_QTY = 0` with the box unticked, so the positive case is unobserved.
- **The per-slot `udf*` caption source.** Labels exist in `APPLICATION_GLOBAL`, `UDF_LAYOUT` and `SITE`, and
  `PART_UDF_LABELS` is empty here, so no single source can be confirmed.
- **The VBScript shape of `LINES`.** The hierarchy is proven and `LINES(1)` fails outside VISUAL, but no
  document states the in-module accessor. This is why table automation in this app reads the grid through
  automation and the database, not through `LINES`.
- **The compressed outline on disk.** Names exist only at runtime, so static decompression would require the
  Gupta TD string-table format, which was not attempted beyond the zlib probe.

## See Also

- [Infor VISUAL Macro Automation](./InforVisual-Macro-Automation.md)
- [Infor Visual database reference files](../../.github/instructions/architecture/infor-visual-database-reference.instructions.md)
- [Running VISUAL macros from outside VISUAL](https://community.infor.com/discussion/3127/running-visual-macros-from-outside-visual)
- [Developing Infor VISUAL macros](https://www.gingerhelp.com/infor-visual-blog/developing-and-using-macros-to-automate-common-tasks)
