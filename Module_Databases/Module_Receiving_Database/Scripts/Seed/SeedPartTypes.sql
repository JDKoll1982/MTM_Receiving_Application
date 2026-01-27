-- Post-Deployment: Seed Part Types

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PartType] WHERE [PartTypeCode] = 'COIL')
    INSERT INTO [dbo].[tbl_Receiving_PartType] ([PartTypeName], [PartTypeCode], [Description], [PartPrefixes], [RequiresDiameter], [RequiresWidth], [RequiresLength], [RequiresThickness], [RequiresWeight], [SortOrder], [IsSystemDefault], [CreatedBy])
    VALUES ('Coil', 'COIL', 'Rolled metal coils', 'MMC,MMCCS,MMCSR', 1, 1, 0, 1, 1, 1, 1, 'SYSTEM');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PartType] WHERE [PartTypeCode] = 'FLAT')
    INSERT INTO [dbo].[tbl_Receiving_PartType] ([PartTypeName], [PartTypeCode], [Description], [PartPrefixes], [RequiresDiameter], [RequiresWidth], [RequiresLength], [RequiresThickness], [RequiresWeight], [SortOrder], [IsSystemDefault], [CreatedBy])
    VALUES ('Flat Stock', 'FLAT', 'Flat sheets and plates', 'MMF,MMFCS,MMFSR', 0, 1, 1, 1, 1, 2, 1, 'SYSTEM');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PartType] WHERE [PartTypeCode] = 'TUBE')
    INSERT INTO [dbo].[tbl_Receiving_PartType] ([PartTypeName], [PartTypeCode], [Description], [PartPrefixes], [RequiresDiameter], [RequiresWidth], [RequiresLength], [RequiresThickness], [RequiresWeight], [SortOrder], [IsSystemDefault], [CreatedBy])
    VALUES ('Tubing', 'TUBE', 'Hollow tubes and pipes', 'MMT', 1, 0, 1, 1, 1, 3, 1, 'SYSTEM');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_PartType] WHERE [PartTypeCode] = 'BAR')
    INSERT INTO [dbo].[tbl_Receiving_PartType] ([PartTypeName], [PartTypeCode], [Description], [PartPrefixes], [RequiresDiameter], [RequiresWidth], [RequiresLength], [RequiresThickness], [RequiresWeight], [SortOrder], [IsSystemDefault], [CreatedBy])
    VALUES ('Bar Stock', 'BAR', 'Solid bars and rods', 'MMB', 1, 0, 1, 0, 1, 4, 1, 'SYSTEM');
GO
