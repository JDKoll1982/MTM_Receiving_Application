USE `mtm_waitlist`;

INSERT INTO `waitlist_active` (
  `WorkCenter`, `RequestType`, `Request`, `RequestPriority`, `MHandler`,
  `TimeRemaining`, `RequestTime`, `StartTime`, `AllocatedTime`, `PartID`, `Operation`
)
VALUES
  (
    'WC-01', 'Pickup Parts', 'Pickup Finished Goods. Part Number: PART-1001', 'Normal', '',
    DATE_ADD(NOW(), INTERVAL 30 MINUTE), NOW(), NULL, '00:20:00', 'PART-1001', 'OP-10'
  );
