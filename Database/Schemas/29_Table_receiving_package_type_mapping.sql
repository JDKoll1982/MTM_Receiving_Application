-- ============================================================================
-- Table: receiving_package_type_mapping
-- Module: Settings
-- Purpose: Maps part prefixes to package types (Receiving module)
-- ============================================================================

DROP TABLE IF EXISTS receiving_package_type_mapping;

CREATE TABLE receiving_package_type_mapping (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Primary key for mapping record',

    -- Mapping
    part_prefix VARCHAR(10) NOT NULL COMMENT 'Part number prefix (e.g., MCC, MMF)',
    package_type VARCHAR(50) NOT NULL COMMENT 'Package type label (e.g., Coils, Sheets, Skids)',

    -- Metadata
    is_default BOOLEAN DEFAULT FALSE COMMENT 'If true, used when no prefix matches (fallback)',
    display_order INT DEFAULT 0 COMMENT 'Sort order for display and selection precedence',
    is_active BOOLEAN DEFAULT TRUE COMMENT 'Whether this mapping is active (soft-delete flag)',

    -- Timestamps
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when record was created',
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Timestamp when record was last updated',
    created_by INT NULL COMMENT 'FK toauth_users table (nullable)',

    -- Constraints
    UNIQUE KEY unique_prefix (part_prefix),
    INDEX idx_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Part prefix to package type mappings for receiving workflow';

-- ============================================================================
-- Initial Data
-- ============================================================================
INSERT IGNORE INTO receiving_package_type_mapping (part_prefix, package_type, is_default, display_order, is_active) VALUES
('MCC', 'Coils', FALSE, 1, TRUE),
('MMF', 'Sheets', FALSE, 2, TRUE),
('DEFAULT', 'Skids', TRUE, 99, TRUE);

-- ============================================================================
