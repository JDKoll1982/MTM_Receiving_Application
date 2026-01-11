-- =============================================
-- Stored Procedure: sp_PackageType_Insert
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Insert Package Type
-- =============================================
DROP PROCEDURE IF EXISTS sp_PackageType_Insert$$
CREATE PROCEDURE sp_PackageType_Insert(
    IN p_name VARCHAR(50),
    IN p_code VARCHAR(20),
    IN p_created_by INT
)
BEGIN
    -- Check for duplicate name
    IF EXISTS (SELECT 1 FROM dunnage_types WHERE name = p_name AND is_active = TRUE) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Package type name already exists';
    END IF;

    -- Check for duplicate code
    IF EXISTS (SELECT 1 FROM dunnage_types WHERE code = p_code AND is_active = TRUE) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Package type code already exists';
    END IF;

    INSERT INTO dunnage_types (name, code, is_active, created_by)
    VALUES (p_name, p_code, TRUE, p_created_by);

    SELECT LAST_INSERT_ID() AS id;
END$$



DELIMITER ;
