-- ============================================================================
-- Table: users
-- Module: Authentication
-- Purpose: Store employee information for authentication and session management
-- ============================================================================

CREATE TABLE IF NOT EXISTS users (
    employee_number INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for each employee',
    windows_username VARCHAR(50) UNIQUE NOT NULL COMMENT 'Active Directory username for Windows authentication',
    full_name VARCHAR(100) NOT NULL COMMENT 'Employee full name for display purposes',
    pin VARCHAR(4) NOT NULL COMMENT '4-digit PIN for quick authentication at workstations',
    department VARCHAR(50) NOT NULL COMMENT 'Employee department (e.g., Receiving, Shipping, Quality)',
    shift ENUM('1st Shift', '2nd Shift', '3rd Shift') NOT NULL COMMENT 'Employee assigned shift',
    is_active BOOLEAN NOT NULL DEFAULT TRUE COMMENT 'Flag indicating if employee account is active',
    visual_username VARCHAR(50) NULL COMMENT 'Infor Visual ERP username for integration (optional)',
    visual_password VARCHAR(100) NULL COMMENT 'Encrypted Infor Visual password (optional)',
    default_receiving_mode VARCHAR(20) NULL COMMENT 'Default receiving workflow mode: "guided", "manual", or NULL for always showing selection',
    default_dunnage_mode VARCHAR(20) NULL COMMENT 'Default dunnage workflow mode: "guided", "manual", "edit", or NULL for always showing selection',
    created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when user record was created',
    created_by VARCHAR(50) NULL COMMENT 'Username of person who created this record',
    modified_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Timestamp of last modification',

    INDEX idx_users_windows_username (windows_username) COMMENT 'Index for fast Windows authentication lookups',
    INDEX idx_users_pin (pin) COMMENT 'Index for fast PIN authentication lookups',
    INDEX idx_users_active (is_active) COMMENT 'Index for filtering active users',

    CONSTRAINT chk_pin_format CHECK (pin REGEXP '^[0-9]{4}$')
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Employee authentication and profile data';

-- ============================================================================
