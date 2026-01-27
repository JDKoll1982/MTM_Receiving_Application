-- ==============================================================================
-- Table: tbl_Receiving_Location
-- Purpose: Reference table for warehouse receiving locations
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE TABLE [dbo].[tbl_Receiving_Location]
(
    -- Primary Key
    [LocationId] INT IDENTITY(1,1) NOT NULL,
    
    -- Location Information
    [LocationCode] NVARCHAR(50) NOT NULL,            -- 'V-C0-01', 'RECV', etc.
    [LocationName] NVARCHAR(100) NULL,
    [Description] NVARCHAR(500) NULL,
    
    -- Location Details
    [Warehouse] NVARCHAR(50) NULL,                   -- Warehouse identifier
    [Aisle] NVARCHAR(20) NULL,
    [Bay] NVARCHAR(20) NULL,
    [Level] NVARCHAR(20) NULL,
    
    -- Integration
    [InforVisualLocation] NVARCHAR(50) NULL,         -- Corresponding Infor Visual location code
    [IsInforVisualSynced] BIT NOT NULL DEFAULT 0,    -- True if synced from Infor Visual
    [LastSyncDate] DATETIME2 NULL,
    
    -- Capacity (optional)
    [MaxCapacity] DECIMAL(18, 2) NULL,               -- Maximum weight capacity
    [CurrentLoad] DECIMAL(18, 2) NULL,               -- Current weight stored
    
    -- Flags
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [IsSystemDefault] BIT NOT NULL DEFAULT 0,        -- Cannot be deleted
    [AllowReceiving] BIT NOT NULL DEFAULT 1,         -- Can be used for receiving
    
    -- Audit Fields
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT [PK_Receiving_Location] PRIMARY KEY CLUSTERED ([LocationId]),
    CONSTRAINT [UQ_Receiving_Location_Code] UNIQUE ([LocationCode]),
    CONSTRAINT [CK_Receiving_Location_CapacityPositive] CHECK ([MaxCapacity] IS NULL OR [MaxCapacity] > 0),
    CONSTRAINT [CK_Receiving_Location_CurrentLoadPositive] CHECK ([CurrentLoad] IS NULL OR [CurrentLoad] >= 0)
);
GO

-- Indexes for lookups
CREATE NONCLUSTERED INDEX [IX_Receiving_Location_Warehouse]
    ON [dbo].[tbl_Receiving_Location] ([Warehouse] ASC, [LocationCode] ASC)
    WHERE [IsActive] = 1 AND [IsDeleted] = 0 AND [AllowReceiving] = 1;
GO

CREATE NONCLUSTERED INDEX [IX_Receiving_Location_InforVisual]
    ON [dbo].[tbl_Receiving_Location] ([InforVisualLocation] ASC)
    WHERE [IsActive] = 1 AND [IsDeleted] = 0 AND [IsInforVisualSynced] = 1;
GO

-- Extended Property
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Reference table for warehouse receiving locations. Can be synced from Infor Visual or manually managed.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_Location';
GO
