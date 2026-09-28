CREATE TABLE IF NOT EXISTS receiving_scanner_run (
    id CHAR(36) PRIMARY KEY COMMENT 'Run GUID',
    session_id CHAR(36) NOT NULL,
    user_id VARCHAR(100) NOT NULL,
    profile_id CHAR(36) NULL,
    run_status VARCHAR(24) NOT NULL COMMENT 'Running, Stopped, Completed, Failed',
    stop_reason VARCHAR(120) NULL,
    sent_count INT NOT NULL DEFAULT 0,
    failed_count INT NOT NULL DEFAULT 0,
    waiting_count INT NOT NULL DEFAULT 0,
    started_at DATETIME NOT NULL,
    ended_at DATETIME NULL,
    summary_message VARCHAR(500) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_receiving_scanner_run_session
        FOREIGN KEY (session_id) REFERENCES receiving_scanner_session(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_receiving_scanner_run_profile
        FOREIGN KEY (profile_id) REFERENCES receiving_scanner_profile(id)
        ON DELETE SET NULL,
    INDEX idx_receiving_scanner_run_session (session_id),
    INDEX idx_receiving_scanner_run_user (user_id),
    INDEX idx_receiving_scanner_run_started (started_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Historical run headers for Receiving scanner sends.';