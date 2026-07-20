-- Stored Procedure: sp_Receiving_Load_Update
-- Description: Updates an existing receiving history row by the persisted integer
--              record ID when available, otherwise falls back to the GUID.
--              Parameter names match Dao_ReceivingLoad.UpdateLoadsAsync exactly
--              (the DAO helper auto-prepends p_ to each key).
--              Parameters for fields not stored in receiving_history are still
--              used to keep the active receiving_label_data row aligned.

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Receiving_Load_Update` $$

CREATE PROCEDURE `sp_Receiving_Load_Update`(
    IN p_LoadID           CHAR(36),
    IN p_HistoryRecordID  INT,
    IN p_PartID           VARCHAR(50),
    IN p_PartType         VARCHAR(100),
    IN p_PONumber         VARCHAR(20),
    IN p_POLineNumber     VARCHAR(50),
    IN p_LoadNumber       INT,
    IN p_WeightQuantity   DECIMAL(18,4),
    IN p_HeatLotNumber    VARCHAR(100),
    IN p_InitialLocation  VARCHAR(50),
    IN p_PackagesPerLoad  INT,
    IN p_PackageTypeName  VARCHAR(100),
    IN p_WeightPerPackage DECIMAL(18,4),
    IN p_IsNonPOItem      TINYINT(1),
    IN p_ReceivedDate     DATETIME
)
BEGIN
    SET FOREIGN_KEY_CHECKS = 0;
    UPDATE receiving_history
    SET
        part_id          = p_PartID,
        weight_quantity  = p_WeightQuantity,
        part_type        = p_PartType,
        po_line_number   = p_POLineNumber,
        po_number        = p_PONumber,
        load_number      = IFNULL(p_LoadNumber, 1),
        quantity         = ROUND(p_WeightQuantity, 0),
        weight_per_package = p_WeightPerPackage,
        packages_per_load = p_PackagesPerLoad,
        package_type_name = p_PackageTypeName,
        heat             = p_HeatLotNumber,
        initial_location = p_InitialLocation,
        transaction_date = DATE(p_ReceivedDate),
        label_number     = IFNULL(p_LoadNumber, 1),
        is_non_po_item   = IFNULL(p_IsNonPOItem, 0)
    WHERE (p_HistoryRecordID IS NOT NULL AND id = p_HistoryRecordID)
       OR (
            p_HistoryRecordID IS NULL
            AND p_LoadID IS NOT NULL
            AND p_LoadID <> ''
            AND load_guid = p_LoadID
        );

    UPDATE receiving_label_data
    SET
        part_id = p_PartID,
        part_type = p_PartType,
        po_number = p_PONumber,
        po_line_number = p_POLineNumber,
        load_number = IFNULL(p_LoadNumber, 1),
        quantity = ROUND(p_WeightQuantity, 0),
        weight_quantity = p_WeightQuantity,
        heat = p_HeatLotNumber,
        initial_location = p_InitialLocation,
        packages_per_load = p_PackagesPerLoad,
        package_type_name = p_PackageTypeName,
        weight_per_package = p_WeightPerPackage,
        is_non_po_item = IFNULL(p_IsNonPOItem, 0),
        received_date = p_ReceivedDate,
        transaction_date = DATE(p_ReceivedDate)
    WHERE load_id = p_LoadID;
    SET FOREIGN_KEY_CHECKS = 1;
END $$

DELIMITER;