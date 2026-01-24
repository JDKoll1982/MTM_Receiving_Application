-- =============================================
-- Stored Procedure: sp_Get_Session_With_Loads
-- Feature: Receiving Workflow Consolidation
-- Purpose: Retrieve session and all associated load details
-- Returns: Two result sets - session data and load details
-- =============================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Get_Session_With_Loads$$

CREATE PROCEDURE sp_Get_Session_With_Loads(
    IN p_session_id CHAR(36)
)
BEGIN
    -- Result set 1: Session data
    SELECT 
        session_id,
        created_at,
        last_modified_at,
        current_step,
        is_edit_mode,
        has_unsaved_changes,
        po_number,
        part_id,
        part_number,
        part_description,
        load_count,
        copy_source_load_number,
        last_copy_operation_at,
        is_saved,
        saved_at,
        saved_csv_path
    FROM receiving_workflow_sessions 
    WHERE session_id = p_session_id;
    
    -- Result set 2: All load details (ordered by load number)
    SELECT 
        id,
        session_id,
        load_number,
        weight_or_quantity,
        heat_lot,
        package_type,
        packages_per_load,
        is_weight_auto_filled,
        is_heat_lot_auto_filled,
        is_package_type_auto_filled,
        is_packages_per_load_auto_filled,
        created_at,
        updated_at
    FROM receiving_load_details 
    WHERE session_id = p_session_id 
    ORDER BY load_number;
END$$

DELIMITER ;
