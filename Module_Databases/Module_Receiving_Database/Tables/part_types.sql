-- =============================================
-- Table: part_types
-- Description: Master table for all part type classifications
-- Used by: Module_Receiving, Module_Settings.Receiving
-- =============================================
CREATE TABLE [dbo].[part_types]
(
    [type_id] INT IDENTITY(1,1) NOT NULL,
    [type_name] NVARCHAR(50) NOT NULL,
    [description] NVARCHAR(200) NULL,
    [active] BIT NOT NULL DEFAULT 1,
    [display_order] INT NOT NULL DEFAULT 999,
    [created_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [created_by] NVARCHAR(100) NULL,
    [updated_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [updated_by] NVARCHAR(100) NULL,
    
    CONSTRAINT [PK_part_types] PRIMARY KEY CLUSTERED ([type_id] ASC),
    CONSTRAINT [UQ_part_types_type_name] UNIQUE NONCLUSTERED ([type_name] ASC)
);
GO

-- Index for active type lookups
CREATE NONCLUSTERED INDEX [IX_part_types_active_display_order]
    ON [dbo].[part_types]([active] ASC, [display_order] ASC)
    INCLUDE ([type_name]);
GO

-- Seed data with common part types
INSERT INTO [dbo].[part_types] ([type_name], [description], [display_order])
VALUES
    ('Coils', 'Coiled metal stock', 1),
    ('Flat Stock', 'Flat metal sheets/plates', 2),
    ('Tubing', 'Metal tubing/pipes', 3),
    ('Barstock', 'Bar stock material', 4),
    ('Nuts', 'Fastener nuts', 5),
    ('Bolts', 'Fastener bolts', 6),
    ('Washers', 'Fastener washers', 7),
    ('Bushings', 'Bushings and bearings', 8),
    ('Screws', 'Fastener screws', 9),
    ('Misc Hardware', 'Miscellaneous hardware', 10);
GO
