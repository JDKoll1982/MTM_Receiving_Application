DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_DeleteByType`$$

CREATE PROCEDURE `sp_Dunnage_Specs_DeleteByType`(
    IN p_type_id INT
)
BEGIN
    SET FOREIGN_KEY_CHECKS = 0;
    DELETE FROM dunnage_specs
    WHERE type_id = p_type_id;
    SET FOREIGN_KEY_CHECKS = 1;
END $$

DELIMITER ;
