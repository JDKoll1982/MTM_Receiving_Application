-- ============================================================================
-- Stored Procedure: sp_routing_user_preference_get
-- Description: Retrieve user preferences for routing module
-- Parameters:
--   @p_employee_number: Employee to retrieve preferences for
-- Returns: User preference record from settings_routing_personal
-- Feature: Routing Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_routing_user_preference_get`;

DELIMITER $$

CREATE PROCEDURE `sp_routing_user_preference_get`(
    IN p_employee_number INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    DECLARE v_count INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        SET p_status = -1;
        SET p_error_msg = 'Unexpected SQL exception';
    END;

    IF p_employee_number IS NULL THEN
        SET p_status = -1;
        SET p_error_msg = 'Employee number is required';
    ELSE

        -- Return the matching row (if any)
        SELECT
            id,
            employee_number,
            default_mode,
            enable_validation,
            updated_date
        FROM settings_routing_personal
        WHERE employee_number = p_employee_number;

        -- Determine whether a row was found and set status/message accordingly
        SELECT COUNT(*) INTO v_count
        FROM settings_routing_personal
        WHERE employee_number = p_employee_number;

        IF v_count = 0 THEN
            SET p_status = 0;
            SET p_error_msg = 'No user preferences found';
        ELSE
            SET p_status = 1;
            SET p_error_msg = 'User preferences retrieved';
        END IF;
    END IF;
END $$

DELIMITER ;
