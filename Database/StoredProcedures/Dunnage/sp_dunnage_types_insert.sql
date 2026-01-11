DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_types_insert$$

CREATE PROCEDURE sp_dunnage_types_insert(
    IN p_type_name VARCHAR(100),
    IN p_icon VARCHAR(20) CHARSET utf8mb4,
    IN p_user VARCHAR(50),
    OUT p_new_id INT
)
BEGIN
    INSERT INTO dunnage_types (
        type_name,
        icon,
        created_by,
        created_date
    ) VALUES (
        p_type_name,
        p_icon,
        p_user,
        NOW()
    );

    SET p_new_id = LAST_INSERT_ID();
END$$

DELIMITER ;
