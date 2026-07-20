USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Volvo_GeneratedLabelData_ClearToHistory`;

DELIMITER $$

CREATE PROCEDURE `sp_Volvo_GeneratedLabelData_ClearToHistory`(
    IN  p_archived_by       VARCHAR(100),
    IN  p_employee_number   INT,
    IN  p_clear_all         TINYINT(1),
    OUT p_rows_moved        INT,
    OUT p_archive_batch_id  CHAR(36),
    OUT p_status            INT,
    OUT p_error_message     VARCHAR(1000)
)
BEGIN
    SET FOREIGN_KEY_CHECKS = 0;
    DECLARE v_rows_count INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        SET FOREIGN_KEY_CHECKS = 1;
        ROLLBACK;
        SET p_rows_moved = 0;
        SET p_status = 1;
        SET p_error_message = 'Clear generated Volvo label data failed. Transaction rolled back.';
    END;

    SET p_rows_moved = 0;
    SET p_status = 0;
    SET p_error_message = NULL;
    SET p_archive_batch_id = UUID();

    IF COALESCE(p_clear_all, 0) = 0 AND COALESCE(p_employee_number, 0) <= 0 THEN
        SET p_status = 1;
        SET p_error_message = 'A valid employee number is required to clear your own Volvo generated label rows.';
    ELSE
        START TRANSACTION;

        SELECT COUNT(*) INTO v_rows_count
        FROM volvo_generated_label_data
        WHERE COALESCE(p_clear_all, 0) = 1
           OR employee_number = p_employee_number;

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
                employee_number,
                source_created_at,
                source_updated_at,
                archived_at,
                archived_by,
                archive_batch_id
            )
            SELECT
                vgl.id,
                COALESCE(vgl.shipment_id, 0),
                COALESCE(vgl.shipment_number, 0),
                COALESCE(vgl.shipment_date, CURRENT_DATE),
                COALESCE(vgl.part_number, ''),
                COALESCE(vgl.quantity, 0),
                COALESCE(vgl.skid_number, 0),
                COALESCE(vgl.total_skids, 0),
                COALESCE(vgl.part_description, ''),
                vgl.employee_number,
                COALESCE(vgl.created_at, NOW()),
                COALESCE(vgl.updated_at, NOW()),
                COALESCE(NOW(), CURRENT_TIMESTAMP),
                COALESCE(NULLIF(TRIM(p_archived_by), ''), ''),
                COALESCE(NULLIF(TRIM(p_archive_batch_id), ''), UUID())
            FROM volvo_generated_label_data vgl
            WHERE COALESCE(p_clear_all, 0) = 1
               OR vgl.employee_number = p_employee_number;

            DELETE FROM volvo_generated_label_data
            WHERE COALESCE(p_clear_all, 0) = 1
               OR employee_number = p_employee_number;

            SET p_rows_moved = v_rows_count;

            COMMIT;
        END IF;
    END IF;
    SET FOREIGN_KEY_CHECKS = 1;
END $$

DELIMITER;

DELIMITER;