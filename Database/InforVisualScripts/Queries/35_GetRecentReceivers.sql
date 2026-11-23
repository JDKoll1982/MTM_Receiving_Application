-- ========================================
-- Query: Get Recent Receivers
-- Description: Returns one row per receiver and part for the most
--   recently received inventory, with received, ordered, and remaining
--   quantities for the By Receiver list.
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- ========================================

-- Read-only Infor Visual (MTMFG) query. One bounded window, newest first.
-- Parameters expected from C#:
--   @MaxRows   INT   -- safety cap on returned rows (default 1000)

-- A receiver can hold several lines for one part, so the lines are ranked per receiver and
-- part and the first PO line found is that row's representative line. Everything the row shows
-- about the line (PO number, line number, vendor, warehouse, location, ordered and total
-- received) comes from that one line, while the quantity received is summed across all of them.
;WITH ReceiverLines AS
(
    SELECT
        r.ID AS ReceiverNumber,
        r.RECEIVED_DATE AS ReceivedDate,
        po.ID AS PONumber,
        pol.LINE_NO AS POLineNumber,
        pol.PART_ID AS PartNumber,
        COALESCE(p.DESCRIPTION, '') AS PartDescription,
        COALESCE(pol.PURCHASE_UM, 'EA') AS UnitOfMeasure,
        COALESCE(NULLIF(v.NAME, ''), po.VENDOR_ID, '') AS VendorName,
        rl.WAREHOUSE_ID AS Warehouse,
        ISNULL(rl.LOCATION_ID, '') AS LocationReceivedTo,
        rl.RECEIVED_QTY AS QtyReceived,
        pol.ORDER_QTY AS QtyOrdered,
        pol.TOTAL_RECEIVED_QTY AS TotalReceivedQty,
        ROW_NUMBER() OVER (
            PARTITION BY r.ID, pol.PART_ID
            ORDER BY pol.LINE_NO, rl.LINE_NO, rl.WAREHOUSE_ID, rl.LOCATION_ID
        ) AS RepresentativeRank
    FROM dbo.RECEIVER r
    INNER JOIN dbo.RECEIVER_LINE rl ON r.ID = rl.RECEIVER_ID
    INNER JOIN dbo.PURC_ORDER_LINE pol
        ON rl.PURC_ORDER_ID = pol.PURC_ORDER_ID
        AND rl.PURC_ORDER_LINE_NO = pol.LINE_NO
    LEFT JOIN dbo.PURCHASE_ORDER po ON po.ID = pol.PURC_ORDER_ID
    LEFT JOIN dbo.VENDOR v ON po.VENDOR_ID = v.ID
    LEFT JOIN dbo.PART p ON p.ID = pol.PART_ID
    WHERE r.RECEIVED_DATE IS NOT NULL
      AND pol.PART_ID IS NOT NULL AND LTRIM(RTRIM(pol.PART_ID)) <> ''
      AND rl.WAREHOUSE_ID IS NOT NULL AND LTRIM(RTRIM(rl.WAREHOUSE_ID)) <> ''
      AND ISNULL(r.MARKED_FOR_PURGE, '') <> 'Y'
),
GroupedReceivers AS
(
    SELECT
        ReceiverNumber,
        ReceivedDate,
        PartNumber,
        MAX(CASE WHEN RepresentativeRank = 1 THEN PONumber END) AS PONumber,
        MAX(CASE WHEN RepresentativeRank = 1 THEN POLineNumber END) AS POLineNumber,
        MAX(CASE WHEN RepresentativeRank = 1 THEN PartDescription END) AS PartDescription,
        MAX(CASE WHEN RepresentativeRank = 1 THEN UnitOfMeasure END) AS UnitOfMeasure,
        MAX(CASE WHEN RepresentativeRank = 1 THEN VendorName END) AS VendorName,
        MAX(CASE WHEN RepresentativeRank = 1 THEN Warehouse END) AS Warehouse,
        MAX(CASE WHEN RepresentativeRank = 1 THEN LocationReceivedTo END) AS LocationReceivedTo,
        SUM(QtyReceived) AS QtyReceived,
        MAX(CASE WHEN RepresentativeRank = 1 THEN QtyOrdered END) AS QtyOrdered,
        MAX(CASE WHEN RepresentativeRank = 1 THEN TotalReceivedQty END) AS TotalReceivedQty
    FROM ReceiverLines
    GROUP BY ReceiverNumber, ReceivedDate, PartNumber
),
RankedReceivers AS
(
    SELECT
        g.ReceiverNumber,
        g.ReceivedDate,
        g.PONumber,
        g.POLineNumber,
        g.PartNumber,
        g.PartDescription,
        g.UnitOfMeasure,
        g.VendorName,
        g.Warehouse,
        g.LocationReceivedTo,
        g.QtyReceived,
        g.QtyOrdered,
        CASE
            WHEN g.QtyOrdered - g.TotalReceivedQty < 0 THEN CAST(0 AS decimal(38, 8))
            ELSE g.QtyOrdered - g.TotalReceivedQty
        END AS QtyRemaining,
        ROW_NUMBER() OVER (ORDER BY g.ReceivedDate DESC, g.ReceiverNumber DESC) AS RowRank
    FROM GroupedReceivers g
)
SELECT TOP (@MaxRows)
    ReceiverNumber,
    ReceivedDate,
    PONumber,
    POLineNumber,
    PartNumber,
    PartDescription,
    UnitOfMeasure,
    VendorName,
    Warehouse,
    LocationReceivedTo,
    QtyReceived,
    QtyOrdered,
    QtyRemaining
FROM RankedReceivers
ORDER BY RowRank;

-- Expected Results:
-- - ReceiverNumber: Receiver identifier shown in column one and matched by search
-- - ReceivedDate: Date the receiver was received (ordering key)
-- - PONumber: Purchase order the representative line belongs to
-- - POLineNumber: First PO line found for the part within the receiver
-- - PartNumber: Part the row receives
-- - PartDescription: Description of that part, blank when the part master has none
-- - UnitOfMeasure: Purchase unit of measure of the representative line
-- - VendorName: Vendor name, falling back to the vendor identifier
-- - Warehouse: Receiving line's warehouse
-- - LocationReceivedTo: Receiving line's location (may be blank)
-- - QtyReceived: Summed receipt quantity across the receiver's lines for this part
-- - QtyOrdered: Ordered quantity on the representative PO line
-- - QtyRemaining: Ordered minus total received on that line, never negative
