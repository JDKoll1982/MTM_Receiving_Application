DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_DunnageHistory_GetByDateRange`$$

CREATE PROCEDURE `sp_Reporting_DunnageHistory_GetByDateRange`(
    IN p_start_date DATETIME,
    IN p_end_date DATETIME
)
BEGIN
    SELECT
        id,
        po_number,
        dunnage_type,
        part_number,
        udc1,
        udc2,
        udc3,
        udc4,
        udc5,
        udc6,
        udc7,
        udc8,
        udc9,
        udc10,
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
        WHERE created_date >= p_start_date
            AND created_date < p_end_date
    ORDER BY created_date DESC;
END $$

DELIMITER ;