-- =============================================
-- Table: user_preferences
-- Description: User-specific workflow preferences
-- Used by: Module_Settings.Receiving and Module_Receiving
-- =============================================
CREATE TABLE [dbo].[user_preferences]
(
    [preference_id] INT IDENTITY(1,1) NOT NULL,
    [user_id] INT NOT NULL,                             -- User identifier
    [preference_key] NVARCHAR(100) NOT NULL,            -- Preference key
    [preference_value] NVARCHAR(MAX) NULL,              -- Preference value
    [preference_type] NVARCHAR(50) NOT NULL DEFAULT 'String',  -- String, Int, Bool, JSON
    [created_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [updated_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT [PK_user_preferences] PRIMARY KEY CLUSTERED ([preference_id] ASC),
    CONSTRAINT [UQ_user_preferences_user_key] UNIQUE NONCLUSTERED ([user_id] ASC, [preference_key] ASC)
);
GO

-- Index for fast user preference lookup
CREATE NONCLUSTERED INDEX [IX_user_preferences_user_id]
    ON [dbo].[user_preferences]([user_id] ASC);
GO

-- Index for key lookup
CREATE NONCLUSTERED INDEX [IX_user_preferences_key]
    ON [dbo].[user_preferences]([preference_key] ASC);
GO
