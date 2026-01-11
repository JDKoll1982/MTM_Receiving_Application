-- ============================================================================
-- Table: dunnage_types
-- Module: Dunnage
-- Purpose: Type categorization for dunnage
-- ============================================================================

SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS dunnage_types;
SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE dunnage_types (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Primary key for dunnage type',
    type_name VARCHAR(100) NOT NULL UNIQUE COMMENT 'Display name of dunnage type (e.g., Box, Pallet, Container)',
    icon VARCHAR(50) NOT NULL DEFAULT 'PackageVariantClosed' COMMENT 'MaterialIconKind name for UI display (e.g., PackageVariantClosed, Folder)',
    created_by VARCHAR(50) NOT NULL COMMENT 'Username of user who created this record',
    created_date DATETIME NOT NULL COMMENT 'Timestamp when record was created',
    modified_by VARCHAR(50) COMMENT 'Username of user who last modified this record',
    modified_date DATETIME COMMENT 'Timestamp when record was last modified'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Type categorization for dunnage items';

-- ============================================================================
