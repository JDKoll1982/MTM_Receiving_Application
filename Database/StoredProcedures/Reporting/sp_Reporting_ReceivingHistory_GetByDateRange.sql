DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_ReceivingHistory_GetByDateRange`$$

CREATE PROCEDURE `sp_Reporting_ReceivingHistory_GetByDateRange`(
    IN p_start_date DATE,
    IN p_end_date DATE
)
BEGIN
    SELECT
        id,
        po_number,
        part_number,
        part_description,
        quantity,
        weight_lbs,
        heat_lot_number,
        created_date,
        employee_number,
        NULL AS created_by_username,
        source_module
    FROM view_receiving_history
    WHERE created_date BETWEEN p_start_date AND p_end_date
    ORDER BY created_date DESC, id DESC;
END$$

DELIMITER ;