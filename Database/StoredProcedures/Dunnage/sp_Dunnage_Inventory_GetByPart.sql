DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Inventory_GetByPart`$$

CREATE PROCEDURE `sp_Dunnage_Inventory_GetByPart`(
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
    FROM dunnage_requires_inventory i
    JOIN dunnage_parts p ON i.part_id = p.part_id
    JOIN dunnage_types t ON p.type_id = t.id
    WHERE i.part_id = p_part_id;
END$$

DELIMITER ;
