-- ============================================================================
-- Migration: 38_Migration_receiving_label_queue_history_alignment
-- Module: Receiving
-- Purpose:
--   1) Align receiving_label_data (active label queue) with full workflow payload
--   2) Align receiving_history (archive) with full queue payload + archive metadata
-- Lifecycle:
--   Workflow Complete -> write to receiving_label_data
--   Clear Label Data -> move rows to receiving_history, delete queue rows
-- ============================================================================

-- ==============================
-- Queue table alignment
-- ==============================
ALTER TABLE receiving_label_data
    ADD COLUMN IF NOT EXISTS load_id CHAR(36) NULL COMMENT 'Workflow load UUID',
    ADD COLUMN IF NOT EXISTS load_number INT NULL COMMENT 'Sequential workflow load number',
    ADD COLUMN IF NOT EXISTS part_type VARCHAR(50) NULL COMMENT 'Part classification/category',
    ADD COLUMN IF NOT EXISTS po_line_number VARCHAR(10) NULL COMMENT 'PO line number',
    ADD COLUMN IF NOT EXISTS po_vendor VARCHAR(255) NULL COMMENT 'PO vendor name snapshot',
    ADD COLUMN IF NOT EXISTS po_status VARCHAR(100) NULL COMMENT 'PO status snapshot',
    ADD COLUMN IF NOT EXISTS po_due_date DATE NULL COMMENT 'PO due date snapshot',
    ADD COLUMN IF NOT EXISTS qty_ordered DECIMAL(18,2) NULL COMMENT 'Ordered quantity snapshot',
    ADD COLUMN IF NOT EXISTS unit_of_measure VARCHAR(20) NULL COMMENT 'UOM snapshot',
    ADD COLUMN IF NOT EXISTS remaining_quantity INT NULL COMMENT 'Remaining PO quantity snapshot',
    ADD COLUMN IF NOT EXISTS packages_per_load INT NULL COMMENT 'Packages per load',
    ADD COLUMN IF NOT EXISTS package_type_name VARCHAR(50) NULL COMMENT 'Package type name',
    ADD COLUMN IF NOT EXISTS weight_per_package DECIMAL(18,2) NULL COMMENT 'Calculated weight per package',
    ADD COLUMN IF NOT EXISTS is_non_po_item TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Non-PO item flag',
    ADD COLUMN IF NOT EXISTS received_date DATETIME NULL COMMENT 'Workflow received datetime',
    ADD COLUMN IF NOT EXISTS user_id VARCHAR(100) NULL COMMENT 'Windows user / app user id',
    ADD COLUMN IF NOT EXISTS is_quality_hold_required TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Quality hold required',
    ADD COLUMN IF NOT EXISTS is_quality_hold_acknowledged TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Quality hold acknowledged',
    ADD COLUMN IF NOT EXISTS quality_hold_restriction_type VARCHAR(255) NULL COMMENT 'Quality hold restriction type',
    ADD COLUMN IF NOT EXISTS weight_quantity DECIMAL(18,2) NULL COMMENT 'Workflow weight/quantity value';

ALTER TABLE receiving_label_data
    MODIFY COLUMN po_number VARCHAR(20) NULL COMMENT 'Purchase order number (string-safe for all PO formats)';

CREATE INDEX IF NOT EXISTS idx_load_id ON receiving_label_data (load_id);
CREATE INDEX IF NOT EXISTS idx_received_date ON receiving_label_data (received_date);
CREATE INDEX IF NOT EXISTS idx_user_id ON receiving_label_data (user_id);

-- ==============================
-- History table alignment
-- ==============================
ALTER TABLE receiving_history
    ADD COLUMN IF NOT EXISTS PartDescription VARCHAR(500) NULL COMMENT 'Part description snapshot',
    ADD COLUMN IF NOT EXISTS POVendor VARCHAR(255) NULL COMMENT 'PO vendor snapshot',
    ADD COLUMN IF NOT EXISTS POStatus VARCHAR(100) NULL COMMENT 'PO status snapshot',
    ADD COLUMN IF NOT EXISTS PODueDate DATE NULL COMMENT 'PO due date snapshot',
    ADD COLUMN IF NOT EXISTS QtyOrdered DECIMAL(18,2) NULL COMMENT 'Ordered quantity snapshot',
    ADD COLUMN IF NOT EXISTS UnitOfMeasure VARCHAR(20) NULL COMMENT 'UOM snapshot',
    ADD COLUMN IF NOT EXISTS RemainingQuantity INT NULL COMMENT 'Remaining PO quantity snapshot',
    ADD COLUMN IF NOT EXISTS UserID VARCHAR(100) NULL COMMENT 'Windows user / app user id',
    ADD COLUMN IF NOT EXISTS EmployeeNumber INT NULL COMMENT 'Employee number',
    ADD COLUMN IF NOT EXISTS IsQualityHoldRequired TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Quality hold required',
    ADD COLUMN IF NOT EXISTS IsQualityHoldAcknowledged TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Quality hold acknowledged',
    ADD COLUMN IF NOT EXISTS QualityHoldRestrictionType VARCHAR(255) NULL COMMENT 'Quality hold restriction type',
    ADD COLUMN IF NOT EXISTS ArchivedAt DATETIME NULL COMMENT 'Archive transfer timestamp',
    ADD COLUMN IF NOT EXISTS ArchivedBy VARCHAR(100) NULL COMMENT 'Archive action user',
    ADD COLUMN IF NOT EXISTS ArchiveBatchID CHAR(36) NULL COMMENT 'Archive operation batch id';

CREATE INDEX IF NOT EXISTS idx_employee_number ON receiving_history (EmployeeNumber);
CREATE INDEX IF NOT EXISTS idx_po_due_date ON receiving_history (PODueDate);
CREATE INDEX IF NOT EXISTS idx_archived_at ON receiving_history (ArchivedAt);
CREATE INDEX IF NOT EXISTS idx_archive_batch ON receiving_history (ArchiveBatchID);

-- ============================================================================
