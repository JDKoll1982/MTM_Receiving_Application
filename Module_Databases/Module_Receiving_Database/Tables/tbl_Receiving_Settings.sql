-- ==============================================================================
-- Table: tbl_Receiving_Settings
-- Purpose: Application-wide and user-specific settings for receiving module
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE TABLE [dbo].[tbl_Receiving_Settings]
(
    -- Primary Key
    [SettingId] INT IDENTITY(1,1) NOT NULL,
    
    -- Setting Information
    [SettingKey] NVARCHAR(100) NOT NULL,             -- e.g., 'Receiving.Defaults.DefaultPackageType'
    [SettingValue] NVARCHAR(500) NULL,               -- Setting value (string representation)
    [SettingType] NVARCHAR(20) NOT NULL,             -- 'String', 'Integer', 'Boolean', 'Decimal'
    [Category] NVARCHAR(50) NOT NULL,                -- 'Default Values', 'Business Rules', 'Workflow Preferences', etc.
    [Description] NVARCHAR(500) NULL,
    
    -- Scope
    [Scope] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Receiving_Settings_Scope] DEFAULT 'System',
    [ScopeUserId] NVARCHAR(100) NULL,
    
    -- Validation
    [ValidValues] NVARCHAR(500) NULL,
    [MinValue] DECIMAL(18, 2) NULL,
    [MaxValue] DECIMAL(18, 2) NULL,
    
    -- Flags
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Receiving_Settings_IsActive] DEFAULT 1,
    [IsDeleted] BIT NOT NULL CONSTRAINT [DF_Receiving_Settings_IsDeleted] DEFAULT 0,
    [IsSystemDefault] BIT NOT NULL CONSTRAINT [DF_Receiving_Settings_IsSystemDefault] DEFAULT 0,
    [RequiresRestart] BIT NOT NULL CONSTRAINT [DF_Receiving_Settings_RequiresRestart] DEFAULT 0,
    
    -- Audit Fields
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL CONSTRAINT [DF_Receiving_Settings_CreatedDate] DEFAULT GETUTCDATE(),
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT [PK_Receiving_Settings] PRIMARY KEY CLUSTERED ([SettingId]),
    CONSTRAINT [UQ_Receiving_Settings_KeyScope] 
        UNIQUE ([SettingKey], [Scope], [ScopeUserId]),
    CONSTRAINT [CK_Receiving_Settings_ScopeValid] CHECK ([Scope] IN ('System', 'User')),
    CONSTRAINT [CK_Receiving_Settings_TypeValid] CHECK ([SettingType] IN ('String', 'Integer', 'Boolean', 'Decimal'))
);
GO

-- Indexes for lookups
CREATE NONCLUSTERED INDEX [IX_Receiving_Settings_Key]
    ON [dbo].[tbl_Receiving_Settings] ([SettingKey] ASC, [Scope] ASC)
    INCLUDE ([SettingValue], [SettingType])
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_Settings_Category]
    ON [dbo].[tbl_Receiving_Settings] ([Category] ASC, [SettingKey] ASC)
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

-- Extended Property
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Application settings for receiving module. Stores system-wide and user-specific configuration values.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_Settings';
GO
