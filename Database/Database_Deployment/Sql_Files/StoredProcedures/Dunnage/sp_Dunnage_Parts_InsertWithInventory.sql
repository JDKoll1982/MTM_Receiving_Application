DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Parts_InsertWithInventory` $$

CREATE PROCEDURE `sp_Dunnage_Parts_InsertWithInventory`(
    IN p_part_id VARCHAR(50),
    IN p_type_id INT,
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
        udc1,
        udc2,
        udc3,
        udc4,
        udc5,
        udc6,
        udc7,
        udc8,
        udc9,
        udc10,
        image_path,
        quantity_type,
        home_location,
        created_by,
        created_date
    ) VALUES (
        p_part_id,
        p_type_id,
        p_udc1,
        p_udc2,
        p_udc3,
        p_udc4,
        p_udc5,
        p_udc6,
        p_udc7,
        p_udc8,
        p_udc9,
        p_udc10,
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