-- ============================================================================
-- Table: workstation_config
-- Module: Authentication
-- Purpose: Workstation computer names and types for authentication flow determination
-- ============================================================================

CREATE TABLE IF NOT EXISTS workstation_config (
    config_id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for workstation configuration',
    workstation_name VARCHAR(50) UNIQUE NOT NULL COMMENT 'Computer name or hostname for workstation identification',
    workstation_type ENUM('shared_terminal', 'personal_workstation') NOT NULL COMMENT 'Type of workstation: shared_terminal (requires login) or personal_workstation (auto-login)',
    is_active BOOLEAN NOT NULL DEFAULT TRUE COMMENT 'Indicates if this workstation configuration is currently active',
    description VARCHAR(200) NULL COMMENT 'Optional description of the workstation location or purpose',
    created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when this configuration was created',
    modified_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Timestamp of last modification',

    INDEX idx_workstation_name (workstation_name) COMMENT 'Index for fast lookup by workstation name',
    INDEX idx_workstation_active (is_active) COMMENT 'Index for filtering active workstations'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Workstation type detection configuration for authentication flow determination';

-- ============================================================================
