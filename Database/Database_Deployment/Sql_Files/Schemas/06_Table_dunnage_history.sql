SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS dunnage_history;

SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE dunnage_history (
    load_uuid CHAR(36) PRIMARY KEY COMMENT 'Unique identifier for the load transaction',
    part_id VARCHAR(50) NOT NULL COMMENT 'Foreign key to dunnage_parts',
    quantity DECIMAL(10, 2) NOT NULL COMMENT 'Quantity received in this transaction',
    quantity_type VARCHAR(100) NOT NULL DEFAULT 'Quantity' COMMENT 'Quantity label header snapshot preserved when queue rows move to history',
    received_date DATETIME NOT NULL COMMENT 'Date and time the dunnage was received',
    created_by VARCHAR(50) NOT NULL COMMENT 'Username of user who created the record',
    user_id VARCHAR(100) NULL DEFAULT NULL COMMENT 'Application user identifier copied from the active queue row',
    employee_number INT NULL COMMENT '4-digit employee identifier preserved from the active queue row',
    created_date DATETIME NOT NULL COMMENT 'Timestamp when record was created',
    modified_by VARCHAR(50) COMMENT 'Username of user who last modified the record',
    modified_date DATETIME COMMENT 'Timestamp when record was last modified',
    po_number VARCHAR(50) NULL COMMENT 'PO number snapshot; NULL for non-PO items',
    type_id INT NULL COMMENT 'FK to dunnage_types.id; snapshot at time of archive',
    type_name VARCHAR(100) NULL COMMENT 'Type name snapshot (e.g. Corrugated Cardboard)',
    type_icon VARCHAR(100) NULL COMMENT 'MaterialIconKind string snapshot (e.g. PackageVariantClosed)',
    location VARCHAR(100) NULL COMMENT 'Warehouse location for received dunnage',
    label_number VARCHAR(50) NULL COMMENT 'Label number for this row (supports multi-label splits)',
    part_skid_sequence INT NULL COMMENT 'Position of this skid among all skids for the same part in the saved batch',
    part_skid_total INT NULL COMMENT 'Total skids for the same part in the saved batch',
    specs_json JSON NULL COMMENT 'Snapshot of all per-line dynamic spec key/value pairs as JSON object',
    archived_at DATETIME NULL COMMENT 'Timestamp when the row was moved from the active queue to history',
    archived_by VARCHAR(100) NULL COMMENT 'Username of the user who triggered Clear Label Data',
    archive_batch_id CHAR(36) NULL COMMENT 'UUID shared by all rows archived in the same Clear Label Data operation',
    INDEX IDX_LOADS_DATE (received_date) COMMENT 'Edit Mode date range filtering',
    INDEX IDX_LOADS_USER (created_by) COMMENT 'Edit Mode user filtering',
    INDEX IDX_LOADS_EMPLOYEE (employee_number) COMMENT 'Edit Mode employee filtering',
    INDEX IDX_HISTORY_ARCHIVE_BATCH (archive_batch_id) COMMENT 'Clear Label Data archive batch lookups',
    INDEX IDX_HISTORY_PO_NUMBER (po_number) COMMENT 'PO-based dunnage history queries',
    CONSTRAINT FK_dunnage_history_part_id FOREIGN KEY (part_id) REFERENCES dunnage_parts (part_id) ON DELETE RESTRICT
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Transaction records of received dunnage items';