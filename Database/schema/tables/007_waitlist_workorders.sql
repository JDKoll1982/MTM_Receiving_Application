USE `mtm_waitlist`;

CREATE TABLE IF NOT EXISTS `waitlist_workorders` (
  `WorkOrder` VARCHAR(50) NOT NULL,
  `PartID` VARCHAR(50) NOT NULL,
  PRIMARY KEY (`WorkOrder`),
  KEY `idx_waitlist_workorders_partid` (`PartID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
