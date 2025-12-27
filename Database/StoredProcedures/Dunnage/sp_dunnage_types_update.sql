DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_types_update$$

CREATE PROCEDURE sp_dunnage_types_update(
    IN p_id INT,
    IN p_type_name VARCHAR(100),
    IN p_user VARCHAR(50)
)
BEGIN
    UPDATE dunnage_types
    SET 
        type_name = p_type_name,
        modified_by = p_user,
        modified_date = NOW()
    WHERE id = p_id;
END$$

DELIMITER ;
