-- ============================================================================
-- Table: settings_activity
-- Module: Settings
-- Purpose: Tracks all changes to settings for compliance
-- ============================================================================

DROP TABLE IF EXISTS settings_activity;

CREATE TABLE settings_activity (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Primary key for the audit record',

    -- What changed
    setting_id INT NOT NULL COMMENT 'FK to settings_universal(id)',
    user_setting_id INT NULL COMMENT 'FK to settings_personal(id) if a user-specific override was changed',

    -- Change details
    old_value TEXT NULL COMMENT 'Previous value (serialized as stored; e.g., JSON or plain text)',
    new_value TEXT NULL COMMENT 'New value (serialized as stored; e.g., JSON or plain text)',
    change_type ENUM('create', 'update', 'delete', 'lock', 'unlock', 'reset') NOT NULL
        COMMENT 'Nature of the change: create/update/delete/lock/unlock/reset',

    -- Who and when
    changed_by INT NOT NULL COMMENT 'FK to users(id) - user who performed the change',
    changed_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT 'When the change occurred (server time)',

    -- Context
    ip_address VARCHAR(45) NULL COMMENT 'IP address of the user or system performing the change (IPv4/IPv6)',
    workstation_name VARCHAR(100) NULL COMMENT 'Client workstation or host name where change originated',

    -- Indexes / Constraints
    FOREIGN KEY (setting_id) REFERENCES settings_universal(id) ON DELETE CASCADE,
    INDEX idx_setting (setting_id),
    INDEX idx_user (changed_by),
    INDEX idx_timestamp (changed_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Audit trail for all settings changes';

-- ============================================================================
