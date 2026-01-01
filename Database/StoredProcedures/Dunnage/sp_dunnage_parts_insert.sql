DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_parts_insert$$

CREATE PROCEDURE sp_dunnage_parts_insert(
    IN p_part_id VARCHAR(50),
    IN p_type_id INT,
    IN p_spec_values JSON,
    IN p_home_location VARCHAR(100),
    IN p_user VARCHAR(50),
    OUT p_new_id INT
)
BEGIN
    INSERT INTO dunnage_parts (
        part_id,
        type_id,
        spec_values,
        home_location,
        created_by,
        created_date
    ) VALUES (
        p_part_id,
        p_type_id,
        p_spec_values,
        p_home_location,
        p_user,
        NOW()
    );
    
    SET p_new_id = LAST_INSERT_ID();
END$$

DELIMITER ;
