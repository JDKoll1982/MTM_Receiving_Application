DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_specs_delete_by_id$$

CREATE PROCEDURE sp_dunnage_specs_delete_by_id(
    IN p_id INT
)
BEGIN
    DELETE FROM dunnage_specs
    WHERE id = p_id;
END$$

DELIMITER ;
