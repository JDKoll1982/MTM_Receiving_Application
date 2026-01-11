-- ============================================================================
-- Table: user_activity_log
-- Module: Authentication
-- Purpose: Audit trail for authentication events and user actions
-- ============================================================================

CREATE TABLE IF NOT EXISTS user_activity_log (
    log_id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for each log entry',
    event_type VARCHAR(50) NOT NULL COMMENT 'Type of event (e.g., Login, Logout, Failed_Login, Password_Change)',
    username VARCHAR(50) NULL COMMENT 'Username associated with the event (NULL for system events)',
    workstation_name VARCHAR(50) NULL COMMENT 'Name of the workstation where the event occurred',
    event_timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when the event occurred',
    details TEXT NULL COMMENT 'Additional JSON or text details about the event',

    INDEX idx_log_timestamp (event_timestamp) COMMENT 'Index for querying logs by timestamp',
    INDEX idx_log_username (username) COMMENT 'Index for querying logs by username',
    INDEX idx_log_event_type (event_type) COMMENT 'Index for querying logs by event type'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Audit trail for authentication events and user actions';
