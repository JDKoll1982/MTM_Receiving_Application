DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_parts_search$$

CREATE PROCEDURE sp_dunnage_parts_search(
    IN p_search_text VARCHAR(100),
    IN p_type_id INT
)
BEGIN
    -- Search by part_id or within JSON spec values
    -- If p_type_id is NULL or 0, search all types

    SELECT
        p.id,
        p.part_id,
        p.type_id,
        t.type_name,
        p.spec_values,
        p.created_by,
        p.created_date,
        p.modified_by,
        p.modified_date
    FROM dunnage_parts p
    JOIN dunnage_types t ON p.type_id = t.id
    WHERE (p_type_id IS NULL OR p_type_id = 0 OR p.type_id = p_type_id)
    AND (
        p.part_id LIKE CONCAT('%', p_search_text, '%')
        OR JSON_SEARCH(p.spec_values, 'one', CONCAT('%', p_search_text, '%')) IS NOT NULL
    )
    ORDER BY p.part_id
    LIMIT 100; -- Limit results for performance
END$$

DELIMITER ;
