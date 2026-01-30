-- ==============================================================================
-- Migration Script: Add Quality Hold Fields to tbl_Receiving_Line
-- Purpose: Add two-step acknowledgment tracking fields to existing table
-- Module: Module_Receiving
-- Created: 2026-01-30
-- NOTE: WeightPerPackage already exists, only adding new Quality Hold fields
-- ==============================================================================

-- Check if new columns exist before adding
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_Receiving_Line') AND name = 'IsQualityHoldRequired')
BEGIN
    ALTER TABLE [dbo].[tbl_Receiving_Line]
    ADD [IsQualityHoldRequired] BIT NOT NULL CONSTRAINT [DF_Receiving_Line_IsQualityHoldRequired] DEFAULT 0;
    PRINT 'Added column: IsQualityHoldRequired';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_Receiving_Line') AND name = 'QualityHoldRestrictionType')
BEGIN
    ALTER TABLE [dbo].[tbl_Receiving_Line]
    ADD [QualityHoldRestrictionType] NVARCHAR(50) NULL;
    PRINT 'Added column: QualityHoldRestrictionType';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_Receiving_Line') AND name = 'UserAcknowledgedQualityHold')
BEGIN
    ALTER TABLE [dbo].[tbl_Receiving_Line]
    ADD [UserAcknowledgedQualityHold] BIT NOT NULL CONSTRAINT [DF_Receiving_Line_UserAcknowledgedQualityHold] DEFAULT 0;
    PRINT 'Added column: UserAcknowledgedQualityHold';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_Receiving_Line') AND name = 'FinalAcknowledgedQualityHold')
BEGIN
    ALTER TABLE [dbo].[tbl_Receiving_Line]
    ADD [FinalAcknowledgedQualityHold] BIT NOT NULL CONSTRAINT [DF_Receiving_Line_FinalAcknowledgedQualityHold] DEFAULT 0;
    PRINT 'Added column: FinalAcknowledgedQualityHold';
END
GO

-- Add index for quality hold filtering
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Receiving_Line_QualityHoldRequired' AND object_id = OBJECT_ID('dbo.tbl_Receiving_Line'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Receiving_Line_QualityHoldRequired]
        ON [dbo].[tbl_Receiving_Line] ([IsQualityHoldRequired] ASC, [UserAcknowledgedQualityHold] ASC, [FinalAcknowledgedQualityHold] ASC)
        INCLUDE ([LineId], [PartNumber], [QualityHoldRestrictionType])
        WHERE [IsActive] = 1 AND [IsDeleted] = 0;
    PRINT 'Added index: IX_Receiving_Line_QualityHoldRequired';
END
GO

-- Add extended properties for new columns
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Whether this line requires quality hold based on part pattern match',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_Line',
    @level2type = N'COLUMN', @level2name = N'IsQualityHoldRequired';
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Type of quality hold restriction (e.g., Weight Sensitive, Quality Control)',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_Line',
    @level2type = N'COLUMN', @level2name = N'QualityHoldRestrictionType';
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Whether user acknowledged quality hold (step 1 of 2)',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_Line',
    @level2type = N'COLUMN', @level2name = N'UserAcknowledgedQualityHold';
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Whether final acknowledgment was completed (step 2 of 2). Hard block: Cannot save without this being true',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_Line',
    @level2type = N'COLUMN', @level2name = N'FinalAcknowledgedQualityHold';
GO

PRINT 'Quality Hold fields migration completed successfully';
GO