-- =============================================
-- Table: system_settings
-- Description: System-wide configuration settings (admin-level)
-- Scope: Application-wide settings that affect all users
-- Used by: Module_Settings.Core, Module_Settings.Receiving
-- =============================================
CREATE TABLE [dbo].[system_settings]
(
    [setting_id] INT IDENTITY(1,1) NOT NULL,
    [category] NVARCHAR(50) NOT NULL,           -- e.g., 'Receiving', 'Core', 'Dunnage'
    [key_name] NVARCHAR(100) NOT NULL,          -- e.g., 'Validation.RequirePoNumber'
    [value] NVARCHAR(MAX) NOT NULL,             -- Setting value (string, JSON, etc.)
    [data_type] NVARCHAR(20) NOT NULL,          -- 'Boolean', 'Integer', 'String', 'Decimal'
    [description] NVARCHAR(500) NULL,           -- Human-readable description
    [created_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [created_by] NVARCHAR(100) NULL,
    [updated_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [updated_by] NVARCHAR(100) NULL,
    
    CONSTRAINT [PK_system_settings] PRIMARY KEY CLUSTERED ([setting_id] ASC),
    CONSTRAINT [UQ_system_settings_category_key] UNIQUE NONCLUSTERED ([category] ASC, [key_name] ASC)
);
GO

-- Index for fast category-based lookups
CREATE NONCLUSTERED INDEX [IX_system_settings_category]
    ON [dbo].[system_settings]([category] ASC)
    INCLUDE ([key_name], [value], [data_type]);
GO

-- Index for fast key lookups
CREATE NONCLUSTERED INDEX [IX_system_settings_key_name]
    ON [dbo].[system_settings]([key_name] ASC)
    INCLUDE ([category], [value]);
GO
