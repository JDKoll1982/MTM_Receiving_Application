-- Stored Procedure: sp_Receiving_LabelData_ClearToHistory
-- Description: Atomically moves all rows from receiving_label_data (queue) to receiving_history (archive)
-- Lifecycle: Clear Label Data -> Queue to History + Queue Delete

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_LabelData_ClearToHistory`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_LabelData_ClearToHistory`(
    IN p_archived_by VARCHAR(100),
    OUT p_rows_moved INT,
    OUT p_archive_batch_id CHAR(36),
    OUT p_status INT,
    OUT p_error_message VARCHAR(1000)
)
BEGIN
    DECLARE v_rows_to_move INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_rows_moved = 0;
        SET p_status = 1;
        SET p_error_message = 'Clear Label Data failed. Transaction rolled back.';
    END;

    SET p_rows_moved = 0;
    SET p_status = 0;
    SET p_error_message = NULL;
    SET p_archive_batch_id = UUID();

    START TRANSACTION;

    SELECT COUNT(*) INTO v_rows_to_move
    FROM receiving_label_data;

    IF v_rows_to_move = 0 THEN
        COMMIT;
        SET p_rows_moved = 0;
        SET p_status = 0;
        SET p_error_message = NULL;
    ELSE
        INSERT INTO receiving_history
        (
            LoadID,
            PartID,
            PartType,
            PONumber,
            POLineNumber,
            LoadNumber,
            WeightQuantity,
            HeatLotNumber,
            PackagesPerLoad,
            PackageTypeName,
            WeightPerPackage,
            IsNonPOItem,
            ReceivedDate,
            PartDescription,
            POVendor,
            POStatus,
            PODueDate,
            QtyOrdered,
            UnitOfMeasure,
            RemainingQuantity,
            UserID,
            EmployeeNumber,
            IsQualityHoldRequired,
            IsQualityHoldAcknowledged,
            QualityHoldRestrictionType,
            ArchivedAt,
            ArchivedBy,
            ArchiveBatchID
        )
        SELECT
            COALESCE(rld.load_id, UUID()) AS LoadID,
            rld.part_id AS PartID,
            COALESCE(rld.part_type, 'Standard') AS PartType,
            rld.po_number AS PONumber,
            COALESCE(rld.po_line_number, '') AS POLineNumber,
            COALESCE(rld.load_number, 1) AS LoadNumber,
            COALESCE(rld.weight_quantity, CAST(rld.quantity AS DECIMAL(10,2))) AS WeightQuantity,
            COALESCE(rld.heat, '') AS HeatLotNumber,
            COALESCE(rld.packages_per_load, 1) AS PackagesPerLoad,
            COALESCE(rld.package_type_name, '') AS PackageTypeName,
            COALESCE(rld.weight_per_package, 0) AS WeightPerPackage,
            COALESCE(rld.is_non_po_item, 0) AS IsNonPOItem,
            COALESCE(rld.received_date, CAST(rld.transaction_date AS DATETIME), NOW()) AS ReceivedDate,
            rld.part_description AS PartDescription,
            COALESCE(rld.po_vendor, rld.vendor_name) AS POVendor,
            rld.po_status AS POStatus,
            rld.po_due_date AS PODueDate,
            rld.qty_ordered AS QtyOrdered,
            rld.unit_of_measure AS UnitOfMeasure,
            rld.remaining_quantity AS RemainingQuantity,
            rld.user_id AS UserID,
            rld.employee_number AS EmployeeNumber,
            COALESCE(rld.is_quality_hold_required, 0) AS IsQualityHoldRequired,
            COALESCE(rld.is_quality_hold_acknowledged, 0) AS IsQualityHoldAcknowledged,
            rld.quality_hold_restriction_type AS QualityHoldRestrictionType,
            NOW() AS ArchivedAt,
            p_archived_by AS ArchivedBy,
            p_archive_batch_id AS ArchiveBatchID
        FROM receiving_label_data rld
        ON DUPLICATE KEY UPDATE
            PartID = VALUES(PartID),
            PartType = VALUES(PartType),
            PONumber = VALUES(PONumber),
            POLineNumber = VALUES(POLineNumber),
            LoadNumber = VALUES(LoadNumber),
            WeightQuantity = VALUES(WeightQuantity),
            HeatLotNumber = VALUES(HeatLotNumber),
            PackagesPerLoad = VALUES(PackagesPerLoad),
            PackageTypeName = VALUES(PackageTypeName),
            WeightPerPackage = VALUES(WeightPerPackage),
            IsNonPOItem = VALUES(IsNonPOItem),
            ReceivedDate = VALUES(ReceivedDate),
            PartDescription = VALUES(PartDescription),
            POVendor = VALUES(POVendor),
            POStatus = VALUES(POStatus),
            PODueDate = VALUES(PODueDate),
            QtyOrdered = VALUES(QtyOrdered),
            UnitOfMeasure = VALUES(UnitOfMeasure),
            RemainingQuantity = VALUES(RemainingQuantity),
            UserID = VALUES(UserID),
            EmployeeNumber = VALUES(EmployeeNumber),
            IsQualityHoldRequired = VALUES(IsQualityHoldRequired),
            IsQualityHoldAcknowledged = VALUES(IsQualityHoldAcknowledged),
            QualityHoldRestrictionType = VALUES(QualityHoldRestrictionType),
            ArchivedAt = VALUES(ArchivedAt),
            ArchivedBy = VALUES(ArchivedBy),
            ArchiveBatchID = VALUES(ArchiveBatchID);

        DELETE FROM receiving_label_data;

        COMMIT;

        SET p_rows_moved = v_rows_to_move;
        SET p_status = 0;
        SET p_error_message = NULL;
    END IF;
END$$

DELIMITER ;
