-- ==============================================================================
-- Table: tbl_Receiving_AuditLog
-- Purpose: Comprehensive audit trail for all receiving transactions and edits
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE TABLE [dbo].[tbl_Receiving_AuditLog]
(
    -- Primary Key
    [AuditId] CHAR(36) NOT NULL,
    
    -- Related Entity
    [TableName] NVARCHAR(100) NOT NULL,              -- Which table was modified
    [RecordId] CHAR(36) NOT NULL,                    -- ID of the modified record
    [Action] NVARCHAR(20) NOT NULL,                  -- 'INSERT', 'UPDATE', 'DELETE'
    
    -- Field-Level Changes (for UPDATE operations)
    [FieldName] NVARCHAR(100) NULL,                  -- Name of field that changed
    [OldValue] NVARCHAR(MAX) NULL,                   -- Previous value
    [NewValue] NVARCHAR(MAX) NULL,                   -- New value
    
    -- Context Information
    [TransactionId] CHAR(36) NULL,                   -- Related transaction (if applicable)
    [ChangeReason] NVARCHAR(500) NULL,               -- Why the change was made
    [ChangeContext] NVARCHAR(50) NULL,               -- 'Wizard', 'Manual', 'EditMode', 'Correction'
    
    -- User Information
    [PerformedBy] NVARCHAR(100) NOT NULL,            -- Windows username
    [PerformedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IPAddress] NVARCHAR(50) NULL,
    [MachineName] NVARCHAR(100) NULL,
    
    -- Additional Details
    [Details] NVARCHAR(MAX) NULL,                    -- JSON or free text for complex changes
    
    -- Constraints
    CONSTRAINT [PK_Receiving_AuditLog] PRIMARY KEY CLUSTERED ([AuditId]),
    CONSTRAINT [CK_Receiving_AuditLog_ActionValid] CHECK ([Action] IN ('INSERT', 'UPDATE', 'DELETE', 'SELECT'))
);
GO

-- Indexes for audit queries
CREATE NONCLUSTERED INDEX [IX_Receiving_AuditLog_RecordId]
    ON [dbo].[tbl_Receiving_AuditLog] ([RecordId] ASC, [PerformedDate] DESC)
    INCLUDE ([TableName], [Action], [FieldName], [PerformedBy]);
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_AuditLog_TransactionId]
    ON [dbo].[tbl_Receiving_AuditLog] ([TransactionId] ASC, [PerformedDate] DESC)
    INCLUDE ([TableName], [Action], [PerformedBy]);
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_AuditLog_PerformedDate]
    ON [dbo].[tbl_Receiving_AuditLog] ([PerformedDate] DESC)
    INCLUDE ([TableName], [Action], [PerformedBy], [RecordId]);
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_AuditLog_PerformedBy]
    ON [dbo].[tbl_Receiving_AuditLog] ([PerformedBy] ASC, [PerformedDate] DESC)
    INCLUDE ([TableName], [Action], [RecordId]);
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_AuditLog_TableAction]
    ON [dbo].[tbl_Receiving_AuditLog] ([TableName] ASC, [Action] ASC, [PerformedDate] DESC);
GO

-- Extended Property
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Comprehensive audit trail for all receiving transactions. Tracks INSERT, UPDATE, DELETE operations with field-level change tracking.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_AuditLog';
GO
