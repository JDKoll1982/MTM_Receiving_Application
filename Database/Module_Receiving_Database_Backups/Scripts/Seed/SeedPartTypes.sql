-- ==============================================================================
-- Seed Data Script: SeedPartTypes.sql
-- Purpose: Populate default part types based on Module_Receiving specifications
-- Module: Module_Receiving
-- Created: 2026-01-25
-- Version: 1.0
-- ==============================================================================

SET NOCOUNT ON;
GO

PRINT '======================================================================';
PRINT 'Seeding Part Types for Module_Receiving';
PRINT 'Started: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '======================================================================';
GO

-- ==============================================================================
-- PART TYPE 1: Coil
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PartType] WHERE [PartTypeCode] = 'COIL')
BEGIN
    INSERT INTO [dbo].[tbl_Receiving_PartType] (
        [PartTypeName], 
        [PartTypeCode], 
        [Description], 
        [PartPrefixes],
        [RequiresDiameter], 
        [RequiresWidth], 
        [RequiresLength], 
        [RequiresThickness], 
        [RequiresWeight],
        [SortOrder], 
        [IsSystemDefault], 
        [CreatedBy]
    )
    VALUES (
        'Coil',                                          -- PartTypeName
        'COIL',                                          -- PartTypeCode
        'Rolled metal coils',                            -- Description
        'MMC,MMCCS,MMCSR',                              -- PartPrefixes (MMC*, MMCCS*, MMCSR*)
        1,                                               -- RequiresDiameter = TRUE
        1,                                               -- RequiresWidth = TRUE
        0,                                               -- RequiresLength = FALSE
        1,                                               -- RequiresThickness = TRUE
        1,                                               -- RequiresWeight = TRUE
        1,                                               -- SortOrder
        1,                                               -- IsSystemDefault = TRUE (cannot delete)
        'SYSTEM'                                         -- CreatedBy
    );
    PRINT '  ✓ Inserted Part Type: Coil (COIL)';
    PRINT '    - Prefixes: MMC, MMCCS, MMCSR';
    PRINT '    - Measurements: Diameter, Width, Thickness, Weight';
END
ELSE
    PRINT '  → Part Type COIL already exists, skipping...';
GO

-- ==============================================================================
-- PART TYPE 2: Flat Stock
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PartType] WHERE [PartTypeCode] = 'FLAT')
BEGIN
    INSERT INTO [dbo].[tbl_Receiving_PartType] (
        [PartTypeName], 
        [PartTypeCode], 
        [Description], 
        [PartPrefixes],
        [RequiresDiameter], 
        [RequiresWidth], 
        [RequiresLength], 
        [RequiresThickness], 
        [RequiresWeight],
        [SortOrder], 
        [IsSystemDefault], 
        [CreatedBy]
    )
    VALUES (
        'Flat Stock',                                    -- PartTypeName
        'FLAT',                                          -- PartTypeCode
        'Flat sheets and plates',                        -- Description
        'MMF,MMFCS,MMFSR',                              -- PartPrefixes (MMF*, MMFCS*, MMFSR*)
        0,                                               -- RequiresDiameter = FALSE
        1,                                               -- RequiresWidth = TRUE
        1,                                               -- RequiresLength = TRUE
        1,                                               -- RequiresThickness = TRUE
        1,                                               -- RequiresWeight = TRUE
        2,                                               -- SortOrder
        1,                                               -- IsSystemDefault = TRUE
        'SYSTEM'                                         -- CreatedBy
    );
    PRINT '  ✓ Inserted Part Type: Flat Stock (FLAT)';
    PRINT '    - Prefixes: MMF, MMFCS, MMFSR';
    PRINT '    - Measurements: Width, Length, Thickness, Weight';
END
ELSE
    PRINT '  → Part Type FLAT already exists, skipping...';
GO

-- ==============================================================================
-- PART TYPE 3: Tubing
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PartType] WHERE [PartTypeCode] = 'TUBE')
BEGIN
    INSERT INTO [dbo].[tbl_Receiving_PartType] (
        [PartTypeName], 
        [PartTypeCode], 
        [Description], 
        [PartPrefixes],
        [RequiresDiameter], 
        [RequiresWidth], 
        [RequiresLength], 
        [RequiresThickness], 
        [RequiresWeight],
        [SortOrder], 
        [IsSystemDefault], 
        [CreatedBy]
    )
    VALUES (
        'Tubing',                                        -- PartTypeName
        'TUBE',                                          -- PartTypeCode
        'Hollow tubes and pipes',                        -- Description
        'MMT',                                           -- PartPrefixes (MMT*)
        1,                                               -- RequiresDiameter = TRUE (Outside Diameter)
        0,                                               -- RequiresWidth = FALSE
        1,                                               -- RequiresLength = TRUE
        1,                                               -- RequiresThickness = TRUE (Wall Thickness)
        1,                                               -- RequiresWeight = TRUE
        3,                                               -- SortOrder
        1,                                               -- IsSystemDefault = TRUE
        'SYSTEM'                                         -- CreatedBy
    );
    PRINT '  ✓ Inserted Part Type: Tubing (TUBE)';
    PRINT '    - Prefixes: MMT';
    PRINT '    - Measurements: Outside Diameter, Length, Wall Thickness, Weight';
END
ELSE
    PRINT '  → Part Type TUBE already exists, skipping...';
GO

-- ==============================================================================
-- PART TYPE 4: Bar Stock
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PartType] WHERE [PartTypeCode] = 'BAR')
BEGIN
    INSERT INTO [dbo].[tbl_Receiving_PartType] (
        [PartTypeName], 
        [PartTypeCode], 
        [Description], 
        [PartPrefixes],
        [RequiresDiameter], 
        [RequiresWidth], 
        [RequiresLength], 
        [RequiresThickness], 
        [RequiresWeight],
        [SortOrder], 
        [IsSystemDefault], 
        [CreatedBy]
    )
    VALUES (
        'Bar Stock',                                     -- PartTypeName
        'BAR',                                           -- PartTypeCode
        'Solid bars and rods',                           -- Description
        'MMB',                                           -- PartPrefixes (MMB*)
        1,                                               -- RequiresDiameter = TRUE
        0,                                               -- RequiresWidth = FALSE
        1,                                               -- RequiresLength = TRUE
        0,                                               -- RequiresThickness = FALSE
        1,                                               -- RequiresWeight = TRUE
        4,                                               -- SortOrder
        1,                                               -- IsSystemDefault = TRUE
        'SYSTEM'                                         -- CreatedBy
    );
    PRINT '  ✓ Inserted Part Type: Bar Stock (BAR)';
    PRINT '    - Prefixes: MMB';
    PRINT '    - Measurements: Diameter, Length, Weight';
END
ELSE
    PRINT '  → Part Type BAR already exists, skipping...';
GO

-- ==============================================================================
-- VERIFICATION
-- ==============================================================================
PRINT '';
PRINT '----------------------------------------------------------------------';
PRINT 'Verification: Part Types Summary';
PRINT '----------------------------------------------------------------------';

SELECT 
    [PartTypeId],
    [PartTypeName],
    [PartTypeCode],
    [PartPrefixes],
    [SortOrder]
FROM [dbo].[tbl_Receiving_PartType]
WHERE [IsActive] = 1 AND [IsDeleted] = 0
ORDER BY [SortOrder];

DECLARE @PartTypeCount INT = (SELECT COUNT(*) FROM [dbo].[tbl_Receiving_PartType] WHERE [IsActive] = 1 AND [IsDeleted] = 0);
PRINT '';
PRINT 'Total Active Part Types: ' + CAST(@PartTypeCount AS VARCHAR);

PRINT '';
PRINT '======================================================================';
PRINT 'Part Types Seeding Completed Successfully!';
PRINT 'Completed: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '======================================================================';
GO
