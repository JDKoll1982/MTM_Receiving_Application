DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_Insert`$$

CREATE PROCEDURE `sp_Dunnage_Specs_Insert`(
    IN p_type_id INT,
    IN p_spec_key VARCHAR(100),
    IN p_spec_value JSON,
    IN p_user VARCHAR(50),
    OUT p_new_id INT
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
    INSERT INTO dunnage_specs (
        type_id,
        spec_key,
        spec_value,
        created_by,
        created_date
    ) VALUES (
        p_type_id,
        p_spec_key,
        p_spec_value,
        p_user,
        NOW()
    );
    
    SET p_new_id = LAST_INSERT_ID();
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
