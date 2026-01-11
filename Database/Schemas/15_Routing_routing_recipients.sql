-- ============================================================================
-- Table: routing_recipients
-- Module: Routing
-- Purpose: Master table of package recipients
-- ============================================================================

DROP TABLE IF EXISTS `routing_recipients`;

CREATE TABLE `routing_recipients` (
    -- Primary key
    `id` INT NOT NULL AUTO_INCREMENT COMMENT 'Unique recipient identifier',

    -- Recipient information
    `name` VARCHAR(100) NOT NULL COMMENT 'Full name of the recipient',
    `location` VARCHAR(100) NOT NULL COMMENT 'Physical location or building',
    `department` VARCHAR(100) NULL COMMENT 'Department or organizational unit',

    -- Status tracking
    `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Active status (1=active, 0=inactive)',

    -- Audit fields
    `created_date` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Record creation timestamp',
    `updated_date` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Last modification timestamp',

    PRIMARY KEY (`id`),
    UNIQUE INDEX `idx_unique_name` (`name`) COMMENT 'Ensures unique recipient names',
    INDEX `idx_is_active` (`is_active`) COMMENT 'Optimizes queries filtering by active status'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Package recipients master table';

-- ============================================================================
