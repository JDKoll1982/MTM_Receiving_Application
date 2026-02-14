USE `mtm_waitlist`;

CREATE TABLE IF NOT EXISTS `waitlist_dunnage` (
  `ID` INT NOT NULL AUTO_INCREMENT,
  `Dunnage` VARCHAR(100) NULL,
  `Skid` VARCHAR(100) NULL,
  `Cardboard` VARCHAR(100) NULL,
  `Box` VARCHAR(100) NULL,
  `Other` VARCHAR(100) NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
