DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_loads_get_by_id$$

CREATE PROCEDURE sp_dunnage_loads_get_by_id(
    IN p_load_uuid CHAR(36)
)
BEGIN
    SELECT 
        l.load_uuid,
        l.part_id,
        p.type_id,
        t.type_name,
        l.quantity,
        l.received_date,
        l.created_by,
        l.created_date,
        l.modified_by,
        l.modified_date
    FROM dunnage_loads l
    JOIN dunnage_parts p ON l.part_id = p.part_id
    JOIN dunnage_types t ON p.type_id = t.id
    WHERE l.load_uuid = p_load_uuid;
END$$

DELIMITER ;
