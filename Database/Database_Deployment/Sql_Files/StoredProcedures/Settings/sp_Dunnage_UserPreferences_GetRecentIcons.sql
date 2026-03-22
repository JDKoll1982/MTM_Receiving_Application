DELIMITER $$

SET NAMES utf8mb4 COLLATE utf8mb4_general_ci$$

DROP PROCEDURE IF EXISTS `sp_Dunnage_UserPreferences_GetRecentIcons` $$

CREATE PROCEDURE `sp_Dunnage_UserPreferences_GetRecentIcons`(
    IN p_user_id VARCHAR(50),
    IN p_count INT
)
BEGIN
    SELECT
        SUBSTRING(PreferenceKey, 12) as icon_name
    FROM settings_dunnage_personal
        WHERE UserId = (CONVERT(p_user_id USING utf8mb4) COLLATE utf8mb4_general_ci)
      AND PreferenceKey LIKE 'RecentIcon_%'
    ORDER BY LastUpdated DESC
    LIMIT p_count;
END $$

DELIMITER ;
