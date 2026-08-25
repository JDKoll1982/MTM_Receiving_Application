-- Stored Procedure: sp_Receiving_History_GetForReprint
-- Description: Retrieves receiving history rows for the Reprint Labels page together with an
--              `already_queued` flag (1 when an is_reprint = 1 row already exists in the
--              active receiving_label_data queue for the same history record). Supports the
--              date range and the Search By / text filters used by the Reprint page.
--              All parameters are optional; pass NULL / '' to skip a filter.

USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Receiving_History_GetForReprint`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_History_GetForReprint`(
    IN p_start_date  DATE,
    IN p_end_date    DATE,
    IN p_search_by   VARCHAR(50),
    IN p_search_text VARCHAR(255)
)
BEGIN
    SELECT
        rh.id                                              AS history_id,
        COALESCE(rh.transaction_date, DATE(rh.received_date)) AS record_date,
        rh.part_id                                         AS part_id,
        rh.part_description                                AS part_description,
        rh.po_number                                       AS po_number,
        COALESCE(rh.vendor_name, rh.po_vendor)             AS vendor_name,
        rh.heat                                            AS heat,
        rh.load_number                                     AS load_number,
        rh.label_number                                    AS label_number,
        rh.quantity                                        AS quantity,
        COALESCE(rh.load_id, rh.load_guid)                 AS load_id,
        CASE WHEN rld.id IS NULL THEN 0 ELSE 1 END         AS already_queued
    FROM receiving_history rh
    LEFT JOIN receiving_label_data rld
        ON rld.load_id = COALESCE(rh.load_id, rh.load_guid)
       AND rld.is_reprint = 1
    WHERE
        (p_start_date IS NULL OR COALESCE(rh.transaction_date, DATE(rh.received_date)) >= p_start_date)
        AND (p_end_date   IS NULL OR COALESCE(rh.transaction_date, DATE(rh.received_date)) <= p_end_date)
        AND (
            p_search_text IS NULL
            OR p_search_text = ''
            OR (p_search_by = 'part'        AND rh.part_id LIKE CONCAT('%', p_search_text, '%'))
            OR (p_search_by = 'po'          AND rh.po_number LIKE CONCAT('%', p_search_text, '%'))
            OR (p_search_by = 'description' AND rh.part_description LIKE CONCAT('%', p_search_text, '%'))
            OR (p_search_by = 'vendor'      AND COALESCE(rh.vendor_name, rh.po_vendor) LIKE CONCAT('%', p_search_text, '%'))
            OR (p_search_by = 'heat'        AND rh.heat LIKE CONCAT('%', p_search_text, '%'))
        )
    ORDER BY COALESCE(rh.transaction_date, DATE(rh.received_date)) DESC, rh.id DESC;
END $$
