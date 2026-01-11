DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_user_preferences_get_recent_icons` $$

CREATE PROCEDURE `sp_user_preferences_get_recent_icons`(
    IN p_user_id VARCHAR(50),
    IN p_count INT
)
BEGIN
    SELECT
        SUBSTRING(PreferenceKey, 12) as icon_name
    FROM settings_dunnage_personal
    WHERE UserId = p_user_id
      AND PreferenceKey LIKE 'RecentIcon_%'
    ORDER BY LastUpdated DESC
    LIMIT p_count;
END $$

DELIMITER ;
