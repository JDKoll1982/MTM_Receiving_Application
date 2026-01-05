-- Migration: {{migration_number}}_{{migration_name}}
-- Created: {{created_date}}
-- Module: {{module_name}}
-- Description: {{migration_description}}

-- ============================================================================
-- APPLY MIGRATION
-- ============================================================================

{{#if is_constraint}}
-- Add constraint
ALTER TABLE {{table_name}}
ADD CONSTRAINT {{constraint_name}} {{constraint_definition}};

{{/if}}
{{#if is_index}}
-- Add index
CREATE {{#if is_unique}}UNIQUE {{/if}}INDEX {{index_name}}
ON {{table_name}} ({{index_columns}});

{{/if}}
{{#if is_column}}
-- Add column
ALTER TABLE {{table_name}}
ADD COLUMN {{column_name}} {{column_type}} {{#if column_not_null}}NOT NULL{{/if}} {{#if column_default}}DEFAULT {{column_default}}{{/if}};

{{/if}}
{{#if is_trigger}}
-- Create trigger
DELIMITER $$
CREATE TRIGGER {{trigger_name}}
{{trigger_timing}} {{trigger_event}} ON {{table_name}}
FOR EACH ROW
BEGIN
    {{trigger_body}}
END$$
DELIMITER ;

{{/if}}
{{#if custom_sql}}
-- Custom migration
{{custom_sql}}

{{/if}}

-- ============================================================================
-- ROLLBACK (Run this to undo the migration)
-- ============================================================================

{{#if is_constraint}}
-- Remove constraint
-- ALTER TABLE {{table_name}} DROP CONSTRAINT {{constraint_name}};

{{/if}}
{{#if is_index}}
-- Remove index
-- DROP INDEX {{index_name}} ON {{table_name}};

{{/if}}
{{#if is_column}}
-- Remove column
-- ALTER TABLE {{table_name}} DROP COLUMN {{column_name}};

{{/if}}
{{#if is_trigger}}
-- Remove trigger
-- DROP TRIGGER IF EXISTS {{trigger_name}};

{{/if}}
{{#if rollback_sql}}
-- Custom rollback
-- {{rollback_sql}}

{{/if}}

-- ============================================================================
-- NOTES
-- ============================================================================
-- {{migration_notes}}
-- 
-- Related Issue: {{related_issue}}
-- Impact: {{impact_description}}
-- Tested: {{tested_status}}
