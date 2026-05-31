---
applyTo: "**/*.md"
description: >
  General Markdown authoring standards for all .md files in this repository.
  Based on the Google Markdown Style Guide (https://google.github.io/styleguide/docguide/style.html).
  Applies to specs, docs, module FeatureUpdates, README files, and all other Markdown documents.
---

# Markdown Style Guide

Source of truth: <https://google.github.io/styleguide/docguide/style.html>

## Core Principle

Prefer standard Markdown syntax in all cases. Never use HTML or XML hacks when standard Markdown
can express the same structure. XML tags (`<feature_name>`, `<guardrails>`, etc.) are forbidden
in `.md` files — use `##` headings instead.

## Document Layout

Every document should follow this general structure:

```markdown
# Document Title

Short 1–3 sentence introduction.

## Topic

Content.

## See Also

- https://link-to-more-info
```

- Use a single `#` H1 heading as the document title (matches the filename without extension).
- The first heading should be the only H1. All subsequent headings start at H2 (`##`).
- Add a short introduction (1–3 sentences) immediately after the H1.
- Place a `## See Also` section at the bottom for miscellaneous links when relevant.

## Headings

- Use **ATX-style** headings only: `#`, `##`, `###`. Never use Setext underline style (`===`/`---`).
- Add one blank line before and after every heading.
- Use unique, fully descriptive heading names — no generic `Summary` or `Example` repeated in
  multiple sections. Prefer `## Foo Summary` and `## Bar Summary` over two headings both named
  `## Summary`.
- Never use XML tags as section wrappers. Replace every XML section wrapper with a `##` heading of
  the same name:

```markdown
<!-- FORBIDDEN -->
<confirmed_decisions>
- Decision one
</confirmed_decisions>

<!-- CORRECT -->
## Confirmed Decisions

- Decision one
```

## Lists

- Use `-` for all unordered list items. Do not mix `*` and `-` in the same document.
- Use `1.` for ordered/step lists. For long lists that may change, lazy-number with all `1.` items.
- Indent nested list items by 4 spaces.
- Use GitHub-flavored checkbox syntax for task lists: `- [ ]` (incomplete), `- [x]` (complete).

## Code

### Inline Code

Use backticks for inline code, file names, field names, method names, and short code snippets:

```markdown
Run `dotnet build` to compile the solution.
Update your `appsettings.json` file.
```

### Code Blocks

- Always use fenced code blocks (` ``` `) — never indented 4-space code blocks.
- Always declare the language on every fenced code block:

````markdown
```csharp
public async Task LoadAsync() { }
```
````

- Use `shell`, `json`, `sql`, `csharp`, `xml`, `powershell`, or `text` as appropriate.
- Escape shell command newlines with `\` so they can be copy-pasted directly.

## Line Length

- Keep lines at or under 120 characters where practical.
- Exceptions (lines may exceed 120 characters): links, table cells, headings, code blocks.
- Do not add trailing whitespace. Use a trailing `\` for intentional line breaks when necessary.

## Links

- Use descriptive link text — not "here", "link", or a bare URL as the label.
- Use explicit paths for links within the same repository: `[text](/path/to/file.md)`.
- Use reference-style links when the same URL appears multiple times or when inline URLs would
  make table cells too wide.

## Tables

- Use tables only for genuinely tabular data with relatively uniform cell content.
- Prefer bullet lists for most structured information — they are easier to read and write.
- Use reference links inside table cells to keep rows short.

## Images

- Use images sparingly. Prefer plain text where possible.
- Always provide descriptive alt text: `![Alt description](path/to/image.png)`.

## Strongly Prefer Markdown Over HTML

Never use raw HTML in `.md` files unless there is no Markdown equivalent. Every bit of HTML
hacking reduces readability and portability. Gitiles and many preview tools do not render HTML.

## File-Type-Specific Rules

### Spec Slice Files (`specs/**/*.md`) and Module Feature Docs (`Module_*/docs/**/*.md`)

These files follow the additional rules in `spec-slice-format.instructions.md`. The most
important rule: **never use XML tags as section wrappers** — always use `##` headings.

### README Files

- Include a short introduction, a quick-start section, and a link to further documentation.
- Keep the top-level README focused; link to sub-docs rather than embedding all content.

## Anti-Patterns

| Forbidden                                         | Use Instead                          |
| ------------------------------------------------- | ------------------------------------ |
| `<feature_name>My feature</feature_name>`         | `## Feature\n\nMy feature`           |
| `<guardrails>- Do not X</guardrails>`             | `## Guardrails\n\n- Do not X`        |
| Setext heading: `Title\n======`                   | `# Title`                            |
| Indented code block (4 spaces)                    | Fenced code block with language tag  |
| `[click here](url)` or `[link](url)`              | `[descriptive phrase](url)`          |
| Mixing `*` and `-` in lists                       | Use `-` throughout                   |
| Raw HTML `<br>`, `<div>`, `<span>`                | Use Markdown paragraph breaks        |
| Trailing whitespace                               | Remove it; use `\` for line breaks   |
