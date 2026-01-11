-- ============================================================================
-- Table: package_types
-- Module: Settings
-- Purpose: Master list of package types for CRUD operations
-- ============================================================================

DROP TABLE IF EXISTS package_types;

CREATE TABLE IF NOT EXISTS package_types (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Primary key - surrogate identifier for package type',

    -- Package type definition
    name VARCHAR(50) NOT NULL COMMENT 'Display name (e.g., Box, Pallet, Crate)',
    code VARCHAR(20) NOT NULL COMMENT 'Unique code (e.g., BOX, PLT, CRT)',

    -- Metadata
    is_active BOOLEAN DEFAULT TRUE COMMENT 'Active flag - determines availability for selection',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT 'Record creation timestamp',
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Record last update timestamp',
    created_by INT NULL COMMENT 'FK to users table (nullable) - user who created the record',

    -- Constraints
    UNIQUE KEY unique_name (name),
    UNIQUE KEY unique_code (code),
    INDEX idx_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Master list of package types for receiving operations';

-- ============================================================================
-- Initial Data
-- ============================================================================
INSERT IGNORE INTO package_types (name, code, is_active) VALUES
('Box', 'BOX', TRUE),
('Pallet', 'PLT', TRUE),
('Crate', 'CRT', TRUE),
('Skids', 'SKD', TRUE),
('Coils', 'COIL', TRUE),
('Sheets', 'SHT', TRUE),
('Drums', 'DRM', TRUE),
('Cartons', 'CTN', TRUE);

-- ============================================================================
