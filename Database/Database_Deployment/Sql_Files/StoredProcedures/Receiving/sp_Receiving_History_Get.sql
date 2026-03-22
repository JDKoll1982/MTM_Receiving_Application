-- ============================================================================
-- Procedure: sp_Receiving_History_Get
-- Purpose: Retrieve receiving history records filtered by part and date range.
--          Aliases snake_case table columns to the PascalCase names expected
--          by Dao_ReceivingLoad.MapRowToLoad. Parameters match
--          Dao_ReceivingLoad.GetHistoryAsync exactly (PartID, StartDate, EndDate).
-- All parameters are optional (pass NULL to skip that filter).
-- ============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS `sp_Receiving_History_Get` //

CREATE PROCEDURE `sp_Receiving_History_Get`(
    IN p_PartID    VARCHAR(50),
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
    WHERE
        (p_PartID    IS NULL OR part_id          = p_PartID)
        AND (p_StartDate IS NULL OR transaction_date >= p_StartDate)
        AND (p_EndDate   IS NULL OR transaction_date <= p_EndDate)
    ORDER BY transaction_date DESC, id DESC;
END //

DELIMITER ;
