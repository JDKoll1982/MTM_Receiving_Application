-- =====================================================
-- Table: volvo_settings
-- =====================================================
-- Purpose: Stores configurable settings for Volvo module
-- Database: mtm_receiving_application
-- =====================================================

CREATE TABLE IF NOT EXISTS volvo_settings (
    setting_key VARCHAR(100) PRIMARY KEY,
    setting_value TEXT NOT NULL,
    setting_type ENUM('String','Integer','Boolean','Path','Enum') NOT NULL,
    category VARCHAR(50) NOT NULL,
    description TEXT,
    default_value TEXT NOT NULL,
    min_value INT NULL,
    max_value INT NULL,
    modified_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    modified_by VARCHAR(50),
    INDEX idx_category (category)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
