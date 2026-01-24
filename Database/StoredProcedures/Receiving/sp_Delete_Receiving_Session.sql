DROP PROCEDURE IF EXISTS sp_Delete_Receiving_Session;

DELIMITER $$

CREATE PROCEDURE sp_Delete_Receiving_Session(
    IN p_SessionId VARCHAR(36),
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
        -- Delete load details first (FK constraint)
        DELETE FROM receiving_load_details WHERE SessionId = p_SessionId;
        
        -- Delete session
        DELETE FROM receiving_workflow_sessions WHERE SessionId = p_SessionId;

        SET p_Status = 0;
        SET p_ErrorMsg = NULL;
        COMMIT;
    END IF;
END$$

DELIMITER ;
