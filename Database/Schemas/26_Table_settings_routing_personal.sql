-- ============================================================================
-- Table: settings_routing_personal
-- Module: Routing
-- Purpose: User preferences for default mode and settings
-- ============================================================================

DROP TABLE IF EXISTS `settings_routing_personal`;

CREATE TABLE `settings_routing_personal` (
    `id` INT NOT NULL AUTO_INCREMENT COMMENT 'Surrogate PK for user preference record',
    `employee_number` INT NOT NULL COMMENT 'Employee number (owner of these preferences)',
    `default_mode` VARCHAR(20) NOT NULL DEFAULT 'WIZARD' COMMENT 'Default routing mode (e.g., WIZARD, MANUAL)',
    `enable_validation` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Flag (1=true,0=false) to enable routing validation checks',
    `updated_date` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Last updated timestamp (auto-managed)',
    PRIMARY KEY (`id`),
    UNIQUE INDEX `idx_unique_employee` (`employee_number`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='User preferences for routing module';

-- ============================================================================
