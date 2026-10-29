DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay`$$

CREATE PROCEDURE `sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay`(
    IN p_start_date DATETIME,
    IN p_end_date DATETIME
)
BEGIN
    SELECT
        DATE(COALESCE(rh.received_date, rh.transaction_date)) AS bucket_date,
        rh.part_type AS part_type,
        rh.unit_of_measure AS unit_of_measure,
        day_totals.load_count AS load_count,
        day_totals.label_count AS label_count,
        SUM(COALESCE(rh.weight_quantity, rh.quantity)) AS quantity_total,
        COUNT(*) AS receipt_count
    FROM receiving_history rh
    INNER JOIN (
        SELECT
            DATE(COALESCE(received_date, transaction_date)) AS bucket_date,
            COUNT(DISTINCT COALESCE(load_id, load_guid, CAST(id AS CHAR))) AS load_count,
            COUNT(*) AS label_count
        FROM receiving_history
        WHERE
            (
                (received_date IS NOT NULL AND received_date >= p_start_date AND received_date < p_end_date)
                OR
                (received_date IS NULL AND transaction_date >= p_start_date AND transaction_date < p_end_date)
            )
        GROUP BY DATE(COALESCE(received_date, transaction_date))
    ) AS day_totals
        ON day_totals.bucket_date = DATE(COALESCE(rh.received_date, rh.transaction_date))
    WHERE
        (
            (rh.received_date IS NOT NULL AND rh.received_date >= p_start_date AND rh.received_date < p_end_date)
            OR
            (rh.received_date IS NULL AND rh.transaction_date >= p_start_date AND rh.transaction_date < p_end_date)
        )
    GROUP BY
        DATE(COALESCE(rh.received_date, rh.transaction_date)),
        rh.part_type,
        rh.unit_of_measure,
        day_totals.load_count,
        day_totals.label_count
    ORDER BY bucket_date ASC, rh.part_type ASC, rh.unit_of_measure ASC;
END $$

DELIMITER ;
