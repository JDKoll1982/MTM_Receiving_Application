-- ============================================================================
-- Stored Procedure: sp_routing_label_get_by_id
-- Description: Retrieve a specific routing label by ID
-- Parameters:
--   @p_label_id: The unique ID of the routing label
-- Returns: Full label details with recipient and reason information
-- Feature: Routing Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_routing_label_get_by_id`;

DELIMITER $$

CREATE PROCEDURE `sp_routing_label_get_by_id`(
    IN p_label_id INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    -- Use a local counter to detect existence and avoid RETURN/ROW_COUNT() issues.
    DECLARE v_count INT DEFAULT 0;

    IF p_label_id IS NULL THEN
        SET p_status = -1;
        SET p_error_msg = 'Label ID is required';
    ELSE
        SELECT COUNT(*) INTO v_count
        FROM routing_label_data l
        WHERE l.id = p_label_id AND l.is_active = 1;

        IF v_count = 0 THEN
            SET p_status = 0;
            SET p_error_msg = 'Label not found';
        ELSE
            SELECT
                l.id,
                l.po_number,
                l.line_number,
                l.description,
                l.recipient_id,
                r.name AS recipient_name,
                r.location AS recipient_location,
                l.quantity,
                l.created_by,
                l.created_date,
                l.other_reason_id,
                o.description AS other_reason_description,
                l.is_active,
                l.csv_exported,
                l.csv_export_date
            FROM routing_label_data l
            INNER JOIN routing_recipients r ON l.recipient_id = r.id
            LEFT JOIN routing_po_alternatives o ON l.other_reason_id = o.id
            WHERE l.id = p_label_id AND l.is_active = 1;

            SET p_status = 1;
            SET p_error_msg = 'Label retrieved successfully';
        END IF;
    END IF;

END $$

DELIMITER ;
