# CopilotForms Improvement Plan

Last Updated: 2026-03-20

> **Goal:** Reduce the time spent inside any form to the absolute minimum while producing
> richer, more actionable Copilot exports. Every idea below is graded by impact level and
> implementation effort so items can be prioritized independently.

---

## Implementation Status

| Tier | # | Item | Status |
|------|---|------|--------|
| 1 | 1.1 | VS Code task auto-starts the server | ✅ Done — `.vscode/tasks.json` |
| 1 | 1.2 | Sticky feature selection | ✅ Done — `sessionStorage` in JS |
| 1 | 1.3 | Required-field progress indicator | ✅ Done — progress bar in sidebar |
| 1 | 1.4 | Dynamic add/remove list rows | ✅ Done — `list-widget` in JS/CSS |
| 1 | 1.5 | Expand feature catalog | ✅ Done — 13 features (all modules) in config.json |
| 1 | 1.6 | Keyboard shortcut Ctrl+Shift+C | ✅ Done — `initKeyboardShortcuts()` |
| 2 | 2.1 | Smart form routing filter on index | ✅ Done — `initIndexPage()` search |
| 2 | 2.2 | Consolidate HTML files into SPA | ✅ Done — all 10 forms + index rebuilt |
| 2 | 2.3 | One-click save to `outputs/` folder | ✅ Done — File System Access API |
| 2 | 2.4 | Recent exports panel on index | ✅ Done — `renderRecents()` |
| 2 | 2.5 | User defaults + URL pre-fill | ✅ Done — defaults modal + `applyUrlParams()` |
| 2 | 2.6 | Dark mode | ✅ Done — CSS variables + theme toggle |
| 3 | 3.1 | Natural-language-to-fields pre-fill | ✅ Done — `initNlPrefill()` injects a "Describe your request" textarea above the form fields; "Parse into fields ↓" button runs client-side keyword extraction to fill feature select, select fields, list fields, and text/textarea fields |
| 3 | 3.2 | VS Code WebView sidebar panel | ✅ Done — HTML Preview Pro (`GingerTurtle.html-preview-pro`) + `CopilotForms: Launch` task + `.vscode/settings.json` |
| 3 | 3.3 | Pre-filled example template library | ⏳ Not started |
| 3 | 3.4 | Prompt runner / "Run in Copilot" | ✅ Done — `initPromptRunner()` |

---

## Current State Summary

| Area              | After Tier 1 + 2 implementation                                                               |
| ----------------- | --------------------------------------------------------------------------------------------- |
| Forms             | 10 form files all using shared SPA structure; one template to maintain                        |
| Navigation        | Full consistent topbar with breadcrumbs across all forms and index                            |
| Data entry        | User defaults pre-fill priority/audience/scope; URL params supported                         |
| Feature selection | Full catalog (all modules), sticky across forms, searchable on index                          |
| Output            | One-click save direct to `outputs/` folder via File System Access API                        |
| Draft saving      | Per-form `localStorage` with Clear Draft button                                               |
| Smart defaults    | Defaults modal; applies on every form open                                                    |
| Field UX          | Dynamic add/remove row widgets for list fields; inline delete buttons                         |
| Validation        | Progress bar shows required-field completion; highlights incomplete on export attempt         |
| Server            | `Start CopilotForms Server` VS Code task (`isBackground: true`)                               |
| Post-submit       | "Run in Copilot" button copies chat message; recent exports panel shows history               |

---

## Priority-Ranked Improvements

### Tier 1 — High Impact, Low Effort (do first)

These eliminate the most repetitive daily friction with minimal code change.

---

#### 1.1 Auto-Start via VS Code Task

**Problem:** The PowerShell server (`Start-CopilotForms.ps1`) must be launched manually every
session. If forgotten, the JSON config refuses to load in the browser and the user clicks the
awkward "Load Local Config" fallback.

**Improvement:** Add a `Start CopilotForms Server` VS Code task to `.vscode/tasks.json` that
runs `Start-CopilotForms.ps1` as a background process. Mark it `isBackground: true` with a
problem-matcher pattern. Optionally bind it to the `preLaunchTask` of the debug profile so
it starts automatically when the developer opens the project.

**Impact:** Eliminates "why won't the form load?" entirely.

**Effort:** 30 minutes.

---

#### 1.2 Sticky Feature Selection (Session Storage)

**Problem:** The user picks the same feature (almost always `receiving-workflow`) on every
form they open. The selection resets on every page load.

**Improvement:** Write the last-selected `featureId` to `sessionStorage` (not just
`localStorage` drafts). On form init, pre-select it before calling `renderForm`.

**Impact:** Saves two clicks on every single form visit.

**Effort:** 10 lines of JS.

---

#### 1.3 Required-Field Progress Indicator

**Problem:** There is no visual cue about form completeness. Users submit partial exports
without knowing they left required fields empty.

**Improvement:** Add a lightweight progress bar or "X / Y required fields filled" counter in
the sidebar that updates on every `input` / `change` event. Color it amber until 100% then
green. Also highlight unfilled required fields with a red border on the first download
attempt rather than silently exporting.

**Impact:** Stops empty exports from reaching Copilot. Reduces back-and-forth.

**Effort:** ~40 lines of JS + minor CSS.

---

#### 1.4 Dynamic List Fields (Add/Remove Rows)

**Problem:** List fields (reproduction steps, acceptance criteria, target files, etc.) use a
plain `textarea` with "one item per line" instructions. This produces messy mixed-whitespace
input and makes reordering or deleting a single item painful.

**Improvement:** Replace `type: "list"` rendering with a dynamic row widget: a vertical stack
of single-line `<input>` elements each with a delete button, plus an "Add item" button at the
bottom. The existing `readFieldValue` / `writeFieldValue` contract is preserved because the
value is still a string array. Draft restore already handles arrays.

**Impact:** Much faster to enter ordered lists (repro steps, acceptance criteria). Reduces
typos from accidental newlines.

**Effort:** ~100 lines of JS, existing CSS variables already cover it.

---

#### 1.5 Expand Feature Catalog to Cover All Modules

**Problem:** The catalog has only 5 features. The workspace has 11+ modules with many
distinct features. When the user picks "Custom / Not listed" Copilot gets no related-files
hint at all.

**Improvement:** Expand `copilot-forms.config.json` with entries for every current module and
key feature. At minimum add:

- `settings-core` — Module_Settings.Core
- `settings-receiving` — Module_Settings.Receiving
- `settings-dunnage` — Module_Settings.Dunnage
- `settings-reporting` — Module_Settings.Reporting
- `settings-volvo` — Module_Settings.Volvo
- `settings-developer-tools` — Module_Settings.DeveloperTools
- `shiprec-tools` — Module_ShipRec_Tools
- `shared-infrastructure` — Module_Shared + Infrastructure
- `volvo-integration` — already present but needs real file paths

For each, add actual file paths (not just folder paths) for the 2-3 most commonly touched
files so Copilot doesn't have to discover them.

**Impact:** Copilot always gets targeted context. Shorter investigation phase.

**Effort:** 30–60 minutes of catalog editing.

---

#### 1.6 Keyboard Shortcut: Copy Markdown

**Problem:** The most common action — "fill form, copy markdown, paste in chat" — requires a
mouse click on the Copy Markdown button.

**Improvement:** Bind `Ctrl+Shift+C` (or `Ctrl+Enter`) to the Copy Markdown action when focus
is inside the form. Show a transient "Copied!" toast for 1.5 seconds.

**Impact:** For power users this is the single most-used button; keyboard access matters.

**Effort:** 5 lines of JS.

---

### Tier 2 — High Impact, Moderate Effort

---

#### 2.1 Smart Form Routing from Natural Language (Index Page)

**Problem:** The user has to decide which of the 10 form types fits their need before they
open anything. A new or uncertain user picks the wrong form and wastes time.

**Improvement:** Add a single search/description box on `index.html`. As the user types, the
card grid filters and re-ranks the form cards using keyword scoring against each form's
`summary`, `whenToUse`, and `endUserTips` from the config. No LLM needed — simple keyword
matching across 10 items is fast and sufficient.

**Examples:**

- Typing "crash" or "error" or "not working" → surfaces Debugging first.
- Typing "add logs" or "logging too noisy" → surfaces Logging Refactor first.
- Typing "clean up" or "hard to read" → surfaces Improvement Refactor first.

**Impact:** Eliminates decision paralysis. Gets the user to the right form in one step.

**Effort:** ~80 lines of JS on the index page.

---

#### 2.2 Unified Single-Page Application (SPA) Mode

**Problem:** The 10 HTML form files are structurally identical except for `data-form-id`.
Any structural change (e.g., adding a new sidebar section) requires editing 10 files. The
nav bar in each form is also incomplete — it only lists a subset of the other forms.

**Improvement:** Consolidate all 10 forms into a single `index.html` with route-based
rendering. Use `location.hash` or `URLSearchParams` (`?form=debugging`) to load the correct
form definition. The sidebar navigation becomes a consistent full list generated from the
config. The 10 individual HTML files either become simple redirects or are removed.

**Benefits:**

- One place to maintain structure, nav, sidebar, and output panels.
- Full nav menu visible from every form.
- Adding a new form type = one JSON entry, not a new HTML file.

**Effort:** Half-day refactor. The JS and CSS are already shared — this is mostly template
consolidation.

---

#### 2.3 One-Click Export Save to `outputs/` Folder (File System Access API)

**Problem:** After generating output the user must: click Download, move the file to the
correct `outputs/<scenario>/` folder, rename it, then link it in chat. Three to five manual
steps.

**Improvement:** Use the browser's [File System Access API](https://developer.mozilla.org/en-US/docs/Web/API/File_System_API)
to write the export directly to the correct `outputs/<scenario>/` folder in one click.
The app prompts once to select the `outputs/` root (saved to `localStorage`), then on
subsequent saves it auto-derives the subfolder from `form.outputFolder`. Support a fallback
to the current download behavior if the API is unavailable.

**Auto-generated filename:** `<featureId>-<formId>-<YYYYMMDD-HHmm>.export.md`

**Impact:** Removes the manual save-and-move step entirely. The file lands in the right
place, ready to link.

**Effort:** ~60 lines of JS.

---

#### 2.4 "Recent Exports" Panel on Index Page

**Problem:** After saving exports in the outputs folders there is no way to see what was
recently submitted without using the file system. Users re-open forms to re-read their last
export.

**Improvement:** Store a short history (last 20 exports) in `localStorage` with the form
type, feature name, title, timestamp, and draft content. Render a "Recent" section on
`index.html` with links that re-open the form with the previous draft pre-loaded (deeping linking
to `?form=debugging&restoreId=<id>`).

**Impact:** Dramatically reduces "let me re-do that form" time. Also safety-net for
accidental browser closure.

**Effort:** ~80 lines of JS.

---

#### 2.5 Context-Aware Field Pre-Population

**Problem:** Many fields have answers that are mostly constant per session: the priority is
almost always "High", the audience is almost always "Developer", the scope is almost always
"Single feature". Every form makes you re-pick these.

**Improvement 1 — User Defaults:** Add a "My Defaults" modal on the index page where the
user sets their usual `priority`, `audience`, `scope`, and `defaultFeatureId`. These are
written to `localStorage` and applied as field defaults on every form open.

**Improvement 2 — URL Pre-fill:** Support query parameters for any field value so a VS Code
extension, a task, or a shell script can pre-fill fields. Example:
`?form=debugging&featureId=receiving-workflow&priority=High`. This enables "open with
context" shortcuts from the IDE.

**Impact:** Most forms become 3–5 fields to fill instead of 8–12.

**Effort:** ~60 lines of JS for defaults modal; 20 lines for URL pre-fill parsing.

---

#### 2.6 Dark Mode Support

**Problem:** The CSS uses only a single `color-scheme: light` root. Extended evening
sessions in a dark IDE result in a blinding white browser form.

**Improvement:** Add a `color-scheme: light dark` toggle with a `@media (prefers-color-scheme: dark)` block that maps dark equivalents for all CSS variables, plus a manual toggle button. The preference is saved to `localStorage`.

**Effort:** ~50 lines of CSS + 10 lines of JS.

---

### Tier 3 — Transformative, Higher Effort

These change how much time the user spends in the form fundamentally.

---

#### 3.1 Natural-Language-to-Form Pre-fill (Copilot Chat Context Bridge)

**Problem:** The user already knows what they want to ask Copilot. They type it into chat,
then have to translate that same thought into structured fields. This is the biggest source
of form friction.

**Improvement:** Add a large "Describe your request in plain English" text area at the **top
of every form** (above the structured fields). When the user pauses typing (debounced 600ms),
the JS makes a `localhost` call to the CopilotForms server with the text and the form's field
definitions, and the server runs a lightweight keyword/pattern-extraction routine to
suggest values for each field. Suggestions appear as grey placeholder text. The user can
accept (Tab), ignore, or override.

**Alternatively (simpler):** Skip the server call. Instead, provide a "Parse into fields"
button that calls the Copilot API (or just opens a prepared chat prompt) with the text and
the form field names, and the user copies the JSON back.

**Impact:** Potentially eliminates manual field-by-field entry entirely for most requests.

**Effort:** Significant — 2–4 hours for the server-side extraction, or ~1 hour for the
manual-bridge version.

---

#### 3.2 VS Code Extension or Sidebar Panel (Long-Term)

**Problem:** The browser is a detour from VS Code. The user switches windows, fills the
form, copies output, and switches back.

**Improvement:** Build a minimal VS Code WebView panel that embeds the CopilotForms SPA
directly in the IDE sidebar. Key capabilities:

- No server needed; reads config from workspace file system directly.
- Knows the current active file and pre-populates `targetFiles` / `filesToInspect`
  automatically.
- "Copy to Chat" directly inserts the Markdown into the currently open Copilot Chat window.
- No browser, no saving, no switching.

**Impact:** Eliminates every cross-application step. Form → output → Copilot in one panel.

**Effort:** 2–3 days (VS Code extension scaffold, WebView, IPC for active-file context).

---

#### 3.3 Template Library with Pre-filled Common Requests

**Problem:** The inputs/templates folder exists but all templates are empty JSON shells with
no example content. They are only useful for bulk AI authoring.

**Improvement:** Create a library of 3–5 fully worked example requests per form type, stored
as `.input.example.json` files. Expose them in the form UI under a "Load Example" dropdown.
Examples should mirror the most common requests in this codebase (e.g., "Receiving workflow
field refresh bug", "Add ViewModel tests for a new DAO", "Cleanup service layer duplication").

**Benefits:**

- New users learn what good input looks like by example.
- Repeat request types become one-click (load example → tweak → copy).

**Effort:** Writing examples: 1–2 hours. UI for loading them: ~30 lines of JS.

---

#### 3.4 Prompt Runner Integration (Skip the Manual Prompt Step)

**Problem:** After saving the export the user still has to manually open the matching
`.prompt.md` file in VS Code chat or type the prompt name. This final step is easy to forget
or misspell.

**Improvement:** In the sidebar (after generating output) show a "Run in Copilot" button that
deep-links using the `vscode://` URI scheme to open the matching `.prompt.md` file and
pre-populate it with the path of the just-saved export. The exact URI format is:
`vscode://GitHub.copilot-chat/openPrompt?file=...`

Alternatively, generate the full chat message text ("Use [/copilotforms-debugging](...)
against this export: [path]") so the user only has to paste one string.

**Impact:** Eliminates the final manual handoff from form to Copilot.

**Effort:** ~30 lines of JS once the URI format is verified.

---

## UI Improvements Summary

| #   | Improvement                                    | Area                 |
| --- | ---------------------------------------------- | -------------------- |
| 1.1 | VS Code task auto-starts the server            | Developer tooling    |
| 1.2 | Sticky feature selection across forms          | Navigation           |
| 1.3 | Required-field progress indicator + validation | Form UX              |
| 1.4 | Dynamic add/remove list rows                   | Field UX             |
| 1.5 | Full feature catalog (all 11+ modules)         | Data quality         |
| 1.6 | Keyboard shortcut for Copy Markdown            | Power user UX        |
| 2.1 | Smart form routing filter on index             | Navigation           |
| 2.2 | Consolidate 10 HTML files into one SPA         | Maintainability      |
| 2.3 | One-click save direct to `outputs/` folder     | Output workflow      |
| 2.4 | Recent exports panel on index                  | History / recall     |
| 2.5 | User defaults + URL pre-fill                   | Form UX              |
| 2.6 | Dark mode                                      | Visual comfort       |
| 3.1 | Natural-language-to-fields pre-fill            | Input intelligence   |
| 3.2 | VS Code WebView sidebar panel                  | Platform integration |
| 3.3 | Pre-filled example template library            | Onboarding / recall  |
| 3.4 | "Run in Copilot" prompt runner button          | Post-submit workflow |

---

## Time-in-Form Reduction Estimate

| Scenario                                | Today  | After Tier 1 | After Tier 2 | After Tier 3 |
| --------------------------------------- | ------ | ------------ | ------------ | ------------ |
| Submit a debugging request (first time) | ~6 min | ~4 min       | ~2 min       | ~45 sec      |
| Submit same request type again          | ~5 min | ~3 min       | ~1.5 min     | ~30 sec      |
| Find and re-open a recent export        | ~3 min | ~2 min       | ~30 sec      | ~10 sec      |
| Handoff export to Copilot Chat          | ~2 min | ~2 min       | ~1 min       | ~5 sec       |

---

## Implementation Notes

- **No breaking schema changes** are required for Tier 1 or Tier 2. All improvements are
  additive to the existing `copilot-forms.config.json` schema or to the JS/CSS layer.
- Tier 2.2 (SPA consolidation) is the only structurally breaking change. The existing HTML
  files can remain as permanent redirects while the new SPA stabilizes.
- **File System Access API** (2.3) requires Chromium 86+. The download fallback keeps older
  browser support.
- The VS Code extension (3.2) should be treated as a separate project, not a patch to this
  system.
- All improvements preserve the existing export format — Copilot instructions and prompt
  files require no changes.

---

## Suggested Implementation Order

1. `1.5` — Expand feature catalog (immediate value, no code change)
2. `1.1` — VS Code task for server startup (removes daily friction)
3. `1.2` — Sticky feature selection (trivial JS)
4. `1.4` — Dynamic list rows (biggest form-UX win)
5. `1.3` — Progress indicator + required validation
6. `1.6` — Keyboard shortcut
7. `2.1` — Smart routing filter on index
8. `2.5` — User defaults
9. `2.2` — SPA consolidation
10. `2.3` — Direct save to `outputs/`
11. `2.4` — Recent exports
12. `2.6` — Dark mode
13. `3.3` — Example template library
14. `3.4` — Prompt runner button
15. `3.1` — Natural-language pre-fill
16. `3.2` — VS Code extension (separate initiative)
