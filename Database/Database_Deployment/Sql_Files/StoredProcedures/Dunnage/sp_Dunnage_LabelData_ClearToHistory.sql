-- Stored Procedure: sp_Dunnage_LabelData_ClearToHistory
-- Description: Atomically moves all rows from dunnage_label_data (active queue)
--              to dunnage_history (archive) and deletes them from the queue.
--              All rows moved in one call share the same archive_batch_id.
-- Lifecycle: Clear Label Data -> Queue to History + Queue Delete

USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Dunnage_LabelData_ClearToHistory`;

DELIMITER $$

CREATE PROCEDURE `sp_Dunnage_LabelData_ClearToHistory`(
    IN  p_archived_by       VARCHAR(100),
    IN  p_employee_number   INT,
    IN  p_clear_all         TINYINT(1),
    OUT p_rows_moved        INT,
    OUT p_archive_batch_id  CHAR(36),
    OUT p_status            INT,
    OUT p_error_message     VARCHAR(1000)
)
BEGIN
    DECLARE v_rows_to_move INT DEFAULT 0;
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;

    -- Roll back and surface the error if anything inside the transaction fails.
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
        SET p_rows_moved       = 0;
        SET p_status           = 1;
        SET p_error_message    = 'Clear Label Data failed. Transaction rolled back.';
    END;

    SET FOREIGN_KEY_CHECKS = 0;

    SET p_rows_moved      = 0;
    SET p_status          = 0;
    SET p_error_message   = NULL;
    SET p_archive_batch_id = UUID();

    IF COALESCE(p_clear_all, 0) = 0 AND COALESCE(p_employee_number, 0) <= 0 THEN
        SET p_status = 1;
        SET p_error_message = 'A valid employee number is required to clear your own dunnage label rows.';
    ELSE
        START TRANSACTION;

        SELECT COUNT(*) INTO v_rows_to_move
        FROM dunnage_label_data
        WHERE COALESCE(p_clear_all, 0) = 1
           OR employee_number = p_employee_number;

        IF v_rows_to_move = 0 THEN
            -- Nothing to move — succeed silently.
            COMMIT;
        ELSE
            -- Archive matching queue rows into dunnage_history, stamping archive metadata.
            INSERT INTO dunnage_history
            (
                load_uuid,
                part_id,
                quantity,
                quantity_type,
                received_date,
                created_at,
                created_by,
                employee_number,
                created_date,
                po_number,
                type_id,
                type_name,
                type_icon,
                dunnage_type_id,
                dunnage_type_name,
                dunnage_type_icon,
                location,
                label_number,
                part_skid_sequence,
                part_skid_total,
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
                archived_at,
                archived_by,
                archive_batch_id
            )
            SELECT
                dld.load_uuid,
                dld.part_id,
                dld.quantity,
                dld.quantity_type,
                COALESCE(dld.received_date, NOW())  AS received_date,
                dld.created_at                      AS created_at,
                dld.user_id                         AS created_by,
                dld.employee_number                AS employee_number,
                NOW()                               AS created_date,
                dld.po_number,
                dld.dunnage_type_id                 AS type_id,
                dld.dunnage_type_name               AS type_name,
                dld.dunnage_type_icon               AS type_icon,
                dld.dunnage_type_id                 AS dunnage_type_id,
                dld.dunnage_type_name               AS dunnage_type_name,
                dld.dunnage_type_icon               AS dunnage_type_icon,
                dld.location,
                dld.label_number,
                dld.part_skid_sequence,
                dld.part_skid_total,
                dld.udc1,
                dld.udc2,
                dld.udc3,
                dld.udc4,
                dld.udc5,
                dld.udc6,
                dld.udc7,
                dld.udc8,
                dld.udc9,
                dld.udc10,
                NOW()                               AS archived_at,
                p_archived_by                       AS archived_by,
                p_archive_batch_id                  AS archive_batch_id
            FROM dunnage_label_data dld
            WHERE dld.is_reprint = 0
              AND (COALESCE(p_clear_all, 0) = 1
                OR dld.employee_number = p_employee_number);

            -- Remove archived rows from the active queue.
            DELETE FROM dunnage_label_data
            WHERE COALESCE(p_clear_all, 0) = 1
               OR employee_number = p_employee_number;

            COMMIT;

            SET p_rows_moved = v_rows_to_move;
        END IF;
    END IF;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;

-- ============================================================================