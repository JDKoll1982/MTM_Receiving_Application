-- =============================================
-- Table: system_settings
-- Description: System-wide configuration key-value pairs
-- Used by: Module_Settings.Receiving
-- =============================================
CREATE TABLE [dbo].[system_settings]
(
    [setting_id] INT IDENTITY(1,1) NOT NULL,
    [setting_key] NVARCHAR(100) NOT NULL,               -- Unique setting key
    [setting_value] NVARCHAR(MAX) NULL,                 -- Setting value (JSON for complex objects)
    [setting_type] NVARCHAR(50) NOT NULL,               -- Data type: String, Int, Bool, JSON
    [category] NVARCHAR(50) NULL,                       -- Category: Workflow, ERP, Export, etc.
    [description] NVARCHAR(500) NULL,                   -- Human-readable description
    [is_user_editable] BIT NOT NULL DEFAULT 1,          -- Can users edit this setting?
    [created_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [updated_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [updated_by] NVARCHAR(100) NULL,
    
    CONSTRAINT [PK_system_settings] PRIMARY KEY CLUSTERED ([setting_id] ASC),
    CONSTRAINT [UQ_system_settings_key] UNIQUE NONCLUSTERED ([setting_key] ASC)
);
GO

-- Index for fast key lookup
CREATE NONCLUSTERED INDEX [IX_system_settings_key]
    ON [dbo].[system_settings]([setting_key] ASC);
GO

-- Index for category filtering
CREATE NONCLUSTERED INDEX [IX_system_settings_category]
    ON [dbo].[system_settings]([category] ASC);
GO

-- Insert default system settings
INSERT INTO [dbo].[system_settings] 
    ([setting_key], [setting_value], [setting_type], [category], [description], [is_user_editable])
VALUES
    ('DefaultReceivingLocation', 'RECV', 'String', 'Workflow', 'Fallback receiving location when no default is configured', 1),
    ('DefaultPackageType', 'Skid', 'String', 'Workflow', 'Default package type for new loads', 1),
    ('CSVExportPathLocal', 'C:\ReceivingData\CSV', 'String', 'Export', 'Local CSV export directory path', 1),
    ('CSVExportPathNetwork', '\\NetworkShare\ReceivingData', 'String', 'Export', 'Network CSV export directory path', 1),
    ('AutoSaveEnabled', 'true', 'Bool', 'Workflow', 'Enable automatic session saving', 1),
    ('AutoSaveIntervalSeconds', '300', 'Int', 'Workflow', 'Auto-save interval in seconds (default: 5 minutes)', 1),
    ('ValidationStrictness', 'Medium', 'String', 'Workflow', 'Validation level: Strict, Medium, Permissive', 1),
    ('ERPSyncEnabled', 'true', 'Bool', 'ERP', 'Enable ERP data synchronization', 0),
    ('ERPConnectionTimeout', '30', 'Int', 'ERP', 'ERP connection timeout in seconds', 0);
GO
