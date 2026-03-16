-- ============================================================================
-- Table: volvo_label_history
-- Module: Volvo
-- Purpose: Archive table for completed Volvo shipment headers cleared from
--          the active volvo_label_data queue via sp_Volvo_LabelData_ClearToHistory.
--          Mirrors all business columns of volvo_label_data plus archive metadata.
-- Lifecycle: Clear Label Data -> volvo_label_data (completed) -> volvo_label_history
-- ============================================================================

DROP TABLE IF EXISTS volvo_label_history;

CREATE TABLE IF NOT EXISTS volvo_label_history (
    id                  INT NOT NULL AUTO_INCREMENT COMMENT 'Surrogate PK for this history record',
    original_id         INT NOT NULL COMMENT 'Preserved PK from volvo_label_data at time of archive',
    shipment_date       DATE NOT NULL COMMENT 'Date of the original shipment (local business date)',
    shipment_number     INT NOT NULL COMMENT 'Auto-increment shipment number (within same day)',
    po_number           VARCHAR(50) NULL COMMENT 'Purchase order number (populated when shipment was completed)',
    receiver_number     VARCHAR(50) NULL COMMENT 'Receiver number from Infor Visual',
    employee_number     VARCHAR(20) NOT NULL COMMENT 'Employee identifier who created/owned the shipment',
    notes               TEXT NULL COMMENT 'Free-text notes about the shipment',
    status              VARCHAR(20) NOT NULL COMMENT 'Lifecycle status at time of archive (completed)',
    created_date        DATETIME NOT NULL COMMENT 'Timestamp when original active record was created',
    modified_date       DATETIME NOT NULL COMMENT 'Timestamp of last modification before archive',
    archived_at         DATETIME NOT NULL COMMENT 'Timestamp when record was moved to history by Clear Label Data',
    archived_by         VARCHAR(100) NOT NULL COMMENT 'Employee number who triggered Clear Label Data',
    archive_batch_id    CHAR(36) NOT NULL COMMENT 'UUID shared by all records moved in the same Clear Label Data operation',

    PRIMARY KEY (id),
    INDEX idx_original_id      (original_id) COMMENT 'Look up history by original active-table ID',
    INDEX idx_archive_batch_id (archive_batch_id) COMMENT 'Retrieve all records moved in the same Clear operation',
    INDEX idx_shipment_date    (shipment_date) COMMENT 'Filter history by shipment date',
    INDEX idx_employee_number  (employee_number) COMMENT 'Filter history by employee'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Archive of completed Volvo shipment headers cleared from the active queue';

-- ============================================================================
