DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_GetById`$$

CREATE PROCEDURE `sp_Dunnage_Specs_GetById`(
    IN p_id INT
)
BEGIN
    SELECT 
        id,
        type_id,
        spec_key,
        spec_value,
        created_by,
        created_date,
        modified_by,
        modified_date
    FROM dunnage_specs
    WHERE id = p_id;
END$$

DELIMITER ;
