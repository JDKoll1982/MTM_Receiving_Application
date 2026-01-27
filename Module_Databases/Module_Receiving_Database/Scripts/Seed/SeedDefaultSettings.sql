-- Post-Deployment: Seed Default Settings

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_Settings] WHERE [SettingKey] = 'Receiving.Defaults.DefaultPackageType')
    INSERT INTO [dbo].[tbl_Receiving_Settings] ([SettingKey], [SettingValue], [SettingType], [Category], [Description], [Scope], [ValidValues], [IsSystemDefault], [RequiresRestart], [CreatedBy])
    VALUES ('Receiving.Defaults.DefaultPackageType', 'Skid', 'String', 'Default Values', 'Default package type when no part-specific preference exists', 'System', 'Skid,Pallet,Box,Bundle,Crate,Loose', 1, 0, 'SYSTEM');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_Settings] WHERE [SettingKey] = 'Receiving.Defaults.DefaultPackagesPerLoad')
    INSERT INTO [dbo].[tbl_Receiving_Settings] ([SettingKey], [SettingValue], [SettingType], [Category], [Description], [Scope], [MinValue], [MaxValue], [IsSystemDefault], [RequiresRestart], [CreatedBy])
    VALUES ('Receiving.Defaults.DefaultPackagesPerLoad', '1', 'Integer', 'Default Values', 'Default number of packages per load', 'System', 1, 999, 1, 0, 'SYSTEM');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_Settings] WHERE [SettingKey] = 'Receiving.Defaults.DefaultLocation')
    INSERT INTO [dbo].[tbl_Receiving_Settings] ([SettingKey], [SettingValue], [SettingType], [Category], [Description], [Scope], [IsSystemDefault], [RequiresRestart], [CreatedBy])
    VALUES ('Receiving.Defaults.DefaultLocation', 'RECV', 'String', 'Default Values', 'Default receiving location when no part-specific default exists', 'System', 1, 0, 'SYSTEM');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_Settings] WHERE [SettingKey] = 'Receiving.Defaults.DefaultReceivingMode')
    INSERT INTO [dbo].[tbl_Receiving_Settings] ([SettingKey], [SettingValue], [SettingType], [Category], [Description], [Scope], [ValidValues], [IsSystemDefault], [RequiresRestart], [CreatedBy])
    VALUES ('Receiving.Defaults.DefaultReceivingMode', 'Wizard', 'String', 'Workflow Preferences', 'Default workflow mode on startup', 'System', 'Wizard,Manual,Ask', 1, 0, 'SYSTEM');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_Settings] WHERE [SettingKey] = 'Receiving.Export.CSVPath')
    INSERT INTO [dbo].[tbl_Receiving_Settings] ([SettingKey], [SettingValue], [SettingType], [Category], [Description], [Scope], [IsSystemDefault], [RequiresRestart], [CreatedBy])
    VALUES ('Receiving.Export.CSVPath', '', 'String', 'Advanced Settings', 'Network path for CSV export file (requires configuration)', 'System', 1, 0, 'SYSTEM');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_Settings] WHERE [SettingKey] = 'Receiving.BusinessRules.EnableAutoCalculate')
    INSERT INTO [dbo].[tbl_Receiving_Settings] ([SettingKey], [SettingValue], [SettingType], [Category], [Description], [Scope], [ValidValues], [IsSystemDefault], [RequiresRestart], [CreatedBy])
    VALUES ('Receiving.BusinessRules.EnableAutoCalculate', 'true', 'Boolean', 'Business Rules', 'Enable automatic load number calculation', 'System', 'true,false', 1, 0, 'SYSTEM');
GO