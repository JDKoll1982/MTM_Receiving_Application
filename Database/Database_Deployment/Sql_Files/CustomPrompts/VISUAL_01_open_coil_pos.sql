-- =============================================================================
-- VISUAL_01: Open Coil POs with remaining quantity
-- Server  : VISUAL
-- Database: MTMFG
-- User    : JKOLL  (READ ONLY — never INSERT/UPDATE/DELETE)
-- Run in  : SSMS or any SQL Server client
-- Purpose : Lists every open PO line for MMC coil parts showing what has been
--           ordered, what has been received in Visual, and what is still open.
--           Use this to plan incoming coil deliveries and dock space.
-- =============================================================================
USE MTMFG;

SELECT
    po.ID                               AS po_number,
    po.VENDOR_ID                        AS vendor_id,
    v.NAME                              AS vendor_name,
    pol.LINE_NO                         AS po_line,
    pol.PART_ID                         AS part_number,
    p.DESCRIPTION                       AS description,
    p.STOCK_UM                          AS unit_of_measure,
    pol.USER_ORDER_QTY                  AS qty_ordered,
    pol.TOTAL_USR_RECD_QTY              AS qty_received_visual,
    pol.USER_ORDER_QTY
        - pol.TOTAL_USR_RECD_QTY        AS qty_remaining,
    pol.UNIT_PRICE                      AS unit_price,
    pol.DESIRED_RECV_DATE               AS desired_recv_date,
    pol.PROMISE_DATE                    AS promise_date,
    pol.LAST_RECEIVED_DATE              AS last_received_date,
    pol.LINE_STATUS                     AS line_status,
    po.STATUS                           AS po_status,
    po.ORDER_DATE                       AS order_date
FROM PURCHASE_ORDER  po
JOIN PURC_ORDER_LINE pol ON pol.PURC_ORDER_ID = po.ID
JOIN VENDOR          v   ON v.ID              = po.VENDOR_ID
LEFT JOIN PART       p   ON p.ID              = pol.PART_ID
WHERE pol.PART_ID LIKE 'MMC%'
  AND pol.LINE_STATUS NOT IN ('C', 'X')   -- exclude Closed / Cancelled lines
  AND po.STATUS       NOT IN ('C', 'X')   -- exclude Closed / Cancelled POs
  AND pol.USER_ORDER_QTY - pol.TOTAL_USR_RECD_QTY > 0
ORDER BY po.VENDOR_ID, pol.DESIRED_RECV_DATE, pol.PART_ID;
