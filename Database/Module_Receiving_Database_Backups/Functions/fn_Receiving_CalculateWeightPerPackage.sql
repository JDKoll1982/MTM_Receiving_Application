-- ==============================================================================
-- Function: fn_Receiving_CalculateWeightPerPackage
-- Purpose: Calculate weight per package from total weight and package count
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE FUNCTION [dbo].[fn_Receiving_CalculateWeightPerPackage]
(
    @TotalWeight DECIMAL(18, 2),
    @PackagesPerLoad INT
)
RETURNS DECIMAL(18, 2)
AS
BEGIN
    DECLARE @WeightPerPackage DECIMAL(18, 2);
    
    IF @PackagesPerLoad IS NULL OR @PackagesPerLoad = 0
        SET @WeightPerPackage = NULL;
    ELSE IF @TotalWeight IS NULL
        SET @WeightPerPackage = NULL;
    ELSE
        SET @WeightPerPackage = @TotalWeight / @PackagesPerLoad;
    
    RETURN @WeightPerPackage;
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Calculates weight per package by dividing total weight by number of packages. Returns NULL if either parameter is NULL or PackagesPerLoad is 0.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'FUNCTION', @level1name = N'fn_Receiving_CalculateWeightPerPackage';
GO
