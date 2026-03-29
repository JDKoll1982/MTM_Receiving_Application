DROP TABLE IF EXISTS outside_service_request_package;

CREATE TABLE IF NOT EXISTS outside_service_request_package (
    outside_service_request_package_id INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for outside service package row',
    outside_service_request_line_id INT NOT NULL COMMENT 'FK to outside_service_request_line.outside_service_request_line_id',
    package_sequence INT NOT NULL COMMENT '1-based package sequence number',
    package_quantity DECIMAL(18, 4) NOT NULL COMMENT 'Quantity in the physical package',
    PRIMARY KEY (outside_service_request_package_id),
    UNIQUE KEY uq_outside_service_request_package_line_sequence (outside_service_request_line_id, package_sequence),
    INDEX idx_outside_service_request_package_line (outside_service_request_line_id),
    CONSTRAINT fk_outside_service_request_package_line FOREIGN KEY (outside_service_request_line_id) REFERENCES outside_service_request_line (outside_service_request_line_id) ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Outside Service per-package quantities';