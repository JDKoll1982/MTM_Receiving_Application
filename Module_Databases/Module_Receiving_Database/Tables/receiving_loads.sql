-- =============================================
-- Table: receiving_loads
-- Description: Individual loads within a receiving transaction
-- Used by: Module_Receiving (all workflow modes)
-- =============================================
CREATE TABLE [dbo].[receiving_loads]
(
    [load_id] INT IDENTITY(1,1) NOT NULL,
    [transaction_id] INT NOT NULL,              -- FK to receiving_transactions
    [load_number] INT NOT NULL,                 -- Sequential within transaction (1, 2, 3, ...)
    [part_id] NVARCHAR(50) NOT NULL,            -- Part number
    [part_type] NVARCHAR(50) NULL,              -- Part type (from part_types or auto-detected)
    [quantity] DECIMAL(18,3) NOT NULL,          -- Total weight/count for this load
    [unit_of_measure] NVARCHAR(10) NOT NULL DEFAULT 'LBS',
    [heat_lot_number] NVARCHAR(50) NULL,
    [package_type] NVARCHAR(50) NULL,           -- Skid, Pallet, Box, etc.
    [packages_per_load] INT NOT NULL DEFAULT 1,
    [weight_per_package] DECIMAL(18,3) NULL,    -- Calculated or entered
    [receiving_location] NVARCHAR(20) NULL,
    [quality_hold_acknowledged] BIT NOT NULL DEFAULT 0,
    [quality_hold_acknowledged_at] DATETIME2 NULL,
    [created_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [updated_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT [PK_receiving_loads] PRIMARY KEY CLUSTERED ([load_id] ASC),
    CONSTRAINT [FK_receiving_loads_transactions] FOREIGN KEY ([transaction_id])
        REFERENCES [dbo].[receiving_transactions]([transaction_id])
        ON DELETE CASCADE,
    CONSTRAINT [UQ_receiving_loads_transaction_load] UNIQUE NONCLUSTERED ([transaction_id] ASC, [load_number] ASC)
);
GO

-- Index for transaction-based queries
CREATE NONCLUSTERED INDEX [IX_receiving_loads_transaction_id]
    ON [dbo].[receiving_loads]([transaction_id] ASC)
    INCLUDE ([load_number], [part_id], [quantity]);
GO

-- Index for part number analytics
CREATE NONCLUSTERED INDEX [IX_receiving_loads_part_id]
    ON [dbo].[receiving_loads]([part_id] ASC, [created_at] DESC)
    INCLUDE ([quantity], [unit_of_measure]);
GO

-- Index for quality hold filtering
CREATE NONCLUSTERED INDEX [IX_receiving_loads_quality_hold]
    ON [dbo].[receiving_loads]([quality_hold_acknowledged] ASC)
    WHERE [quality_hold_acknowledged] = 1;
GO
