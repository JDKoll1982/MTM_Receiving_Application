DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_DeleteById`$$

CREATE PROCEDURE `sp_Dunnage_Specs_DeleteById`(
    IN p_id INT
)
BEGIN
    DELETE FROM dunnage_specs
    WHERE id = p_id;
END$$

DELIMITER ;
