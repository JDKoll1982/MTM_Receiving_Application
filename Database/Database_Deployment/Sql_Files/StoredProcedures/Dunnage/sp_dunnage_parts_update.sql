DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Parts_Update`$$

CREATE PROCEDURE `sp_Dunnage_Parts_Update`(
    IN p_id INT,
    IN p_part_id VARCHAR(50),
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
    IN p_user VARCHAR(50)
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
    UPDATE dunnage_parts
    SET 
        part_id = p_part_id,
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
        image_path = NULLIF(p_image_path, ''),
        quantity_type = COALESCE(NULLIF(TRIM(p_quantity_type), ''), 'Quantity'),
        home_location = p_home_location,
        modified_by = p_user,
        modified_date = NOW()
    WHERE id = p_id;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
