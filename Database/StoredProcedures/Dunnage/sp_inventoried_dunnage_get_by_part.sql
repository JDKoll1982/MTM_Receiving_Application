DELIMITER $$

DROP PROCEDURE IF EXISTS sp_inventoried_dunnage_get_by_part$$

CREATE PROCEDURE sp_inventoried_dunnage_get_by_part(
    IN p_part_id VARCHAR(50)
)
BEGIN
    SELECT 
        i.id,
        i.part_id,
        t.type_name,
        i.inventory_method,
        i.notes,
        i.created_by,
        i.created_date,
        i.modified_by,
        i.modified_date
    FROM inventoried_dunnage i
    JOIN dunnage_parts p ON i.part_id = p.part_id
    JOIN dunnage_types t ON p.type_id = t.id
    WHERE i.part_id = p_part_id;
END$$

DELIMITER ;
