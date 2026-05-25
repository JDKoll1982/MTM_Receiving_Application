DROP TABLE IF EXISTS customer_pull_pack_waitlist_location;

CREATE TABLE IF NOT EXISTS customer_pull_pack_waitlist_location (
    customer_pull_pack_waitlist_location_id INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for one selected location row',
    waitlist_id CHAR(36) NOT NULL COMMENT 'FK to customer_pull_pack_waitlist.waitlist_id',
    selection_order INT NOT NULL DEFAULT 1 COMMENT 'Order captured from the UI selection list',
    location_id VARCHAR(30) NOT NULL COMMENT 'Selected location identifier from the main report display',
    selected_by_user_id VARCHAR(100) NOT NULL COMMENT 'User who last saved this selected location set',
    selected_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when this location row was last written',
    PRIMARY KEY (customer_pull_pack_waitlist_location_id),
    UNIQUE KEY uq_customer_pull_pack_waitlist_location (waitlist_id, location_id),
    INDEX idx_customer_pull_pack_waitlist_location_waitlist (waitlist_id),
    INDEX idx_customer_pull_pack_waitlist_location_location (location_id),
    CONSTRAINT fk_customer_pull_pack_waitlist_location_waitlist FOREIGN KEY (waitlist_id) REFERENCES customer_pull_pack_waitlist (waitlist_id) ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Selected pull locations for each Customer Pull n'' Pack waitlist entry';