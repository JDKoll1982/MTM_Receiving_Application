DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_routing_usage_increment` $$

CREATE PROCEDURE `sp_routing_usage_increment`(
    IN p_employee_number INT,
    IN p_recipient_id INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
sp_routing_usage_increment: BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
        ROLLBACK;
    END;

    START TRANSACTION;

    IF p_employee_number IS NULL THEN
        SET p_status = -1;
        SET p_error_msg = 'Employee number is required';
        ROLLBACK;
        LEAVE sp_routing_usage_increment;
    END IF;

    IF p_recipient_id IS NULL THEN
        SET p_status = -1;
        SET p_error_msg = 'Recipient ID is required';
        ROLLBACK;
        LEAVE sp_routing_usage_increment;
    END IF;

    -- Insert or update usage tracking
    INSERT INTO routing_usage_tracking (employee_number, recipient_id, usage_count, last_used_date)
    VALUES (p_employee_number, p_recipient_id, 1, NOW())
    ON DUPLICATE KEY UPDATE
        usage_count = usage_count + 1,
        last_used_date = NOW();

    SET p_status = 1;
    SET p_error_msg = 'Usage count incremented';

    COMMIT;
END $$

DELIMITER ;
