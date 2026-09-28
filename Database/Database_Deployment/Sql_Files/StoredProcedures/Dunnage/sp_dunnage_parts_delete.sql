-- ============================================================================
-- Stored Procedure: sp_Dunnage_Parts_Delete
-- Description: Cascade delete a dunnage part. Removes every active label-data
--   queue row, archived history row, inventory-tracking row, and per-part
--   non-PO default that references the part before deleting the part itself.
-- Parameters:
--   p_id: The ID of the dunnage part to delete
-- Feature: Dunnage Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Parts_Delete`$$

CREATE PROCEDURE `sp_Dunnage_Parts_Delete`(
    IN p_id INT
)
BEGIN
    DECLARE v_part_id VARCHAR(50);
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
        RESIGNAL;
    END;

    SELECT part_id
      INTO v_part_id
      FROM dunnage_parts
     WHERE id = p_id
     LIMIT 1;

    SET FOREIGN_KEY_CHECKS = 0;
    START TRANSACTION;

    IF v_part_id IS NOT NULL THEN
        -- Active label-data queue rows for this part.
        DELETE FROM dunnage_label_data WHERE part_id = v_part_id;

        -- Archived history rows for this part (FK is ON DELETE RESTRICT).
        DELETE FROM dunnage_history WHERE part_id = v_part_id;

        -- Inventory tracking rows (also cascade via FK; explicit for clarity).
        DELETE FROM dunnage_requires_inventory WHERE part_id = v_part_id;

        -- Per-part non-PO reference defaults.
        DELETE FROM dunnage_non_po_part_defaults WHERE part_id = v_part_id;
    END IF;

    DELETE FROM dunnage_parts WHERE id = p_id;

    COMMIT;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
