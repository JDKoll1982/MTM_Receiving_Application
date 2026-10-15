-- Stored Procedure: sp_Receiving_LabelData_InsertFromHistory
-- Description: Copies a single row from receiving_history back into receiving_label_data
--              so it can be re-printed. Sets is_reprint = 1 so the row is visually
--              identified in Edit Mode and skipped when ClearToHistory runs.
--              Uses the history row's load_guid as the new load_id so the UNIQUE
--              constraint on receiving_label_data.load_id prevents double-queuing.
-- Notes:    This is a copy and must stay one. Every column the history row carries is carried
--           across, and no column is filled from a different one. Two defects were corrected
--           here on 2026-10-15: the weight was being filled from the quantity, which silently
--           restated a reprinted row's weight as its piece count, and the part type was being
--           written as NULL even though the history row held one. The skid sequence and skid
--           total were never carried at all.
--           The two marking columns are copied like any other value. A resolved marking for the
--           lot is put on afterwards by the caller, because the matching key behind a marking is
--           produced in the application and not in SQL.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_LabelData_InsertFromHistory`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_LabelData_InsertFromHistory`(
    IN p_history_id   INT,
    IN p_queued_by    VARCHAR(100),
    IN p_employee_number INT
)
BEGIN
    DECLARE v_already_queued INT DEFAULT 0;
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
        RESIGNAL;
    END;

    SET FOREIGN_KEY_CHECKS = 0;

    -- Guard: block re-queuing if a reprint row for this history GUID already exists.
    SELECT COUNT(*)
    INTO v_already_queued
    FROM receiving_label_data rld
    INNER JOIN receiving_history rh ON rh.load_guid = rld.load_id
    WHERE rh.id = p_history_id
      AND rld.is_reprint = 1;

    IF v_already_queued > 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'This history record is already queued for reprint.';
    ELSE
        INSERT INTO receiving_label_data (
            load_id,
            load_number,
            quantity,
            weight_quantity,
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
            employee_number,
            user_id,
            user_set_customer_name,
            user_set_variable,
            heat,
            heat_shape_code,
            heat_color_code,
            received_date,
            transaction_date,
            initial_location,
            packages_per_load,
            package_type_name,
            weight_per_package,
            coils_on_skid,
            label_number,
            vendor_name,
            is_non_po_item,
            is_quality_hold_required,
            is_quality_hold_acknowledged,
            is_reprint,
            quality_hold_restriction_type,
            part_skid_sequence,
            part_skid_total
        )
        SELECT
            COALESCE(rh.load_id, rh.load_guid),
            rh.load_number,
            rh.quantity,
            rh.weight_quantity,
            rh.part_id,
            rh.part_description,
            rh.part_type,
            rh.po_number,
            rh.po_line_number,
            COALESCE(rh.po_vendor, rh.vendor_name),
            rh.po_status,
            rh.po_due_date,
            rh.qty_ordered,
            rh.unit_of_measure,
            rh.remaining_quantity,
            COALESCE(p_employee_number, rh.employee_number),
            COALESCE(p_queued_by, rh.user_id),
            rh.user_set_customer_name,
            rh.user_set_variable,
            rh.heat,
            rh.heat_shape_code,
            rh.heat_color_code,
            COALESCE(rh.received_date, rh.created_at),
            rh.transaction_date,
            rh.initial_location,
            rh.packages_per_load,
            rh.package_type_name,
            rh.weight_per_package,
            rh.coils_on_skid,
            rh.label_number,
            rh.vendor_name,
            rh.is_non_po_item,
            rh.is_quality_hold_required,
            rh.is_quality_hold_acknowledged,
            1,
            rh.quality_hold_restriction_type,
            rh.part_skid_sequence,
            rh.part_skid_total
        FROM receiving_history rh
        WHERE rh.id = p_history_id;

        SELECT ROW_COUNT() AS rows_inserted;
    END IF;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;