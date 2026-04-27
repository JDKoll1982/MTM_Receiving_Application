DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_Availability_GetByDateRange`$$

CREATE PROCEDURE `sp_Reporting_Availability_GetByDateRange`(
    IN p_start_date DATETIME,
    IN p_end_date DATETIME
)
BEGIN
    SELECT
        (SELECT COUNT(*) FROM view_receiving_history WHERE created_at >= p_start_date AND created_at < p_end_date) AS receiving_count,
        (SELECT COUNT(*) FROM view_dunnage_history WHERE created_date >= p_start_date AND created_date < p_end_date) AS dunnage_count,
        (SELECT COUNT(*) FROM view_volvo_history WHERE created_date >= p_start_date AND created_date < p_end_date) AS volvo_count;
END $$

DELIMITER ;