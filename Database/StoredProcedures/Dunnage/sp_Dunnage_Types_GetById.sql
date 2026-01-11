-- =============================================
-- Stored Procedure: sp_Dunnage_Types_GetById
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get Package Type by ID
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Dunnage_Types_GetById`$$
CREATE PROCEDURE `sp_Dunnage_Types_GetById`(
    IN p_id INT
)
BEGIN
    SELECT
        *
    FROM dunnage_types
    WHERE id = p_id;
END$$

DELIMITER ;
