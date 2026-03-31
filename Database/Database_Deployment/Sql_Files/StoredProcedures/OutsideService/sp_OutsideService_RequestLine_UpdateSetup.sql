DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_OutsideService_RequestLine_UpdateSetup`$$

CREATE PROCEDURE `sp_OutsideService_RequestLine_UpdateSetup`(
    IN p_outside_service_request_line_id INT,
    IN p_package_count INT,
    IN p_package_summary VARCHAR(500),
    IN p_setup_vendor_id VARCHAR(50),
    IN p_setup_vendor_name VARCHAR(255),
    IN p_setup_vendor_source VARCHAR(30),
    IN p_bol_number VARCHAR(100),
    IN p_scheduled_ship_utc DATETIME,
    IN p_shipping_contact VARCHAR(255),
    IN p_setup_notes TEXT
)
BEGIN
    DECLARE v_package_index INT DEFAULT 1;
    DECLARE v_package_quantity_text VARCHAR(50);

    UPDATE outside_service_request_line
    SET
        package_count = p_package_count,
        package_summary = p_package_summary,
        setup_vendor_id = p_setup_vendor_id,
        setup_vendor_name = p_setup_vendor_name,
        setup_vendor_source = p_setup_vendor_source,
        bol_number = p_bol_number,
        scheduled_ship_utc = p_scheduled_ship_utc,
        shipping_contact = p_shipping_contact,
        setup_notes = p_setup_notes,
        line_phase = 'Setup'
    WHERE outside_service_request_line_id = p_outside_service_request_line_id;

    DELETE FROM outside_service_request_package
    WHERE outside_service_request_line_id = p_outside_service_request_line_id;

    WHILE v_package_index <= p_package_count DO
        SET v_package_quantity_text = TRIM(
            SUBSTRING_INDEX(
                SUBSTRING_INDEX(COALESCE(p_package_summary, ''), ' / ', v_package_index),
                ' / ',
                -1
            )
        );

        IF v_package_quantity_text <> '' THEN
            INSERT INTO outside_service_request_package (
                outside_service_request_line_id,
                package_sequence,
                package_quantity
            )
            VALUES (
                p_outside_service_request_line_id,
                v_package_index,
                CAST(v_package_quantity_text AS DECIMAL(18, 4))
            );
        END IF;

        SET v_package_index = v_package_index + 1;
    END WHILE;
END$$

DELIMITER ;