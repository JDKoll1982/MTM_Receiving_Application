DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_OutsideService_Request_GetOpen`$$

CREATE PROCEDURE `sp_OutsideService_Request_GetOpen`()
BEGIN
    SELECT
        r.outside_service_request_id,
        r.request_number,
        r.created_by_user,
        r.created_by_display,
        r.created_utc,
        r.request_notes,
        l.outside_service_request_line_id,
        l.line_number,
        l.part_id,
        l.package_count,
        l.package_summary,
        l.line_phase,
        l.setup_vendor_id,
        l.setup_vendor_name,
        l.setup_vendor_source,
        l.bol_number,
        l.scheduled_ship_utc,
        l.shipping_contact,
        l.setup_notes,
        l.completed_utc,
        l.completion_notes
    FROM outside_service_request r
    INNER JOIN outside_service_request_line l ON r.outside_service_request_id = l.outside_service_request_id
    WHERE l.line_phase <> 'Complete'
    ORDER BY r.created_utc DESC, l.line_number ASC;
END$$

DELIMITER ;