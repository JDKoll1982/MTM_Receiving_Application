-- =============================================
-- Database Schema: Receiving Tables
-- Feature: 001-receiving-workflow
-- Purpose: Stores receiving load data with package types
-- =============================================

-- Table: receiving_loads
-- Stores individual receiving loads (skids) with all details
CREATE TABLE IF NOT EXISTS receiving_loads (
    LoadID CHAR(36) PRIMARY KEY,
    PartID VARCHAR(50) NOT NULL,
    PartType VARCHAR(50) NOT NULL,
    PONumber VARCHAR(6) NULL,  -- Nullable for non-PO items
    POLineNumber VARCHAR(10) NOT NULL,
    LoadNumber INT NOT NULL,
    WeightQuantity DECIMAL(10,2) NOT NULL CHECK (WeightQuantity > 0),
    HeatLotNumber VARCHAR(50) NOT NULL,
    PackagesPerLoad INT NOT NULL CHECK (PackagesPerLoad > 0),
    PackageTypeName VARCHAR(50) NOT NULL,
    WeightPerPackage DECIMAL(10,2) NOT NULL,
    IsNonPOItem BIT NOT NULL DEFAULT 0,
    ReceivedDate DATETIME NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- Indexes for common queries
    INDEX idx_partid (PartID),
    INDEX idx_ponumber (PONumber),
    INDEX idx_receiveddate (ReceivedDate),
    INDEX idx_receiveddate_part (ReceivedDate, PartID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
