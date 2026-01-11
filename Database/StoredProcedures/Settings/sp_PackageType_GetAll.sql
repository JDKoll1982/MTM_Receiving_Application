-- =============================================
-- Stored Procedure: sp_PackageType_GetAll
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get All Package Types
-- =============================================
DROP PROCEDURE IF EXISTS sp_PackageType_GetAll$$
CREATE PROCEDURE sp_PackageType_GetAll()
BEGIN
    SELECT 
        id,
        name,
        code,
        is_active,
        created_at,
        updated_at,
        created_by
    FROM package_types
    WHERE is_active = TRUE
    ORDER BY name;
END$$



DELIMITER ;
