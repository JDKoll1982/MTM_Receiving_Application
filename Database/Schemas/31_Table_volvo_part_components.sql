-- ============================================================================
-- Table: volvo_part_components
-- Module: Volvo
-- Purpose: Defines which components are included with each parent part (component explosion)
-- ============================================================================

DROP TABLE IF EXISTS volvo_part_components; -- Drop existing table if present (safe redeploy)

CREATE TABLE IF NOT EXISTS volvo_part_components (
    id INT NOT NULL AUTO_INCREMENT COMMENT 'Surrogate primary key for this table',
    parent_part_number VARCHAR(20) NOT NULL COMMENT 'Parent part number (references volvo_masterdata.part_number)',
    component_part_number VARCHAR(20) NOT NULL COMMENT 'Component part number (references volvo_masterdata.part_number)',
    quantity INT NOT NULL COMMENT 'How many of this component per parent skid',

    PRIMARY KEY (id),
    UNIQUE KEY unique_parent_component (parent_part_number, component_part_number) COMMENT 'Ensures each parent/component pair is unique',
    INDEX idx_parent (parent_part_number) COMMENT 'Index to speed parent part lookups',
    INDEX idx_component (component_part_number) COMMENT 'Index to speed component part lookups',

    -- Foreign keys: maintain referential integrity with volvo_masterdata
    CONSTRAINT fk_volvo_part_components_parent
        FOREIGN KEY (parent_part_number)
        REFERENCES volvo_masterdata(part_number)
        ON DELETE CASCADE,
    CONSTRAINT fk_volvo_part_components_component
        FOREIGN KEY (component_part_number)
        REFERENCES volvo_masterdata(part_number)
        ON DELETE RESTRICT
) ENGINE=InnoDB
    DEFAULT CHARSET=utf8mb4
    COLLATE=utf8mb4_unicode_ci
    COMMENT='Component explosion for Volvo parts';

-- ============================================================================
