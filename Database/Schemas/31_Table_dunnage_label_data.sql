-- ============================================================================
-- Table: dunnage_label_data
-- Module: Dunnage
-- Purpose: Active label queue - rows are inserted here when a dunnage workflow
--          completes. LabelView2022 consumes this table to print labels. Rows
--          are moved to dunnage_history (archived) when the user clicks
--          "Clear Label Data" (see sp_Dunnage_LabelData_ClearToHistory).
-- Lifecycle: Workflow Complete -> Insert here -> Clear Label Data -> Move to history
-- ============================================================================

CREATE TABLE IF NOT EXISTS dunnage_label_data (
    id                  INT AUTO_INCREMENT PRIMARY KEY  COMMENT 'Auto-incrementing unique identifier',
    load_uuid           CHAR(36) NOT NULL               COMMENT 'GUID identifying the load transaction from the workflow session',
    part_id             VARCHAR(50) NOT NULL            COMMENT 'Part identifier from dunnage_parts',
    dunnage_type_id     INT NULL                        COMMENT 'FK to dunnage_types.id; snapshot at time of save',
    dunnage_type_name   VARCHAR(100) NULL               COMMENT 'Type name snapshot (e.g. Corrugated Cardboard)',
    dunnage_type_icon   VARCHAR(100) NULL               COMMENT 'MaterialIconKind string snapshot (e.g. PackageVariantClosed)',
    quantity            DECIMAL(10,2) NOT NULL          COMMENT 'Quantity received in this transaction',
    po_number           VARCHAR(50) NULL                COMMENT 'PO number; NULL for non-PO items',
    received_date       DATETIME NOT NULL               COMMENT 'Date and time the dunnage was received',
    user_id             VARCHAR(100) NOT NULL           COMMENT 'Application user identifier (Windows username)',
    location            VARCHAR(100) NULL               COMMENT 'Warehouse location for received dunnage',
    label_number        VARCHAR(50) NULL                COMMENT 'Label number for this row (supports multi-label splits)',
    specs_json          JSON NULL                       COMMENT 'Snapshot of all per-line dynamic spec key/value pairs as JSON object',
    created_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when the queue record was inserted',

    INDEX idx_load_uuid     (load_uuid)     COMMENT 'Lookup by workflow session GUID',
    INDEX idx_part_id       (part_id)       COMMENT 'Part-based queue queries',
    INDEX idx_received_date (received_date) COMMENT 'Date range filtering for queue consumers'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Active dunnage label queue. Rows are moved to dunnage_history on Clear Label Data. Do not write directly to dunnage_history from the workflow.';

-- ============================================================================
