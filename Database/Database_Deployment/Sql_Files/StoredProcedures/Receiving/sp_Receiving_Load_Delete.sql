-- Stored Procedure: sp_Receiving_Load_Delete
-- Description: Deletes a receiving history row by the persisted integer record ID
--              when available, otherwise falls back to the GUID for rows that only
--              have app-generated identity data.

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Receiving_Load_Delete` $$

CREATE PROCEDURE `sp_Receiving_Load_Delete`(
    IN p_LoadID CHAR(36),
    IN p_HistoryRecordID INT
)
BEGIN
    SET FOREIGN_KEY_CHECKS = 0;
    DELETE FROM receiving_history
    WHERE (p_HistoryRecordID IS NOT NULL AND id = p_HistoryRecordID)
       OR (
            p_HistoryRecordID IS NULL
            AND p_LoadID IS NOT NULL
            AND p_LoadID <> ''
            AND load_guid = p_LoadID
        );
    SET FOREIGN_KEY_CHECKS = 1;
END $$

DELIMITER ;
