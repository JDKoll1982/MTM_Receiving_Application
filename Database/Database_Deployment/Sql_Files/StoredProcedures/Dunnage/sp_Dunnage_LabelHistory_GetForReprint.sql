-- Stored Procedure: sp_Dunnage_LabelHistory_GetForReprint
-- Description: Retrieves dunnage history rows for the Reprint Labels page together with an
--              `already_queued` flag (1 when an is_reprint = 1 row already exists in the
--              active dunnage_label_data queue for the same load_uuid). Supports the date
--              range and the Search By / text filters used by the Reprint page.
--              All parameters are optional; pass NULL / '' to skip a filter.

USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Dunnage_LabelHistory_GetForReprint`;

DELIMITER $$

CREATE PROCEDURE `sp_Dunnage_LabelHistory_GetForReprint`(
    IN p_start_date  DATE,
    IN p_end_date    DATE,
    IN p_search_by   VARCHAR(50),
    IN p_search_text VARCHAR(255)
)
BEGIN
    SELECT
        dh.load_uuid                                    AS load_uuid,
        DATE(COALESCE(dh.received_date, dh.created_date)) AS record_date,
        dh.part_id                                      AS part_id,
        COALESCE(NULLIF(dh.dunnage_type_name, ''), dh.type_name) AS type_name,
        dh.location                                     AS location,
        dh.po_number                                    AS po_number,
        dh.label_number                                 AS label_number,
        dh.quantity                                     AS quantity,
        CASE WHEN dld.id IS NULL THEN 0 ELSE 1 END      AS already_queued
    FROM dunnage_history dh
    LEFT JOIN dunnage_label_data dld
        ON dld.load_uuid = dh.load_uuid
       AND dld.is_reprint = 1
    WHERE
        (p_start_date IS NULL OR DATE(COALESCE(dh.received_date, dh.created_date)) >= p_start_date)
        AND (p_end_date   IS NULL OR DATE(COALESCE(dh.received_date, dh.created_date)) <= p_end_date)
        AND (
            p_search_text IS NULL
            OR p_search_text = ''
            OR (p_search_by = 'part'     AND dh.part_id LIKE CONCAT('%', p_search_text, '%'))
            OR (p_search_by = 'po'       AND dh.po_number LIKE CONCAT('%', p_search_text, '%'))
            OR (p_search_by = 'type'     AND COALESCE(NULLIF(dh.dunnage_type_name, ''), dh.type_name) LIKE CONCAT('%', p_search_text, '%'))
            OR (p_search_by = 'quantity' AND CAST(dh.quantity AS CHAR) LIKE CONCAT('%', p_search_text, '%'))
        )
    ORDER BY DATE(COALESCE(dh.received_date, dh.created_date)) DESC, dh.load_uuid DESC;
END $$
