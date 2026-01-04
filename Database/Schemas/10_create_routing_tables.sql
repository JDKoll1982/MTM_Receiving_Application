-- =============================================
-- Schema: Routing Module
-- Purpose: Internal routing label tables for inter-department delivery
-- Database: mtm_receiving_application (MySQL 8.x)
-- Compatibility: MySQL 5.7.24+
-- =============================================

-- Drop existing objects if they exist (for re-deployment)
DROP TABLE IF EXISTS routing_labels;
DROP TABLE IF EXISTS routing_recipients;

-- =============================================
-- Table: routing_recipients
-- Purpose: Recipient lookup with default department for auto-fill
-- =============================================
CREATE TABLE routing_recipients (
  id INT NOT NULL AUTO_INCREMENT,
  name VARCHAR(100) NOT NULL COMMENT 'Recipient name (unique)',
  default_department VARCHAR(100) NULL COMMENT 'Auto-filled when recipient selected',
  is_active TINYINT(1) NOT NULL DEFAULT 1 COMMENT '1 = active, 0 = inactive',
  created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  
  PRIMARY KEY (id),
  UNIQUE KEY unique_name (name),
  INDEX idx_is_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Routing recipient lookup with default departments';

-- =============================================
-- Table: routing_labels
-- Purpose: Internal routing label records
-- Notes: 
--   - label_number auto-increments per day (reset daily in application logic)
--   - is_archived: 0 = active/pending, 1 = archived to history after printing
-- =============================================
CREATE TABLE routing_labels (
  id INT NOT NULL AUTO_INCREMENT,
  label_number INT NOT NULL COMMENT 'Auto-incremented per day (managed by application)',
  deliver_to VARCHAR(100) NOT NULL COMMENT 'Recipient name (references routing_recipients.name)',
  department VARCHAR(100) NOT NULL COMMENT 'Destination department',
  package_description TEXT NULL COMMENT 'Description of package contents',
  po_number VARCHAR(20) NULL COMMENT 'Purchase order number (formatted as PO-######)',
  work_order VARCHAR(50) NULL COMMENT 'Work order number',
  employee_number VARCHAR(20) NOT NULL COMMENT 'Employee who created the label',
  created_date DATE NOT NULL COMMENT 'Date label was created',
  is_archived TINYINT(1) NOT NULL DEFAULT 0 COMMENT '1 = archived to history, 0 = active',
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  
  PRIMARY KEY (id),
  INDEX idx_label_number (label_number),
  INDEX idx_created_date (created_date),
  INDEX idx_is_archived (is_archived),
  INDEX idx_employee_number (employee_number),
  INDEX idx_deliver_to (deliver_to)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Internal routing labels for inter-department delivery';

-- =============================================
-- View: vw_routing_history
-- Purpose: Flattened view for reporting and history display
-- =============================================
CREATE OR REPLACE VIEW vw_routing_history AS
SELECT 
  id,
  label_number,
  deliver_to,
  department,
  package_description,
  po_number,
  work_order as work_order_number,
  employee_number,
  created_date,
  created_at,
  'Routing' as source_module
FROM routing_labels
WHERE is_archived = 1
ORDER BY created_date DESC, label_number ASC;

-- =============================================
-- Test Query: Verify schema creation
-- =============================================
-- SELECT 'routing_recipients table created' AS status;
-- SELECT 'routing_labels table created' AS status;
-- SELECT 'vw_routing_history view created' AS status;
