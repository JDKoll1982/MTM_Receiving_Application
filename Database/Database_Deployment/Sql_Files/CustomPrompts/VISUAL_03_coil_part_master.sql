-- =============================================================================
-- VISUAL_03: Coil part master — on-hand, on-order, vendor, weight
-- Server  : VISUAL
-- Database: MTMFG
-- User    : JKOLL  (READ ONLY — never INSERT/UPDATE/DELETE)
-- Run in  : SSMS or any SQL Server client
-- Purpose : Shows the current state of each MMC coil part in Visual's inventory:
--           qty on hand, qty on order, preferred vendor, unit weight, and whether
--           the part is still active (STOCKED = 'Y'). Useful for warehouse
--           slotting decisions and cycle-count planning.
-- =============================================================================
USE MTMFG;

SELECT
    p.ID                                AS part_number,
    p.DESCRIPTION                       AS description,
    p.STOCK_UM                          AS unit_of_measure,
    p.QTY_ON_HAND                       AS qty_on_hand,
    p.QTY_ON_ORDER                      AS qty_on_order,
    p.QTY_AVAILABLE_ISS                 AS qty_available,
    p.QTY_IN_DEMAND                     AS qty_in_demand,
    p.WEIGHT                            AS weight_per_unit,
    p.WEIGHT_UM                         AS weight_um,
    CAST(ROUND(p.QTY_ON_HAND * ISNULL(p.WEIGHT, 0), 2) AS decimal(18,2))
                                        AS total_weight_on_hand,
    p.PREF_VENDOR_ID                    AS pref_vendor_id,
    v.NAME                              AS pref_vendor_name,
    p.STOCKED                           AS stocked,
    p.PURCHASED                         AS purchased,
    p.ABC_CODE                          AS abc_code,
    p.BUYER_USER_ID                     AS buyer
FROM PART   p
LEFT JOIN VENDOR v ON v.ID = p.PREF_VENDOR_ID
WHERE p.ID LIKE 'MMC%'
ORDER BY p.QTY_ON_HAND DESC, p.ID;
