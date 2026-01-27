-- ==============================================================================
-- Table: tbl_Receiving_CompletedTransaction
-- Purpose: Historical archive of completed receiving transactions
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE TABLE [dbo].[tbl_Receiving_CompletedTransaction]
(
    -- Primary Key
    [CompletedTransactionId] CHAR(36) NOT NULL,
    
    -- Original Transaction Reference
    [OriginalTransactionId] CHAR(36) NOT NULL,       -- Reference to original transaction
    
    -- Complete Transaction Data (denormalized for historical snapshot)
    [PONumber] NVARCHAR(50) NULL,
    [PartNumber] NVARCHAR(50) NOT NULL,
    [TotalLoads] INT NOT NULL,
    [TotalWeight] DECIMAL(18, 2) NULL,
    [TotalQuantity] INT NULL,
    [WorkflowMode] NVARCHAR(20) NOT NULL,
    
    -- Completion Information
    [CompletedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CompletedBy] NVARCHAR(100) NOT NULL,
    
    -- CSV Export Information
    [CSVFilePath] NVARCHAR(500) NULL,
    [CSVExportDate] DATETIME2 NULL,
    [CSVRowCount] INT NULL,
    
    -- Complete Transaction JSON (full details)
    [TransactionDataJson] NVARCHAR(MAX) NULL,        -- JSON snapshot of entire transaction
    [LinesDataJson] NVARCHAR(MAX) NULL,              -- JSON snapshot of all lines
    
    -- Quality Hold Summary
    [HasQualityHold] BIT NOT NULL DEFAULT 0,
    [QualityHoldCount] INT NOT NULL DEFAULT 0,
    
    -- Modification Tracking
    [HasBeenModified] BIT NOT NULL DEFAULT 0,        -- True if modified in Edit Mode
    [LastModifiedDate] DATETIME2 NULL,
    [LastModifiedBy] NVARCHAR(100) NULL,
    [ModificationCount] INT NOT NULL DEFAULT 0,
    
    -- Re-Export Tracking
    [ReExportCount] INT NOT NULL DEFAULT 0,
    [LastReExportDate] DATETIME2 NULL,
    [LastReExportBy] NVARCHAR(100) NULL,
    
    -- Flags
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    
    -- Audit Fields
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT [PK_Receiving_CompletedTransaction] PRIMARY KEY CLUSTERED ([CompletedTransactionId]),
    CONSTRAINT [UQ_Receiving_CompletedTransaction_OriginalId] UNIQUE ([OriginalTransactionId]),
    CONSTRAINT [CK_Receiving_CompletedTransaction_TotalLoadsPositive] CHECK ([TotalLoads] > 0),
    CONSTRAINT [CK_Receiving_CompletedTransaction_ModificationCountPositive] CHECK ([ModificationCount] >= 0),
    CONSTRAINT [CK_Receiving_CompletedTransaction_ReExportCountPositive] CHECK ([ReExportCount] >= 0)
);
GO

-- Indexes for historical queries
CREATE NONCLUSTERED INDEX [IX_Receiving_CompletedTransaction_PONumber]
    ON [dbo].[tbl_Receiving_CompletedTransaction] ([PONumber] ASC, [CompletedDate] DESC)
    INCLUDE ([PartNumber], [TotalLoads], [CompletedBy])
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_CompletedTransaction_PartNumber]
    ON [dbo].[tbl_Receiving_CompletedTransaction] ([PartNumber] ASC, [CompletedDate] DESC)
    INCLUDE ([PONumber], [TotalLoads], [CompletedBy])
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_CompletedTransaction_CompletedDate]
    ON [dbo].[tbl_Receiving_CompletedTransaction] ([CompletedDate] DESC)
    INCLUDE ([PONumber], [PartNumber], [TotalLoads], [CompletedBy])
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_CompletedTransaction_CompletedBy]
    ON [dbo].[tbl_Receiving_CompletedTransaction] ([CompletedBy] ASC, [CompletedDate] DESC)
    INCLUDE ([PONumber], [PartNumber], [TotalLoads])
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_CompletedTransaction_Modified]
    ON [dbo].[tbl_Receiving_CompletedTransaction] ([HasBeenModified] ASC, [LastModifiedDate] DESC)
    INCLUDE ([PONumber], [PartNumber], [LastModifiedBy])
    WHERE [IsActive] = 1 AND [IsDeleted] = 0 AND [HasBeenModified] = 1;
GO

-- Extended Property
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Historical archive of completed receiving transactions. Stores denormalized snapshot for Edit Mode queries and reporting. Tracks modifications and re-exports.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_CompletedTransaction';
GO
