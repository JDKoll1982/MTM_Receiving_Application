DROP TABLE IF EXISTS outside_service_request;

CREATE TABLE IF NOT EXISTS outside_service_request (
    outside_service_request_id INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for outside service request header',
    request_number VARCHAR(20) NOT NULL COMMENT 'Human-readable request number in the OS-000001 format',
    created_by_user VARCHAR(255) NOT NULL COMMENT 'Windows username of the request creator',
    created_by_display VARCHAR(255) NOT NULL COMMENT 'Display name of the request creator',
    created_utc DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'UTC timestamp when the request was created',
    request_notes TEXT NULL COMMENT 'Request-level notes for Shipping',
    PRIMARY KEY (outside_service_request_id),
    UNIQUE KEY uq_outside_service_request_number (request_number),
    INDEX idx_outside_service_request_created_utc (created_utc)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Outside Service request headers';