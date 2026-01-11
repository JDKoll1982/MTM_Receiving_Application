-- ============================================================================
-- Table: dunnage_specs
-- Module: Dunnage
-- Purpose: Dynamic schema definitions per dunnage type
-- ============================================================================

SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS dunnage_specs;
SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE dunnage_specs (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Primary key for dunnage specifications',
    type_id INT NOT NULL COMMENT 'Foreign key to dunnage_types table',
    spec_key VARCHAR(100) NOT NULL COMMENT 'Specification attribute name (e.g., length, width, material)',
    spec_value JSON COMMENT 'JSON value for the specification (supports flexible data types)',
    created_by VARCHAR(50) NOT NULL COMMENT 'Username who created this record',
    created_date DATETIME NOT NULL COMMENT 'Timestamp when record was created',
    modified_by VARCHAR(50) COMMENT 'Username who last modified this record',
    modified_date DATETIME COMMENT 'Timestamp when record was last modified',
    CONSTRAINT FK_dunnage_specs_type_id
        FOREIGN KEY (type_id)
        REFERENCES dunnage_types(id)
        ON DELETE CASCADE,
    UNIQUE KEY UK_dunnage_specs_type_key (type_id, spec_key)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Dynamic specifications and attributes for each dunnage type';

-- ============================================================================
