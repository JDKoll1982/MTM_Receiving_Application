DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Parts_Insert`$$

CREATE PROCEDURE `sp_Dunnage_Parts_Insert`(
    IN p_part_id VARCHAR(50),
    IN p_type_id INT,
    IN p_spec_values JSON,
    IN p_image_path VARCHAR(255),
    IN p_quantity_type VARCHAR(100),
    IN p_home_location VARCHAR(100),
    IN p_user VARCHAR(50),
    OUT p_new_id INT
)
BEGIN
    SET FOREIGN_KEY_CHECKS = 0;
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
    SET FOREIGN_KEY_CHECKS = 1;
END $$

DELIMITER ;
