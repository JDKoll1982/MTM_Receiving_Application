USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Volvo_GeneratedLabelData_ClearToHistory`;

DELIMITER $$

CREATE PROCEDURE `sp_Volvo_GeneratedLabelData_ClearToHistory`(
    IN  p_archived_by       VARCHAR(100),
    OUT p_rows_moved        INT,
    OUT p_archive_batch_id  CHAR(36),
    OUT p_status            INT,
    OUT p_error_message     VARCHAR(1000)
)
BEGIN
    DECLARE v_rows_count INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_rows_moved = 0;
        SET p_status = 1;
        SET p_error_message = 'Clear generated Volvo label data failed. Transaction rolled back.';
    END;

    SET p_rows_moved = 0;
    SET p_status = 0;
    SET p_error_message = NULL;
    SET p_archive_batch_id = UUID();

    START TRANSACTION;

    SELECT COUNT(*) INTO v_rows_count
    FROM volvo_generated_label_data;

    IF v_rows_count = 0 THEN
        COMMIT;
    ELSE
        INSERT INTO volvo_generated_label_history (
            original_id,
            shipment_id,
            shipment_number,
            shipment_date,
            part_number,
            quantity,
            skid_number,
            total_skids,
            part_description,
            source_created_at,
            source_updated_at,
            archived_at,
            archived_by,
            archive_batch_id
        )
        SELECT
            vgl.id,
            vgl.shipment_id,
            vgl.shipment_number,
            vgl.shipment_date,
            vgl.part_number,
            vgl.quantity,
            vgl.skid_number,
            vgl.total_skids,
            vgl.part_description,
            vgl.created_at,
            vgl.updated_at,
            NOW(),
            p_archived_by,
            p_archive_batch_id
        FROM volvo_generated_label_data vgl;

        DELETE FROM volvo_generated_label_data;

        SET p_rows_moved = v_rows_count;

        COMMIT;
    END IF;
END $$

DELIMITER ;