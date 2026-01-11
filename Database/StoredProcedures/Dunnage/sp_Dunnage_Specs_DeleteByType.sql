DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_DeleteByType`$$

CREATE PROCEDURE `sp_Dunnage_Specs_DeleteByType`(
    IN p_type_id INT
)
BEGIN
    DELETE FROM dunnage_specs
    WHERE type_id = p_type_id;
END$$

DELIMITER ;
