-- ============================================================================
-- Migration: Expand receiving_label_data to full workflow schema
-- Run once against the live database when upgrading from the original
-- minimal 13-column schema to the full 32-column schema required by
-- sp_Receiving_LabelData_Insert.
-- Safe to re-run: each ALTER TABLE uses IF NOT EXISTS / MODIFY only when needed.
-- ============================================================================

USE mtm_receiving_application;

-- 1. Change po_number from INT NOT NULL to VARCHAR(20) NULL
--    (required to support formats like PO-066914 and nullable PO fields)
ALTER TABLE receiving_label_data
    MODIFY COLUMN po_number VARCHAR(20) NULL
        COMMENT 'PO number; VARCHAR to support formats like PO-066914 and mixed alphanumeric POs';

-- 2. Add all missing columns (each wrapped in IF NOT EXISTS via a stored procedure trick)

-- load_id
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'load_id');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN load_id CHAR(36) NULL COMMENT ''GUID from the ReceivingLoad session object'' AFTER id',
    'SELECT ''load_id already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- load_number
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'load_number');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN load_number INT NULL COMMENT ''Sequential load number within the session'' AFTER load_id',
    'SELECT ''load_number already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- weight_quantity
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'weight_quantity');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN weight_quantity DECIMAL(18,2) NULL COMMENT ''Raw weight or quantity value'' AFTER quantity',
    'SELECT ''weight_quantity already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- part_type
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'part_type');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN part_type VARCHAR(50) NULL COMMENT ''Part type classification'' AFTER part_description',
    'SELECT ''part_type already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- po_line_number
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'po_line_number');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN po_line_number VARCHAR(10) NULL COMMENT ''PO line number from Infor Visual'' AFTER po_number',
    'SELECT ''po_line_number already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- po_vendor
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'po_vendor');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN po_vendor VARCHAR(255) NULL COMMENT ''Vendor name from the PO in Infor Visual'' AFTER po_line_number',
    'SELECT ''po_vendor already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- po_status
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'po_status');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN po_status VARCHAR(100) NULL COMMENT ''PO status from Infor Visual'' AFTER po_vendor',
    'SELECT ''po_status already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- po_due_date
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'po_due_date');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN po_due_date DATE NULL COMMENT ''PO due date from Infor Visual'' AFTER po_status',
    'SELECT ''po_due_date already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- qty_ordered
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'qty_ordered');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN qty_ordered DECIMAL(18,2) NULL COMMENT ''Quantity ordered on the PO line'' AFTER po_due_date',
    'SELECT ''qty_ordered already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- unit_of_measure
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'unit_of_measure');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN unit_of_measure VARCHAR(20) NULL COMMENT ''Unit of measure (EA, LB, FT, etc.)'' AFTER qty_ordered',
    'SELECT ''unit_of_measure already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- remaining_quantity
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'remaining_quantity');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN remaining_quantity INT NULL COMMENT ''Remaining open quantity on the PO line'' AFTER unit_of_measure',
    'SELECT ''remaining_quantity already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- user_id
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'user_id');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN user_id VARCHAR(100) NULL COMMENT ''Application user identifier'' AFTER employee_number',
    'SELECT ''user_id already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- received_date
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'received_date');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN received_date DATETIME NULL COMMENT ''Full timestamp when the record was received'' AFTER user_id',
    'SELECT ''received_date already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- packages_per_load
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'packages_per_load');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN packages_per_load INT NULL COMMENT ''Number of packages per load/skid'' AFTER initial_location',
    'SELECT ''packages_per_load already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- package_type_name
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'package_type_name');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN package_type_name VARCHAR(50) NULL COMMENT ''Package type description'' AFTER packages_per_load',
    'SELECT ''package_type_name already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- weight_per_package
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'weight_per_package');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN weight_per_package DECIMAL(18,2) NULL COMMENT ''Weight of each individual package'' AFTER package_type_name',
    'SELECT ''weight_per_package already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- is_non_po_item
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'is_non_po_item');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN is_non_po_item TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''1 when the part has no PO or was not found in Infor Visual'' AFTER vendor_name',
    'SELECT ''is_non_po_item already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- is_quality_hold_required
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'is_quality_hold_required');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN is_quality_hold_required TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''1 when the part requires a quality hold'' AFTER is_non_po_item',
    'SELECT ''is_quality_hold_required already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- is_quality_hold_acknowledged
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'is_quality_hold_acknowledged');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN is_quality_hold_acknowledged TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''1 when quality hold has been acknowledged'' AFTER is_quality_hold_required',
    'SELECT ''is_quality_hold_acknowledged already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- quality_hold_restriction_type
SET @col = (SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND COLUMN_NAME = 'quality_hold_restriction_type');
SET @sql = IF(@col = 0,
    'ALTER TABLE receiving_label_data ADD COLUMN quality_hold_restriction_type VARCHAR(255) NULL COMMENT ''Restriction type code from the quality hold check'' AFTER is_quality_hold_acknowledged',
    'SELECT ''quality_hold_restriction_type already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 3. Add load_id index if not present
SET @idx = (SELECT COUNT(*) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'receiving_label_data' AND INDEX_NAME = 'idx_load_id');
SET @sql = IF(@idx = 0,
    'ALTER TABLE receiving_label_data ADD INDEX idx_load_id (load_id)',
    'SELECT ''idx_load_id already exists'' AS info');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 4. Redeploy the corrected stored procedures
-- (Run sp_Receiving_LabelData_Insert.sql and sp_Receiving_LabelData_ClearToHistory.sql
--  separately via the Deploy script, or paste their contents here.)

SELECT 'Migration complete: receiving_label_data expanded to full workflow schema.' AS result;
