DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_routing_recipient_get_top_by_usage` $$

-- =============================================
-- Procedure: sp_routing_recipient_get_top_by_usage
-- Purpose: Get top N recipients by usage frequency
-- Parameters:
--   p_employee_number: Employee to get personalized results for
--   p_top_count: Number of recipients to return
-- Returns: Result set with recipients sorted by usage
-- =============================================
CREATE PROCEDURE `sp_routing_recipient_get_top_by_usage`(
    IN p_employee_number INT,
    IN p_top_count INT
)
BEGIN
    DECLARE v_employee_label_count INT DEFAULT 0;

    IF p_top_count IS NULL OR p_top_count <= 0 THEN
        SET p_top_count = 5;
    END IF;

    -- Check how many labels this employee has created
    SELECT COUNT(*)
    INTO v_employee_label_count
    FROM routing_labels
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
        LEFT JOIN routing_usage_tracking u ON r.id = u.recipient_id AND u.employee_number = p_employee_number
        WHERE r.is_active = 1
        ORDER BY COALESCE(u.usage_count, 0) DESC, r.name
        LIMIT p_top_count;
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
        LEFT JOIN routing_usage_tracking u ON r.id = u.recipient_id
        WHERE r.is_active = 1
        GROUP BY r.id, r.name, r.location, r.department, r.is_active, r.created_date, r.updated_date
        ORDER BY COALESCE(SUM(u.usage_count), 0) DESC, r.name
        LIMIT p_top_count;
    END IF;
END $$

DELIMITER ;
