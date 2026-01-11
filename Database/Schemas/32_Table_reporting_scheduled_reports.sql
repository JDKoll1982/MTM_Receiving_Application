-- ============================================================================
-- Table: reporting_scheduled_reports
-- Module: Settings
-- Purpose: Scheduled report configurations
-- ============================================================================

DROP TABLE IF EXISTS reporting_scheduled_reports;

CREATE TABLE IF NOT EXISTS reporting_scheduled_reports (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Primary key: unique scheduled report identifier',

    -- Report configuration
    report_type VARCHAR(50) NOT NULL COMMENT 'Report name/type',
    schedule VARCHAR(100) NOT NULL COMMENT 'Schedule string (e.g., Daily at 8:00 AM)',
    email_recipients TEXT NULL COMMENT 'Comma-separated email list of recipients',

    -- Execution tracking
    is_active BOOLEAN DEFAULT TRUE COMMENT 'Whether this scheduled report is active/enabled',
    next_run_date DATETIME NULL COMMENT 'Calculated next run time for the schedule',
    last_run_date DATETIME NULL COMMENT 'Last execution timestamp of the report',

    -- Metadata
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT 'Record creation timestamp',
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Record last updated timestamp',
    created_by INT NULL COMMENT 'FK toauth_users table who created this schedule',

    -- Indexes (for query performance; comments describe purpose)
    -- Index on next_run_date to efficiently find upcoming runs
    INDEX idx_next_run (next_run_date),
    -- Index on is_active to quickly filter enabled schedules
    INDEX idx_active (is_active),
    -- Index on report_type to speed lookups by report name/type
    INDEX idx_report_type (report_type)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Scheduled report configurations for reporting module';

-- ============================================================================
