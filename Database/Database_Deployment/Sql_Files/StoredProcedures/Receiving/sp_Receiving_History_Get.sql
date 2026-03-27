-- ============================================================================
-- Procedure: sp_Receiving_History_Get
-- Purpose: Retrieve receiving history records filtered by part and date range.
-- Returns snake_case columns that match the canonical receiving_history schema.
-- All parameters are optional (pass NULL to skip that filter).
-- ============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS `sp_Receiving_History_Get` //

CREATE PROCEDURE `sp_Receiving_History_Get`(
    IN p_PartID    VARCHAR(50),
    IN p_StartDate DATE,
    IN p_EndDate   DATE
)
BEGIN
    SELECT
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
    WHERE
        (p_PartID    IS NULL OR part_id          = p_PartID)
        AND (p_StartDate IS NULL OR transaction_date >= p_StartDate)
        AND (p_EndDate   IS NULL OR transaction_date <= p_EndDate)
    ORDER BY transaction_date DESC, id DESC;
END //

DELIMITER ;
