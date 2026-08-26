-- Function: fn_Receiving_Scanner_IsShortcutDistinct
-- Description: Returns 1 when send and stop shortcuts are different.

USE mtm_receiving_application_test;

DROP FUNCTION IF EXISTS `fn_Receiving_Scanner_IsShortcutDistinct`;

DELIMITER $$

CREATE FUNCTION `fn_Receiving_Scanner_IsShortcutDistinct`(
    p_send_shortcut VARCHAR(64),
    p_stop_shortcut VARCHAR(64)
)
RETURNS TINYINT(1)
DETERMINISTIC
BEGIN
    RETURN CASE
        WHEN COALESCE(TRIM(p_send_shortcut), '') = '' THEN 0
        WHEN COALESCE(TRIM(p_stop_shortcut), '') = '' THEN 0
        WHEN LOWER(TRIM(p_send_shortcut)) = LOWER(TRIM(p_stop_shortcut)) THEN 0
        ELSE 1
    END;
END $$

DELIMITER ;