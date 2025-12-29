-- ============================================
-- Dunnage Complete Implementation - Schema Extensions
-- Version: 1.0
-- Date: 2025-12-29
-- Feature: 010-dunnage-complete
-- Dependencies: Spec 004 database foundation
-- ============================================

USE mtm_receiving_application;

-- ============================================
-- SECTION 1: MODIFY EXISTING TABLES
-- ============================================

-- Add Icon column to dunnage_types
SET @column_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'mtm_receiving_application' 
    AND TABLE_NAME = 'dunnage_types' 
    AND COLUMN_NAME = 'icon'
);

SET @sql = IF(@column_exists = 0,
    'ALTER TABLE dunnage_types ADD COLUMN icon VARCHAR(10) NOT NULL DEFAULT ''&#xE7B8;'' COMMENT ''Segoe Fluent Icon glyph (e.g., &#xE7B8; = Box)'' AFTER type_name',
    'SELECT ''Column icon already exists in dunnage_types'' AS message'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- NOTE: inventory_method and notes columns are already included in 07_create_dunnage_tables_v2.sql
-- ALTER TABLE inventoried_dunnage
-- ADD COLUMN inventory_method VARCHAR(50) NOT NULL DEFAULT 'Both' COMMENT 'Visual ERP inventory tracking method: Adjust In, Receive In, Both' 
-- AFTER requires_inventory,
-- ADD COLUMN notes TEXT NULL COMMENT 'Optional notes about why part requires inventory tracking' 
-- AFTER inventory_method;

-- ============================================
-- SECTION 2: CREATE NEW TABLES
-- ============================================

-- custom_field_definitions: User-defined custom fields for dunnage types (Add Type Dialog)
CREATE TABLE IF NOT EXISTS custom_field_definitions (
    ID INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for each custom field',
    DunnageTypeID INT NOT NULL COMMENT 'References dunnage_types.ID',
    FieldName VARCHAR(100) NOT NULL COMMENT 'Display name of the field (e.g., "Weight (lbs)")',
    DatabaseColumnName VARCHAR(64) NOT NULL COMMENT 'Sanitized column name for database (e.g., "weight_lbs")',
    FieldType VARCHAR(20) NOT NULL COMMENT 'Field type: Text, Number, Date, Boolean',
    DisplayOrder INT NOT NULL COMMENT 'Order of field in UI (1, 2, 3, ...)',
    IsRequired BOOLEAN NOT NULL DEFAULT FALSE COMMENT 'Whether field is mandatory during data entry',
    ValidationRules TEXT NULL COMMENT 'JSON validation rules (e.g., {"min": 1, "max": 9999, "decimals": 2})',
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when field was created',
    CreatedBy VARCHAR(50) NOT NULL COMMENT 'Username of person who created the field',
    FOREIGN KEY (DunnageTypeID) REFERENCES dunnage_types(ID) ON DELETE CASCADE,
    UNIQUE KEY IDX_CUSTOM_002 (DunnageTypeID, DatabaseColumnName) COMMENT 'Prevent duplicate columns per type',
    KEY IDX_CUSTOM_001 (DunnageTypeID) COMMENT 'FK lookup performance'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='User-defined custom fields for Add Type Dialog';

-- user_preferences: Per-user UI preferences (recently used icons, pagination settings)
CREATE TABLE IF NOT EXISTS user_preferences (
    ID INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for each preference record',
    UserId VARCHAR(50) NOT NULL COMMENT 'Windows username or employee number',
    PreferenceKey VARCHAR(100) NOT NULL COMMENT 'Preference identifier (e.g., icon_usage_history, pagination_size)',
    PreferenceValue TEXT NOT NULL COMMENT 'JSON value for the preference',
    LastUpdated DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Timestamp of last update',
    UNIQUE KEY IDX_PREF_001 (UserId, PreferenceKey) COMMENT 'One value per user per key'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Per-user UI preferences for icon picker, pagination, etc.';

-- ============================================
-- SECTION 3: ADD PERFORMANCE INDEXES
-- ============================================

-- Index for Edit Mode date filtering
SET @index_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = 'mtm_receiving_application' 
    AND TABLE_NAME = 'dunnage_loads' 
    AND INDEX_NAME = 'IDX_LOADS_DATE'
);

SET @sql = IF(@index_exists = 0,
    'CREATE INDEX IDX_LOADS_DATE ON dunnage_loads(received_date) COMMENT ''Edit Mode date range filtering''',
    'SELECT ''Index IDX_LOADS_DATE already exists'' AS message'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Index for Edit Mode user filtering
SET @index_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = 'mtm_receiving_application' 
    AND TABLE_NAME = 'dunnage_loads' 
    AND INDEX_NAME = 'IDX_LOADS_USER'
);

SET @sql = IF(@index_exists = 0,
    'CREATE INDEX IDX_LOADS_USER ON dunnage_loads(created_by) COMMENT ''Edit Mode user filtering''',
    'SELECT ''Index IDX_LOADS_USER already exists'' AS message'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================
-- SECTION 4: OPTIONAL DEFAULT PREFERENCES
-- ============================================

-- Insert default user preferences (commented out - run manually if needed)
-- INSERT INTO user_preferences (UserId, PreferenceKey, PreferenceValue) 
-- VALUES ('DEFAULT', 'pagination_size_admin', '20'),
--        ('DEFAULT', 'pagination_size_edit', '50');

-- ============================================
-- MIGRATION COMPLETE
-- ============================================

-- Verify schema extensions
SELECT 'custom_field_definitions table created' AS Status 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'mtm_receiving_application' 
  AND TABLE_NAME = 'custom_field_definitions';

SELECT 'user_preferences table created' AS Status 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'mtm_receiving_application' 
  AND TABLE_NAME = 'user_preferences';

SELECT 'Icon column added to dunnage_types' AS Status
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'mtm_receiving_application'
  AND TABLE_NAME = 'dunnage_types'
  AND COLUMN_NAME = 'icon';

SELECT 'inventory_method column exists in inventoried_dunnage' AS Status
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'mtm_receiving_application'
  AND TABLE_NAME = 'inventoried_dunnage'
  AND COLUMN_NAME = 'inventory_method';

-- ============================================
-- ROLLBACK SCRIPT (USE ONLY IF MIGRATION FAILS)
-- ============================================

/*
-- Drop new tables
DROP TABLE IF EXISTS custom_field_definitions;
DROP TABLE IF EXISTS user_preferences;

-- Drop indexes
DROP INDEX IF EXISTS IDX_LOADS_DATE ON dunnage_loads;
DROP INDEX IF EXISTS IDX_LOADS_USER ON dunnage_loads;

-- Remove new columns (MySQL 5.7.24 syntax)
ALTER TABLE dunnage_types DROP COLUMN icon;
-- ALTER TABLE inventoried_dunnage DROP COLUMN inventory_method;
-- ALTER TABLE inventoried_dunnage DROP COLUMN notes;
*/

COMMIT;
