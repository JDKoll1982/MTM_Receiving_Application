-- ============================================================================
-- Table: routing_labels
-- Module: Routing
-- Purpose: Primary table for routing labels
-- ============================================================================

DROP TABLE IF EXISTS `routing_labels`;

CREATE TABLE `routing_labels` (
    `id` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key - unique identifier for each routing label',
    `po_number` VARCHAR(20) NOT NULL COMMENT 'Purchase order number associated with the label',
    `line_number` VARCHAR(10) NOT NULL COMMENT 'PO line number associated with the label',
    `description` VARCHAR(200) NOT NULL COMMENT 'Human-readable description for the label',
    `recipient_id` INT NOT NULL COMMENT 'FK to routing_recipients.id - intended recipient of the label',
    `quantity` INT NOT NULL COMMENT 'Quantity of items represented by this label',
    `created_by` INT NOT NULL COMMENT 'User id who created the label',
    `created_date` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when the label was created',
    `other_reason_id` INT NULL COMMENT 'Optional FK to routing_other_reasons.id for custom reasons',
    `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Flag indicating whether the label is active (1) or inactive (0)',
    `csv_exported` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Flag indicating whether this label has been exported to CSV (1 = yes)',
    `csv_export_date` DATETIME NULL COMMENT 'Timestamp when the label was exported to CSV (nullable)',
    PRIMARY KEY (`id`),
    -- Indexes for common lookup patterns
    INDEX `idx_po_number` (`po_number`), -- index on PO number for fast PO-based queries
    INDEX `idx_created_by` (`created_by`), -- index on creator for audit / user-based queries
    INDEX `idx_created_date` (`created_date`), -- index on creation date for reporting
    INDEX `idx_recipient_id` (`recipient_id`), -- index on recipient for join/filter performance
    -- Foreign key constraints to related routing tables
    CONSTRAINT `fk_label_recipient` FOREIGN KEY (`recipient_id`) REFERENCES `routing_recipients` (`id`),
    CONSTRAINT `fk_label_other_reason` FOREIGN KEY (`other_reason_id`) REFERENCES `routing_other_reasons` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
  COMMENT='Routing labels primary table - stores label metadata and export state';

-- ============================================================================
