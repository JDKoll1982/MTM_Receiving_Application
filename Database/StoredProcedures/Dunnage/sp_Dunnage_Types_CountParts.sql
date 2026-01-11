DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Types_CountParts`$$

CREATE PROCEDURE `sp_Dunnage_Types_CountParts`(
    IN p_type_id INT
)
BEGIN
    SELECT COUNT(*) as part_count
    FROM dunnage_parts
    WHERE type_id = p_type_id;
END$$

DELIMITER ;
