-- Stored Procedure: sp_Receiving_Load_GetAll
-- Description: Retrieves all receiving loads within a date range for Edit Mode.
--              Aliases snake_case table columns to the PascalCase names
--              expected by Dao_ReceivingLoad.MapRowToLoad.
--              Columns with no counterpart in receiving_history return NULL.
-- Parameters:
--   p_StartDate - Start date for retrieval (DATE)
--   p_EndDate   - End date for retrieval   (DATE)

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_Load_GetAll`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_Load_GetAll`(
    IN p_StartDate DATE,
    IN p_EndDate   DATE
)
BEGIN
    SELECT
        load_guid           AS LoadID,
        part_id             AS PartID,
        part_description    AS PartDescription,
        NULL                AS PartType,
        po_number           AS PONumber,
        NULL                AS POLineNumber,
        vendor_name         AS POVendor,
        NULL                AS POStatus,
        NULL                AS PODueDate,
        NULL                AS QtyOrdered,
        NULL                AS UnitOfMeasure,
        NULL                AS RemainingQuantity,
        label_number        AS LoadNumber,
        quantity            AS WeightQuantity,
        heat                AS HeatLotNumber,
        NULL                AS PackagesPerLoad,
        NULL                AS PackageTypeName,
        NULL                AS WeightPerPackage,
        is_non_po_item      AS IsNonPOItem,
        transaction_date    AS ReceivedDate,
        NULL                AS UserID,
        employee_number     AS EmployeeNumber,
        0                   AS IsQualityHoldRequired,
        0                   AS IsQualityHoldAcknowledged,
        NULL                AS QualityHoldRestrictionType
    FROM receiving_history
    WHERE transaction_date >= p_StartDate
      AND transaction_date <= p_EndDate
    ORDER BY transaction_date DESC, label_number ASC;
END$$

DELIMITER ;
