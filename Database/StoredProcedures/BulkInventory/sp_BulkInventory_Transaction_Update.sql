-- ============================================================================
-- Stored Procedure: sp_BulkInventory_Transaction_Update
-- Description: Updates the editable fields of an existing Pending bulk inventory
--              transaction row. Only rows in 'Pending' status are modified so
--              rows that have already been pushed cannot be accidentally overwritten
--              by a late cell-leave save.
-- Feature: Module_Bulk_Inventory — Cell-leave auto-save
-- Created: 2026-05-29
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_BulkInventory_Transaction_Update`;

DELIMITER $$

CREATE PROCEDURE `sp_BulkInventory_Transaction_Update`(
    IN p_id               INT,
    IN p_transaction_type ENUM('Transfer','NewTransaction'),
    IN p_part_id          VARCHAR(50),
    IN p_from_warehouse   VARCHAR(10),
    IN p_from_location    VARCHAR(30),
    IN p_to_warehouse     VARCHAR(10),
    IN p_to_location      VARCHAR(30),
    IN p_quantity         DECIMAL(18,4),
    IN p_work_order       VARCHAR(50),
    IN p_lot_no           VARCHAR(20)
)
BEGIN
    UPDATE `bulk_inventory_transactions`
    SET
        `transaction_type` = p_transaction_type,
        `part_id`          = p_part_id,
        `from_warehouse`   = p_from_warehouse,
        `from_location`    = p_from_location,
        `to_warehouse`     = p_to_warehouse,
        `to_location`      = p_to_location,
        `quantity`         = p_quantity,
        `work_order`       = p_work_order,
        `lot_no`           = p_lot_no,
        `updated_at`       = NOW()
    WHERE `id`     = p_id
      AND `status` = 'Pending';
END$$

DELIMITER ;
