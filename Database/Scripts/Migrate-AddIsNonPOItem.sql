-- ============================================================================
-- Migration: Add is_non_po_item column to receiving_history
-- Purpose:   Tracks rows where the part was not found in Infor Visual and
--            had no PO number. Set automatically by Import-ReceivingHistory.ps1.
-- Run:       Execute once against mtm_receiving_application database.
-- ============================================================================

-- Add column (safe to run multiple times – IF NOT EXISTS guard)
SET @col_exists = (
    SELECT COUNT(*)
    FROM   INFORMATION_SCHEMA.COLUMNS
    WHERE  TABLE_SCHEMA = DATABASE()
      AND  TABLE_NAME   = 'receiving_history'
      AND  COLUMN_NAME  = 'is_non_po_item'
);

SET @sql = IF(
    @col_exists = 0,
    'ALTER TABLE receiving_history ADD COLUMN is_non_po_item TINYINT(1) NOT NULL DEFAULT 0 AFTER part_description',
    'SELECT ''Column is_non_po_item already exists – skipping ALTER TABLE'' AS info'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Re-deploy the updated stored procedure so it accepts the new parameter
SOURCE ../StoredProcedures/Receiving/sp_Receiving_History_Import.sql
