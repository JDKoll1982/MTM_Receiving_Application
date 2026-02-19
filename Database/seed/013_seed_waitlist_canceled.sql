USE `mtm_waitlist`;

INSERT INTO `waitlist_canceled` (
  `WorkCenter`, `RequestType`, `Request`, `RequestPriority`, `MHandler`,
  `TimeRemaining`, `RequestTime`, `StartTime`, `CanceledTime`
)
VALUES
  (
    'WC-01', 'Setup Request', 'Setup for PART-1002', 'Normal', 'John Doe',
    DATE_ADD(NOW(), INTERVAL -2 HOUR), DATE_ADD(NOW(), INTERVAL -3 HOUR), DATE_ADD(NOW(), INTERVAL -170 MINUTE), DATE_ADD(NOW(), INTERVAL -160 MINUTE)
  );
