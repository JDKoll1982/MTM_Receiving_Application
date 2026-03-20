# Copilot Forms

Last Updated: 2026-03-18

This folder contains browser-based forms that help users produce structured, Copilot-friendly change requests.

## Purpose

The forms are designed for these common request types:

- UI changes
- Debugging
- Feature logic correction when behavior does not match user intent
- Refactoring for code quality or maintainability
- Refactoring for adding or removing logging
- Code review
- Database issue remediation
- Test generation
- Documentation changes
- UI mockup generation

Each form reads from a shared JSON catalog so Copilot or a maintainer can update the feature list as modules, workflows, and ownership change.

## Folder Layout

- `index.html`: landing page with links to each form
- `forms/`: scenario-specific HTML pages
- `assets/`: shared JavaScript and CSS
- `data/`: shared JSON catalog and schema
- `inputs/templates/`: machine-editable request templates for bulk authoring or AI-assisted drafting
- `outputs/templates/`: export format references
- `outputs/<scenario>/`: suggested place to save generated Markdown or JSON exports

## How To Use

1. Open `index.html`.
2. Pick the request type that best matches the work you want Copilot to perform.
3. Fill in the form.
4. Generate Markdown and JSON output.
5. Save the generated output into the matching folder under `outputs/`.
6. Link the saved export in chat and run the matching prompt from `.github/prompts/`.

## Run Locally

To serve the forms locally and open the index page automatically:

```powershell
.\docs\CopilotForms\Start-CopilotForms.ps1
```

Optional parameters:

- `-Port 9001` to use a different localhost port
- `-NoBrowser` to start the server without opening a browser window

This avoids browser restrictions around loading JSON directly from disk and lets the forms load `data/copilot-forms.config.json` automatically.

## Updating The Feature Catalog

Edit `data/copilot-forms.config.json` when:

- A new feature or workflow is added
- A module name changes
- File ownership changes
- A feature is removed or deprecated
- Prompt or instruction file names change

The HTML forms read that catalog at runtime. If the browser blocks local JSON loading, use the built-in `Load Local Config` button and select `data/copilot-forms.config.json` manually.

## Prompt And Instruction Pairing

Each scenario has:

- A `.prompt.md` file in `.github/prompts/`
- A `.instructions.md` file in `.github/instructions/`

Use the prompt file to tell Copilot how to act on an export. The instruction file explains how Copilot should interpret linked exports of that type.
