-- ==============================================================================
-- Table: tbl_Receiving_Transaction
-- Purpose: Header/parent table for receiving transactions
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE TABLE [dbo].[tbl_Receiving_Transaction]
(
    -- Primary Key
    [TransactionId] CHAR(36) NOT NULL,
    
    -- Business Data
    [PONumber] NVARCHAR(50) NULL,                    -- Format: PO-XXXXXX
    [PartNumber] NVARCHAR(50) NOT NULL,              -- Format: MMC0001000, etc.
    [TotalLoads] INT NOT NULL,                       -- Total number of loads
    [TotalWeight] DECIMAL(18, 2) NULL,               -- Sum of all load weights
    [TotalQuantity] INT NULL,                        -- Sum of all quantities
    
    -- Workflow Information
    [WorkflowMode] NVARCHAR(20) NOT NULL,
    [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Receiving_Transaction_Status] DEFAULT 'Draft',
    [CompletedDate] DATETIME2 NULL,
    
    -- Non-PO Receiving
    [IsNonPO] BIT NOT NULL CONSTRAINT [DF_Receiving_Transaction_IsNonPO] DEFAULT 0,
    
    -- Quality Hold
    [RequiresQualityHold] BIT NOT NULL CONSTRAINT [DF_Receiving_Transaction_RequiresQualityHold] DEFAULT 0,
    [QualityHoldAcknowledged] BIT NOT NULL CONSTRAINT [DF_Receiving_Transaction_QualityHoldAcknowledged] DEFAULT 0,
    
    -- Save Status
    [SaveDataTransferObjectsCSV] BIT NOT NULL CONSTRAINT [DF_Receiving_Transaction_SaveDataTransferObjectsCSV] DEFAULT 0,
    [SaveDataTransferObjectsDatabase] BIT NOT NULL CONSTRAINT [DF_Receiving_Transaction_SaveDataTransferObjectsDatabase] DEFAULT 1,
    [CSVFilePath] NVARCHAR(500) NULL,
    [CSVExportDate] DATETIME2 NULL,
    
    -- Session Information
    [SessionId] CHAR(36) NULL,
    
    -- Flags
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Receiving_Transaction_IsActive] DEFAULT 1,
    [IsDeleted] BIT NOT NULL CONSTRAINT [DF_Receiving_Transaction_IsDeleted] DEFAULT 0,
    
    -- Audit Fields (Standard across all tables)
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL CONSTRAINT [DF_Receiving_Transaction_CreatedDate] DEFAULT GETUTCDATE(),
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT [PK_Receiving_Transaction] PRIMARY KEY CLUSTERED ([TransactionId]),
    CONSTRAINT [CK_Receiving_Transaction_StatusValid] CHECK ([Status] IN ('Draft', 'Completed', 'Cancelled')),
    CONSTRAINT [CK_Receiving_Transaction_WorkflowModeValid] CHECK ([WorkflowMode] IN ('Wizard', 'Manual', 'Edit')),
    CONSTRAINT [CK_Receiving_Transaction_TotalLoadsPositive] CHECK ([TotalLoads] > 0),
    CONSTRAINT [CK_Receiving_Transaction_TotalWeightPositive] CHECK ([TotalWeight] IS NULL OR [TotalWeight] > 0)
);
GO

-- Indexes for performance
CREATE NONCLUSTERED INDEX [IX_Receiving_Transaction_PONumber]
    ON [dbo].[tbl_Receiving_Transaction] ([PONumber] ASC)
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_Transaction_PartNumber]
    ON [dbo].[tbl_Receiving_Transaction] ([PartNumber] ASC)
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_Transaction_CreatedDate_Desc]
    ON [dbo].[tbl_Receiving_Transaction] ([CreatedDate] DESC)
    INCLUDE ([PONumber], [PartNumber], [Status], [WorkflowMode])
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_Transaction_Status]
    ON [dbo].[tbl_Receiving_Transaction] ([Status] ASC)
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

-- Extended Property for documentation
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Header table for receiving transactions. Contains PO, Part, totals, and workflow metadata. Parent of tbl_Receiving_Line.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_Transaction';
GO
