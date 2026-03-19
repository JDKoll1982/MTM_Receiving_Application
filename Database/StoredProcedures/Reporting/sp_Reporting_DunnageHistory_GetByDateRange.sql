DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_DunnageHistory_GetByDateRange`$$

CREATE PROCEDURE `sp_Reporting_DunnageHistory_GetByDateRange`(
    IN p_start_date DATE,
    IN p_end_date DATE
)
BEGIN
    SELECT
        id,
        po_number,
        dunnage_type,
        part_number,
        quantity,
        created_date,
        employee_number,
        created_by_username,
        source_module,
        location,
        notes,
        NULL AS load_number,
        NULL AS label_number,
        NULL AS packages_per_load,
        NULL AS package_type_name,
        NULL AS coils_on_skid,
        NULL AS quantity_per_skid,
        NULL AS received_skid_count
    FROM view_dunnage_history
    WHERE created_date BETWEEN p_start_date AND p_end_date
    ORDER BY created_date DESC;
END$$

DELIMITER ;