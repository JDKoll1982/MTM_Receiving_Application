SET
    FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS dunnage_custom_fields;

SET
    FOREIGN_KEY_CHECKS = 1;

CREATE TABLE IF NOT EXISTS dunnage_custom_fields (
    ID INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for each custom field',
    DunnageTypeID INT NOT NULL COMMENT 'References dunnage_types.id',
    FieldName VARCHAR(100) NOT NULL COMMENT 'Custom display name for the udc slot (shown on the UI)',
    FieldType VARCHAR(20) NOT NULL COMMENT 'Field type: Text, Number, Date, Boolean, Choices',
    DisplayOrder INT NOT NULL COMMENT 'UDC slot (1-10) - maps to udc{DisplayOrder} on dunnage_parts/history/label_data',
    IsRequired BOOLEAN NOT NULL DEFAULT FALSE COMMENT 'Whether field is mandatory during data entry',
    Unit VARCHAR(50) NULL COMMENT 'Unit of measure shown on the UI for this field',
    MinValue DECIMAL(18, 4) NULL COMMENT 'Minimum value enforced when the field is a Number',
    MaxValue DECIMAL(18, 4) NULL COMMENT 'Maximum value enforced when the field is a Number',
    DefaultValue VARCHAR(255) NULL COMMENT 'Default value pre-filled when the field has no stored value',
    ValidationRules TEXT NULL COMMENT 'Reserved for future use',
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when field was created',
    CreatedBy VARCHAR(50) NOT NULL COMMENT 'Username of person who created the field',
    FOREIGN KEY (DunnageTypeID) REFERENCES dunnage_types (id) ON DELETE CASCADE,
    UNIQUE KEY IDX_CUSTOM_001 (DunnageTypeID, DisplayOrder) COMMENT 'One field per udc slot per type',
    UNIQUE KEY IDX_CUSTOM_002 (DunnageTypeID, FieldName) COMMENT 'Distinct display names per type',
    KEY IDX_CUSTOM_003 (DunnageTypeID) COMMENT 'FK lookup performance'
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'User-defined custom fields (UDC slots 1-10) for dunnage types';