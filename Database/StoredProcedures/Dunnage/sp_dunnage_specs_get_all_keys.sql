/*
sp_dunnage_specs_get_all_keys
------------------------------------------------------------
Description:
    Retrieves a sorted list of distinct JSON object keys found in
    the `spec_value` (JSON) column of the `dunnage_specs` table.
    The procedure temporarily materializes keys into a temp table,
    filters out NULL/empty keys, orders them, and returns the result
    set. Temporary resources are cleaned up before completion.

Usage:
    CALL sp_dunnage_specs_get_all_keys();

Parameters:
    None.

Result set:
    - SpecKey (VARCHAR) : distinct JSON object key names present
        across all non-NULL JSON objects in dunnage_specs.spec_value,
        ordered alphabetically.

Side effects and behavior:
    - Creates (if not exists) and truncates a temporary table
        `temp_spec_keys` to collect keys, then drops it at the end.
    - Uses a numeric sequence (1..50) to enumerate JSON key array
        positions; therefore it only extracts up to 50 keys per JSON
        object. Increase the sequence if objects can contain more keys.
    - Filters rows where spec_value IS NULL or JSON_TYPE(...) != 'OBJECT'.
    - Requires MySQL JSON functions (MySQL 5.7+).

Permissions required:
    - SELECT on `dunnage_specs`.
    - CREATE TEMPORARY TABLE and DROP privileges in the session.

Notes and considerations:
    - If JSON objects may contain more than the current sequence limit,
        extend the sequence generator to cover the maximum expected keys.
    - Depending on data volume, consider a more efficient key-unrolling
        approach (e.g., a numbers table) to avoid long inline UNION_ALL lists.
    - This procedure returns only top-level object keys; nested keys are
        not extracted.
*/
DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_dunnage_specs_get_all_keys` $$

CREATE PROCEDURE `sp_dunnage_specs_get_all_keys`()
BEGIN
    -- Collect distinct JSON object keys from spec_value (JSON) column of dunnage_specs
    CREATE TEMPORARY TABLE IF NOT EXISTS temp_spec_keys (
        SpecKey VARCHAR(100)
    );

    TRUNCATE TABLE temp_spec_keys;

    INSERT INTO temp_spec_keys (SpecKey)
    SELECT DISTINCT
        JSON_UNQUOTE(JSON_EXTRACT(JSON_KEYS(ds.spec_value), CONCAT('$[', nums.n - 1, ']'))) AS SpecKey
    FROM dunnage_specs ds
    JOIN (
        SELECT 1 n UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5
        UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9 UNION ALL SELECT 10
        UNION ALL SELECT 11 UNION ALL SELECT 12 UNION ALL SELECT 13 UNION ALL SELECT 14 UNION ALL SELECT 15
        UNION ALL SELECT 16 UNION ALL SELECT 17 UNION ALL SELECT 18 UNION ALL SELECT 19 UNION ALL SELECT 20
        UNION ALL SELECT 21 UNION ALL SELECT 22 UNION ALL SELECT 23 UNION ALL SELECT 24 UNION ALL SELECT 25
        UNION ALL SELECT 26 UNION ALL SELECT 27 UNION ALL SELECT 28 UNION ALL SELECT 29 UNION ALL SELECT 30
        UNION ALL SELECT 31 UNION ALL SELECT 32 UNION ALL SELECT 33 UNION ALL SELECT 34 UNION ALL SELECT 35
        UNION ALL SELECT 36 UNION ALL SELECT 37 UNION ALL SELECT 38 UNION ALL SELECT 39 UNION ALL SELECT 40
        UNION ALL SELECT 41 UNION ALL SELECT 42 UNION ALL SELECT 43 UNION ALL SELECT 44 UNION ALL SELECT 45
        UNION ALL SELECT 46 UNION ALL SELECT 47 UNION ALL SELECT 48 UNION ALL SELECT 49 UNION ALL SELECT 50
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
