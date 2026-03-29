DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_OutsideService_RequestPackage_Insert`$$

CREATE PROCEDURE `sp_OutsideService_RequestPackage_Insert`(
    IN p_outside_service_request_line_id INT,
    IN p_package_sequence INT,
    IN p_package_quantity DECIMAL(18, 4)
)
BEGIN
    INSERT INTO outside_service_request_package
    (
        outside_service_request_line_id,
        package_sequence,
        package_quantity
    )
    VALUES
    (
        p_outside_service_request_line_id,
        p_package_sequence,
        p_package_quantity
    );
END$$

DELIMITER ;