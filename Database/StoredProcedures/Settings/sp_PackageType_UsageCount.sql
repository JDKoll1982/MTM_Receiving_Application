-- =============================================
-- Stored Procedure: sp_PackageType_UsageCount
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get Package Type Usage Count
-- =============================================
DROP PROCEDURE IF EXISTS sp_PackageType_UsageCount$$
CREATE PROCEDURE sp_PackageType_UsageCount(
    IN p_id INT
)
BEGIN
    SELECT COUNT(*) AS usage_count
    FROM receiving_package_type_mapping
    WHERE package_type = (SELECT name FROM dunnage_types WHERE id = p_id)
      AND is_active = TRUE;
END$$

-- =============================================
-- ROUTING RULES CRUD OPERATIONS
-- =============================================



DELIMITER ;
