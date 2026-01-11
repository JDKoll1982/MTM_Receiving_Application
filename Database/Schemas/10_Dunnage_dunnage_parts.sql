-- ============================================================================
-- Table: dunnage_parts
-- Module: Dunnage
-- Purpose: Master data for physical items
-- ============================================================================

SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS dunnage_parts;
SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE dunnage_parts (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for dunnage part',
    part_id VARCHAR(50) NOT NULL UNIQUE COMMENT 'Business identifier (e.g., barcode, SKU) for the dunnage part',
    type_id INT NOT NULL COMMENT 'Foreign key to dunnage_types - defines what kind of dunnage this is',
    spec_values JSON COMMENT 'Dynamic attributes based on type specifications (dimensions, capacity, etc.)',
    home_location VARCHAR(100) NULL COMMENT 'Default storage location for this dunnage part',
    created_by VARCHAR(50) NOT NULL COMMENT 'User who created this record',
    created_date DATETIME NOT NULL COMMENT 'Timestamp when record was created',
    modified_by VARCHAR(50) COMMENT 'User who last modified this record',
    modified_date DATETIME COMMENT 'Timestamp when record was last modified',
    CONSTRAINT FK_dunnage_parts_type_id
        FOREIGN KEY (type_id)
        REFERENCES dunnage_types(id)
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Master data for physical dunnage items - links part IDs to types and specifications';

-- ============================================================================
