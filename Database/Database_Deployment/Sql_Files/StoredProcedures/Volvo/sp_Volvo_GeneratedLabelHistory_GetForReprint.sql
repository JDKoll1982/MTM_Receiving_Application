-- Stored Procedure: sp_Volvo_GeneratedLabelHistory_GetForReprint
-- Description: Retrieves volvo generated-label history rows for the Reprint Labels page
--              together with an `already_queued` flag (1 when an is_reprint = 1 row already
--              exists in the active volvo_generated_label_data queue for the same original_id).
--              Supports the date range and the Search By / text filters used by the Reprint page.
--              All parameters are optional; pass NULL / '' to skip a filter.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Volvo_GeneratedLabelHistory_GetForReprint`;

DELIMITER $$

CREATE PROCEDURE `sp_Volvo_GeneratedLabelHistory_GetForReprint`(
    IN p_start_date  DATE,
    IN p_end_date    DATE,
    IN p_search_by   VARCHAR(50),
    IN p_search_text VARCHAR(255)
)
BEGIN
    SELECT
        vgh.id                  AS history_id,
        vgh.shipment_date       AS record_date,
        vgh.part_number         AS part_number,
        vgh.part_description    AS part_description,
        vgh.quantity            AS quantity,
        vgh.shipment_number     AS shipment_number,
        vgh.skid_number         AS skid_number,
        vgh.total_skids         AS total_skids,
        vgh.original_id         AS original_id,
        CASE WHEN vgld.id IS NULL THEN 0 ELSE 1 END AS already_queued
    FROM volvo_generated_label_history vgh
    LEFT JOIN volvo_generated_label_data vgld
        ON vgld.id = vgh.original_id
       AND vgld.is_reprint = 1
    WHERE
        (p_start_date IS NULL OR vgh.shipment_date >= p_start_date)
        AND (p_end_date   IS NULL OR vgh.shipment_date <= p_end_date)
        AND (
            p_search_text IS NULL
            OR p_search_text = ''
            OR (p_search_by = 'part'        AND vgh.part_number LIKE CONCAT('%', p_search_text, '%'))
            OR (p_search_by = 'description' AND vgh.part_description LIKE CONCAT('%', p_search_text, '%'))
            OR (p_search_by = 'quantity'    AND CAST(vgh.quantity AS CHAR) LIKE CONCAT('%', p_search_text, '%'))
        )
    ORDER BY vgh.shipment_date DESC, vgh.id DESC;
END $$
