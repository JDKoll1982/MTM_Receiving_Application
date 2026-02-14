USE `mtm_waitlist`;

CREATE TABLE IF NOT EXISTS `waitlist_errors` (
  `ID` INT NOT NULL AUTO_INCREMENT,
  `method` VARCHAR(100) NOT NULL,
  `message` TEXT NOT NULL,
  `date` DATE NOT NULL,
  `time` TIME NOT NULL,
  `user` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `idx_waitlist_errors_method` (`method`),
  KEY `idx_waitlist_errors_date` (`date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
