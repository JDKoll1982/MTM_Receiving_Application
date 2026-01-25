-- ==============================================================================
-- Table: tbl_Receiving_WorkflowSession
-- Purpose: Stores wizard workflow session state for resuming/tracking
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE TABLE [dbo].[tbl_Receiving_WorkflowSession]
(
    -- Primary Key
    [SessionId] CHAR(36) NOT NULL,
    
    -- Session Information
    [WorkflowMode] NVARCHAR(20) NOT NULL,            -- 'Wizard', 'Manual'
    [CurrentStep] INT NOT NULL DEFAULT 1,            -- Current step number (1, 2, 3)
    [SessionStatus] NVARCHAR(20) NOT NULL DEFAULT 'Active', -- 'Active', 'Completed', 'Abandoned'
    
    -- Business Data (Step 1)
    [PONumber] NVARCHAR(50) NULL,
    [PartNumber] NVARCHAR(50) NULL,
    [LoadCount] INT NULL,
    [IsNonPO] BIT NOT NULL DEFAULT 0,
    
    -- Session Preferences (persisted across steps)
    [DefaultReceivingLocation] NVARCHAR(100) NULL,   -- Session override
    [DefaultPackageType] NVARCHAR(50) NULL,          -- Session override
    [DefaultPackagesPerLoad] INT NULL,               -- Session override
    
    -- Load Details JSON (Step 2 - all load data serialized)
    [LoadDetailsJson] NVARCHAR(MAX) NULL,            -- JSON array of load details
    
    -- Validation State
    [Step1Valid] BIT NOT NULL DEFAULT 0,
    [Step2Valid] BIT NOT NULL DEFAULT 0,
    [AllStepsValid] BIT NOT NULL DEFAULT 0,
    
    -- Session Timing
    [SessionStartDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [SessionEndDate] DATETIME2 NULL,
    [LastActivityDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    -- Flags
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    
    -- Audit Fields
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT [PK_Receiving_WorkflowSession] PRIMARY KEY CLUSTERED ([SessionId]),
    CONSTRAINT [CK_Receiving_WorkflowSession_WorkflowModeValid] CHECK ([WorkflowMode] IN ('Wizard', 'Manual')),
    CONSTRAINT [CK_Receiving_WorkflowSession_SessionStatusValid] CHECK ([SessionStatus] IN ('Active', 'Completed', 'Abandoned')),
    CONSTRAINT [CK_Receiving_WorkflowSession_CurrentStepValid] CHECK ([CurrentStep] BETWEEN 1 AND 3),
    CONSTRAINT [CK_Receiving_WorkflowSession_LoadCountPositive] CHECK ([LoadCount] IS NULL OR [LoadCount] > 0)
);
GO

-- Indexes for performance
CREATE NONCLUSTERED INDEX [IX_Receiving_WorkflowSession_CreatedBy]
    ON [dbo].[tbl_Receiving_WorkflowSession] ([CreatedBy] ASC, [LastActivityDate] DESC)
    WHERE [IsActive] = 1 AND [IsDeleted] = 0 AND [SessionStatus] = 'Active';
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_WorkflowSession_LastActivity]
    ON [dbo].[tbl_Receiving_WorkflowSession] ([LastActivityDate] DESC)
    WHERE [IsActive] = 1 AND [IsDeleted] = 0 AND [SessionStatus] = 'Active';
GO

-- Extended Property for documentation
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Stores wizard workflow session state. Enables resuming interrupted workflows and tracking progress through steps.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_WorkflowSession';
GO
