-- Add default_mode column to users table
-- This allows users to set a default receiving workflow mode

-- Check if column exists before adding it
SET @column_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'mtm_receiving_application' 
    AND TABLE_NAME = 'users' 
    AND COLUMN_NAME = 'default_receiving_mode'
);

SET @sql = IF(@column_exists = 0,
    'ALTER TABLE users ADD COLUMN default_receiving_mode VARCHAR(20) NULL COMMENT ''Default receiving workflow mode: "guided", "manual", or NULL for always showing selection''',
    'SELECT ''Column default_receiving_mode already exists'' AS message'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Valid values: 'guided', 'manual', or NULL
-- NULL means show mode selection screen every time
