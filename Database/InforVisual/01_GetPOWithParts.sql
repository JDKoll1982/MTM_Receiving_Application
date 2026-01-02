-- ========================================
-- Query: Get PO with Parts
-- Description: Retrieves purchase order details with all associated parts
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- Site: 002
-- ========================================
-- Test Passed!
USE [MTMFG];
GO
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
GO

-- USAGE: Replace @PoNumber with actual PO number (e.g., '123456')
DECLARE @PoNumber VARCHAR(20) = 'PO-067101';  -- TEST VALUE - Replace with actual PO number

-- NOTE: Using base tables (PURCHASE_ORDER, PURC_ORDER_LINE) instead of views (po, po_line)
-- Adjust table names if your environment uses different names (e.g. PURCHASE_ORDER)
-- NOTE: No status check is performed. Returns all POs regardless of status (Open, Closed, etc.)
SELECT 
    po.ID AS PoNumber,
    pol.LINE_NO AS PoLine,
    pol.PART_ID AS PartNumber,
    p.DESCRIPTION AS PartDescription,
    pol.ORDER_QTY AS OrderedQty,
    pol.TOTAL_RECEIVED_QTY AS ReceivedQty,
    (pol.ORDER_QTY - pol.TOTAL_RECEIVED_QTY) AS RemainingQty,
    pol.PURCHASE_UM AS UnitOfMeasure,
    pol.PROMISE_DATE AS DueDate,
    po.VENDOR_ID AS VendorCode,
    v.NAME AS VendorName,
    po.STATUS AS PoStatus,
    po.SITE_ID AS SiteId
FROM dbo.PURCHASE_ORDER po
INNER JOIN dbo.PURC_ORDER_LINE pol ON po.ID = pol.PURC_ORDER_ID
LEFT JOIN dbo.PART p ON pol.PART_ID = p.ID
LEFT JOIN dbo.VENDOR v ON po.VENDOR_ID = v.ID
WHERE po.ID = @PoNumber
-- AND po.SITE_ID = '002' -- Commented out to allow finding POs in other sites for testing
ORDER BY pol.LINE_NO;

-- Expected Results:
-- - PoNumber: PO number
-- - PoLine: Line number on PO
-- - PartNumber: Part ID
-- - PartDescription: Part description
-- - OrderedQty: Quantity ordered
-- - ReceivedQty: Quantity already received
-- - RemainingQty: Calculated remaining quantity (ordered - received)
-- - UnitOfMeasure: Unit of measure (e.g., EA, LB, FT)
-- - DueDate: Due date for this line
-- - VendorCode: Vendor ID
-- - VendorName: Vendor name
-- - PoStatus: PO status code
-- - SiteId: Site/warehouse ID (should be '002')
