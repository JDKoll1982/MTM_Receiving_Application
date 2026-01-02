-- ========================================
-- Query: Validate PO Number
-- Description: Checks if a PO number exists in the system
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- Site: 002
-- ========================================

-- NOTE: Using base table PURCHASE_ORDER instead of view po
SELECT COUNT(*) AS POExists
FROM dbo.PURCHASE_ORDER
WHERE ID = @PoNumber;

-- Expected Results:
-- - Returns 1 if PO exists for site 002
-- - Returns 0 if PO does not exist
