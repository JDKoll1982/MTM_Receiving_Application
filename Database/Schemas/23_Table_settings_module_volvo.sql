-- ============================================================================
-- Table: settings_module_volvo
-- Module: Volvo
-- Purpose: Stores configurable settings for Volvo module
-- ============================================================================

DROP TABLE IF EXISTS settings_module_volvo;

CREATE TABLE IF NOT EXISTS settings_module_volvo (
    setting_key VARCHAR(100) NOT NULL COMMENT 'Unique key identifier for the setting (e.g., AutoPrintEnabled)',
    setting_value TEXT NOT NULL COMMENT 'Stored value as text; interpretation depends on setting_type',
    setting_type ENUM('String','Integer','Boolean','Path','Enum') NOT NULL COMMENT 'Data type used to interpret setting_value',
    category VARCHAR(50) NOT NULL COMMENT 'Logical grouping for settings (e.g., Printing, Integration)',
    description TEXT COMMENT 'Human-readable description/documentation for the setting',
    default_value TEXT NOT NULL COMMENT 'Default value as text used when no explicit value is set',
    min_value INT NULL COMMENT 'Minimum allowed numeric value (applies when setting_type = Integer)',
    max_value INT NULL COMMENT 'Maximum allowed numeric value (applies when setting_type = Integer)',
    modified_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Timestamp of last modification',
    modified_by VARCHAR(50) COMMENT 'User identifier who last modified the setting',
    PRIMARY KEY (setting_key),
    INDEX idx_category (category) -- Index to speed lookups by category
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Volvo module configurable settings';

-- ============================================================================
