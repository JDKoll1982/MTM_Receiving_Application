USE `mtm_waitlist`;

CREATE TABLE IF NOT EXISTS `waitlist_setupnotes` (
  `PartID` VARCHAR(50) NOT NULL,
  `Operation` VARCHAR(50) NOT NULL,
  `SetupNotes` TEXT NULL,
  PRIMARY KEY (`PartID`, `Operation`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
