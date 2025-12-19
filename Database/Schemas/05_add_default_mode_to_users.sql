-- Add default_mode column to users table
-- This allows users to set a default receiving workflow mode

ALTER TABLE users 
ADD COLUMN default_receiving_mode VARCHAR(20) NULL
COMMENT 'Default receiving workflow mode: "guided", "manual", or NULL for always showing selection';

-- Valid values: 'guided', 'manual', or NULL
-- NULL means show mode selection screen every time
