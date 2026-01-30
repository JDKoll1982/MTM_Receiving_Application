-- ==============================================================================
-- Function: fn_Receiving_CalculateTotalWeight
-- Purpose: Calculate total weight for a transaction from all active lines
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE FUNCTION [dbo].[fn_Receiving_CalculateTotalWeight]
(
    @TransactionId CHAR(36)
)
RETURNS DECIMAL(18, 2)
AS
BEGIN
    DECLARE @TotalWeight DECIMAL(18, 2);
    
    SELECT @TotalWeight = SUM([Weight])
    FROM [dbo].[tbl_Receiving_Line]
    WHERE [TransactionId] = @TransactionId
      AND [IsActive] = 1
      AND [IsDeleted] = 0
      AND [Weight] IS NOT NULL;
    
    RETURN ISNULL(@TotalWeight, 0.00);
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Calculates total weight for a transaction by summing all active line weights. Returns 0 if no weights found.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'FUNCTION', @level1name = N'fn_Receiving_CalculateTotalWeight';
GO
