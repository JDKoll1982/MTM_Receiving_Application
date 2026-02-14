USE `mtm_waitlist`;

CREATE TABLE IF NOT EXISTS `waitlist_users` (
  `ID` INT NOT NULL AUTO_INCREMENT,
  `User` VARCHAR(25) NOT NULL,
  `Pin` VARCHAR(20) NOT NULL,
  `Full Name` VARCHAR(100) NOT NULL,
  `Shift` VARCHAR(20) NOT NULL,
  `User Type` VARCHAR(50) NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `uk_waitlist_users_user` (`User`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
