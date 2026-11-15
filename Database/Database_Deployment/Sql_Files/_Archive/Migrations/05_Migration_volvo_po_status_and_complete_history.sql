-- =====================================================
-- Migration: 05_Migration_volvo_po_status_and_complete_history
-- Purpose: Add po_status to Volvo active/history lines and align completion with immediate archive-to-history behavior.
-- Database: mtm_receiving_application
-- Target: MySQL 5.7
-- =====================================================

USE mtm_receiving_application;

SET @line_po_status_exists = (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name = 'volvo_line_data'
      AND column_name = 'po_status'
);

SET @ddl = IF(
    @line_po_status_exists = 0,
    'ALTER TABLE volvo_line_data ADD COLUMN po_status VARCHAR(20) NOT NULL DEFAULT ''Pending'' COMMENT ''Card status: Pending or Received'' AFTER part_number',
    'SELECT ''volvo_line_data.po_status already exists'''
);
PREPARE stmt FROM @ddl;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @line_po_status_index_exists = (
    SELECT COUNT(*)
    FROM information_schema.statistics
    WHERE table_schema = DATABASE()
      AND table_name = 'volvo_line_data'
      AND index_name = 'idx_po_status'
);

SET @ddl = IF(
    @line_po_status_index_exists = 0,
    'ALTER TABLE volvo_line_data ADD INDEX idx_po_status (po_status)',
    'SELECT ''volvo_line_data.idx_po_status already exists'''
);
PREPARE stmt FROM @ddl;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

UPDATE volvo_line_data
SET po_status = 'Pending'
WHERE po_status IS NULL OR TRIM(po_status) = '' OR po_status NOT IN ('Pending', 'Received');

SET @line_history_po_status_exists = (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name = 'volvo_line_history'
      AND column_name = 'po_status'
);

SET @ddl = IF(
    @line_history_po_status_exists = 0,
    'ALTER TABLE volvo_line_history ADD COLUMN po_status VARCHAR(20) NOT NULL DEFAULT ''Received'' COMMENT ''Final card status after completion/archive'' AFTER part_number',
    'SELECT ''volvo_line_history.po_status already exists'''
);
PREPARE stmt FROM @ddl;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

UPDATE volvo_line_history
SET po_status = 'Received'
WHERE po_status IS NULL OR TRIM(po_status) = '' OR po_status NOT IN ('Pending', 'Received');