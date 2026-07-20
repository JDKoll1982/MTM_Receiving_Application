DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_DeleteById`$$

CREATE PROCEDURE `sp_Dunnage_Specs_DeleteById`(
    IN p_id INT
)
BEGIN
    SET FOREIGN_KEY_CHECKS = 0;
    DELETE FROM dunnage_specs
    WHERE id = p_id;
    SET FOREIGN_KEY_CHECKS = 1;
END $$

DELIMITER ;
