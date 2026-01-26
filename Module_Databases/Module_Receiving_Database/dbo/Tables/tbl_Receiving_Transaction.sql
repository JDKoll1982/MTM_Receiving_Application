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
    [WorkflowMode] NVARCHAR(20) NOT NULL,            -- 'Wizard', 'Manual', 'Edit'
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'Draft',  -- 'Draft', 'Completed', 'Cancelled'
    [CompletedDate] DATETIME2 NULL,                  -- When transaction was completed
    
    -- Non-PO Receiving
    [IsNonPO] BIT NOT NULL DEFAULT 0,                -- True if no PO number
    
    -- Quality Hold
    [RequiresQualityHold] BIT NOT NULL DEFAULT 0,    -- True if part requires QH
    [QualityHoldAcknowledged] BIT NOT NULL DEFAULT 0,-- User acknowledged QH procedures
    
    -- Save Status
    [SaveDataTransferObjectsCSV] BIT NOT NULL DEFAULT 0,             -- Successfully saved to network CSV
    [SaveDataTransferObjectsDatabase] BIT NOT NULL DEFAULT 1,        -- Successfully saved locally
    [CSVFilePath] NVARCHAR(500) NULL,                -- Path to generated CSV file
    [CSVExportDate] DATETIME2 NULL,                  -- When CSV was exported
    
    -- Session Information
    [SessionId] CHAR(36) NULL,                       -- Link to workflow session
    
    -- Flags
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    
    -- Audit Fields (Standard across all tables)
    [CreatedBy] NVARCHAR(100) NOT NULL,              -- Windows username
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
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
