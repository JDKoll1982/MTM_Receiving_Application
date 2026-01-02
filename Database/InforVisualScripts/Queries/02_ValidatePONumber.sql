-- ========================================
-- Query: Validate PO Number
-- Description: Checks if a PO number exists in the system
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- Site: 002
-- ========================================
-- Test Passed

USE [MTMFG];
GO
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
GO

-- USAGE: Replace @PoNumber with actual PO number (e.g., '123456')
DECLARE @PoNumber VARCHAR(20) = 'PO-067101';  -- TEST VALUE - Replace with actual PO number

-- NOTE: Using base table PURCHASE_ORDER instead of view po
SELECT COUNT(*) AS POExists
FROM dbo.PURCHASE_ORDER
WHERE ID = @PoNumber;

-- Expected Results:
-- - Returns 1 if PO exists for site 002
-- - Returns 0 if PO does not exist
