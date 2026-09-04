-- ============================================================================
-- Stored Procedure: sp_Dunnage_Parts_ChangeType
-- Description: Re-associates a dunnage part with a different dunnage type and
--   rewrites every saved reference to match the new type. Updates:
--     1) dunnage_parts       - type_id + new udc layout
--     2) dunnage_label_data  - type snapshot + new udc layout (active queue)
--     3) dunnage_history     - type snapshot aliases + new udc layout (archived)
--   The caller supplies the part's udc1..10 already laid out for the NEW type.
-- Parameters:
--   p_part_id:    dunnage_parts.part_id (business id, used by label/history rows)
--   p_new_type_id: dunnage_types.id to move the part to
--   p_udc1..p_udc10: final spec values arranged for the new type's slots
--   p_user:       user performing the change
-- Feature: Dunnage Module - Change Type
-- MySQL Version: 5.7 compatible
-- ============================================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Parts_ChangeType` $$

CREATE PROCEDURE `sp_Dunnage_Parts_ChangeType`(
    IN p_part_id VARCHAR(50),
    IN p_new_type_id INT,
    IN p_udc1 VARCHAR(255),
    IN p_udc2 VARCHAR(255),
    IN p_udc3 VARCHAR(255),
    IN p_udc4 VARCHAR(255),
    IN p_udc5 VARCHAR(255),
    IN p_udc6 VARCHAR(255),
    IN p_udc7 VARCHAR(255),
    IN p_udc8 VARCHAR(255),
    IN p_udc9 VARCHAR(255),
    IN p_udc10 VARCHAR(255),
    IN p_user VARCHAR(50)
)
BEGIN
    DECLARE v_type_name VARCHAR(100);
    DECLARE v_type_icon VARCHAR(100);
    DECLARE v_exists INT DEFAULT 0;
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
        RESIGNAL;
    END;

    SELECT COUNT(*) INTO v_exists
      FROM dunnage_parts
     WHERE part_id = p_part_id;

    IF v_exists = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Dunnage part not found';
    END IF;

    SELECT type_name, icon
      INTO v_type_name, v_type_icon
      FROM dunnage_types
     WHERE id = p_new_type_id;

    IF v_type_name IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Target dunnage type not found';
    END IF;

    SET FOREIGN_KEY_CHECKS = 0;
    START TRANSACTION;

    -- 1) Master part record.
    UPDATE dunnage_parts
       SET type_id = p_new_type_id,
           udc1 = p_udc1,
           udc2 = p_udc2,
           udc3 = p_udc3,
           udc4 = p_udc4,
           udc5 = p_udc5,
           udc6 = p_udc6,
           udc7 = p_udc7,
           udc8 = p_udc8,
           udc9 = p_udc9,
           udc10 = p_udc10,
           modified_by = p_user,
           modified_date = NOW()
     WHERE part_id = p_part_id;

    -- 2) Active label-data queue rows.
    UPDATE dunnage_label_data
       SET dunnage_type_id = p_new_type_id,
           dunnage_type_name = v_type_name,
           dunnage_type_icon = v_type_icon,
           udc1 = p_udc1,
           udc2 = p_udc2,
           udc3 = p_udc3,
           udc4 = p_udc4,
           udc5 = p_udc5,
           udc6 = p_udc6,
           udc7 = p_udc7,
           udc8 = p_udc8,
           udc9 = p_udc9,
           udc10 = p_udc10
     WHERE part_id = p_part_id;

    -- 3) Archived history rows (both snapshot column sets).
    UPDATE dunnage_history
       SET dunnage_type_id = p_new_type_id,
           dunnage_type_name = v_type_name,
           dunnage_type_icon = v_type_icon,
           type_id = p_new_type_id,
           type_name = v_type_name,
           type_icon = v_type_icon,
           udc1 = p_udc1,
           udc2 = p_udc2,
           udc3 = p_udc3,
           udc4 = p_udc4,
           udc5 = p_udc5,
           udc6 = p_udc6,
           udc7 = p_udc7,
           udc8 = p_udc8,
           udc9 = p_udc9,
           udc10 = p_udc10,
           modified_by = p_user,
           modified_date = NOW()
     WHERE part_id = p_part_id;

    COMMIT;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
