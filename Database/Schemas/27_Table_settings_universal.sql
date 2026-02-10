-- ============================================================================
-- Table: settings_universal
-- Module: Settings
-- Purpose: Stores all system-wide configuration settings
-- ============================================================================

DROP TABLE IF EXISTS settings_universal;

CREATE TABLE settings_universal (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Auto-incrementing primary key for the setting record',

    -- Organization
    category VARCHAR(50) NOT NULL COMMENT 'Settings category (System, Security, Receiving, Dunnage, Volvo, Reporting, ERP, UserDefaults)',
    sub_category VARCHAR(50) NULL COMMENT 'Optional sub-category for hierarchical organization',

    -- Setting identification
    setting_key VARCHAR(100) NOT NULL COMMENT 'Unique key for the setting (machine identifier, used in code)',
    setting_name VARCHAR(200) NOT NULL COMMENT 'Human-readable display name shown in UI',
    description TEXT NULL COMMENT 'Detailed description / help text shown toauth_users or administrators',

    -- Value and type
    setting_value TEXT NULL COMMENT 'Current value (store JSON for complex types; plain text for simple types)',
    default_value TEXT NULL COMMENT 'Factory default value to fall back to when no user value is provided',
    data_type ENUM('string', 'int', 'boolean', 'json', 'path', 'password', 'email') NOT NULL DEFAULT 'string' COMMENT 'Data type hint for validation, UI rendering, and storage handling',

    -- Access control
    scope ENUM('system', 'user') NOT NULL DEFAULT 'system' COMMENT 'Determines if the setting is system-wide or can be overridden per user',
    permission_level ENUM('user', 'operator', 'admin', 'developer', 'superadmin') NOT NULL DEFAULT 'admin' COMMENT 'Minimum permission level required to view/modify this setting',
    is_locked BOOLEAN DEFAULT FALSE COMMENT 'If true, prevents modification even by higher-permission auth_users (administrative lock)',
    is_sensitive BOOLEAN DEFAULT FALSE COMMENT 'If true, value should be treated as sensitive: encrypted at rest and masked in the UI',

    -- Validation
    validation_rules JSON NULL COMMENT 'JSON object containing validation metadata (min, max, pattern, allowed_values, etc.)',

    -- UI hints
    ui_control_type ENUM('textbox', 'numberbox', 'toggleswitch', 'combobox', 'passwordbox', 'folderpicker', 'datagrid') NOT NULL DEFAULT 'textbox' COMMENT 'Preferred UI control type for editing this setting',
    ui_order INT DEFAULT 0 COMMENT 'Display order within category (lower values shown first)',

    -- Metadata
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT 'Record creation timestamp (UTC recommended)',
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Record last updated timestamp (updated automatically)',
    updated_by INT NULL COMMENT 'FK toauth_users table - ID of the user who last updated the setting',

    -- Indexes
    UNIQUE KEY unique_setting (category, setting_key),
    INDEX idx_category (category),
    INDEX idx_scope (scope),
    INDEX idx_permission (permission_level)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='System-wide application settings';

-- ============================================================================
