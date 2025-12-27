DELIMITER $$

DROP PROCEDURE IF EXISTS sp_inventoried_dunnage_check$$

CREATE PROCEDURE sp_inventoried_dunnage_check(
    IN p_part_id VARCHAR(50)
)
BEGIN
    SELECT COUNT(*) > 0 as requires_inventory
    FROM inventoried_dunnage
    WHERE part_id = p_part_id;
END$$

DELIMITER ;
