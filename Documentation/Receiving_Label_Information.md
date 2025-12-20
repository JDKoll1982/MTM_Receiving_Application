# Expo - Return Label Analysis & Layout Guide

## 1. Label Overview
*   **File:** `Expo - Return Label ver. 1.0.lbl`
*   **Software:** Teklynx LabelView 2022 (inferred)
*   **Dimensions:** 11" x 8.5" (Landscape)
*   **Printer Target:** Konica Minolta Universal PS (IP: 172.20.21.23)
*   **Fonts:** Primarily **Arial**

## 2. Data Sources (Database)
The label connects directly to the **Infor Visual ERP** database via ODBC (`VISUAL` DSN).

| Field Name | Database Table.Column | SQL Query Used | Notes |
| :--- | :--- | :--- | :--- |
| **Material ID** | `PART.ID` | `SELECT * FROM PART WHERE ID = ?` | Primary identifier |
| **Weight** | `RECEIVER_LINE.QTY` | (Inferred) | Displayed as "Weight" |
| **Description** | `PART.DESCRIPTION` | `SELECT DESCRIPTION FROM PART...` | |
| **PO Number** | `PURCHASE_ORDER.ID` | Linked via `PO Filler A` | |
| **Vendor** | `VENDOR.NAME` | `SELECT NAME FROM VENDOR WHERE ID = ?` | |
| **Transaction Date** | System Date | | |
| **Lot Number** | `TRACE.ID` | | Replaces Heat Number in some contexts |
| **Received By** | User Input | | Employee ID |
| **Located To** | Warehouse Loc | | |
| **Receivers** | Variable | | |
| **Scrap Type** | User Defined | | |

## 3. Layout Structure

### A. Header Section
*   **Docking:** Top
*   **Top Left:** MTM Logo (`MTM_Logo-Rev_100H.png`).
*   **Top Right:** MTM Logo (`MTM_Logo-Rev_100H.png`).
*   **Top Right Corner (Above Logo):** Small text "QF840-3-1 Rev. B." (approx 10pt).
*   **Center:**
    *   Line 1: "Manitowoc Tool and Manufacturing" (Medium-Large, ~24pt, Arial Regular).
    *   Line 2: "Receiving Tag" (Extra Large, ~36pt, Arial Regular).

### B. Main Body (Center)
This section contains the primary identification data, centered and prominent.
*   **Docking:** Fill (Center Vertical Alignment)

1.  **Material ID Group:**
    *   Label: "Material ID" (Centered, Underlined, Arial Regular, ~18pt).
    *   Value: **Huge Bold Text** (~72pt+, e.g., "MMF0005516").
    *   Barcode: 1D Barcode (Code 128/39) positioned to the right of the value.

2.  **Weight Group:**
    *   Label: "Weight" (Centered, Underlined, Arial Regular, ~18pt).
    *   Value: **Huge Bold Text** (~72pt+, e.g., "2500").
    *   Barcode: 1D Barcode positioned to the right of the value.

3.  **Details Lines (Below Weight):**
    *   Font Size: ~14-16pt.
    *   **Row 1:** "Scrap Type: [Value]" (Left Aligned).
    *   **Row 2:**
        *   **Left:** "Description: [Value]" (Left Aligned).
        *   **Right:** "Received By: [ID]" (Right Aligned, **Bold**, **Underlined**, **Blue** text).

### C. Footer Section
Separated from the body by a **thick horizontal line** (~3-4pt).
*   **Docking:** Bottom

*   Font Size: ~16-18pt.
*   **Left Column:**
    *   "Transaction Date: [MM/DD/YYYY]"
    *   "PO Number: [PO-XXXXXX]"
    *   "Located To: [Location]"
*   **Right Column:**
    *   "Lot Number: [Value]"
    *   "Vendor: [Vendor Name]"
    *   "Receivers: [Value]"

## 4. Formula Logic (Business Rules)

### A. PO Number Logic
Handles special order types where a standard PO number might not exist.
```vb
IF (Search({PO Number}, "PO-", 1) = 1) THEN
    Result = {PO Number}
ELSEIF (Exact({PO Number}, "")) THEN
    Result = "Nothing Entered"
ELSEIF ({PO Number} = "Customer Supplied") THEN
    Result = "Customer Supplied"
ELSEIF (Search({PO Number}, "Credit Card", 1)) THEN
    Result = "Credit Card"
ELSE
    Result = "PO-" & {PO Number}
END IF
```

### B. Employee / Received By
Conditionally displays the employee ID.
```vb
Result = "Received By: " & {EmployeeID}
```

### C. Scrap Type
Displays the reason for return/scrap.
```vb
IF ({Get Scrap (User Defined 8)} = "") THEN
    Result = ""
ELSE
    Result = "Scrap Type: " & {Get Scrap (User Defined 8)}
END IF
```

## 5. Replication Guide for WinUI 3

1.  **ViewModel (`ViewModel_LabelPrint`):**
    *   Properties: `MaterialID`, `Weight`, `Description`, `ScrapType`, `ReceivedBy`, `TransactionDate`, `PoNumber`, `LocatedTo`, `LotNumber`, `Vendor`, `Receivers`.
    *   Implement formatting logic (e.g., "PO-" prefix) in the ViewModel.

2.  **View (`LabelPrintView.xaml`):**
    *   **Grid Layout:**
        *   Row 0: Header (Logos, Title).
        *   Row 1: Material ID (Label, Value, Barcode).
        *   Row 2: Weight (Label, Value, Barcode).
        *   Row 3: Details (Scrap, Desc, Received By).
        *   Row 4: Separator Line (Rectangle with Height=2).
        *   Row 5: Footer (2 Columns).
    *   **Styling:**
        *   Use `Underline` text decorations for labels.
        *   Use `FontWeight="Bold"` for values.
        *   Use `Foreground="Blue"` for "Received By".

3.  **Printing:**
    *   Ensure the page size is set to 11x8.5 Landscape.
    *   Use `PrintDocument` or a reporting service to send the XAML visual to the printer.
