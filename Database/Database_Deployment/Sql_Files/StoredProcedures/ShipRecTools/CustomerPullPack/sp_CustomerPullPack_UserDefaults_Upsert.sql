-- ============================================================
-- Procedure: sp_CustomerPullPack_UserDefaults_Upsert
-- Purpose:   Saves or replaces the per-user default working context
--            for the Customer Pull n' Pack tool.
--            Supports FR-030 and FR-031.
-- ============================================================

DROP PROCEDURE IF EXISTS `sp_CustomerPullPack_UserDefaults_Upsert`;

DELIMITER $$
CREATE PROCEDURE `sp_CustomerPullPack_UserDefaults_Upsert`(
    IN p_user_id VARCHAR(100),
    IN p_default_customer_id VARCHAR(15),
    IN p_favorite_customer_ids_json TEXT,
    IN p_last_good_date_range_type VARCHAR(50),
    IN p_last_good_date_from DATETIME,
    IN p_last_good_date_to DATETIME,
    IN p_default_sort_mode VARCHAR(30),
    IN p_default_shortages_only TINYINT(1),
    IN p_default_unpulled_only TINYINT(1),
    IN p_default_late_orders_only TINYINT(1),
    IN p_default_waitlist_status_set_json TEXT,
    IN p_default_print_preset VARCHAR(30),
    IN p_last_updated_by_user_id VARCHAR(100),
    OUT p_status INT,
    OUT p_error_message VARCHAR(500)
)
BEGIN
    DECLARE v_default_sort_mode VARCHAR(30) DEFAULT 'PullDate';
    DECLARE v_default_print_preset VARCHAR(30) DEFAULT 'CurrentView';
    DECLARE v_favorite_customer_ids_json TEXT DEFAULT '[]';
    DECLARE v_default_waitlist_status_set_json TEXT DEFAULT '[]';

    SET p_status = 0;
    SET p_error_message = NULL;

    IF CHAR_LENGTH(TRIM(COALESCE(p_user_id, ''))) = 0 THEN
        SET p_status = 1;
        SET p_error_message = 'User ID is required.';
    ELSEIF CHAR_LENGTH(TRIM(COALESCE(p_last_updated_by_user_id, ''))) = 0 THEN
        SET p_status = 1;
        SET p_error_message = 'Last-updated-by user ID is required.';
    END IF;

    IF p_status = 0 THEN
        SET v_favorite_customer_ids_json = COALESCE(NULLIF(TRIM(COALESCE(p_favorite_customer_ids_json, '')), ''), '[]');
        SET v_default_waitlist_status_set_json = COALESCE(NULLIF(TRIM(COALESCE(p_default_waitlist_status_set_json, '')), ''), '[]');

        IF JSON_VALID(v_favorite_customer_ids_json) = 0 THEN
            SET p_status = 1;
            SET p_error_message = 'Favorite customer IDs must be a valid JSON array.';
        ELSEIF JSON_VALID(v_default_waitlist_status_set_json) = 0 THEN
            SET p_status = 1;
            SET p_error_message = 'Default waitlist status set must be a valid JSON array.';
        END IF;
    END IF;

    IF p_status = 0 THEN
        CASE UPPER(TRIM(COALESCE(p_default_sort_mode, 'PULLDATE')))
            WHEN 'PULLDATE' THEN SET v_default_sort_mode = 'PullDate';
            WHEN 'SHORTAGEFIRST' THEN SET v_default_sort_mode = 'ShortageFirst';
            WHEN 'PART' THEN SET v_default_sort_mode = 'Part';
            ELSE
                SET p_status = 1;
                SET p_error_message = 'Default sort mode is invalid.';
        END CASE;
    END IF;

    IF p_status = 0 THEN
        CASE UPPER(TRIM(COALESCE(p_default_print_preset, 'CURRENTVIEW')))
            WHEN 'CURRENTVIEW' THEN SET v_default_print_preset = 'CurrentView';
            WHEN 'FLOORCOPY' THEN SET v_default_print_preset = 'FloorCopy';
            WHEN 'PULLLIST' THEN SET v_default_print_preset = 'PullList';
            WHEN 'SHORTAGEONLY' THEN SET v_default_print_preset = 'ShortageOnly';
            WHEN 'WAITLISTONLY' THEN SET v_default_print_preset = 'WaitlistOnly';
            WHEN 'SELECTEDCONTEXT' THEN SET v_default_print_preset = 'SelectedContext';
            ELSE
                SET p_status = 1;
                SET p_error_message = 'Default print preset is invalid.';
        END CASE;
    END IF;

    IF p_status = 0 THEN
        INSERT INTO customer_pull_pack_user_defaults
        (
            user_id,
            default_customer_id,
            favorite_customer_ids_json,
            last_good_date_range_type,
            last_good_date_from,
            last_good_date_to,
            default_sort_mode,
            default_shortages_only,
            default_unpulled_only,
            default_late_orders_only,
            default_waitlist_status_set_json,
            default_print_preset,
            last_updated_by_user_id
        )
        VALUES
        (
            TRIM(p_user_id),
            NULLIF(UPPER(TRIM(COALESCE(p_default_customer_id, ''))), ''),
            v_favorite_customer_ids_json,
            NULLIF(TRIM(COALESCE(p_last_good_date_range_type, '')), ''),
            p_last_good_date_from,
            p_last_good_date_to,
            v_default_sort_mode,
            COALESCE(p_default_shortages_only, 0),
            COALESCE(p_default_unpulled_only, 0),
            COALESCE(p_default_late_orders_only, 0),
            v_default_waitlist_status_set_json,
            v_default_print_preset,
            TRIM(p_last_updated_by_user_id)
        )
        ON DUPLICATE KEY UPDATE
            default_customer_id = VALUES(default_customer_id),
            favorite_customer_ids_json = VALUES(favorite_customer_ids_json),
            last_good_date_range_type = VALUES(last_good_date_range_type),
            last_good_date_from = VALUES(last_good_date_from),
            last_good_date_to = VALUES(last_good_date_to),
            default_sort_mode = VALUES(default_sort_mode),
            default_shortages_only = VALUES(default_shortages_only),
            default_unpulled_only = VALUES(default_unpulled_only),
            default_late_orders_only = VALUES(default_late_orders_only),
            default_waitlist_status_set_json = VALUES(default_waitlist_status_set_json),
            default_print_preset = VALUES(default_print_preset),
            last_updated_by_user_id = VALUES(last_updated_by_user_id),
            last_updated_timestamp = CURRENT_TIMESTAMP;
    END IF;
END $$
DELIMITER ;