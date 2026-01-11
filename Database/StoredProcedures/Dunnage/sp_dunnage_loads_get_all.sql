DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_history_get_all$$

CREATE PROCEDURE sp_dunnage_history_get_all()
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
    FROM dunnage_history l
    JOIN dunnage_parts p ON l.part_id = p.part_id
    JOIN dunnage_types t ON p.type_id = t.id
    ORDER BY l.received_date DESC;
END$$

DELIMITER ;
