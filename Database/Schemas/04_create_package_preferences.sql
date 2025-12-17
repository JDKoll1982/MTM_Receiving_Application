-- =============================================
-- Database Schema: Package Type Preferences
-- Feature: 001-receiving-workflow
-- Purpose: Stores user-defined package type preferences per part ID
-- =============================================

-- Table: package_type_preferences
-- Stores package type preferences to remember user's custom choices
CREATE TABLE IF NOT EXISTS package_type_preferences (
    PreferenceID INT PRIMARY KEY AUTO_INCREMENT,
    PartID VARCHAR(50) UNIQUE NOT NULL,
    PackageTypeName VARCHAR(50) NOT NULL,
    CustomTypeName VARCHAR(100) NULL,  -- For custom package types
    LastModified DATETIME NOT NULL,
    
    -- Index for fast lookups by part ID
    INDEX idx_partid (PartID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
