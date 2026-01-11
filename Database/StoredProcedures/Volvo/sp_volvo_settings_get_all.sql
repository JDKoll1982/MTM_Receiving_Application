-- =====================================================
-- Stored Procedure: sp_settings_module_volvo_get_all
-- =====================================================
-- Purpose: Get all Volvo settings, optionally filtered by category
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_settings_module_volvo_get_all$$

CREATE PROCEDURE sp_settings_module_volvo_get_all(
  IN p_category VARCHAR(50)
)
BEGIN
  IF p_category IS NULL OR p_category = '' THEN
    SELECT
      setting_key,
      setting_value,
      setting_type,
      category,
      description,
      default_value,
      min_value,
      max_value,
      modified_date,
      modified_by
    FROM settings_module_volvo
    ORDER BY category, setting_key;
  ELSE
    SELECT
      setting_key,
      setting_value,
      setting_type,
      category,
      description,
      default_value,
      min_value,
      max_value,
      modified_date,
      modified_by
    FROM settings_module_volvo
    WHERE category = p_category
    ORDER BY setting_key;
  END IF;
END$$

DELIMITER ;
