-- Stored Procedure: sp_Receiving_ScannerProfile_Upsert
-- Description: Creates or updates one scanner profile, including target app metadata and warehouse defaults.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_ScannerProfile_Upsert`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_ScannerProfile_Upsert`(
    IN p_id CHAR(36),
    IN p_user_id VARCHAR(100),
    IN p_profile_name VARCHAR(120),
    IN p_is_default TINYINT(1),
    IN p_target_executable_name VARCHAR(255),
    IN p_app_window_title VARCHAR(255),
    IN p_target_child_window_title VARCHAR(255),
    IN p_app_window_class VARCHAR(255),
    IN p_require_exact_title_match TINYINT(1),
    IN p_from_warehouse_default VARCHAR(10),
    IN p_to_warehouse_default VARCHAR(10),
    IN p_activation_delay_ms INT,
    IN p_delay_between_fields_ms INT,
    IN p_pause_after_item_ms INT,
    IN p_popup_timeout_ms INT,
    IN p_popup_close_timeout_ms INT,
    IN p_send_shortcut_chord VARCHAR(64),
    IN p_stop_shortcut_chord VARCHAR(64),
    IN p_allow_advanced_timing TINYINT(1)
)
BEGIN
    INSERT INTO receiving_scanner_profile
    (
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
        allow_advanced_timing
    )
    VALUES
    (
        p_id,
        p_user_id,
        p_profile_name,
        p_is_default,
        p_target_executable_name,
        p_app_window_title,
        p_target_child_window_title,
        p_app_window_class,
        p_require_exact_title_match,
        p_from_warehouse_default,
        p_to_warehouse_default,
        p_activation_delay_ms,
        p_delay_between_fields_ms,
        p_pause_after_item_ms,
        p_popup_timeout_ms,
        p_popup_close_timeout_ms,
        p_send_shortcut_chord,
        p_stop_shortcut_chord,
        p_allow_advanced_timing
    )
    ON DUPLICATE KEY UPDATE
        profile_name = VALUES(profile_name),
        is_default = VALUES(is_default),
        target_executable_name = VALUES(target_executable_name),
        app_window_title = VALUES(app_window_title),
        target_child_window_title = VALUES(target_child_window_title),
        app_window_class = VALUES(app_window_class),
        require_exact_title_match = VALUES(require_exact_title_match),
        from_warehouse_default = VALUES(from_warehouse_default),
        to_warehouse_default = VALUES(to_warehouse_default),
        activation_delay_ms = VALUES(activation_delay_ms),
        delay_between_fields_ms = VALUES(delay_between_fields_ms),
        pause_after_item_ms = VALUES(pause_after_item_ms),
        popup_timeout_ms = VALUES(popup_timeout_ms),
        popup_close_timeout_ms = VALUES(popup_close_timeout_ms),
        send_shortcut_chord = VALUES(send_shortcut_chord),
        stop_shortcut_chord = VALUES(stop_shortcut_chord),
        allow_advanced_timing = VALUES(allow_advanced_timing),
        updated_at = CURRENT_TIMESTAMP;
END $$

DELIMITER ;