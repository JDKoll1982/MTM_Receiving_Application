-- ============================================================================
-- Stored Procedure: sp_Dunnage_History_Insert
-- Description: Writes one load straight into dunnage_history, skipping the
--   active label queue. Used for parts flagged in dunnage_no_label_parts: they
--   never need a printed label, so there is nothing for LabelView to collect and
--   the row would sit in dunnage_label_data for no reason.
-- Notes:
--   * archived_at, archived_by and archive_batch_id stay NULL: these rows were
--     never in the queue, so they were never "archived" out of it.
--   * Everything else copies the queue insert exactly, so a history row looks the
--     same whichever path wrote it.
-- Feature: Dunnage Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_History_Insert` $$

CREATE PROCEDURE `sp_Dunnage_History_Insert`(
    IN p_load_uuid          CHAR(36),
    IN p_part_id            VARCHAR(50),
    IN p_dunnage_type_id    INT,
    IN p_dunnage_type_name  VARCHAR(100),
    IN p_dunnage_type_icon  VARCHAR(100),
    IN p_quantity           DECIMAL(10,2),
    IN p_quantity_type      VARCHAR(100),
    IN p_po_number          VARCHAR(50),
    IN p_received_date      DATETIME,
    IN p_user_id            VARCHAR(100),
    IN p_employee_number    INT,
    IN p_location           VARCHAR(100),
    IN p_label_number       VARCHAR(50),
    IN p_part_skid_sequence INT,
    IN p_part_skid_total    INT,
    IN p_udc1 VARCHAR(255),
    IN p_udc2 VARCHAR(255),
    IN p_udc3 VARCHAR(255),
    IN p_udc4 VARCHAR(255),
    IN p_udc5 VARCHAR(255),
    IN p_udc6 VARCHAR(255),
    IN p_udc7 VARCHAR(255),
    IN p_udc8 VARCHAR(255),
    IN p_udc9 VARCHAR(255),
    IN p_udc10 VARCHAR(255)
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;

    INSERT INTO dunnage_history
    (
        load_uuid,
        part_id,
        quantity,
        quantity_type,
        received_date,
        created_at,
        created_by,
        user_id,
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
        udc10
    )
    VALUES
    (
        p_load_uuid,
        p_part_id,
        p_quantity,
        COALESCE(NULLIF(TRIM(p_quantity_type), ''), 'Quantity'),
        COALESCE(p_received_date, NOW()),
        NOW(),
        p_user_id,
        p_user_id,
        p_employee_number,
        NOW(),
        p_po_number,
        p_dunnage_type_id,
        p_dunnage_type_name,
        p_dunnage_type_icon,
        p_dunnage_type_id,
        p_dunnage_type_name,
        p_dunnage_type_icon,
        p_location,
        p_label_number,
        p_part_skid_sequence,
        p_part_skid_total,
        p_udc1,
        p_udc2,
        p_udc3,
        p_udc4,
        p_udc5,
        p_udc6,
        p_udc7,
        p_udc8,
        p_udc9,
        p_udc10
    );

    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
