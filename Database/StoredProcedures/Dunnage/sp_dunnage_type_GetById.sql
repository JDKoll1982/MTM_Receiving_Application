-- =============================================
-- Stored Procedure: sp_dunnage_type_GetById
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get Package Type by ID
-- =============================================
DROP PROCEDURE IF EXISTS sp_dunnage_type_GetById$$
CREATE PROCEDURE sp_dunnage_type_GetById(
    IN p_id INT
)
BEGIN
    SELECT
        *
    FROM dunnage_types
    WHERE id = p_id;
END$$

DELIMITER ;
