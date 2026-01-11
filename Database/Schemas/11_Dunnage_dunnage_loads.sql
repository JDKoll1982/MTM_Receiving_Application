-- ============================================================================
-- Table: dunnage_loads
-- Module: Dunnage
-- Purpose: Transaction records of received items
-- ============================================================================

SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS dunnage_loads;
SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE dunnage_loads (
    load_uuid CHAR(36) PRIMARY KEY COMMENT 'Unique identifier for the load transaction',
    part_id VARCHAR(50) NOT NULL COMMENT 'Foreign key to dunnage_parts',
    quantity DECIMAL(10,2) NOT NULL COMMENT 'Quantity received in this transaction',
    received_date DATETIME NOT NULL COMMENT 'Date and time the dunnage was received',
    created_by VARCHAR(50) NOT NULL COMMENT 'Username of user who created the record',
    created_date DATETIME NOT NULL COMMENT 'Timestamp when record was created',
    modified_by VARCHAR(50) COMMENT 'Username of user who last modified the record',
    modified_date DATETIME COMMENT 'Timestamp when record was last modified',
    INDEX IDX_LOADS_DATE (received_date) COMMENT 'Edit Mode date range filtering',
    INDEX IDX_LOADS_USER (created_by) COMMENT 'Edit Mode user filtering',
    CONSTRAINT FK_dunnage_loads_part_id
        FOREIGN KEY (part_id)
        REFERENCES dunnage_parts(part_id)
        ON DELETE RESTRICT
        COMMENT 'Ensures load references a valid dunnage part'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Transaction records of received dunnage items';

-- ============================================================================
