-- ========================================
-- Query: Search Parts by Description
-- Description: Searches for parts matching a description pattern
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- Site: 002
-- ========================================

-- USAGE: Replace @SearchTerm with search text and @MaxResults with max records to return
DECLARE @SearchTerm VARCHAR(100) = 'BOLT';  -- TEST VALUE - Replace with search term
DECLARE @MaxResults INT = 50;  -- Maximum number of results to return

SELECT TOP (@MaxResults)
    p.part_id AS PartNumber,
    p.description AS Description,
    p.part_type AS PartType,
    p.unit_cost AS UnitCost,
    p.u_m AS PrimaryUom,
    COALESCE(inv.on_hand, 0) AS OnHandQty,
    COALESCE(inv.allocated, 0) AS AllocatedQty,
    (COALESCE(inv.on_hand, 0) - COALESCE(inv.allocated, 0)) AS AvailableQty,
    p.site_id AS DefaultSite,
    p.stat AS PartStatus,
    p.prod_line AS ProductLine
FROM part p
LEFT JOIN inventory inv ON p.part_id = inv.part_id AND inv.site_id = '002'
WHERE p.description LIKE @SearchTerm + '%'
ORDER BY p.part_id;

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
