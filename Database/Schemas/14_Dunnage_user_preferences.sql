-- ============================================================================
-- Table: user_preferences
-- Module: Dunnage
-- Purpose: Per-user UI preferences (recently used icons, pagination settings)
-- ============================================================================

SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS user_preferences;
SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE IF NOT EXISTS user_preferences (
    ID INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for each preference record',
    UserId VARCHAR(50) NOT NULL COMMENT 'Windows username or employee number',
    PreferenceKey VARCHAR(100) NOT NULL COMMENT 'Preference identifier (e.g., icon_usage_history, pagination_size)',
    PreferenceValue TEXT NOT NULL COMMENT 'JSON value for the preference',
    LastUpdated DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Timestamp of last update',
    UNIQUE KEY IDX_PREF_001 (UserId, PreferenceKey) COMMENT 'One value per user per key'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Per-user UI preferences for icon picker, pagination, etc.';

-- ============================================================================
