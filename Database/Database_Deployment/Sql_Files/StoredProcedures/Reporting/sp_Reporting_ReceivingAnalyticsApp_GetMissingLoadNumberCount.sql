DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount`$$

CREATE PROCEDURE `sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount`(
    IN p_start_date DATETIME,
    IN p_end_date DATETIME
)
BEGIN
    SELECT
        COUNT(*) AS missing_load_number_count
    FROM receiving_history
    WHERE
        (
            (received_date IS NOT NULL AND received_date >= p_start_date AND received_date < p_end_date)
            OR
            (received_date IS NULL AND transaction_date >= p_start_date AND transaction_date < p_end_date)
        )
        AND load_number IS NULL;
END $$

DELIMITER ;
