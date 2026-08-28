<!-- 
[DOC-META-START]
- File Name: copilotforms-index-notes.md
- Description: Index notes for CopilotForms export handlers.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 12-18: # CopilotForms Index Notes
- Critical Notes: None
[DOC-META-END]
-->

# CopilotForms Index Notes

- CopilotForms landing page now supports search, grouped category filters (Build/Debug/Review/Data/Docs), sort modes, result counts, quick jump links, empty-state reset, and keyboard shortcuts (`/` focuses search, `Esc` clears it).
- Shared implementation lives in `docs/CopilotForms/index.html`, `docs/CopilotForms/assets/copilot-forms.css`, and `docs/CopilotForms/assets/copilot-forms.js`.
- Verified in browser with Playwright on 2026-03-22 against the local CopilotForms server at `http://172.16.1.104:8421/docs/CopilotForms/index.html`.
- CopilotForms form pages now expose explicit `All Features` and `All Sub-Features` options in the shared module/feature/sub-feature cascade. Aggregate selections flow through feature hints, summaries, and Markdown/JSON exports via `docs/CopilotForms/assets/copilot-forms.js`.
- VS Code test coverage is now scoped through `.vscode/settings.json` -> `dotnet.unitTests.runSettingsPath` pointing to `MTM_Receiving_Application.Tests/.runsettings`. That runsettings limits coverlet coverage to `[MTM_Receiving_Application]*`, excludes the test assembly, generated files, and NuGet package source helpers so coverage reflects publishable application code only.
