DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_OutsideService_RequestLine_UpdateSetup`$$

CREATE PROCEDURE `sp_OutsideService_RequestLine_UpdateSetup`(
    IN p_outside_service_request_line_id INT,
    IN p_setup_vendor_id VARCHAR(50),
    IN p_setup_vendor_name VARCHAR(255),
    IN p_setup_vendor_source VARCHAR(30),
    IN p_bol_number VARCHAR(100),
    IN p_scheduled_ship_utc DATETIME,
    IN p_shipping_contact VARCHAR(255),
    IN p_setup_notes TEXT
)
BEGIN
    UPDATE outside_service_request_line
    SET
        setup_vendor_id = p_setup_vendor_id,
        setup_vendor_name = p_setup_vendor_name,
        setup_vendor_source = p_setup_vendor_source,
        bol_number = p_bol_number,
        scheduled_ship_utc = p_scheduled_ship_utc,
        shipping_contact = p_shipping_contact,
        setup_notes = p_setup_notes,
        line_phase = 'Setup'
    WHERE outside_service_request_line_id = p_outside_service_request_line_id;
END$$

DELIMITER ;