DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_GetAll`$$

CREATE PROCEDURE `sp_Dunnage_Specs_GetAll`()
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
    FROM dunnage_specs;
END$$

DELIMITER ;
