DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_specs_update$$

CREATE PROCEDURE sp_dunnage_specs_update(
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
