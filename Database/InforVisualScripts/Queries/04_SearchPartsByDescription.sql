-- ========================================
-- Query: Search Parts by Description
-- Description: Searches for parts matching a description pattern
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- Site: 002
-- ========================================

USE [MTMFG];
GO
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
GO

-- USAGE: Replace @SearchTerm with search text and @MaxResults with max records to return
DECLARE @SearchTerm VARCHAR(100) = 'BOLT';  -- TEST VALUE - Replace with search term
DECLARE @MaxResults INT = 50;  -- Maximum number of results to return

-- NOTE: Using base tables (PART, PART_SITE) instead of views (part, inventory)
SELECT TOP (@MaxResults)
    p.ID AS PartNumber,
    p.DESCRIPTION AS Description,
    ps.UNIT_MATERIAL_COST AS UnitCost,
    p.STOCK_UM AS PrimaryUom,
    COALESCE(ps.QTY_ON_HAND, 0) AS OnHandQty,
    COALESCE(ps.QTY_COMMITTED, 0) AS AllocatedQty,
    (COALESCE(ps.QTY_ON_HAND, 0) - COALESCE(ps.QTY_COMMITTED, 0)) AS AvailableQty,
    ps.SITE_ID AS DefaultSite,
    ps.STATUS AS PartStatus,
    p.PRODUCT_CODE AS ProductLine
FROM dbo.PART p
LEFT JOIN dbo.PART_SITE ps ON p.ID = ps.PART_ID -- AND ps.SITE_ID = '002'
WHERE p.DESCRIPTION LIKE @SearchTerm + '%'
ORDER BY p.ID;

-- Expected Results:
-- - Returns up to @MaxResults parts where description starts with @SearchTerm
-- - PartNumber: Part ID
-- - Description: Part description
-- - PartType: Type of part (e.g., FG, RM, WIP)
-- - UnitCost: Cost per unit
-- - PrimaryUom: Primary unit of measure
-- - OnHandQty: Current quantity on hand
-- - AllocatedQty: Quantity allocated
-- - AvailableQty: Available quantity (on_hand - allocated)
-- - DefaultSite: Default site for this part
-- - PartStatus: Part status
-- - ProductLine: Product line
