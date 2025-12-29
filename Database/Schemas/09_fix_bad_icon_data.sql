-- =============================================
-- Fix bad icon data
-- Feature: Dunnage Type Icons
-- Description: Clears invalid icon strings so they fallback to default in code
-- =============================================

UPDATE dunnage_types 
SET icon = NULL 
WHERE icon LIKE '%\uE7B8%' OR icon LIKE '%uE7B8%';
