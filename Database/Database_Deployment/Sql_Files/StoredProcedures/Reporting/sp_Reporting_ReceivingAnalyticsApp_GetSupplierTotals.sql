DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals`$$

CREATE PROCEDURE `sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals`(
    IN p_start_date DATETIME,
    IN p_end_date DATETIME
)
BEGIN
    SELECT
        COALESCE(NULLIF(TRIM(vendor_name), ''), 'Unknown') AS vendor_name,
        unit_of_measure AS unit_of_measure,
        COUNT(*) AS label_count,
        COUNT(DISTINCT COALESCE(load_id, load_guid, CAST(id AS CHAR))) AS load_count,
        SUM(COALESCE(weight_quantity, quantity)) AS quantity_total
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
        COALESCE(NULLIF(TRIM(vendor_name), ''), 'Unknown'),
        unit_of_measure
    ORDER BY vendor_name ASC, unit_of_measure ASC;
END $$

DELIMITER ;
