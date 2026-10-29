DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_ReceivingAnalyticsApp_GetCoverageLines`$$

-- One row per received date and part, counting the purchase-order lines the app completed.
--
-- The part is carried as part_id, not the stored part_type: part_type is blank on nearly every
-- receipt row, so classifying from it would put almost everything in one category. The category is
-- derived from the part number by the one shared translation in C#, exactly as the app classifies
-- material everywhere else.
-- Infor Visual counts a PO line once, on its LAST_RECEIVED_DATE, so a like-for-like comparison has
-- to anchor the app the same way: each PO line is attributed to its own final receipt date and is
-- skipped when that date falls outside the range. Counting every label instead would report several
-- times the ERP's figure, because one PO line commonly arrives as many labelled packages.
--
-- The inner aggregate carries no date bound on purpose: a line's final receipt may sit after the
-- range, and that line must then fall outside it, exactly as Infor Visual's LAST_RECEIVED_DATE
-- behaves. Non-PO receipts carry no PO line, so they are absent here; Infor Visual has no
-- counterpart for them either.
CREATE PROCEDURE `sp_Reporting_ReceivingAnalyticsApp_GetCoverageLines`(
    IN p_start_date DATETIME,
    IN p_end_date DATETIME
)
BEGIN
    SELECT
        per_line.last_received_date AS last_received_date,
        per_line.part_id AS part_id,
        COUNT(*) AS line_count
    FROM (
        SELECT
            po_number AS po_number,
            po_line_number AS po_line_number,
            MAX(DATE(COALESCE(received_date, transaction_date))) AS last_received_date,
            MAX(part_id) AS part_id
        FROM receiving_history
        WHERE
            po_number IS NOT NULL
            AND TRIM(po_number) <> ''
            -- DAYOFWEEK: 1 = Sunday, 7 = Saturday. The dock does not read weekend work.
            AND DAYOFWEEK(COALESCE(received_date, transaction_date)) NOT IN (1, 7)
        GROUP BY po_number, po_line_number
    ) AS per_line
    WHERE
        per_line.last_received_date >= DATE(p_start_date)
        AND per_line.last_received_date < DATE(p_end_date)
    GROUP BY per_line.last_received_date, per_line.part_id
    ORDER BY per_line.last_received_date ASC, per_line.part_id ASC;
END $$

DELIMITER ;
