-- ============================================================================
-- Table: settings_activity
-- Module: Settings
-- Purpose: Tracks all changes to settings for compliance
-- ============================================================================
DROP TABLE IF EXISTS settings_activity;

CREATE TABLE settings_activity (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Primary key for the audit record',
    -- What changed
    scope VARCHAR(20) NOT NULL COMMENT 'Setting scope: System or User',
    category VARCHAR(100) NOT NULL COMMENT 'Settings category (matches settings_universal.category)',
    setting_key VARCHAR(150) NOT NULL COMMENT 'Setting key (matches settings_universal.setting_key)',
    -- Change details
    old_value TEXT NULL COMMENT 'Previous value before the change',
    new_value TEXT NULL COMMENT 'New value after the change',
    change_type VARCHAR(50) NOT NULL DEFAULT 'update' COMMENT 'Nature of the change: create, update, delete, lock, unlock, reset, DefaultApplied',
    -- Who and when
    user_id INT NULL COMMENT 'ID of the user who performed the change (0 or NULL for system actions)',
    changed_by VARCHAR(100) NOT NULL COMMENT 'Username of the user or system account that made the change',
    changed_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT 'When the change occurred',
    -- Context
    ip_address VARCHAR(50) NULL COMMENT 'IP address of the client performing the change',
    workstation VARCHAR(100) NULL COMMENT 'Client workstation or host name where change originated',
    -- Indexes
    INDEX idx_category_key (category, setting_key),
    INDEX idx_user_id (user_id),
    INDEX idx_changed_by (changed_by),
    INDEX idx_timestamp (changed_at)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Audit trail for all settings changes';

-- ============================================================================