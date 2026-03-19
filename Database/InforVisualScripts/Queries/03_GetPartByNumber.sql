-- ========================================
-- Query: Get Part by Part Number
-- Description: Retrieves detailed part information including inventory
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- Site: 002
-- ========================================

-- NOTE: Using base tables (PART, PART_SITE) instead of views (part, inventory)
SELECT 
    p.ID AS PartNumber,
    p.DESCRIPTION AS Description,
    ps.UNIT_MATERIAL_COST AS UnitCost, -- Using Material Cost as proxy for Unit Cost
    p.STOCK_UM AS PrimaryUom,
    COALESCE(ps.QTY_ON_HAND, 0) AS OnHandQty,
    COALESCE(ps.QTY_COMMITTED, 0) AS AllocatedQty,
    (COALESCE(ps.QTY_ON_HAND, 0) - COALESCE(ps.QTY_COMMITTED, 0)) AS AvailableQty,
    ps.SITE_ID AS DefaultSite,
    COALESCE(ps.PRIMARY_LOC_ID, fallback.LocationId, '') AS DefaultLocationId,
    ps.STATUS AS PartStatus,
    p.PRODUCT_CODE AS ProductLine
FROM dbo.PART p
LEFT JOIN dbo.PART_SITE ps ON p.ID = ps.PART_ID AND ps.SITE_ID = '002'
OUTER APPLY
(
    SELECT TOP (1)
        cpl.LOCATION_ID AS LocationId
    FROM dbo.CR_PART_LOCATION cpl
    WHERE cpl.ID = p.ID
      AND cpl.WAREHOUSE_ID = '002'
      AND cpl.LOCATION_ID IS NOT NULL
      AND cpl.LOCATION_ID <> ''
    ORDER BY
        CASE WHEN cpl.DEF_INSPECT_LOC = 'Y' THEN 0 ELSE 1 END,
        CASE WHEN cpl.AUTO_ISSUE_LOC = 'Y' THEN 0 ELSE 1 END,
        cpl.LOCATION_ID
) fallback
WHERE p.ID = @PartNumber;

-- Expected Results:
-- - PartNumber: Part ID
-- - Description: Part description
-- - UnitCost: Cost per unit
-- - PrimaryUom: Primary unit of measure (e.g., EA, LB, FT)
-- - OnHandQty: Current quantity on hand in inventory
-- - AllocatedQty: Quantity allocated to orders
-- - AvailableQty: Calculated available quantity (on_hand - allocated)
-- - DefaultSite: Default site for this part
-- - PartStatus: Part status (e.g., ACTIVE, OBSOLETE)
-- - ProductLine: Product line designation
