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
        -- load_guid uniquely identifies app-generated records; the
        -- ON DUPLICATE KEY clause prevents double-archiving.
        INSERT INTO receiving_history
        (
            load_guid,
            quantity,
            part_id,
            part_description,
            po_number,
            employee_number,
            heat,
            transaction_date,
            initial_location,
            coils_on_skid,
            label_number,
            vendor_name,
            is_non_po_item
        )
        SELECT
            COALESCE(rld.load_id, UUID())                              AS load_guid,
            rld.quantity                                               AS quantity,
            rld.part_id                                                AS part_id,
            rld.part_description                                       AS part_description,
            rld.po_number                                              AS po_number,
            rld.employee_number                                        AS employee_number,
            rld.heat                                                   AS heat,
            COALESCE(DATE(rld.received_date), rld.transaction_date)    AS transaction_date,
            rld.initial_location                                       AS initial_location,
            rld.coils_on_skid                                          AS coils_on_skid,
            COALESCE(rld.label_number, 1)                              AS label_number,
            COALESCE(rld.po_vendor, rld.vendor_name)                   AS vendor_name,
            COALESCE(rld.is_non_po_item, 0)                            AS is_non_po_item
        FROM receiving_label_data rld
        ON DUPLICATE KEY UPDATE
            quantity         = VALUES(quantity),
            part_id          = VALUES(part_id),
            part_description = VALUES(part_description),
            po_number        = VALUES(po_number),
            employee_number  = VALUES(employee_number),
            heat             = VALUES(heat),
            transaction_date = VALUES(transaction_date),
            initial_location = VALUES(initial_location),
            coils_on_skid    = VALUES(coils_on_skid),
            label_number     = VALUES(label_number),
            vendor_name      = VALUES(vendor_name),
            is_non_po_item   = VALUES(is_non_po_item);

        DELETE FROM receiving_label_data;

        COMMIT;

        SET p_rows_moved = v_rows_to_move;
        SET p_status = 0;
        SET p_error_message = NULL;
    END IF;
END$$

DELIMITER ;
