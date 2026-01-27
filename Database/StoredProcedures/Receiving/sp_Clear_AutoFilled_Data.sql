-- =============================================
-- Stored Procedure: sp_Clear_AutoFilled_Data
-- Feature: Receiving Workflow Consolidation
-- Purpose: Clear auto-filled data for specified fields
-- Note: Only clears fields marked as auto-filled, preserves user input
-- =============================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Clear_AutoFilled_Data$$

CREATE PROCEDURE sp_Clear_AutoFilled_Data(
    IN p_session_id CHAR(36),
    IN p_fields_to_clear VARCHAR(50)  -- 'ALL', 'WEIGHT', 'HEAT_LOT', 'PACKAGE_TYPE', 'PACKAGES'
)
BEGIN
    DECLARE v_rows_affected INT DEFAULT 0;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Failed to clear auto-filled data';
    END;
    
    START TRANSACTION;
    
    -- Clear ALL auto-filled fields
    IF p_fields_to_clear = 'ALL' THEN
        UPDATE receiving_load_details
        SET weight_or_quantity = IF(is_weight_auto_filled, NULL, weight_or_quantity),
            heat_lot = IF(is_heat_lot_auto_filled, NULL, heat_lot),
            package_type = IF(is_package_type_auto_filled, NULL, package_type),
            packages_per_load = IF(is_packages_per_load_auto_filled, NULL, packages_per_load),
            is_weight_auto_filled = FALSE,
            is_heat_lot_auto_filled = FALSE,
            is_package_type_auto_filled = FALSE,
            is_packages_per_load_auto_filled = FALSE,
            updated_at = CURRENT_TIMESTAMP
        WHERE session_id = p_session_id
          AND (is_weight_auto_filled OR is_heat_lot_auto_filled 
               OR is_package_type_auto_filled OR is_packages_per_load_auto_filled);
    
    -- Clear WEIGHT only
    ELSEIF p_fields_to_clear = 'WEIGHT' THEN
        UPDATE receiving_load_details
        SET weight_or_quantity = NULL,
            is_weight_auto_filled = FALSE,
            updated_at = CURRENT_TIMESTAMP
        WHERE session_id = p_session_id AND is_weight_auto_filled = TRUE;
    
    -- Clear HEAT_LOT only
    ELSEIF p_fields_to_clear = 'HEAT_LOT' THEN
        UPDATE receiving_load_details
        SET heat_lot = NULL,
            is_heat_lot_auto_filled = FALSE,
            updated_at = CURRENT_TIMESTAMP
        WHERE session_id = p_session_id AND is_heat_lot_auto_filled = TRUE;
    
    -- Clear PACKAGE_TYPE only
    ELSEIF p_fields_to_clear = 'PACKAGE_TYPE' THEN
        UPDATE receiving_load_details
        SET package_type = NULL,
            is_package_type_auto_filled = FALSE,
            updated_at = CURRENT_TIMESTAMP
        WHERE session_id = p_session_id AND is_package_type_auto_filled = TRUE;
    
    -- Clear PACKAGES only
    ELSEIF p_fields_to_clear = 'PACKAGES' THEN
        UPDATE receiving_load_details
        SET packages_per_load = NULL,
            is_packages_per_load_auto_filled = FALSE,
            updated_at = CURRENT_TIMESTAMP
        WHERE session_id = p_session_id AND is_packages_per_load_auto_filled = TRUE;
    END IF;
    
    SET v_rows_affected = ROW_COUNT();
    
    -- Update session metadata
    UPDATE receiving_workflow_sessions
    SET last_modified_at = CURRENT_TIMESTAMP,
        has_unsaved_changes = TRUE
    WHERE session_id = p_session_id;
    
    COMMIT;
    
    -- Return operation summary
    SELECT 
        v_rows_affected as loads_cleared,
        p_fields_to_clear as fields_cleared,
        CURRENT_TIMESTAMP as operation_time;
END$$

DELIMITER ;
