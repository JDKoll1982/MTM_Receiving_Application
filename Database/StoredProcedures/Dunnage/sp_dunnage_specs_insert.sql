DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_specs_insert$$

CREATE PROCEDURE sp_dunnage_specs_insert(
    IN p_type_id INT,
    IN p_spec_key VARCHAR(100),
    IN p_spec_value JSON,
    IN p_user VARCHAR(50),
    OUT p_new_id INT
)
BEGIN
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
END$$

DELIMITER ;
