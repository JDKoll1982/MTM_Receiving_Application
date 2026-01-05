DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_routing_recipient_get_top_by_usage` $$

CREATE PROCEDURE `sp_routing_recipient_get_top_by_usage`(
    IN p_employee_number INT,
    IN p_top_count INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    DECLARE v_employee_label_count INT DEFAULT 0;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
    END;

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
            COALESCE(SUM(u.usage_count), 0) AS usage_count
        FROM routing_recipients r
        LEFT JOIN routing_usage_tracking u ON r.id = u.recipient_id
        WHERE r.is_active = 1
        GROUP BY r.id, r.name, r.location, r.department
        ORDER BY COALESCE(SUM(u.usage_count), 0) DESC, r.name
        LIMIT p_top_count;
    END IF;

    SET p_status = 1;
    SET p_error_msg = 'Top recipients retrieved successfully';
END $$

DELIMITER ;
