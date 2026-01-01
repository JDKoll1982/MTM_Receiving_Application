DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_parts_get_by_type$$

CREATE PROCEDURE sp_dunnage_parts_get_by_type(
    IN p_type_id INT
)
BEGIN
    SELECT 
        p.id,
        p.part_id,
        p.type_id,
        t.type_name,
        p.spec_values,
        p.home_location,
        p.created_by,
        p.created_date,
        p.modified_by,
        p.modified_date
    FROM dunnage_parts p
    JOIN dunnage_types t ON p.type_id = t.id
    WHERE p.type_id = p_type_id
    ORDER BY p.part_id;
END$$

DELIMITER ;
