-- ============================================================================
-- Stored Procedure: sp_routing_other_reason_get_all_active
-- Description: Get all active non-PO package reasons for dropdown population
-- Returns: List of active routing_po_alternatives ordered by display_order
-- Feature: Routing Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_routing_other_reason_get_all_active`;

DELIMITER $$

CREATE PROCEDURE `sp_routing_other_reason_get_all_active`(
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    -- local variable to capture diagnostic message (declare before handlers)
    DECLARE diag_msg VARCHAR(500) DEFAULT '';

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        -- capture diagnostic message into local variable (MySQL 5.7 compatible)
        GET DIAGNOSTICS CONDITION 1
            diag_msg = MESSAGE_TEXT;
        SET p_error_msg = diag_msg;
        SET p_status = -1;
    END;

    SELECT
        id,
        reason_code,
        description,
        is_active,
        display_order
    FROM routing_po_alternatives
    WHERE is_active = 1
    ORDER BY display_order, description;

    SET p_status = 1;
    SET p_error_msg = 'Other reasons retrieved successfully';
END $$

DELIMITER ;
