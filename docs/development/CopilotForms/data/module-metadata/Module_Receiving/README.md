# Module_Receiving Metadata

Last Updated: 2026-03-21

This folder contains split CopilotForms metadata for `Module_Receiving`.

## Purpose

These files exist so the main `copilot-forms.config.json` file does not keep growing into one oversized catalog.

## Structure

- `index.json` - module-level manifest and file map
- `receiving-workflow.json` - guided receiving workflow metadata
- `receiving-edit-mode.json` - edit mode metadata
- `receiving-quality-hold.json` - quality hold metadata
- `receiving-package-preferences.json` - package preference metadata
- `receiving-label-export.json` - label data export metadata

## Notes

- The current live forms load split module metadata through the runtime's `project.moduleMetadataIndexes` support.
- These files are now part of the active metadata path, not just a staging area.
- New metadata work for `Module_Receiving` should be added here first.
