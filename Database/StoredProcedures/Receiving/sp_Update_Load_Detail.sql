-- =============================================
-- Stored Procedure: sp_Update_Load_Detail
-- Feature: Receiving Workflow Consolidation
-- Purpose: Insert or update a single load detail record
-- Note: Uses ON DUPLICATE KEY UPDATE for upsert behavior
-- =============================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Update_Load_Detail$$

CREATE PROCEDURE sp_Update_Load_Detail(
    IN p_session_id CHAR(36),
    IN p_load_number INT,
    IN p_weight_or_quantity DECIMAL(10,3),
    IN p_heat_lot VARCHAR(50),
    IN p_package_type VARCHAR(50),
    IN p_packages_per_load INT
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Failed to update load detail';
    END;
    
    START TRANSACTION;
    
    -- Upsert load detail
    INSERT INTO receiving_load_details (
        session_id, 
        load_number, 
        weight_or_quantity, 
        heat_lot, 
        package_type, 
        packages_per_load
    ) VALUES (
        p_session_id, 
        p_load_number, 
        p_weight_or_quantity, 
        p_heat_lot,
        p_package_type, 
        p_packages_per_load
    )
    ON DUPLICATE KEY UPDATE
        weight_or_quantity = VALUES(weight_or_quantity),
        heat_lot = VALUES(heat_lot),
        package_type = VALUES(package_type),
        packages_per_load = VALUES(packages_per_load),
        updated_at = CURRENT_TIMESTAMP;
    
    -- Update session last modified
    UPDATE receiving_workflow_sessions
    SET last_modified_at = CURRENT_TIMESTAMP,
        has_unsaved_changes = TRUE
    WHERE session_id = p_session_id;
    
    COMMIT;
    
    -- Return updated record
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
        updated_at
    FROM receiving_load_details
    WHERE session_id = p_session_id AND load_number = p_load_number;
END$$

DELIMITER ;
