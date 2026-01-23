-- Quality Hold Tracking: Stored Procedure
-- Procedure: sp_Receiving_QualityHolds_GetByLoadID
-- Purpose: Retrieve all quality holds for a specific load

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Receiving_QualityHolds_GetByLoadID` $$

CREATE PROCEDURE `sp_Receiving_QualityHolds_GetByLoadID`(
    IN p_LoadID INT,
    OUT p_Status INT,
    OUT p_ErrorMsg VARCHAR(500)
)
BEGIN
    SET p_Status = 1;
    SET p_ErrorMsg = '';

    -- Validate input and return empty result set if invalid
    IF p_LoadID IS NULL OR p_LoadID <= 0 THEN
        SET p_Status = 0;
        SET p_ErrorMsg = 'LoadID is required and must be positive';
        SELECT NULL AS quality_hold_id LIMIT 0;
    ELSE
        -- Retrieve quality holds for the load
        SELECT
            quality_hold_id,
            load_id,
            part_id,
            restriction_type,
            quality_acknowledged_by,
            quality_acknowledged_at,
            created_at
        FROM receiving_quality_holds
        WHERE load_id = p_LoadID
        ORDER BY created_at DESC;
    END IF;

END $$

DELIMITER ;
