-- ============================================================================
-- Table: routing_other_reasons
-- Module: Routing
-- Purpose: Enumerated reasons for non-PO packages
-- ============================================================================

-- Drop existing table if present
DROP TABLE IF EXISTS `routing_other_reasons`;

CREATE TABLE `routing_other_reasons` (
    `id` INT NOT NULL AUTO_INCREMENT COMMENT 'Primary surrogate key for the reasons table',
    `reason_code` VARCHAR(20) NOT NULL COMMENT 'Short unique code identifying the non-PO reason',
    `description` VARCHAR(200) NOT NULL COMMENT 'Human-readable description of the reason',
    `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '1 = active (available for selection); 0 = inactive',
    `display_order` INT NOT NULL DEFAULT 999 COMMENT 'Numeric ordering for UI display; lower values shown first',
    PRIMARY KEY (`id`),
    UNIQUE INDEX `idx_unique_reason_code` (`reason_code`),
    INDEX `idx_display_order` (`display_order`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Non-PO package reasons enumeration';

-- ============================================================================
