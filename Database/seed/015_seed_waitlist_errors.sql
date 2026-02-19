USE `mtm_waitlist`;

INSERT INTO `waitlist_errors` (`method`, `message`, `date`, `time`, `user`)
VALUES
  ('SeedData', 'Placeholder error log entry', CURDATE(), CURTIME(), 'system');
