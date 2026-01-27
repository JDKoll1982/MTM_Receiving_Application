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
    [PartTypeId] INT NULL,                           -- Override part type (instead of prefix-based)
    
    -- Default Values
    [DefaultReceivingLocation] NVARCHAR(100) NULL,   -- Default location for this part
    [DefaultPackageType] NVARCHAR(50) NULL,          -- Default package type for this part
    [DefaultPackagesPerLoad] INT NULL,               -- Default packages per load for this part
    
    -- Quality Hold
    [RequiresQualityHold] BIT NOT NULL DEFAULT 0,    -- True if this part requires QH
    [QualityHoldProcedure] NVARCHAR(MAX) NULL,       -- QH procedure text
    
    -- Integration
    [InforVisualPartNumber] NVARCHAR(50) NULL,       -- Corresponding Infor Visual part
    [IsInforVisualSynced] BIT NOT NULL DEFAULT 0,    -- True if synced from Infor Visual
    [LastSyncDate] DATETIME2 NULL,
    
    -- Scope
    [Scope] NVARCHAR(20) NOT NULL DEFAULT 'System',  -- 'System' or 'User'
    [ScopeUserId] NVARCHAR(100) NULL,                -- If user-specific
    
    -- Flags
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    
    -- Audit Fields
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
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
