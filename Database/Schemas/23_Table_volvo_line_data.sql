-- ============================================================================
-- Table: volvo_line_data
-- Module: Volvo
-- Purpose: Individual part line items within a shipment
-- ============================================================================

DROP TABLE IF EXISTS volvo_line_data;

CREATE TABLE IF NOT EXISTS volvo_line_data (
    id INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for volvo_line_data',
    shipment_id INT NOT NULL COMMENT 'FK to volvo_label_data.id (shipment header)',
    part_number VARCHAR(20) NOT NULL COMMENT 'From volvo_masterdata',
    quantity_per_skid INT NOT NULL DEFAULT 0 COMMENT 'Cached quantity per skid from master data at time of shipment',
    received_skid_count INT NOT NULL COMMENT 'User-entered actual skid count',
    calculated_piece_count INT NOT NULL COMMENT 'Stored snapshot of calculated piece count from component explosion',
    has_discrepancy TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Flag indicating mismatch between expected and received counts (0=false,1=true)',
    expected_skid_count INT NULL COMMENT 'Volvo packlist quantity if discrepancy exists (nullable)',
    discrepancy_note TEXT NULL COMMENT 'Optional note describing discrepancy or resolution steps',

    PRIMARY KEY (id),
    INDEX idx_shipment_id (shipment_id) COMMENT 'Index for lookup by shipment_id',
    INDEX idx_part_number (part_number) COMMENT 'Index for lookup by part_number',

    FOREIGN KEY (shipment_id) REFERENCES volvo_label_data(id) ON DELETE CASCADE,
    FOREIGN KEY (part_number) REFERENCES volvo_masterdata(part_number) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Volvo shipment line items';

-- ============================================================================
