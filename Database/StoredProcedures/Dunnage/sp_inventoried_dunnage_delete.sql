DELIMITER $$

DROP PROCEDURE IF EXISTS sp_inventoried_dunnage_delete$$

CREATE PROCEDURE sp_inventoried_dunnage_delete(
    IN p_id INT
)
BEGIN
    DELETE FROM inventoried_dunnage
    WHERE id = p_id;
END$$

DELIMITER ;
