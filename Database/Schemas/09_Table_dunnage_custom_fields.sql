-- ============================================================================
-- Table: dunnage_custom_fields
-- Module: Dunnage
-- Purpose: User-defined custom fields for dunnage types (Add Type Dialog)
-- ============================================================================

SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS dunnage_custom_fields;
SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE IF NOT EXISTS dunnage_custom_fields (
    ID INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for each custom field',
    DunnageTypeID INT NOT NULL COMMENT 'References dunnage_types.id',
    FieldName VARCHAR(100) NOT NULL COMMENT 'Display name of the field (e.g., "Weight (lbs)")',
    DatabaseColumnName VARCHAR(64) NOT NULL COMMENT 'Sanitized column name for database (e.g., "weight_lbs")',
    FieldType VARCHAR(20) NOT NULL COMMENT 'Field type: Text, Number, Date, Boolean',
    DisplayOrder INT NOT NULL COMMENT 'Order of field in UI (1, 2, 3, ...)',
    IsRequired BOOLEAN NOT NULL DEFAULT FALSE COMMENT 'Whether field is mandatory during data entry',
    ValidationRules TEXT NULL COMMENT 'JSON validation rules (e.g., {"min": 1, "max": 9999, "decimals": 2})',
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when field was created',
    CreatedBy VARCHAR(50) NOT NULL COMMENT 'Username of person who created the field',
    FOREIGN KEY (DunnageTypeID) REFERENCES dunnage_types(id) ON DELETE CASCADE,
    UNIQUE KEY IDX_CUSTOM_002 (DunnageTypeID, DatabaseColumnName) COMMENT 'Prevent duplicate columns per type',
    KEY IDX_CUSTOM_001 (DunnageTypeID) COMMENT 'FK lookup performance'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='User-defined custom fields for Add Type Dialog';

-- ============================================================================
