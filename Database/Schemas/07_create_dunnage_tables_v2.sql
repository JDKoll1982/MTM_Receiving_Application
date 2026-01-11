-- =============================================
-- Dunnage Database Schema - v2
-- Feature: 005-dunnage-stored-procedures
-- Description: Re-creates dunnage tables to match approved data model
-- =============================================

SET FOREIGN_KEY_CHECKS = 0;

-- Drop tables in reverse dependency order
DROP TABLE IF EXISTS inventoried_dunnage;
DROP TABLE IF EXISTS dunnage_loads;
DROP TABLE IF EXISTS dunnage_parts;
DROP TABLE IF EXISTS dunnage_specs;

-- Drop legacy tables from previous schema versions to avoid confusion
DROP TABLE IF EXISTS inventoried_dunnage_list;
DROP TABLE IF EXISTS dunnage_part_numbers;

-- Finally drop the base table
DROP TABLE IF EXISTS dunnage_types;

-- Drop new tables if they exist
DROP TABLE IF EXISTS custom_field_definitions;
DROP TABLE IF EXISTS user_preferences;

SET FOREIGN_KEY_CHECKS = 1;

-- =============================================
-- Table 1: dunnage_types
-- Purpose: Type categorization for dunnage
-- =============================================
CREATE TABLE dunnage_types (
    id INT AUTO_INCREMENT PRIMARY KEY,
    type_name VARCHAR(100) NOT NULL UNIQUE,
    icon VARCHAR(50) NOT NULL DEFAULT 'PackageVariantClosed' COMMENT 'MaterialIconKind name (e.g., PackageVariantClosed, Folder)',
    created_by VARCHAR(50) NOT NULL,
    created_date DATETIME NOT NULL,
    modified_by VARCHAR(50),
    modified_date DATETIME
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Table 2: dunnage_specs
-- Purpose: Dynamic schema definitions per dunnage type
-- =============================================
CREATE TABLE dunnage_specs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    type_id INT NOT NULL,
    spec_key VARCHAR(100) NOT NULL,
    spec_value JSON,
    created_by VARCHAR(50) NOT NULL,
    created_date DATETIME NOT NULL,
    modified_by VARCHAR(50),
    modified_date DATETIME,
    CONSTRAINT FK_dunnage_specs_type_id
        FOREIGN KEY (type_id)
        REFERENCES dunnage_types(id)
        ON DELETE CASCADE,
    UNIQUE KEY UK_dunnage_specs_type_key (type_id, spec_key)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Table 3: dunnage_parts
-- Purpose: Master data for physical items
-- =============================================
CREATE TABLE dunnage_parts (
    id INT AUTO_INCREMENT PRIMARY KEY,
    part_id VARCHAR(50) NOT NULL UNIQUE,
    type_id INT NOT NULL,
    spec_values JSON,
    home_location VARCHAR(100) NULL COMMENT 'Default storage location for this dunnage part',
    created_by VARCHAR(50) NOT NULL,
    created_date DATETIME NOT NULL,
    modified_by VARCHAR(50),
    modified_date DATETIME,
    CONSTRAINT FK_dunnage_parts_type_id
        FOREIGN KEY (type_id)
        REFERENCES dunnage_types(id)
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Table 4: dunnage_loads
-- Purpose: Transaction records of received items
-- =============================================
CREATE TABLE dunnage_loads (
    load_uuid CHAR(36) PRIMARY KEY,
    part_id VARCHAR(50) NOT NULL,
    quantity DECIMAL(10,2) NOT NULL,
    received_date DATETIME NOT NULL,
    created_by VARCHAR(50) NOT NULL,
    created_date DATETIME NOT NULL,
    modified_by VARCHAR(50),
    modified_date DATETIME,
    INDEX IDX_LOADS_DATE (received_date) COMMENT 'Edit Mode date range filtering',
    INDEX IDX_LOADS_USER (created_by) COMMENT 'Edit Mode user filtering',
    CONSTRAINT FK_dunnage_loads_part_id
        FOREIGN KEY (part_id)
        REFERENCES dunnage_parts(part_id)
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Table 5: inventoried_dunnage
-- Purpose: Parts requiring Visual ERP inventory notification
-- =============================================
CREATE TABLE inventoried_dunnage (
    id INT AUTO_INCREMENT PRIMARY KEY,
    part_id VARCHAR(50) NOT NULL,
    inventory_method VARCHAR(100),
    notes TEXT,
    created_by VARCHAR(50) NOT NULL,
    created_date DATETIME NOT NULL,
    modified_by VARCHAR(50),
    modified_date DATETIME,
    CONSTRAINT FK_inventoried_dunnage_part_id
        FOREIGN KEY (part_id)
        REFERENCES dunnage_parts(part_id)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Table 6: custom_field_definitions
-- Purpose: User-defined custom fields for dunnage types (Add Type Dialog)
-- =============================================
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

-- =============================================
-- Table 7: user_preferences
-- Purpose: Per-user UI preferences (recently used icons, pagination settings)
-- =============================================
CREATE TABLE IF NOT EXISTS user_preferences (
    ID INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for each preference record',
    UserId VARCHAR(50) NOT NULL COMMENT 'Windows username or employee number',
    PreferenceKey VARCHAR(100) NOT NULL COMMENT 'Preference identifier (e.g., icon_usage_history, pagination_size)',
    PreferenceValue TEXT NOT NULL COMMENT 'JSON value for the preference',
    LastUpdated DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Timestamp of last update',
    UNIQUE KEY IDX_PREF_001 (UserId, PreferenceKey) COMMENT 'One value per user per key'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Per-user UI preferences for icon picker, pagination, etc.';
