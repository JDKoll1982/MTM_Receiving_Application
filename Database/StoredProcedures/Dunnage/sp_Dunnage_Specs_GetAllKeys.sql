/*
sp_Dunnage_Specs_GetAllKeys
------------------------------------------------------------
Description:
    Retrieves a sorted list of distinct JSON object keys found in
    the `spec_value` (JSON) column of the `dunnage_specs` table.

    Temporarily materializes keys into a temp table using a UNION-ALL
    numbers generator, filters out NULL/empty keys, orders them, and
    returns the result set. The numbers generator covers positions 1..100
    (raised from the original 50). Requires MySQL 5.7+ (no JSON_TABLE).

Usage:
    CALL sp_Dunnage_Specs_GetAllKeys();

Parameters:
    None.

Result set:
    - SpecKey (VARCHAR) : distinct JSON object key names present
        across all non-NULL JSON objects in dunnage_specs.spec_value,
        ordered alphabetically.

Notes:
    - The sequence ceiling is 100. Spec objects with more than 100
      top-level keys will have the surplus keys silently omitted.
      In practice, dunnage spec objects are expected to have far fewer
      keys than this limit.
    - Only top-level object keys are extracted; nested keys are not.
    - MySQL 8.0+ users may replace this with a JSON_TABLE approach
      to remove the ceiling entirely.
*/
DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_GetAllKeys` $$

CREATE PROCEDURE `sp_Dunnage_Specs_GetAllKeys`()
BEGIN
    CREATE TEMPORARY TABLE IF NOT EXISTS temp_spec_keys (
        SpecKey VARCHAR(255)
    );

    TRUNCATE TABLE temp_spec_keys;

    INSERT INTO temp_spec_keys (SpecKey)
    SELECT DISTINCT
        JSON_UNQUOTE(JSON_EXTRACT(JSON_KEYS(ds.spec_value), CONCAT('$[', nums.n - 1, ']'))) AS SpecKey
    FROM dunnage_specs ds
    JOIN (
        SELECT  1 n UNION ALL SELECT  2 UNION ALL SELECT  3 UNION ALL SELECT  4 UNION ALL SELECT  5
        UNION ALL SELECT  6 UNION ALL SELECT  7 UNION ALL SELECT  8 UNION ALL SELECT  9 UNION ALL SELECT 10
        UNION ALL SELECT 11 UNION ALL SELECT 12 UNION ALL SELECT 13 UNION ALL SELECT 14 UNION ALL SELECT 15
        UNION ALL SELECT 16 UNION ALL SELECT 17 UNION ALL SELECT 18 UNION ALL SELECT 19 UNION ALL SELECT 20
        UNION ALL SELECT 21 UNION ALL SELECT 22 UNION ALL SELECT 23 UNION ALL SELECT 24 UNION ALL SELECT 25
        UNION ALL SELECT 26 UNION ALL SELECT 27 UNION ALL SELECT 28 UNION ALL SELECT 29 UNION ALL SELECT 30
        UNION ALL SELECT 31 UNION ALL SELECT 32 UNION ALL SELECT 33 UNION ALL SELECT 34 UNION ALL SELECT 35
        UNION ALL SELECT 36 UNION ALL SELECT 37 UNION ALL SELECT 38 UNION ALL SELECT 39 UNION ALL SELECT 40
        UNION ALL SELECT 41 UNION ALL SELECT 42 UNION ALL SELECT 43 UNION ALL SELECT 44 UNION ALL SELECT 45
        UNION ALL SELECT 46 UNION ALL SELECT 47 UNION ALL SELECT 48 UNION ALL SELECT 49 UNION ALL SELECT 50
        UNION ALL SELECT 51 UNION ALL SELECT 52 UNION ALL SELECT 53 UNION ALL SELECT 54 UNION ALL SELECT 55
        UNION ALL SELECT 56 UNION ALL SELECT 57 UNION ALL SELECT 58 UNION ALL SELECT 59 UNION ALL SELECT 60
        UNION ALL SELECT 61 UNION ALL SELECT 62 UNION ALL SELECT 63 UNION ALL SELECT 64 UNION ALL SELECT 65
        UNION ALL SELECT 66 UNION ALL SELECT 67 UNION ALL SELECT 68 UNION ALL SELECT 69 UNION ALL SELECT 70
        UNION ALL SELECT 71 UNION ALL SELECT 72 UNION ALL SELECT 73 UNION ALL SELECT 74 UNION ALL SELECT 75
        UNION ALL SELECT 76 UNION ALL SELECT 77 UNION ALL SELECT 78 UNION ALL SELECT 79 UNION ALL SELECT 80
        UNION ALL SELECT 81 UNION ALL SELECT 82 UNION ALL SELECT 83 UNION ALL SELECT 84 UNION ALL SELECT 85
        UNION ALL SELECT 86 UNION ALL SELECT 87 UNION ALL SELECT 88 UNION ALL SELECT 89 UNION ALL SELECT 90
        UNION ALL SELECT 91 UNION ALL SELECT 92 UNION ALL SELECT 93 UNION ALL SELECT 94 UNION ALL SELECT 95
        UNION ALL SELECT 96 UNION ALL SELECT 97 UNION ALL SELECT 98 UNION ALL SELECT 99 UNION ALL SELECT 100
    ) AS nums
    WHERE ds.spec_value IS NOT NULL
      AND JSON_TYPE(ds.spec_value) = 'OBJECT'
      AND nums.n <= JSON_LENGTH(JSON_KEYS(ds.spec_value));

    SELECT DISTINCT SpecKey
    FROM temp_spec_keys
    WHERE SpecKey IS NOT NULL AND SpecKey != ''
    ORDER BY SpecKey;

    DROP TEMPORARY TABLE IF EXISTS temp_spec_keys;
END $$

DELIMITER ;
