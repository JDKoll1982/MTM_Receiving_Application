-- =============================================================================
-- VISUAL_02: Coil PO receipt reconciliation (Visual side)
-- Server  : VISUAL
-- Database: MTMFG
-- User    : JKOLL  (READ ONLY — never INSERT/UPDATE/DELETE)
-- Run in  : SSMS or any SQL Server client
-- Purpose : For every MMC coil PO line shows ordered qty, what Visual recorded
--           as received, and the delta. Pair with APP_02 (MySQL) to detect gaps
--           between what Visual recorded and what the receiving app recorded.
-- =============================================================================
USE MTMFG;

SELECT
    po.ID                               AS po_number,
    po.VENDOR_ID                        AS vendor_id,
    v.NAME                              AS vendor_name,
    pol.LINE_NO                         AS po_line,
    pol.PART_ID                         AS part_number,
    p.DESCRIPTION                       AS description,
    pol.USER_ORDER_QTY                  AS qty_ordered,
    pol.TOTAL_USR_RECD_QTY              AS qty_received_visual,
    pol.USER_ORDER_QTY
        - pol.TOTAL_USR_RECD_QTY        AS qty_still_open,
    CASE
        WHEN pol.USER_ORDER_QTY <= 0 THEN 'N/A'
        ELSE CAST(
            ROUND(pol.TOTAL_USR_RECD_QTY / pol.USER_ORDER_QTY * 100.0, 1)
            AS nvarchar(10)) + '%'
    END                                 AS pct_received,
    pol.LAST_RECEIVED_DATE              AS last_received_date,
    pol.LINE_STATUS                     AS line_status,
    po.ORDER_DATE                       AS order_date,
    po.STATUS                           AS po_status
FROM PURCHASE_ORDER  po
JOIN PURC_ORDER_LINE pol ON pol.PURC_ORDER_ID = po.ID
JOIN VENDOR          v   ON v.ID              = po.VENDOR_ID
LEFT JOIN PART       p   ON p.ID              = pol.PART_ID
WHERE pol.PART_ID LIKE 'MMC%'
ORDER BY po.ID, pol.LINE_NO;
