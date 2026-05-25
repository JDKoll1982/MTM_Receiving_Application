-- ============================================================
-- Procedure: sp_CustomerPullPack_UserDefaults_GetByUser
-- Purpose:   Returns one user's saved Customer Pull n' Pack defaults row.
-- ============================================================

DROP PROCEDURE IF EXISTS `sp_CustomerPullPack_UserDefaults_GetByUser`;

DELIMITER $$
CREATE PROCEDURE `sp_CustomerPullPack_UserDefaults_GetByUser`(
    IN p_user_id VARCHAR(100)
)
BEGIN
    SELECT
        user_id AS UserId,
        default_customer_id AS DefaultCustomerId,
        favorite_customer_ids_json AS FavoriteCustomerIdsJson,
        last_good_date_range_type AS LastGoodDateRangeType,
        last_good_date_from AS LastGoodDateFrom,
        last_good_date_to AS LastGoodDateTo,
        default_sort_mode AS DefaultSortMode,
        default_shortages_only AS DefaultShortagesOnly,
        default_unpulled_only AS DefaultUnpulledOnly,
        default_late_orders_only AS DefaultLateOrdersOnly,
        default_waitlist_status_set_json AS DefaultWaitlistStatusSetJson,
        default_print_preset AS DefaultPrintPreset,
        last_updated_by_user_id AS LastUpdatedByUserId,
        last_updated_timestamp AS LastUpdatedTimestamp
    FROM customer_pull_pack_user_defaults
    WHERE user_id = p_user_id
    LIMIT 1;
END $$
DELIMITER ;