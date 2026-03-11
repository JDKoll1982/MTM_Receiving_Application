-- Stored Procedure: sp_Receiving_LabelData_GetAll
-- Description: Retrieves all rows from receiving_label_data (the active print queue).
--              Used by Edit Mode "Current Labels" to load today's pending labels from the
--              database instead of from XLS files.
--              Aliases snake_case columns to the PascalCase names expected by
--              Dao_ReceivingLoad.MapRowToLoad.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_LabelData_GetAll`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_LabelData_GetAll`()
BEGIN
    SELECT
        load_id                       AS LoadID,
        part_id                       AS PartID,
        part_description              AS PartDescription,
        part_type                     AS PartType,
        po_number                     AS PONumber,
        po_line_number                AS POLineNumber,
        po_vendor                     AS POVendor,
        po_status                     AS POStatus,
        po_due_date                   AS PODueDate,
        qty_ordered                   AS QtyOrdered,
        unit_of_measure               AS UnitOfMeasure,
        remaining_quantity            AS RemainingQuantity,
        load_number                   AS LoadNumber,
        weight_quantity               AS WeightQuantity,
        heat                          AS HeatLotNumber,
        packages_per_load             AS PackagesPerLoad,
        package_type_name             AS PackageTypeName,
        weight_per_package            AS WeightPerPackage,
        is_non_po_item                AS IsNonPOItem,
        received_date                 AS ReceivedDate,
        user_id                       AS UserID,
        employee_number               AS EmployeeNumber,
        is_quality_hold_required      AS IsQualityHoldRequired,
        is_quality_hold_acknowledged  AS IsQualityHoldAcknowledged,
        quality_hold_restriction_type AS QualityHoldRestrictionType
    FROM receiving_label_data
    ORDER BY load_number ASC;
END$$

DELIMITER ;
