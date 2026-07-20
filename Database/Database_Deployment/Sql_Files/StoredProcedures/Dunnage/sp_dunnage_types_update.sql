-- =============================================
-- Stored Procedure: sp_Dunnage_Types_Update
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Update Dunnage Type
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Dunnage_Types_Update`$$
CREATE PROCEDURE `sp_Dunnage_Types_Update`(
    IN p_id INT,
    IN p_type_name VARCHAR(100),
    IN p_icon VARCHAR(50),
    IN p_image_path VARCHAR(255),
    IN p_modified_by VARCHAR(50)
)
BEGIN
    DECLARE v_old_type_name VARCHAR(100);
    DECLARE v_old_icon VARCHAR(50);
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
        RESIGNAL;
    END;

    SELECT type_name, icon
    INTO v_old_type_name, v_old_icon
    FROM dunnage_types
    WHERE id = p_id
    LIMIT 1;

    SET FOREIGN_KEY_CHECKS = 0;
    START TRANSACTION;

    -- Check for duplicate type_name (excluding self)
    IF EXISTS (SELECT 1 FROM dunnage_types WHERE type_name = p_type_name AND id != p_id) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Dunnage type name already exists';
    END IF;

    UPDATE dunnage_types
    SET type_name = p_type_name,
        icon = p_icon,
        image_path = NULLIF(p_image_path, ''),
        modified_by = p_modified_by,
        modified_date = CURRENT_TIMESTAMP
    WHERE id = p_id;

    IF COALESCE(v_old_type_name, '') <> COALESCE(p_type_name, '')
        OR COALESCE(v_old_icon, '') <> COALESCE(p_icon, '') THEN
        UPDATE dunnage_history
        SET
            type_id = p_id,
            type_name = p_type_name,
            type_icon = p_icon,
            dunnage_type_id = p_id,
            dunnage_type_name = p_type_name,
            dunnage_type_icon = p_icon,
            modified_by = p_modified_by,
            modified_date = CURRENT_TIMESTAMP
        WHERE type_id = p_id
           OR (type_id IS NULL AND type_name = v_old_type_name);

        UPDATE dunnage_label_data
        SET
            dunnage_type_id = p_id,
            dunnage_type_name = p_type_name,
            dunnage_type_icon = p_icon
        WHERE dunnage_type_id = p_id
           OR (dunnage_type_id IS NULL AND dunnage_type_name = v_old_type_name);
    END IF;

    SELECT ROW_COUNT() AS affected_rows;
    COMMIT;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$



DELIMITER ;
