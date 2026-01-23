-- Quality Hold Tracking: Stored Procedure
-- Procedure: sp_Receiving_QualityHolds_Insert
-- Purpose: Insert a quality hold record for restricted part numbers (MMFSR, MMCSR)

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Receiving_QualityHolds_Insert` $$

CREATE PROCEDURE `sp_Receiving_QualityHolds_Insert`(
    IN p_LoadID INT,
    IN p_PartID VARCHAR(50),
    IN p_RestrictionType VARCHAR(50),
    IN p_QualityAcknowledgedBy VARCHAR(255),
    IN p_QualityAcknowledgedAt DATETIME,
    OUT p_QualityHoldID INT,
    OUT p_Status INT,
    OUT p_ErrorMsg VARCHAR(500)
)
BEGIN
    DECLARE v_RowCount INT;

    SET p_Status = 1;
    SET p_ErrorMsg = '';
    SET p_QualityHoldID = -1;

    -- Validate inputs
    IF p_LoadID IS NULL OR p_LoadID <= 0 THEN
        SET p_Status = 0;
        SET p_ErrorMsg = 'LoadID is required and must be positive';
    ELSEIF p_PartID IS NULL OR p_PartID = '' THEN
        SET p_Status = 0;
        SET p_ErrorMsg = 'PartID is required';
    ELSEIF p_RestrictionType IS NULL OR p_RestrictionType = '' THEN
        SET p_Status = 0;
        SET p_ErrorMsg = 'RestrictionType is required';
    ELSE
        -- Insert quality hold record
        INSERT INTO receiving_quality_holds (
            load_id,
            part_id,
            restriction_type,
            quality_acknowledged_by,
            quality_acknowledged_at,
            created_at
        ) VALUES (
            p_LoadID,
            p_PartID,
            p_RestrictionType,
            p_QualityAcknowledgedBy,
            p_QualityAcknowledgedAt,
            NOW()
        );

        SET v_RowCount = ROW_COUNT();

        IF v_RowCount > 0 THEN
            SET p_QualityHoldID = LAST_INSERT_ID();
            SET p_Status = 1;
        ELSE
            SET p_Status = 0;
            SET p_ErrorMsg = 'Failed to insert quality hold record';
        END IF;
    END IF;

END $$

DELIMITER ;
