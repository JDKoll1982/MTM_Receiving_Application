DROP TABLE IF EXISTS settings_personal;

CREATE TABLE settings_personal (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Primary key for the settings_personal table',
    user_id INT NOT NULL COMMENT 'FK to users.id - identifies the user owning this setting',
    category VARCHAR(100) NOT NULL COMMENT 'Settings category (matches settings_universal.category)',
    setting_key VARCHAR(150) NOT NULL COMMENT 'Setting key (matches settings_universal.setting_key)',
    setting_value TEXT NULL COMMENT 'User-specific value for this setting',
    data_type VARCHAR(50) NOT NULL DEFAULT 'String' COMMENT 'Data type hint: String, Int, Bool, Json, etc.',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when the record was created',
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Timestamp when the record was last updated',
    updated_by VARCHAR(100) NULL COMMENT 'Username of the user who last updated the setting',
    UNIQUE KEY unique_user_setting (user_id, category, setting_key) COMMENT 'Ensures a single override per user per setting',
    INDEX idx_user (user_id) COMMENT 'Index to speed lookups by user_id',
    INDEX idx_category (category)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'User-specific application settings';