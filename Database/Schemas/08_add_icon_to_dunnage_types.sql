-- =============================================
-- Add icon column to dunnage_types
-- Feature: Dunnage Type Icons
-- =============================================

-- Check if column exists before adding it
SET @column_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'mtm_receiving_application' 
    AND TABLE_NAME = 'dunnage_types' 
    AND COLUMN_NAME = 'icon'
);

SET @sql = IF(@column_exists = 0,
    'ALTER TABLE dunnage_types ADD COLUMN icon VARCHAR(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL AFTER type_name',
    'SELECT ''Column icon already exists'' AS message'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
