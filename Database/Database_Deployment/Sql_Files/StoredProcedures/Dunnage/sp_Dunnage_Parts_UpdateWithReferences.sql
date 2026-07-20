DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Parts_UpdateWithReferences` $$

CREATE PROCEDURE `sp_Dunnage_Parts_UpdateWithReferences`(
    IN p_id INT,
    IN p_original_part_id VARCHAR(50),
    IN p_new_part_id VARCHAR(50),
    IN p_spec_values JSON,
    IN p_image_path VARCHAR(255),
    IN p_quantity_type VARCHAR(100),
    IN p_home_location VARCHAR(100),
    IN p_inventory_method VARCHAR(100),
    IN p_inventory_notes TEXT,
    IN p_user VARCHAR(50)
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
        RESIGNAL;
    END;

    SET FOREIGN_KEY_CHECKS = 0;

    START TRANSACTION;

    UPDATE dunnage_parts
    SET
        part_id = p_new_part_id,
        spec_values = p_spec_values,
        image_path = NULLIF(p_image_path, ''),
        quantity_type = COALESCE(NULLIF(TRIM(p_quantity_type), ''), 'Quantity'),
        home_location = p_home_location,
        modified_by = p_user,
        modified_date = NOW()
    WHERE id = p_id;

    UPDATE dunnage_history
    SET
        part_id = p_new_part_id,
        quantity_type = COALESCE(NULLIF(TRIM(p_quantity_type), ''), 'Quantity'),
        specs_json = p_spec_values,
        modified_by = p_user,
        modified_date = NOW()
    WHERE part_id IN (p_original_part_id, p_new_part_id);

    UPDATE dunnage_label_data
    SET
        part_id = p_new_part_id,
        quantity_type = COALESCE(NULLIF(TRIM(p_quantity_type), ''), 'Quantity'),
        specs_json = p_spec_values
    WHERE part_id IN (p_original_part_id, p_new_part_id);

    IF p_inventory_method IS NULL
        OR TRIM(p_inventory_method) = ''
        OR UPPER(TRIM(p_inventory_method)) = 'NOT INVENTORIED'
    THEN
        DELETE FROM dunnage_requires_inventory
        WHERE part_id IN (p_original_part_id, p_new_part_id);
    ELSE
        IF EXISTS(
            SELECT 1
            FROM dunnage_requires_inventory
            WHERE part_id = p_original_part_id
            LIMIT 1
        ) THEN
            UPDATE dunnage_requires_inventory
            SET
                part_id = p_new_part_id,
                inventory_method = p_inventory_method,
                notes = NULLIF(p_inventory_notes, ''),
                modified_by = p_user,
                modified_date = NOW()
            WHERE part_id = p_original_part_id;
        ELSEIF EXISTS(
            SELECT 1
            FROM dunnage_requires_inventory
            WHERE part_id = p_new_part_id
            LIMIT 1
        ) THEN
            UPDATE dunnage_requires_inventory
            SET
                inventory_method = p_inventory_method,
                notes = NULLIF(p_inventory_notes, ''),
                modified_by = p_user,
                modified_date = NOW()
            WHERE part_id = p_new_part_id;
        ELSE
            INSERT INTO dunnage_requires_inventory (
                part_id,
                inventory_method,
                notes,
                created_by,
                created_date
            ) VALUES (
                p_new_part_id,
                p_inventory_method,
                NULLIF(p_inventory_notes, ''),
                p_user,
                NOW()
            );
        END IF;
    END IF;

    COMMIT;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER;