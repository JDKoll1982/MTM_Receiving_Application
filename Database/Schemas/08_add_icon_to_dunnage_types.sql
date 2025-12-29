-- =============================================
-- Add icon column to dunnage_types
-- Feature: Dunnage Type Icons
-- =============================================

ALTER TABLE dunnage_types
ADD COLUMN icon VARCHAR(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL AFTER type_name;
