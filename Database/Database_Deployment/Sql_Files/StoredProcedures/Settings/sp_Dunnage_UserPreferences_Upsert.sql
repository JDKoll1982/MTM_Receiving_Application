DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_UserPreferences_Upsert` $$

CREATE PROCEDURE `sp_Dunnage_UserPreferences_Upsert`(
    IN p_user_id    VARCHAR(50),
    IN p_pref_key   VARCHAR(100),
    IN p_pref_value TEXT
)
BEGIN
    INSERT INTO settings_dunnage_personal (UserId, PreferenceKey, PreferenceValue, LastUpdated)
    VALUES (p_user_id, p_pref_key, p_pref_value, NOW())
    ON DUPLICATE KEY UPDATE
        PreferenceValue = VALUES(PreferenceValue),
        LastUpdated     = NOW();
END $$

DELIMITER ;
