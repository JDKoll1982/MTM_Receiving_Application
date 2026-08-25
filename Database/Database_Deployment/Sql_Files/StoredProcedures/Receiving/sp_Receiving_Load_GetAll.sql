-- Stored Procedure: sp_Receiving_Load_GetAll
-- Description: Retrieves all receiving loads within a date range for Edit Mode.
--              Returns actual snake_case columns from receiving_history.
--              Columns with no counterpart in receiving_history return NULL.
-- Parameters:
--   p_StartDate - Start date for retrieval (DATE)
--   p_EndDate   - End date for retrieval   (DATE)

USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Receiving_Load_GetAll`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_Load_GetAll`(
    IN p_StartDate DATE,
    IN p_EndDate   DATE
)
BEGIN
    SELECT
        id,
        load_guid,
        part_id,
        part_description,
        NULL AS part_type,
        po_number,
        po_line_number,
        vendor_name,
        po_status,
        po_due_date,
        qty_ordered,
        unit_of_measure,
        remaining_quantity,
        load_number,
        quantity,
        heat,
        initial_location,
        packages_per_load,
        package_type_name,
        weight_per_package,
        is_non_po_item,
        created_at,
        transaction_date,
        user_id,
        employee_number,
        is_quality_hold_required,
        is_quality_hold_acknowledged,
        quality_hold_restriction_type
    FROM receiving_history
    WHERE transaction_date >= p_StartDate
      AND transaction_date <= p_EndDate
      AND part_id IS NOT NULL
      AND part_id != ''
    ORDER BY transaction_date DESC, label_number ASC;
END $$

DELIMITER ;
