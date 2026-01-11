DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Parts_Delete`$$

CREATE PROCEDURE `sp_Dunnage_Parts_Delete`(
    IN p_id INT
)
BEGIN
    DELETE FROM dunnage_parts
    WHERE id = p_id;
END$$

DELIMITER ;
