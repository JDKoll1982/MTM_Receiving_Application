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
        p.udc1,
        p.udc2,
        p.udc3,
        p.udc4,
        p.udc5,
        p.udc6,
        p.udc7,
        p.udc8,
        p.udc9,
        p.udc10,
        p.image_path,
        p.quantity_type,
        t.image_path AS type_image_path,
        p.home_location,
        p.created_by,
        p.created_date,
        p.modified_by,
        p.modified_date
    FROM dunnage_parts p
    JOIN dunnage_types t ON p.type_id = t.id
    WHERE p.part_id = p_part_id;
END $$

DELIMITER ;
