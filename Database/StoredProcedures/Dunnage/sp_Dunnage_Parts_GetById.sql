DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Parts_GetById`$$

CREATE PROCEDURE `sp_Dunnage_Parts_GetById`(
    IN p_part_id VARCHAR(50)
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
    WHERE p.part_id = p_part_id;
END$$

DELIMITER ;
