DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Parts_Update`$$

CREATE PROCEDURE `sp_Dunnage_Parts_Update`(
    IN p_id INT,
    IN p_spec_values JSON,
    IN p_home_location VARCHAR(100),
    IN p_user VARCHAR(50)
)
BEGIN
    UPDATE dunnage_parts
    SET 
        spec_values = p_spec_values,
        home_location = p_home_location,
        modified_by = p_user,
        modified_date = NOW()
    WHERE id = p_id;
END$$

DELIMITER ;
