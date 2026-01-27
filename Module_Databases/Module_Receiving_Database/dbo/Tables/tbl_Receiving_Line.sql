-- ==============================================================================
-- Table: tbl_Receiving_Line
-- Purpose: Detail/child table for receiving transaction lines
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE TABLE [dbo].[tbl_Receiving_Line]
(
    -- Primary Key
    [LineId] CHAR(36) NOT NULL,
    
    -- Foreign Keys
    [TransactionId] CHAR(36) NOT NULL,
    
    -- Line Information
    [LineNumber] INT NOT NULL,                       -- Sequential line number (1, 2, 3...)
    
    -- Business Data
    [PONumber] NVARCHAR(50) NULL,                    -- Can differ from transaction if edited
    [PartNumber] NVARCHAR(50) NOT NULL,
    [LoadNumber] INT NOT NULL,                       -- Load number (1, 2, 3...)
    [Quantity] INT NULL,                             -- Quantity of pieces
    [Weight] DECIMAL(18, 2) NULL,                    -- Total weight of load
    [HeatLot] NVARCHAR(100) NULL,                    -- Heat lot number
    [PackageType] NVARCHAR(50) NULL,                 -- Skid, Pallet, Box, Bundle, etc.
    [PackagesPerLoad] INT NULL,                      -- Number of packages on this load
    [WeightPerPackage] DECIMAL(18, 2) NULL,          -- Calculated: Weight / PackagesPerLoad
    [ReceivingLocation] NVARCHAR(100) NULL,          -- Warehouse location
    
    -- Part Type (from Settings)
    [PartType] NVARCHAR(50) NULL,                    -- Coil, Flat Stock, Tubing, etc.
    
    -- Non-PO Receiving
    [IsNonPO] BIT NOT NULL DEFAULT 0,
    
    -- Auto-Fill Tracking
    [IsAutoFilled] BIT NOT NULL DEFAULT 0,           -- True if populated by bulk copy
    [AutoFillSource] INT NULL,                       -- Source load number if auto-filled
    
    -- Quality Hold
    [OnQualityHold] BIT NOT NULL DEFAULT 0,
    [QualityHoldReason] NVARCHAR(500) NULL,
    [QualityHoldDate] DATETIME2 NULL,
    [QualityHoldReleasedBy] NVARCHAR(100) NULL,
    [QualityHoldReleaseDate] DATETIME2 NULL,
    
    -- Flags
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    
    -- Audit Fields (Standard across all tables)
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT [PK_Receiving_Line] PRIMARY KEY CLUSTERED ([LineId]),
    CONSTRAINT [FK_Receiving_Line_Transaction] 
        FOREIGN KEY ([TransactionId]) 
        REFERENCES [dbo].[tbl_Receiving_Transaction]([TransactionId])
        ON DELETE CASCADE,
    CONSTRAINT [UQ_Receiving_Line_TransactionLine] 
        UNIQUE ([TransactionId], [LineNumber]),
    CONSTRAINT [CK_Receiving_Line_LineNumberPositive] CHECK ([LineNumber] > 0),
    CONSTRAINT [CK_Receiving_Line_LoadNumberPositive] CHECK ([LoadNumber] > 0),
    CONSTRAINT [CK_Receiving_Line_QuantityPositive] CHECK ([Quantity] IS NULL OR [Quantity] > 0),
    CONSTRAINT [CK_Receiving_Line_WeightPositive] CHECK ([Weight] IS NULL OR [Weight] > 0),
    CONSTRAINT [CK_Receiving_Line_PackagesPerLoadPositive] CHECK ([PackagesPerLoad] IS NULL OR [PackagesPerLoad] > 0)
);
GO

-- Indexes for performance
CREATE NONCLUSTERED INDEX [IX_Receiving_Line_TransactionId]
    ON [dbo].[tbl_Receiving_Line] ([TransactionId] ASC)
    INCLUDE ([LineNumber], [LoadNumber], [Weight])
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_Line_PONumber]
    ON [dbo].[tbl_Receiving_Line] ([PONumber] ASC)
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_Line_PartNumber]
    ON [dbo].[tbl_Receiving_Line] ([PartNumber] ASC)
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_Line_QualityHold]
    ON [dbo].[tbl_Receiving_Line] ([OnQualityHold] ASC, [QualityHoldDate] DESC)
    WHERE [IsActive] = 1 AND [IsDeleted] = 0 AND [OnQualityHold] = 1;
GO

-- Extended Property for documentation
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Detail table for receiving lines. One row per load. Child of tbl_Receiving_Transaction. Contains weight, heat lot, package info.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_Line';
GO
