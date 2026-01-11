-- ============================================================================
-- Stored Procedure: sp_routing_label_archive
-- Description: Archive routing labels (mark as is_archived = 1) for history
-- Feature: Routing Module (001-routing-module)
-- Created: January 4, 2026
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_routing_label_archive;

DELIMITER $$

CREATE PROCEDURE sp_routing_label_archive(
    IN p_label_ids TEXT,  -- Comma-separated list of IDs (e.g., '1,2,3,4')
    OUT p_archived_count INT,
    OUT p_error_message VARCHAR(500)
)
BEGIN
    -- local variable to capture diagnostic message (MySQL 5.7 compatible)
    DECLARE v_error_message VARCHAR(500) DEFAULT NULL;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        -- capture the diagnostic message into a local variable, then expose via OUT param
        GET DIAGNOSTICS CONDITION 1
            v_error_message = MESSAGE_TEXT;
        SET p_error_message = v_error_message;
        SET p_archived_count = 0;
        ROLLBACK;
    END;

    -- Initialize outputs
    SET p_archived_count = 0;
    SET p_error_message = NULL;

    -- Start transaction
    START TRANSACTION;

    -- Archive labels by setting is_archived = 1
    -- Note: In MySQL 5.7.24, we can't use JSON or CTEs, so we use FIND_IN_SET for simple comma-separated values
    UPDATE routing_label_data
    SET is_archived = 1
    WHERE FIND_IN_SET(id, p_label_ids) > 0
        AND is_archived = 0;

    -- Get count of archived rows
    SET p_archived_count = ROW_COUNT();

    -- Commit transaction
    COMMIT;
END$$

DELIMITER ;
