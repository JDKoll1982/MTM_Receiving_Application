-- ============================================================================
-- Table: label_table_receiving
-- Module: Receiving
-- Purpose: Store receiving line label data
-- ============================================================================

CREATE TABLE IF NOT EXISTS label_table_receiving (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Primary key, auto-incrementing unique identifier',
    quantity INT NOT NULL COMMENT 'Quantity of parts received on this label',
    part_id VARCHAR(50) NOT NULL COMMENT 'Part identifier from Infor Visual',
    po_number INT NOT NULL COMMENT 'Purchase order number from Infor Visual',
    employee_number INT NOT NULL COMMENT 'Employee ID who processed the receiving',
    heat VARCHAR(100) COMMENT 'Heat/lot number for material traceability (optional)',
    transaction_date DATE NOT NULL COMMENT 'Date the receiving transaction occurred',
    initial_location VARCHAR(50) COMMENT 'Initial warehouse location for received parts (optional)',
    coils_on_skid INT COMMENT 'Number of coils on the skid (for coil materials, optional)',
    label_number INT DEFAULT 1 COMMENT 'Sequential label number when splitting quantities (default: 1)',
    vendor_name VARCHAR(255) COMMENT 'Vendor/supplier name (optional, for reference)',
    part_description VARCHAR(500) COMMENT 'Part description from Infor Visual (optional, for reference)',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when record was created',

    -- Indexes for frequently-queried columns
    INDEX idx_part_id (part_id) COMMENT 'Index for part lookup queries',
    INDEX idx_po_number (po_number) COMMENT 'Index for PO-based queries',
    INDEX idx_transaction_date (transaction_date) COMMENT 'Index for date range queries',
    INDEX idx_employee_number (employee_number) COMMENT 'Index for employee activity queries'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Receiving label data - stores information for each receiving label printed';

-- ============================================================================
