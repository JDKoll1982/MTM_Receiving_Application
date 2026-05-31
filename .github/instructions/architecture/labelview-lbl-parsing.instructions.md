---
applyTo: "docs/LabelView/**/*.lbl, docs/LabelView/**/*.md"
description: >
  Step-by-step guide for reading and extracting all meaningful data from a
  TEKLYNX LabelView 2022 .lbl file without launching LabelView. Covers the
  OLE2 container, every named stream, binary object layout, variable/SQL
  definitions, printer binding, and the AIAG MH10.8.2 barcode formula.
  Derived from binary analysis of docs/LabelView/ReverseEngThis.lbl.
---

# TEKLYNX LabelView 2022 — `.lbl` File Parsing Guide

## Overview

A LabelView `.lbl` file is a **Microsoft OLE2 Compound Document** (Structured
Storage), the same binary container used by legacy `.doc` / `.xls` files.
Inside that container LabelView stores a fixed set of named streams, each
holding a different aspect of the label design: drawing objects, variable
definitions, printer settings, canvas properties, and more.

You do **not** need LabelView installed to read the data — you only need an
OLE2 reader (e.g., OpenMcdf in C#, `olefile` in Python, or raw PowerShell byte
parsing).

---

## 1. Identify the Format

Before parsing anything, confirm the first 8 bytes are the OLE2 magic:

```
D0 CF 11 E0 A1 B1 1A E1
```

If those bytes are not present the file is not a valid OLE2 document. Do not
attempt to parse it further.

| Property | Value |
|---|---|
| Sector size | 512 bytes |
| Mini-sector size | 64 bytes |
| Mini-stream cutoff | 4 096 bytes (streams ≤ 4 096 bytes use mini-sectors) |
| Directory entries | 128 bytes each, 4 per sector |
| FAT location | bytes 76–508 of the header (up to 109 FAT sector numbers) |

---

## 2. Stream Inventory

LabelView 2022 labels contain the following named streams. Read them in this
order so that cross-stream references (e.g., variable names referenced in
`Objects`) resolve cleanly.

| Stream Name | Typical Size | Storage | What to Extract |
|---|---|---|---|
| `Printer` | ~2 KB | Mini-stream | Printer name, IP/port, media settings |
| `Form` | ~26 KB | Regular sectors | Canvas dimensions, orientation, margins |
| `Variables` | ~40 KB | Regular sectors | All field/variable definitions + SQL queries + ODBC strings |
| `Objects` | ~135 KB | Regular sectors | All drawing objects, positions, font specs, field bindings |
| `Format` | ~130 bytes | Mini-stream | Print format flags |
| `InputPrintOrder` | ~18 bytes | Mini-stream | Print-order sequence |
| `BackGround` | ~32 bytes | Mini-stream | Background fill settings |
| `GroupsObjects` | ~382 bytes | Mini-stream | Object group membership |
| `Database` | 2 bytes | Mini-stream | DB connection flag (1 = connected) |
| `VarsCategories` | 2 bytes | Mini-stream | Variable category flags |
| `General` | 12 bytes | Mini-stream | Miscellaneous label flags |
| `Contents` | ~733 bytes | Mini-stream | Label content summary (name, description) |
| `\x05SummaryInformation` | varies | Regular sectors | LabelView render metadata (not human-readable) |
| `\x05DocumentSummaryInformation` | ~193 bytes | Mini-stream | Standard OLE2 document properties |
| `RfTagObject` | 0 bytes | — | RFID placeholder; skip if empty |

---

## 3. Reading the `Printer` Stream

The `Printer` stream is a length-prefixed binary record. Look for these ASCII
strings within it using a sliding-window search (the record is not a fixed
schema):

| Field | How to Find It | Example |
|---|---|---|
| Driver / device name | ASCII string after a 2-byte length prefix | `ZDesigner GK420d zebra7` |
| Port / IP address | ASCII string following the driver name | `172.16.3.112` |
| Media type description | ASCII string near end of stream | `User defined` |

**Practical approach:**

```csharp
var printerBytes = ole.GetStream("Printer");
var text = Encoding.ASCII.GetString(printerBytes);
// Scan for printable runs of ≥ 4 chars — each run is a field value.
var fields = Regex.Matches(text, @"[\x20-\x7E]{4,}");
```

From the device name you can infer the printer language:

| Device name prefix | Language |
|---|---|
| `ZDesigner` | ZPL II (Zebra) |
| `TSC` | TSPL |
| `Intermec` | IPL / ZPL |
| `SATO` | SBPL |

---

## 4. Reading the `Variables` Stream

This is the most information-rich stream. It is encoded as a series of
**length-prefixed UTF-16LE string fields** interleaved with binary property
records. Each named variable block follows this pattern:

```
[2-byte BE length N]  [N bytes UTF-16LE class name]
[2-byte BE length M]  [M bytes UTF-16LE variable name]
[binary property records...]
[embedded SQL query as UTF-16LE string, if lookup field]
[ODBC connection string as UTF-16LE string, if database-connected]
```

### 4.1 Variable Classes

| Class Token (UTF-16LE) | Type | What it Contains |
|---|---|---|
| `CVariableField` | User input / counter | Current value (seed), increment, format |
| `CLookupField` | Database lookup | SQL SELECT query + ODBC connection string |
| `CFormulaField` | Computed expression | Formula text that assembles output from other variables |
| `CImportField` | External import | Mapping key for CSV / data-file import |
| `CDateField` | System date/time | Format string (e.g., `ddMONyyyy`) |
| `CCounterField` | Auto-increment counter | Start value, step, reset trigger |

### 4.2 Extracting Variable Names and Values

```csharp
// Extract all UTF-16LE strings ≥ 6 chars from the Variables stream.
// Each printable run is either a class name, a variable name, a SQL query,
// a connection string, or a current/sample value.
var raw = ole.GetStream("Variables");
var positions = new List<(int Offset, string Text)>();
for (int i = 0; i < raw.Length - 1; i += 2)
{
    // read a 2-byte char
}
// Alternatively, use a regex on the decoded string:
var text = Encoding.Unicode.GetString(raw);
var strings = Regex.Matches(text, @"[\u0020-\u007E\u00A0-\u00FF]{4,}");
```

### 4.3 SQL Queries

Each `CLookupField` variable stores a complete SQL `SELECT` statement.
Scan for strings starting with `SELECT` (case-insensitive). In
`ReverseEngThis.lbl`, three queries were found:

```sql
-- Customer address lookup
SELECT *
FROM   [dbo].[CUST_ADDRESS]
WHERE  [CUST_ADDRESS].[CUSTOMER_ID] = ?
  AND  [CUST_ADDRESS].[ADDR_NO]     = ?

-- Shipper / PO reference
SELECT *
FROM   [dbo].[SHIPPER]
WHERE  [SHIPPER].[SHIPPER_ID] = ?

-- Customer order line details
SELECT *
FROM   [dbo].[CUST_LINE_DEL]
WHERE  [CUST_LINE_DEL].[CUST_ORDER_ID]      = ?
  AND  [CUST_LINE_DEL].[CUST_ORDER_LINE_NO] = ?
```

The `?` placeholders are ODBC parameter markers filled at print-time from other
variables.

### 4.4 ODBC Connection Strings

Immediately following or preceding each SQL query you will find the connection
string:

```
Type=ODBC;DSN=VISUAL;Name=VISUAL;UID=SHOP2;PWD=SHOP!
```

> ⚠️ **Security:** These credentials are embedded in plain text in the binary
> file. Anyone with read access to the `.lbl` file can recover them. Rotate the
> ODBC service-account password after any label file is shared outside a trusted
> perimeter.

Parse the connection string with semicolon splitting. Extract `DSN`, `UID`,
and `PWD` keys.

---

## 5. Reading the `Objects` Stream

The `Objects` stream serialises the label's visual layer as a graph of C++
objects. Each object record has:

```
[1-byte or 2-byte length prefix]
[ASCII class name bytes]
[binary property chain — CDocObjectProperty records]
  FF FE FF   ← sentinel / end-of-properties marker
[coordinate / size data]
[child object records...]
```

### 5.1 Object Classes

| Class Name (ASCII) | Visual Element |
|---|---|
| `CDrawText` | A text or variable field placed on the canvas |
| `CDocObjectProperty` | A named property record (holds variable name bindings) |
| `CsFont` | Font specification attached to a `CDrawText` |
| `CCrlf` | Line-break object |
| `CVariableField` | Reference to a named variable from the `Variables` stream |
| `CLookupField` | Inline lookup reference |
| `CFormulaField` | Formula expression object |
| `CImportField` | Import-mapped field object |
| `CBarCode` | Barcode object (type, symbology, field binding) |
| `CGraphic` | Image / logo object |
| `CLine` / `CBox` | Decorative line or rectangle |

### 5.2 Extracting Field Bindings

To find which variable is bound to which visual text object:

1. Scan for ASCII class name `CDrawText`.
2. Read subsequent `CDocObjectProperty` records until the `FF FE FF` sentinel.
3. The property named `"VAR"` (or similar) contains the variable name that
   supplies the text value.

```csharp
var objectBytes = ole.GetStream("Objects");
var text = Encoding.ASCII.GetString(objectBytes);
// Find every CDrawText block and the variable name that follows it
var bindings = Regex.Matches(text, @"CDrawText.{1,200}?CDocObjectProperty",
    RegexOptions.Singleline);
```

### 5.3 Named Fields Found (ReverseEngThis.lbl)

| Field Name (property record) | Bound Variable |
|---|---|
| `Quantity - Data` | `FORMATTED QUANTITY` |
| `Gross Weight - Data` | `Gross Weight Formula` |
| `Gross Weight Formula` | `Gross Weight Formula` |
| `Date` | `Date` (system date, `ddMONyyyy` format) |
| `2DBarcoded` | assembled barcode formula |

---

## 6. The AIAG MH10.8.2 Format 06 Barcode

The field named **`2DBarcoded`** is a `CFormulaField` that assembles a
**Data Matrix** barcode string conforming to **ANSI MH10.8.2 Format 06**.

### 6.1 Barcode String Structure

```
[)>\x1E06\x1DP{CustomerPart}\x1DQ{Qty}\x1D1J{TraceNo}\x1DK{PO#-Line#}
\x1D20L{Description}\x1D21L{Dock}\x1DB{SupplierCode}\x1D7Q{GrossWeight}\x1E
```

| Control Char | Hex | Name |
|---|---|---|
| `[)>` | `5B 29 3E` | GS1/AIAG header |
| Record Separator | `\x1E` | `RS` — separates records |
| Group Separator | `\x1D` | `GS` — separates data elements within a record |

### 6.2 Application Identifier (AI) Reference

| AI | Field | Note |
|---|---|---|
| `06` (after RS) | Format indicator | Fixed: MH10.8.2 Format 06 |
| `P` | Customer Part Number | No width limit defined in spec |
| `Q` | Quantity | Integer, no leading zeros |
| `1J` | Traceability / Serial Number | `UN` prefix + serial digits |
| `K` | Customer PO Number + Line | `{PO#}-{Line#}` |
| `20` | Supplemental / Description | Part description text |
| `21L` | Ship-To Location / Dock | e.g., `DOCK 80` |
| `B` | Supplier Code | Plant/supplier ID |
| `7Q` | Gross Weight | Numeric + unit suffix (`GT` = gross tons in Labelview use; confirm unit with customer) |

### 6.3 C# Builder

```csharp
public static string BuildAiagMh108Format06(
    string customerPartNumber,
    int quantity,
    string traceabilityNumber,
    string poNumberWithLine,
    string description,
    string dockLocation,
    string supplierCode,
    string grossWeightWithUnit)
{
    const char GS = '\x1D';
    const char RS = '\x1E';

    return $"[)>{RS}06{GS}" +
           $"P{customerPartNumber}{GS}" +
           $"Q{quantity}{GS}" +
           $"1J{traceabilityNumber}{GS}" +
           $"K{poNumberWithLine}{GS}" +
           $"20{description}{GS}" +
           $"21L{dockLocation}{GS}" +
           $"B{supplierCode}{GS}" +
           $"7Q{grossWeightWithUnit}{RS}";
}
```

---

## 7. Reading the `Form` Stream (Canvas Properties)

The `Form` stream encodes the label canvas. Look for these values:

| Property | How to Identify | Typical Value |
|---|---|---|
| Label width | 4-byte little-endian float near offset 0x10 | e.g., `4.00` inches |
| Label height | Next 4-byte float | e.g., `6.00` inches |
| Orientation | 1-byte flag (0 = portrait, 1 = landscape) | `0` |
| Units | 1-byte flag (0 = inches, 1 = mm, 2 = dots) | `0` (inches) |

Scan for ASCII runs — the stream also stores the label name and description as
length-prefixed strings.

---

## 8. Practical Parsing Strategy

### 8.1 Recommended Library (C#)

Use **OpenMcdf** (NuGet: `OpenMcdf`):

```csharp
using OpenMcdf;

var cf = new CompoundFile("ReverseEngThis.lbl");

byte[] printerBytes  = ReadStream(cf, "Printer");
byte[] variablesBytes = ReadStream(cf, "Variables");
byte[] objectsBytes  = ReadStream(cf, "Objects");
byte[] formBytes     = ReadStream(cf, "Form");

cf.Close();

static byte[] ReadStream(CompoundFile cf, string name)
{
    var stream = cf.RootStorage.GetStream(name);
    return stream.GetData();
}
```

### 8.2 PowerShell One-liner (Quick Inspection)

```powershell
$bytes = [IO.File]::ReadAllBytes("ReverseEngThis.lbl")
# Dump printable strings of length >= 6 from the Variables region
$str = [Text.Encoding]::Unicode.GetString($bytes)
[regex]::Matches($str, '[\u0020-\u007E]{6,}') | Select-Object -ExpandProperty Value
```

### 8.3 Python (olefile)

```python
import olefile, re

with olefile.OleFileIO("ReverseEngThis.lbl") as ole:
    printer  = ole.openstream("Printer").read()
    variables = ole.openstream("Variables").read()
    objects  = ole.openstream("Objects").read()

# Extract UTF-16LE strings from Variables stream
text = variables.decode("utf-16-le", errors="replace")
strings = re.findall(r'[\x20-\x7E\u00c0-\u00ff]{4,}', text)
sql_queries = [s for s in strings if s.upper().startswith("SELECT")]
odbc_strings = [s for s in strings if "DSN=" in s]
```

---

## 9. Field Inventory Checklist

When asked to fully parse a `.lbl` file, extract and document:

- [ ] **File identity** — size, OLE2 magic confirmed, LabelView version hint from `Contents` stream
- [ ] **Stream list** — names, sizes, storage type (regular / mini)
- [ ] **Printer binding** — device name, IP/port, language (ZPL II / TSPL / etc.)
- [ ] **Canvas** — width × height, orientation, units (from `Form`)
- [ ] **All variables** — name, class, current/sample value (from `Variables`)
- [ ] **All SQL queries** — full text, table names, parameter count (from `Variables`)
- [ ] **ODBC connection strings** — DSN, UID (flag PWD as sensitive, do not log)
- [ ] **All drawing objects** — class, canvas position, variable binding (from `Objects`)
- [ ] **Barcode fields** — symbology, AI structure, formula text
- [ ] **Font inventory** — families and sizes used
- [ ] **Security findings** — any hardcoded credentials, flag for rotation

---

## 10. Known Limitations and Edge Cases

| Situation | Guidance |
|---|---|
| Stream not found | Skip gracefully — not all labels use all streams |
| Length prefix is 1 byte vs 2 bytes | LabelView uses both; test both and take the one that produces a printable string |
| Multi-byte UTF-16 chars in variable names | Use `Encoding.Unicode` (UTF-16LE), not ASCII |
| Null bytes interleaved with text | Strip null bytes before regex matching when treating as ASCII |
| Binary property records between strings | Treat any run of non-printable bytes as a property fence; skip to next printable run |
| Multiple ODBC connection strings | Each `CLookupField` stores its own copy; deduplicate by DSN |
| Encrypted / obfuscated labels | Labelview Pro supports password protection; the `General` stream flag byte 4 != 0 indicates a protected label — binary parsing will still work but field values will be scrambled |

---

## 11. Version Notes

- **LabelView 2018 / 2019 / 2022** all use the same OLE2 container and stream
  names documented here.
- **LabelView 12 and earlier** used a different (non-OLE2) proprietary format.
  Do not apply this guide to pre-2018 `.lbl` files.
- The `\x05SummaryInformation` stream in 2022 labels is significantly larger
  than in 2019 labels due to embedded render-cache data; its content is not
  human-readable and can be skipped for field extraction purposes.
