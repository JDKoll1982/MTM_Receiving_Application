DELIMITER //

DROP PROCEDURE IF EXISTS `sp_Receiving_History_Get` //

CREATE PROCEDURE `sp_Receiving_History_Get`(
    IN p_PartID VARCHAR(50),
    IN p_StartDate DATETIME,
    IN p_EndDate DATETIME
)
BEGIN
    SELECT * FROM receiving_history
    WHERE PartID = p_PartID
    AND ReceivedDate BETWEEN p_StartDate AND p_EndDate
    ORDER BY ReceivedDate DESC;
END //

DELIMITER ;
