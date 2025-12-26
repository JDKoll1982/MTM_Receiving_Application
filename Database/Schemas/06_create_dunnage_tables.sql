-- =============================================
-- Dunnage Database Foundation - Schema Migration
-- Feature: 004-database-foundation
-- Description: Creates 5 new dunnage tables and removes legacy label_table_dunnage
-- MySQL Version: 5.7.24+
-- Character Set: utf8mb4_unicode_ci
-- =============================================

-- Drop legacy table
DROP TABLE IF EXISTS label_table_dunnage;

-- Drop tables in reverse dependency order (for idempotency)
DROP TABLE IF EXISTS dunnage_loads;
DROP TABLE IF EXISTS inventoried_dunnage_list;
DROP TABLE IF EXISTS dunnage_part_numbers;
DROP TABLE IF EXISTS dunnage_specs;
DROP TABLE IF EXISTS dunnage_types;

-- =============================================
-- Table 1: dunnage_types
-- Purpose: Master table for dunnage type classifications
-- =============================================
CREATE TABLE dunnage_types (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    DunnageType VARCHAR(100) NOT NULL UNIQUE,
    EntryDate DATETIME NOT NULL,
    EntryUser VARCHAR(50) NOT NULL,
    AlterDate DATETIME NULL,
    AlterUser VARCHAR(50) NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Table 2: dunnage_specs
-- Purpose: JSON specification schemas for each dunnage type
-- =============================================
CREATE TABLE dunnage_specs (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    DunnageTypeID INT NOT NULL,
    DunnageSpecs JSON NOT NULL,
    SpecAlterDate DATETIME NOT NULL,
    SpecAlterUser VARCHAR(50) NOT NULL,
    CONSTRAINT FK_dunnage_specs_DunnageTypeID 
        FOREIGN KEY (DunnageTypeID) 
        REFERENCES dunnage_types(ID) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Table 3: dunnage_part_numbers
-- Purpose: Master data table for actual part IDs with spec values
-- =============================================
CREATE TABLE dunnage_part_numbers (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    PartID VARCHAR(100) NOT NULL UNIQUE,
    DunnageTypeID INT NOT NULL,
    DunnageSpecValues JSON NOT NULL,
    EntryDate DATETIME NOT NULL,
    EntryUser VARCHAR(50) NOT NULL,
    CONSTRAINT FK_dunnage_part_numbers_DunnageTypeID 
        FOREIGN KEY (DunnageTypeID) 
        REFERENCES dunnage_types(ID) 
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Table 4: inventoried_dunnage_list
-- Purpose: Reference table for parts requiring Visual ERP inventory tracking
-- =============================================
CREATE TABLE inventoried_dunnage_list (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    PartID VARCHAR(100) NOT NULL UNIQUE,
    RequiresInventory BOOLEAN NOT NULL,
    InventoryMethod VARCHAR(50) NULL,
    Notes TEXT NULL,
    DateAdded DATETIME NOT NULL,
    AddedBy VARCHAR(50) NOT NULL,
    CONSTRAINT FK_inventoried_dunnage_list_PartID 
        FOREIGN KEY (PartID) 
        REFERENCES dunnage_part_numbers(PartID) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Table 5: dunnage_loads
-- Purpose: Transaction table for received dunnage loads
-- =============================================
CREATE TABLE dunnage_loads (
    ID CHAR(36) PRIMARY KEY,
    PartID VARCHAR(100) NOT NULL,
    DunnageTypeID INT NOT NULL,
    Quantity DECIMAL(10,2) NOT NULL,
    PONumber VARCHAR(50) NULL,
    ReceivedDate DATETIME NOT NULL,
    UserId VARCHAR(50) NOT NULL,
    Location VARCHAR(100) NULL,
    LabelNumber VARCHAR(50) NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_dunnage_loads_PartID 
        FOREIGN KEY (PartID) 
        REFERENCES dunnage_part_numbers(PartID) 
        ON DELETE RESTRICT,
    CONSTRAINT FK_dunnage_loads_DunnageTypeID 
        FOREIGN KEY (DunnageTypeID) 
        REFERENCES dunnage_types(ID) 
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Indexes for Performance
-- =============================================

-- IDX-001: Fast type lookups by name
CREATE UNIQUE INDEX IDX_dunnage_types_DunnageType 
    ON dunnage_types(DunnageType);

-- IDX-002: Fast spec retrieval per type
CREATE INDEX IDX_dunnage_specs_DunnageTypeID 
    ON dunnage_specs(DunnageTypeID);

-- IDX-003: Fast part lookups (most common query)
CREATE UNIQUE INDEX IDX_dunnage_part_numbers_PartID 
    ON dunnage_part_numbers(PartID);

-- IDX-004: Filtered queries by type
CREATE INDEX IDX_dunnage_part_numbers_DunnageTypeID 
    ON dunnage_part_numbers(DunnageTypeID);

-- IDX-005: Transaction history per part
CREATE INDEX IDX_dunnage_loads_PartID 
    ON dunnage_loads(PartID);

-- IDX-006: Date range filtering for reports
CREATE INDEX IDX_dunnage_loads_ReceivedDate 
    ON dunnage_loads(ReceivedDate);

-- IDX-007: PO-based searches
CREATE INDEX IDX_dunnage_loads_PONumber 
    ON dunnage_loads(PONumber);

-- IDX-008: User-specific query performance
CREATE INDEX IDX_dunnage_loads_UserId 
    ON dunnage_loads(UserId);

-- IDX-009: Fast inventory checks
CREATE UNIQUE INDEX IDX_inventoried_dunnage_list_PartID 
    ON inventoried_dunnage_list(PartID);

-- =============================================
-- Verification Queries (for manual testing)
-- =============================================
-- SELECT 'Tables created successfully' AS Status;
-- SHOW TABLES LIKE 'dunnage%';
-- SHOW CREATE TABLE dunnage_types;
-- SHOW CREATE TABLE dunnage_specs;
-- SHOW CREATE TABLE dunnage_part_numbers;
-- SHOW CREATE TABLE inventoried_dunnage_list;
-- SHOW CREATE TABLE dunnage_loads;
