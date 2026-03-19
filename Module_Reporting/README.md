# Module Reporting

Last Updated: 2026-03-19

## Overview

`Module_Reporting` provides end-of-day reporting for the live Receiving, Dunnage, and Volvo workflows.

The module is under active refactor because the upstream modules changed significantly and the original reporting contracts no longer match the live data model in several places.

## Current Scope

- Receiving report generation
- Dunnage report generation
- Volvo report generation
- Date-range availability checks
- PO normalization
- Clipboard-based simplified report summary output for Outlook-style paste workflows

## Current Refactor Focus

The current reporting work is focused on:

1. fixing DAO and SQL view mismatches
2. restoring correct Volvo availability and formatting behavior
3. replacing weak raw-HTML clipboard handling with CF_HTML-compliant clipboard output
4. removing stale spreadsheet/export and Routing assumptions from the module

## Source Documents

Use these two files as the source of truth for the reporting refactor:

- `Module_Reporting/Documentation/Reporting-Refactor-Audit.md`
- `Module_Reporting/Documentation/Reporting-Refactor-Checklist.md`

## Known Issues

- Receiving reporting still has DAO/view contract mismatches.
- Volvo reporting needed availability and formatting fixes and is still being validated.
- Outlook-style paste formatting is being upgraded from raw HTML copy behavior to a proper rich clipboard payload.

## Current Output Shape

The reporting screen and clipboard output are intentionally reduced to the business summary fields users asked for:

- PO
- Part/Dunnage
- Quantity
- Location
- Notes when available
- Loads/Skids when available
- Coils/Pcs/Type per Skid when available

## Validation Priorities

- Verify report generation for Receiving, Dunnage, and Volvo.
- Verify clipboard output in Outlook desktop.
- Verify clipboard output in New Outlook.
- Verify fallback paste behavior in plain-text targets.

## Notes

This module should not reintroduce CSV, XLS, or XLSX export workflows unless explicitly requested and re-scoped.
