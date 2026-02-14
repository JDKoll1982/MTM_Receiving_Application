USE `mtm_waitlist`;

CREATE TABLE IF NOT EXISTS `waitlist_ncm_report` (
  `ID` INT NOT NULL AUTO_INCREMENT,
  `PartID` VARCHAR(50) NOT NULL,
  `WorkCenter` VARCHAR(50) NOT NULL,
  `RequestTime` DATETIME NOT NULL,
  `MHandler` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `idx_waitlist_ncm_report_part` (`PartID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
