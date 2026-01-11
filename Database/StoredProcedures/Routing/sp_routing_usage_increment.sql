-- ============================================================================
-- Stored Procedure: sp_routing_usage_increment
-- Description: Increment usage counter for employee-recipient pair
-- Parameters:
--   @p_employee_number: Employee tracking usage
--   @p_recipient_id: Recipient being used
-- Action: Inserts new tracking row or increments existing usage_count
-- Feature: Routing Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_routing_usage_increment`;

DELIMITER $$

CREATE PROCEDURE `sp_routing_usage_increment`(
    IN p_employee_number INT,
    IN p_recipient_id INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        SET p_status = -1;
        SET p_error_msg = 'Unexpected SQL exception in sp_routing_usage_increment';
        ROLLBACK;
    END;

    START TRANSACTION;

    IF p_employee_number IS NULL THEN
        SET p_status = -1;
        SET p_error_msg = 'Employee number is required';
        ROLLBACK;
    ELSEIF p_recipient_id IS NULL THEN
        SET p_status = -1;
        SET p_error_msg = 'Recipient ID is required';
        ROLLBACK;
    ELSE
        -- Insert or update usage tracking
        INSERT INTO routing_recipient_tracker (employee_number, recipient_id, usage_count, last_used_date)
        VALUES (p_employee_number, p_recipient_id, 1, NOW())
        ON DUPLICATE KEY UPDATE
            usage_count = usage_count + 1,
            last_used_date = NOW();

        SET p_status = 1;
        SET p_error_msg = 'Usage count incremented';
        COMMIT;
    END IF;
END $$

DELIMITER ;
