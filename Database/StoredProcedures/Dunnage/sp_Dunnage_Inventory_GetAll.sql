DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Inventory_GetAll`$$

CREATE PROCEDURE `sp_Dunnage_Inventory_GetAll`()
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
    FROM dunnage_requires_inventory i
    JOIN dunnage_parts p ON i.part_id = p.part_id
    JOIN dunnage_types t ON p.type_id = t.id
    ORDER BY i.part_id;
END$$

DELIMITER ;
