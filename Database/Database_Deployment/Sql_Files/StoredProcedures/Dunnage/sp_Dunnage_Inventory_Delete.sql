DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Inventory_Delete`$$

CREATE PROCEDURE `sp_Dunnage_Inventory_Delete`(
    IN p_id INT
)
BEGIN
    SET FOREIGN_KEY_CHECKS = 0;
    DELETE FROM dunnage_requires_inventory
    WHERE id = p_id;
    SET FOREIGN_KEY_CHECKS = 1;
END $$

DELIMITER ;
