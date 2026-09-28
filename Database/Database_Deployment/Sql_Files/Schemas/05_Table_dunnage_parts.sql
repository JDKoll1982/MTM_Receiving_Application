SET
    FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS dunnage_parts;

SET
    FOREIGN_KEY_CHECKS = 1;

CREATE TABLE dunnage_parts (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for dunnage part',
    part_id VARCHAR(50) NOT NULL UNIQUE COMMENT 'Business identifier (e.g., barcode, SKU) for the dunnage part',
    type_id INT NOT NULL COMMENT 'Foreign key to dunnage_types - defines what kind of dunnage this is',
    udc1 VARCHAR(255) NULL COMMENT 'User Defined Column slot 1 - display name from dunnage_custom_fields',
    udc2 VARCHAR(255) NULL COMMENT 'User Defined Column slot 2 - display name from dunnage_custom_fields',
    udc3 VARCHAR(255) NULL COMMENT 'User Defined Column slot 3 - display name from dunnage_custom_fields',
    udc4 VARCHAR(255) NULL COMMENT 'User Defined Column slot 4 - display name from dunnage_custom_fields',
    udc5 VARCHAR(255) NULL COMMENT 'User Defined Column slot 5 - display name from dunnage_custom_fields',
    udc6 VARCHAR(255) NULL COMMENT 'User Defined Column slot 6 - display name from dunnage_custom_fields',
    udc7 VARCHAR(255) NULL COMMENT 'User Defined Column slot 7 - display name from dunnage_custom_fields',
    udc8 VARCHAR(255) NULL COMMENT 'User Defined Column slot 8 - display name from dunnage_custom_fields',
    udc9 VARCHAR(255) NULL COMMENT 'User Defined Column slot 9 - display name from dunnage_custom_fields',
    udc10 VARCHAR(255) NULL COMMENT 'User Defined Column slot 10 - display name from dunnage_custom_fields',
    image_path VARCHAR(255) NULL COMMENT 'Relative PNG path for app-managed part imagery stored in local app data',
    quantity_type VARCHAR(100) NOT NULL DEFAULT 'Quantity' COMMENT 'Label header used for quantity on printed dunnage labels',
    home_location VARCHAR(100) NULL COMMENT 'Default storage location for this dunnage part',
    created_by VARCHAR(50) NOT NULL COMMENT 'User who created this record',
    created_date DATETIME NOT NULL COMMENT 'Timestamp when record was created',
    modified_by VARCHAR(50) COMMENT 'User who last modified this record',
    modified_date DATETIME COMMENT 'Timestamp when record was last modified',
    CONSTRAINT FK_dunnage_parts_type_id FOREIGN KEY (type_id) REFERENCES dunnage_types (id) ON DELETE RESTRICT
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Master data for physical dunnage items - links part IDs to types and specifications';