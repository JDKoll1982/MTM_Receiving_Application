---
applyTo: '**/.github/skills/mtm-update-application-version/references/*.md'
---

<!-- 
[DOC-META-START]
- File Name: update-page-authoring.md
- Description: Content, structure, asset, interaction-hook, and accessibility rules for MTM end-user update pages under docs/updates.
- Last Updated: 2026-11-16
- Quick TOC:
  - Line 24-24: # Update Page Authoring
  - Line 28-28: ## File ownership
  - Line 41-41: ## Required page anatomy
  - Line 66-66: ## Interaction hooks the shared script expects
  - Line 76-76: ## Styling hooks
  - Line 88-88: ## Content rules
  - Line 110-110: ## Accessibility
  - Line 119-119: ## Updating `docs/updates/index.html`
  - Line 132-132: ## Style details
- Critical Notes: Reuse the shared docs/updates/styles.css and app.js, keep the mockup's page anatomy and JS hooks, and describe only changes the diff supports.
[DOC-META-END]
-->

# Update Page Authoring

Last Updated: 2026-11-16

## File ownership

| File | This workflow's action |
| --- | --- |
| `docs/updates/{version}/index.html` | Create for the new version |
| `docs/updates/index.html` | Add the new row, refresh latest-release surfaces |
| `docs/updates/styles.css` | Change only when the new page genuinely needs a new style |
| `docs/updates/app.js` | Change only when a new interaction is genuinely needed |
| `docs/updates/mockup/` | Reference only; never edit while producing a release |
| `docs/updates/{older}/index.html` | Never edit |

Version folders contain exactly one file: `index.html`. No per-version `styles.css` or `app.js`.

## Required page anatomy

Keep this order and these ids; the shared script and styles depend on them.

```text
skip-link (.skip-link -> #main-content)
page-shell
  header.hero
    hero-brand > img.hero-logo (../../../Assets/MTMLogo.jpg) + eyebrow, h1, hero-copy
    hero-meta > span.badge, span.meta-text ("Released {Month DD, YYYY}"), hero-actions
      button.button-primary[data-scroll-target="#highlights"]
      button#toggle-details.button-secondary
  main#main-content.content-grid
    aside.sidebar.card > "On this page" nav (#highlights, #changes, #impact, #action, #support)
      button#copy-version[data-copy-text="{version}"]
    div.main-column
      section#highlights.card.section-block > .highlight-grid > article.highlight-card
      section#changes.card.section-block > .filter-row + details.change-item[data-category]
      section#impact.card.section-block > .impact-list > article
      section#action.card.section-block.action-panel > p.action-state.success + ul.action-list
      section#support.card.section-block.support-panel
button#back-to-top.back-to-top
script src="../app.js"
```

## Interaction hooks the shared script expects

- `#toggle-details` toggles every `.change-item`.
- `.change-item[data-category]` values must match the `.filter-chip[data-filter]` values, plus an
  `all` chip.
- `[data-scroll-target]` scrolls to that selector.
- `#copy-version` copies its `data-copy-text` value.
- `#back-to-top` shows after scrolling.
- `.badge` is the version badge the copy button falls back to.

## Styling hooks

Use the existing classes rather than new CSS: `page-shell`, `hero`, `hero-brand`, `hero-logo`,
`eyebrow`, `hero-copy`, `hero-meta`, `badge`, `meta-text`, `hero-actions`, `button`,
`button-primary`, `button-secondary`, `button-block`, `content-grid`, `sidebar`, `sidebar-note`,
`main-column`, `card`, `section-block`, `section-heading`, `split-heading`, `section-label`,
`highlight-grid`, `highlight-card`, `highlight-title`, `highlight-text`, `accent-blue`,
`accent-green`, `accent-gold`, `filter-row`, `filter-chip`, `is-active`, `change-item`,
`impact-list`, `action-panel`, `action-state success`, `action-list`, `support-panel`,
`muted-note`, `release-table`, `release-table-wrapper`, `release-link`, `skip-link`,
`back-to-top`.

## Content rules

- Audience: operators, leads, and support staff. Never developers.
- Explain the screen the user sees and what changed there. No file paths, class names, method
  names, table names, or stored procedure names.
- Group the detail section by the categories used in the filter chips, typically `workflow`,
  `tools`, and `support`. Every `.change-item` carries one of them.
- Three highlight cards is the established rhythm: the three changes a user would notice first.
- "Why it matters" translates each highlight into a floor-level consequence.
- "What you need to do" states plainly either `No action is required from operators after this
  update.` or the exact steps. Never leave it implied.
- Mention deployment-only work (schema, label procedures) as handled by the MTM IT support team
  when it is in the change set.
- Support section names the symptoms that should be reported and reminds the user to quote the
  version shown on the page.
- Describe only what the change set supports. If a change is internal, summarize its user-visible
  effect or leave it out. Inventing a feature is a release defect.
- Use the release date the release is being published: today's date at generation time, written
  as `Released {Month DD, YYYY}` (for example `Released November 16, 2026`).
- Plain, scannable language: short paragraphs, real examples, no marketing superlatives.
- Descriptive link text only. Never `here`, `click here`, or `this link`.

## Accessibility

- `lang="en"` on `<html>`, viewport meta present, single `<h1>`.
- Headings in order; sections use `<h2>`, cards and impact items use `<h3>`.
- Landmarks: `<header>`, `<main>`, `<nav>`, `<section>`; keep the skip link.
- Decorative-free images: the logo keeps an `alt` describing MTM, not "logo".
- Native `<button>`, `<a>`, `<details>`, and `<summary>` only; no click handlers on `<div>`.
- Do not remove focus styles or set `tabindex` above `0`.

## Updating `docs/updates/index.html`

1. Insert the new `<tr>` as the **first row** of the release table body, newest at top.
2. Keep the row shape: version link to `{version}/index.html`, `{Month DD, YYYY}` date, one
   sentence summary that matches the release page.
3. Hero badge: `Latest release {version}`.
4. Hero copy button: `data-copy-text="{version}"`.
5. `Latest release` section heading: `Version {version} at a glance`.
6. Replace the three at-a-glance cards so they summarize the new page rather than the previous
   release. Wording may be shorter than the page, but must not contradict it.
7. Update the header meta line if it names a month (`Updated November 2026`).
8. Never delete older rows unless the user explicitly asks for cleanup.

## Style details

- Four-space indentation, matching the existing pages.
- One `<section>` per required id; no duplicate ids.
- Keep the HTML language simple enough to read; no templating engines.
