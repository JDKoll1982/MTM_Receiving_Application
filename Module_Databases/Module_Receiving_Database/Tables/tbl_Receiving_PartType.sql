-- ==============================================================================
-- Table: tbl_Receiving_PartType
-- Purpose: Reference table for part type categories (Coil, Flat Stock, etc.)
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE TABLE [dbo].[tbl_Receiving_PartType]
(
    -- Primary Key
    [PartTypeId] INT IDENTITY(1,1) NOT NULL,
    
    -- Part Type Information
    [PartTypeName] NVARCHAR(50) NOT NULL,            -- 'Coil', 'Flat Stock', 'Tubing', etc.
    [PartTypeCode] NVARCHAR(20) NOT NULL,            -- 'COIL', 'FLAT', 'TUBE', etc.
    [Description] NVARCHAR(500) NULL,
    
    -- Prefix Associations (for auto-assignment)
    [PartPrefixes] NVARCHAR(200) NULL,               -- CSV: 'MMC,MMCCS,MMCSR' for Coils
    
    -- Expected Measurements
    [RequiresDiameter] BIT NOT NULL CONSTRAINT [DF_Receiving_PartType_RequiresDiameter] DEFAULT 0,
    [RequiresWidth] BIT NOT NULL CONSTRAINT [DF_Receiving_PartType_RequiresWidth] DEFAULT 0,
    [RequiresLength] BIT NOT NULL CONSTRAINT [DF_Receiving_PartType_RequiresLength] DEFAULT 0,
    [RequiresThickness] BIT NOT NULL CONSTRAINT [DF_Receiving_PartType_RequiresThickness] DEFAULT 0,
    [RequiresWeight] BIT NOT NULL CONSTRAINT [DF_Receiving_PartType_RequiresWeight] DEFAULT 1,
    
    -- Display Order
    [SortOrder] INT NOT NULL CONSTRAINT [DF_Receiving_PartType_SortOrder] DEFAULT 0,
    
    -- Flags
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Receiving_PartType_IsActive] DEFAULT 1,
    [IsDeleted] BIT NOT NULL CONSTRAINT [DF_Receiving_PartType_IsDeleted] DEFAULT 0,
    [IsSystemDefault] BIT NOT NULL CONSTRAINT [DF_Receiving_PartType_IsSystemDefault] DEFAULT 0,
    
    -- Audit Fields
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL CONSTRAINT [DF_Receiving_PartType_CreatedDate] DEFAULT GETUTCDATE(),
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT [PK_Receiving_PartType] PRIMARY KEY CLUSTERED ([PartTypeId]),
    CONSTRAINT [UQ_Receiving_PartType_Name] UNIQUE ([PartTypeName]),
    CONSTRAINT [UQ_Receiving_PartType_Code] UNIQUE ([PartTypeCode])
);
GO

-- Index for lookups
CREATE NONCLUSTERED INDEX [IX_Receiving_PartType_SortOrder]
    ON [dbo].[tbl_Receiving_PartType] ([SortOrder] ASC, [PartTypeName] ASC)
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;
GO

-- Extended Property
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Reference table for part type categories. Defines expected measurements and auto-assignment based on part number prefixes.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_PartType';
GO
