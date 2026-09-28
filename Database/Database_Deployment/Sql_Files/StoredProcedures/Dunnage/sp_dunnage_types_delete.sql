-- ============================================================================
-- Stored Procedure: sp_Dunnage_Types_Delete
-- Description: Soft delete a dunnage type by marking it inactive
-- Parameters:
--   @p_id: The ID of the dunnage type to delete
-- Feature: Dunnage Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Dunnage_Types_Delete`;

DELIMITER $$

CREATE PROCEDURE `sp_Dunnage_Types_Delete`(
    IN p_id INT,
    IN p_modified_by VARCHAR(50),
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
        ROLLBACK;
    END;

    SET FOREIGN_KEY_CHECKS = 0;

    START TRANSACTION;

    -- Check if dunnage type exists
    SELECT COUNT(*) INTO v_exists
    FROM dunnage_types
    WHERE id = p_id;

    IF v_exists = 0 THEN
        SET p_status = 0;
        SET p_error_msg = 'Dunnage type not found';
        ROLLBACK;
    ELSE
        -- Full cascade: deleting the type also deletes its parts and every
        -- active label-data / archived history row that references the type
        -- or one of its parts. Inventory rows, custom fields, and custom-field
        -- choices are removed via ON DELETE CASCADE foreign keys.

        -- 1) Active label-data queue rows for the type's parts or type snapshot.
        DELETE FROM dunnage_label_data
        WHERE dunnage_type_id = p_id
           OR part_id IN (SELECT part_id FROM dunnage_parts WHERE type_id = p_id);

        -- 2) Archived history rows for the type's parts or type snapshot.
        DELETE FROM dunnage_history
        WHERE dunnage_type_id = p_id
           OR type_id = p_id
           OR part_id IN (SELECT part_id FROM dunnage_parts WHERE type_id = p_id);

        -- 3) Per-part non-PO reference defaults for the type's parts.
        DELETE FROM dunnage_non_po_part_defaults
        WHERE part_id IN (SELECT part_id FROM dunnage_parts WHERE type_id = p_id);

        -- 4) The type's parts (inventory rows cascade via FK).
        DELETE FROM dunnage_parts WHERE type_id = p_id;

        -- 5) The type (custom fields + choices cascade via FK).
        DELETE FROM dunnage_types WHERE id = p_id;

        SET p_status = 1;
        SET p_error_msg = 'Dunnage type deleted successfully';
        COMMIT;
    END IF;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;