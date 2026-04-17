DELIMITER $$

DROP PROCEDURE IF EXISTS sp_SoftwareVersion_Upsert $$
CREATE PROCEDURE sp_SoftwareVersion_Upsert(
    IN p_required_version VARCHAR(50),
    IN p_updated_by VARCHAR(100)
)
BEGIN
    INSERT INTO software_version (
        id,
        required_version,
        updated_by
    ) VALUES (
        1,
        p_required_version,
        p_updated_by
    )
    ON DUPLICATE KEY UPDATE
        required_version = VALUES(required_version),
        updated_by = VALUES(updated_by),
        updated_at = NOW();
END $$

DELIMITER ;