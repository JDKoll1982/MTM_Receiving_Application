-- =============================================
-- Routing Module Database Schema
-- Feature: 001-routing-module
-- Date: 2026-01-04
-- Database: mtm_receiving_application (MySQL 5.7.24 compatible)
-- =============================================

-- Drop tables if they exist (in reverse order of dependencies)
DROP TABLE IF EXISTS `routing_label_history`;
DROP TABLE IF EXISTS `routing_usage_tracking`;
DROP TABLE IF EXISTS `routing_user_preferences`;
DROP TABLE IF EXISTS `routing_labels`;
DROP TABLE IF EXISTS `routing_other_reasons`;
DROP TABLE IF EXISTS `routing_recipients`;

-- =============================================
-- Table: routing_recipients
-- Purpose: Master table of package recipients
-- =============================================
CREATE TABLE `routing_recipients` (
    `id` INT NOT NULL AUTO_INCREMENT,
    `name` VARCHAR(100) NOT NULL,
    `location` VARCHAR(100) NOT NULL,
    `department` VARCHAR(100) NULL,
    `is_active` TINYINT(1) NOT NULL DEFAULT 1,
    `created_date` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_date` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    UNIQUE INDEX `idx_unique_name` (`name`),
    INDEX `idx_is_active` (`is_active`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Package recipients master table';

-- =============================================
-- Table: routing_other_reasons
-- Purpose: Enumerated reasons for non-PO packages
-- =============================================
CREATE TABLE `routing_other_reasons` (
    `id` INT NOT NULL AUTO_INCREMENT,
    `reason_code` VARCHAR(20) NOT NULL,
    `description` VARCHAR(200) NOT NULL,
    `is_active` TINYINT(1) NOT NULL DEFAULT 1,
    `display_order` INT NOT NULL DEFAULT 999,
    PRIMARY KEY (`id`),
    UNIQUE INDEX `idx_unique_reason_code` (`reason_code`),
    INDEX `idx_display_order` (`display_order`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Non-PO package reasons enumeration';

-- =============================================
-- Table: routing_labels
-- Purpose: Primary table for routing labels
-- =============================================
CREATE TABLE `routing_labels` (
    `id` INT NOT NULL AUTO_INCREMENT,
    `po_number` VARCHAR(20) NOT NULL,
    `line_number` VARCHAR(10) NOT NULL,
    `description` VARCHAR(200) NOT NULL,
    `recipient_id` INT NOT NULL,
    `quantity` INT NOT NULL,
    `created_by` INT NOT NULL,
    `created_date` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `other_reason_id` INT NULL,
    `is_active` TINYINT(1) NOT NULL DEFAULT 1,
    `csv_exported` TINYINT(1) NOT NULL DEFAULT 0,
    `csv_export_date` DATETIME NULL,
    PRIMARY KEY (`id`),
    INDEX `idx_po_number` (`po_number`),
    INDEX `idx_created_by` (`created_by`),
    INDEX `idx_created_date` (`created_date`),
    INDEX `idx_recipient_id` (`recipient_id`),
    CONSTRAINT `fk_label_recipient` FOREIGN KEY (`recipient_id`) REFERENCES `routing_recipients` (`id`),
    CONSTRAINT `fk_label_other_reason` FOREIGN KEY (`other_reason_id`) REFERENCES `routing_other_reasons` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Routing labels primary table';

-- =============================================
-- Table: routing_usage_tracking
-- Purpose: Track employee-recipient usage frequency
-- =============================================
CREATE TABLE `routing_usage_tracking` (
    `id` INT NOT NULL AUTO_INCREMENT,
    `employee_number` INT NOT NULL,
    `recipient_id` INT NOT NULL,
    `usage_count` INT NOT NULL DEFAULT 0,
    `last_used_date` DATETIME NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE INDEX `idx_unique_employee_recipient` (`employee_number`, `recipient_id`),
    INDEX `idx_employee_number` (`employee_number`),
    INDEX `idx_recipient_id` (`recipient_id`),
    CONSTRAINT `fk_usage_recipient` FOREIGN KEY (`recipient_id`) REFERENCES `routing_recipients` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Employee-recipient usage tracking for personalization';

-- =============================================
-- Table: routing_user_preferences
-- Purpose: User preferences for default mode and settings
-- =============================================
CREATE TABLE `routing_user_preferences` (
    `id` INT NOT NULL AUTO_INCREMENT,
    `employee_number` INT NOT NULL,
    `default_mode` VARCHAR(20) NOT NULL DEFAULT 'WIZARD',
    `enable_validation` TINYINT(1) NOT NULL DEFAULT 1,
    `updated_date` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    UNIQUE INDEX `idx_unique_employee` (`employee_number`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='User preferences for routing module';

-- =============================================
-- Table: routing_label_history
-- Purpose: Audit trail for label edits
-- =============================================
CREATE TABLE `routing_label_history` (
    `id` INT NOT NULL AUTO_INCREMENT,
    `label_id` INT NOT NULL,
    `field_changed` VARCHAR(50) NOT NULL,
    `old_value` VARCHAR(200) NULL,
    `new_value` VARCHAR(200) NULL,
    `edited_by` INT NOT NULL,
    `edit_date` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    INDEX `idx_label_id` (`label_id`),
    INDEX `idx_edit_date` (`edit_date`),
    CONSTRAINT `fk_history_label` FOREIGN KEY (`label_id`) REFERENCES `routing_labels` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Label edit history for audit trail';
