-- Stored Procedure: sp_Receiving_ScannerProfile_GetByUser
-- Description: Returns scanner profiles for one user.

USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Receiving_ScannerProfile_GetByUser`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_ScannerProfile_GetByUser`(
    IN p_user_id VARCHAR(100)
)
BEGIN
    SELECT
        id,
        user_id,
        profile_name,
        is_default,
        target_executable_name,
        app_window_title,
        target_child_window_title,
        app_window_class,
        require_exact_title_match,
        from_warehouse_default,
        to_warehouse_default,
        activation_delay_ms,
        delay_between_fields_ms,
        pause_after_item_ms,
        popup_timeout_ms,
        popup_close_timeout_ms,
        send_shortcut_chord,
        stop_shortcut_chord,
        allow_advanced_timing,
        created_at,
        updated_at
    FROM receiving_scanner_profile
    WHERE user_id = p_user_id
    ORDER BY is_default DESC, profile_name ASC;
END $$

DELIMITER ;
