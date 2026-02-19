USE `mtm_waitlist`;

INSERT INTO `waitlist_jobdunnage` (
  `PartID`, `Operation`, `Dunnage`, `Skid`, `Cardboard`, `Boxes`,
  `Other1`, `Other2`, `Other3`, `Other4`, `Other5`, `PartType`
)
VALUES
  (
    'PART-1001', 'OP-10', 'Dunnage - Small', 'Skid - 48 x 40', 'Cardboard - 24 x 36', 'Box - 12 x 12 x 12',
    'Other - Stretch Wrap', 'Other - Corner Guard', NULL, NULL, NULL, 'Finished Goods'
  );
