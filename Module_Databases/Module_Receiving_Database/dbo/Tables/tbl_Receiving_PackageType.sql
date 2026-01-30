-- ==============================================================================
-- Table: tbl_Receiving_PackageType
-- Purpose: Reference table for package types (Skid, Pallet, Box, Bundle, etc.)
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE TABLE [dbo].[tbl_Receiving_PackageType]
(
    -- Primary Key
    [PackageTypeId] INT IDENTITY(1,1) NOT NULL,
    
    -- Package Type Information
    [PackageTypeName] NVARCHAR(50) NOT NULL,         -- 'Skid', 'Pallet', 'Box', 'Bundle', etc.
    [PackageTypeCode] NVARCHAR(20) NOT NULL,         -- 'SKID', 'PALLET', 'BOX', etc.
    [Description] NVARCHAR(500) NULL,
    
    -- Default Settings
    [DefaultPackagesPerLoad] INT NULL,               -- Typical number per load (e.g., 1 for skids, 4 for pallets)
    
    -- Display Order
    [SortOrder] INT NOT NULL CONSTRAINT [DF_Receiving_PackageType_SortOrder] DEFAULT 0,
    
    -- Flags
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Receiving_PackageType_IsActive] DEFAULT 1,
    [IsDeleted] BIT NOT NULL CONSTRAINT [DF_Receiving_PackageType_IsDeleted] DEFAULT 0,
    [IsSystemDefault] BIT NOT NULL CONSTRAINT [DF_Receiving_PackageType_IsSystemDefault] DEFAULT 0,
    
    -- Audit Fields
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL CONSTRAINT [DF_Receiving_PackageType_CreatedDate] DEFAULT GETUTCDATE(),
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT [PK_Receiving_PackageType] PRIMARY KEY CLUSTERED ([PackageTypeId]),
    CONSTRAINT [UQ_Receiving_PackageType_Name] UNIQUE ([PackageTypeName]),
    CONSTRAINT [UQ_Receiving_PackageType_Code] UNIQUE ([PackageTypeCode]),
    CONSTRAINT [CK_Receiving_PackageType_DefaultPackagesPositive] CHECK ([DefaultPackagesPerLoad] IS NULL OR [DefaultPackagesPerLoad] > 0)
);
GO

-- Index for lookups
CREATE NONCLUSTERED INDEX [IX_Receiving_PackageType_SortOrder]
    ON [dbo].[tbl_Receiving_PackageType] ([SortOrder] ASC, [PackageTypeName] ASC)
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

-- Extended Property
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Reference table for package types. Defines available package types and default packages per load.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_PackageType';
GO
