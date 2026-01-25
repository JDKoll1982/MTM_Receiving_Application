-- =============================================
-- Table: part_settings
-- Description: Part-specific configuration overrides
-- Used by: Module_Settings.Receiving (Part Number Management)
-- References: part_types table
-- =============================================
CREATE TABLE [dbo].[part_settings]
(
    [part_id] NVARCHAR(50) NOT NULL,            -- Part number (e.g., 'MMC0001000')
    [part_type_id] INT NULL,                    -- FK to part_types (override auto-detection)
    [default_location] NVARCHAR(20) NULL,       -- Override default receiving location
    [quality_hold_required] BIT NOT NULL DEFAULT 0,
    [created_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [created_by] NVARCHAR(100) NULL,
    [updated_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [updated_by] NVARCHAR(100) NULL,
    
    CONSTRAINT [PK_part_settings] PRIMARY KEY CLUSTERED ([part_id] ASC),
    CONSTRAINT [FK_part_settings_part_types] FOREIGN KEY ([part_type_id])
        REFERENCES [dbo].[part_types]([type_id])
);
GO

-- Index for part type lookups
CREATE NONCLUSTERED INDEX [IX_part_settings_type]
    ON [dbo].[part_settings]([part_type_id] ASC)
    INCLUDE ([part_id], [default_location]);
GO

-- Index for quality hold filtering
CREATE NONCLUSTERED INDEX [IX_part_settings_quality_hold]
    ON [dbo].[part_settings]([quality_hold_required] ASC)
    WHERE [quality_hold_required] = 1;
GO

-- Index for location lookups
CREATE NONCLUSTERED INDEX [IX_part_settings_location]
    ON [dbo].[part_settings]([default_location] ASC)
    WHERE [default_location] IS NOT NULL;
GO
