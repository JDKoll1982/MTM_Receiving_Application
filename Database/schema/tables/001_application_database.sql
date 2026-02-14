USE `mtm_waitlist`;

CREATE TABLE IF NOT EXISTS `application_database` (
  `ID` INT NOT NULL AUTO_INCREMENT,
  `work_center` VARCHAR(50) NOT NULL,
  `request_type` VARCHAR(100) NOT NULL,
  `DefaultTime` TIME NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `idx_application_database_work_center` (`work_center`),
  KEY `idx_application_database_request_type` (`request_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
