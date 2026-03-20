# CopilotForms Implementation Checklist

Last Updated: 2026-03-20

Track every improvement item from [IMPROVEMENT-PLAN.md](IMPROVEMENT-PLAN.md).
Check off each item as it is implemented. Update the "Status" and "Notes" columns inline.

---

## Tier 1 — High Impact, Low Effort

| #   | Item                                              | Status  | Notes                                                           |
| --- | ------------------------------------------------- | ------- | --------------------------------------------------------------- |
| 1.1 | VS Code task auto-starts the server               | ✅ Done | `.vscode/tasks.json` — task `CopilotForms: Start Server` added  |
| 1.2 | Sticky feature selection (sessionStorage)         | ✅ Done | `copilot-forms.js` — `saveStickyFeature` / `loadStickyFeature`  |
| 1.3 | Required-field progress indicator + validation    | ✅ Done | Progress bar in sidebar; toast on copy if required fields empty |
| 1.4 | Dynamic list fields (add / remove rows)           | ✅ Done | `createListWidget()` in JS; `list-widget` component in CSS      |
| 1.5 | Expand feature catalog to all 11+ modules         | ✅ Done | `copilot-forms.config.json` — 14 features across all modules    |
| 1.6 | Keyboard shortcut `Ctrl+Shift+C` to copy Markdown | ✅ Done | `initKeyboardShortcuts()` in JS                                 |

---

## Tier 2 — High Impact, Moderate Effort

| #   | Item                                                         | Status  | Notes                                                             |
| --- | ------------------------------------------------------------ | ------- | ----------------------------------------------------------------- |
| 2.1 | Smart form routing filter on index                           | ✅ Done | `scoreFormForQuery()` + search box on `index.html`                |
| 2.2 | SPA consolidation (10 HTML → `form.html` + redirects)        | ✅ Done | `form.html` is the SPA entry; each `forms/*.html` redirects to it |
| 2.3 | One-click save to `outputs/` folder (File System Access API) | ✅ Done | `saveToOutputsFolder()` in JS; falls back to download             |
| 2.4 | Recent exports panel on index                                | ✅ Done | `renderRecentExportsOnIndex()` — last 20 exports, shown on index  |
| 2.5 | User defaults + URL pre-fill                                 | ✅ Done | Defaults modal (`$('defaults-modal')`); `applyUrlParams()`        |
| 2.6 | Dark mode                                                    | ✅ Done | `data-theme` on `<html>`; full dark palette in CSS                |

---

## Tier 3 — Transformative

| #   | Item                                  | Status     | Notes                                                                                                                                    |
| --- | ------------------------------------- | ---------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| 3.1 | Natural-language-to-fields pre-fill   | ✅ Done    | `initNlPrefill()` — "Describe your request" textarea at top of every form + "Parse into fields ↓" button; client-side keyword extraction fills feature select, select fields, list fields, and text/textarea fields from plain English |
| 3.2 | VS Code extension / WebView panel     | ✅ Done    | `HTML Preview Pro` (GingerTurtle) — `Ctrl+Shift+V` for side panel; `CopilotForms: Launch` task starts server + opens index.html; `.vscode/settings.json` configures auto-reload, hot-reload, no-console |
| 3.3 | Pre-filled example template library   | ✅ Done    | `Load Example` dropdown per form; populated from `inputs/templates/`                                                                     |
| 3.4 | `Run in Copilot` prompt runner button | ✅ Done    | Copies prepared chat message text pointing at prompt file                                                                                |

---

## HTML Redesign — Per-File Status

| File                              | Status  | Notes                                                                   |
| --------------------------------- | ------- | ----------------------------------------------------------------------- |
| `assets/copilot-forms.css`        | ✅ Done | Full redesign: dark mode, progress bar, list widget, toast, modal, tabs |
| `assets/copilot-forms.js`         | ✅ Done | Full rewrite: SPA routing, all new features, clean module structure     |
| `index.html`                      | ✅ Done | Redesigned: smart search filter, recent exports panel, new card grid    |
| `form.html` (new)                 | ✅ Done | New SPA entry point; reads `?form=<id>` from URL                        |
| `forms/ui-change.html`            | ✅ Done | Redirect to `form.html?form=ui-change`                                  |
| `forms/debugging.html`            | ✅ Done | Redirect to `form.html?form=debugging`                                  |
| `forms/logic-correction.html`     | ✅ Done | Redirect to `form.html?form=logic-correction`                           |
| `forms/improvement-refactor.html` | ✅ Done | Redirect to `form.html?form=improvement-refactor`                       |
| `forms/logging-refactor.html`     | ✅ Done | Redirect to `form.html?form=logging-refactor`                           |
| `forms/code-review.html`          | ✅ Done | Redirect to `form.html?form=code-review`                                |
| `forms/database-issue.html`       | ✅ Done | Redirect to `form.html?form=database-issue`                             |
| `forms/test-generation.html`      | ✅ Done | Redirect to `form.html?form=test-generation`                            |
| `forms/documentation-change.html` | ✅ Done | Redirect to `form.html?form=documentation-change`                       |
| `forms/ui-mockup.html`            | ✅ Done | Redirect to `form.html?form=ui-mockup`                                  |

---

## Data / Infrastructure

| Item                                               | Status  | Notes                                       |
| -------------------------------------------------- | ------- | ------------------------------------------- |
| `data/copilot-forms.config.json` — expand features | ✅ Done | All modules catalogued with real file paths |
| `.vscode/tasks.json`                               | ✅ Done | `CopilotForms: Start Server` task added     |

---

## Legend

| Symbol         | Meaning                           |
| -------------- | --------------------------------- |
| ✅ Done        | Implemented and verified          |
| 🔄 In Progress | Currently being worked on         |
| ⬜ Pending     | Not yet started                   |
| ❌ Blocked     | Blocked by dependency or decision |
