DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_ReceivingHistory_GetByDateRange`$$

CREATE PROCEDURE `sp_Reporting_ReceivingHistory_GetByDateRange`(
    IN p_start_date DATETIME,
    IN p_end_date DATETIME
)
BEGIN
    SELECT
        id,
        po_number,
        po_line_number,
        part_id,
        part_description,
        quantity,
        weight_lbs,
        heat,
        transaction_date,
        created_at,
        employee_number,
        user_id,
        source_module,
        initial_location,
        notes,
        load_number,
        label_number,
        packages_per_load,
        package_type_name,
        weight_per_package,
        coils_on_skid,
        vendor_name,
        po_status,
        po_due_date,
        qty_ordered,
        unit_of_measure,
        remaining_quantity,
        is_non_po_item,
        is_quality_hold_required,
        is_quality_hold_acknowledged,
        quality_hold_restriction_type,
        part_skid_total,
        NULL AS quantity_per_skid,
        NULL AS received_skid_count
    FROM view_receiving_history
        WHERE created_at >= p_start_date
            AND created_at < p_end_date
    ORDER BY created_at DESC, id DESC;
END $$

DELIMITER ;