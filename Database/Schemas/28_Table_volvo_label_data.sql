-- ============================================================================
-- Table: volvo_label_data
-- Module: Volvo
-- Purpose: Header record for each Volvo dunnage shipment
-- ============================================================================

DROP TABLE IF EXISTS volvo_label_data;

CREATE TABLE IF NOT EXISTS volvo_label_data (
    id INT NOT NULL AUTO_INCREMENT COMMENT 'Surrogate primary key for shipment record',
    shipment_date DATE NOT NULL COMMENT 'Date of the shipment (local business date)',
    shipment_number INT NOT NULL COMMENT 'Auto-increment within same day, resets daily; sequence number for shipment_date',
    po_number VARCHAR(50) NULL COMMENT 'Purchase order number provided by Purchasing (nullable until PO created)',
    receiver_number VARCHAR(50) NULL COMMENT 'Received package/receiver number populated after Infor Visual receiving',
    employee_number VARCHAR(20) NOT NULL COMMENT 'Employee identifier from authentication context who created/owns the shipment',
    notes TEXT NULL COMMENT 'Optional free-text notes about the shipment',
    status ENUM('pending_po', 'completed') NOT NULL DEFAULT 'pending_po' COMMENT 'Current lifecycle status of the shipment',
    created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when record was created',
    modified_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Timestamp when record was last modified',
    is_archived TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Soft-delete flag; 0 = active, 1 = archived',

    PRIMARY KEY (id),
    UNIQUE KEY unique_shipment_per_day (shipment_date, shipment_number) COMMENT 'Ensures shipment_number is unique per shipment_date',
    INDEX idx_status (status) COMMENT 'Index to speed queries filtered by status',
    INDEX idx_shipment_date (shipment_date) COMMENT 'Index to speed queries by shipment_date',
    INDEX idx_po_number (po_number) COMMENT 'Index to speed lookups by PO number'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Volvo dunnage shipment headers';

-- ============================================================================
