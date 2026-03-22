DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_VolvoHistory_GetByDateRange`$$

CREATE PROCEDURE `sp_Reporting_VolvoHistory_GetByDateRange`(
    IN p_start_date DATE,
    IN p_end_date DATE
)
BEGIN
    SELECT
        id,
        po_number,
        part_number,
        quantity,
        employee_number,
        notes,
        created_date,
        NULL AS created_by_username,
        source_module,
        location,
        NULL AS load_number,
        NULL AS label_number,
        NULL AS packages_per_load,
        NULL AS package_type_name,
        NULL AS coils_on_skid,
        quantity_per_skid,
        received_skid_count,
        shipment_number,
        receiver_number,
        status,
        part_count
    FROM view_volvo_history
    WHERE DATE(created_date) BETWEEN p_start_date AND p_end_date
    ORDER BY created_date DESC;
END $$

DELIMITER ;