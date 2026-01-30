-- =============================================
-- Stored Procedure: sp_Copy_To_Loads
-- Feature: Receiving Workflow Consolidation
-- Purpose: Bulk copy field values from source load to target loads
-- Note: Preserves existing user data (only overwrites auto-filled fields)
-- =============================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Copy_To_Loads$$

CREATE PROCEDURE sp_Copy_To_Loads(
    IN p_session_id CHAR(36),
    IN p_source_load_number INT,
    IN p_target_load_numbers TEXT,  -- Comma-separated list, or NULL for all
    IN p_fields_to_copy VARCHAR(50)  -- 'ALL', 'WEIGHT', 'HEAT_LOT', 'PACKAGE_TYPE', 'PACKAGES'
)
BEGIN
    DECLARE v_weight DECIMAL(10,3);
    DECLARE v_heat_lot VARCHAR(50);
    DECLARE v_package_type VARCHAR(50);
    DECLARE v_packages INT;
    DECLARE v_cells_copied INT DEFAULT 0;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Failed to copy data to loads';
    END;
    
    START TRANSACTION;
    
    -- Get source load data
    SELECT 
        weight_or_quantity,
        heat_lot,
        package_type,
        packages_per_load
    INTO
        v_weight,
        v_heat_lot,
        v_package_type,
        v_packages
    FROM receiving_load_details
    WHERE session_id = p_session_id AND load_number = p_source_load_number;
    
    -- Build target list (all loads except source if NULL provided)
    IF p_target_load_numbers IS NULL OR p_target_load_numbers = '' THEN
        SET @target_clause = CONCAT('load_number != ', p_source_load_number);
    ELSE
        SET @target_clause = CONCAT('load_number IN (', p_target_load_numbers, ')');
    END IF;
    
    -- Copy ALL fields
    IF p_fields_to_copy = 'ALL' THEN
        SET @update_sql = CONCAT(
            'UPDATE receiving_load_details SET ',
            'weight_or_quantity = IF(is_weight_auto_filled OR weight_or_quantity IS NULL, ', IFNULL(v_weight, 'NULL'), ', weight_or_quantity), ',
            'heat_lot = IF(is_heat_lot_auto_filled OR heat_lot IS NULL, ', QUOTE(IFNULL(v_heat_lot, '')), ', heat_lot), ',
            'package_type = IF(is_package_type_auto_filled OR package_type IS NULL, ', QUOTE(IFNULL(v_package_type, '')), ', package_type), ',
            'packages_per_load = IF(is_packages_per_load_auto_filled OR packages_per_load IS NULL, ', IFNULL(v_packages, 'NULL'), ', packages_per_load), ',
            'is_weight_auto_filled = TRUE, ',
            'is_heat_lot_auto_filled = TRUE, ',
            'is_package_type_auto_filled = TRUE, ',
            'is_packages_per_load_auto_filled = TRUE, ',
            'updated_at = CURRENT_TIMESTAMP ',
            'WHERE session_id = ', QUOTE(p_session_id), ' AND ', @target_clause
        );
        
    -- Copy WEIGHT only
    ELSEIF p_fields_to_copy = 'WEIGHT' THEN
        SET @update_sql = CONCAT(
            'UPDATE receiving_load_details SET ',
            'weight_or_quantity = IF(is_weight_auto_filled OR weight_or_quantity IS NULL, ', IFNULL(v_weight, 'NULL'), ', weight_or_quantity), ',
            'is_weight_auto_filled = TRUE, ',
            'updated_at = CURRENT_TIMESTAMP ',
            'WHERE session_id = ', QUOTE(p_session_id), ' AND ', @target_clause
        );
        
    -- Copy HEAT_LOT only
    ELSEIF p_fields_to_copy = 'HEAT_LOT' THEN
        SET @update_sql = CONCAT(
            'UPDATE receiving_load_details SET ',
            'heat_lot = IF(is_heat_lot_auto_filled OR heat_lot IS NULL, ', QUOTE(IFNULL(v_heat_lot, '')), ', heat_lot), ',
            'is_heat_lot_auto_filled = TRUE, ',
            'updated_at = CURRENT_TIMESTAMP ',
            'WHERE session_id = ', QUOTE(p_session_id), ' AND ', @target_clause
        );
        
    -- Copy PACKAGE_TYPE only
    ELSEIF p_fields_to_copy = 'PACKAGE_TYPE' THEN
        SET @update_sql = CONCAT(
            'UPDATE receiving_load_details SET ',
            'package_type = IF(is_package_type_auto_filled OR package_type IS NULL, ', QUOTE(IFNULL(v_package_type, '')), ', package_type), ',
            'is_package_type_auto_filled = TRUE, ',
            'updated_at = CURRENT_TIMESTAMP ',
            'WHERE session_id = ', QUOTE(p_session_id), ' AND ', @target_clause
        );
        
    -- Copy PACKAGES only
    ELSEIF p_fields_to_copy = 'PACKAGES' THEN
        SET @update_sql = CONCAT(
            'UPDATE receiving_load_details SET ',
            'packages_per_load = IF(is_packages_per_load_auto_filled OR packages_per_load IS NULL, ', IFNULL(v_packages, 'NULL'), ', packages_per_load), ',
            'is_packages_per_load_auto_filled = TRUE, ',
            'updated_at = CURRENT_TIMESTAMP ',
            'WHERE session_id = ', QUOTE(p_session_id), ' AND ', @target_clause
        );
    END IF;
    
    -- Execute dynamic update
    PREPARE stmt FROM @update_sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
    
    SET v_cells_copied = ROW_COUNT();
    
    -- Update session metadata
    UPDATE receiving_workflow_sessions
    SET copy_source_load_number = p_source_load_number,
        last_copy_operation_at = CURRENT_TIMESTAMP,
        last_modified_at = CURRENT_TIMESTAMP,
        has_unsaved_changes = TRUE
    WHERE session_id = p_session_id;
    
    COMMIT;
    
    -- Return operation summary
    SELECT 
        p_source_load_number as source_load_number,
        v_cells_copied as loads_updated,
        p_fields_to_copy as fields_copied,
        CURRENT_TIMESTAMP as operation_time;
END$$

DELIMITER ;
