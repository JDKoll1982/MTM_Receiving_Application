CREATE TABLE IF NOT EXISTS receiving_scanner_item (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    session_id CHAR(36) NOT NULL COMMENT 'Parent scanner session id',
    item_order INT NOT NULL COMMENT 'Stable ordered position in current list',
    part_id VARCHAR(50) NOT NULL,
    from_warehouse_id VARCHAR(10) NOT NULL,
    from_location_id VARCHAR(50) NULL,
    to_warehouse_id VARCHAR(10) NOT NULL,
    to_location_id VARCHAR(50) NULL,
    quantity DECIMAL(18,2) NOT NULL,
    payload_json JSON NULL COMMENT 'Optional serialized payload snapshot',
    validation_state VARCHAR(24) NOT NULL DEFAULT 'NotValidated' COMMENT 'NotValidated, Valid, Invalid',
    validation_notes VARCHAR(500) NULL COMMENT 'User-facing validation result shown in Manage Items notes column',
    status VARCHAR(24) NOT NULL DEFAULT 'Waiting' COMMENT 'Waiting, Sent, Failed, Skipped',
    failure_code VARCHAR(120) NULL,
    failure_message VARCHAR(500) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_receiving_scanner_item_session
        FOREIGN KEY (session_id) REFERENCES receiving_scanner_session(id)
        ON DELETE CASCADE,
    UNIQUE KEY uq_receiving_scanner_item_session_order (session_id, item_order),
    INDEX idx_receiving_scanner_item_session (session_id),
    INDEX idx_receiving_scanner_item_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Ordered scanner items in a current list session.';
