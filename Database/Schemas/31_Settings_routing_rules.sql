-- ============================================================================
-- Table: routing_rules
-- Module: Settings
-- Purpose: Auto-routing rules with pattern matching
-- Notes: Adds column-level COMMENTS and inline comments for indexes/constraints
-- ============================================================================

DROP TABLE IF EXISTS routing_rules;

CREATE TABLE IF NOT EXISTS routing_rules (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Primary key: unique identifier for the routing rule',

    -- Rule definition
    match_type ENUM('Part Number', 'Vendor', 'PO Type', 'Part Category') NOT NULL
        COMMENT 'Type of pattern matching (e.g., Part Number, Vendor, PO Type, Part Category)',
    pattern VARCHAR(100) NOT NULL COMMENT 'Wildcard pattern (e.g., VOL-*, *-BOLT)',
    destination_location VARCHAR(50) NOT NULL COMMENT 'Target location code to route matched items to',

    -- Priority (lower = higher priority)
    priority INT DEFAULT 50 COMMENT 'Priority (1-100, lower values execute first)',

    -- Metadata
    is_active BOOLEAN DEFAULT TRUE COMMENT 'Flag indicating if the rule is currently enabled/active',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT 'Record creation timestamp',
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Record last-updated timestamp',
    created_by INT NULL COMMENT 'FK to users table (creator user id)',

    -- Constraints / Indexes
    UNIQUE KEY unique_pattern (match_type, pattern), -- Ensure no duplicate rule for same match_type/pattern
    INDEX idx_priority (priority),                    -- Index to speed ordering/selection by priority
    INDEX idx_active (is_active)                      -- Index to quickly filter active rules
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Auto-routing rules with pattern matching for routing module';

-- ============================================================================
-- Initial Data (Examples)
-- ============================================================================
INSERT IGNORE INTO routing_rules (match_type, pattern, destination_location, priority, is_active) VALUES
('Part Number', 'VMB-*', 'VOLVO-VITS', 10, TRUE),
('Part Number', 'MMC-*', 'RECV', 20, TRUE),
('Part Number', 'MMF-*', 'T-00', 30, TRUE);

-- ============================================================================
