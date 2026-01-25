-- =============================================
-- Table: receiving_audit_trail
-- Description: Audit log for all changes to receiving transactions
-- Used by: Module_Receiving Edit Mode
-- Purpose: Track all modifications to saved transactions (Edge Case 15)
-- =============================================
CREATE TABLE [dbo].[receiving_audit_trail]
(
    [audit_id] INT IDENTITY(1,1) NOT NULL,
    [transaction_id] INT NOT NULL,              -- FK to receiving_transactions
    [load_id] INT NULL,                         -- NULL for transaction-level changes
    [action_type] NVARCHAR(20) NOT NULL,        -- 'INSERT', 'UPDATE', 'DELETE', 'CSV_EXPORT'
    [field_name] NVARCHAR(50) NULL,             -- Field that changed (NULL for row-level actions)
    [old_value] NVARCHAR(MAX) NULL,
    [new_value] NVARCHAR(MAX) NULL,
    [changed_by] NVARCHAR(100) NOT NULL,
    [changed_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [change_reason] NVARCHAR(500) NULL,         -- Optional user-provided reason
    
    CONSTRAINT [PK_receiving_audit_trail] PRIMARY KEY CLUSTERED ([audit_id] ASC),
    CONSTRAINT [FK_receiving_audit_trail_transactions] FOREIGN KEY ([transaction_id])
        REFERENCES [dbo].[receiving_transactions]([transaction_id])
);
GO

-- Index for transaction history lookups
CREATE NONCLUSTERED INDEX [IX_receiving_audit_trail_transaction]
    ON [dbo].[receiving_audit_trail]([transaction_id] ASC, [changed_at] DESC)
    INCLUDE ([action_type], [field_name], [changed_by]);
GO

-- Index for load-specific history
CREATE NONCLUSTERED INDEX [IX_receiving_audit_trail_load]
    ON [dbo].[receiving_audit_trail]([load_id] ASC, [changed_at] DESC)
    WHERE [load_id] IS NOT NULL;
GO

-- Index for user activity tracking
CREATE NONCLUSTERED INDEX [IX_receiving_audit_trail_user]
    ON [dbo].[receiving_audit_trail]([changed_by] ASC, [changed_at] DESC)
    INCLUDE ([transaction_id], [action_type]);
GO
