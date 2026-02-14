USE `mtm_waitlist`;

CREATE TABLE IF NOT EXISTS `waitlist_history` (
  `ID` INT NOT NULL AUTO_INCREMENT,
  `WorkCenter` VARCHAR(50) NOT NULL,
  `RequestType` VARCHAR(100) NOT NULL,
  `Request` VARCHAR(255) NOT NULL,
  `RequestPriority` VARCHAR(20) NOT NULL,
  `MHandler` VARCHAR(100) NOT NULL DEFAULT '',
  `TimeRemaining` DATETIME NOT NULL,
  `RequestTime` DATETIME NOT NULL,
  `StartTime` DATETIME NULL,
  `CompleteTime` DATETIME NULL,
  PRIMARY KEY (`ID`),
  KEY `idx_waitlist_history_workcenter` (`WorkCenter`),
  KEY `idx_waitlist_history_requesttime` (`RequestTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
