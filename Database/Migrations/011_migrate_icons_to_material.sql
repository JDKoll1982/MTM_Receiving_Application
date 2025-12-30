-- =============================================
-- Migration: Migrate Legacy Icons to Material Design
-- Feature: Material Icons Migration
-- Description: Updates dunnage_types and user_preferences to use MaterialIconKind names
-- =============================================

-- 1. Update dunnage_types
-- Map known legacy names/glyphs to Material names
UPDATE dunnage_types SET Icon = 'PackageVariantClosed' WHERE Icon = 'Box' OR Icon = '\uE8B8';
UPDATE dunnage_types SET Icon = 'PackageVariant' WHERE Icon = 'Package' OR Icon = '\uE7B8';
UPDATE dunnage_types SET Icon = 'CubeOutline' WHERE Icon = 'Cube';
UPDATE dunnage_types SET Icon = 'Folder' WHERE Icon = 'Folder';
UPDATE dunnage_types SET Icon = 'Calendar' WHERE Icon = 'Calendar';
UPDATE dunnage_types SET Icon = 'Email' WHERE Icon = 'Mail';
UPDATE dunnage_types SET Icon = 'Flag' WHERE Icon = 'Flag';
UPDATE dunnage_types SET Icon = 'Star' WHERE Icon = 'Star';
UPDATE dunnage_types SET Icon = 'Heart' WHERE Icon = 'Heart';
UPDATE dunnage_types SET Icon = 'Pin' WHERE Icon = 'Pin';
UPDATE dunnage_types SET Icon = 'Tag' WHERE Icon = 'Tag';
UPDATE dunnage_types SET Icon = 'AlertCircle' WHERE Icon = 'Important';
UPDATE dunnage_types SET Icon = 'Alert' WHERE Icon = 'Warning';
UPDATE dunnage_types SET Icon = 'Information' WHERE Icon = 'Info';
UPDATE dunnage_types SET Icon = 'FileDocument' WHERE Icon = 'Document';
UPDATE dunnage_types SET Icon = 'Image' WHERE Icon = 'Image';
UPDATE dunnage_types SET Icon = 'MusicNote' WHERE Icon = 'Music';
UPDATE dunnage_types SET Icon = 'Video' WHERE Icon = 'Video';

-- Set default for any remaining unmapped or null icons
UPDATE dunnage_types SET Icon = 'PackageVariantClosed' WHERE Icon IS NULL OR Icon = '' OR LENGTH(Icon) < 3;

-- 2. Update user_preferences (Recently Used Icons)
-- Delete legacy entries (easier than renaming keys which might cause duplicates)
DELETE FROM user_preferences WHERE PreferenceKey LIKE 'RecentIcon_%' AND LENGTH(PreferenceKey) < 15; -- Heuristic: Material names are usually longer than glyphs
-- Or just clear all recent icons to force fresh start
DELETE FROM user_preferences WHERE PreferenceKey LIKE 'RecentIcon_%';
