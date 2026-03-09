-- ============================================================================
-- Stored Procedure: sp_BulkInventory_Transaction_UpdateStatus
-- Description: Update the status and optional error message for a given row.
--              updated_at is refreshed automatically via ON UPDATE CURRENT_TIMESTAMP.
-- Feature: Module_Bulk_Inventory — Phase 2 Database
-- Created: 2026-03-08
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_BulkInventory_Transaction_UpdateStatus`;

DELIMITER $$

CREATE PROCEDURE `sp_BulkInventory_Transaction_UpdateStatus`(
    IN p_id             INT UNSIGNED,
    IN p_status         ENUM('Pending','InProgress','WaitingForConfirmation','Success','Failed','Skipped','Consolidated'),
    IN p_error_message  TEXT
)
BEGIN
    UPDATE `bulk_inventory_transactions`
    SET
        `status`        = p_status,
        `error_message` = p_error_message
    WHERE `id` = p_id;

    SELECT ROW_COUNT() AS `rows_affected`;
END$$

DELIMITER ;
