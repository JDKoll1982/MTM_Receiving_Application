-- Stored Procedure: sp_Receiving_LabelData_GetAll
-- Description: Retrieves all rows from receiving_label_data (the active print queue).
--              Used by Edit Mode "Current Labels" to load today's pending labels from the
--              database instead of from XLS files.
--              Returns actual snake_case column names from receiving_label_data.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_LabelData_GetAll`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_LabelData_GetAll`()
BEGIN
    SELECT
        load_id,
        part_id,
        part_description,
        part_type,
        po_number,
        po_line_number,
        po_vendor,
        po_status,
        po_due_date,
        qty_ordered,
        unit_of_measure,
        remaining_quantity,
        load_number,
        weight_quantity,
        heat,
        initial_location,
        packages_per_load,
        package_type_name,
        weight_per_package,
        is_non_po_item,
        received_date,
        created_at,
        user_id,
        employee_number,
        is_quality_hold_required,
        is_quality_hold_acknowledged,
        quality_hold_restriction_type
    FROM receiving_label_data
    ORDER BY load_number ASC;
END $$

DELIMITER ;
