-- =============================================
-- Table: part_settings
-- Description: Part-specific configuration and preferences
-- Used by: Module_Settings.Receiving and Module_Receiving
-- =============================================
CREATE TABLE [dbo].[part_settings]
(
    [setting_id] INT IDENTITY(1,1) NOT NULL,
    [part_id] NVARCHAR(50) NOT NULL,                    -- Part number (unique)
    [part_type] NVARCHAR(50) NULL,                      -- Override auto-detected part type
    [default_package_type] NVARCHAR(50) NULL,           -- Default package type for this part
    [default_receiving_location] NVARCHAR(20) NULL,     -- Override ERP default location
    [quality_hold_required] BIT NOT NULL DEFAULT 0,     -- Whether quality hold applies
    [packages_per_load_default] INT NULL,               -- Default packages per load
    [weight_per_package_default] DECIMAL(18,3) NULL,    -- Default weight per package
    [notes] NVARCHAR(500) NULL,                         -- Admin notes
    [created_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [updated_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [updated_by] NVARCHAR(100) NULL,
    
    CONSTRAINT [PK_part_settings] PRIMARY KEY CLUSTERED ([setting_id] ASC),
    CONSTRAINT [UQ_part_settings_part_id] UNIQUE NONCLUSTERED ([part_id] ASC)
);
GO

-- Index for fast part lookup
CREATE NONCLUSTERED INDEX [IX_part_settings_part_id]
    ON [dbo].[part_settings]([part_id] ASC);
GO

-- Index for quality hold filtering
CREATE NONCLUSTERED INDEX [IX_part_settings_quality_hold]
    ON [dbo].[part_settings]([quality_hold_required] ASC)
    WHERE [quality_hold_required] = 1;
GO
