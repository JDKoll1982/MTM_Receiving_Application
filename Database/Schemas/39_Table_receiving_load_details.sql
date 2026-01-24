-- =============================================
-- Receiving Workflow Consolidation - Load Details
-- Feature: copilot/follow-instructions-in-agent-files
-- Table: receiving_load_details
-- Purpose: Stores individual load details for each workflow session
-- =============================================

CREATE TABLE IF NOT EXISTS receiving_load_details (
    -- Identity
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Auto-increment primary key',
    session_id CHAR(36) NOT NULL COMMENT 'References receiving_workflow_sessions',
    load_number INT NOT NULL COMMENT 'Load number within session (1-based)',
    
    -- Data Fields
    weight_or_quantity DECIMAL(10,3) COMMENT 'Weight or quantity for the load',
    heat_lot VARCHAR(50) COMMENT 'Heat lot number',
    package_type VARCHAR(50) COMMENT 'Package type identifier',
    packages_per_load INT COMMENT 'Number of packages in this load',
    
    -- Auto-Fill Tracking (preserves user data integrity)
    is_weight_auto_filled BOOLEAN NOT NULL DEFAULT FALSE COMMENT 'True if weight was auto-filled by copy operation',
    is_heat_lot_auto_filled BOOLEAN NOT NULL DEFAULT FALSE COMMENT 'True if heat lot was auto-filled',
    is_package_type_auto_filled BOOLEAN NOT NULL DEFAULT FALSE COMMENT 'True if package type was auto-filled',
    is_packages_per_load_auto_filled BOOLEAN NOT NULL DEFAULT FALSE COMMENT 'True if packages/load was auto-filled',
    
    -- Timestamps
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Record creation time',
    updated_at DATETIME ON UPDATE CURRENT_TIMESTAMP COMMENT 'Last update time',
    
    -- Foreign Keys
    FOREIGN KEY (session_id) REFERENCES receiving_workflow_sessions(session_id) ON DELETE CASCADE,
    
    -- Unique Constraints
    UNIQUE KEY unique_session_load (session_id, load_number),
    
    -- Indexes
    INDEX idx_session_id (session_id),
    
    -- Constraints
    CONSTRAINT chk_weight_positive CHECK (weight_or_quantity IS NULL OR weight_or_quantity > 0),
    CONSTRAINT chk_packages_positive CHECK (packages_per_load IS NULL OR packages_per_load > 0),
    CONSTRAINT chk_load_number_positive CHECK (load_number > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Load details for consolidated receiving workflow';
