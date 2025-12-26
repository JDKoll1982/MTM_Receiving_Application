-- T027: Test CASCADE delete
INSERT INTO dunnage_types (DunnageType, EntryDate, EntryUser) VALUES ('TEST_CASCADE', NOW(), 'TEST');
SET @test_id = LAST_INSERT_ID();
INSERT INTO dunnage_specs (DunnageTypeID, DunnageSpecs, SpecAlterDate, SpecAlterUser) 
VALUES (@test_id, '{"test": true}', NOW(), 'TEST');
DELETE FROM dunnage_types WHERE ID = @test_id;
SELECT COUNT(*) AS remaining_specs_should_be_0 FROM dunnage_specs WHERE DunnageTypeID = @test_id;
