-- ============================================================================
-- Table: volvo_masterdata
-- Module: Volvo
-- Purpose: Catalog of Volvo dunnage parts with quantities per skid
-- ============================================================================

DROP TABLE IF EXISTS volvo_masterdata;

CREATE TABLE IF NOT EXISTS volvo_masterdata (
    part_number VARCHAR(20) NOT NULL COMMENT 'Volvo part identifier (primary key)',
    quantity_per_skid INT NOT NULL COMMENT 'Pieces per skid for this part (from DataSheet.csv)',
    is_active TINYINT(1) NOT NULL DEFAULT 1 COMMENT '0=deactivated, hidden from dropdowns',
    created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Record creation timestamp',
    modified_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Record last modified timestamp',

    PRIMARY KEY (part_number)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
    COMMENT='Volvo parts catalog (from DataSheet.csv)';

-- ============================================================================
