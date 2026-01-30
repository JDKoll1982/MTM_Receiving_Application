-- Post-Deployment: Seed Package Types

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PackageType] WHERE [PackageTypeCode] = 'SKID')
    INSERT INTO [dbo].[tbl_Receiving_PackageType] ([PackageTypeName], [PackageTypeCode], [Description], [DefaultPackagesPerLoad], [SortOrder], [IsSystemDefault], [CreatedBy])
    VALUES ('Skid', 'SKID', 'Wooden skid/pallet for heavy loads', 1, 1, 1, 'SYSTEM');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PackageType] WHERE [PackageTypeCode] = 'PALLET')
    INSERT INTO [dbo].[tbl_Receiving_PackageType] ([PackageTypeName], [PackageTypeCode], [Description], [DefaultPackagesPerLoad], [SortOrder], [IsSystemDefault], [CreatedBy])
    VALUES ('Pallet', 'PALLET', 'Standard pallet (40x48 or 48x48)', 4, 2, 1, 'SYSTEM');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PackageType] WHERE [PackageTypeCode] = 'BOX')
    INSERT INTO [dbo].[tbl_Receiving_PackageType] ([PackageTypeName], [PackageTypeCode], [Description], [DefaultPackagesPerLoad], [SortOrder], [IsSystemDefault], [CreatedBy])
    VALUES ('Box', 'BOX', 'Cardboard or wooden box', 6, 3, 1, 'SYSTEM');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PackageType] WHERE [PackageTypeCode] = 'BUNDLE')
    INSERT INTO [dbo].[tbl_Receiving_PackageType] ([PackageTypeName], [PackageTypeCode], [Description], [DefaultPackagesPerLoad], [SortOrder], [IsSystemDefault], [CreatedBy])
    VALUES ('Bundle', 'BUNDLE', 'Strapped bundle of material', 1, 4, 1, 'SYSTEM');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PackageType] WHERE [PackageTypeCode] = 'CRATE')
    INSERT INTO [dbo].[tbl_Receiving_PackageType] ([PackageTypeName], [PackageTypeCode], [Description], [DefaultPackagesPerLoad], [SortOrder], [IsSystemDefault], [CreatedBy])
    VALUES ('Crate', 'CRATE', 'Wooden crate for secure shipping', 2, 5, 1, 'SYSTEM');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PackageType] WHERE [PackageTypeCode] = 'LOOSE')
    INSERT INTO [dbo].[tbl_Receiving_PackageType] ([PackageTypeName], [PackageTypeCode], [Description], [DefaultPackagesPerLoad], [SortOrder], [IsSystemDefault], [CreatedBy])
    VALUES ('Loose', 'LOOSE', 'Loose/unpackaged material', 1, 6, 1, 'SYSTEM');
GO