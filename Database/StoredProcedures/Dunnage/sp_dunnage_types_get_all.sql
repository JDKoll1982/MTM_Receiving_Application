DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_types_get_all$$

CREATE PROCEDURE sp_dunnage_types_get_all()
BEGIN
    SELECT
        id,
        type_name,
        icon,
        created_by,
        created_date,
        modified_by,
        modified_date
    FROM dunnage_types
    ORDER BY type_name;
END$$

DELIMITER ;
