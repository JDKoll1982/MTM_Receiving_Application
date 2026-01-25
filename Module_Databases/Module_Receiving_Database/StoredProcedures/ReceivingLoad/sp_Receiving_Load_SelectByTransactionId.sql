-- =============================================
-- Stored Procedure: sp_Receiving_Load_SelectByTransactionId
-- Description: Retrieve all loads for a transaction
-- Used by: Edit Mode and transaction review
-- =============================================
CREATE PROCEDURE [dbo].[sp_Receiving_Load_SelectByTransactionId]
    @TransactionId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT
        load_id,
        transaction_id,
        load_number,
        part_id,
        part_type,
        quantity,
        unit_of_measure,
        heat_lot_number,
        package_type,
        packages_per_load,
        weight_per_package,
        receiving_location,
        quality_hold_acknowledged,
        quality_hold_acknowledged_at,
        created_at,
        updated_at
    FROM receiving_loads
    WHERE transaction_id = @TransactionId
    ORDER BY load_number;
END
GO
