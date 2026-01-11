-- =============================================
-- MTM Receiving Application - Settings System
-- Database Schema
-- =============================================

-- Drop existing tables if they exist (for clean migration)
DROP TABLE IF EXISTS settings_audit_log;
DROP TABLE IF EXISTS user_settings;
DROP TABLE IF EXISTS scheduled_reports;
DROP TABLE IF EXISTS routing_rules;
DROP TABLE IF EXISTS package_types;
DROP TABLE IF EXISTS package_type_mappings;
DROP TABLE IF EXISTS system_settings;

-- =============================================
-- TABLE: system_settings
-- Stores all system-wide configuration settings
-- =============================================
CREATE TABLE system_settings (
    id INT AUTO_INCREMENT PRIMARY KEY,
    
    -- Organization
    category VARCHAR(50) NOT NULL COMMENT 'Settings category (System, Security, Receiving, Dunnage, Routing, Volvo, Reporting, ERP, UserDefaults)',
    sub_category VARCHAR(50) NULL COMMENT 'Optional sub-category for hierarchical organization',
    
    -- Setting identification
    setting_key VARCHAR(100) NOT NULL COMMENT 'Unique key for the setting',
    setting_name VARCHAR(200) NOT NULL COMMENT 'Display name shown in UI',
    description TEXT NULL COMMENT 'Detailed description/help text',
    
    -- Value and type
    setting_value TEXT NULL COMMENT 'Current value (JSON for complex types)',
    default_value TEXT NULL COMMENT 'Factory default value',
    data_type ENUM('string', 'int', 'boolean', 'json', 'path', 'password', 'email') NOT NULL DEFAULT 'string',
    
    -- Access control
    scope ENUM('system', 'user') NOT NULL DEFAULT 'system' COMMENT 'System-wide or user-overridable',
    permission_level ENUM('user', 'operator', 'admin', 'developer', 'superadmin') NOT NULL DEFAULT 'admin',
    is_locked BOOLEAN DEFAULT FALSE COMMENT 'If true, prevents modification',
    is_sensitive BOOLEAN DEFAULT FALSE COMMENT 'If true, encrypt value and mask in UI',
    
    -- Validation
    validation_rules JSON NULL COMMENT 'JSON object with min, max, pattern, allowed_values, etc.',
    
    -- UI hints
    ui_control_type ENUM('textbox', 'numberbox', 'toggleswitch', 'combobox', 'passwordbox', 'folderpicker', 'datagrid') NOT NULL DEFAULT 'textbox',
    ui_order INT DEFAULT 0 COMMENT 'Display order within category',
    
    -- Metadata
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    updated_by INT NULL COMMENT 'FK to users table',
    
    -- Indexes
    UNIQUE KEY unique_setting (category, setting_key),
    INDEX idx_category (category),
    INDEX idx_scope (scope),
    INDEX idx_permission (permission_level)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='System-wide application settings';

-- =============================================
-- TABLE: user_settings
-- User-specific overrides for settings where scope='user'
-- =============================================
CREATE TABLE user_settings (
    id INT AUTO_INCREMENT PRIMARY KEY,
    
    -- User association
    user_id INT NOT NULL COMMENT 'FK to users table',
    
    -- Setting reference
    setting_id INT NOT NULL COMMENT 'FK to system_settings table',
    
    -- Override value
    setting_value TEXT NULL COMMENT 'User-specific override value',
    
    -- Metadata
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Constraints
    FOREIGN KEY (setting_id) REFERENCES system_settings(id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_setting (user_id, setting_id),
    INDEX idx_user (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='User-specific setting overrides';

-- =============================================
-- TABLE: settings_audit_log
-- Tracks all changes to settings for compliance
-- =============================================
CREATE TABLE settings_audit_log (
    id INT AUTO_INCREMENT PRIMARY KEY,
    
    -- What changed
    setting_id INT NOT NULL COMMENT 'FK to system_settings',
    user_setting_id INT NULL COMMENT 'FK to user_settings if user override',
    
    -- Change details
    old_value TEXT NULL,
    new_value TEXT NULL,
    change_type ENUM('create', 'update', 'delete', 'lock', 'unlock', 'reset') NOT NULL,
    
    -- Who and when
    changed_by INT NOT NULL COMMENT 'FK to users table',
    changed_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    -- Context
    ip_address VARCHAR(45) NULL COMMENT 'IP address of user making change',
    workstation_name VARCHAR(100) NULL,
    
    -- Indexes
    FOREIGN KEY (setting_id) REFERENCES system_settings(id) ON DELETE CASCADE,
    INDEX idx_setting (setting_id),
    INDEX idx_user (changed_by),
    INDEX idx_timestamp (changed_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Audit trail for all settings changes';

-- =============================================
-- TABLE: package_type_mappings
-- Maps part prefixes to package types (Receiving module)
-- =============================================
CREATE TABLE package_type_mappings (
    id INT AUTO_INCREMENT PRIMARY KEY,
    
    -- Mapping
    part_prefix VARCHAR(10) NOT NULL COMMENT 'Part number prefix (e.g., MCC, MMF)',
    package_type VARCHAR(50) NOT NULL COMMENT 'Package type label (e.g., Coils, Sheets, Skids)',
    
    -- Metadata
    is_default BOOLEAN DEFAULT FALSE COMMENT 'If true, used when no prefix matches',
    display_order INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    
    -- Timestamps
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_by INT NULL COMMENT 'FK to users table',
    
    -- Constraints
    UNIQUE KEY unique_prefix (part_prefix),
    INDEX idx_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Part prefix to package type mappings for receiving workflow';

-- =============================================
-- TABLE: package_types
-- Master list of package types for CRUD operations
-- =============================================
CREATE TABLE IF NOT EXISTS package_types (
    id INT AUTO_INCREMENT PRIMARY KEY,
    
    -- Package type definition
    name VARCHAR(50) NOT NULL COMMENT 'Display name (e.g., Box, Pallet, Crate)',
    code VARCHAR(20) NOT NULL COMMENT 'Unique code (e.g., BOX, PLT, CRT)',
    
    -- Metadata
    is_active BOOLEAN DEFAULT TRUE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_by INT NULL COMMENT 'FK to users table',
    
    -- Constraints
    UNIQUE KEY unique_name (name),
    UNIQUE KEY unique_code (code),
    INDEX idx_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Master list of package types for receiving operations';

-- =============================================
-- TABLE: routing_rules
-- Auto-routing rules with pattern matching
-- =============================================
CREATE TABLE IF NOT EXISTS routing_rules (
    id INT AUTO_INCREMENT PRIMARY KEY,
    
    -- Rule definition
    match_type ENUM('Part Number', 'Vendor', 'PO Type', 'Part Category') NOT NULL COMMENT 'Type of pattern matching',
    pattern VARCHAR(100) NOT NULL COMMENT 'Wildcard pattern (e.g., VOL-*, *-BOLT)',
    destination_location VARCHAR(50) NOT NULL COMMENT 'Target location code',
    
    -- Priority (lower = higher priority)
    priority INT DEFAULT 50 COMMENT 'Priority (1-100, lower executes first)',
    
    -- Metadata
    is_active BOOLEAN DEFAULT TRUE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_by INT NULL COMMENT 'FK to users table',
    
    -- Constraints
    UNIQUE KEY unique_pattern (match_type, pattern),
    INDEX idx_priority (priority),
    INDEX idx_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Auto-routing rules with pattern matching for routing module';

-- =============================================
-- TABLE: scheduled_reports
-- Scheduled report configurations
-- =============================================
CREATE TABLE IF NOT EXISTS scheduled_reports (
    id INT AUTO_INCREMENT PRIMARY KEY,
    
    -- Report configuration
    report_type VARCHAR(50) NOT NULL COMMENT 'Report name/type',
    schedule VARCHAR(100) NOT NULL COMMENT 'Schedule string (e.g., Daily at 8:00 AM)',
    email_recipients TEXT NULL COMMENT 'Comma-separated email list',
    
    -- Execution tracking
    is_active BOOLEAN DEFAULT TRUE,
    next_run_date DATETIME NULL COMMENT 'Calculated next run time',
    last_run_date DATETIME NULL COMMENT 'Last execution timestamp',
    
    -- Metadata
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_by INT NULL COMMENT 'FK to users table',
    
    -- Indexes
    INDEX idx_next_run (next_run_date),
    INDEX idx_active (is_active),
    INDEX idx_report_type (report_type)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Scheduled report configurations for reporting module';

-- =============================================
-- Initial Data Migration from appsettings.json
-- =============================================

-- System Settings
INSERT IGNORE INTO system_settings (category, setting_key, setting_name, description, setting_value, default_value, data_type, scope, permission_level, is_sensitive, validation_rules, ui_control_type, ui_order) VALUES
-- Dev/Test Category
('System', 'UseInforVisualMockData', 'Use Mock Data', 'If enabled, uses mock data instead of querying Infor Visual database (Development/QA only)', 'false', 'false', 'boolean', 'system', 'developer', FALSE, '{"allowed_values": ["true", "false"]}', 'toggleswitch', 10),
('System', 'DefaultMockPONumber', 'Default Mock PO', 'Default PO number used when mock mode is enabled', 'PO-066868', 'PO-066868', 'string', 'system', 'developer', FALSE, '{"pattern": "^PO-\\\\d+$"}', 'textbox', 20),
('System', 'Environment', 'Environment Name', 'Current environment (Development, Production)', 'Production', 'Production', 'string', 'system', 'superadmin', FALSE, '{"allowed_values": ["Development", "Production"]}', 'combobox', 30),

-- Database Category
('System', 'DatabaseMaxRetries', 'Database Max Retries', 'Maximum retry attempts for transient database failures', '3', '3', 'int', 'system', 'developer', FALSE, '{"min": 1, "max": 10}', 'numberbox', 40),
('System', 'DatabaseRetryDelaysMs', 'Retry Delays (ms)', 'Comma-separated retry delay values in milliseconds', '100,200,400', '100,200,400', 'string', 'system', 'developer', FALSE, NULL, 'textbox', 50),

-- Security & Session Category
('Security', 'SharedTerminalTimeoutMinutes', 'Shared Terminal Timeout', 'Session timeout for shared terminals (minutes)', '15', '15', 'int', 'system', 'admin', FALSE, '{"min": 1, "max": 120}', 'numberbox', 10),
('Security', 'PersonalWorkstationTimeoutMinutes', 'Personal Workstation Timeout', 'Session timeout for personal workstations (minutes)', '30', '30', 'int', 'system', 'admin', FALSE, '{"min": 1, "max": 480}', 'numberbox', 20),
('Security', 'SessionMonitorIntervalSeconds', 'Session Check Interval', 'How often to check session timeout (seconds)', '60', '60', 'int', 'system', 'admin', FALSE, '{"min": 10, "max": 300}', 'numberbox', 30),
('Security', 'LoginMaxAttempts', 'Max Login Attempts', 'Maximum failed PIN attempts before lockout', '3', '3', 'int', 'system', 'admin', FALSE, '{"min": 1, "max": 10}', 'numberbox', 40),
('Security', 'PinLength', 'PIN Length', 'Required PIN length for shared terminals', '4', '4', 'int', 'system', 'admin', FALSE, '{"min": 4, "max": 8}', 'numberbox', 50),
('Security', 'LockoutDelayMs', 'Lockout Delay', 'Delay before closing dialog after lockout (milliseconds)', '5000', '5000', 'int', 'system', 'admin', FALSE, '{"min": 1000, "max": 30000}', 'numberbox', 60),

-- ERP Integration Category
('ERP', 'InforVisualServer', 'SQL Server Host', 'Infor Visual SQL Server hostname or IP', 'VISUAL', 'VISUAL', 'string', 'system', 'superadmin', FALSE, NULL, 'textbox', 10),
('ERP', 'InforVisualDatabase', 'Database Name', 'Infor Visual database name', 'MTMFG', 'MTMFG', 'string', 'system', 'superadmin', FALSE, NULL, 'textbox', 20),
('ERP', 'InforVisualUserId', 'User ID', 'Infor Visual connection username', 'SHOP2', 'SHOP2', 'string', 'system', 'superadmin', TRUE, NULL, 'textbox', 30),
('ERP', 'InforVisualPassword', 'Password', 'Infor Visual connection password (encrypted)', 'SHOP', 'SHOP', 'password', 'system', 'superadmin', TRUE, NULL, 'passwordbox', 40),
('ERP', 'InforVisualSiteId', 'Site/Warehouse ID', 'Default warehouse/site for filtering queries', '002', '002', 'string', 'system', 'admin', FALSE, NULL, 'textbox', 50),
('ERP', 'DefaultUnitOfMeasure', 'Default UOM', 'Default unit of measure when ERP data missing', 'EA', 'EA', 'string', 'system', 'admin', FALSE, NULL, 'textbox', 60),

-- Receiving Module Category
('Receiving', 'DefaultReceivingMode', 'Default Workflow Mode', 'Default receiving workflow mode for new users', 'guided', 'guided', 'string', 'user', 'user', FALSE, '{"allowed_values": ["guided", "manual", "edit"]}', 'combobox', 10),
('Receiving', 'MockAutoLoadDelayMs', 'Mock Auto-Load Delay', 'Delay before auto-loading mock data (milliseconds)', '500', '500', 'int', 'system', 'developer', FALSE, '{"min": 0, "max": 5000}', 'numberbox', 20),
('Receiving', 'ManualEntryGridSelectionDelayMs', 'Grid Selection Delay', 'Delay to allow grid selection/render (milliseconds)', '100', '100', 'int', 'system', 'developer', FALSE, '{"min": 0, "max": 1000}', 'numberbox', 30),

-- Dunnage Module Category
('Dunnage', 'DefaultDunnageMode', 'Default Workflow Mode', 'Default dunnage workflow mode for new users', 'guided', 'guided', 'string', 'user', 'user', FALSE, '{"allowed_values": ["guided", "manual", "edit"]}', 'combobox', 10),
('Dunnage', 'ManualEntryGridSelectionDelayMs', 'Grid Selection Delay', 'Delay to allow grid selection/render (milliseconds)', '100', '100', 'int', 'system', 'developer', FALSE, '{"min": 0, "max": 1000}', 'numberbox', 20),

-- Routing Module Category
('Routing', 'CsvExportPathNetwork', 'Network CSV Export Path', 'UNC network path for CSV exports', '\\\\server\\share\\routing', '', 'path', 'system', 'admin', FALSE, NULL, 'folderpicker', 10),
('Routing', 'CsvExportPathLocal', 'Local CSV Export Path', 'Local fallback path for CSV exports', '%APPDATA%\\MTM_Receiving_Application\\Routing', '%APPDATA%\\MTM_Receiving_Application\\Routing', 'path', 'system', 'admin', FALSE, NULL, 'folderpicker', 20),
('Routing', 'DefaultMode', 'Default Workflow Mode', 'Default routing mode', 'WIZARD', 'WIZARD', 'string', 'user', 'user', FALSE, '{"allowed_values": ["WIZARD", "MANUAL", "EDIT"]}', 'combobox', 30),
('Routing', 'PersonalizationThreshold', 'Personalization Threshold', 'Threshold for UI personalization behaviors', '20', '20', 'int', 'system', 'admin', FALSE, '{"min": 1, "max": 100}', 'numberbox', 40),
('Routing', 'QuickAddButtonCount', 'Quick-Add Button Count', 'Number of quick-add recipient buttons', '5', '5', 'int', 'system', 'admin', FALSE, '{"min": 1, "max": 10}', 'numberbox', 50),
('Routing', 'HistoryPageSize', 'History Page Size', 'Default history page size', '100', '100', 'int', 'system', 'admin', FALSE, '{"min": 10, "max": 500}', 'numberbox', 60),
('Routing', 'EnableValidation', 'Enable Validation', 'Enable Infor Visual validation checks', 'true', 'true', 'boolean', 'user', 'operator', FALSE, NULL, 'toggleswitch', 70),
('Routing', 'DuplicateDetectionHours', 'Duplicate Detection Window', 'Detection window for duplicate labels (hours)', '24', '24', 'int', 'system', 'admin', FALSE, '{"min": 0, "max": 168}', 'numberbox', 80),
('Routing', 'CsvRetryMaxAttempts', 'CSV Retry Attempts', 'Retry attempts for CSV write failures', '3', '3', 'int', 'system', 'developer', FALSE, '{"min": 1, "max": 10}', 'numberbox', 90),
('Routing', 'CsvRetryDelayMs', 'CSV Retry Delay', 'Delay between CSV retry attempts (milliseconds)', '500', '500', 'int', 'system', 'developer', FALSE, '{"min": 100, "max": 5000}', 'numberbox', 100),

-- Volvo Module Category (DB-driven already, but adding UI settings)
('Volvo', 'HistoryStatusOptions', 'History Status Filter', 'Available status filter options (JSON array)', '["All", "Pending PO", "Completed"]', '["All", "Pending PO", "Completed"]', 'json', 'system', 'admin', FALSE, NULL, 'textbox', 10),

-- Reporting Module Category
('Reporting', 'ExportFolderPath', 'Export Folder Path', 'Default folder for report exports', '%APPDATA%\\MTM_Receiving_Application\\Reports', '%APPDATA%\\MTM_Receiving_Application\\Reports', 'path', 'system', 'admin', FALSE, NULL, 'folderpicker', 10),
('Reporting', 'FileNameTemplate', 'File Name Template', 'Export filename pattern (use {moduleName} and {timestamp})', 'EoD_{moduleName}_{timestamp}.csv', 'EoD_{moduleName}_{timestamp}.csv', 'string', 'system', 'admin', FALSE, NULL, 'textbox', 20),
('Reporting', 'TimestampFormat', 'Timestamp Format', 'Timestamp format for filenames', 'yyyyMMdd_HHmmss', 'yyyyMMdd_HHmmss', 'string', 'system', 'admin', FALSE, NULL, 'textbox', 30),
('Reporting', 'CsvDateFormat', 'CSV Date Format', 'Date format written to CSV files', 'yyyy-MM-dd', 'yyyy-MM-dd', 'string', 'system', 'admin', FALSE, NULL, 'textbox', 40),

-- UI/UX Category
('System', 'NotificationAutoDismissMs', 'Notification Auto-Dismiss', 'Auto-dismiss duration for status messages (milliseconds)', '5000', '5000', 'int', 'system', 'admin', FALSE, '{"min": 1000, "max": 30000}', 'numberbox', 70);

-- =============================================
-- Package Type Mappings Initial Data
-- =============================================
INSERT IGNORE INTO package_type_mappings (part_prefix, package_type, is_default, display_order, is_active) VALUES
('MCC', 'Coils', FALSE, 1, TRUE),
('MMF', 'Sheets', FALSE, 2, TRUE),
('DEFAULT', 'Skids', TRUE, 99, TRUE);

-- =============================================
-- Package Types Initial Data
-- =============================================
INSERT IGNORE INTO package_types (name, code, is_active) VALUES
('Box', 'BOX', TRUE),
('Pallet', 'PLT', TRUE),
('Crate', 'CRT', TRUE),
('Skids', 'SKD', TRUE),
('Coils', 'COIL', TRUE),
('Sheets', 'SHT', TRUE),
('Drums', 'DRM', TRUE),
('Cartons', 'CTN', TRUE);

-- =============================================
-- Routing Rules Initial Data (Examples)
-- =============================================
INSERT IGNORE INTO routing_rules (match_type, pattern, destination_location, priority, is_active) VALUES
('Part Number', 'VOL-*', 'VOLVO-AREA', 10, TRUE),
('Part Number', '*-BOLT', 'HARDWARE', 20, TRUE),
('Vendor', 'ACME*', 'RECEIVING-1', 30, TRUE);

-- =============================================
-- Indexes for Performance
-- =============================================
-- Already created in table definitions above
