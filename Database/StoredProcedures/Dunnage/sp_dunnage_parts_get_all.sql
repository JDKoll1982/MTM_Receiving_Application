DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_parts_get_all$$

CREATE PROCEDURE sp_dunnage_parts_get_all()
BEGIN
    SELECT 
        p.id,
        p.part_id,
        p.type_id,
        t.type_name,
        p.spec_values,
        p.created_by,
        p.created_date,
        p.modified_by,
        p.modified_date
    FROM dunnage_parts p
    JOIN dunnage_types t ON p.type_id = t.id
    ORDER BY p.part_id;
END$$

DELIMITER ;
