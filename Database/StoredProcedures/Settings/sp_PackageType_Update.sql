-- =============================================
-- Stored Procedure: sp_PackageType_Update
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Update Package Type
-- =============================================
DROP PROCEDURE IF EXISTS sp_PackageType_Update$$
CREATE PROCEDURE sp_PackageType_Update(
    IN p_id INT,
    IN p_name VARCHAR(50),
    IN p_code VARCHAR(20)
)
BEGIN
    -- Check for duplicate name (excluding self)
    IF EXISTS (SELECT 1 FROM dunnage_types WHERE name = p_name AND id != p_id AND is_active = TRUE) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Package type name already exists';
    END IF;

    -- Check for duplicate code (excluding self)
    IF EXISTS (SELECT 1 FROM dunnage_types WHERE code = p_code AND id != p_id AND is_active = TRUE) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Package type code already exists';
    END IF;

    UPDATE dunnage_types
    SET name = p_name,
        code = p_code,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_id;

    SELECT ROW_COUNT() AS affected_rows;
END$$



DELIMITER ;
