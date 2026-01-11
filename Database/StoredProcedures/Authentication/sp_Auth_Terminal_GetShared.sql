-- ============================================================================
-- Stored Procedure: sp_Auth_Terminal_GetShared
-- Description: Retrieve list of shared terminal computer names
-- Feature: User Authentication & Login System (002-user-login)
-- Created: December 16, 2025
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Auth_Terminal_GetShared`;

DELIMITER $$

CREATE PROCEDURE `sp_Auth_Terminal_GetShared`()
BEGIN
    -- Get list of active shared terminal workstation names
    -- Used for workstation type detection during startup

    SELECT
        workstation_name,
        workstation_type,
        description
    FROM auth_workstation_config
    WHERE workstation_type = 'shared_terminal'
      AND is_active = TRUE
    ORDER BY workstation_name;

END$$

DELIMITER ;

-- ============================================================================
-- Test Query (comment out in production)
-- ============================================================================
-- CALL sp_Auth_Terminal_GetShared();
