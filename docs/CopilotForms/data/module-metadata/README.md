# Split Module Metadata

Last Updated: 2026-03-21

This folder contains split CopilotForms metadata files grouped by module.

## Why This Exists

The main `copilot-forms.config.json` file should stay readable. Detailed feature metadata now lives here so it can grow by module without turning the main config into one oversized file.

## Structure

- One folder per module
- One `index.json` file per module folder
- One feature JSON file per feature

## Runtime Pattern

The CopilotForms runtime reads the main config first, then loads each module index listed in `project.moduleMetadataIndexes`, then loads the feature files named by those module indexes.

## Current Split Modules

- `Module_Receiving`
- `Module_Dunnage`
- `Module_Reporting`
- `Module_Volvo`

## Editing Rule

When a feature belongs to one of the split modules, update the file in this folder first rather than adding more detail back into the main config.
