-- Migration: Add default_dunnage_mode column to users table
-- Purpose: Store user's preferred default workflow mode for Dunnage feature
-- Date: 2026-01-01
-- Author: System

USE mtm_receiving_application;

-- Add column if it doesn't exist
SET @column_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'users'
    AND COLUMN_NAME = 'default_dunnage_mode'
);

SET @sql = IF(
    @column_exists = 0,
    'ALTER TABLE users ADD COLUMN default_dunnage_mode VARCHAR(20) NULL COMMENT ''Default dunnage workflow mode: "guided", "manual", "edit", or NULL for always showing selection''',
    'SELECT ''Column default_dunnage_mode already exists'' AS message'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SELECT 
    COLUMN_NAME,
    COLUMN_TYPE,
    IS_NULLABLE,
    COLUMN_COMMENT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'users'
    AND COLUMN_NAME = 'default_dunnage_mode';
