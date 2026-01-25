-- ==============================================================================
-- Seed Data Script: SeedPackageTypes.sql
-- Purpose: Populate default package types for Module_Receiving
-- Module: Module_Receiving
-- Created: 2026-01-25
-- Version: 1.0
-- ==============================================================================

SET NOCOUNT ON;
GO

PRINT '======================================================================';
PRINT 'Seeding Package Types for Module_Receiving';
PRINT 'Started: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '======================================================================';
GO

-- ==============================================================================
-- PACKAGE TYPE 1: Skid
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PackageType] WHERE [PackageTypeCode] = 'SKID')
BEGIN
    INSERT INTO [dbo].[tbl_Receiving_PackageType] (
        [PackageTypeName], 
        [PackageTypeCode], 
        [Description], 
        [DefaultPackagesPerLoad],
        [SortOrder], 
        [IsSystemDefault], 
        [CreatedBy]
    )
    VALUES (
        'Skid',                                          -- PackageTypeName
        'SKID',                                          -- PackageTypeCode
        'Wooden skid/pallet for heavy loads',            -- Description
        1,                                               -- DefaultPackagesPerLoad (typically 1 bundle per skid)
        1,                                               -- SortOrder
        1,                                               -- IsSystemDefault = TRUE
        'SYSTEM'                                         -- CreatedBy
    );
    PRINT '  ✓ Inserted Package Type: Skid (SKID)';
    PRINT '    - Default Packages Per Load: 1';
END
ELSE
    PRINT '  → Package Type SKID already exists, skipping...';
GO

-- ==============================================================================
-- PACKAGE TYPE 2: Pallet
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PackageType] WHERE [PackageTypeCode] = 'PALLET')
BEGIN
    INSERT INTO [dbo].[tbl_Receiving_PackageType] (
        [PackageTypeName], 
        [PackageTypeCode], 
        [Description], 
        [DefaultPackagesPerLoad],
        [SortOrder], 
        [IsSystemDefault], 
        [CreatedBy]
    )
    VALUES (
        'Pallet',                                        -- PackageTypeName
        'PALLET',                                        -- PackageTypeCode
        'Standard Coil Skid',                            -- Description
        4,                                               -- DefaultPackagesPerLoad (typically 4 boxes)
        2,                                               -- SortOrder
        1,                                               -- IsSystemDefault = TRUE
        'SYSTEM'                                         -- CreatedBy
    );
    PRINT '  ✓ Inserted Package Type: Pallet (PALLET)';
    PRINT '    - Default Packages Per Load: 4';
END
ELSE
    PRINT '  → Package Type PALLET already exists, skipping...';
GO

-- ==============================================================================
-- PACKAGE TYPE 3: Box
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PackageType] WHERE [PackageTypeCode] = 'BOX')
BEGIN
    INSERT INTO [dbo].[tbl_Receiving_PackageType] (
        [PackageTypeName], 
        [PackageTypeCode], 
        [Description], 
        [DefaultPackagesPerLoad],
        [SortOrder], 
        [IsSystemDefault], 
        [CreatedBy]
    )
    VALUES (
        'Box',                                           -- PackageTypeName
        'BOX',                                           -- PackageTypeCode
        'Cardboard or wooden box',                       -- Description
        6,                                               -- DefaultPackagesPerLoad (typically 6 boxes)
        3,                                               -- SortOrder
        1,                                               -- IsSystemDefault = TRUE
        'SYSTEM'                                         -- CreatedBy
    );
    PRINT '  ✓ Inserted Package Type: Box (BOX)';
    PRINT '    - Default Packages Per Load: 6';
END
ELSE
    PRINT '  → Package Type BOX already exists, skipping...';
GO

-- ==============================================================================
-- PACKAGE TYPE 4: Bundle
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PackageType] WHERE [PackageTypeCode] = 'BUNDLE')
BEGIN
    INSERT INTO [dbo].[tbl_Receiving_PackageType] (
        [PackageTypeName], 
        [PackageTypeCode], 
        [Description], 
        [DefaultPackagesPerLoad],
        [SortOrder], 
        [IsSystemDefault], 
        [CreatedBy]
    )
    VALUES (
        'Bundle',                                        -- PackageTypeName
        'BUNDLE',                                        -- PackageTypeCode
        'Strapped bundle of material',                   -- Description
        1,                                               -- DefaultPackagesPerLoad (typically 1 bundle)
        4,                                               -- SortOrder
        1,                                               -- IsSystemDefault = TRUE
        'SYSTEM'                                         -- CreatedBy
    );
    PRINT '  ✓ Inserted Package Type: Bundle (BUNDLE)';
    PRINT '    - Default Packages Per Load: 1';
END
ELSE
    PRINT '  → Package Type BUNDLE already exists, skipping...';
GO

-- ==============================================================================
-- PACKAGE TYPE 5: Crate
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PackageType] WHERE [PackageTypeCode] = 'CRATE')
BEGIN
    INSERT INTO [dbo].[tbl_Receiving_PackageType] (
        [PackageTypeName], 
        [PackageTypeCode], 
        [Description], 
        [DefaultPackagesPerLoad],
        [SortOrder], 
        [IsSystemDefault], 
        [CreatedBy]
    )
    VALUES (
        'Crate',                                         -- PackageTypeName
        'CRATE',                                         -- PackageTypeCode
        'Wooden crate for secure shipping',              -- Description
        2,                                               -- DefaultPackagesPerLoad (typically 2 crates)
        5,                                               -- SortOrder
        1,                                               -- IsSystemDefault = TRUE
        'SYSTEM'                                         -- CreatedBy
    );
    PRINT '  ✓ Inserted Package Type: Crate (CRATE)';
    PRINT '    - Default Packages Per Load: 2';
END
ELSE
    PRINT '  → Package Type CRATE already exists, skipping...';
GO

-- ==============================================================================
-- PACKAGE TYPE 6: Loose
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PackageType] WHERE [PackageTypeCode] = 'LOOSE')
BEGIN
    INSERT INTO [dbo].[tbl_Receiving_PackageType] (
        [PackageTypeName], 
        [PackageTypeCode], 
        [Description], 
        [DefaultPackagesPerLoad],
        [SortOrder], 
        [IsSystemDefault], 
        [CreatedBy]
    )
    VALUES (
        'Loose',                                         -- PackageTypeName
        'LOOSE',                                         -- PackageTypeCode
        'Loose/unpackaged material',                     -- Description
        1,                                               -- DefaultPackagesPerLoad
        6,                                               -- SortOrder
        1,                                               -- IsSystemDefault = TRUE
        'SYSTEM'                                         -- CreatedBy
    );
    PRINT '  ✓ Inserted Package Type: Loose (LOOSE)';
    PRINT '    - Default Packages Per Load: 1';
END
ELSE
    PRINT '  → Package Type LOOSE already exists, skipping...';
GO

-- ==============================================================================
-- VERIFICATION
-- ==============================================================================
PRINT '';
PRINT '----------------------------------------------------------------------';
PRINT 'Verification: Package Types Summary';
PRINT '----------------------------------------------------------------------';

SELECT 
    [PackageTypeId],
    [PackageTypeName],
    [PackageTypeCode],
    [DefaultPackagesPerLoad],
    [SortOrder]
FROM [dbo].[tbl_Receiving_PackageType]
WHERE [IsActive] = 1 AND [IsDeleted] = 0
ORDER BY [SortOrder];

DECLARE @PackageTypeCount INT = (SELECT COUNT(*) FROM [dbo].[tbl_Receiving_PackageType] WHERE [IsActive] = 1 AND [IsDeleted] = 0);
PRINT '';
PRINT 'Total Active Package Types: ' + CAST(@PackageTypeCount AS VARCHAR);

PRINT '';
PRINT '======================================================================';
PRINT 'Package Types Seeding Completed Successfully!';
PRINT 'Completed: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '======================================================================';
GO
