-- =============================================
-- Database Schema: Receiving Tables
-- Feature: 003-database-foundation
-- Purpose: Stores receiving load data with package types
-- =============================================

-- Table: receiving_history
-- Stores individual receiving loads (skids) with all details
CREATE TABLE IF NOT EXISTS receiving_history (
    LoadID CHAR(36) PRIMARY KEY COMMENT 'GUID (UUID) identifier for the receiving load',
    PartID VARCHAR(50) NOT NULL COMMENT 'Internal part identifier (FK to parts table)',
    PartType VARCHAR(50) NOT NULL COMMENT 'Part classification or category',
    PONumber VARCHAR(20) NULL COMMENT 'Purchase Order number (nullable for non-PO items)',
    POLineNumber VARCHAR(10) NOT NULL COMMENT 'Purchase Order line number',
    LoadNumber INT NOT NULL COMMENT 'Sequential load number assigned by receiving',
    WeightQuantity DECIMAL(10,2) NOT NULL CHECK (WeightQuantity > 0) COMMENT 'Total weight for the entire load (units: configured weight unit)',
    HeatLotNumber VARCHAR(50) NOT NULL COMMENT 'Heat or lot tracking number for traceability',
    PackagesPerLoad INT NOT NULL CHECK (PackagesPerLoad > 0) COMMENT 'Number of packages contained in this load',
    PackageTypeName VARCHAR(50) NOT NULL COMMENT 'Human-readable package type name (e.g., pallet, skid, coil)',
    WeightPerPackage DECIMAL(10,2) NOT NULL COMMENT 'Calculated or recorded weight per package (units: configured weight unit)',
    IsNonPOItem BIT NOT NULL DEFAULT 0 COMMENT 'Flag indicating this item was received without a purchase order (1 = non-PO)',
    ReceivedDate DATETIME NOT NULL COMMENT 'Date and time when the load was received by warehouse',
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT 'Record creation timestamp (automatically set)',

    -- Indexes for common queries
    -- Index to speed lookups by PartID
    INDEX idx_partid (PartID),
    -- Index to speed lookups by PONumber (nullable)
    INDEX idx_ponumber (PONumber),
    -- Index to support queries filtered/sorted by ReceivedDate
    INDEX idx_receiveddate (ReceivedDate),
    -- Composite index for queries that filter by ReceivedDate and PartID together
    INDEX idx_receiveddate_part (ReceivedDate, PartID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Receiving loads (skids) table storing package counts, weights, PO linkage, and traceability metadata';

-- =============================================
