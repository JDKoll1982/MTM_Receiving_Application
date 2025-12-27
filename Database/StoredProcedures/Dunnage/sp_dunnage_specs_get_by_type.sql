DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_specs_get_by_type$$

CREATE PROCEDURE sp_dunnage_specs_get_by_type(
    IN p_type_id INT
)
BEGIN
    SELECT 
        id,
        type_id,
        spec_key,
        spec_value,
        created_by,
        created_date,
        modified_by,
        modified_date
    FROM dunnage_specs
    WHERE type_id = p_type_id;
END$$

DELIMITER ;
