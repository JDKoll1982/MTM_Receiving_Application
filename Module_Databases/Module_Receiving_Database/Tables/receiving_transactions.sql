-- =============================================
-- Table: receiving_transactions
-- Description: Master table for receiving transactions
-- Used by: Module_Receiving (all workflow modes)
-- =============================================
CREATE TABLE [dbo].[receiving_transactions]
(
    [transaction_id] INT IDENTITY(1,1) NOT NULL,
    [po_number] NVARCHAR(50) NULL,              -- NULL for non-PO receiving
    [user_id] INT NOT NULL,
    [user_name] NVARCHAR(100) NOT NULL,
    [workflow_mode] NVARCHAR(20) NOT NULL,      -- 'Wizard', 'Manual', 'Edit'
    [transaction_date] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [exported_to_csv] BIT NOT NULL DEFAULT 0,
    [csv_export_path_local] NVARCHAR(500) NULL,
    [csv_export_path_network] NVARCHAR(500) NULL,
    [csv_exported_at] DATETIME2 NULL,
    [created_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [updated_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [updated_by] NVARCHAR(100) NULL,
    
    CONSTRAINT [PK_receiving_transactions] PRIMARY KEY CLUSTERED ([transaction_id] ASC)
);
GO

-- Index for PO number lookups
CREATE NONCLUSTERED INDEX [IX_receiving_transactions_po_number]
    ON [dbo].[receiving_transactions]([po_number] ASC)
    WHERE [po_number] IS NOT NULL;
GO

-- Index for user + date filtering (Edit Mode)
CREATE NONCLUSTERED INDEX [IX_receiving_transactions_user_date]
    ON [dbo].[receiving_transactions]([user_id] ASC, [transaction_date] DESC)
    INCLUDE ([po_number], [workflow_mode]);
GO

-- Index for date range queries
CREATE NONCLUSTERED INDEX [IX_receiving_transactions_date]
    ON [dbo].[receiving_transactions]([transaction_date] DESC)
    INCLUDE ([transaction_id], [po_number], [user_name]);
GO
