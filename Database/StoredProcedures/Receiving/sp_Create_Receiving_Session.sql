-- =============================================
-- Stored Procedure: sp_Create_Receiving_Session
-- Feature: Receiving Workflow Consolidation
-- Purpose: Initialize a new workflow session
-- Returns: Session ID and creation timestamp
-- =============================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Create_Receiving_Session$$

CREATE PROCEDURE sp_Create_Receiving_Session(
    IN p_session_id CHAR(36),
    IN p_po_number VARCHAR(50),
    IN p_part_id INT,
    IN p_load_count INT
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Failed to create receiving session';
    END;
    
    START TRANSACTION;
    
    -- Insert new session
    INSERT INTO receiving_workflow_sessions (
        session_id, 
        po_number, 
        part_id, 
        load_count, 
        current_step,
        has_unsaved_changes
    ) VALUES (
        p_session_id, 
        p_po_number, 
        p_part_id, 
        p_load_count, 
        1,  -- Start at step 1
        TRUE
    );
    
    COMMIT;
    
    -- Return session details
    SELECT 
        session_id, 
        created_at,
        current_step,
        po_number,
        part_id,
        load_count
    FROM receiving_workflow_sessions 
    WHERE session_id = p_session_id;
END$$

DELIMITER ;
