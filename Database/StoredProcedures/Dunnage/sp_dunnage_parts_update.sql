DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_parts_update$$

CREATE PROCEDURE sp_dunnage_parts_update(
    IN p_id INT,
    IN p_spec_values JSON,
    IN p_user VARCHAR(50)
)
BEGIN
    UPDATE dunnage_parts
    SET 
        spec_values = p_spec_values,
        modified_by = p_user,
        modified_date = NOW()
    WHERE id = p_id;
END$$

DELIMITER ;
