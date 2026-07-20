DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Parts_Update`$$

CREATE PROCEDURE `sp_Dunnage_Parts_Update`(
    IN p_id INT,
    IN p_part_id VARCHAR(50),
    IN p_spec_values JSON,
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
        spec_values = p_spec_values,
        image_path = NULLIF(p_image_path, ''),
        quantity_type = COALESCE(NULLIF(TRIM(p_quantity_type), ''), 'Quantity'),
        home_location = p_home_location,
        modified_by = p_user,
        modified_date = NOW()
    WHERE id = p_id;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
