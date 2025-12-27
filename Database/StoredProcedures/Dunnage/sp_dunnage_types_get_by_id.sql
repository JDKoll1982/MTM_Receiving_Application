DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_types_get_by_id$$

CREATE PROCEDURE sp_dunnage_types_get_by_id(
    IN p_id INT
)
BEGIN
    SELECT 
        id,
        type_name,
        created_by,
        created_date,
        modified_by,
        modified_date
    FROM dunnage_types
    WHERE id = p_id;
END$$

DELIMITER ;
