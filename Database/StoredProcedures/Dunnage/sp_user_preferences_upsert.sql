DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_user_preferences_upsert` $$

CREATE PROCEDURE `sp_user_preferences_upsert`(
    IN p_user_id VARCHAR(50),
    IN p_preference_key VARCHAR(100),
    IN p_preference_value TEXT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    -- Error handler
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
        ROLLBACK;
    END;

    -- Start transaction
    START TRANSACTION;

    -- Insert or update user preference
    INSERT INTO user_preferences (UserId, PreferenceKey, PreferenceValue, LastUpdated)
    VALUES (p_user_id, p_preference_key, p_preference_value, NOW())
    ON DUPLICATE KEY UPDATE 
        PreferenceValue = VALUES(PreferenceValue),
        LastUpdated = NOW();

    -- Success
    SET p_status = 1;
    SET p_error_msg = 'User preference saved successfully';
    COMMIT;
END $$

DELIMITER ;
