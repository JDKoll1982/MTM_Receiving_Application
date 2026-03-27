-- Stored Procedure: sp_Receiving_LabelData_ClearToHistory
-- Description: Atomically moves all rows from receiving_label_data (queue) to receiving_history (archive)
-- Lifecycle: Clear Label Data -> Queue to History + Queue Delete

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_LabelData_ClearToHistory`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_LabelData_ClearToHistory`(
    IN p_archived_by VARCHAR(100),
    OUT p_rows_moved INT,
    OUT p_archive_batch_id CHAR(36),
    OUT p_status INT,
    OUT p_error_message VARCHAR(1000)
)
BEGIN
    DECLARE v_rows_to_move INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_rows_moved = 0;
        SET p_status = 1;
        SET p_error_message = 'Clear Label Data failed. Transaction rolled back.';
    END;

    SET p_rows_moved = 0;
    SET p_status = 0;
    SET p_error_message = NULL;
    SET p_archive_batch_id = UUID();

    START TRANSACTION;

    SELECT COUNT(*) INTO v_rows_to_move
    FROM receiving_label_data;

    IF v_rows_to_move = 0 THEN
        COMMIT;
        SET p_rows_moved = 0;
        SET p_status = 0;
        SET p_error_message = NULL;
    ELSE
        -- Insert into receiving_history using snake_case column names
        -- that match the current receiving_history table schema.
        -- packages_per_load remains the package-count snapshot, while
        -- coils_on_skid preserves the coil-count field and is no longer
        -- repurposed to store packages_per_load during archival.
        -- load_guid uniquely identifies app-generated records; the
        -- ON DUPLICATE KEY clause prevents double-archiving.
        INSERT INTO receiving_history
        (
            load_guid,
            quantity,
            part_id,
            part_description,
            po_number,
            po_line_number,
            employee_number,
            heat,
            transaction_date,
            created_at,
            initial_location,
            load_number,
            packages_per_load,
            package_type_name,
            weight_per_package,
            coils_on_skid,
            label_number,
            vendor_name,
            po_status,
            po_due_date,
            qty_ordered,
            unit_of_measure,
            remaining_quantity,
            user_id,
            is_non_po_item,
            is_quality_hold_required,
            is_quality_hold_acknowledged,
            quality_hold_restriction_type,
            part_skid_sequence,
            part_skid_total
        )
        SELECT
            COALESCE(rld.load_id, UUID())                              AS load_guid,
            rld.quantity                                               AS quantity,
            rld.part_id                                                AS part_id,
            rld.part_description                                       AS part_description,
            rld.po_number                                              AS po_number,
            rld.po_line_number                                         AS po_line_number,
            rld.employee_number                                        AS employee_number,
            rld.heat                                                   AS heat,
            COALESCE(DATE(rld.received_date), rld.transaction_date)    AS transaction_date,
            COALESCE(rld.received_date, rld.created_at, CURRENT_TIMESTAMP) AS created_at,
            rld.initial_location                                       AS initial_location,
            rld.load_number                                            AS load_number,
            rld.packages_per_load                                      AS packages_per_load,
            rld.package_type_name                                      AS package_type_name,
            rld.weight_per_package                                     AS weight_per_package,
            rld.coils_on_skid                                          AS coils_on_skid,
            COALESCE(rld.label_number, 1)                              AS label_number,
            COALESCE(rld.vendor_name, rld.po_vendor)                   AS vendor_name,
            rld.po_status                                              AS po_status,
            rld.po_due_date                                            AS po_due_date,
            rld.qty_ordered                                            AS qty_ordered,
            COALESCE(NULLIF(rld.unit_of_measure, ''), 'EA')            AS unit_of_measure,
            rld.remaining_quantity                                     AS remaining_quantity,
            rld.user_id                                                AS user_id,
            COALESCE(rld.is_non_po_item, 0)                            AS is_non_po_item,
            COALESCE(rld.is_quality_hold_required, 0)                  AS is_quality_hold_required,
            COALESCE(rld.is_quality_hold_acknowledged, 0)              AS is_quality_hold_acknowledged,
            rld.quality_hold_restriction_type                          AS quality_hold_restriction_type,
            rld.part_skid_sequence                                     AS part_skid_sequence,
            rld.part_skid_total                                        AS part_skid_total
        FROM receiving_label_data rld
        ON DUPLICATE KEY UPDATE
            quantity         = VALUES(quantity),
            part_id          = VALUES(part_id),
            part_description = VALUES(part_description),
            po_number        = VALUES(po_number),
            po_line_number   = VALUES(po_line_number),
            employee_number  = VALUES(employee_number),
            heat             = VALUES(heat),
            transaction_date = VALUES(transaction_date),
            created_at       = VALUES(created_at),
            initial_location = VALUES(initial_location),
            load_number      = VALUES(load_number),
            packages_per_load = VALUES(packages_per_load),
            package_type_name = VALUES(package_type_name),
            weight_per_package = VALUES(weight_per_package),
            coils_on_skid    = VALUES(coils_on_skid),
            label_number     = VALUES(label_number),
            vendor_name      = VALUES(vendor_name),
            po_status        = VALUES(po_status),
            po_due_date      = VALUES(po_due_date),
            qty_ordered      = VALUES(qty_ordered),
            unit_of_measure  = VALUES(unit_of_measure),
            remaining_quantity = VALUES(remaining_quantity),
            user_id          = VALUES(user_id),
            is_non_po_item   = VALUES(is_non_po_item),
            is_quality_hold_required = VALUES(is_quality_hold_required),
            is_quality_hold_acknowledged = VALUES(is_quality_hold_acknowledged),
            quality_hold_restriction_type = VALUES(quality_hold_restriction_type),
            part_skid_sequence = VALUES(part_skid_sequence),
            part_skid_total  = VALUES(part_skid_total);

        DELETE FROM receiving_label_data;

        COMMIT;

        SET p_rows_moved = v_rows_to_move;
        SET p_status = 0;
        SET p_error_message = NULL;
    END IF;
END $$

DELIMITER ;
