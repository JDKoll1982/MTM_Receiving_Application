# Infor VISUAL (MTMFG 9.0.8) Purchase Receipt Entry Macro Variable Reference

Last Updated: 2026-11-10

## Introduction

This document reverse-engineers the macro variable surface of `VMRCVENT.EXE`, the Purchase
Receipt Entry screen in Infor VISUAL (MTMFG, ERP version 9.0.8). It maps every field name in
the screen's macro "Available Fields" dropdown to a concrete Infor VISUAL business object, a
`VMFG\VMFG Ref\` toolkit assembly type, a toolkit reference PDF section, and an MTMFG SQL
Server table/column, and records the data type and whether the variable is stored (writable)
or derived (read-only).

`VMRCVENT.EXE` itself is a native x86 Gupta/Team Developer binary. Its string table is packed,
so the field list cannot be recovered from the binary directly. The field list below is
therefore the one exposed by the native macro editor; each name is anchored to the managed
VISUAL API Toolkit and to the MTMFG schema using the evidence described next.

## How the Evidence Was Gathered

Three independent evidence sources were combined:

1. **Managed metadata dump (primary).** The nine .NET Framework assemblies under
   `\\visual\visual908$\VMFG\VMFG Ref\` were read with `System.Reflection.Metadata` in
   metadata-only mode (never `Assembly.Load`, because they are x86 .NET Framework and will
   not load into an x64 .NET 10 process). A scratch console tool printed every assembly's
   name, version, assembly references (target framework), public types, and per-type
   properties, fields, methods, and enum members. All nine assemblies report assembly
   version `8.0.0.0` and reference `mscorlib 4.0.0.0` (i.e. .NET Framework 4.x).

2. **PDF text extraction (secondary).** The eleven PDFs under
   `\\visual\visual908$\VMFG\API Toolkit Documents\` were parsed for `stream`/`endstream`
   objects, `FlateDecode` streams were inflated with `System.IO.Compression.DeflateStream`,
   and text was recovered from `Tj`/`TJ` operators. All eleven produced usable text. The most
   useful for this document is `VISUALTOOLKITVMFGPURCHASINGREFERENCE_45424.pdf`, which is
   titled for toolkit version `8.1.100.0` and documents the `Receipt`, `Receiver`,
   `PurchaseOrder`, `Vendor`, `VendorContact`, and `GetInspectionReceivers` objects and their
   data-table/column layouts.

3. **MTMFG schema CSVs.** The repo's exported schema at
   `docs/development/InforVisual/DatabaseCSVFiles/` (one CSV per table under `Tables/`, plus
   `ColumnDetails/` for length/precision) supplies every concrete `table.column`, data type,
   nullability, and length/precision cited below.

A key finding of the metadata dump: the toolkit business objects are **schema-driven**. For
example `Lsa.Vmfg.Purchasing.ReceiptDataTransaction` exposes only `DataObjectType` and
`ServicedComponentType` properties and holds its rows in a generic `Lsa.Data.DataSet`
(`notificationDataSet`). The per-field names therefore do not appear as .NET properties;
they come from the screen definition in the native binary and are documented in the PDFs as
**data-table columns** (for example the `RECEIPT` and `RECEIPT_LINE` tables below).

## How VISUAL Produces the Macro Variable List

VISUAL's macro editor runs VBScript against a screen form. For `VMRCVENT`, the native
Gupta/Team Developer form defines a set of named data fields; the editor's "Available Fields"
dropdown lists exactly those names (alphabetically). At macro run time each field is exposed
to VBScript as a same-named read/write variable; assigning a value updates the screen field
and, for stored fields, the value is saved with the receipt.

The managed toolkit does not enumerate this list (it is schema-driven), so the native form is
the authoritative source of the variable names. The table below treats the provided dropdown
list as authoritative and attaches evidence from the toolkit assemblies, the toolkit PDFs,
and the MTMFG schema.

## Macro Variable Reference

Column `RW` distinguishes **stored** (backed by a saved MTMFG column; a macro may set it
before save) from **derived** (computed or system-supplied; effectively read-only) and
**output** (set by the macro to communicate back to VISUAL).

| Variable | Meaning | Best evidence | MTMFG table.column | Data type | RW |
| --- | --- | --- | --- | --- | --- |
| BOL_ID | Bill of lading number on the receiver header | MTMFG schema; `VmfgPurchasing.dll` type `Lsa.Vmfg.Purchasing.Receiver` | `RECEIVER.BOL_ID` | nvarchar(20), nullable | stored |
| BUYER | Buyer code/user assigned to the purchase order | MTMFG schema | `PURCHASE_ORDER.BUYER` | nvarchar(20), nullable | stored |
| CARRIER_ID | Carrier code for the shipment | MTMFG schema | `RECEIVER.CARRIER_ID` (also `PURCHASE_ORDER.CARRIER_ID`) | nvarchar(15), nullable | stored |
| CONTACT_FAX | PO contact fax number | MTMFG schema | `PURCHASE_ORDER.CONTACT_FAX` (mirrors `VENDOR.CONTACT_FAX`) | nvarchar(20), nullable | stored |
| CONTACT_FIRST_NAME | PO contact first name | MTMFG schema | `PURCHASE_ORDER.CONTACT_FIRST_NAME` (mirrors `VENDOR.CONTACT_FIRST_NAME`) | nvarchar(30), nullable | stored |
| CONTACT_HONORIFIC | PO contact honorific/title | MTMFG schema | `PURCHASE_ORDER.CONTACT_HONORIFIC` (mirrors `VENDOR.CONTACT_HONORIFIC`) | nvarchar(15), nullable | stored |
| CONTACT_INITIAL | PO contact middle initial | MTMFG schema | `PURCHASE_ORDER.CONTACT_INITIAL` (mirrors `VENDOR.CONTACT_INITIAL`) | nvarchar(2), nullable | stored |
| CONTACT_LAST_NAME | PO contact last name | MTMFG schema | `PURCHASE_ORDER.CONTACT_LAST_NAME` (mirrors `VENDOR.CONTACT_LAST_NAME`) | nvarchar(30), nullable | stored |
| CONTACT_PHONE | PO contact phone number | MTMFG schema | `PURCHASE_ORDER.CONTACT_PHONE` (mirrors `VENDOR.CONTACT_PHONE`) | nvarchar(20), nullable | stored |
| DATABASE | Active database/schema name for the session | Runtime value; not a table column | none | n/a | derived |
| DESIRED_RECV_DATE | Desired receive date (shown per PO line) | MTMFG schema | `PURC_ORDER_LINE.DESIRED_RECV_DATE` (also `PURCHASE_ORDER.DESIRED_RECV_DATE`) | datetime, nullable | stored |
| DISPATCH_ID | Dispatch/ASN identifier on the receipt | Inferred from name + MTMFG schema; no matching purchase-receipt column | nearest: `ADV_SHIP_NOTICE.id`; `DISPATCH_PALLET.DISPATCH_ID` is service-only | nvarchar(20) (ASN) | unknown |
| FREE_ON_BOARD | Free-on-board (FOB) code for the order | Toolkit PDF: `RECEIPT` data table `FREE_ON_BOARD` ("Optional, not validated") | `PURCHASE_ORDER.FREE_ON_BOARD` | nvarchar(25), nullable | stored |
| LINES | Collection handle to the receipt's line items | Engine collection; see the `LINES` subsection | none | n/a | derived |
| LONG_DESCRIPTION | Long description of the PO/part (rich text) | Inferred from name + MTMFG schema | `PURC_ORDER_BINARY`/`PURC_LINE_BINARY` (image); `VENDOR_PART.LONG_DESCRIPTION` (image) | image | stored |
| MACRO_MESSAGE | Text the macro returns to VISUAL for display | Macro status channel; see subsection | none | n/a | output |
| MACRO_SUCCESS | Success/failure flag the macro sets on exit | Macro status channel; see subsection | none | n/a | output |
| ORDER_ID | Purchase order number being received | Toolkit PDF: `RECEIPT` data table `PURC_ORDER_ID` ("Required") | `PURCHASE_ORDER.ID` | nvarchar(15), not null | stored |
| PO_USER_1 | PO user-defined text field 1 | MTMFG schema | `PURCHASE_ORDER.USER_1` | nvarchar(80), nullable | stored |
| PO_USER_2 | PO user-defined text field 2 | MTMFG schema | `PURCHASE_ORDER.USER_2` | nvarchar(80), nullable | stored |
| PO_USER_3 | PO user-defined text field 3 | MTMFG schema | `PURCHASE_ORDER.USER_3` | nvarchar(80), nullable | stored |
| PO_USER_4 | PO user-defined text field 4 | MTMFG schema | `PURCHASE_ORDER.USER_4` | nvarchar(80), nullable | stored |
| PO_USER_5 | PO user-defined text field 5 | MTMFG schema | `PURCHASE_ORDER.USER_5` | nvarchar(80), nullable | stored |
| PO_USER_6 | PO user-defined text field 6 | MTMFG schema | `PURCHASE_ORDER.USER_6` | nvarchar(80), nullable | stored |
| PO_USER_7 | PO user-defined text field 7 | MTMFG schema | `PURCHASE_ORDER.USER_7` | nvarchar(80), nullable | stored |
| PO_USER_8 | PO user-defined text field 8 | MTMFG schema | `PURCHASE_ORDER.USER_8` | nvarchar(80), nullable | stored |
| PO_USER_9 | PO user-defined text field 9 | MTMFG schema | `PURCHASE_ORDER.USER_9` | nvarchar(80), nullable | stored |
| PO_USER_10 | PO user-defined text field 10 | MTMFG schema | `PURCHASE_ORDER.USER_10` | nvarchar(80), nullable | stored |
| POSTING_CANDIDATE | Flag marking the PO as a GL posting candidate | MTMFG schema | `PURCHASE_ORDER.POSTING_CANDIDATE` | nchar(1), not null | derived |
| PROMISE_DATE | Promised delivery date (shown per PO line) | MTMFG schema | `PURC_ORDER_LINE.PROMISE_DATE` (also `PURCHASE_ORDER.PROMISE_DATE`) | datetime, nullable | stored |
| PROMISE_SHIP_DATE | Promised ship date (shown per PO line) | MTMFG schema | `PURC_ORDER_LINE.PROMISE_SHIP_DATE` (also `PURCHASE_ORDER.PROMISE_SHIP_DATE`) | datetime, nullable | stored |
| RECEIVED_DATE | Date of the receipt transaction | Toolkit PDF: `RECEIPT` data table `TRANSACTION_DATE` ("Defaults to current date") | `RECEIVER.RECEIVED_DATE` | datetime, nullable | stored |
| RECEIVER_ID | Receiver ID being created | Toolkit PDF: `RECEIPT` data table `RECEIVER_ID` ("If not provided, receiver will be automatically numbered") | `RECEIVER.ID` | nvarchar(15), not null | stored |
| RECVR_LONG_DESC | Receiver long description (rich text) | Inferred from name + MTMFG schema | `RECEIVER_BINARY`/`RECVR_LINE_BINARY` (image) | image | stored |
| SHIP_VIA | Shipping method for the order | Toolkit PDF: `RECEIPT` data table `SHIP_VIA` ("Optional, not validated") | `PURCHASE_ORDER.SHIP_VIA` | nvarchar(40), nullable | stored |
| SITE_ID | Site for the order/receipt | Toolkit PDF: `RECEIPT` data table `SITE_ID` ("Required") | `RECEIVER.SITE_ID` (also `PURCHASE_ORDER.SITE_ID`) | nvarchar(15), not null | stored |
| STATUS | Status flag of the purchase order | MTMFG schema | `PURCHASE_ORDER.STATUS` | nchar(1), not null | derived |
| USER | User ID performing the transaction | Toolkit PDF: `RECEIPT` data table `USER_ID` ("Defaults to SYSADM") | `RECEIVER.USER_ID` | nvarchar(20), not null | stored |
| VEND_FREIGHT_ID | Vendor freight code | MTMFG schema | `RECEIVER.VEND_FREIGHT_ID` (also `PURCHASE_ORDER.VEND_FREIGHT_ID`) | nvarchar(40), nullable | stored |
| VEND_PACK_DATE | Vendor pack slip date | MTMFG schema | `RECEIVER.VEND_PACK_DATE` (also `PURCHASE_ORDER.VEND_PACK_DATE`) | datetime, nullable | stored |
| VEND_PACK_ID | Vendor pack slip ID | MTMFG schema | `RECEIVER.VEND_PACK_ID` (also `PURCHASE_ORDER.VEND_PACK_ID`) | nvarchar(40), nullable | stored |
| VENDOR_ID | Vendor for the purchase order | Toolkit PDF: `INSPECTION_RECEIVERS` output `VENDOR_ID` -> `PURCHASE_ORDER.VENDOR_ID` | `PURCHASE_ORDER.VENDOR_ID` | nvarchar(15), not null | stored |
| udfBoolean1 | User-defined boolean field 1 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.BOOL_VAL` | int | stored |
| udfBoolean2 | User-defined boolean field 2 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.BOOL_VAL` | int | stored |
| udfBoolean3 | User-defined boolean field 3 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.BOOL_VAL` | int | stored |
| udfBoolean4 | User-defined boolean field 4 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.BOOL_VAL` | int | stored |
| udfBoolean5 | User-defined boolean field 5 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.BOOL_VAL` | int | stored |
| udfDate1 | User-defined date field 1 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.DATE_VAL` | datetime | stored |
| udfDate2 | User-defined date field 2 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.DATE_VAL` | datetime | stored |
| udfDate3 | User-defined date field 3 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.DATE_VAL` | datetime | stored |
| udfDate4 | User-defined date field 4 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.DATE_VAL` | datetime | stored |
| udfDate5 | User-defined date field 5 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.DATE_VAL` | datetime | stored |
| udfNumber1 | User-defined numeric field 1 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.NUMBER_VAL` | decimal(23,8) | stored |
| udfNumber2 | User-defined numeric field 2 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.NUMBER_VAL` | decimal(23,8) | stored |
| udfNumber3 | User-defined numeric field 3 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.NUMBER_VAL` | decimal(23,8) | stored |
| udfNumber4 | User-defined numeric field 4 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.NUMBER_VAL` | decimal(23,8) | stored |
| udfNumber5 | User-defined numeric field 5 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.NUMBER_VAL` | decimal(23,8) | stored |
| udfNumber6 | User-defined numeric field 6 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.NUMBER_VAL` | decimal(23,8) | stored |
| udfNumber7 | User-defined numeric field 7 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.NUMBER_VAL` | decimal(23,8) | stored |
| udfNumber8 | User-defined numeric field 8 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.NUMBER_VAL` | decimal(23,8) | stored |
| udfNumber9 | User-defined numeric field 9 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.NUMBER_VAL` | decimal(23,8) | stored |
| udfNumber10 | User-defined numeric field 10 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.NUMBER_VAL` | decimal(23,8) | stored |
| udfStrCombo1 | User-defined combo/select field 1 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfStrCombo2 | User-defined combo/select field 2 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfStrCombo3 | User-defined combo/select field 3 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfStrCombo4 | User-defined combo/select field 4 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfStrCombo5 | User-defined combo/select field 5 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfStrCombo6 | User-defined combo/select field 6 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfStrCombo7 | User-defined combo/select field 7 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfStrCombo8 | User-defined combo/select field 8 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfStrCombo9 | User-defined combo/select field 9 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfStrCombo10 | User-defined combo/select field 10 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfString1 | User-defined text field 1 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfString2 | User-defined text field 2 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfString3 | User-defined text field 3 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfString4 | User-defined text field 4 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfString5 | User-defined text field 5 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfString6 | User-defined text field 6 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfString7 | User-defined text field 7 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfString8 | User-defined text field 8 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfString9 | User-defined text field 9 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfString10 | User-defined text field 10 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfURL1 | User-defined URL field 1 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfURL2 | User-defined URL field 2 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfURL3 | User-defined URL field 3 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfURL4 | User-defined URL field 4 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |
| udfURL5 | User-defined URL field 5 | `USER_DEF_FIELDS` schema (inferred slot) | `USER_DEF_FIELDS.STRING_VAL` | nvarchar(254) | stored |

### MACRO_MESSAGE and MACRO_SUCCESS status channel

The macro engine communicates the script's outcome back to VISUAL through two dedicated
variables, neither of which is a database column:

- `MACRO_MESSAGE` is a string the macro writes to return text (for example an error or note)
  that VISUAL displays to the user after the macro finishes.
- `MACRO_SUCCESS` is a boolean the macro writes to signal overall success or failure. VISUAL
  reads it after the script exits and may abort the pending receipt action when it is false.

These two variables are documented here as **inferred** in shape: the assemblies and PDFs do
not describe the VBScript macro engine, so their exact semantics (does `MACRO_SUCCESS` gate
the save, or only the message box?) are not proven from the evidence on the share.

### Required variables and required conditions

The toolkit PDF documents the `Receipt` transaction's `RECEIPT` (header) data table as the
authoritative statement of what a receipt save requires:

- `PURC_ORDER_ID` — "Purchase Order ID being received. Required." (macro variable `ORDER_ID`).
- `SITE_ID` — "The Site ID for the order. Required."
- `RECEIVER_ID` — "If not provided, receiver will be automatically numbered."
- `TRANSACTION_DATE` — "Defaults to current date." (macro variable `RECEIVED_DATE`).
- `USER_ID` — "Defaults to SYSADM." (macro variable `USER`).
- `FREE_ON_BOARD` and `SHIP_VIA` — "Optional, not validated."
- `CLOSE_SHORT` and `RECEIVE_ALL` are transaction flags documented by the PDF but are not in
  the screen's macro variable list.

At the line level, the `RECEIPT_LINE` sub-table requires `PURC_ORDER_LINE_NO`, and the
`DEL_SCHED_LINE` and `TRACE` sub-tables are only populated when the order has delivery
schedules or traceable parts respectively.

The macro editor itself does not mandate a set of assigned variables — a macro may read any
field and write any stored field — but for a receipt to be completed the underlying data
tables must satisfy the required-column rules above.

Per-header vs per-line scoping (inferred from the screen and schema; not stated by the DLLs):

- Header-scoped (one value per receipt): `ORDER_ID`, `VENDOR_ID`, `BUYER`, `CONTACT_*`,
  `FREE_ON_BOARD`, `SHIP_VIA`, `CARRIER_ID`, `BOL_ID`, `SITE_ID`, `VEND_PACK_ID`,
  `VEND_PACK_DATE`, `VEND_FREIGHT_ID`, `RECEIVER_ID`, `RECEIVED_DATE`, `STATUS`,
  `POSTING_CANDIDATE`, `USER`, `DATABASE`, `PO_USER_1`..`PO_USER_10`, `LONG_DESCRIPTION`.
- Line-scoped (one value per receiver line): `DESIRED_RECV_DATE`, `PROMISE_DATE`,
  `PROMISE_SHIP_DATE`, `RECVR_LONG_DESC`, and the `LINES` collection.
- `udf*` fields can be header- or line-scoped: `USER_DEF_FIELDS` carries `LINE_NO` and
  `DEL_LINE_NO` columns, so a UDF row may attach to a header (`LINE_NO` null) or a line.

### LINES and receipt lines

`LINES` is the macro's handle to the receipt's line-item collection. The toolkit PDF
documents the underlying relational hierarchy explicitly: the `Receipt` transaction is
hierarchical, where "each Receiver header has one to many receiver lines, these lines may
have one to many delivery schedule lines, and each receiver line may have one to many trace
entries." In data-table terms this is:

```text
RECEIPT (header, PK PURC_ORDER_ID)
  +-- RECEIPT_LINE   (PK PURC_ORDER_ID, PURC_ORDER_LINE_NO)
  |     +-- DEL_SCHED_LINE (PK PURC_ORDER_ID, PURC_ORDER_LINE_NO, DEL_SCHED_LINE_NO)
  |     +-- TRACE          (PK PURC_ORDER_ID, PURC_ORDER_LINE_NO, TRACE_ID)
```

The DLLs expose this only as `Receipt.NewReceiptLineRow()`,
`Receipt.NewReceiptDelSchedLineRow()`, and `Receipt.NewReceiptTraceRow()` (methods that
insert rows into the `RECEIPT_LINE`, `DEL_SCHED_LINE`, and `TRACE` data tables) — they do
**not** expose a typed per-line .NET collection. Neither the DLLs nor the PDFs document the
VBScript shape of `LINES` (for example `LINES.Count` or `LINES(n).<field>`). The existence
and hierarchy of the line collection is proven; its exact scripting API is inferred and is
listed under "What the evidence could not establish" below.

## What the Evidence Could Not Establish

The following were not provable from the assemblies, the PDFs, or the schema CSVs on the
share, and are marked as such in the table rather than asserted as fact:

- The VBScript macro engine itself is entirely inside the native `VMRCVENT.EXE`; the managed
  toolkit has no macro-related types, so the exact read/write semantics of each variable
  (including whether assigning a header field propagates to the database or only to the
  screen) are not directly documented.
- `MACRO_MESSAGE` and `MACRO_SUCCESS` semantics (whether `MACRO_SUCCESS=false` aborts the
  save) are inferred; no toolkit artifact documents them.
- `DISPATCH_ID` has no matching column on `PURCHASE_ORDER` or `RECEIVER`; the nearest schema
  matches are the advance ship notice (`ADV_SHIP_NOTICE.id`) and service-dispatch tables. Its
  exact meaning on the purchase receipt screen is not documented.
- `LONG_DESCRIPTION` vs `RECVR_LONG_DESC`: the exact backing binary table
  (`PURC_ORDER_BINARY`/`PURC_LINE_BINARY` vs `RECEIVER_BINARY`/`RECVR_LINE_BINARY`) and
  whether they are header- or line-scoped is inferred from names and the schema.
- The `udf*` slot numbers (1..5 / 1..10) map to the `USER_DEF_FIELDS` storage columns, but
  which `UDF_LAYOUT_ID` definition assigns each slot's label is row data not present in the
  schema CSVs.
- The exact single-character code values for `PURCHASE_ORDER.STATUS` and
  `POSTING_CANDIDATE` are not recoverable: MTMFG check-constraint definitions are encrypted
  and absent from the exported schema.
- The toolkit PDFs are titled for version `8.1.100.0` while the on-share DLLs report assembly
  version `8.0.0.0`; minor field drift between those two is possible but was not observed for
  the variables above.

## See Also

- `docs/development/InforVisual/DatabaseCSVFiles/` — MTMFG SQL Server schema exports
  (`Tables/RECEIVER.csv`, `Tables/PURCHASE_ORDER.csv`, `Tables/USER_DEF_FIELDS.csv`,
  `ColumnDetails/PURCHASE_ORDER.csv`, and related files).
- `docs/development/InforVisual-Macro-Automation.md` — existing macro automation notes in
  this repo.
- `\\visual\visual908$\VMFG\VMFG Ref\` — the nine VISUAL API Toolkit assemblies
  (`VmfgPurchasing.dll`, `VmfgShared.dll`, `LsaCore.dll`, `LsaShared.dll`, and others).
- `\\visual\visual908$\VMFG\API Toolkit Documents\VISUALTOOLKITVMFGPURCHASINGREFERENCE_45424.pdf`
  — the `Receipt`, `Receiver`, `PurchaseOrder`, `Vendor`, and `GetInspectionReceivers`
  reference (toolkit version 8.1.100.0).
- Infor VISUAL API Toolkit class reference (all eleven PDFs) under
  `\\visual\visual908$\VMFG\API Toolkit Documents\`.
