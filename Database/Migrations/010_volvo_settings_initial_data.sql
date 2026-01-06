-- =====================================================
-- Migration: Initial Volvo Settings Data
-- =====================================================
-- Purpose: Populate default settings for Volvo module
-- Database: mtm_receiving_application
-- =====================================================

-- Delete old format if exists and insert new JSON format
DELETE FROM volvo_settings WHERE setting_key IN ('email_to_recipients', 'email_cc_recipients', 'email_subject_template', 'email_greeting');

-- Email Recipients Settings
INSERT INTO volvo_settings (setting_key, setting_value, setting_type, category, description, default_value)
VALUES
(
    'email_to_recipients',
    '[{"name":"Jose Rosas","email":"jrosas@mantoolmfg.com"},{"name":"Sandy Miller","email":"smiller@mantoolmfg.com"},{"name":"Steph Wittmus","email":"swittmus@mantoolmfg.com"}]',
    'String',
    'Email',
    'JSON array of primary email recipients with name and email fields',
    '[{"name":"Jose Rosas","email":"jrosas@mantoolmfg.com"},{"name":"Sandy Miller","email":"smiller@mantoolmfg.com"},{"name":"Steph Wittmus","email":"swittmus@mantoolmfg.com"}]'
),
(
    'email_cc_recipients',
    '[{"name":"Debra Alexander","email":"dalexander@mantoolmfg.com"},{"name":"Michelle Laurin","email":"mlaurin@mantoolmfg.com"}]',
    'String',
    'Email',
    'JSON array of CC email recipients with name and email fields',
    '[{"name":"Debra Alexander","email":"dalexander@mantoolmfg.com"},{"name":"Michelle Laurin","email":"mlaurin@mantoolmfg.com"}]'
),
(
    'email_subject_template',
    'PO Requisition - Volvo Dunnage - {Date} Shipment #{Number}',
    'String',
    'Email',
    'Email subject line template. Variables: {Date}, {Number}, {EmployeeNumber}',
    'PO Requisition - Volvo Dunnage - {Date} Shipment #{Number}'
),
(
    'email_greeting',
    'Good morning,',
    'String',
    'Email',
    'Opening greeting for email body',
    'Good morning,'
);
