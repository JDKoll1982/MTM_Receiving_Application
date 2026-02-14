USE `mtm_waitlist`;

CREATE TABLE IF NOT EXISTS `waitlist_dies` (
  `PartID` VARCHAR(50) NOT NULL,
  `Operation` VARCHAR(50) NOT NULL,
  `FGT` VARCHAR(50) NOT NULL,
  `Location` VARCHAR(100) NULL,
  PRIMARY KEY (`PartID`, `Operation`, `FGT`),
  KEY `idx_waitlist_dies_fgt` (`FGT`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
