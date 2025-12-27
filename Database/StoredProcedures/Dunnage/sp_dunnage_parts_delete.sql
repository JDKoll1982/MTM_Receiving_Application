DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_parts_delete$$

CREATE PROCEDURE sp_dunnage_parts_delete(
    IN p_id INT
)
BEGIN
    DELETE FROM dunnage_parts
    WHERE id = p_id;
END$$

DELIMITER ;
