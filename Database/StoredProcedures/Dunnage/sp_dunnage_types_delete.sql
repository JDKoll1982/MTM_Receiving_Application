DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_types_delete$$

CREATE PROCEDURE sp_dunnage_types_delete(
    IN p_id INT
)
BEGIN
    DELETE FROM dunnage_types
    WHERE id = p_id;
END$$

DELIMITER ;
