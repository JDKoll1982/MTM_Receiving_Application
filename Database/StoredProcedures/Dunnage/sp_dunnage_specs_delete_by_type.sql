DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_specs_delete_by_type$$

CREATE PROCEDURE sp_dunnage_specs_delete_by_type(
    IN p_type_id INT
)
BEGIN
    DELETE FROM dunnage_specs
    WHERE type_id = p_type_id;
END$$

DELIMITER ;
