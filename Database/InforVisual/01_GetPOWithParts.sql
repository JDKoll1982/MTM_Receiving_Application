-- ========================================
-- Query: Get PO with Parts
-- Description: Retrieves purchase order details with all associated parts
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- Site: 002
-- ========================================

-- USAGE: Replace @PoNumber with actual PO number (e.g., '123456')
DECLARE @PoNumber VARCHAR(20) = '123456';  -- TEST VALUE - Replace with actual PO number

SELECT 
    po.po_num AS PoNumber,
    pol.po_line AS PoLine,
    pol.part AS PartNumber,
    p.description AS PartDescription,
    pol.qty_ordered AS OrderedQty,
    pol.qty_received AS ReceivedQty,
    (pol.qty_ordered - pol.qty_received) AS RemainingQty,
    pol.u_m AS UnitOfMeasure,
    pol.due_date AS DueDate,
    po.vend_id AS VendorCode,
    v.name AS VendorName,
    po.stat AS PoStatus,
    po.site_id AS SiteId
FROM po
INNER JOIN po_line pol ON po.po_num = pol.po_num
INNER JOIN part p ON pol.part = p.part_id
LEFT JOIN vendor v ON po.vend_id = v.vend_id
WHERE po.po_num = @PoNumber
AND po.site_id = '002'
ORDER BY pol.po_line;

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
