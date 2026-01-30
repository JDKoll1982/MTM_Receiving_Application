-- ==============================================================================
-- Stored Procedure: sp_Receiving_QualityHold_SelectByLineID
-- Purpose: Retrieve all quality holds for a specific line
-- Module: Module_Receiving
-- Created: 2026-01-30
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_QualityHold_SelectByLineID]
    @p_LineId CHAR(36)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF @p_LineId IS NULL OR @p_LineId = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'LineId is required' AS ErrorMessage;
            RETURN;
        END
        
        SELECT 
            [QualityHoldId],
            [LineId],
            [TransactionId],
            [PartNumber],
            [PartPattern],
            [RestrictionType],
            [UserAcknowledgedDate],
            [UserAcknowledgedBy],
            [UserAcknowledgmentMessage],
            [FinalAcknowledgedDate],
            [FinalAcknowledgedBy],
            [FinalAcknowledgmentMessage],
            [IsFullyAcknowledged],
            [QualityInspectorName],
            [QualityInspectorDate],
            [QualityInspectorNotes],
            [IsReleased],
            [ReleasedDate],
            [LoadNumber],
            [TotalWeight],
            [PackageType],
            [Notes],
            [IsActive],
            [IsDeleted],
            [CreatedBy],
            [CreatedDate],
            [ModifiedBy],
            [ModifiedDate]
        FROM [dbo].[tbl_Receiving_QualityHold]
        WHERE [LineId] = @p_LineId
          AND [IsActive] = 1 
          AND [IsDeleted] = 0
        ORDER BY [CreatedDate] DESC;
        
        IF @@ROWCOUNT = 0
        BEGIN
            SELECT 0 AS IsSuccess, 'No quality hold records found for this line' AS ErrorMessage;
        END
        ELSE
        BEGIN
            SELECT 1 AS IsSuccess, '' AS ErrorMessage;
        END
    END TRY
    BEGIN CATCH
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves all quality hold records for a specific line. Returns records ordered by creation date (newest first).',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_QualityHold_SelectByLineID';
GO
