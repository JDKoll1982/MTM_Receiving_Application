-- ============================================================================
-- Table: routing_label_history
-- Module: Routing
-- Purpose: Audit trail for label edits
-- ============================================================================

DROP TABLE IF EXISTS `routing_label_history`;

CREATE TABLE `routing_label_history` (
    `id` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key: unique history id',
    `label_id` INT NOT NULL COMMENT 'Foreign key to routing_labels.id',
    `field_changed` VARCHAR(50) NOT NULL COMMENT 'Name of the label field that was changed',
    `old_value` VARCHAR(200) NULL COMMENT 'Previous value before the edit',
    `new_value` VARCHAR(200) NULL COMMENT 'New value after the edit',
    `edited_by` INT NOT NULL COMMENT 'User id of the editor',
    `edit_date` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when the edit was made',
    PRIMARY KEY (`id`),
    INDEX `idx_label_id` (`label_id`) COMMENT 'Index to quickly find history rows for a label',
    INDEX `idx_edit_date` (`edit_date`) COMMENT 'Index to query history by edit timestamp',
    CONSTRAINT `fk_history_label` FOREIGN KEY (`label_id`) REFERENCES `routing_labels` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Label edit history for audit trail';

-- ============================================================================
