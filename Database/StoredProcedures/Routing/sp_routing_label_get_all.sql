-- ============================================================================
-- Stored Procedure: sp_routing_label_get_all
-- Description: Retrieve all active routing labels with pagination
-- Parameters:
--   @p_limit: Maximum number of records to return (default 100)
--   @p_offset: Number of records to skip for pagination (default 0)
-- Returns: Paginated list of routing labels with recipient and reason details
-- Feature: Routing Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_routing_label_get_all`;

DELIMITER $$

CREATE PROCEDURE `sp_routing_label_get_all`(
    IN p_limit INT,
    IN p_offset INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    -- local diagnostic message variable
    DECLARE v_error_msg VARCHAR(500) DEFAULT '';

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        -- populate local variable from diagnostics and propagate to OUT params
        GET DIAGNOSTICS CONDITION 1 v_error_msg = MESSAGE_TEXT;
        SET p_error_msg = v_error_msg;
        SET p_status = -1;
    END;

    -- initialize OUT params
    SET p_status = 0;
    SET p_error_msg = '';

    IF p_limit IS NULL OR p_limit <= 0 THEN
        SET p_limit = 100;
    END IF;

    IF p_offset IS NULL THEN
        SET p_offset = 0;
    END IF;

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
        l.csv_exported,
        l.csv_export_date
    FROM routing_label_data l
    INNER JOIN routing_recipients r ON l.recipient_id = r.id
    LEFT JOIN routing_po_alternatives o ON l.other_reason_id = o.id
    WHERE l.is_active = 1
    ORDER BY l.created_date DESC
    LIMIT p_limit OFFSET p_offset;

    SET p_status = 1;
    SET p_error_msg = 'Labels retrieved successfully';
END $$

DELIMITER ;
