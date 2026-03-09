-- ============================================================================
-- Stored Procedure: sp_BulkInventory_Transaction_GetByUser
-- Description: Return all transaction rows for a given user, optionally filtered
--              by status.  Pass NULL for p_status to return all statuses.
--              Ordered by created_at DESC (most recent first).
--              Used for loading a user's current pending batch and for crash-recovery
--              on startup (filter by status = 'InProgress').
-- Feature: Module_Bulk_Inventory — Phase 2 Database
-- Created: 2026-03-08
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_BulkInventory_Transaction_GetByUser`;

DELIMITER $$

CREATE PROCEDURE `sp_BulkInventory_Transaction_GetByUser`(
    IN p_username   VARCHAR(100),
    IN p_status     ENUM('Pending','InProgress','WaitingForConfirmation','Success','Failed','Skipped','Consolidated')
)
BEGIN
    SELECT
        `id`,
        `created_by_user`,
        `created_at`,
        `transaction_type`,
        `part_id`,
        `from_warehouse`,
        `from_location`,
        `to_warehouse`,
        `to_location`,
        `quantity`,
        `work_order`,
        `lot_no`,
        `status`,
        `error_message`,
        `visual_username`,
        `updated_at`
    FROM `bulk_inventory_transactions`
    WHERE `created_by_user` = p_username
      AND (p_status IS NULL OR `status` = p_status)
    ORDER BY `created_at` DESC;
END$$

DELIMITER ;
