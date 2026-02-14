USE `mtm_waitlist`;

CREATE TABLE IF NOT EXISTS `waitlist_current_jobs` (
  `work_center` VARCHAR(50) NOT NULL,
  `work_order` VARCHAR(50) NOT NULL,
  `part_id` VARCHAR(50) NOT NULL,
  `operation` VARCHAR(50) NOT NULL,
  PRIMARY KEY (`work_center`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
