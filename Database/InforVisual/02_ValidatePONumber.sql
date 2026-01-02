-- ========================================
-- Query: Validate PO Number
-- Description: Checks if a PO number exists in the system
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- Site: 002
-- ========================================

-- USAGE: Replace @PoNumber with actual PO number (e.g., '123456')
DECLARE @PoNumber VARCHAR(20) = '123456';  -- TEST VALUE - Replace with actual PO number

SELECT COUNT(*) AS POExists
FROM po
WHERE po_num = @PoNumber
AND site_id = '002';

-- Expected Results:
-- - Returns 1 if PO exists for site 002
-- - Returns 0 if PO does not exist
