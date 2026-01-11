-- ============================================================================
-- Table: receiving_package_types
-- Module: Receiving
-- Purpose: Stores user-defined package type preferences per part ID
-- ============================================================================

CREATE TABLE IF NOT EXISTS receiving_package_types (
    PreferenceID INT PRIMARY KEY AUTO_INCREMENT COMMENT 'Unique identifier for each package type preference',
    PartID VARCHAR(50) UNIQUE NOT NULL COMMENT 'Part identifier from Infor Visual (unique constraint ensures one preference per part)',
    PackageTypeName VARCHAR(50) NOT NULL COMMENT 'Selected package type (Box, Pallet, Custom, etc.)',
    CustomTypeName VARCHAR(100) NULL COMMENT 'User-defined custom package type name when PackageTypeName is "Custom"',
    LastModified DATETIME NOT NULL COMMENT 'Timestamp of last preference update',

    -- Index for fast lookups by part ID
    INDEX idx_partid (PartID) COMMENT 'Fast lookup index for retrieving preferences by part ID'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='User-defined package type preferences mapped to part IDs for receiving workflow';

-- ============================================================================
