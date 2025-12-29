DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_dunnage_specs_get_all_keys` $$

CREATE PROCEDURE `sp_dunnage_specs_get_all_keys`()
BEGIN
    -- Get union of all unique spec keys across all types (for dynamic CSV columns)
    -- MySQL 5.7.24 compatible approach (no JSON_TABLE)
    
    -- Create temporary table to hold all keys
    CREATE TEMPORARY TABLE IF NOT EXISTS temp_spec_keys (
        SpecKey VARCHAR(100)
    );

    -- Clear temp table
    TRUNCATE TABLE temp_spec_keys;

    -- Extract keys from DunnageSpecs JSON
    -- Since MySQL 5.7.24 doesn't have JSON_TABLE, we use a workaround
    -- Parse JSON manually by iterating through records
    INSERT INTO temp_spec_keys (SpecKey)
    SELECT DISTINCT 
        TRIM(BOTH '"' FROM SUBSTRING_INDEX(SUBSTRING_INDEX(keyval, ':', 1), '"', -1)) AS SpecKey
    FROM (
        SELECT 
            SUBSTRING_INDEX(SUBSTRING_INDEX(
                REPLACE(REPLACE(DunnageSpecs, '{', ''), '}', ''), 
                ',', 
                numbers.n
            ), ',', -1) AS keyval
        FROM dunnage_specs
        CROSS JOIN (
            SELECT 1 n UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5
            UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9 UNION ALL SELECT 10
        ) numbers
        WHERE DunnageSpecs IS NOT NULL 
          AND DunnageSpecs != ''
          AND CHAR_LENGTH(DunnageSpecs) - CHAR_LENGTH(REPLACE(DunnageSpecs, ',', '')) >= numbers.n - 1
    ) AS extracted
    WHERE keyval LIKE '%:%';

    -- Return distinct keys ordered alphabetically
    SELECT DISTINCT SpecKey
    FROM temp_spec_keys
    WHERE SpecKey IS NOT NULL 
      AND SpecKey != ''
    ORDER BY SpecKey;

    -- Cleanup
    DROP TEMPORARY TABLE IF EXISTS temp_spec_keys;
END $$

DELIMITER ;
