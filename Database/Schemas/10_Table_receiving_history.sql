-- ============================================================================
-- Table: receiving_history
-- Module: Receiving
-- Purpose: Stores all receiving transactions - mirrors receiving_label_data structure
--          Also used as the import target for historical Google Sheets data.
-- Column alignment: matches receiving_label_data exactly, except po_number is
--          VARCHAR(20) to support formats like 'PO-066914' and 'PO-064489B'.
-- Migration: Run the migration block below if upgrading from the pre-2026 schema.
-- ============================================================================

-- ============================================================================
-- MIGRATION BLOCK (run once when upgrading from old schema)
-- This renames the old table and creates the new one. Run manually if needed.
-- ============================================================================
-- RENAME TABLE receiving_history TO receiving_history_v1_backup;
-- ============================================================================

CREATE TABLE IF NOT EXISTS receiving_history (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Auto-incrementing unique identifier',
    load_guid CHAR(36) NULL COMMENT 'GUID identifier for app-generated records; NULL for imported historical records',
    quantity INT NOT NULL COMMENT 'Quantity of parts received on this label/skid',
    part_id VARCHAR(50) NOT NULL COMMENT 'Part identifier from Infor Visual',
    po_number VARCHAR(20) NULL COMMENT 'Purchase order number (VARCHAR to support formats like PO-066914 and PO-064489B)',
    employee_number INT NOT NULL DEFAULT 0 COMMENT 'Employee ID who processed the receiving',
    heat VARCHAR(100) COMMENT 'Heat/lot number for material traceability (optional)',
    transaction_date DATE NOT NULL COMMENT 'Date the receiving transaction occurred',
    initial_location VARCHAR(50) COMMENT 'Initial warehouse location for received parts (optional)',
    coils_on_skid INT COMMENT 'Number of coils on the skid (for coil materials, optional)',
    label_number INT DEFAULT 1 COMMENT 'Sequential label number when splitting quantities (default: 1)',
    vendor_name VARCHAR(255) COMMENT 'Vendor/supplier name (optional, for reference)',
    part_description VARCHAR(500) COMMENT 'Part description from Infor Visual (optional, for reference)',
    is_non_po_item TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Non-PO item flag: 1 when part not found in Infor Visual and no PO number',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when record was created',

    -- Indexes for frequently-queried columns
    UNIQUE INDEX idx_load_guid (load_guid) COMMENT 'Unique index for app GUID lookups (allows NULLs for imported records)',
    INDEX idx_part_id (part_id) COMMENT 'Index for part lookup queries',
    INDEX idx_po_number (po_number) COMMENT 'Index for PO-based queries',
    INDEX idx_transaction_date (transaction_date) COMMENT 'Index for date range queries',
    INDEX idx_employee_number (employee_number) COMMENT 'Index for employee activity queries'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Receiving history - stores all receiving transactions matching receiving_label_data structure';

-- ============================================================================
