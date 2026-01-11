DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_Update`$$

CREATE PROCEDURE `sp_Dunnage_Specs_Update`(
    IN p_id INT,
    IN p_spec_value JSON,
    IN p_user VARCHAR(50)
)
BEGIN
    UPDATE dunnage_specs
    SET 
        spec_value = p_spec_value,
        modified_by = p_user,
        modified_date = NOW()
    WHERE id = p_id;
END$$

DELIMITER ;
