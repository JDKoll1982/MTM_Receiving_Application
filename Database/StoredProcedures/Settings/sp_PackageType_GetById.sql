-- =============================================
-- Stored Procedure: sp_PackageType_GetById
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get Package Type by ID
-- =============================================
DROP PROCEDURE IF EXISTS sp_PackageType_GetById$$
CREATE PROCEDURE sp_PackageType_GetById(
    IN p_id INT
)
BEGIN
    SELECT
        id,
        name,
        code,
        is_active,
        created_at,
        updated_at,
        created_by
    FROM dunnage_types
    WHERE id = p_id;
END$$



DELIMITER ;
