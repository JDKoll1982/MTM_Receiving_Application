-- =============================================
-- Receiving Workflow Consolidation - Completed Transactions
-- Feature: copilot/follow-instructions-in-agent-files
-- Table: receiving_completed_transactions
-- Purpose: Persists completed workflow data for historical tracking
-- =============================================

CREATE TABLE IF NOT EXISTS receiving_completed_transactions (
    -- Identity
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Auto-increment primary key',
    session_id CHAR(36) NOT NULL COMMENT 'References original workflow session',
    
    -- Transaction Data
    po_number VARCHAR(50) COMMENT 'Purchase order number',
    part_id INT COMMENT 'Part ID from Infor Visual',
    part_number VARCHAR(50) COMMENT 'Part number',
    load_number INT NOT NULL COMMENT 'Load number within transaction',
    
    -- Load Details (denormalized for historical record)
    weight_or_quantity DECIMAL(10,3) COMMENT 'Weight or quantity',
    heat_lot VARCHAR(50) COMMENT 'Heat lot number',
    package_type VARCHAR(50) COMMENT 'Package type',
    packages_per_load INT COMMENT 'Number of packages',
    
    -- Audit Fields
    csv_file_path VARCHAR(500) COMMENT 'Path to exported CSV file',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Transaction save time',
    created_by VARCHAR(50) COMMENT 'Username who saved the transaction',
    
    -- Indexes for reporting and lookup
    INDEX idx_po_number (po_number),
    INDEX idx_part_number (part_number),
    INDEX idx_created_at (created_at),
    INDEX idx_session_id (session_id),
    INDEX idx_created_by (created_by)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Historical record of completed receiving transactions';
