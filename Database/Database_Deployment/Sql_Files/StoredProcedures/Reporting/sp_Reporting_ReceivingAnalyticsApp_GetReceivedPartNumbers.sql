DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_ReceivingAnalyticsApp_GetReceivedPartNumbers`$$

-- The part numbers the app itself received in the range. The coverage comparison uses this as
-- its cohort, so Infor Visual is only counted for material the receiving app also handles.
-- Weekend receipts are excluded here for the same reason every other App Data figure excludes
-- them, so the cohort cannot include a part that only arrived on a Saturday or Sunday.
CREATE PROCEDURE `sp_Reporting_ReceivingAnalyticsApp_GetReceivedPartNumbers`(
    IN p_start_date DATETIME,
    IN p_end_date DATETIME
)
BEGIN
    SELECT DISTINCT
        part_id AS part_id
    FROM receiving_history
    WHERE
        (
            (received_date IS NOT NULL AND received_date >= p_start_date AND received_date < p_end_date)
            OR
            (received_date IS NULL AND transaction_date >= p_start_date AND transaction_date < p_end_date)
        )
        AND part_id IS NOT NULL
        AND TRIM(part_id) <> ''
        -- DAYOFWEEK: 1 = Sunday, 7 = Saturday. The dock does not read weekend work.
        AND DAYOFWEEK(COALESCE(received_date, transaction_date)) NOT IN (1, 7)
    ORDER BY part_id ASC;
END $$

DELIMITER ;
