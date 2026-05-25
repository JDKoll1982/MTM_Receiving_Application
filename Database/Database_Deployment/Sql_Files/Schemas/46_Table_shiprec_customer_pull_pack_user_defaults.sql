DROP TABLE IF EXISTS customer_pull_pack_user_defaults;

CREATE TABLE IF NOT EXISTS customer_pull_pack_user_defaults (
    user_id VARCHAR(100) NOT NULL COMMENT 'User identifier owning this saved default set',
    default_customer_id VARCHAR(15) NULL COMMENT 'Preferred startup customer identifier',
    favorite_customer_ids_json TEXT NOT NULL COMMENT 'JSON array of favorite customer IDs',
    last_good_date_range_type VARCHAR(50) NULL COMMENT 'Named date-range behavior preference such as Today or ThisWeek',
    last_good_date_from DATETIME NULL COMMENT 'Last working date-range start used by the user',
    last_good_date_to DATETIME NULL COMMENT 'Last working date-range end used by the user',
    default_sort_mode ENUM('PullDate', 'ShortageFirst', 'Part') NOT NULL DEFAULT 'PullDate' COMMENT 'Saved default grid sort mode',
    default_shortages_only TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Saved shortage-only filter default',
    default_unpulled_only TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Saved unpulled/open-demand filter default',
    default_late_orders_only TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Saved late-order-only filter default',
    default_waitlist_status_set_json TEXT NOT NULL COMMENT 'JSON array of default queue statuses, typically Requested/Accepted/Problem',
    default_print_preset ENUM('CurrentView', 'FloorCopy', 'PullList', 'ShortageOnly', 'WaitlistOnly', 'SelectedContext') NOT NULL DEFAULT 'CurrentView' COMMENT 'Saved default print preset',
    last_updated_by_user_id VARCHAR(100) NOT NULL COMMENT 'User who last saved this defaults row',
    last_updated_timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Last save timestamp',
    PRIMARY KEY (user_id)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Per-user defaults for the Customer Pull n'' Pack tool';