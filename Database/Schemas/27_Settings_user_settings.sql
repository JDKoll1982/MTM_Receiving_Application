-- ============================================================================
-- Table: user_settings
-- Module: Settings
-- Purpose: User-specific overrides for settings where scope='user'
-- ============================================================================

DROP TABLE IF EXISTS user_settings;

CREATE TABLE user_settings (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Primary key for the user_settings table',

    -- User association
    user_id INT NOT NULL COMMENT 'FK to users.id - identifies the user owning this override',

    -- Setting reference
    setting_id INT NOT NULL COMMENT 'FK to system_settings.id - references the system setting being overridden',

    -- Override value
    setting_value TEXT NULL COMMENT 'User-specific override value for the referenced setting',

    -- Metadata
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when the record was created',
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Timestamp when the record was last updated',

    -- Constraints
    FOREIGN KEY (setting_id) REFERENCES system_settings(id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_setting (user_id, setting_id) COMMENT 'Ensures a single override per user per setting',
    INDEX idx_user (user_id) COMMENT 'Index to speed lookups by user_id'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='User-specific setting overrides';

-- ============================================================================
