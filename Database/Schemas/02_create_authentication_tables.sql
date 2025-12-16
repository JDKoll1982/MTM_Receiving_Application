-- ============================================================================
-- Script: 02_create_authentication_tables.sql
-- Description: Create authentication tables for User Login & Authentication
-- Feature: User Authentication & Login System (001-user-login)
-- Created: December 16, 2025
-- ============================================================================
-- Tables:
--   1. users - Employee authentication and profile data
--   2. workstation_config - Workstation type detection configuration
--   3. departments - Department dropdown options
--   4. user_activity_log - Audit trail for authentication events
-- ============================================================================

USE mtm_receiving_db;

-- ============================================================================
-- Table: departments
-- Purpose: Department configuration for user assignment dropdown
-- ============================================================================
CREATE TABLE IF NOT EXISTS departments (
    department_id INT AUTO_INCREMENT PRIMARY KEY,
    department_name VARCHAR(50) UNIQUE NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    sort_order INT NOT NULL DEFAULT 999,
    created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_departments_active (is_active),
    INDEX idx_departments_sort (sort_order)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Department options for user assignment';

-- ============================================================================
-- Table: users
-- Purpose: Store employee information for authentication and session management
-- ============================================================================
CREATE TABLE IF NOT EXISTS users (
    employee_number INT AUTO_INCREMENT PRIMARY KEY,
    windows_username VARCHAR(50) UNIQUE NOT NULL,
    full_name VARCHAR(100) NOT NULL,
    pin VARCHAR(4) UNIQUE NOT NULL,
    department VARCHAR(50) NOT NULL,
    shift ENUM('1st Shift', '2nd Shift', '3rd Shift') NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    visual_username VARCHAR(50) NULL,
    visual_password VARCHAR(100) NULL,
    created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50) NULL,
    modified_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_users_windows_username (windows_username),
    INDEX idx_users_pin (pin),
    INDEX idx_users_active (is_active),
    
    CONSTRAINT chk_pin_format CHECK (pin REGEXP '^[0-9]{4}$')
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Employee authentication and profile data';

-- ============================================================================
-- Table: workstation_config
-- Purpose: Workstation computer names and types for authentication flow determination
-- ============================================================================
CREATE TABLE IF NOT EXISTS workstation_config (
    config_id INT AUTO_INCREMENT PRIMARY KEY,
    workstation_name VARCHAR(50) UNIQUE NOT NULL,
    workstation_type ENUM('shared_terminal', 'personal_workstation') NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    description VARCHAR(200) NULL,
    created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_workstation_name (workstation_name),
    INDEX idx_workstation_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Workstation type detection configuration';

-- ============================================================================
-- Table: user_activity_log
-- Purpose: Audit trail for authentication events and user actions
-- ============================================================================
CREATE TABLE IF NOT EXISTS user_activity_log (
    log_id INT AUTO_INCREMENT PRIMARY KEY,
    event_type VARCHAR(50) NOT NULL,
    username VARCHAR(50) NULL,
    workstation_name VARCHAR(50) NULL,
    event_timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    details TEXT NULL,
    
    INDEX idx_log_timestamp (event_timestamp),
    INDEX idx_log_username (username),
    INDEX idx_log_event_type (event_type)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Audit trail for authentication events';

-- ============================================================================
-- Initial Data: departments
-- Purpose: Pre-populate department options for user creation dropdown
-- ============================================================================
INSERT INTO departments (department_name, is_active, sort_order) VALUES
('Receiving', TRUE, 1),
('Shipping', TRUE, 2),
('Production', TRUE, 3),
('Quality Control', TRUE, 4),
('Maintenance', TRUE, 5),
('Administration', TRUE, 6),
('Management', TRUE, 7)
ON DUPLICATE KEY UPDATE 
    sort_order = VALUES(sort_order),
    is_active = VALUES(is_active);

-- ============================================================================
-- Initial Data: workstation_config
-- Purpose: Configure shared terminal workstations for PIN authentication
-- ============================================================================
INSERT INTO workstation_config (workstation_name, workstation_type, is_active, description) VALUES
('SHOP2', 'shared_terminal', TRUE, 'Shop floor terminal 2 - Receiving area'),
('MTMDC', 'shared_terminal', TRUE, 'Main shop floor data collection terminal'),
('SHOP-FLOOR-01', 'shared_terminal', TRUE, 'Production floor terminal 1'),
('SHOP-FLOOR-02', 'shared_terminal', TRUE, 'Production floor terminal 2')
ON DUPLICATE KEY UPDATE 
    workstation_type = VALUES(workstation_type),
    is_active = VALUES(is_active),
    description = VALUES(description);

-- ============================================================================
-- Sample Test Data: users
-- Purpose: Test users for development and testing
-- Note: Remove or replace in production deployment
-- ============================================================================
INSERT INTO users (windows_username, full_name, pin, department, shift, is_active, created_by) VALUES
('JSMITH', 'John Smith', '1234', 'Receiving', '1st Shift', TRUE, 'SYSTEM'),
('MJONES', 'Mary Jones', '5678', 'Shipping', '2nd Shift', TRUE, 'SYSTEM'),
('RBROWN', 'Robert Brown', '9012', 'Production', '1st Shift', TRUE, 'SYSTEM'),
('ADMIN', 'System Administrator', '0000', 'Administration', '1st Shift', TRUE, 'SYSTEM')
ON DUPLICATE KEY UPDATE 
    full_name = VALUES(full_name),
    department = VALUES(department),
    shift = VALUES(shift);

-- ============================================================================
-- Verification Queries
-- ============================================================================
-- SELECT 'Departments:', COUNT(*) FROM departments;
-- SELECT 'Workstations:', COUNT(*) FROM workstation_config;
-- SELECT 'Test Users:', COUNT(*) FROM users;
-- SELECT 'Activity Log:', COUNT(*) FROM user_activity_log;

-- ============================================================================
-- Schema Creation Complete
-- ============================================================================
SELECT 'Authentication tables created successfully' AS Status;
