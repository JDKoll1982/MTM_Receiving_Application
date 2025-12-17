DELIMITER //

DROP PROCEDURE IF EXISTS sp_GetReceivingHistory //

CREATE PROCEDURE sp_GetReceivingHistory(
    IN p_PartID VARCHAR(50),
    IN p_StartDate DATETIME,
    IN p_EndDate DATETIME
)
BEGIN
    SELECT * FROM receiving_loads 
    WHERE PartID = p_PartID 
    AND ReceivedDate BETWEEN p_StartDate AND p_EndDate
    ORDER BY ReceivedDate DESC;
END //

DELIMITER ;
