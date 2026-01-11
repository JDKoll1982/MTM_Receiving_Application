-- ============================================================================
-- Stored Procedure: sp_routing_recipient_get_top_by_usage
-- Description: Get top N recipients by usage frequency (personalized or system-wide)
-- Parameters:
--   @p_employee_number: Employee to get personalized usage for
--   @p_top_count: Number of top recipients to return (default 5)
-- Logic: Uses personal usage if employee has 20+ labels, otherwise system-wide usage
-- Feature: Routing Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_routing_recipient_get_top_by_usage`;

DELIMITER $$

CREATE PROCEDURE `sp_routing_recipient_get_top_by_usage`(
    IN p_employee_number INT,
    IN p_top_count INT
)
BEGIN
    DECLARE v_employee_label_count INT DEFAULT 0;
    DECLARE v_top_count INT DEFAULT 5;

    -- Respect provided top count if valid, but don't modify IN param
    IF p_top_count IS NOT NULL AND p_top_count > 0 THEN
        SET v_top_count = p_top_count;
    END IF;

    -- Check how many labels this employee has created
    SELECT COUNT(*)
    INTO v_employee_label_count
    FROM routing_label_data
    WHERE created_by = p_employee_number
      AND is_active = 1;

    -- If employee has 20+ labels, use personal usage
    IF v_employee_label_count >= 20 THEN
        SELECT
            r.id,
            r.name,
            r.location,
            r.department,
            r.is_active,
            r.created_date,
            r.updated_date,
            COALESCE(u.usage_count, 0) AS usage_count
        FROM routing_recipients r
        LEFT JOIN routing_recipient_tracker u
            ON r.id = u.recipient_id
            AND u.employee_number = p_employee_number
        WHERE r.is_active = 1
        ORDER BY usage_count DESC, r.name
        LIMIT v_top_count;
    ELSE
        -- Use system-wide usage
        SELECT
            r.id,
            r.name,
            r.location,
            r.department,
            r.is_active,
            r.created_date,
            r.updated_date,
            COALESCE(SUM(u.usage_count), 0) AS usage_count
        FROM routing_recipients r
        LEFT JOIN routing_recipient_tracker u
            ON r.id = u.recipient_id
        WHERE r.is_active = 1
        GROUP BY r.id, r.name, r.location, r.department, r.is_active, r.created_date, r.updated_date
        ORDER BY usage_count DESC, r.name
        LIMIT v_top_count;
    END IF;
END $$

DELIMITER ;
