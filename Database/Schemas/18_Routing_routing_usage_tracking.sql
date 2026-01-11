-- ============================================================================
-- Table: routing_usage_tracking
-- Module: Routing
-- Purpose: Track employee-recipient usage frequency
-- ============================================================================

DROP TABLE IF EXISTS `routing_usage_tracking`;

CREATE TABLE `routing_usage_tracking` (
    `id` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key - surrogate id',
    `employee_number` INT NOT NULL COMMENT 'Employee number (MTM employee identifier)',
    `recipient_id` INT NOT NULL COMMENT 'Foreign key referencing routing_recipients.id',
    `usage_count` INT NOT NULL DEFAULT 0 COMMENT 'Count of times employee selected this recipient',
    `last_used_date` DATETIME NOT NULL COMMENT 'Last datetime when the recipient was used by the employee',
    PRIMARY KEY (`id`),
    UNIQUE INDEX `idx_unique_employee_recipient` (`employee_number`, `recipient_id`) COMMENT 'Ensures single tracking row per employee/recipient pair',
    INDEX `idx_employee_number` (`employee_number`) COMMENT 'Index to quickly find usage by employee',
    INDEX `idx_recipient_id` (`recipient_id`) COMMENT 'Index to quickly find usage by recipient',
    CONSTRAINT `fk_usage_recipient` FOREIGN KEY (`recipient_id`) REFERENCES `routing_recipients` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Employee-recipient usage tracking for personalization';

-- ============================================================================
