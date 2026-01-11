-- ============================================================================
-- Table: departments
-- Module: Authentication
-- Purpose: Department configuration for user assignment dropdown
-- ============================================================================

CREATE TABLE IF NOT EXISTS departments (
    department_id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for department',
    department_name VARCHAR(50) UNIQUE NOT NULL COMMENT 'Display name of department (must be unique)',
    is_active BOOLEAN NOT NULL DEFAULT TRUE COMMENT 'Whether department is available for selection',
    sort_order INT NOT NULL DEFAULT 999 COMMENT 'Display order in dropdowns (lower = higher)',
    created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when department was created',

    INDEX idx_departments_active (is_active) COMMENT 'Fast filtering of active departments',
    INDEX idx_departments_sort (sort_order) COMMENT 'Optimized sorting for dropdown display'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Department options for user assignment';

-- ============================================================================
-- Initial Data: Pre-populate department options
-- ============================================================================
INSERT INTO departments (department_name, is_active, sort_order) VALUES
('Receiving', TRUE, 1),
('Shipping', TRUE, 2),
('Production', TRUE, 3),
('Quality Control', TRUE, 4),
('Maintenance', TRUE, 5),
('Administration', TRUE, 6),
('Management', TRUE, 7)
ON DUPLICATE KEY UPDATE
    sort_order = VALUES(sort_order),
    is_active = VALUES(is_active);

-- ============================================================================
