-- =============================================
-- Receiving Workflow Consolidation - Session Management
-- Feature: copilot/follow-instructions-in-agent-files
-- Table: receiving_workflow_sessions
-- Purpose: Tracks workflow sessions for 3-step Wizard receiving process
-- =============================================

CREATE TABLE IF NOT EXISTS receiving_workflow_sessions (
    -- Identity
    session_id CHAR(36) PRIMARY KEY COMMENT 'UUID for workflow session',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Session creation timestamp',
    last_modified_at DATETIME COMMENT 'Last modification timestamp',
    
    -- Workflow State
    current_step TINYINT NOT NULL DEFAULT 1 COMMENT 'Current step: 1=Order&Part, 2=LoadDetails, 3=Review',
    is_edit_mode BOOLEAN NOT NULL DEFAULT FALSE COMMENT 'True if editing from review step',
    has_unsaved_changes BOOLEAN NOT NULL DEFAULT TRUE COMMENT 'Tracks unsaved changes',
    
    -- Step 1 Data (Order & Part Selection)
    po_number VARCHAR(50) COMMENT 'Purchase order number (null for non-PO mode)',
    part_id INT COMMENT 'Part ID from Infor Visual',
    part_number VARCHAR(50) COMMENT 'Part number for display',
    part_description VARCHAR(200) COMMENT 'Part description for display',
    load_count INT NOT NULL COMMENT 'Number of loads to receive (1-100)',
    
    -- Copy Operation State
    copy_source_load_number INT NOT NULL DEFAULT 1 COMMENT 'Load number used as copy source',
    last_copy_operation_at DATETIME COMMENT 'Timestamp of last bulk copy operation',
    
    -- Save State
    is_saved BOOLEAN NOT NULL DEFAULT FALSE COMMENT 'True when workflow completed and saved',
    saved_at DATETIME COMMENT 'Timestamp when workflow was saved',
    saved_csv_path VARCHAR(500) COMMENT 'Path to saved CSV file',
    
    -- Indexes for performance
    INDEX idx_created_at (created_at),
    INDEX idx_po_number (po_number),
    INDEX idx_saved_at (saved_at),
    
    -- Constraints
    CONSTRAINT chk_current_step CHECK (current_step BETWEEN 1 AND 3),
    CONSTRAINT chk_load_count CHECK (load_count BETWEEN 1 AND 100)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Workflow sessions for Wizard receiving process';
