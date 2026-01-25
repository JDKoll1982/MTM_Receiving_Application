-- =============================================
-- Stored Procedure: sp_Receiving_Transaction_Insert
-- Description: Insert a new receiving transaction with loads
-- Used by: Step 3 - Save operation
-- =============================================
CREATE PROCEDURE [dbo].[sp_Receiving_Transaction_Insert]
    @PONumber NVARCHAR(50) = NULL,
    @UserId INT,
    @UserName NVARCHAR(100),
    @WorkflowMode NVARCHAR(20),
    @LoadsJson NVARCHAR(MAX) -- JSON array of loads
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @TransactionId INT;
        
        -- Insert transaction
        INSERT INTO receiving_transactions (
            po_number, user_id, user_name, workflow_mode,
            transaction_date, exported_to_csv, created_at, updated_at
        )
        VALUES (
            @PONumber, @UserId, @UserName, @WorkflowMode,
            GETDATE(), 0, GETDATE(), GETDATE()
        );
        
        SET @TransactionId = SCOPE_IDENTITY();
        
        -- Insert loads from JSON
        INSERT INTO receiving_loads (
            transaction_id, load_number, part_id, part_type, quantity,
            unit_of_measure, heat_lot_number, package_type, packages_per_load,
            weight_per_package, receiving_location, quality_hold_acknowledged,
            created_at, updated_at
        )
        SELECT
            @TransactionId,
            JSON_VALUE(value, '$.LoadNumber'),
            JSON_VALUE(value, '$.PartId'),
            JSON_VALUE(value, '$.PartType'),
            CAST(JSON_VALUE(value, '$.Quantity') AS DECIMAL(18,3)),
            JSON_VALUE(value, '$.UnitOfMeasure'),
            JSON_VALUE(value, '$.HeatLotNumber'),
            JSON_VALUE(value, '$.PackageType'),
            CAST(JSON_VALUE(value, '$.PackagesPerLoad') AS INT),
            CAST(JSON_VALUE(value, '$.WeightPerPackage') AS DECIMAL(18,3)),
            JSON_VALUE(value, '$.ReceivingLocation'),
            CAST(JSON_VALUE(value, '$.QualityHoldAcknowledged') AS BIT),
            GETDATE(),
            GETDATE()
        FROM OPENJSON(@LoadsJson);
        
        COMMIT TRANSACTION;
        
        SELECT @TransactionId AS TransactionId, 'SUCCESS' AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO
