-- ============================================================================
-- Stored Procedure: sp_BulkInventory_Transaction_Insert
-- Description: Insert a new pending bulk inventory transaction row.
--              Returns the auto-generated id via SELECT LAST_INSERT_ID().
-- Feature: Module_Bulk_Inventory — Phase 2 Database
-- Created: 2026-03-08
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_BulkInventory_Transaction_Insert`;

DELIMITER $$

CREATE PROCEDURE `sp_BulkInventory_Transaction_Insert`(
    IN p_created_by_user    VARCHAR(100),
    IN p_transaction_type   ENUM('Transfer','NewTransaction'),
    IN p_part_id            VARCHAR(50),
    IN p_from_warehouse     VARCHAR(10),
    IN p_from_location      VARCHAR(30),
    IN p_to_warehouse       VARCHAR(10),
    IN p_to_location        VARCHAR(30),
    IN p_quantity           DECIMAL(18,4),
    IN p_work_order         VARCHAR(50),
    IN p_lot_no             VARCHAR(20),
    IN p_visual_username    VARCHAR(100)
)
BEGIN
    INSERT INTO `bulk_inventory_transactions`
    (
        `created_by_user`,
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
        `visual_username`
    )
    VALUES
    (
        p_created_by_user,
        p_transaction_type,
        p_part_id,
        p_from_warehouse,
        p_from_location,
        p_to_warehouse,
        p_to_location,
        p_quantity,
        p_work_order,
        p_lot_no,
        'Pending',
        p_visual_username
    );

    SELECT LAST_INSERT_ID() AS `id`;
END$$

DELIMITER ;
