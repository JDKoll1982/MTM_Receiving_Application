-- Quality Hold Schema
-- Table: receiving_quality_holds
-- Purpose: Track quality holds for restricted part numbers (MMFSR, MMCSR)

CREATE TABLE IF NOT EXISTS `receiving_quality_holds` (
    `quality_hold_id` INT PRIMARY KEY AUTO_INCREMENT COMMENT 'Unique quality hold identifier',
    `load_id` INT NOT NULL COMMENT 'Reference to receiving load',
    `part_id` VARCHAR(50) NOT NULL COMMENT 'Restricted part number (e.g., MMFSR05645)',
    `restriction_type` VARCHAR(50) NOT NULL COMMENT 'Type of restriction (MMFSR, MMCSR)',
    `quality_acknowledged_by` VARCHAR(255) COMMENT 'Quality person who acknowledged the hold',
    `quality_acknowledged_at` DATETIME COMMENT 'Timestamp when quality acknowledged',
    `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT 'Record creation timestamp',
    `updated_at` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Last update timestamp',
    
    -- Indexes
    INDEX idx_load_id (load_id) COMMENT 'Fast lookup by load',
    INDEX idx_part_id (part_id) COMMENT 'Fast lookup by part number',
    INDEX idx_restriction_type (restriction_type) COMMENT 'Filter by restriction type',
    INDEX idx_acknowledged_at (quality_acknowledged_at) COMMENT 'Find unacknowledged holds',
    
    -- Foreign key (if applicable)
    CONSTRAINT fk_quality_holds_load FOREIGN KEY (load_id)
        REFERENCES receiving_loads(load_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Tracks quality holds for restricted part numbers requiring immediate quality review';
