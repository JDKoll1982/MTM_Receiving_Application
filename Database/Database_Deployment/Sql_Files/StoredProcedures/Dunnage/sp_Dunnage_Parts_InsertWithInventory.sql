DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Parts_InsertWithInventory` $$

CREATE PROCEDURE `sp_Dunnage_Parts_InsertWithInventory`(
    IN p_part_id VARCHAR(50),
    IN p_type_id INT,
    IN p_spec_values JSON,
    IN p_image_path VARCHAR(255),
    IN p_quantity_type VARCHAR(100),
    IN p_home_location VARCHAR(100),
    IN p_inventory_method VARCHAR(100),
    IN p_inventory_notes TEXT,
    IN p_user VARCHAR(50),
    OUT p_new_id INT
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
        ROLLBACK;
        RESIGNAL;
    END;

    SET FOREIGN_KEY_CHECKS = 0;

    START TRANSACTION;

    INSERT INTO dunnage_parts (
        part_id,
        type_id,
        spec_values,
        image_path,
        quantity_type,
        home_location,
        created_by,
        created_date
    ) VALUES (
        p_part_id,
        p_type_id,
        p_spec_values,
        NULLIF(p_image_path, ''),
        COALESCE(NULLIF(TRIM(p_quantity_type), ''), 'Quantity'),
        p_home_location,
        p_user,
        NOW()
    );

    SET p_new_id = LAST_INSERT_ID();

    IF p_inventory_method IS NOT NULL
        AND TRIM(p_inventory_method) <> ''
        AND UPPER(TRIM(p_inventory_method)) <> 'NOT INVENTORIED'
    THEN
        INSERT INTO dunnage_requires_inventory (
            part_id,
            inventory_method,
            notes,
            created_by,
            created_date
        ) VALUES (
            p_part_id,
            p_inventory_method,
            NULLIF(p_inventory_notes, ''),
            p_user,
            NOW()
        );
    END IF;

    COMMIT;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;