-- ==============================================================================
-- Seed Data Script: SeedDefaultSettings.sql
-- Purpose: Populate default application settings for Module_Receiving
-- Module: Module_Receiving
-- Created: 2026-01-25
-- Version: 1.0
-- ==============================================================================

SET NOCOUNT ON;
GO

PRINT '======================================================================';
PRINT 'Seeding Default Settings for Module_Receiving';
PRINT 'Started: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '======================================================================';
GO

-- ==============================================================================
-- SETTING 1: Default Package Type
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_Settings] WHERE [SettingKey] = 'Receiving.Defaults.DefaultPackageType')
BEGIN
    INSERT INTO [dbo].[tbl_Receiving_Settings] (
        [SettingKey], 
        [SettingValue], 
        [SettingType], 
        [Category], 
        [Description],
        [Scope], 
        [ValidValues],
        [IsSystemDefault], 
        [RequiresRestart],
        [CreatedBy]
    )
    VALUES (
        'Receiving.Defaults.DefaultPackageType',         -- SettingKey
        'Skid',                                          -- SettingValue
        'String',                                        -- SettingType
        'Default Values',                                -- Category
        'Default package type when no part-specific preference exists', -- Description
        'System',                                        -- Scope
        'Skid,Pallet,Box,Bundle,Crate,Loose',           -- ValidValues
        1,                                               -- IsSystemDefault = TRUE
        0,                                               -- RequiresRestart = FALSE
        'SYSTEM'                                         -- CreatedBy
    );
    PRINT '  ✓ Inserted Setting: Receiving.Defaults.DefaultPackageType = Skid';
END
ELSE
    PRINT '  → Setting Receiving.Defaults.DefaultPackageType already exists, skipping...';
GO

-- ==============================================================================
-- SETTING 2: Default Packages Per Load
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_Settings] WHERE [SettingKey] = 'Receiving.Defaults.DefaultPackagesPerLoad')
BEGIN
    INSERT INTO [dbo].[tbl_Receiving_Settings] (
        [SettingKey], 
        [SettingValue], 
        [SettingType], 
        [Category], 
        [Description],
        [Scope], 
        [MinValue],
        [MaxValue],
        [IsSystemDefault], 
        [RequiresRestart],
        [CreatedBy]
    )
    VALUES (
        'Receiving.Defaults.DefaultPackagesPerLoad',     -- SettingKey
        '1',                                             -- SettingValue
        'Integer',                                       -- SettingType
        'Default Values',                                -- Category
        'Default number of packages per load',           -- Description
        'System',                                        -- Scope
        1,                                               -- MinValue
        999,                                             -- MaxValue
        1,                                               -- IsSystemDefault = TRUE
        0,                                               -- RequiresRestart = FALSE
        'SYSTEM'                                         -- CreatedBy
    );
    PRINT '  ✓ Inserted Setting: Receiving.Defaults.DefaultPackagesPerLoad = 1';
END
ELSE
    PRINT '  → Setting Receiving.Defaults.DefaultPackagesPerLoad already exists, skipping...';
GO

-- ==============================================================================
-- SETTING 3: Default Receiving Location
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_Settings] WHERE [SettingKey] = 'Receiving.Defaults.DefaultLocation')
BEGIN
    INSERT INTO [dbo].[tbl_Receiving_Settings] (
        [SettingKey], 
        [SettingValue], 
        [SettingType], 
        [Category], 
        [Description],
        [Scope], 
        [IsSystemDefault], 
        [RequiresRestart],
        [CreatedBy]
    )
    VALUES (
        'Receiving.Defaults.DefaultLocation',            -- SettingKey
        'RECV',                                          -- SettingValue (generic receiving location)
        'String',                                        -- SettingType
        'Default Values',                                -- Category
        'Default receiving location when no part-specific default exists', -- Description
        'System',                                        -- Scope
        1,                                               -- IsSystemDefault = TRUE
        0,                                               -- RequiresRestart = FALSE
        'SYSTEM'                                         -- CreatedBy
    );
    PRINT '  ✓ Inserted Setting: Receiving.Defaults.DefaultLocation = RECV';
END
ELSE
    PRINT '  → Setting Receiving.Defaults.DefaultLocation already exists, skipping...';
GO

-- ==============================================================================
-- SETTING 4: Default Workflow Mode
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_Settings] WHERE [SettingKey] = 'Receiving.Defaults.DefaultReceivingMode')
BEGIN
    INSERT INTO [dbo].[tbl_Receiving_Settings] (
        [SettingKey], 
        [SettingValue], 
        [SettingType], 
        [Category], 
        [Description],
        [Scope], 
        [ValidValues],
        [IsSystemDefault], 
        [RequiresRestart],
        [CreatedBy]
    )
    VALUES (
        'Receiving.Defaults.DefaultReceivingMode',       -- SettingKey
        'Wizard',                                        -- SettingValue
        'String',                                        -- SettingType
        'Workflow Preferences',                          -- Category
        'Default workflow mode on startup',              -- Description
        'System',                                        -- Scope
        'Wizard,Manual,Ask',                             -- ValidValues
        1,                                               -- IsSystemDefault = TRUE
        0,                                               -- RequiresRestart = FALSE
        'SYSTEM'                                         -- CreatedBy
    );
    PRINT '  ✓ Inserted Setting: Receiving.Defaults.DefaultReceivingMode = Wizard';
END
ELSE
    PRINT '  → Setting Receiving.Defaults.DefaultReceivingMode already exists, skipping...';
GO

-- ==============================================================================
-- SETTING 5: CSV Export Path
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_Settings] WHERE [SettingKey] = 'Receiving.Export.CSVPath')
BEGIN
    INSERT INTO [dbo].[tbl_Receiving_Settings] (
        [SettingKey], 
        [SettingValue], 
        [SettingType], 
        [Category], 
        [Description],
        [Scope], 
        [IsSystemDefault], 
        [RequiresRestart],
        [CreatedBy]
    )
    VALUES (
        'Receiving.Export.CSVPath',                      -- SettingKey
        '',                                              -- SettingValue (empty - needs configuration)
        'String',                                        -- SettingType
        'Advanced Settings',                             -- Category
        'Network path for CSV export file (requires configuration)', -- Description
        'System',                                        -- Scope
        1,                                               -- IsSystemDefault = TRUE
        0,                                               -- RequiresRestart = FALSE
        'SYSTEM'                                         -- CreatedBy
    );
    PRINT '  ✓ Inserted Setting: Receiving.Export.CSVPath (empty - needs configuration)';
    PRINT '    NOTE: Configure this setting with network path after deployment';
END
ELSE
    PRINT '  → Setting Receiving.Export.CSVPath already exists, skipping...';
GO

-- ==============================================================================
-- SETTING 6: Enable Auto-Calculate for Load Numbers
-- ==============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_Settings] WHERE [SettingKey] = 'Receiving.BusinessRules.EnableAutoCalculate')
BEGIN
    INSERT INTO [dbo].[tbl_Receiving_Settings] (
        [SettingKey], 
        [SettingValue], 
        [SettingType], 
        [Category], 
        [Description],
        [Scope], 
        [ValidValues],
        [IsSystemDefault], 
        [RequiresRestart],
        [CreatedBy]
    )
    VALUES (
        'Receiving.BusinessRules.EnableAutoCalculate',   -- SettingKey
        'true',                                          -- SettingValue
        'Boolean',                                       -- SettingType
        'Business Rules',                                -- Category
        'Enable automatic load number calculation',      -- Description
        'System',                                        -- Scope
        'true,false',                                    -- ValidValues
        1,                                               -- IsSystemDefault = TRUE
        0,                                               -- RequiresRestart = FALSE
        'SYSTEM'                                         -- CreatedBy
    );
    PRINT '  ✓ Inserted Setting: Receiving.BusinessRules.EnableAutoCalculate = true';
END
ELSE
    PRINT '  → Setting Receiving.BusinessRules.EnableAutoCalculate already exists, skipping...';
GO

-- ==============================================================================
-- VERIFICATION
-- ==============================================================================
PRINT '';
PRINT '----------------------------------------------------------------------';
PRINT 'Verification: Settings Summary';
PRINT '----------------------------------------------------------------------';

SELECT 
    [SettingId],
    [SettingKey],
    [SettingValue],
    [SettingType],
    [Category],
    [Scope]
FROM [dbo].[tbl_Receiving_Settings]
WHERE [IsActive] = 1 AND [IsDeleted] = 0
ORDER BY [Category], [SettingKey];

DECLARE @SettingsCount INT = (SELECT COUNT(*) FROM [dbo].[tbl_Receiving_Settings] WHERE [IsActive] = 1 AND [IsDeleted] = 0);
PRINT '';
PRINT 'Total Active Settings: ' + CAST(@SettingsCount AS VARCHAR);

PRINT '';
PRINT '----------------------------------------------------------------------';
PRINT 'IMPORTANT: Post-Deployment Configuration Required';
PRINT '----------------------------------------------------------------------';
PRINT '  1. Update Receiving.Export.CSVPath with actual network path';
PRINT '  2. Create default location RECV in tbl_Receiving_Location if needed';
PRINT '  3. Configure additional user-specific settings as needed';
PRINT '';
PRINT '======================================================================';
PRINT 'Default Settings Seeding Completed Successfully!';
PRINT 'Completed: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '======================================================================';
GO
