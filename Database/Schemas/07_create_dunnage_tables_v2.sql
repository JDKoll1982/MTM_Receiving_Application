-- =============================================
-- Dunnage Database Schema - v2
-- Feature: 005-dunnage-stored-procedures
-- Description: Re-creates dunnage tables to match approved data model
-- =============================================

SET FOREIGN_KEY_CHECKS = 0;

-- Drop tables in reverse dependency order
DROP TABLE IF EXISTS inventoried_dunnage;
DROP TABLE IF EXISTS dunnage_loads;
DROP TABLE IF EXISTS dunnage_parts;
DROP TABLE IF EXISTS dunnage_specs;

-- Drop legacy tables from previous schema versions to avoid confusion
DROP TABLE IF EXISTS inventoried_dunnage_list;
DROP TABLE IF EXISTS dunnage_part_numbers;

-- Finally drop the base table
DROP TABLE IF EXISTS dunnage_types;

-- Drop new tables if they exist
DROP TABLE IF EXISTS custom_field_definitions;
DROP TABLE IF EXISTS user_preferences;

SET FOREIGN_KEY_CHECKS = 1;

-- =============================================
-- Table 1: dunnage_types
-- Purpose: Type categorization for dunnage
-- =============================================
CREATE TABLE dunnage_types (
    id INT AUTO_INCREMENT PRIMARY KEY,
    type_name VARCHAR(100) NOT NULL UNIQUE,
    created_by VARCHAR(50) NOT NULL,
    created_date DATETIME NOT NULL,
    modified_by VARCHAR(50),
    modified_date DATETIME
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Table 2: dunnage_specs
-- Purpose: Dynamic schema definitions per dunnage type
-- =============================================
CREATE TABLE dunnage_specs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    type_id INT NOT NULL,
    spec_key VARCHAR(100) NOT NULL,
    spec_value JSON,
    created_by VARCHAR(50) NOT NULL,
    created_date DATETIME NOT NULL,
    modified_by VARCHAR(50),
    modified_date DATETIME,
    CONSTRAINT FK_dunnage_specs_type_id 
        FOREIGN KEY (type_id) 
        REFERENCES dunnage_types(id) 
        ON DELETE CASCADE,
    UNIQUE KEY UK_dunnage_specs_type_key (type_id, spec_key)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Table 3: dunnage_parts
-- Purpose: Master data for physical items
-- =============================================
CREATE TABLE dunnage_parts (
    id INT AUTO_INCREMENT PRIMARY KEY,
    part_id VARCHAR(50) NOT NULL UNIQUE,
    type_id INT NOT NULL,
    spec_values JSON,
    created_by VARCHAR(50) NOT NULL,
    created_date DATETIME NOT NULL,
    modified_by VARCHAR(50),
    modified_date DATETIME,
    CONSTRAINT FK_dunnage_parts_type_id 
        FOREIGN KEY (type_id) 
        REFERENCES dunnage_types(id) 
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Table 4: dunnage_loads
-- Purpose: Transaction records of received items
-- =============================================
CREATE TABLE dunnage_loads (
    load_uuid CHAR(36) PRIMARY KEY,
    part_id VARCHAR(50) NOT NULL,
    quantity DECIMAL(10,2) NOT NULL,
    received_date DATETIME NOT NULL,
    created_by VARCHAR(50) NOT NULL,
    created_date DATETIME NOT NULL,
    modified_by VARCHAR(50),
    modified_date DATETIME,
    CONSTRAINT FK_dunnage_loads_part_id 
        FOREIGN KEY (part_id) 
        REFERENCES dunnage_parts(part_id) 
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Table 5: inventoried_dunnage
-- Purpose: Parts requiring Visual ERP inventory notification
-- =============================================
CREATE TABLE inventoried_dunnage (
    id INT AUTO_INCREMENT PRIMARY KEY,
    part_id VARCHAR(50) NOT NULL,
    inventory_method VARCHAR(100),
    notes TEXT,
    created_by VARCHAR(50) NOT NULL,
    created_date DATETIME NOT NULL,
    modified_by VARCHAR(50),
    modified_date DATETIME,
    CONSTRAINT FK_inventoried_dunnage_part_id 
        FOREIGN KEY (part_id) 
        REFERENCES dunnage_parts(part_id) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
