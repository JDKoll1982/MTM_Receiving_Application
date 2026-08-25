DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Parts_Search`$$

CREATE PROCEDURE `sp_Dunnage_Parts_Search`(
    IN p_search_text VARCHAR(100),
    IN p_type_id INT
)
BEGIN
    -- Search by part_id or within any of the udc1..udc10 values.
    -- All comparisons are lower-cased so searches are case-insensitive.
    -- If p_type_id is NULL or 0, search all types.

    SELECT
        p.id,
        p.part_id,
        p.type_id,
        t.type_name,
        p.udc1,
        p.udc2,
        p.udc3,
        p.udc4,
        p.udc5,
        p.udc6,
        p.udc7,
        p.udc8,
        p.udc9,
        p.udc10,
        p.image_path,
        t.image_path AS type_image_path,
        p.home_location,
        p.created_by,
        p.created_date,
        p.modified_by,
        p.modified_date
    FROM dunnage_parts p
    JOIN dunnage_types t ON p.type_id = t.id
    WHERE (p_type_id IS NULL OR p_type_id = 0 OR p.type_id = p_type_id)
    AND (
        LOWER(p.part_id) LIKE LOWER(CONCAT('%', p_search_text, '%'))
        OR LOWER(COALESCE(p.udc1, '')) LIKE LOWER(CONCAT('%', p_search_text, '%'))
        OR LOWER(COALESCE(p.udc2, '')) LIKE LOWER(CONCAT('%', p_search_text, '%'))
        OR LOWER(COALESCE(p.udc3, '')) LIKE LOWER(CONCAT('%', p_search_text, '%'))
        OR LOWER(COALESCE(p.udc4, '')) LIKE LOWER(CONCAT('%', p_search_text, '%'))
        OR LOWER(COALESCE(p.udc5, '')) LIKE LOWER(CONCAT('%', p_search_text, '%'))
        OR LOWER(COALESCE(p.udc6, '')) LIKE LOWER(CONCAT('%', p_search_text, '%'))
        OR LOWER(COALESCE(p.udc7, '')) LIKE LOWER(CONCAT('%', p_search_text, '%'))
        OR LOWER(COALESCE(p.udc8, '')) LIKE LOWER(CONCAT('%', p_search_text, '%'))
        OR LOWER(COALESCE(p.udc9, '')) LIKE LOWER(CONCAT('%', p_search_text, '%'))
        OR LOWER(COALESCE(p.udc10, '')) LIKE LOWER(CONCAT('%', p_search_text, '%'))
    )
    ORDER BY p.part_id
    LIMIT 100;
END $$

DELIMITER ;
