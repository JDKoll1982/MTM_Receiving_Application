-- Stored Procedure: sp_Dunnage_LabelData_ClearToHistory
-- Description: Atomically moves all rows from dunnage_label_data (active queue)
--              to dunnage_history (archive) and deletes them from the queue.
--              All rows moved in one call share the same archive_batch_id.
-- Lifecycle: Clear Label Data -> Queue to History + Queue Delete

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Dunnage_LabelData_ClearToHistory`;

DELIMITER $$

CREATE PROCEDURE `sp_Dunnage_LabelData_ClearToHistory`(
    IN  p_archived_by       VARCHAR(100),
    OUT p_rows_moved        INT,
    OUT p_archive_batch_id  CHAR(36),
    OUT p_status            INT,
    OUT p_error_message     VARCHAR(1000)
)
BEGIN
    DECLARE v_rows_to_move INT DEFAULT 0;

    -- Roll back and surface the error if anything inside the transaction fails.
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_rows_moved       = 0;
        SET p_status           = 1;
        SET p_error_message    = 'Clear Label Data failed. Transaction rolled back.';
    END;

    SET p_rows_moved      = 0;
    SET p_status          = 0;
    SET p_error_message   = NULL;
    SET p_archive_batch_id = UUID();

    START TRANSACTION;

    SELECT COUNT(*) INTO v_rows_to_move
    FROM dunnage_label_data;

    IF v_rows_to_move = 0 THEN
        -- Nothing to move — succeed silently.
        COMMIT;
    ELSE
        -- Archive all queue rows into dunnage_history, stamping archive metadata.
        INSERT INTO dunnage_history
        (
            load_uuid,
            part_id,
            quantity,
            received_date,
            created_by,
            created_date,
            po_number,
            type_id,
            type_name,
            type_icon,
            location,
            label_number,
            specs_json,
            archived_at,
            archived_by,
            archive_batch_id
        )
        SELECT
            dld.load_uuid,
            dld.part_id,
            dld.quantity,
            COALESCE(dld.received_date, NOW())  AS received_date,
            dld.user_id                         AS created_by,
            NOW()                               AS created_date,
            dld.po_number,
            dld.dunnage_type_id                 AS type_id,
            dld.dunnage_type_name               AS type_name,
            dld.dunnage_type_icon               AS type_icon,
            dld.location,
            dld.label_number,
            dld.specs_json,
            NOW()                               AS archived_at,
            p_archived_by                       AS archived_by,
            p_archive_batch_id                  AS archive_batch_id
        FROM dunnage_label_data dld;

        -- Remove archived rows from the active queue.
        DELETE FROM dunnage_label_data;

        COMMIT;

        SET p_rows_moved = v_rows_to_move;
    END IF;
END$$

DELIMITER ;

-- ============================================================================
