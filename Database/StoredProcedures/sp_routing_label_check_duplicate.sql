DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_routing_label_check_duplicate` $$

CREATE PROCEDURE `sp_routing_label_check_duplicate`(
    IN p_po_number VARCHAR(20),
    IN p_line_number VARCHAR(10),
    IN p_recipient_id INT,
    IN p_hours_window INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    DECLARE v_count INT DEFAULT 0;
    DECLARE v_latest_date DATETIME;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
    END;

    IF p_hours_window IS NULL OR p_hours_window <= 0 THEN
        SET p_hours_window = 24;  -- Default 24 hour window
    END IF;

    -- Check for duplicate labels in time window
    SELECT COUNT(*), MAX(created_date)
    INTO v_count, v_latest_date
    FROM routing_labels
    WHERE po_number = p_po_number
        AND line_number = p_line_number
        AND recipient_id = p_recipient_id
        AND is_active = 1
        AND created_date >= DATE_SUB(NOW(), INTERVAL p_hours_window HOUR);

    IF v_count > 0 THEN
        -- Return duplicate found
        SELECT
            id,
            po_number,
            line_number,
            recipient_id,
            created_by,
            created_date
        FROM routing_labels
        WHERE po_number = p_po_number
            AND line_number = p_line_number
            AND recipient_id = p_recipient_id
            AND is_active = 1
            AND created_date >= DATE_SUB(NOW(), INTERVAL p_hours_window HOUR)
        ORDER BY created_date DESC
        LIMIT 1;
        
        SET p_status = 0;  -- Duplicate found
        SET p_error_msg = CONCAT('Duplicate label found created at ', v_latest_date);
    ELSE
        SET p_status = 1;  -- No duplicate
        SET p_error_msg = 'No duplicate found';
    END IF;
END $$

DELIMITER ;
