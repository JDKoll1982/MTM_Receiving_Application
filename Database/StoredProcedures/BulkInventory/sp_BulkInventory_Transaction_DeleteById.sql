-- ============================================================================
-- Stored Procedure: sp_BulkInventory_Transaction_DeleteById
-- Description: Hard-delete a single bulk inventory transaction row by id.
--              Used when the user removes a row from the grid during data entry.
--              Not callable during an active push (the overlay prevents deletion).
-- Feature: Module_Bulk_Inventory — Phase 2 Database
-- Created: 2026-03-08
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_BulkInventory_Transaction_DeleteById`;

DELIMITER $$

CREATE PROCEDURE `sp_BulkInventory_Transaction_DeleteById`(
    IN p_id INT UNSIGNED
)
BEGIN
    DELETE FROM `bulk_inventory_transactions`
    WHERE `id` = p_id;

    SELECT ROW_COUNT() AS `rows_affected`;
END$$

DELIMITER ;
