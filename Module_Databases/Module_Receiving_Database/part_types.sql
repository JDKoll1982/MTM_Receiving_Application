-- =============================================
-- Table: part_types
-- Description: Lookup table for part type definitions
-- Used by: Module_Receiving for part type assignment
-- =============================================
CREATE TABLE [dbo].[part_types]
(
    [part_type_id] INT IDENTITY(1,1) NOT NULL,
    [part_type_code] NVARCHAR(50) NOT NULL,             -- Unique code (Casting, Forging, etc.)
    [part_type_name] NVARCHAR(100) NOT NULL,            -- Display name
    [prefix_pattern] NVARCHAR(50) NULL,                 -- Part number prefix pattern (MMC, MMF, etc.)
    [description] NVARCHAR(500) NULL,                   -- Description
    [is_active] BIT NOT NULL DEFAULT 1,                 -- Whether this type is active
    [created_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [updated_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT [PK_part_types] PRIMARY KEY CLUSTERED ([part_type_id] ASC),
    CONSTRAINT [UQ_part_types_code] UNIQUE NONCLUSTERED ([part_type_code] ASC)
);
GO

-- Insert default part types
INSERT INTO [dbo].[part_types] 
    ([part_type_code], [part_type_name], [prefix_pattern], [description])
VALUES
    ('Casting', 'Castings', 'MMC', 'Cast metal parts'),
    ('Forging', 'Forgings', 'MMF', 'Forged metal parts'),
    ('CastingWithCoreSand', 'Castings with Core Sand', 'MMCCS', 'Castings that include core sand'),
    ('MachinedPart', 'Machined Parts', 'MMP', 'Precision machined components'),
    ('RawMaterial', 'Raw Materials', 'MMR', 'Unprocessed materials'),
    ('Assembly', 'Assemblies', 'MMA', 'Multi-component assemblies'),
    ('Component', 'Components', 'MMCO', 'Individual components'),
    ('Tooling', 'Tooling', 'MMT', 'Manufacturing tooling'),
    ('Consumable', 'Consumables', 'MMCON', 'Consumable supplies'),
    ('Other', 'Other/Unknown', NULL, 'Parts that do not match standard prefixes');
GO
