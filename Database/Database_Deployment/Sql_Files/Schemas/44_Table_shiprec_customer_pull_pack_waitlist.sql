DROP TABLE IF EXISTS customer_pull_pack_waitlist;

CREATE TABLE IF NOT EXISTS customer_pull_pack_waitlist (
    waitlist_id CHAR(36) NOT NULL COMMENT 'Internal stable waitlist identifier (UUID text)',
    source_line_key VARCHAR(150) NOT NULL COMMENT 'Hidden report-side source row key used to link demand rows to MTM waitlist work',
    customer_id VARCHAR(15) NOT NULL COMMENT 'Infor Visual customer identifier',
    customer_name VARCHAR(255) NOT NULL COMMENT 'Cached customer display name from the originating report line',
    customer_order_id VARCHAR(15) NOT NULL COMMENT 'Infor Visual customer order number from the originating report line',
    parent_part_id VARCHAR(30) NOT NULL COMMENT 'Parent part number tied to the originating report line',
    requested_quantity DECIMAL(20, 8) NOT NULL COMMENT 'Derived quantity requested for this one waitlist entry',
    requested_by_user_id VARCHAR(100) NOT NULL COMMENT 'Requester user identifier',
    requested_by_display_name VARCHAR(255) NOT NULL COMMENT 'Requester display name shown in the queue',
    requester_context_note TEXT NULL COMMENT 'Requester-facing context or exception note',
    current_status ENUM('Requested', 'Accepted', 'Completed', 'Cancelled', 'Problem') NOT NULL DEFAULT 'Requested' COMMENT 'Current single waitlist status - Workflow 2.1/2.2/3.1',
    current_owner_user_id VARCHAR(100) NULL COMMENT 'Current material-handler owner, blank when unowned',
    current_owner_display_name VARCHAR(255) NULL COMMENT 'Display name for the current owner',
    location_review_flag TINYINT(1) NOT NULL DEFAULT 0 COMMENT '1 when the request was created without a selectable location and needs follow-up review',
    problem_reason ENUM('None', 'NotInLocation', 'IncorrectQty', 'IncorrectPartNumber', 'Other') NOT NULL DEFAULT 'None' COMMENT 'Current Problem reason if the waitlist line is in Problem state',
    handler_note TEXT NULL COMMENT 'Handler-facing execution note or problem detail',
    completion_user_id VARCHAR(100) NULL COMMENT 'User who completed or cancelled the line most recently',
    completion_timestamp DATETIME NULL COMMENT 'Timestamp when the line was completed or cancelled',
    last_updated_by_user_id VARCHAR(100) NOT NULL COMMENT 'User who last saved the waitlist entry',
    last_updated_timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Last save timestamp for last-write-wins behavior',
    request_timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Original request timestamp',
    recheck_indicator TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Requester-facing recheck flag kept outside the normal status model',
    open_instance_key VARCHAR(191) AS (
        CASE
            WHEN current_status IN ('Requested', 'Accepted', 'Problem') THEN source_line_key
            ELSE CONCAT(source_line_key, '|', waitlist_id)
        END
    ) STORED COMMENT 'Generated uniqueness key that enforces only one open waitlist entry per source line',
    PRIMARY KEY (waitlist_id),
    UNIQUE KEY uq_customer_pull_pack_waitlist_open_instance (open_instance_key),
    INDEX idx_customer_pull_pack_waitlist_source (source_line_key),
    INDEX idx_customer_pull_pack_waitlist_status (current_status),
    INDEX idx_customer_pull_pack_waitlist_customer (customer_id),
    INDEX idx_customer_pull_pack_waitlist_order (customer_order_id),
    INDEX idx_customer_pull_pack_waitlist_part (parent_part_id),
    INDEX idx_customer_pull_pack_waitlist_requester (requested_by_user_id),
    INDEX idx_customer_pull_pack_waitlist_owner (current_owner_user_id),
    INDEX idx_customer_pull_pack_waitlist_updated (last_updated_timestamp)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'MTM-managed Customer Pull n'' Pack waitlist entries';