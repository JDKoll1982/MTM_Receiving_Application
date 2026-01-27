DROP PROCEDURE IF EXISTS sp_Update_Receiving_Session;

DELIMITER $$

CREATE PROCEDURE sp_Update_Receiving_Session(
    IN p_SessionId VARCHAR(36),
    IN p_CurrentStep INT,
    IN p_PONumber VARCHAR(50),
    IN p_PartId INT,
    IN p_LoadCount INT,
    OUT p_Status INT,
    OUT p_ErrorMsg VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_ErrorMsg = MESSAGE_TEXT;
        SET p_Status = 1;
        ROLLBACK;
    END;

    START TRANSACTION;

    -- Validate session exists
    IF NOT EXISTS (SELECT 1 FROM receiving_workflow_sessions WHERE SessionId = p_SessionId) THEN
        SET p_Status = 1;
        SET p_ErrorMsg = CONCAT('Session ', p_SessionId, ' not found');
        ROLLBACK;
    ELSE
        -- Update session
        UPDATE receiving_workflow_sessions
        SET 
            CurrentStep = p_CurrentStep,
            PONumber = NULLIF(p_PONumber, ''),
            PartId = NULLIF(p_PartId, 0),
            LoadCount = NULLIF(p_LoadCount, 0),
            UpdatedAt = NOW()
        WHERE SessionId = p_SessionId;

        SET p_Status = 0;
        SET p_ErrorMsg = NULL;
        COMMIT;
    END IF;
END$$

DELIMITER ;
