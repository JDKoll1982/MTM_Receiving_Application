DELIMITER $$

DROP PROCEDURE IF EXISTS sp_SoftwareVersion_GetCurrent $$
CREATE PROCEDURE sp_SoftwareVersion_GetCurrent()
BEGIN
    SELECT
        id,
        required_version,
        created_at,
        updated_at,
        updated_by
    FROM software_version
    WHERE id = 1
    LIMIT 1;
END $$

DELIMITER ;