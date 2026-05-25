-- ============================================================
-- Procedure: sp_CustomerPullPack_Waitlist_GetQueue
-- Purpose:   Returns Customer Pull n' Pack waitlist queue rows with
--            optional filters for status, location, customer, requester,
--            owner, or one specific waitlist item.
--            Implements Workflow 3.1 and supports Workflow 3.2 visibility.
-- ============================================================

DROP PROCEDURE IF EXISTS `sp_CustomerPullPack_Waitlist_GetQueue`;

DELIMITER $$
CREATE PROCEDURE `sp_CustomerPullPack_Waitlist_GetQueue`(
    IN p_waitlist_id CHAR(36),
    IN p_customer_id VARCHAR(15),
    IN p_requester_user_id VARCHAR(100),
    IN p_current_owner_user_id VARCHAR(100),
    IN p_location_id VARCHAR(30),
    IN p_status_set_json TEXT,
    IN p_use_default_open_work TINYINT(1),
    IN p_max_results INT
)
BEGIN
    DECLARE v_status_filter_json TEXT DEFAULT NULL;
    DECLARE v_use_default_open_work TINYINT(1) DEFAULT 1;
    DECLARE v_max_results INT DEFAULT 250;

    SET v_status_filter_json = NULLIF(TRIM(COALESCE(p_status_set_json, '')), '');
    SET v_use_default_open_work = COALESCE(p_use_default_open_work, 1);
    SET v_max_results = CASE
        WHEN COALESCE(p_max_results, 0) <= 0 THEN 250
        ELSE p_max_results
    END;

    SELECT
        w.waitlist_id                                                       AS WaitlistId,
        w.source_line_key                                                   AS SourceLineKey,
        w.customer_id                                                       AS CustomerId,
        w.customer_name                                                     AS CustomerName,
        w.customer_order_id                                                 AS CustomerOrderId,
        w.parent_part_id                                                    AS ParentPartId,
        w.requested_quantity                                                AS RequestedQuantity,
        COALESCE(loc.SelectedLocationsPipe, '')                             AS SelectedLocations,
        w.requested_by_user_id                                              AS RequestedByUserId,
        w.requested_by_display_name                                         AS RequestedByDisplayName,
        COALESCE(w.requester_context_note, '')                              AS RequesterContextNote,
        w.current_status                                                    AS CurrentStatus,
        COALESCE(w.current_owner_user_id, '')                               AS CurrentOwnerUserId,
        COALESCE(w.current_owner_display_name, '')                          AS CurrentOwnerDisplayName,
        w.location_review_flag                                              AS LocationReviewFlag,
        w.problem_reason                                                    AS ProblemReason,
        COALESCE(w.handler_note, '')                                        AS HandlerNote,
        COALESCE(w.completion_user_id, '')                                  AS CompletionUserId,
        w.completion_timestamp                                              AS CompletionTimestamp,
        w.last_updated_by_user_id                                           AS LastUpdatedByUserId,
        w.last_updated_timestamp                                            AS LastUpdatedTimestamp,
        w.request_timestamp                                                 AS RequestTimestamp,
        w.recheck_indicator                                                 AS RecheckIndicator,
        COALESCE(loc.SelectedLocationCount, 0)                              AS SelectedLocationCount,
        COALESCE(loc.PrimarySelectedLocationId, '')                         AS PrimarySelectedLocationId
    FROM customer_pull_pack_waitlist w
    LEFT JOIN
    (
        SELECT
            wl.waitlist_id                                                  AS WaitlistId,
            GROUP_CONCAT(wl.location_id ORDER BY wl.selection_order SEPARATOR '|')
                                                                        AS SelectedLocationsPipe,
            COUNT(*)                                                        AS SelectedLocationCount,
            MIN(wl.location_id)                                             AS PrimarySelectedLocationId
        FROM customer_pull_pack_waitlist_location wl
        GROUP BY wl.waitlist_id
    ) loc
        ON loc.WaitlistId = w.waitlist_id
    WHERE (p_waitlist_id IS NULL OR CHAR_LENGTH(TRIM(p_waitlist_id)) = 0 OR w.waitlist_id = TRIM(p_waitlist_id))
      AND (p_customer_id IS NULL OR CHAR_LENGTH(TRIM(p_customer_id)) = 0 OR w.customer_id = UPPER(TRIM(p_customer_id)))
      AND (p_requester_user_id IS NULL OR CHAR_LENGTH(TRIM(p_requester_user_id)) = 0 OR w.requested_by_user_id = TRIM(p_requester_user_id))
      AND (p_current_owner_user_id IS NULL OR CHAR_LENGTH(TRIM(p_current_owner_user_id)) = 0 OR COALESCE(w.current_owner_user_id, '') = TRIM(p_current_owner_user_id))
      AND (
            p_location_id IS NULL
            OR CHAR_LENGTH(TRIM(p_location_id)) = 0
            OR EXISTS
            (
                SELECT 1
                FROM customer_pull_pack_waitlist_location location_filter
                WHERE location_filter.waitlist_id = w.waitlist_id
                  AND location_filter.location_id = UPPER(TRIM(p_location_id))
            )
          )
      AND (
            (
                v_status_filter_json IS NOT NULL
                AND JSON_VALID(v_status_filter_json) = 1
                AND JSON_CONTAINS(v_status_filter_json, JSON_QUOTE(w.current_status), '$')
            )
            OR (
                v_status_filter_json IS NULL
                AND v_use_default_open_work = 1
                AND w.current_status IN ('Requested', 'Accepted', 'Problem')
            )
            OR (
                v_status_filter_json IS NULL
                AND v_use_default_open_work = 0
            )
          )
    ORDER BY
        CASE w.current_status
            WHEN 'Requested' THEN 0
            WHEN 'Accepted' THEN 1
            WHEN 'Problem' THEN 2
            WHEN 'Completed' THEN 3
            WHEN 'Cancelled' THEN 4
            ELSE 5
        END,
        w.request_timestamp,
        w.customer_id,
        w.customer_order_id,
        w.parent_part_id
    LIMIT v_max_results;
END $$
DELIMITER ;