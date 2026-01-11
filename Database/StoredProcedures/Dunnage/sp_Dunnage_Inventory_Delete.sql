DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Inventory_Delete`$$

CREATE PROCEDURE `sp_Dunnage_Inventory_Delete`(
    IN p_id INT
)
BEGIN
    DELETE FROM dunnage_requires_inventory
    WHERE id = p_id;
END$$

DELIMITER ;
