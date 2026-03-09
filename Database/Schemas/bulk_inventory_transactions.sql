-- ============================================================================
-- Table: bulk_inventory_transactions
-- Module: Module_Bulk_Inventory
-- Purpose: Audit log for every inventory transaction row pushed to Infor Visual.
--          Rows are inserted with status 'Pending' before automation starts and
--          updated to 'Success', 'Failed', 'Skipped', or 'Consolidated' after.
--          Crash-recovery uses rows left in 'InProgress' at startup.
-- Created: 2026-03-08
-- ============================================================================

USE mtm_receiving_application;

CREATE TABLE IF NOT EXISTS `bulk_inventory_transactions` (
    `id`                     INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    `created_by_user`        VARCHAR(100)    NOT NULL,
    `created_at`             DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `transaction_type`       ENUM('Transfer','NewTransaction') NOT NULL,
    `part_id`                VARCHAR(50)     NOT NULL,
    `from_warehouse`         VARCHAR(10)     NULL,
    `from_location`          VARCHAR(30)     NULL,
    `to_warehouse`           VARCHAR(10)     NULL,
    `to_location`            VARCHAR(30)     NULL,
    `quantity`               DECIMAL(18,4)   NOT NULL,
    `work_order`             VARCHAR(50)     NULL,
    `lot_no`                 VARCHAR(20)     NULL,
    `status`                 ENUM('Pending','InProgress','WaitingForConfirmation','Success','Failed','Skipped','Consolidated') NOT NULL DEFAULT 'Pending',
    `error_message`          TEXT            NULL,
    `visual_username`        VARCHAR(100)    NOT NULL COMMENT 'Visual user who executed the transaction (never store password)',
    `updated_at`             DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    INDEX `idx_created_by_status` (`created_by_user`, `status`),
    INDEX `idx_part_id`           (`part_id`),
    INDEX `idx_created_at`        (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
