-- =============================================
-- Stored Procedure: sp_Save_Completed_Transaction
-- Feature: Receiving Workflow Consolidation
-- Purpose: Save workflow session to completed transactions table
-- Note: Copies all session loads to permanent storage
-- =============================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Save_Completed_Transaction$$

CREATE PROCEDURE sp_Save_Completed_Transaction(
    IN p_session_id CHAR(36),
    IN p_csv_file_path VARCHAR(500),
    IN p_created_by VARCHAR(50)
)
BEGIN
    DECLARE v_rows_inserted INT DEFAULT 0;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Failed to save completed transaction';
    END;
    
    START TRANSACTION;
    
    -- Insert all loads from session into completed transactions
    INSERT INTO receiving_completed_transactions (
        session_id, 
        po_number, 
        part_id, 
        part_number, 
        load_number,
        weight_or_quantity, 
        heat_lot, 
        package_type, 
        packages_per_load,
        csv_file_path, 
        created_by
    )
    SELECT 
        s.session_id, 
        s.po_number, 
        s.part_id, 
        s.part_number, 
        l.load_number,
        l.weight_or_quantity, 
        l.heat_lot, 
        l.package_type, 
        l.packages_per_load,
        p_csv_file_path, 
        p_created_by
    FROM receiving_workflow_sessions s
    INNER JOIN receiving_load_details l ON s.session_id = l.session_id
    WHERE s.session_id = p_session_id
    ORDER BY l.load_number;
    
    SET v_rows_inserted = ROW_COUNT();
    
    -- Update session as saved
    UPDATE receiving_workflow_sessions
    SET is_saved = TRUE, 
        saved_at = CURRENT_TIMESTAMP, 
        saved_csv_path = p_csv_file_path,
        has_unsaved_changes = FALSE
    WHERE session_id = p_session_id;
    
    COMMIT;
    
    -- Return result summary
    SELECT 
        v_rows_inserted as rows_saved,
        p_session_id as session_id,
        p_csv_file_path as csv_file_path,
        CURRENT_TIMESTAMP as saved_at;
END$$

DELIMITER ;
