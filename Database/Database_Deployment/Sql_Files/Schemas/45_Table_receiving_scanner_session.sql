CREATE TABLE IF NOT EXISTS receiving_scanner_session (
    id CHAR(36) PRIMARY KEY COMMENT 'Session GUID',
    user_id VARCHAR(100) NOT NULL COMMENT 'Owner application user id',
    profile_id CHAR(36) NULL COMMENT 'Selected scanner profile id',
    session_name VARCHAR(120) NOT NULL COMMENT 'User-facing session name',
    status VARCHAR(24) NOT NULL DEFAULT 'Ready' COMMENT 'Ready, Running, Stopped, Completed, Failed',
    stop_requested TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Stop flag honored between item sends',
    sent_count INT NOT NULL DEFAULT 0,
    failed_count INT NOT NULL DEFAULT 0,
    waiting_count INT NOT NULL DEFAULT 0,
    last_message VARCHAR(500) NULL COMMENT 'Latest user-facing status message',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_receiving_scanner_session_user (user_id),
    INDEX idx_receiving_scanner_session_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Scanner current-list sessions for Receiving feature.';
