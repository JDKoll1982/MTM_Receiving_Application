# Copilot Forms

Last Updated: 2026-03-22

This folder contains browser-based forms that help users produce structured, Copilot-friendly change requests.

## Purpose

The forms are designed for these common request types:

- UI changes
- Debugging
- Feature logic correction when behavior does not match user intent
- New feature requests
- Feature removal or retirement requests
- Performance issues and optimization work
- Configuration and environment issues
- Naming and consistency cleanup
- Refactoring for code quality or maintainability
- Refactoring for adding or removing logging
- Combined UI and logic changes
- Combined debugging and logging changes
- Combined logic changes with test generation
- Code review
- Database issue remediation
- Test generation
- Documentation changes
- UI mockup generation

Each form reads from a shared JSON catalog so Copilot or a maintainer can update the feature list as modules, workflows, and ownership change.

Detailed feature metadata can now be split into module-specific files under `data/module-metadata/` so the main config stays smaller and easier to maintain.

## Folder Layout

- `index.html`: landing page with links to each form
- `forms/`: scenario-specific HTML pages
- `assets/`: shared JavaScript and CSS
- `data/`: shared JSON catalog and schema
- `data/module-metadata/`: split per-module feature metadata loaded at runtime
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

Generated exports now also include an explicit reminder that the CopilotForms metadata for the edited module must be reviewed and updated when needed as part of the same request.

When you use `Save to outputs`, `Download Markdown`, or `Download JSON`, the app now targets the matching request-type folder automatically, such as `docs/CopilotForms/outputs/code-review/` for the code review form. The first save asks you to choose the repo root, the `docs/CopilotForms` folder, or the `outputs` folder so the browser can get permission to write files there.

The landing page supports faster navigation with search, category filter chips, multiple sort modes, result counts, quick jumps between forms and recent exports, and keyboard shortcuts like `/` to focus search and `Esc` to clear it.

On forms that use the shared module, feature, and sub-feature cascade, the dropdowns now include explicit `All Features` and `All Sub-Features` options so broader requests can be scoped without picking a single feature first.

## Run Locally

To serve the forms locally and open the index page automatically:

```powershell
.\docs\CopilotForms\Start-CopilotForms.ps1
```

Optional parameters:

- `-Port 9001` to use a different localhost port
- `-NoBrowser` to start the server without opening a browser window
- `-OpenTarget editor` to open `docs/CopilotForms/index.html` in VS Code and trigger the HTML Preview Pro interactive preview automatically
- `-OpenTarget none` to start the server without opening anything

This avoids browser restrictions around loading JSON directly from disk and lets the forms load `data/copilot-forms.config.json` automatically.

When you run the `Start CopilotForms Server` task from VS Code, it now asks whether to open CopilotForms in a VS Code interactive preview, in the web browser, or not open it automatically.

## Updating The Feature Catalog

Edit `data/copilot-forms.config.json` when:

- A new feature or workflow is added
- A module name changes
- File ownership changes
- A feature is removed or deprecated
- Prompt or instruction file names change

Edit `data/module-metadata/<ModuleName>/` when:

- A split module feature needs richer metadata
- New sub-feature hints are added
- Feature-aware prompt defaults or validation hints are updated
- An analyzed module is moved out of the main config and into split files

The HTML forms read that catalog at runtime. The runtime can also load split metadata indexes listed in `project.moduleMetadataIndexes` inside the main config.

The generated Markdown and JSON exports include a metadata follow-up section so the person or agent using the export is reminded to review both the main config and the module-specific metadata path for the edited module.

If the browser blocks local JSON loading, use the built-in `Load Local Config` button and select `data/copilot-forms.config.json` manually.

## Prompt And Instruction Pairing

Each scenario has:

- A `.prompt.md` file in `.github/prompts/`
- A `.instructions.md` file in `.github/instructions/`

Use the prompt file to tell Copilot how to act on an export. The instruction file explains how Copilot should interpret linked exports of that type.

## Current Scenario Set

- `ui-change`
- `debugging`
- `logic-correction`
- `new-feature-request`
- `feature-removal-request`
- `performance-issue-optimization`
- `configuration-environment-issue`
- `naming-consistency-cleanup`
- `ui-change-logic-change`
- `debugging-logging`
- `logic-change-test-generation`
- `improvement-refactor`
- `logging-refactor`
- `code-review`
- `database-issue`
- `test-generation`
- `documentation-change`
- `ui-mockup`
