-- ============================================================
-- Procedure: sp_CustomerPullPack_Waitlist_Upsert
-- Purpose:   Creates or updates one MTM-managed Customer Pull n' Pack
--            waitlist entry and replaces its selected location set.
--            Implements Workflow 2.1, 2.2, and 2.3.
-- ============================================================

DROP PROCEDURE IF EXISTS `sp_CustomerPullPack_Waitlist_Upsert`;

DELIMITER $$
CREATE PROCEDURE `sp_CustomerPullPack_Waitlist_Upsert`(
    INOUT p_waitlist_id CHAR(36),
    IN p_source_line_key VARCHAR(150),
    IN p_customer_id VARCHAR(15),
    IN p_customer_name VARCHAR(255),
    IN p_customer_order_id VARCHAR(15),
    IN p_parent_part_id VARCHAR(30),
    IN p_requested_quantity DECIMAL(20, 8),
    IN p_requested_by_user_id VARCHAR(100),
    IN p_requested_by_display_name VARCHAR(255),
    IN p_requester_context_note TEXT,
    IN p_current_status VARCHAR(20),
    IN p_current_owner_user_id VARCHAR(100),
    IN p_current_owner_display_name VARCHAR(255),
    IN p_location_review_flag TINYINT(1),
    IN p_problem_reason VARCHAR(30),
    IN p_handler_note TEXT,
    IN p_completion_user_id VARCHAR(100),
    IN p_completion_timestamp DATETIME,
    IN p_last_updated_by_user_id VARCHAR(100),
    IN p_recheck_indicator TINYINT(1),
    IN p_selected_locations_json TEXT,
    OUT p_duplicate_existing_waitlist_id CHAR(36),
    OUT p_status INT,
    OUT p_error_message VARCHAR(1000)
)
BEGIN
    DECLARE v_existing_waitlist_id CHAR(36) DEFAULT NULL;
    DECLARE v_selected_location_count INT DEFAULT 0;
    DECLARE v_selected_location_index INT DEFAULT 0;
    DECLARE v_selected_location_id VARCHAR(30) DEFAULT NULL;
    DECLARE v_current_status VARCHAR(20) DEFAULT 'Requested';
    DECLARE v_problem_reason VARCHAR(30) DEFAULT 'None';
    DECLARE v_effective_location_review_flag TINYINT(1) DEFAULT 0;
    DECLARE v_effective_completion_timestamp DATETIME DEFAULT NULL;
    DECLARE v_effective_completion_user_id VARCHAR(100) DEFAULT NULL;
    DECLARE v_is_create TINYINT(1) DEFAULT 0;
    DECLARE v_waitlist_exists INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_status = 1;
        IF p_error_message IS NULL THEN
            SET p_error_message = 'Customer Pull n'' Pack waitlist save failed. Transaction rolled back.';
        END IF;
    END;

    SET p_status = 0;
    SET p_error_message = NULL;
    SET p_duplicate_existing_waitlist_id = NULL;

    IF p_selected_locations_json IS NULL OR CHAR_LENGTH(TRIM(p_selected_locations_json)) = 0 THEN
        SET p_selected_locations_json = '[]';
    END IF;

    IF CHAR_LENGTH(TRIM(COALESCE(p_source_line_key, ''))) = 0 THEN
        SET p_status = 1;
        SET p_error_message = 'Source report line key is required.';
    ELSEIF CHAR_LENGTH(TRIM(COALESCE(p_customer_id, ''))) = 0 THEN
        SET p_status = 1;
        SET p_error_message = 'Customer ID is required.';
    ELSEIF CHAR_LENGTH(TRIM(COALESCE(p_customer_order_id, ''))) = 0 THEN
        SET p_status = 1;
        SET p_error_message = 'Customer order ID is required.';
    ELSEIF CHAR_LENGTH(TRIM(COALESCE(p_parent_part_id, ''))) = 0 THEN
        SET p_status = 1;
        SET p_error_message = 'Parent part ID is required.';
    ELSEIF COALESCE(p_requested_quantity, 0) <= 0 THEN
        SET p_status = 1;
        SET p_error_message = 'Requested quantity must be greater than zero.';
    ELSEIF CHAR_LENGTH(TRIM(COALESCE(p_requested_by_user_id, ''))) = 0 THEN
        SET p_status = 1;
        SET p_error_message = 'Requested-by user ID is required.';
    ELSEIF CHAR_LENGTH(TRIM(COALESCE(p_last_updated_by_user_id, ''))) = 0 THEN
        SET p_status = 1;
        SET p_error_message = 'Last-updated-by user ID is required.';
    ELSEIF JSON_VALID(p_selected_locations_json) = 0 THEN
        SET p_status = 1;
        SET p_error_message = 'Selected locations must be a valid JSON array.';
    END IF;

    IF p_status = 0 THEN
        CASE UPPER(TRIM(COALESCE(p_current_status, 'REQUESTED')))
            WHEN 'REQUESTED' THEN SET v_current_status = 'Requested';
            WHEN 'ACCEPTED' THEN SET v_current_status = 'Accepted';
            WHEN 'COMPLETED' THEN SET v_current_status = 'Completed';
            WHEN 'CANCELLED' THEN SET v_current_status = 'Cancelled';
            WHEN 'PROBLEM' THEN SET v_current_status = 'Problem';
            ELSE
                SET p_status = 1;
                SET p_error_message = 'Current status is invalid.';
        END CASE;
    END IF;

    IF p_status = 0 THEN
        CASE UPPER(TRIM(COALESCE(p_problem_reason, 'NONE')))
            WHEN 'NONE' THEN SET v_problem_reason = 'None';
            WHEN 'NOTINLOCATION' THEN SET v_problem_reason = 'NotInLocation';
            WHEN 'INCORRECTQTY' THEN SET v_problem_reason = 'IncorrectQty';
            WHEN 'INCORRECTPARTNUMBER' THEN SET v_problem_reason = 'IncorrectPartNumber';
            WHEN 'OTHER' THEN SET v_problem_reason = 'Other';
            ELSE
                SET p_status = 1;
                SET p_error_message = 'Problem reason is invalid.';
        END CASE;
    END IF;

    IF p_status = 0 THEN
        SET v_selected_location_count = COALESCE(JSON_LENGTH(p_selected_locations_json), 0);
        SET v_effective_location_review_flag = CASE
            WHEN v_selected_location_count = 0 THEN 1
            ELSE COALESCE(p_location_review_flag, 0)
        END;

        IF v_selected_location_count = 0
           AND CHAR_LENGTH(TRIM(COALESCE(p_requester_context_note, ''))) = 0 THEN
            SET p_status = 1;
            SET p_error_message = 'A requester note is required when no locations are selected.';
        ELSEIF v_current_status = 'Problem'
           AND v_problem_reason = 'None'
           AND CHAR_LENGTH(TRIM(COALESCE(p_handler_note, ''))) = 0 THEN
            SET p_status = 1;
            SET p_error_message = 'Problem status requires a preset reason or a handler note.';
        END IF;
    END IF;

    IF p_status = 0 THEN
        IF v_current_status IN ('Completed', 'Cancelled') THEN
            SET v_effective_completion_timestamp = COALESCE(p_completion_timestamp, CURRENT_TIMESTAMP);
            SET v_effective_completion_user_id = NULLIF(TRIM(COALESCE(p_completion_user_id, '')), '');
            IF v_effective_completion_user_id IS NULL THEN
                SET v_effective_completion_user_id = TRIM(p_last_updated_by_user_id);
            END IF;
        ELSE
            SET v_effective_completion_timestamp = NULL;
            SET v_effective_completion_user_id = NULL;
        END IF;

        IF p_waitlist_id IS NULL OR CHAR_LENGTH(TRIM(p_waitlist_id)) = 0 THEN
            SET v_is_create = 1;

            SELECT w.waitlist_id
            INTO v_existing_waitlist_id
            FROM customer_pull_pack_waitlist w
            WHERE w.source_line_key = TRIM(p_source_line_key)
              AND w.current_status IN ('Requested', 'Accepted', 'Problem')
            ORDER BY w.request_timestamp DESC
            LIMIT 1;

            IF v_existing_waitlist_id IS NOT NULL THEN
                SET p_duplicate_existing_waitlist_id = v_existing_waitlist_id;
                SET p_status = 2;
                SET p_error_message = 'An open waitlist item already exists for this report line.';
            ELSE
                SET p_waitlist_id = UUID();
            END IF;
        ELSE
            SET p_waitlist_id = TRIM(p_waitlist_id);

            SELECT COUNT(*)
            INTO v_waitlist_exists
            FROM customer_pull_pack_waitlist w
            WHERE w.waitlist_id = p_waitlist_id;

            IF v_waitlist_exists = 0 THEN
                SET p_status = 1;
                SET p_error_message = 'The waitlist item to update was not found.';
            ELSE
                SELECT w.waitlist_id
                INTO v_existing_waitlist_id
                FROM customer_pull_pack_waitlist w
                WHERE w.source_line_key = TRIM(p_source_line_key)
                  AND w.current_status IN ('Requested', 'Accepted', 'Problem')
                  AND w.waitlist_id <> p_waitlist_id
                ORDER BY w.request_timestamp DESC
                LIMIT 1;

                IF v_existing_waitlist_id IS NOT NULL THEN
                    SET p_duplicate_existing_waitlist_id = v_existing_waitlist_id;
                    SET p_status = 2;
                    SET p_error_message = 'Another open waitlist item already exists for this report line.';
                END IF;
            END IF;
        END IF;
    END IF;

    IF p_status = 0 THEN
        START TRANSACTION;

        IF v_is_create = 1 THEN
            INSERT INTO customer_pull_pack_waitlist
            (
                waitlist_id,
                source_line_key,
                customer_id,
                customer_name,
                customer_order_id,
                parent_part_id,
                requested_quantity,
                requested_by_user_id,
                requested_by_display_name,
                requester_context_note,
                current_status,
                current_owner_user_id,
                current_owner_display_name,
                location_review_flag,
                problem_reason,
                handler_note,
                completion_user_id,
                completion_timestamp,
                last_updated_by_user_id,
                request_timestamp,
                recheck_indicator
            )
            VALUES
            (
                p_waitlist_id,
                TRIM(p_source_line_key),
                UPPER(TRIM(p_customer_id)),
                COALESCE(TRIM(p_customer_name), ''),
                UPPER(TRIM(p_customer_order_id)),
                UPPER(TRIM(p_parent_part_id)),
                p_requested_quantity,
                TRIM(p_requested_by_user_id),
                COALESCE(TRIM(p_requested_by_display_name), ''),
                NULLIF(TRIM(COALESCE(p_requester_context_note, '')), ''),
                v_current_status,
                NULLIF(TRIM(COALESCE(p_current_owner_user_id, '')), ''),
                NULLIF(TRIM(COALESCE(p_current_owner_display_name, '')), ''),
                v_effective_location_review_flag,
                v_problem_reason,
                NULLIF(TRIM(COALESCE(p_handler_note, '')), ''),
                v_effective_completion_user_id,
                v_effective_completion_timestamp,
                TRIM(p_last_updated_by_user_id),
                CURRENT_TIMESTAMP,
                COALESCE(p_recheck_indicator, 0)
            );
        ELSE
            UPDATE customer_pull_pack_waitlist
            SET
                source_line_key = TRIM(p_source_line_key),
                customer_id = UPPER(TRIM(p_customer_id)),
                customer_name = COALESCE(TRIM(p_customer_name), ''),
                customer_order_id = UPPER(TRIM(p_customer_order_id)),
                parent_part_id = UPPER(TRIM(p_parent_part_id)),
                requested_quantity = p_requested_quantity,
                requested_by_user_id = TRIM(p_requested_by_user_id),
                requested_by_display_name = COALESCE(TRIM(p_requested_by_display_name), ''),
                requester_context_note = NULLIF(TRIM(COALESCE(p_requester_context_note, '')), ''),
                current_status = v_current_status,
                current_owner_user_id = NULLIF(TRIM(COALESCE(p_current_owner_user_id, '')), ''),
                current_owner_display_name = NULLIF(TRIM(COALESCE(p_current_owner_display_name, '')), ''),
                location_review_flag = v_effective_location_review_flag,
                problem_reason = v_problem_reason,
                handler_note = NULLIF(TRIM(COALESCE(p_handler_note, '')), ''),
                completion_user_id = v_effective_completion_user_id,
                completion_timestamp = v_effective_completion_timestamp,
                last_updated_by_user_id = TRIM(p_last_updated_by_user_id),
                recheck_indicator = COALESCE(p_recheck_indicator, 0)
            WHERE waitlist_id = p_waitlist_id;
        END IF;

        DELETE FROM customer_pull_pack_waitlist_location
        WHERE waitlist_id = p_waitlist_id;

        SET v_selected_location_index = 0;
        WHILE v_selected_location_index < v_selected_location_count DO
            SET v_selected_location_id = UPPER(
                TRIM(
                    JSON_UNQUOTE(
                        JSON_EXTRACT(
                            p_selected_locations_json,
                            CONCAT('$[', v_selected_location_index, ']')
                        )
                    )
                )
            );

            IF v_selected_location_id IS NOT NULL AND CHAR_LENGTH(v_selected_location_id) > 0 THEN
                INSERT INTO customer_pull_pack_waitlist_location
                (
                    waitlist_id,
                    selection_order,
                    location_id,
                    selected_by_user_id,
                    selected_at
                )
                VALUES
                (
                    p_waitlist_id,
                    v_selected_location_index + 1,
                    v_selected_location_id,
                    TRIM(p_last_updated_by_user_id),
                    CURRENT_TIMESTAMP
                )
                ON DUPLICATE KEY UPDATE
                    selection_order = VALUES(selection_order),
                    selected_by_user_id = VALUES(selected_by_user_id),
                    selected_at = VALUES(selected_at);
            END IF;

            SET v_selected_location_index = v_selected_location_index + 1;
        END WHILE;

        COMMIT;
    END IF;
END $$
DELIMITER ;