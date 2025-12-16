-- ============================================================================
-- Stored Procedure: sp_GetDepartments
-- Description: Retrieve active departments for dropdown population
-- Feature: User Authentication & Login System (001-user-login)
-- Created: December 16, 2025
-- ============================================================================

USE mtm_receiving_db;

DROP PROCEDURE IF EXISTS sp_GetDepartments;

DELIMITER $$

CREATE PROCEDURE sp_GetDepartments()
BEGIN
    -- Get list of active departments ordered by sort_order
    -- Used to populate department dropdown in New User Setup dialog
    
    SELECT 
        department_id,
        department_name,
        sort_order
    FROM departments
    WHERE is_active = TRUE
    ORDER BY sort_order ASC, department_name ASC;
    
END$$

DELIMITER ;

-- ============================================================================
-- Test Query (comment out in production)
-- ============================================================================
-- CALL sp_GetDepartments();
