-- =============================================
-- Stored Procedure: sp_Dunnage_Types_GetAll
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get All Package Types
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Dunnage_Types_GetAll`$$
CREATE PROCEDURE `sp_Dunnage_Types_GetAll`()
BEGIN
    SELECT
        *
    FROM dunnage_types
    ORDER BY id;
END$$

DELIMITER ;
