-- Migration: Add home_location to dunnage_parts table
-- Date: 2025-12-30
-- Purpose: Add default storage location tracking for dunnage parts

USE mtm_receiving_application;

-- Add home_location column to dunnage_parts table
ALTER TABLE dunnage_parts 
ADD COLUMN home_location VARCHAR(100) NULL 
COMMENT 'Default storage location for this dunnage part';

-- Update existing records with placeholder
UPDATE dunnage_parts 
SET home_location = 'Not Specified' 
WHERE home_location IS NULL;
