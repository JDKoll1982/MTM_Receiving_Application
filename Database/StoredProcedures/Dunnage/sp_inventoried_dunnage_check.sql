DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_requires_inventory_check$$

CREATE PROCEDURE sp_dunnage_requires_inventory_check(
    IN p_part_id VARCHAR(50)
)
BEGIN
    SELECT COUNT(*) > 0 as requires_inventory
    FROM dunnage_requires_inventory
    WHERE part_id = p_part_id;
END$$

DELIMITER ;
