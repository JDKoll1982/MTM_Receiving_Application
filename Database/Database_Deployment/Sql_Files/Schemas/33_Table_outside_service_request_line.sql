DROP TABLE IF EXISTS outside_service_request_line;

CREATE TABLE IF NOT EXISTS outside_service_request_line (
    outside_service_request_line_id INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for outside service request line',
    outside_service_request_id INT NOT NULL COMMENT 'FK to outside_service_request.outside_service_request_id',
    line_number INT NOT NULL COMMENT 'Line number within the request',
    part_id VARCHAR(50) NOT NULL COMMENT 'Infor Visual part identifier',
    package_count INT NOT NULL COMMENT 'Number of physical packages for this line',
    package_summary VARCHAR(500) NULL COMMENT 'Cached slash-delimited package summary for quick display',
    line_phase ENUM ('Initialize', 'Setup', 'Complete') NOT NULL DEFAULT 'Initialize' COMMENT 'Current line lifecycle phase',
    setup_vendor_id VARCHAR(50) NULL COMMENT 'Infor Visual vendor identifier when sourced from history',
    setup_vendor_name VARCHAR(255) NULL COMMENT 'Selected or custom vendor name',
    setup_vendor_source VARCHAR(30) NULL COMMENT 'Values: suggested or custom',
    bol_number VARCHAR(100) NULL COMMENT 'Shipping-owned bill of lading number',
    scheduled_ship_utc DATETIME NULL COMMENT 'Scheduled ship timestamp captured during setup',
    shipping_contact VARCHAR(255) NULL COMMENT 'Shipping contact or handoff owner',
    setup_notes TEXT NULL COMMENT 'Shipping setup notes',
    completed_utc DATETIME NULL COMMENT 'UTC timestamp when the line was completed',
    completion_notes TEXT NULL COMMENT 'Completion notes captured when the line is marked complete',
    PRIMARY KEY (outside_service_request_line_id),
    UNIQUE KEY uq_outside_service_request_line (outside_service_request_id, line_number),
    INDEX idx_outside_service_request_line_request (outside_service_request_id),
    INDEX idx_outside_service_request_line_phase (line_phase),
    INDEX idx_outside_service_request_line_part (part_id),
    INDEX idx_outside_service_request_line_vendor (setup_vendor_name),
    CONSTRAINT fk_outside_service_request_line_request FOREIGN KEY (outside_service_request_id) REFERENCES outside_service_request (outside_service_request_id) ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Outside Service request lines';