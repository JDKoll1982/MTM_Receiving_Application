-- =============================================
-- Table: user_preferences
-- Description: User-specific preference settings
-- Scope: Per-user settings that override system defaults
-- Used by: Module_Settings.Core, all modules with user preferences
-- =============================================
CREATE TABLE [dbo].[user_preferences]
(
    [preference_id] INT IDENTITY(1,1) NOT NULL,
    [user_id] INT NOT NULL,                     -- Windows username or ID from user table
    [user_name] NVARCHAR(100) NOT NULL,         -- Stored for audit/display purposes
    [category] NVARCHAR(50) NOT NULL,           -- e.g., 'Receiving', 'Core', 'Dunnage'
    [key_name] NVARCHAR(100) NOT NULL,          -- e.g., 'BusinessRules.DefaultModeOnStartup'
    [value] NVARCHAR(MAX) NOT NULL,             -- Preference value
    [data_type] NVARCHAR(20) NOT NULL,          -- 'Boolean', 'Integer', 'String', 'Decimal'
    [created_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [updated_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT [PK_user_preferences] PRIMARY KEY CLUSTERED ([preference_id] ASC),
    CONSTRAINT [UQ_user_preferences_user_category_key] UNIQUE NONCLUSTERED ([user_id] ASC, [category] ASC, [key_name] ASC)
);
GO

-- Index for user-specific lookups
CREATE NONCLUSTERED INDEX [IX_user_preferences_user_id]
    ON [dbo].[user_preferences]([user_id] ASC)
    INCLUDE ([category], [key_name], [value]);
GO

-- Index for user + category lookups
CREATE NONCLUSTERED INDEX [IX_user_preferences_user_category]
    ON [dbo].[user_preferences]([user_id] ASC, [category] ASC)
    INCLUDE ([key_name], [value], [data_type]);
GO
