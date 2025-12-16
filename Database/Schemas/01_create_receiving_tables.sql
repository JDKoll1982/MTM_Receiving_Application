-- Phase 1 Infrastructure: Database Schema
-- Tables: receiving_lines, dunnage_lines, routing_labels
-- Purpose: Store all label types matching Google Sheets structure

-- Table 1: Receiving Lines
CREATE TABLE IF NOT EXISTS receiving_lines (
    id INT AUTO_INCREMENT PRIMARY KEY,
    quantity INT NOT NULL,
    part_id VARCHAR(50) NOT NULL,
    po_number INT NOT NULL,
    employee_number INT NOT NULL,
    heat VARCHAR(100),
    transaction_date DATE NOT NULL,
    initial_location VARCHAR(50),
    coils_on_skid INT,
    label_number INT DEFAULT 1,
    vendor_name VARCHAR(255),
    part_description VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- Indexes for frequently-queried columns
    INDEX idx_part_id (part_id),
    INDEX idx_po_number (po_number),
    INDEX idx_transaction_date (transaction_date),
    INDEX idx_employee_number (employee_number)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table 2: Dunnage Lines
CREATE TABLE IF NOT EXISTS dunnage_lines (
    id INT AUTO_INCREMENT PRIMARY KEY,
    line1 VARCHAR(255) NOT NULL,
    line2 VARCHAR(255),
    po_number INT NOT NULL,
    transaction_date DATE NOT NULL,
    employee_number INT NOT NULL,
    vendor_name VARCHAR(255),
    location VARCHAR(50),
    label_number INT DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- Indexes for frequently-queried columns
    INDEX idx_po_number (po_number),
    INDEX idx_transaction_date (transaction_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table 3: Routing Labels
CREATE TABLE IF NOT EXISTS routing_labels (
    id INT AUTO_INCREMENT PRIMARY KEY,
    deliver_to VARCHAR(255) NOT NULL,
    department VARCHAR(100),
    package_description VARCHAR(500),
    po_number INT,
    work_order_number VARCHAR(50),
    employee_number INT NOT NULL,
    label_number INT DEFAULT 1,
    transaction_date DATE NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- Indexes for frequently-queried columns
    INDEX idx_deliver_to (deliver_to),
    INDEX idx_department (department),
    INDEX idx_transaction_date (transaction_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
