-- ========================================
-- Query: Get Part by Part Number
-- Description: Retrieves detailed part information including inventory
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- Site: 002
-- ========================================

-- USAGE: Replace @PartNumber with actual part number (e.g., 'PART-001')
DECLARE @PartNumber VARCHAR(50) = 'PART-001';  -- TEST VALUE - Replace with actual part number

SELECT 
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
WHERE p.part_id = @PartNumber;

-- Expected Results:
-- - PartNumber: Part ID
-- - Description: Part description
-- - PartType: Type of part (e.g., FG, RM, WIP)
-- - UnitCost: Cost per unit
-- - PrimaryUom: Primary unit of measure (e.g., EA, LB, FT)
-- - OnHandQty: Current quantity on hand in inventory
-- - AllocatedQty: Quantity allocated to orders
-- - AvailableQty: Calculated available quantity (on_hand - allocated)
-- - DefaultSite: Default site for this part
-- - PartStatus: Part status (e.g., ACTIVE, OBSOLETE)
-- - ProductLine: Product line designation
