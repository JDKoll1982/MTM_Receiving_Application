-- =============================================
-- Stored Procedure: sp_dunnage_type_GetAll
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get All Package Types
-- =============================================
DROP PROCEDURE IF EXISTS sp_dunnage_type_GetAll$$
CREATE PROCEDURE sp_dunnage_type_GetAll()
BEGIN
    SELECT
        *
    FROM dunnage_types
    WHERE is_active = TRUE
    ORDER BY id;
END$$

DELIMITER ;
