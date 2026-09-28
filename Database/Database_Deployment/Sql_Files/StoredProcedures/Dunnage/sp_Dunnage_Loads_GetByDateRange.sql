DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Loads_GetByDateRange`$$

CREATE PROCEDURE `sp_Dunnage_Loads_GetByDateRange`(
    IN p_start_date DATETIME,
    IN p_end_date DATETIME
)
BEGIN
    SELECT
        l.load_uuid,
        l.part_id,
        COALESCE(l.type_id, p.type_id) AS type_id,
        COALESCE(l.type_name, t.type_name) AS type_name,
        COALESCE(l.type_icon, 'Help') AS type_icon,
        l.quantity,
        COALESCE(l.quantity_type, p.quantity_type, 'Quantity') AS quantity_type,
        COALESCE(l.po_number, '') AS po_number,
        l.received_date,
        l.created_by,
        l.created_date,
        l.modified_by,
        l.modified_date,
        l.location,
        l.label_number,
        l.part_skid_sequence,
        l.part_skid_total,
        l.udc1,
        l.udc2,
        l.udc3,
        l.udc4,
        l.udc5,
        l.udc6,
        l.udc7,
        l.udc8,
        l.udc9,
        l.udc10
    FROM dunnage_history l
    LEFT JOIN dunnage_parts p ON l.part_id = p.part_id
    LEFT JOIN dunnage_types t ON p.type_id = t.id
    WHERE l.received_date BETWEEN p_start_date AND p_end_date
    ORDER BY l.received_date DESC;
END $$

DELIMITER ;
