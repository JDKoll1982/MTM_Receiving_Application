-- Quality Hold Tracking: Stored Procedure
-- Procedure: sp_Receiving_QualityHolds_Update
-- Purpose: Update quality hold acknowledgment status

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Receiving_QualityHolds_Update` $$

CREATE PROCEDURE `sp_Receiving_QualityHolds_Update`(
    IN p_QualityHoldID INT,
    IN p_QualityAcknowledgedBy VARCHAR(255),
    IN p_QualityAcknowledgedAt DATETIME,
    OUT p_Status INT,
    OUT p_ErrorMsg VARCHAR(500)
)
BEGIN
    DECLARE v_RowCount INT;

    SET p_Status = 1;
    SET p_ErrorMsg = '';

    -- Validate inputs
    IF p_QualityHoldID IS NULL OR p_QualityHoldID <= 0 THEN
        SET p_Status = 0;
        SET p_ErrorMsg = 'QualityHoldID is required and must be positive';
    ELSE
        -- Update quality hold acknowledgment
        UPDATE receiving_quality_holds
        SET
            quality_acknowledged_by = p_QualityAcknowledgedBy,
            quality_acknowledged_at = p_QualityAcknowledgedAt,
            updated_at = NOW()
        WHERE quality_hold_id = p_QualityHoldID;

        SET v_RowCount = ROW_COUNT();

        IF v_RowCount > 0 THEN
            SET p_Status = 1;
        ELSE
            SET p_Status = 0;
            SET p_ErrorMsg = 'Quality hold record not found';
        END IF;
    END IF;

END $$

DELIMITER ;
