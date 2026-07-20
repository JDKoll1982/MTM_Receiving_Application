DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_Update`$$

CREATE PROCEDURE `sp_Dunnage_Specs_Update`(
    IN p_id INT,
    IN p_spec_value JSON,
    IN p_user VARCHAR(50)
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
    UPDATE dunnage_specs
    SET 
        spec_value = p_spec_value,
        modified_by = p_user,
        modified_date = NOW()
    WHERE id = p_id;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
