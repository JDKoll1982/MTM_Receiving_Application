-- =============================================
-- Stored Procedure: sp_Dunnage_Types_GetUsageCount
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get Package Type Usage Count
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Dunnage_Types_GetUsageCount`$$
CREATE PROCEDURE `sp_Dunnage_Types_GetUsageCount`(
    IN p_id INT
)
BEGIN
    SELECT COUNT(*) AS usage_count
    FROM receiving_package_type_mapping
    WHERE package_type = (SELECT type_name FROM dunnage_types WHERE id = p_id)
      AND is_active = TRUE;
END$$



DELIMITER ;
