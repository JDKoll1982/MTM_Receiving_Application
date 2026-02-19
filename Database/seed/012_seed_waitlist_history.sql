USE `mtm_waitlist`;

INSERT INTO `waitlist_history` (
  `WorkCenter`, `RequestType`, `Request`, `RequestPriority`, `MHandler`,
  `TimeRemaining`, `RequestTime`, `StartTime`, `CompleteTime`
)
VALUES
  (
    'WC-02', 'Scrap Pickup', 'Remove scrap bin', 'Normal', 'Alice Smith',
    DATE_ADD(NOW(), INTERVAL -1 HOUR), DATE_ADD(NOW(), INTERVAL -2 HOUR), DATE_ADD(NOW(), INTERVAL -90 MINUTE), NOW()
  );
