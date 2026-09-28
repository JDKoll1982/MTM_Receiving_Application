CREATE TABLE IF NOT EXISTS receiving_scanner_run_item (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    run_id CHAR(36) NOT NULL,
    session_item_id BIGINT NOT NULL,
    item_order INT NOT NULL,
    part_id VARCHAR(50) NOT NULL,
    from_warehouse_id VARCHAR(10) NOT NULL,
    from_location_id VARCHAR(50) NULL,
    to_warehouse_id VARCHAR(10) NOT NULL,
    to_location_id VARCHAR(50) NULL,
    quantity DECIMAL(18,2) NOT NULL,
    result_status VARCHAR(24) NOT NULL COMMENT 'Sent, Failed, Skipped',
    attempt_count INT NOT NULL DEFAULT 1,
    failure_code VARCHAR(120) NULL,
    failure_message VARCHAR(500) NULL,
    processed_at DATETIME NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_receiving_scanner_run_item_run
        FOREIGN KEY (run_id) REFERENCES receiving_scanner_run(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_receiving_scanner_run_item_session_item
        FOREIGN KEY (session_item_id) REFERENCES receiving_scanner_item(id)
        ON DELETE RESTRICT,
    INDEX idx_receiving_scanner_run_item_run (run_id),
    INDEX idx_receiving_scanner_run_item_order (run_id, item_order),
    INDEX idx_receiving_scanner_run_item_status (result_status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Per-item execution outcomes for each scanner run, including source and destination traceability.';