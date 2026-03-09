-- ============================================================================
-- Stored Procedure: sp_Auth_User_GetAll
-- Description: Return all auth_users rows ordered by full_name
-- Feature: User Management UI (Module_Bulk_Inventory Phase 0)
-- Created: 2026-03-08
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Auth_User_GetAll`;

DELIMITER $$

CREATE PROCEDURE `sp_Auth_User_GetAll`()
BEGIN
    SELECT
        employee_number,
        windows_username,
        full_name,
        pin,
        department,
        shift,
        is_active,
        visual_username,
        visual_password,
        default_receiving_mode,
        default_dunnage_mode,
        created_date,
        created_by,
        modified_date
    FROM auth_users
    ORDER BY full_name ASC;
END$$

DELIMITER ;

-- ============================================================================
-- Test Query (comment out in production)
-- ============================================================================
/*
CALL sp_Auth_User_GetAll();
*/
