DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_VolvoHistory_GetByDateRange`$$

CREATE PROCEDURE `sp_Reporting_VolvoHistory_GetByDateRange`(
    IN p_start_date DATE,
    IN p_end_date DATE
)
BEGIN
    SELECT
        id,
        shipment_number,
        shipment_date,
        po_number,
        receiver_number,
        status,
        employee_number,
        notes,
        part_count,
        created_date,
        source_module
    FROM view_volvo_history
    WHERE created_date BETWEEN p_start_date AND p_end_date
    ORDER BY created_date DESC;
END$$

DELIMITER ;