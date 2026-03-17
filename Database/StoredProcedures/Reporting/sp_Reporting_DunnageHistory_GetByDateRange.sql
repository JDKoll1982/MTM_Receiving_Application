DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_DunnageHistory_GetByDateRange`$$

CREATE PROCEDURE `sp_Reporting_DunnageHistory_GetByDateRange`(
    IN p_start_date DATE,
    IN p_end_date DATE
)
BEGIN
    SELECT
        id,
        dunnage_type,
        part_number,
        specs_combined,
        quantity,
        created_date,
        employee_number,
        created_by_username,
        source_module
    FROM view_dunnage_history
    WHERE created_date BETWEEN p_start_date AND p_end_date
    ORDER BY created_date DESC;
END$$

DELIMITER ;