# Infor Visual Queries Report
> **Last Generated:** 2026-01-13
> **Database Target:** VISUAL.MTMFG (Infor Visual)
> **Access Level:** READ ONLY

This document tracks all SQL queries used to access the Infor Visual database.

## Query Inventory

| Query Name / File | Method Usage | Parameter(s) | Query |
|-------------------|--------------|--------------|-------|
| **01_GetPOWithParts.sql** | `Dao_InforVisualConnection.GetPOWithPartsAsync` | `@PoNumber` (string) | `SELECT po.ID AS PoNumber, pol.LINE_NO AS PoLine, pol.PART_ID AS PartNumber, p.DESCRIPTION AS PartDescription, pol.ORDER_QTY AS OrderedQty, pol.TOTAL_RECEIVED_QTY AS ReceivedQty, (pol.ORDER_QTY - pol.TOTAL_RECEIVED_QTY) AS RemainingQty, pol.PURCHASE_UM AS UnitOfMeasure, pol.PROMISE_DATE AS DueDate, po.VENDOR_ID AS VendorCode, v.NAME AS VendorName, po.STATUS AS PoStatus, po.SITE_ID AS SiteId FROM dbo.PURCHASE_ORDER po INNER JOIN dbo.PURC_ORDER_LINE pol ON po.ID = pol.PURC_ORDER_ID LEFT JOIN dbo.PART p ON pol.PART_ID = p.ID LEFT JOIN dbo.VENDOR v ON po.VENDOR_ID = v.ID WHERE po.ID = @PoNumber ORDER BY pol.LINE_NO;` |
| **02_ValidatePONumber.sql** | `Dao_InforVisualConnection.ValidatePoNumberAsync` | `@PoNumber` (string) | `SELECT COUNT(*) AS POExists FROM dbo.PURCHASE_ORDER WHERE ID = @PoNumber;` |
| **03_GetPartByNumber.sql** | `Dao_InforVisualConnection.GetPartByNumberAsync` | `@PartNumber` (string) | `SELECT p.ID AS PartNumber, p.DESCRIPTION AS Description, ps.UNIT_MATERIAL_COST AS UnitCost, p.STOCK_UM AS PrimaryUom, COALESCE(ps.QTY_ON_HAND, 0) AS OnHandQty, COALESCE(ps.QTY_COMMITTED, 0) AS AllocatedQty, (COALESCE(ps.QTY_ON_HAND, 0) - COALESCE(ps.QTY_COMMITTED, 0)) AS AvailableQty, ps.SITE_ID AS DefaultSite, ps.STATUS AS PartStatus, p.PRODUCT_CODE AS ProductLine FROM dbo.PART p LEFT JOIN dbo.PART_SITE ps ON p.ID = ps.PART_ID WHERE p.ID = @PartNumber;` |
| **04_SearchPartsByDescription.sql** | `<Inferred>` | `@SearchTerm` (string), `@MaxResults` (int) | `SELECT TOP (@MaxResults) p.ID AS PartNumber, p.DESCRIPTION AS Description, ps.UNIT_MATERIAL_COST AS UnitCost, p.STOCK_UM AS PrimaryUom, COALESCE(ps.QTY_ON_HAND, 0) AS OnHandQty, COALESCE(ps.QTY_COMMITTED, 0) AS AllocatedQty, (COALESCE(ps.QTY_ON_HAND, 0) - COALESCE(ps.QTY_COMMITTED, 0)) AS AvailableQty, ps.SITE_ID AS DefaultSite, ps.STATUS AS PartStatus, p.PRODUCT_CODE AS ProductLine FROM dbo.PART p LEFT JOIN dbo.PART_SITE ps ON p.ID = ps.PART_ID WHERE p.DESCRIPTION LIKE @SearchTerm + '%' ORDER BY p.ID;` |

## Implementation Details

All queries are executed via `MTM_Receiving_Application.Module_Core.Data.InforVisual.Dao_InforVisualConnection` class.
Queries are stored as external `.sql` files in `Database/InforVisualScripts/Queries/` and loaded at runtime.
All connections must be READ ONLY (`ApplicationIntent=ReadOnly`).
Connection string is managed by `Helper_Database_Variables`.

### File Locations

- **Source Code**: [Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs](../../Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs)
- **SQL Scripts**: [Database/InforVisualScripts/Queries](../../../../Database/InforVisualScripts/Queries)

## Verification Log
**Date:** 2026-01-13
**Tester:** Automated Console App

| Test Case | ID | Result | Notes |
|-----------|----|--------|-------|
| Connection | Server Check | ✅ PASS | Version 13.00.5893 connected successfully |
| PO Lookup (Parts) | PO-067343 | ✅ PASS | Retrieved 1 line, Part: MMC0000658, Desc: Coil... |
| Part Lookup | MMC0000658 | ✅ PASS | Details retrieved from PART table |
| PO Lookup (Service) | PO-067381 | ✅ PASS | Retrieved 2 lines, Empty Part IDs (Expected) |
| Search Query | 'MMC' | ✅ PASS | Query executed (0 results for description search) |
| Hardcoded Validation | PO-067343 | ❌ FAIL | Invalid column names `PO_ID`, `SITE_REF` |
| Hardcoded Lines | PO-067343 | ❌ FAIL | Invalid columns: `PO_ID`, `QTY_RECEIVED`, `PART_NAME`, etc. |
| Correction Verification | PO-067343 | ✅ PASS | Validated with `ID`, `SITE_ID`='MTM2', `STATUS` IN ('O','P','R') |
| Query Verification | GetLines | ✅ PASS | Retrieved 1 row. Columns: `PO_ID` (aliased), `PO_LINE` (aliased), `PART_NAME` (aliased), `QTY_RECEIVED` (aliased) |

### Tested Queries
All queries (01-04) executed successfully against the live database without SQL errors.
READ ONLY intent was respected.

## Module_Routing Inventory

| Query Name / Location | Method Usage | Parameter(s) | Query |
|-----------------------|--------------|--------------|-------|
| **Hardcoded (Module_Routing)** | `Dao_InforVisualPO.ValidatePOAsync` | `@PoNumber` | `SELECT COUNT(*) FROM PURCHASE_ORDER WITH (NOLOCK) WHERE PO_ID = @PoNumber AND SITE_REF = '002' AND STATUS IN ('O', 'P')` |
| **Hardcoded (Module_Routing)** | `Dao_InforVisualPO.GetLinesAsync` | `@PoNumber` | `SELECT pol.PO_ID, pol.PO_LINE, pol.PART_ID, pol.QTY_ORDERED, pol.QTY_RECEIVED, pol.UNIT_PRICE, pol.STATUS, p.PART_NAME, po.VENDOR_ID FROM PURC_ORDER_LINE pol WITH (NOLOCK) INNER JOIN PURCHASE_ORDER po WITH (NOLOCK) ON pol.PO_ID = po.PO_ID LEFT JOIN PART p WITH (NOLOCK) ON pol.PART_ID = p.PART_ID WHERE pol.PO_ID = @PoNumber AND pol.SITE_REF = '002' ORDER BY pol.PO_LINE` |
| **Hardcoded (Module_Routing)** | `Dao_InforVisualPO.GetLineAsync` | `@PoNumber`, `@LineNumber` | Same as GetLinesAsync with `AND pol.PO_LINE = @LineNumber` |

> **Note:** These queries appear to use incorrect column names (e.g. `PO_ID` vs `ID`, `SITE_REF` vs `SITE_ID`) and are expected to fail.

### Findings & Recommendations
The `Module_Routing` DAO (`Dao_InforVisualPO`) contains severely broken SQL that does not match the live schema.
**Action Required:**
**Global Updates Applied (2026-01-13):**
Refactored `Module_Routing\Data\Dao_InforVisualPO.cs` to use corrected column names and aliases matching the C# model.
Verified against live database.
Mismatches resolved:
- `PO_ID` -> `PURC_ORDER_ID` (aliased as PO_ID)
- `SITE_REF` -> `SITE_ID`
- `QTY_RECEIVED` -> `TOTAL_RECEIVED_QTY` (aliased as QTY_RECEIVED)
- `PART_NAME` -> `DESCRIPTION` (aliased as PART_NAME)
