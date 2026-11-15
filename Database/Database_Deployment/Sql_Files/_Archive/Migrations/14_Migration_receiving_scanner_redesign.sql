-- Migration: 14_Migration_receiving_scanner_redesign
-- Description: Removes the legacy Draft / run-snapshot feature from the scanner module and
-- cleans up existing Draft data. Run AFTER the scanner schema/SP files so the retained
-- tables and procedures exist; the statements below are all idempotent.

USE mtm_receiving_application;

-- 1) Drop legacy run-snapshot tables (only used by the removed "Save For Later" feature).
DROP TABLE IF EXISTS receiving_scanner_run_item;
DROP TABLE IF EXISTS receiving_scanner_run;

-- 2) Drop legacy run-snapshot stored procedures.
DROP PROCEDURE IF EXISTS `sp_Receiving_ScannerRunItem_Insert`;
DROP PROCEDURE IF EXISTS `sp_Receiving_ScannerRun_Complete`;
DROP PROCEDURE IF EXISTS `sp_Receiving_ScannerRun_Start`;

-- 3) Drop legacy scanner helper functions (no longer referenced by any procedure/view).
DROP FUNCTION IF EXISTS `fn_Receiving_Scanner_StatusFromCounts`;
DROP FUNCTION IF EXISTS `fn_Receiving_Scanner_IsShortcutDistinct`;

-- 4) Drop legacy scanner views (superseded by the History query over sessions).
DROP VIEW IF EXISTS `view_receiving_scanner_run_summary`;
DROP VIEW IF EXISTS `view_receiving_scanner_active_sessions`;

-- 5) Delete existing Draft sessions (and their items via ON DELETE CASCADE) so no stale
--    Draft rows remain after the Draft status is removed.
DELETE FROM receiving_scanner_item
WHERE session_id IN (
    SELECT id FROM receiving_scanner_session WHERE status = 'Draft'
);

DELETE FROM receiving_scanner_session
WHERE status = 'Draft';

-- 6) Make sure the retained tables exist (no-op when the schema files already ran).
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

CREATE TABLE IF NOT EXISTS receiving_scanner_item (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    session_id CHAR(36) NOT NULL COMMENT 'Parent scanner session id',
    item_order INT NOT NULL COMMENT 'Stable ordered position in current list',
    part_id VARCHAR(50) NOT NULL,
    from_warehouse_id VARCHAR(10) NOT NULL,
    from_location_id VARCHAR(50) NULL,
    to_warehouse_id VARCHAR(10) NOT NULL,
    to_location_id VARCHAR(50) NULL,
    quantity DECIMAL(18,2) NOT NULL,
    payload_json JSON NULL COMMENT 'Optional serialized payload snapshot',
    validation_state VARCHAR(24) NOT NULL DEFAULT 'NotValidated' COMMENT 'NotValidated, Valid, Invalid',
    validation_notes VARCHAR(500) NULL COMMENT 'User-facing validation result shown in Manage Items notes column',
    status VARCHAR(24) NOT NULL DEFAULT 'Waiting' COMMENT 'Waiting, Sent, Failed, Skipped',
    failure_code VARCHAR(120) NULL,
    failure_message VARCHAR(500) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_receiving_scanner_item_session
        FOREIGN KEY (session_id) REFERENCES receiving_scanner_session(id)
        ON DELETE CASCADE,
    UNIQUE KEY uq_receiving_scanner_item_session_order (session_id, item_order),
    INDEX idx_receiving_scanner_item_session (session_id),
    INDEX idx_receiving_scanner_item_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Ordered scanner items in a current list session.';

CREATE TABLE IF NOT EXISTS receiving_scanner_profile (
    id CHAR(36) PRIMARY KEY COMMENT 'Profile GUID',
    user_id VARCHAR(100) NOT NULL,
    profile_name VARCHAR(120) NOT NULL,
    is_default TINYINT(1) NOT NULL DEFAULT 0,
    target_executable_name VARCHAR(255) NOT NULL DEFAULT 'VMINVENT.exe',
    app_window_title VARCHAR(255) NOT NULL,
    target_child_window_title VARCHAR(255) NOT NULL DEFAULT 'Inventory Transfers',
    app_window_class VARCHAR(255) NULL,
    require_exact_title_match TINYINT(1) NOT NULL DEFAULT 0,
    from_warehouse_default VARCHAR(10) NOT NULL DEFAULT '002',
    to_warehouse_default VARCHAR(10) NOT NULL DEFAULT '002',
    activation_delay_ms INT NOT NULL DEFAULT 250,
    delay_between_fields_ms INT NOT NULL DEFAULT 100,
    pause_after_item_ms INT NOT NULL DEFAULT 200,
    popup_timeout_ms INT NOT NULL DEFAULT 3000,
    popup_close_timeout_ms INT NOT NULL DEFAULT 1200,
    send_shortcut_chord VARCHAR(64) NOT NULL DEFAULT 'Ctrl+Alt+M',
    stop_shortcut_chord VARCHAR(64) NOT NULL DEFAULT 'Ctrl+Alt+N',
    allow_advanced_timing TINYINT(1) NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_receiving_scanner_profile_user_name (user_id, profile_name),
    INDEX idx_receiving_scanner_profile_user (user_id),
    INDEX idx_receiving_scanner_profile_default (user_id, is_default)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='User-scoped profile settings for Receiving scanner sending behavior.';
