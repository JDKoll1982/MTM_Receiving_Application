DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_loads_delete$$

CREATE PROCEDURE sp_dunnage_loads_delete(
    IN p_load_uuid CHAR(36)
)
BEGIN
    DELETE FROM dunnage_loads
    WHERE load_uuid = p_load_uuid;
END$$

DELIMITER ;
