-- =====================================================
-- Volvo Dunnage Requisition Module - Database Schema
-- =====================================================
-- Feature: Volvo Dunnage Requisition Module
-- Database: mtm_receiving_application
-- Compatibility: MySQL 5.7.24+
-- Created: 2026-01-04
-- =====================================================

-- Table 1: volvo_shipments
-- Purpose: Header record for each Volvo dunnage shipment
CREATE TABLE IF NOT EXISTS volvo_shipments (
  id INT NOT NULL AUTO_INCREMENT,
  shipment_date DATE NOT NULL,
  shipment_number INT NOT NULL COMMENT 'Auto-increment within same day, resets daily',
  po_number VARCHAR(50) NULL COMMENT 'Filled after purchasing provides PO',
  receiver_number VARCHAR(50) NULL COMMENT 'Filled after Infor Visual receiving',
  employee_number VARCHAR(20) NOT NULL COMMENT 'From authentication context',
  notes TEXT NULL,
  status ENUM('pending_po', 'completed') NOT NULL DEFAULT 'pending_po',
  created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  modified_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  is_archived TINYINT(1) NOT NULL DEFAULT 0,
  
  PRIMARY KEY (id),
  UNIQUE KEY unique_shipment_per_day (shipment_date, shipment_number),
  INDEX idx_status (status),
  INDEX idx_shipment_date (shipment_date),
  INDEX idx_po_number (po_number)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Volvo dunnage shipment headers';

-- Table 2: volvo_shipment_lines
-- Purpose: Individual part line items within a shipment
CREATE TABLE IF NOT EXISTS volvo_shipment_lines (
  id INT NOT NULL AUTO_INCREMENT,
  shipment_id INT NOT NULL,
  part_number VARCHAR(20) NOT NULL COMMENT 'From volvo_parts_master',
  received_skid_count INT NOT NULL COMMENT 'User-entered actual count',
  calculated_piece_count INT NOT NULL COMMENT 'Stored snapshot from component explosion',
  has_discrepancy TINYINT(1) NOT NULL DEFAULT 0,
  expected_skid_count INT NULL COMMENT 'Volvo packlist quantity if discrepancy exists',
  discrepancy_note TEXT NULL,
  
  PRIMARY KEY (id),
  INDEX idx_shipment_id (shipment_id),
  INDEX idx_part_number (part_number),
  
  FOREIGN KEY (shipment_id) REFERENCES volvo_shipments(id) ON DELETE CASCADE,
  FOREIGN KEY (part_number) REFERENCES volvo_parts_master(part_number) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Volvo shipment line items';

-- Table 3: volvo_parts_master
-- Purpose: Catalog of Volvo dunnage parts with quantities per skid
CREATE TABLE IF NOT EXISTS volvo_parts_master (
  part_number VARCHAR(20) NOT NULL,
  quantity_per_skid INT NOT NULL COMMENT 'Pieces per skid for this part',
  is_active TINYINT(1) NOT NULL DEFAULT 1 COMMENT '0=deactivated, hidden from dropdowns',
  created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  modified_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  
  PRIMARY KEY (part_number)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Volvo parts catalog (from DataSheet.csv)';

-- Table 4: volvo_part_components
-- Purpose: Defines which components are included with each parent part (component explosion)
CREATE TABLE IF NOT EXISTS volvo_part_components (
  id INT NOT NULL AUTO_INCREMENT,
  parent_part_number VARCHAR(20) NOT NULL,
  component_part_number VARCHAR(20) NOT NULL,
  quantity INT NOT NULL COMMENT 'How many of this component per parent skid',
  
  PRIMARY KEY (id),
  UNIQUE KEY unique_parent_component (parent_part_number, component_part_number),
  INDEX idx_parent (parent_part_number),
  INDEX idx_component (component_part_number),
  
  FOREIGN KEY (parent_part_number) REFERENCES volvo_parts_master(part_number) ON DELETE CASCADE,
  FOREIGN KEY (component_part_number) REFERENCES volvo_parts_master(part_number) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Component explosion for Volvo parts';

-- View: vw_volvo_shipments_history
-- Purpose: Flattened view for reporting with part details
CREATE OR REPLACE VIEW vw_volvo_shipments_history AS
SELECT 
  s.id as shipment_id,
  s.shipment_date,
  s.shipment_number,
  s.po_number,
  s.receiver_number,
  s.status,
  l.part_number,
  l.received_skid_count,
  l.calculated_piece_count,
  l.has_discrepancy,
  l.expected_skid_count,
  l.discrepancy_note
FROM volvo_shipments s
LEFT JOIN volvo_shipment_lines l ON s.id = l.shipment_id
ORDER BY s.shipment_date DESC, s.shipment_number DESC;
