DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay`$$

CREATE PROCEDURE `sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay`(
    IN p_start_date DATETIME,
    IN p_end_date DATETIME
)
BEGIN
    SELECT
        DATE(COALESCE(received_date, transaction_date)) AS bucket_date,
        CASE
            WHEN po_due_date IS NULL THEN 'NoDueDate'
            WHEN DATE(COALESCE(received_date, transaction_date)) < po_due_date THEN 'Early'
            WHEN DATE(COALESCE(received_date, transaction_date)) = po_due_date THEN 'OnTime'
            ELSE 'Late'
        END AS status,
        COUNT(*) AS receipt_count
    FROM receiving_history
    WHERE
        (
            (received_date IS NOT NULL AND received_date >= p_start_date AND received_date < p_end_date)
            OR
            (received_date IS NULL AND transaction_date >= p_start_date AND transaction_date < p_end_date)
        )
        -- DAYOFWEEK: 1 = Sunday, 7 = Saturday. The dock does not read weekend work.
        AND DAYOFWEEK(COALESCE(received_date, transaction_date)) NOT IN (1, 7)
    GROUP BY
        DATE(COALESCE(received_date, transaction_date)),
        CASE
            WHEN po_due_date IS NULL THEN 'NoDueDate'
            WHEN DATE(COALESCE(received_date, transaction_date)) < po_due_date THEN 'Early'
            WHEN DATE(COALESCE(received_date, transaction_date)) = po_due_date THEN 'OnTime'
            ELSE 'Late'
        END
    ORDER BY
        bucket_date ASC,
        FIELD(status, 'Early', 'OnTime', 'Late', 'NoDueDate');
END $$

DELIMITER ;
