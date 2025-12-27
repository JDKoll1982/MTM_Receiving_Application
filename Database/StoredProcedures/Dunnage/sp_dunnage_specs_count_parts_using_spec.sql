DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_specs_count_parts_using_spec$$

CREATE PROCEDURE sp_dunnage_specs_count_parts_using_spec(
    IN p_type_id INT,
    IN p_spec_key VARCHAR(100)
)
BEGIN
    -- This checks if the spec key exists in the JSON spec_values of parts
    -- Using JSON_CONTAINS_PATH or JSON_EXTRACT to check existence
    SELECT COUNT(*) as part_count
    FROM dunnage_parts
    WHERE type_id = p_type_id
    AND JSON_CONTAINS_PATH(spec_values, 'one', CONCAT('$.', p_spec_key)) = 1;
END$$

DELIMITER ;
