# ReverseEngThis.lbl — Complete Reverse-Engineering Analysis

**File:** `docs/LabelView/ReverseEngThis.lbl`  
**Size:** 324,096 bytes  
**Format:** OLE2 Compound Document (Microsoft Structured Storage)  
**Application:** TEKLYNX Labelview (Labelview 2018 / 2019 / 2022 era)  
**Analysed:** 2026-03-19

---

## 1. OLE2 Stream Inventory

The file is an OLE2 compound document with 14 named streams spread across 4 directory sectors:

| Stream Name | Size (bytes) | Storage Location | Purpose |
|---|---:|---|---|
| `Objects` | 134,566 | Regular sector 230 | All label drawing objects, their positions, fonts, and field-name bindings |
| `\x05SummaryInformation` | 107,201 | Regular sector 19 | Labelview internal metadata / render data |
| `Variables` | 39,691 | Regular sector 547 | Variable/field definitions, lookups, SQL queries, ODBC connection strings |
| `Form` | 26,376 | Regular sector 495 | Canvas/form properties (page size, margins, orientation) |
| `Printer` | 1,855 | Mini-sector 17 | Bound printer name, port, IP address, media settings |
| `Contents` | 733 | Mini-sector 47 | Label content summary |
| `GroupsObjects` | 382 | Mini-sector 7 | Object group definitions |
| `Format` | 130 | Mini-sector 14 | Print format settings |
| `InputPrintOrder` | 18 | Mini-sector 6 | Print-order configuration |
| `BackGround` | 32 | Mini-sector 4 | Background fill/graphic settings |
| `\x05DocumentSummaryInformation` | 193 | Mini-sector 0 | OLE document property |
| `General` | 12 | Mini-sector 46 | Short general label settings |
| `VarsCategories` | 2 | Mini-sector 13 | Variable category flags |
| `Database` | 2 | Mini-sector 5 | DB connection flag |
| `RfTagObject` | 0 | (empty stream) | RFID tag placeholder (unused) |

Mini-sectors live within the root-entry mini-stream container (regular sectors 18 → 625 → … 8 sectors, total 3,776 bytes).

---

## 2. Bound Printer

From the `Printer` stream:

| Property | Value |
|---|---|
| Driver / device name | `ZDesigner GK420d zebra7` |
| IP / port | `172.16.3.112` |
| Media type | `User defined` |
| Printer language | ZPL II (ZDesigner driver) |

The label is printed on a **Zebra GK420d** (203 dpi) over TCP/IP at 172.16.3.112.

---

## 3. Label Fields and Object Classes

All label objects are serialized in the `Objects` stream as a binary graph of C++ objects. The pattern for each record is:

```
[object-class-name (ASCII, length-prefixed)]
[CDocObjectProperty records...]
  ff fe ff   <-- sentinel
  [byte: string length]
  [UTF-16LE field/variable name]
  ...coordinates, font data, flags...
```

### 3.1 Object Classes Encountered

| Class Name | Type | Description |
|---|---|---|
| `CDrawText` | Drawing | Text/variable label field |
| `CDocObjectProperty` | Property | Stores a named variable binding |
| `CsFont` | Font | Font specification (Arial Narrow, etc.) |
| `CCrlf` | Drawing | Carriage-return/line-feed separator |
| `CVariableField` | Variable | Labelview variable referencing a named value |
| `CLookupField` | Lookup | Resolves variable value via ODBC database query |
| `CFormulaField` | Formula | Computes a value from an expression/formula |
| `CImportField` | Import | Field populated from external data import |

### 3.2 Named Fields Found in `Objects` Stream

| Field Name (as stored) | Object Class | Sample Value |
|---|---|---|
| `Quantity - Data` | `CDocObjectProperty` | `1040` |
| `Gross Weight - Data` | `CDocObjectProperty` | `144.04` |
| `Gross Weight Formula` | `CDocObjectProperty` / formula | `144.04 KG` |
| `Date` | `CDocObjectProperty` | `17NOV2025` |

### 3.3 Font Used

- **Arial Narrow**, printed across all text objects.
- Font size varies; smallest observed ~7 pt, largest ~12 pt.

---

## 4. Variable Definitions (`Variables` Stream)

The `Variables` stream contains every named variable used on the label, each with its type, variable name, SQL query (for lookup fields), and a sample/current value.

### 4.1 Complete Variable Inventory

| Variable Name (label identifier) | Class | Sample Value | Source |
|---|---|---|---|
| `Lookup(CUSTOMER_PO_REF)` | `CLookupField` | `505384` | Infor Visual `SHIPPER` |
| `Referenced` | `CLookupField` | `505384-00230` | Composite PO#-Line# |
| `Lookup(Customer_PO_Line)` | `CLookupField` | `00230` | Infor Visual `CUST_LINE_DEL` |
| `Lookup(CUST_ORDER_LINE_NO)` | `CLookupField` | (see SQL) | Infor Visual `CUST_LINE_DEL` |
| `Packlist_Line_Number` | `CVariableField` | `1` | Input/import |
| `LABEL(SHOPADDRESS)` | `CLookupField` | `DOCK 80` | Input / lookup |
| `Label(ShopAddress)` | `CVariableField` | `DOCK 80` | Pulled from `LABEL(SHOPADDRESS)` |
| `Lookup(SHIPPER_SHIPTO_ADDRESS_NO)` | `CLookupField` | (address no.) | Infor Visual `SHIPPER` |
| `FORMATTED QUANTITY` | formula | `1,040` | Formatted `Quantity` |
| `Formula CUST_ADDRESS_CITY` | formula | `INDIANAPOLIS` | `CUST_ADDRESS` |
| `Customer City, State, Zip` | formula | `INDIANAPOLIS, IN 46268` | `CUST_ADDRESS` |
| `Formula CUST_ADDRESS_ZIP` | formula | `46268` | `CUST_ADDRESS` |
| `Formula Address Line 1 NAME` | formula | `ATI INTERNATIONAL CROSS DOCK` | `CUST_ADDRESS` |
| `Lookup(CUST_ORDER_LINE_NO)` | `CLookupField` | (line number) | `CUST_LINE_DEL.USER_2` |
| `Lookup(Customer_PO_Line)` | second reference | `00230` | — |
| `2DBarcoded` | `CFormulaField` | *(full barcode string — see §5)* | Assembled from all other vars |

---

## 5. 2D Barcode — AIAG MH10.8.2 Format 06

The field named **`2DBarcoded`** is a `CFormulaField` that builds a **Data Matrix** (or PDF-417) barcode using **ANSI MH10.8.2 Format 06** encoding.

### 5.1 Sample Barcode Content (UTF-16LE in file)

```
[)>RS 06 GS P295077577 GS Q1040 GS 1JUN829153662000000001 GS K505384-00230 GS 20LOHM 1394 (1394) GS 21LDOCK 80 GS BSC151208 GS 7Q144.04GT RS
```

Legend: `RS` = 0x1E (Record Separator), `GS` = 0x1D (Group Separator)

### 5.2 Application Identifier Mapping

| AI Code | Meaning | Sample Data |
|---|---|---|
| Header `[)>` | GS1 / AIAG symbology ID | fixed |
| `RS 06` | Format indicator (MH10.8.2 Format 06) | fixed |
| `P` | Customer Part Number | `295077577` |
| `Q` | Quantity | `1040` |
| `1J` | Supplier Serial / Traceability Number | `UN829153662000000001` |
| `K` | Customer Purchase Order Number | `505384-00230` |
| `20` | Customer Supplemental Data (description) | `LOHM 1394 (1394)` |
| `21L` | Ship-To Location / Dock | `DOCK 80` |
| `B` | Supplier Code | `SC151208` |
| `7Q` | Gross Weight (with unit suffix `GT` = grams, tonnes?) | `144.04GT` |

### 5.3 C# Barcode Formula

```csharp
// Build AIAG MH10.8.2 Format 06 barcode content
private static string BuildAiagBarcode(
    string customerPartNumber,   // P
    int quantity,                // Q
    string serialNumber,         // 1J
    string poNumber,             // K  (PO# + "-" + line)
    string customerSupplement,   // 20
    string dockLocation,         // 21L
    string supplierCode,         // B
    string grossWeight)          // 7Q  (include unit suffix, e.g. "GT")
{
    const char GS = '\x1D';
    const char RS = '\x1E';

    return $"[)>{RS}06{GS}P{customerPartNumber}"+
           $"{GS}Q{quantity}"+
           $"{GS}1J{serialNumber}"+
           $"{GS}K{poNumber}"+
           $"{GS}20{customerSupplement}"+
           $"{GS}21L{dockLocation}"+
           $"{GS}B{supplierCode}"+
           $"{GS}7Q{grossWeight}{RS}";
}
```

---

## 6. Database Connections (ODBC)

> ⚠️ **SECURITY NOTE:** Hardcoded credentials were found in the label file. These are Infor Visual ODBC credentials stored in the Labelview template and are independent of the C# app's own connection management. Labelview's runtime uses them directly via ODBC. They should be rotated if the service account can be changed, and the label template resaved.

### 6.1 Connection Strings Found

Two slightly different credential variants appear (same DSN, same user, differing password suffix):

```
Type=ODBC;DSN=VISUAL;Name=VISUAL;UID=SHOP2;PWD=SHOP!
Type=ODBC;DSN=VISUAL;Name=VISUAL;UID=SHOP2;PWD=SHOP
```

Both reference the **Windows ODBC DSN `VISUAL`** (Infor Visual's system DSN).

### 6.2 SQL Queries Embedded in Variables

#### Customer Address Lookup
```sql
SELECT *
FROM   [dbo].[CUST_ADDRESS]
WHERE  [CUST_ADDRESS].[CUSTOMER_ID] = ?
  AND  [CUST_ADDRESS].[ADDR_NO]     = ?
```
→ Populates `Customer City, State, Zip`, `Formula Address Line 1 NAME`, `Formula CUST_ADDRESS_CITY`, `Formula CUST_ADDRESS_ZIP`

#### Shipper / Pack-List Header
```sql
SELECT *
FROM   [SHIPPER]
WHERE  [SHIPPER].[CUST_ORDER_ID]  = ?
  AND  [SHIPPER].[PACKLIST_ID]    = ?
```
→ Populates `Lookup(SHIPPER_SHIPTO_ADDRESS_NO)` and related fields.

#### Customer Delivery Line (USER_2 field)
```sql
SELECT TOP 1 CLD.USER_2
FROM   dbo.CUST_LINE_DEL AS CLD
WHERE  CLD.CUST_ORDER_ID        = ?
  AND  CLD.CUST_ORDER_LINE_NO   = ?
```
→ Populates `Lookup(CUST_ORDER_LINE_NO)`.

#### Part / Shipper Line Detail (complex join)
```sql
SELECT …
FROM   SHIPPER_LINE_DEL
         INNER JOIN CUST_ORDER_LINE
                 ON CUST_ORDER_LINE.CUST_ORDER_ID       = SHIPPER_LINE_DEL.CUST_ORDER_ID
                AND CUST_ORDER_LINE.LINE_NO              = SHIPPER_LINE_DEL.CUST_ORDER_LINE_NO
WHERE  SHIPPER_LINE_DEL.PACKLIST_ID     = ?
  AND  CUST_ORDER_LINE.PART_ID          = ?
```

---

## 7. Map: Label Field → Application Data Source

| Label Field or Barcode AI | Maps To (application) | Infor Visual Table / Field |
|---|---|---|
| `P` (Customer Part Number) | `Model_ReceivingLine.PartId` or customer X-ref | `CUST_ORDER_LINE.PART_ID` |
| `Q` (Quantity) | `Model_ReceivingLine.Quantity` | `SHIPPER_LINE_DEL.…` |
| `K` (PO Number + Line) | `Model_ReceivingLine.PONumber + "-" + LineNo` | `SHIPPER.CUST_ORDER_ID` / line |
| `1J` (Serial/Traceability) | Serial number or lot number (generated or imported) | — |
| `20` (Customer Supplement) | Customer item description | `CUST_ORDER_LINE.USER_2` / part desc. |
| `21L` (Dock) | `LABEL(SHOPADDRESS)` — dock assignment | Input / lookup table |
| `B` (Supplier Code) | Supplier / plant code | Configuration or VENDOR table |
| `7Q` (Gross Weight) | `Model_ReceivingLine.GrossWeight` | Calculated / entered |
| `Date` | Receive date (formatted `ddMMMyyy` uppercase) | `DateTime.Now` at print time |
| `Customer City, State, Zip` | Ship-to address — city, state, ZIP | `CUST_ADDRESS` |
| `Formula Address Line 1 NAME` | Ship-to company name | `CUST_ADDRESS` |
| `Packlist_Line_Number` | Pack-list line counter | Sequential, per pack list |

---

## 8. Printing from C# (Instead of Labelview)

Because the label is bound to a specific local Labelview installation and ODBC DSN, the C# application has two options:

### Option A — Drive Labelview via COM Automation
Labelview exposes a COM object (`LVApp.Application`). The application can:
1. Open `ReverseEngThis.lbl` silently.
2. Set each variable by name: `label.Variables["Quantity - Data"].Value = qty.ToString();`
3. Call `label.Print(1)`.

This requires Labelview to be installed on the print machine.

### Option B — Generate ZPL II Directly (No Labelview)
Send ZPL directly to the GK420d at `172.16.3.112:9100`. All field data comes from the app's database/models. The AIAG barcode is assembled with `BuildAiagBarcode(…)` and encoded as a `^BX` (Data Matrix) or `^B2` (Interleaved 2-of-5) command.

Minimal ZPL skeleton matching the observed label layout:

```
^XA
^CI28                      ; UTF-8 encoding
^PW812                     ; 4 inch at 203 dpi
^LL1218                    ; 6 inch label

; --- Customer name / address block ---
^FO30,30^A0N,28,28^FD{shipToName}^FS
^FO30,65^A0N,22,22^FD{shipToAddress}^FS
^FO30,95^A0N,22,22^FD{cityStateZip}^FS

; --- Part number / quantity ---
^FO30,140^A0N,40,40^FDPart: {customerPartNo}^FS
^FO30,190^A0N,40,40^FDQty:  {quantity}^FS

; --- PO / dock / date ---
^FO30,240^A0N,28,28^FDPO: {poNumber}^FS
^FO30,280^A0N,28,28^FDDock: {dockLocation}^FS
^FO30,320^A0N,28,28^FDDate: {date}^FS

; --- Weight ---
^FO30,370^A0N,28,28^FDGross Weight: {grossWeight}^FS

; --- AIAG Data Matrix barcode ---
^FO500,30^BXN,6,200^FD{barcodeContent}^FS

^XZ
```

---

## 9. Implementation Checklist for the MTM Receiving Application

Given the above analysis, a full label-printing feature needs:

### Data Required at Print Time
- [ ] Customer Part Number (from Infor Visual `CUST_ORDER_LINE.PART_ID`)
- [ ] Quantity shipped/received (`SHIPPER_LINE_DEL` or entered)
- [ ] Purchase Order Number + Line Number
- [ ] Serialization / traceability number (lot or sequence)
- [ ] Customer supplement text (item description from `CUST_ORDER_LINE.USER_2` or part description)
- [ ] Dock / ship-to location code (`LABEL(SHOPADDRESS)` — stored in local lookup or infer from customer)
- [ ] Supplier / plant code (configuration value)
- [ ] Gross weight (calculated or entered)
- [ ] Ship date (today)
- [ ] Customer name, address line 1, city/state/zip (from `CUST_ADDRESS` via `CUSTOMER_ID + ADDR_NO`)

### C# Service Method Signature (suggested)

```csharp
public interface IService_Label_Shipping
{
    /// <summary>Prints one AIAG MH10.8.2 shipping label to the Zebra GK420d.</summary>
    Task<Model_Dao_Result> PrintShippingLabelAsync(Model_ShippingLabel labelData);
}

public class Model_ShippingLabel
{
    public string CustomerPartNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string PoNumber { get; set; } = string.Empty;    // "505384-00230" style
    public string SerialNumber { get; set; } = string.Empty;
    public string CustomerSupplement { get; set; } = string.Empty;
    public string DockLocation { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty;
    public string GrossWeight { get; set; } = string.Empty; // "144.04GT"
    public DateTime ShipDate { get; set; } = DateTime.Today;
    public string ShipToName { get; set; } = string.Empty;
    public string ShipToAddress { get; set; } = string.Empty;
    public string ShipToCityStateZip { get; set; } = string.Empty;
    public string PacklistLineNumber { get; set; } = "1";
}
```

---

## 10. Infor Visual SQL Queries Needed (for DAO)

Add these to the Infor Visual (SQL Server, read-only) DAO:

```sql
-- Get ship-to customer address
SELECT ca.ADDR_LINE_1, ca.CITY, ca.STATE, ca.ZIP, ca.COUNTRY
FROM   CUST_ADDRESS ca
WHERE  ca.CUSTOMER_ID = @customerId
  AND  ca.ADDR_NO     = @addrNo;

-- Get shipper header
SELECT sh.CUST_ORDER_ID, sh.PACKLIST_ID, sh.SHIPTO_ADDRESS_NO
FROM   SHIPPER sh
WHERE  sh.CUST_ORDER_ID = @custOrderId
  AND  sh.PACKLIST_ID   = @packlistId;

-- Get customer supplement / user data per line
SELECT TOP 1 cld.USER_2
FROM   CUST_LINE_DEL cld
WHERE  cld.CUST_ORDER_ID       = @custOrderId
  AND  cld.CUST_ORDER_LINE_NO  = @lineNo;
```

---

## Appendix: Raw Binary References

| What | File Offset | Notes |
|---|---|---|
| OLE2 Header | `0x0000` | Sector size 512, 5 FAT sectors |
| Directory sector 0 | `0x0600` | Root, Contents, General, Printer |
| Directory sector 1 | `0x0A00` | Format, VarsCategories, GroupsObjects, Variables |
| Directory sector 2 | `0x0E00` | RfTagObject, Form, InputPrintOrder, Objects |
| Directory sector 3 | `0x1000` | Database, BackGround, SummaryInfo, DocSummaryInfo |
| FAT sector 0 | `0x0800` | Sector chains |
| Mini-FAT sector | `0x0C00` | Mini-stream sector chains |
| Objects stream start | `0x1CE00` | First object: `CDrawText` |
| Variables stream start | `0x44600` | First variable: barcode formula |
| Printer stream (in mini-stream) | mini-sector 17 | "ZDesigner GK420d zebra7" |
| Barcode formula string | `~0x44920` | AIAG MH10.8.2 sample data |
| ODBC connection string | `0x464E8+` | `Type=ODBC;DSN=VISUAL;UID=SHOP2;PWD=SHOP!` |
