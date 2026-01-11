-- =====================================================
-- Migration: 013_allow_duplicate_user_pins
-- Purpose: Remove UNIQUE constraint on users.pin so multiple users may share the same PIN
-- Date: 2026-01-11
-- =====================================================

USE mtm_receiving_application;

-- Drop any UNIQUE index on pin (keep PRIMARY and any non-unique index)
SET @pin_unique_index_name = (
    SELECT s.INDEX_NAME
    FROM INFORMATION_SCHEMA.STATISTICS s
    WHERE s.TABLE_SCHEMA = DATABASE()
      AND s.TABLE_NAME = 'users'
      AND s.COLUMN_NAME = 'pin'
      AND s.NON_UNIQUE = 0
      AND s.INDEX_NAME <> 'PRIMARY'
    LIMIT 1
);

SET @sql = IF(
    @pin_unique_index_name IS NULL,
    'SELECT ''No UNIQUE index on users.pin found'' AS message;',
    CONCAT('ALTER TABLE users DROP INDEX `', @pin_unique_index_name, '`;')
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Ensure a non-unique index exists for performance (idx_users_pin)
SET @pin_index_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.STATISTICS s
    WHERE s.TABLE_SCHEMA = DATABASE()
      AND s.TABLE_NAME = 'users'
      AND s.INDEX_NAME = 'idx_users_pin'
);

SET @sql2 = IF(
    @pin_index_exists = 0,
    'CREATE INDEX idx_users_pin ON users(pin);',
    'SELECT ''Index idx_users_pin already exists'' AS message;'
);

PREPARE stmt2 FROM @sql2;
EXECUTE stmt2;
DEALLOCATE PREPARE stmt2;
