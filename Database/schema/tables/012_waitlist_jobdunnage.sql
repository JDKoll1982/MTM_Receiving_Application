USE `mtm_waitlist`;

CREATE TABLE IF NOT EXISTS `waitlist_jobdunnage` (
  `PartID` VARCHAR(50) NOT NULL,
  `Operation` VARCHAR(50) NOT NULL,
  `Dunnage` VARCHAR(100) NULL,
  `Skid` VARCHAR(100) NULL,
  `Cardboard` VARCHAR(100) NULL,
  `Boxes` VARCHAR(100) NULL,
  `Other1` VARCHAR(100) NULL,
  `Other2` VARCHAR(100) NULL,
  `Other3` VARCHAR(100) NULL,
  `Other4` VARCHAR(100) NULL,
  `Other5` VARCHAR(100) NULL,
  `PartType` VARCHAR(50) NULL,
  PRIMARY KEY (`PartID`, `Operation`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
