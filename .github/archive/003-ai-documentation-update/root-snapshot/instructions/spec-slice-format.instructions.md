---
applyTo: "specs/**/*.md,Module_*/docs/**/*.md"
description: >
  Formatting rules for spec slice files under specs/ and module FeatureUpdates docs under
  Module_*/docs/. Enforces Google Markdown Style Guide conventions — ATX headings, bullet lists,
  numbered steps, fenced code blocks — and explicitly prohibits XML tags (e.g. <feature_name>,
  <guardrails>) inside these documents.
---

# Spec Slice File Formatting Rules

Spec slice files and module FeatureUpdates documents are implementation-handoff documents. They
must be readable as plain text, renderable by any Markdown previewer, and copyable into a Copilot
prompt without XML parser errors.

> **Conflict resolution:** The `prompt-engineer-every-message` instruction recommends XML tags for
> structured AI prompts. That guidance does NOT apply to files covered by this instruction. Even
> though spec slice files contain a `## Copy/Paste Prompt` section, all structure inside the file
> must use `##` Markdown headings, not XML tags. XML tags break Markdown preview and printing.

## Core Rules

- Use ATX-style headings (`#`, `##`, `###`). Never use Setext (underline) style.
- Use a single `#` H1 heading at the top — the filename title.
- Separate every heading from the preceding content with one blank line.
- Use `-` for all unordered list items. Do not mix `*` and `-`.
- Use `1.` for all ordered/step lists.
- Declare the language on every fenced code block (` ```csharp `, ` ```json `, etc.).
- Keep lines at or under 120 characters where practical.
- **Do NOT use XML tags** (`<feature_name>`, `<confirmed_decisions>`, `<guardrails>`, etc.).
  Replace every XML section wrapper with a `##` heading of the same name.

## Required Section Structure

Every spec slice file must contain these sections in this order:

1. `# [Filename title]` — H1, matches the filename without extension.
2. `## Copy/Paste Prompt` — Contains the role sentence and feature description as prose.
3. `## Feature` — One-line description of the feature or fix this slice implements.
4. `## Implementation Order` — Order number and brief rationale.
5. `## Confirmed Decisions` — Bullet list of locked-in choices.
6. `## Current Repo State` — Bullet list of relevant file names, line numbers, and current behavior.
7. `## Required Outcome` — One or two sentences describing the end state.
8. `## Primary Change Areas` — Bullet list of files and methods to touch.
9. `## Files That Must Change` — Flat bullet list of file paths only.
10. `## Recommended Target Shape` — Prose description plus fenced code blocks.
11. `## Implementation Steps` — Numbered list.
12. `## Task Checklist` — GitHub-flavored Markdown checkbox list (`- [ ]`).
13. `## Validation` — Bullet list of verifiable acceptance criteria.
14. `## Guardrails` — Bullet list of explicit constraints and forbidden actions.
15. `## Completion Criteria` — Bullet list summarizing the definition of done.

## Anti-Patterns

```
<!-- FORBIDDEN -->
<feature_name>
My feature
</feature_name>

<!-- CORRECT -->
## Feature

My feature
```

```
<!-- FORBIDDEN -->
<confirmed_decisions>
- Decision one
</confirmed_decisions>

<!-- CORRECT -->
## Confirmed Decisions

- Decision one
```

## Google Markdown Style Guide References

- Headings: https://google.github.io/styleguide/docguide/style.html#headings
- Lists: https://google.github.io/styleguide/docguide/style.html#lists
- Code: https://google.github.io/styleguide/docguide/style.html#code
