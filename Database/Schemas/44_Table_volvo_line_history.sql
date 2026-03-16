-- ============================================================================
-- Table: volvo_line_history
-- Module: Volvo
-- Purpose: Archive table for Volvo shipment line items cleared from the active
--          volvo_line_data table via sp_Volvo_LabelData_ClearToHistory.
--          Mirrors all business columns of volvo_line_data plus archive metadata
--          and back-references to both the archived header and the original IDs.
-- Lifecycle: Clear Label Data -> volvo_line_data -> volvo_line_history
-- ============================================================================

DROP TABLE IF EXISTS volvo_line_history;

CREATE TABLE IF NOT EXISTS volvo_line_history (
    id                      INT NOT NULL AUTO_INCREMENT COMMENT 'Surrogate PK for this history record',
    original_id             INT NOT NULL COMMENT 'Preserved PK from volvo_line_data at time of archive',
    shipment_history_id     INT NOT NULL COMMENT 'FK to volvo_label_history.id (archived header)',
    original_shipment_id    INT NOT NULL COMMENT 'Preserved FK from volvo_line_data.shipment_id (original active header ID)',
    part_number             VARCHAR(20) NOT NULL COMMENT 'Volvo part number (from volvo_masterdata at time of entry)',
    quantity_per_skid       INT NOT NULL DEFAULT 0 COMMENT 'Cached quantity per skid from master data at time of shipment',
    received_skid_count     INT NOT NULL COMMENT 'User-entered actual skid count',
    calculated_piece_count  INT NOT NULL COMMENT 'Snapshot of calculated piece count',
    has_discrepancy         TINYINT(1) NOT NULL DEFAULT 0 COMMENT '0=no discrepancy, 1=discrepancy noted',
    expected_skid_count     INT NULL COMMENT 'Packlist quantity if discrepancy existed (nullable)',
    discrepancy_note        TEXT NULL COMMENT 'Note describing discrepancy or resolution',
    archived_at             DATETIME NOT NULL COMMENT 'Timestamp when record was moved to history',
    archived_by             VARCHAR(100) NOT NULL COMMENT 'Employee number who triggered Clear Label Data',
    archive_batch_id        CHAR(36) NOT NULL COMMENT 'UUID shared by all records moved in the same Clear operation',

    PRIMARY KEY (id),
    INDEX idx_shipment_history_id  (shipment_history_id) COMMENT 'Look up lines by archived header ID',
    INDEX idx_original_shipment_id (original_shipment_id) COMMENT 'Look up lines by original active shipment ID',
    INDEX idx_archive_batch_id     (archive_batch_id) COMMENT 'Retrieve all records moved in the same Clear operation',
    INDEX idx_part_number          (part_number) COMMENT 'Filter history by part number',

    CONSTRAINT fk_volvo_line_history_header
        FOREIGN KEY (shipment_history_id)
        REFERENCES volvo_label_history (id)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Archive of Volvo shipment line items cleared from the active queue';

-- ============================================================================
