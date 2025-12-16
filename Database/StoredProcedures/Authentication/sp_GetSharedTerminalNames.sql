-- ============================================================================
-- Stored Procedure: sp_GetSharedTerminalNames
-- Description: Retrieve list of shared terminal computer names
-- Feature: User Authentication & Login System (001-user-login)
-- Created: December 16, 2025
-- ============================================================================

USE mtm_receiving_db;

DROP PROCEDURE IF EXISTS sp_GetSharedTerminalNames;

DELIMITER $$

CREATE PROCEDURE sp_GetSharedTerminalNames()
BEGIN
    -- Get list of active shared terminal workstation names
    -- Used for workstation type detection during startup
    
    SELECT 
        workstation_name,
        workstation_type,
        description
    FROM workstation_config
    WHERE workstation_type = 'shared_terminal'
      AND is_active = TRUE
    ORDER BY workstation_name;
    
END$$

DELIMITER ;

-- ============================================================================
-- Test Query (comment out in production)
-- ============================================================================
-- CALL sp_GetSharedTerminalNames();
