-- ========================================
-- Query: Get PO with Parts
-- Description: Retrieves purchase order details with all associated parts
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- Site: 002
-- ========================================

-- NOTE: Using base tables (PURCHASE_ORDER, PURC_ORDER_LINE) instead of views (po, po_line)
-- Adjust table names if your environment uses different names (e.g. PURCHASE_ORDER)
-- NOTE: No status check is performed. Returns all POs regardless of status (Open, Closed, etc.)
-- NOTE: PO lines are ordered by expected receive date first, with mostly satisfied lines pushed last.
SELECT 
    po.ID AS PoNumber,
    pol.LINE_NO AS PoLine,
    pol.PART_ID AS PartNumber,
    p.DESCRIPTION AS PartDescription,
    COALESCE(ps.PRIMARY_LOC_ID, fallback.LocationId, pol.CONSIGNED_LOC_ID, '') AS DefaultLocationId,
    pol.ORDER_QTY AS OrderedQty,
    pol.TOTAL_RECEIVED_QTY AS ReceivedQty,
    (pol.ORDER_QTY - pol.TOTAL_RECEIVED_QTY) AS RemainingQty,
    pol.PURCHASE_UM AS UnitOfMeasure,
    sortData.SelectedDueDate AS DueDate,
    po.VENDOR_ID AS VendorCode,
    v.NAME AS VendorName,
    po.STATUS AS PoStatus,
    po.PROMISE_DATE AS HeaderPromiseDate,
    po.DESIRED_RECV_DATE AS HeaderDesiredRecvDate,
    po.FREE_ON_BOARD AS FreeOnBoard,
    po.SITE_ID AS SiteId
FROM dbo.PURCHASE_ORDER po
INNER JOIN dbo.PURC_ORDER_LINE pol ON po.ID = pol.PURC_ORDER_ID
LEFT JOIN dbo.PART p ON pol.PART_ID = p.ID
LEFT JOIN dbo.PART_SITE ps ON pol.PART_ID = ps.PART_ID AND ps.SITE_ID = po.SITE_ID
OUTER APPLY
(
        SELECT TOP (1)
                cpl.LOCATION_ID AS LocationId
        FROM dbo.CR_PART_LOCATION cpl
        WHERE cpl.ID = pol.PART_ID
            AND cpl.WAREHOUSE_ID = po.SITE_ID
            AND cpl.LOCATION_ID IS NOT NULL
            AND cpl.LOCATION_ID <> ''
        ORDER BY
                CASE WHEN cpl.DEF_INSPECT_LOC = 'Y' THEN 0 ELSE 1 END,
                CASE WHEN cpl.AUTO_ISSUE_LOC = 'Y' THEN 0 ELSE 1 END,
                cpl.LOCATION_ID
) fallback
CROSS APPLY
(
        SELECT
                COALESCE(
                    pol.DESIRED_RECV_DATE,
                    pol.PROMISE_DATE,
                    po.DESIRED_RECV_DATE,
                    po.PROMISE_DATE
                ) AS SelectedDueDate,
                CASE
                    WHEN NULLIF(pol.ORDER_QTY, 0) IS NULL THEN CAST(0 AS decimal(20, 8))
                    ELSE pol.TOTAL_RECEIVED_QTY / NULLIF(pol.ORDER_QTY, 0)
                END AS ReceivedSatisfactionRatio
) sortData
LEFT JOIN dbo.VENDOR v ON po.VENDOR_ID = v.ID
WHERE po.ID = @PoNumber
-- AND po.SITE_ID = '002' -- Commented out to allow finding POs in other sites for testing
ORDER BY
    CASE
        WHEN sortData.ReceivedSatisfactionRatio > 0.75 AND sortData.SelectedDueDate IS NOT NULL THEN 2
        WHEN sortData.ReceivedSatisfactionRatio > 0.75 THEN 3
        WHEN sortData.SelectedDueDate IS NULL THEN 1
        ELSE 0
    END,
    sortData.SelectedDueDate,
    pol.LINE_NO;

-- Expected Results:
-- - PoNumber: PO number
-- - PoLine: Line number on PO
-- - PartNumber: Part ID
-- - PartDescription: Part description
-- - OrderedQty: Quantity ordered
-- - ReceivedQty: Quantity already received
-- - RemainingQty: Calculated remaining quantity (ordered - received)
-- - UnitOfMeasure: Unit of measure (e.g., EA, LB, FT)
-- - DueDate: Selected expected receive date (line desired receive date, then promise date, then PO header fallback)
-- - VendorCode: Vendor ID
-- - VendorName: Vendor name
-- - PoStatus: PO status code
-- - HeaderPromiseDate: Header-level promised delivery date
-- - HeaderDesiredRecvDate: Header-level desired receive date
-- - FreeOnBoard: Header-level FOB/free-on-board text
-- - SiteId: Site/warehouse ID (should be '002')
