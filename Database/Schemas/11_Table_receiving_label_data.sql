-- ============================================================================
-- Table: receiving_label_data
-- Module: Receiving
-- Purpose: Store receiving line label data
-- ============================================================================

CREATE TABLE IF NOT EXISTS receiving_label_data (
    id                             INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Auto-incrementing unique identifier',
    load_id                        CHAR(36) NULL              COMMENT 'GUID from the ReceivingLoad session object; NULL until the workflow completes',
    load_number                    INT NULL                   COMMENT 'Sequential load number within the session',
    quantity                       INT NOT NULL               COMMENT 'Integer quantity of parts received (rounded from weight/qty)',
    weight_quantity                DECIMAL(18,2) NULL         COMMENT 'Raw weight or quantity value entered by the operator',
    part_id                        VARCHAR(50) NOT NULL       COMMENT 'Part identifier from Infor Visual',
    part_description               VARCHAR(500) NULL          COMMENT 'Part description sourced from Infor Visual',
    part_type                      VARCHAR(50) NULL           COMMENT 'Part type classification (Standard, Coil, etc.)',
    po_number                      VARCHAR(20) NULL           COMMENT 'PO number; VARCHAR to support formats like PO-066914 and mixed alphanumeric POs',
    po_line_number                 VARCHAR(10) NULL           COMMENT 'PO line number from Infor Visual',
    po_vendor                      VARCHAR(255) NULL          COMMENT 'Vendor name from the PO in Infor Visual',
    po_status                      VARCHAR(100) NULL          COMMENT 'PO status from Infor Visual (Open, Closed, etc.)',
    po_due_date                    DATE NULL                  COMMENT 'PO due date from Infor Visual',
    qty_ordered                    DECIMAL(18,2) NULL         COMMENT 'Quantity ordered on the PO line',
    unit_of_measure                VARCHAR(20) NULL           COMMENT 'Unit of measure (EA, LB, FT, etc.)',
    remaining_quantity             INT NULL                   COMMENT 'Remaining open quantity on the PO line',
    employee_number                INT NOT NULL DEFAULT 0     COMMENT 'Employee ID who processed the receiving',
    user_id                        VARCHAR(100) NULL          COMMENT 'Application user identifier',
    heat                           VARCHAR(100) NULL          COMMENT 'Heat/lot number for material traceability',
    received_date                  DATETIME NULL              COMMENT 'Full timestamp when the record was received',
    transaction_date               DATE NOT NULL              COMMENT 'Date portion of the receiving transaction',
    initial_location               VARCHAR(50) NULL           COMMENT 'Initial warehouse location for received parts',
    packages_per_load              INT NULL                   COMMENT 'Number of packages per load/skid',
    package_type_name              VARCHAR(50) NULL           COMMENT 'Package type description (Skid, Box, Coil, etc.)',
    weight_per_package             DECIMAL(18,2) NULL         COMMENT 'Weight of each individual package',
    coils_on_skid                  INT NULL                   COMMENT 'Number of coils on the skid (coil materials only)',
    label_number                   INT NOT NULL DEFAULT 1     COMMENT 'Sequential label number when splitting quantities',
    vendor_name                    VARCHAR(255) NULL          COMMENT 'Vendor name (mirrors po_vendor; used for non-PO items)',
    is_non_po_item                 TINYINT(1) NOT NULL DEFAULT 0  COMMENT '1 when the part has no PO or was not found in Infor Visual',
    is_quality_hold_required       TINYINT(1) NOT NULL DEFAULT 0  COMMENT '1 when the part requires a quality hold acknowledgment',
    is_quality_hold_acknowledged   TINYINT(1) NOT NULL DEFAULT 0  COMMENT '1 when quality hold has been formally acknowledged',
    quality_hold_restriction_type  VARCHAR(255) NULL          COMMENT 'Restriction type code from the quality hold check',
    created_at                     TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when record was created',

    INDEX idx_load_id         (load_id)           COMMENT 'Index for GUID-based session lookups',
    INDEX idx_part_id         (part_id)           COMMENT 'Index for part lookup queries',
    INDEX idx_po_number       (po_number)         COMMENT 'Index for PO-based queries',
    INDEX idx_transaction_date(transaction_date)  COMMENT 'Index for date range queries',
    INDEX idx_employee_number (employee_number)   COMMENT 'Index for employee activity queries'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Active receiving label queue - rows are moved to receiving_history when the workflow completes (see sp_Receiving_LabelData_ClearToHistory).';

-- ============================================================================
