-- ==============================================================================
-- Table: tbl_Receiving_PartPreference
-- Purpose: Part-specific preferences and defaults (location, package type, etc.)
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE TABLE [dbo].[tbl_Receiving_PartPreference]
(
    -- Primary Key
    [PartPreferenceId] INT IDENTITY(1,1) NOT NULL,
    
    -- Part Information
    [PartNumber] NVARCHAR(50) NOT NULL,              -- The part number these preferences apply to
    
    -- Part Type Assignment
    [PartTypeId] INT NULL,
    
    -- Default Values
    [DefaultReceivingLocation] NVARCHAR(100) NULL,
    [DefaultPackageType] NVARCHAR(50) NULL,
    [DefaultPackagesPerLoad] INT NULL,
    
    -- Quality Hold
    [RequiresQualityHold] BIT NOT NULL CONSTRAINT [DF_Receiving_PartPreference_RequiresQualityHold] DEFAULT 0,
    [QualityHoldProcedure] NVARCHAR(MAX) NULL,
    
    -- Integration
    [InforVisualPartNumber] NVARCHAR(50) NULL,
    [IsInforVisualSynced] BIT NOT NULL CONSTRAINT [DF_Receiving_PartPreference_IsInforVisualSynced] DEFAULT 0,
    [LastSyncDate] DATETIME2 NULL,
    
    -- Scope
    [Scope] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Receiving_PartPreference_Scope] DEFAULT 'System',
    [ScopeUserId] NVARCHAR(100) NULL,
    
    -- Flags
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Receiving_PartPreference_IsActive] DEFAULT 1,
    [IsDeleted] BIT NOT NULL CONSTRAINT [DF_Receiving_PartPreference_IsDeleted] DEFAULT 0,
    
    -- Audit Fields
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL CONSTRAINT [DF_Receiving_PartPreference_CreatedDate] DEFAULT GETUTCDATE(),
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT [PK_Receiving_PartPreference] PRIMARY KEY CLUSTERED ([PartPreferenceId]),
    CONSTRAINT [FK_Receiving_PartPreference_PartType] 
        FOREIGN KEY ([PartTypeId]) 
        REFERENCES [dbo].[tbl_Receiving_PartType]([PartTypeId])
        ON DELETE SET NULL,
    CONSTRAINT [UQ_Receiving_PartPreference_PartScope] 
        UNIQUE ([PartNumber], [Scope], [ScopeUserId]),
    CONSTRAINT [CK_Receiving_PartPreference_ScopeValid] CHECK ([Scope] IN ('System', 'User')),
    CONSTRAINT [CK_Receiving_PartPreference_PackagesPerLoadPositive] CHECK ([DefaultPackagesPerLoad] IS NULL OR [DefaultPackagesPerLoad] > 0)
);
GO

-- Indexes for lookups
CREATE NONCLUSTERED INDEX [IX_Receiving_PartPreference_PartNumber]
    ON [dbo].[tbl_Receiving_PartPreference] ([PartNumber] ASC, [Scope] ASC)
    INCLUDE ([DefaultReceivingLocation], [DefaultPackageType], [RequiresQualityHold])
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_PartPreference_QualityHold]
    ON [dbo].[tbl_Receiving_PartPreference] ([RequiresQualityHold] ASC, [PartNumber] ASC)
    WHERE [IsActive] = 1 AND [IsDeleted] = 0 AND [RequiresQualityHold] = 1;
GO

-- Extended Property
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Part-specific preferences and defaults. Stores default location, package type, quality hold settings per part number. Can be system-wide or user-specific.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_PartPreference';
GO
