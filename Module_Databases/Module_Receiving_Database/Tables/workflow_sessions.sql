-- =============================================
-- Table: workflow_sessions
-- Description: Stores workflow session state for multi-step wizards
-- Used by: Module_Receiving Guided Mode (Wizard)
-- =============================================
CREATE TABLE [dbo].[workflow_sessions]
(
    [session_id] UNIQUEIDENTIFIER NOT NULL,
    [user_id] INT NOT NULL,
    [current_step] INT NOT NULL,                    -- Current wizard step (1, 2, 3)
    [workflow_mode] NVARCHAR(20) NOT NULL,          -- 'Wizard', 'Manual', 'Edit'
    [is_non_po] BIT NOT NULL DEFAULT 0,             -- Whether this is non-PO receiving
    [po_number] NVARCHAR(50) NULL,                  -- PO number (NULL for non-PO)
    [part_id] NVARCHAR(50) NULL,                    -- Selected part ID
    [load_count] INT NOT NULL DEFAULT 0,            -- Number of loads
    [receiving_location_override] NVARCHAR(20) NULL, -- Session-level location override
    [load_details_json] NVARCHAR(MAX) NULL,         -- Serialized load grid state
    [created_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [updated_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [expires_at] DATETIME2 NOT NULL,                -- Session expiration (24 hours)
    
    CONSTRAINT [PK_workflow_sessions] PRIMARY KEY CLUSTERED ([session_id] ASC)
);
GO

-- Index for user lookup (find active session)
CREATE NONCLUSTERED INDEX [IX_workflow_sessions_user_id]
    ON [dbo].[workflow_sessions]([user_id] ASC, [expires_at] DESC);
GO

-- Index for cleanup of expired sessions
CREATE NONCLUSTERED INDEX [IX_workflow_sessions_expires_at]
    ON [dbo].[workflow_sessions]([expires_at] ASC)
    WHERE [expires_at] < GETDATE();
GO
