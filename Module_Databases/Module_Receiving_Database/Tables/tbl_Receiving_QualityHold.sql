-- ==============================================================================
-- Table: tbl_Receiving_QualityHold
-- Purpose: Quality hold audit trail with two-step acknowledgment tracking
-- Module: Module_Receiving
-- Created: 2026-01-30
-- User Requirements:
--   - Configurable part patterns (not hardcoded)
--   - Two-step acknowledgment tracking
--   - Full audit trail
--   - Hard blocking on save
-- ==============================================================================

CREATE TABLE [dbo].[tbl_Receiving_QualityHold]
(
    -- Primary Key
    [QualityHoldId] CHAR(36) NOT NULL,
    
    -- Foreign Keys
    [LineId] CHAR(36) NOT NULL,                      -- References tbl_Receiving_Line
    [TransactionId] CHAR(36) NOT NULL,               -- References tbl_Receiving_Transaction
    
    -- Part Information
    [PartNumber] NVARCHAR(50) NOT NULL,
    [PartPattern] NVARCHAR(100) NOT NULL,            -- The pattern that triggered hold (e.g., "MMFSR*", "MMCSR*")
    [RestrictionType] NVARCHAR(50) NOT NULL,         -- Type of restriction (e.g., "Weight Sensitive", "Quality Control")
    
    -- First Acknowledgment (on part selection)
    [UserAcknowledgedDate] DATETIME2 NULL,           -- When user first acknowledged
    [UserAcknowledgedBy] NVARCHAR(100) NULL,         -- Username of first acknowledgment
    [UserAcknowledgmentMessage] NVARCHAR(500) NULL,  -- What message was shown
    
    -- Final Acknowledgment (before save)
    [FinalAcknowledgedDate] DATETIME2 NULL,          -- When final acknowledgment occurred
    [FinalAcknowledgedBy] NVARCHAR(100) NULL,        -- Username of final acknowledgment
    [FinalAcknowledgmentMessage] NVARCHAR(500) NULL, -- What message was shown
    [IsFullyAcknowledged] BIT NOT NULL CONSTRAINT [DF_Receiving_QualityHold_IsFullyAcknowledged] DEFAULT 0,
    
    -- Quality Inspector Sign-Off (future feature - not required for MVP)
    [QualityInspectorName] NVARCHAR(100) NULL,
    [QualityInspectorDate] DATETIME2 NULL,
    [QualityInspectorNotes] NVARCHAR(MAX) NULL,
    [IsReleased] BIT NOT NULL CONSTRAINT [DF_Receiving_QualityHold_IsReleased] DEFAULT 0,
    [ReleasedDate] DATETIME2 NULL,
    
    -- Additional Context
    [LoadNumber] INT NOT NULL,                       -- Which load this applies to
    [TotalWeight] DECIMAL(18, 2) NULL,               -- Weight at time of hold
    [PackageType] NVARCHAR(50) NULL,                 -- Package type at time of hold
    [Notes] NVARCHAR(MAX) NULL,                      -- General notes
    
    -- Flags
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Receiving_QualityHold_IsActive] DEFAULT 1,
    [IsDeleted] BIT NOT NULL CONSTRAINT [DF_Receiving_QualityHold_IsDeleted] DEFAULT 0,
    
    -- Audit Fields (Standard across all tables)
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL CONSTRAINT [DF_Receiving_QualityHold_CreatedDate] DEFAULT GETUTCDATE(),
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT [PK_Receiving_QualityHold] PRIMARY KEY CLUSTERED ([QualityHoldId]),
    CONSTRAINT [FK_Receiving_QualityHold_Line] 
        FOREIGN KEY ([LineId]) 
        REFERENCES [dbo].[tbl_Receiving_Line]([LineId])
        ON DELETE CASCADE,
    CONSTRAINT [FK_Receiving_QualityHold_Transaction] 
        FOREIGN KEY ([TransactionId]) 
        REFERENCES [dbo].[tbl_Receiving_Transaction]([TransactionId])
        ON DELETE NO ACTION, -- Transaction can remain even if lines are deleted
    CONSTRAINT [CK_Receiving_QualityHold_LoadNumberPositive] CHECK ([LoadNumber] > 0),
    CONSTRAINT [CK_Receiving_QualityHold_WeightPositive] CHECK ([TotalWeight] IS NULL OR [TotalWeight] > 0)
);
GO

-- Indexes for performance
CREATE NONCLUSTERED INDEX [IX_Receiving_QualityHold_LineId]
    ON [dbo].[tbl_Receiving_QualityHold] ([LineId] ASC, [CreatedDate] DESC)
    INCLUDE ([IsFullyAcknowledged], [PartNumber], [RestrictionType])
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_QualityHold_TransactionId]
    ON [dbo].[tbl_Receiving_QualityHold] ([TransactionId] ASC, [CreatedDate] DESC)
    INCLUDE ([IsFullyAcknowledged], [PartNumber])
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_QualityHold_PartNumber]
    ON [dbo].[tbl_Receiving_QualityHold] ([PartNumber] ASC, [CreatedDate] DESC)
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_QualityHold_NotFullyAcknowledged]
    ON [dbo].[tbl_Receiving_QualityHold] ([IsFullyAcknowledged] ASC, [CreatedDate] DESC)
    INCLUDE ([LineId], [PartNumber], [UserAcknowledgedDate], [FinalAcknowledgedDate])
    WHERE [IsActive] = 1 AND [IsDeleted] = 0 AND [IsFullyAcknowledged] = 0;
GO

-- Extended Property for documentation
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Quality hold audit trail with two-step acknowledgment tracking. Tracks user acknowledgment and final acknowledgment before save. Supports configurable part patterns.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_QualityHold';
GO
